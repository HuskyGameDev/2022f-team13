// =====================================================================
// Copyright 2013-2022 ToolBuddy
// All rights reserved
// 
// http://www.toolbuddy.net
// =====================================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using FluffyUnderware.Curvy;
using FluffyUnderware.DevTools;
using FluffyUnderware.DevTools.Extensions;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FluffyUnderware.Curvy
{
    /// <summary>
    /// Connection component
    /// </summary>
    [ExecuteInEditMode]
    [HelpURL(CurvySpline.DOCLINK + "curvyconnection")]
    public class CurvyConnection : DTVersionedMonoBehaviour, ISerializationCallbackReceiver
    {
        #region ### Serialized Fields ###

        [SerializeField, Hide] private List<CurvySplineSegment> m_ControlPoints = new List<CurvySplineSegment>();

        #endregion

        #region ### Private Fields ###

        private ReadOnlyCollection<CurvySplineSegment> readOnlyControlPoints;
        /// <summary>
        /// The coordinates of the connection the last time synchronisation was processed
        /// </summary>
        private Couple<Vector3, Quaternion> processedConnectionCoordinates;
        /// <summary>
        /// The coordinates of the connection's control points the last time synchronisation was processed
        /// </summary>
        [SerializeField, Hide] private List<ControlPointCoordinates> processedControlPointsCoordinates = new List<ControlPointCoordinates>();

        #endregion

        #region ### Public Properties ###


        /// <summary>
        /// The list of connected control points
        /// </summary>
        public ReadOnlyCollection<CurvySplineSegment> ControlPointsList
        {
            //TODO apply the same TODOs than CurvySpline.ControlPointsList
            get
            {
                if (readOnlyControlPoints == null)
                    readOnlyControlPoints = m_ControlPoints.AsReadOnly();
                return readOnlyControlPoints;
            }
        }
        /// <summary>
        /// Gets the number of Control Points being part of this connection
        /// </summary>
        public int Count
        {
            get { return m_ControlPoints.Count; }
        }

        /// <summary>
        /// Gets a certain Control Point by index
        /// </summary>
        /// <param name="idx">index of the Control Point</param>
        /// <returns>a Control Point</returns>
        public CurvySplineSegment this[int idx]
        {
            get
            {
                return m_ControlPoints[idx];
            }
        }

        #endregion

        #region ### Unity Callbacks ###
        /*! \cond UNITY */


        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;

            ResetProcessedCoordinates();
#if UNITY_EDITOR
            EditorApplication.update += EditorUpdate;
#endif
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
#if UNITY_EDITOR
            EditorApplication.update -= EditorUpdate;
#endif
        }

#if UNITY_EDITOR
        private void EditorUpdate()
        {
            DoUpdate();
        }
#endif


        private void Update()
        {
            if (Application.isPlaying)
                DoUpdate();
        }

        private void LateUpdate()
        {
            if (Application.isPlaying)
                DoUpdate();
        }

        private void FixedUpdate()
        {
            if (Application.isPlaying)
                DoUpdate();
        }

        private void OnDestroy()
        {
            bool realDestroy = true;
#if UNITY_EDITOR
            if (EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying)
                realDestroy = false;
#endif
            if (realDestroy)
            {
                List<CurvySplineSegment> controlPointsToDisconnect = new List<CurvySplineSegment>(m_ControlPoints);
                foreach (CurvySplineSegment cp in controlPointsToDisconnect)
                    cp.Disconnect(false);

                //This is needed even if cp.Disconnect removes the cp from those lists via Connection.RemoveControlPoint, because when calling cp.Disconnect you can have cp.Connection == null, which will lead to Connection.RemoveControlPoint not being called. Saw that happening when undoing the creation of a connect CP (via the Smart Connect tool)
                m_ControlPoints.Clear();
                processedControlPointsCoordinates.Clear();
            }
        }


        /*! \endcond */
        #endregion

        #region ### Public Methods ###

        /// <summary>
        /// Creates a connection and adds Control Points
        /// </summary>
        /// <param name="controlPoints">Control Points to add</param>
        /// <returns>the new connection</returns>
        public static CurvyConnection Create(params CurvySplineSegment[] controlPoints)
        {
            CurvyGlobalManager curvyGlobalManager = CurvyGlobalManager.Instance;
            if (curvyGlobalManager == null)
            {
                DTLog.LogError("[Curvy] Couldn't find Curvy Global Manager. Please raise a bug report.");
                return null;
            }

            CurvyConnection con = curvyGlobalManager.AddChildGameObject<CurvyConnection>("Connection");
#if UNITY_EDITOR
            if (!Application.isPlaying)
                Undo.RegisterCreatedObjectUndo(con.gameObject, "Add Connection");
#endif
            if (!con)
                return null;
            if (controlPoints.Length > 0)
            {
                con.transform.position = controlPoints[0].transform.position;
                con.AddControlPoints(controlPoints);
            }

            return con;
        }

        /// <summary>
        /// Adds Control Points to this connection
        /// </summary>
        /// <param name="controlPoints">the Control Points to add</param>
        public void AddControlPoints(params CurvySplineSegment[] controlPoints)
        {
            foreach (CurvySplineSegment cp in controlPoints)
            {
                if (cp.Connection)
                {
                    DTLog.LogErrorFormat(this, "[Curvy] CurvyConnection.AddControlPoints called on a control point '{0}' that has already a connection. Only control points with no connection can be added.", cp);
                    continue;
                }

#if UNITY_EDITOR
                if (!Application.isPlaying)
                    Undo.RecordObject(cp, "Add Connection");
#endif
#if CURVY_SANITY_CHECKS
                Assert.IsFalse(m_ControlPoints.Contains(cp));
#endif
                m_ControlPoints.Add(cp);
                processedControlPointsCoordinates.Add(new ControlPointCoordinates(cp));
                cp.Connection = this;
            }
            AutoSetFollowUp();
        }

        public void AutoSetFollowUp()
        {
            if (Count == 2)
            {
                CurvySplineSegment firstControlPoint = m_ControlPoints[0];
                CurvySplineSegment secondControlPoint = m_ControlPoints[1];
                if (firstControlPoint.transform.position == secondControlPoint.transform.position && firstControlPoint.ConnectionSyncPosition && secondControlPoint.ConnectionSyncPosition)
                {
                    if (firstControlPoint.FollowUp == null && firstControlPoint.Spline && firstControlPoint.Spline.CanControlPointHaveFollowUp(firstControlPoint))
                        firstControlPoint.SetFollowUp(secondControlPoint);
                    if (secondControlPoint.FollowUp == null && secondControlPoint.Spline && secondControlPoint.Spline.CanControlPointHaveFollowUp(secondControlPoint))
                        secondControlPoint.SetFollowUp(firstControlPoint);
                }
            }
        }

        /// <summary>
        /// Removes a Control Point from this connection
        /// </summary>
        /// <param name="controlPoint">the Control Point to remove</param>
        /// <param name="destroySelfIfEmpty">whether the connection should be destroyed when empty afterwards</param>
        public void RemoveControlPoint(CurvySplineSegment controlPoint, bool destroySelfIfEmpty = true)
        {
#if UNITY_EDITOR
            const string undoingStepLabel = "Disconnect from Connection";

            Undo.RegisterCompleteObjectUndo(new UnityEngine.Object[]{
                controlPoint, this
            }, undoingStepLabel);
#endif

            controlPoint.Connection = null;

            m_ControlPoints.Remove(controlPoint);
            processedControlPointsCoordinates.RemoveAll(element => ReferenceEquals(element.ControlPoint, controlPoint));

            foreach (CurvySplineSegment splineSegment in m_ControlPoints)
            {
                if (splineSegment.FollowUp == controlPoint)
                {
#if UNITY_EDITOR
                    Undo.RegisterCompleteObjectUndo(splineSegment, undoingStepLabel);
#endif
                    splineSegment.SetFollowUp(null);
                }
            }
            if (m_ControlPoints.Count == 0 && destroySelfIfEmpty)
                Delete();
        }

        /// <summary>
        /// Deletes the connection
        /// </summary>
        public void Delete()
        {
            gameObject.Destroy(true, true);
        }

        /// <summary>
        /// Gets all Control Points except the one provided
        /// </summary>
        /// <param name="source">the Control Point to filter out</param>
        /// <returns>list of Control Points</returns>
        public List<CurvySplineSegment> OtherControlPoints(CurvySplineSegment source)
        {
            List<CurvySplineSegment> res = new List<CurvySplineSegment>(m_ControlPoints);
            res.Remove(source);
            return res;
        }

        /// <summary>
        /// Synchronise all the connected control points to match the given position and rotation, based on their synchronisation options, namely <see cref="CurvySplineSegment.ConnectionSyncPosition"/> and <see cref="CurvySplineSegment.ConnectionSyncRotation"/>. Will update the CurvyConnection's game object's transform too.
        /// </summary>
        /// <remarks>Can dirty the splines of the updated control points</remarks>
        public void SetSynchronisationPositionAndRotation(Vector3 referencePosition, Quaternion referenceRotation)
        {
            Transform cachedTransform = transform;

            cachedTransform.position = referencePosition;
            cachedTransform.rotation = referenceRotation;
            cachedTransform.hasChanged = false;
            processedConnectionCoordinates.First = referencePosition;
            processedConnectionCoordinates.Second = referenceRotation;

            for (int i = 0; i < m_ControlPoints.Count; i++)
            {
                CurvySplineSegment controlPoint = m_ControlPoints[i];

                bool positionModified = controlPoint.ConnectionSyncPosition && controlPoint.transform.position.NotApproximately(referencePosition);
                bool rotationModified = controlPoint.ConnectionSyncRotation && controlPoint.transform.rotation.DifferentOrientation(referenceRotation);

                if (positionModified)
                    controlPoint.transform.position = referencePosition;
                if (rotationModified)
                    controlPoint.transform.rotation = referenceRotation;

                ControlPointCoordinates processedControlPointCoordinates =
                    processedControlPointsCoordinates.Single(element => ReferenceEquals(element.ControlPoint, controlPoint));

                processedControlPointCoordinates.Position = controlPoint.transform.position;
                processedControlPointCoordinates.Rotation = controlPoint.transform.rotation;

                if (positionModified || (rotationModified && controlPoint.OrientatinInfluencesSpline))
                    controlPoint.Spline.SetDirtyPartial(controlPoint
                        , positionModified == false ? SplineDirtyingType.OrientationOnly : SplineDirtyingType.Everything);
            }
        }


#if UNITY_EDITOR
        /// <summary>
        /// Gets the gizmo color based on the synchronization options of the connected control points
        /// </summary>
        public Color GetGizmoColor()
        {
            Color gizmoColor;

            if (ControlPointsList.Count == 0)
                gizmoColor = Color.black;
            else
            {
                bool allPositionsSynced = true;
                bool allRotationsSynced = true;
                foreach (CurvySplineSegment controlPoint in ControlPointsList)
                {
                    allPositionsSynced = allPositionsSynced && controlPoint.ConnectionSyncPosition;
                    allRotationsSynced = allRotationsSynced && controlPoint.ConnectionSyncRotation;

                    if (allPositionsSynced == false && allRotationsSynced == false)
                        break;
                }

                if (allPositionsSynced)
                    gizmoColor = allRotationsSynced
                        ? Color.white
                        : new Color(255 / 255f, 49 / 255f, 38 / 255f);
                else if (allRotationsSynced)
                    gizmoColor = new Color(1, 1, 0);
                else
                    gizmoColor = Color.black;
            }

            return gizmoColor;
        }
#endif


        #endregion

        #region ### Privates & Internals ###
        /*! \cond PRIVATE */

        private void DoUpdate()
        {
            Transform cachedTransform = transform;

            bool synchronised;
            if (cachedTransform.hasChanged)
            {
                cachedTransform.hasChanged = false;
                if (cachedTransform.position.NotApproximately(processedConnectionCoordinates.First) ||
                    cachedTransform.rotation.DifferentOrientation(processedConnectionCoordinates.Second))
                {
                    SetSynchronisationPositionAndRotation(cachedTransform.position, cachedTransform.rotation);
                    synchronised = true;
                }
                else
                    synchronised = false;
            }
            else
                synchronised = false;

            if (synchronised == false)
            {
                Vector3? synchronisationPosition = null;
                Quaternion? synchronisationRotation = null;

                foreach (CurvySplineSegment controlPoint in m_ControlPoints)
                {
                    if (controlPoint.gameObject == null)
                    {
                        //The only case I am aware of where this happens is when running test (see [TestFixture]), when the test is finished, a connection is duplicated and "restored", while having in its CPs list CPs that have been destroyed.
                        //This is somehow related to the following statement in the RemoveControlPoint method:
                        //Undo.RegisterCompleteObjectUndo(new UnityEngine.Object[]{
                        //    controlPoint, this
                        //}, undoingStepLabel);

                        //If you fix the problem above, remove unnecessary checks on controlPoint.gameObject. Look for the following comment to find such places:
                        // "see comment in CurvyConnection.DoUpdate to know more about when cp.gameObject can be null"

                        DTLog.LogError(String.Format("[Curvy] Connection named '{0}' had in its list a control point with no game object. Control point was ignored", this.name), this);
                        continue;
                    }
                    ControlPointCoordinates processedControlPointsCoordinate =
                        processedControlPointsCoordinates.Single(element => ReferenceEquals(element.ControlPoint, controlPoint));

                    Transform controlPointTransform = controlPoint.transform;

                    if (controlPoint.ConnectionSyncPosition && controlPointTransform.position.NotApproximately(processedControlPointsCoordinate.Position))
                        synchronisationPosition = controlPointTransform.position;

                    if (controlPoint.ConnectionSyncRotation && controlPointTransform.rotation.DifferentOrientation(processedControlPointsCoordinate.Rotation))
                        synchronisationRotation = controlPointTransform.rotation;

                    if (synchronisationPosition != null && synchronisationRotation != null)
                        break;
                }

                if (synchronisationPosition != null || synchronisationRotation != null)
                    SetSynchronisationPositionAndRotation(synchronisationPosition ?? transform.position, synchronisationRotation ?? transform.rotation);
            }
        }

        private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            //m_ControlPoints can have null references in it because if the cp it has is disabled, and then the scene is switched, the cp will not execute its OnDestroy, ans thus will not remove himself from the connection. And since destroyed unity objects become equal to null, the CPs list will have a null value in it
            int removedElementsCount = m_ControlPoints.RemoveAll(cp => cp == null);
            if (removedElementsCount != 0)
            {
                if (m_ControlPoints.Count == 0)
                    Delete();
                else
                {
                    DTLog.LogWarning("[Curvy] Connection " + this.name + " was not destroyed after scene switch. That should not happen. Please raise a bug report.", this);
                    ResetProcessedCoordinates();
                }
            }
        }

        private void ResetProcessedCoordinates()
        {
            Transform cachedTransform = transform;
            processedConnectionCoordinates =
                new Couple<Vector3, Quaternion>(cachedTransform.position, cachedTransform.rotation);
            processedControlPointsCoordinates.Clear();
            for (int index = 0; index < m_ControlPoints.Count; index++)
                processedControlPointsCoordinates.Add(new ControlPointCoordinates(m_ControlPoints[index]));
        }

        /*! \endcond */
        #endregion

        #region ISerializationCallbackReceiver
        /*! \cond PRIVATE */
        /// <summary>
        /// Implementation of UnityEngine.ISerializationCallbackReceiver
        /// Called automatically by Unity, is not meant to be called by Curvy's users
        /// </summary>
        public void OnBeforeSerialize()
        {
            m_ControlPoints.RemoveAll(cp => ReferenceEquals(cp, null));
        }

        /// <summary>
        /// Implementation of UnityEngine.ISerializationCallbackReceiver
        /// Called automatically by Unity, is not meant to be called by Curvy's users
        /// </summary>
        public void OnAfterDeserialize()
        {
            m_ControlPoints.RemoveAll(cp => ReferenceEquals(cp, null));
        }
        /*! \endcond */
        #endregion

    }

    /// <summary>
    /// A class that exists only because Unity does not serialize Dictionary<CurvySplineSegment, Couple<Vector3, Quaternion>>
    /// </summary>
    [Serializable]
    internal class ControlPointCoordinates
    {
        internal ControlPointCoordinates(CurvySplineSegment controlPoint)
        {
            ControlPoint = controlPoint;
            if(controlPoint.gameObject)
            {
                // see comment in CurvyConnection.DoUpdate to know more about when cp.gameObject can be null
                Position = controlPoint.transform.position;
                Rotation = controlPoint.transform.rotation;
            }
        }
        [SerializeField]
        internal CurvySplineSegment ControlPoint;
        [SerializeField]
        internal Vector3 Position;
        [SerializeField]
        internal Quaternion Rotation;
    }
}