// =====================================================================
// Copyright 2013-2022 ToolBuddy
// All rights reserved
// 
// http://www.toolbuddy.net
// =====================================================================

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using FluffyUnderware.DevTools.Extensions;
using FluffyUnderware.Curvy;
using FluffyUnderware.Curvy.Utils;
using System.Collections.Generic;
using FluffyUnderware.DevTools;
using UnityEngine.Serialization;
using System.Reflection;
using System;
using System.Collections.ObjectModel;
using System.Linq;
#if CONTRACTS_FULL
using System.Diagnostics.Contracts;
#endif
using JetBrains.Annotations;
using UnityEngine.Assertions;


namespace FluffyUnderware.Curvy
{
    /// <summary>
    /// Class covering a Curvy Spline Segment / ControlPoint
    /// </summary>
    [ExecuteAlways]
    [HelpURL(CurvySpline.DOCLINK + "curvysplinesegment")]
    public partial class CurvySplineSegment : DTVersionedMonoBehaviour, IPoolable
    {
        /// <summary>
        /// The color used in Gizmos to draw a segment's tangents
        /// </summary>
        public static readonly Color GizmoTangentColor = new Color(0, 0.7f, 0);

        #region ### Public Properties ###

        /// <summary>
        /// List of precalculated interpolations
        /// </summary>
        /// <remarks>Based on Spline's CacheDensity</remarks>
        [NonSerialized]
        public Vector3[] Approximation = new Vector3[0];

        /// <summary>
        /// List of precalculated distances
        /// </summary>
        /// <remarks>Based on Spline's CacheDensity</remarks>
        [NonSerialized]
        public float[] ApproximationDistances = new float[0];

        /// <summary>
        /// List of precalculated Up-Vectors
        /// </summary>
        /// <remarks>Based on Spline's CacheDensity</remarks>
        [NonSerialized]
        public Vector3[] ApproximationUp = new Vector3[0];

        /// <summary>
        /// List of precalculated Tangent-Normals
        /// </summary>
        /// <remarks>Based on Spline's CacheDensity</remarks>
        [NonSerialized]
        public Vector3[] ApproximationT = new Vector3[0];

        /// <summary>
        /// If set, Control Point's rotation will be set to the calculated Up-Vector3
        /// </summary>
        /// <remarks>This is particularly useful when connecting splines</remarks>
        public bool AutoBakeOrientation
        {
            get { return m_AutoBakeOrientation; }
            set
            {
                if (m_AutoBakeOrientation != value)
                {
                    m_AutoBakeOrientation = value;
                }
            }
        }

        /// <summary>
        /// The serialized value of OrientationAnchor. This value is ignored in some cases (invisible control points, first and last visible control points). Use <see cref="CurvySpline.IsControlPointAnOrientationAnchor"/> to get the correct value.
        /// </summary>
        public bool SerializedOrientationAnchor
        {
            get { return m_OrientationAnchor; }
            set
            {
                if (m_OrientationAnchor != value)
                {
                    m_OrientationAnchor = value;
                    Spline.SetDirty(this, SplineDirtyingType.OrientationOnly);
                    Spline.InvalidateControlPointsRelationshipCacheINTERNAL();
                }
            }
        }

        /// <summary>
        /// Swirling Mode
        /// </summary>
        public CurvyOrientationSwirl Swirl
        {
            get { return m_Swirl; }
            set
            {
                if (m_Swirl != value)
                {
                    m_Swirl = value;
                    Spline.SetDirty(this, SplineDirtyingType.OrientationOnly);
                }
            }
        }

        /// <summary>
        /// Turns to swirl
        /// </summary>
        public float SwirlTurns
        {
            get { return m_SwirlTurns; }
            set
            {
                if (m_SwirlTurns != value)
                {
                    m_SwirlTurns = value;
                    Spline.SetDirty(this, SplineDirtyingType.OrientationOnly);
                }
            }
        }

        #region --- Bezier ---
        //TODO Make sure that every place in the code setting Handles respects the constraints of Sync length, Sync direction and Sync connections

        /// <summary>
        /// Bézier spline left handle in spline's local coordinates
        /// </summary>
        public Vector3 HandleIn
        {
            get
            { return m_HandleIn; }
            set
            {
                if (m_HandleIn != value)
                {
                    m_HandleIn = value;
                    Spline.SetDirty(this, SplineDirtyingType.Everything);
                }
            }
        }

        /// <summary>
        /// Bézier spline right handle in spline's local coordinates
        /// </summary>
        public Vector3 HandleOut
        {
            get { return m_HandleOut; }
            set
            {
                if (m_HandleOut != value)
                {
                    m_HandleOut = value;
                    Spline.SetDirty(this, SplineDirtyingType.Everything);
                }
            }
        }

        //BUG this doesn't handle scaled splines. Go through all similar situations and fix them, or add as a constraint that scaled splines should be normalized (scale set back to 1) before doing any operations on them
        /// <summary>
        /// Bézier spline left handle in world coordinates
        /// </summary>
        public Vector3 HandleInPosition
        {
            get
            {
                return cachedTransform.position + Spline.transform.rotation * HandleIn;
            }
            set
            {
                HandleIn = Spline.transform.InverseTransformDirection(value - cachedTransform.position);
            }
        }

        /// <summary>
        /// Bézier spline right handle in world coordinates
        /// </summary>
        public Vector3 HandleOutPosition
        {
            get
            {
                return cachedTransform.position + Spline.transform.rotation * HandleOut;
            }
            set
            {
                HandleOut = Spline.transform.InverseTransformDirection(value - cachedTransform.position);
            }
        }
        /// <summary>
        /// Gets or Sets Auto Handles. When setting it the value of connected control points is also updated
        /// </summary>
        public bool AutoHandles
        {
            get { return m_AutoHandles; }
            set
            {
                if (SetAutoHandles(value))
                    Spline.SetDirty(this, SplineDirtyingType.Everything);
            }
        }

        public float AutoHandleDistance
        {
            get { return m_AutoHandleDistance; }
            set
            {
                if (m_AutoHandleDistance != value)
                {
                    float clampedDistance = Mathf.Clamp01(value);
                    if (m_AutoHandleDistance != clampedDistance)
                    {
                        m_AutoHandleDistance = clampedDistance;
                        Spline.SetDirty(this, SplineDirtyingType.Everything);
                    }
                }
            }
        }

        #endregion

        #region --- TCB ---

        /// <summary>
        /// Keep Start/End-TCB synchronized
        /// </summary>
        /// <remarks>Applies only to TCB Interpolation</remarks>
        public bool SynchronizeTCB
        {
            get { return m_SynchronizeTCB; }
            set
            {
                if (m_SynchronizeTCB != value)
                {
                    m_SynchronizeTCB = value;
                    Spline.SetDirty(this, SplineDirtyingType.Everything);
                }
            }
        }

        /// <summary>
        /// Whether local Tension should be used
        /// </summary>
        /// <remarks>This only applies to interpolation methods using Tension</remarks>
        public bool OverrideGlobalTension
        {
            get { return m_OverrideGlobalTension; }
            set
            {
                if (m_OverrideGlobalTension != value)
                {
                    m_OverrideGlobalTension = value;
                    Spline.SetDirty(this, SplineDirtyingType.Everything);
                }
            }
        }

        /// <summary>
        /// Whether local Continuity should be used
        /// </summary>
        /// <remarks>This only applies to interpolation methods using Continuity</remarks>
        public bool OverrideGlobalContinuity
        {
            get { return m_OverrideGlobalContinuity; }
            set
            {
                if (m_OverrideGlobalContinuity != value)
                {
                    m_OverrideGlobalContinuity = value;
                    Spline.SetDirty(this, SplineDirtyingType.Everything);
                }
            }
        }

        /// <summary>
        /// Whether local Bias should be used
        /// </summary>
        /// <remarks>This only applies to interpolation methods using Bias</remarks>
        public bool OverrideGlobalBias
        {
            get { return m_OverrideGlobalBias; }
            set
            {
                if (m_OverrideGlobalBias != value)
                {
                    m_OverrideGlobalBias = value;
                    Spline.SetDirty(this, SplineDirtyingType.Everything);
                }
            }
        }

        /// <summary>
        /// Start Tension
        /// </summary>
        /// <remarks>This only applies to interpolation methods using Tension</remarks>
        public float StartTension
        {
            get { return m_StartTension; }
            set
            {
                if (m_StartTension != value)
                {
                    m_StartTension = value;
                    Spline.SetDirty(this, SplineDirtyingType.Everything);
                }
            }
        }

        /// <summary>
        /// Start Continuity
        /// </summary>
        /// <remarks>This only applies to interpolation methods using Continuity</remarks>
        public float StartContinuity
        {
            get { return m_StartContinuity; }
            set
            {
                if (m_StartContinuity != value)
                {
                    m_StartContinuity = value;
                    Spline.SetDirty(this, SplineDirtyingType.Everything);
                }
            }
        }

        /// <summary>
        /// Start Bias
        /// </summary>
        /// <remarks>This only applies to interpolation methods using Bias</remarks>
        public float StartBias
        {
            get { return m_StartBias; }
            set
            {
                if (m_StartBias != value)
                {
                    m_StartBias = value;
                    Spline.SetDirty(this, SplineDirtyingType.Everything);
                }
            }
        }

        /// <summary>
        /// End Tension
        /// </summary>
        /// <remarks>This only applies to interpolation methods using Tension</remarks>
        public float EndTension
        {
            get { return m_EndTension; }
            set
            {
                if (m_EndTension != value)
                {
                    m_EndTension = value;
                    Spline.SetDirty(this, SplineDirtyingType.Everything);
                }
            }
        }

        /// <summary>
        /// End Continuity
        /// </summary>
        /// <remarks>This only applies to interpolation methods using Continuity</remarks>
        public float EndContinuity
        {
            get { return m_EndContinuity; }
            set
            {
                if (m_EndContinuity != value)
                {
                    m_EndContinuity = value;
                    Spline.SetDirty(this, SplineDirtyingType.Everything);
                }
            }
        }

        /// <summary>
        /// End Bias
        /// </summary>
        ///<remarks>This only applies to interpolation methods using Bias</remarks>
        public float EndBias
        {
            get { return m_EndBias; }
            set
            {
                if (m_EndBias != value)
                {
                    m_EndBias = value;
                    Spline.SetDirty(this, SplineDirtyingType.Everything);
                }
            }
        }


        #endregion
        /*
#region --- CG ---

        /// <summary>
        /// Material ID (used by PCG)
        /// </summary>
        public int CGMaterialID
        {
            get
            {
                return m_CGMaterialID;
            }
            set
            {
                if (m_CGMaterialID != Mathf.Max(0, value))
                    m_CGMaterialID = Mathf.Max(0, value);
            }
        }

        /// <summary>
        /// Whether to create a hard edge or not (used by PCG)
        /// </summary>
        public bool CGHardEdge
        {
            get { return m_CGHardEdge; }
            set
            {
                if (m_CGHardEdge != value)
                    m_CGHardEdge = value;
            }
        }
        /// <summary>
        /// Maximum vertex distance when using optimization (0=infinite)
        /// </summary>
        public float CGMaxStepDistance
        {
            get
            {
                return m_CGMaxStepDistance;
            }
            set
            {
                if (m_CGMaxStepDistance != Mathf.Max(0, value))
                    m_CGMaxStepDistance = Mathf.Max(0, value);
            }
        }

#endregion
        */
        #region --- Connections ---
        /// <summary>
        /// Gets the connected Control Point that is set as "Head To"
        /// </summary>
        public CurvySplineSegment FollowUp
        {
            get
            {
                //TODO reactivate this sanity check once the connections related data are no more stored in the control point but in the connection. Right now the check is disabled because the code in CurvyConnection.RemoveControlPoint needs to get the follow up while m_FollowUp.Connection == Connection is not true so that it can correct the situation
                //#if CURVY_SANITY_CHECKS
                //                Assert.IsTrue(m_FollowUp == null || m_FollowUp.Connection == Connection);
                //#endif
                return m_FollowUp;
            }
            private set
            {
                if (m_FollowUp != value)
                {
                    m_FollowUp = value;
#if CURVY_SANITY_CHECKS
                    Assert.IsTrue(m_FollowUp == null || m_FollowUp.Connection == Connection);
#endif
                    if (mSpline != null)
                        mSpline.SetDirty(this, SplineDirtyingType.Everything);
                }
            }
        }


        /// <summary>
        /// Gets or sets the heading toward the "Head To" segment
        /// </summary>
        //Remark Set/Get value is validated through GetValidateConnectionHeading
        public ConnectionHeadingEnum FollowUpHeading
        {
            get
            {
                return GetValidateConnectionHeading(m_FollowUpHeading, FollowUp);
            }
            set
            {
                value = GetValidateConnectionHeading(value, FollowUp);

                if (m_FollowUpHeading != value)
                {
                    m_FollowUpHeading = value;
                    if (mSpline != null)
                        mSpline.SetDirty(this, SplineDirtyingType.Everything);
                }
            }
        }

        /// <summary>
        /// When part of a <see cref="CurvyConnection"/>, this defines whether the connection's position is applied to this Control Point
        /// The synchronization process is applied by <see cref="CurvyConnection"/> at each frame in its update methods. So if you modify the value of this property, and want the synchronization to happen right away, you will have to call the connection's <see cref="CurvyConnection.SetSynchronisationPositionAndRotation(Vector3, Quaternion)"/> with the connection's position and rotation as parameters
        /// </summary>
        public bool ConnectionSyncPosition
        {
            get { return m_ConnectionSyncPosition; }
            set
            {
                if (m_ConnectionSyncPosition != value)
                {
                    m_ConnectionSyncPosition = value;
                    //DESIGN think about removing the code that handles ConnectionSyncPosition and ConnectionSyncRotation, and replace it with a code that always runs in Refresh, and make that code happen by calling SetDirty() here;
                }
            }
        }

        /// <summary>
        /// When part of a <see cref="CurvyConnection"/>, this defines whether the connection's rotation is applied to this Control Point
        /// The synchronization process is applied by <see cref="CurvyConnection"/> at each frame in its update methods. So if you modify the value of this property, and want the synchronization to happen right away, you will have to call the connection's <see cref="CurvyConnection.SetSynchronisationPositionAndRotation(Vector3, Quaternion)"/> with the connection's position and rotation as parameters
        /// </summary>
        public bool ConnectionSyncRotation
        {
            get { return m_ConnectionSyncRotation; }
            set
            {
                if (m_ConnectionSyncRotation != value)
                {
                    m_ConnectionSyncRotation = value;
                    //DESIGN think about removing the code that handles ConnectionSyncPosition and ConnectionSyncRotation, and replace it with a code that always runs in Refresh, and make that code happen by calling SetDirty() here;
                }
            }
        }

        /// <summary>
        /// Gets/Sets the connection handler this Control Point is using (if any)
        /// </summary>
        /// <remarks>If set to null, FollowUp wil be set to null to</remarks>
        public CurvyConnection Connection
        {
            get { return m_Connection; }
            internal set
            {
                if (SetConnection(value))
                    if (mSpline != null)
                        mSpline.SetDirty(this, SplineDirtyingType.Everything);
            }
        }

        #endregion


        /// <summary>
        /// Gets the number of individual cache points of this segment
        /// </summary>
        /// <remarks>The actual approximations arrays' size is CacheSize + 1</remarks>
        public int CacheSize
        {
            get
            {
#if CURVY_SANITY_CHECKS
                Assert.IsTrue(Approximation.Length > 0, "[Curvy] CurvySplineSegment has uninitialized cache");
#endif
                return Approximation.Length - 1;
            }

        }

        /// <summary>
        /// Gets this segment's bounds in world space
        /// </summary>
        public Bounds Bounds
        {
            get
            {
                if (!mBounds.HasValue)
                {
                    Bounds result;
                    if (Approximation.Length == 0)
                        result = new Bounds(cachedTransform.position, Vector3.zero);
                    else
                    {
                        Matrix4x4 mat = Spline.transform.localToWorldMatrix;
                        result = new Bounds(mat.MultiplyPoint3x4(Approximation[0]), Vector3.zero);
                        int u = Approximation.Length;
                        for (int i = 1; i < u; i++)
                            result.Encapsulate(mat.MultiplyPoint3x4(Approximation[i]));
                    }

                    mBounds = result;
                }
                return mBounds.Value;
            }
        }

        /// <summary>
        /// Gets the length of this spline segment
        /// </summary>
        public float Length { get; private set; }

        /// <summary>
        /// Gets the distance from spline start to the first control point (localF=0) 
        /// </summary>
        public float Distance { get; internal set; }

        /// <summary>
        /// Gets the TF of this Control Point
        /// TF stands for Total Fragment. It's a value ranging from 0 to 1 inclusive. 0 means the spline's start and 1 means the spline's end.
        /// This is the "time" parameter used in the splines' formulas.
        /// A point's TF is not proportional to its distance from the spline's start. Depending on the spline, a value of 0.5 does not always mean the middle, distance wise, of the spline
        /// </summary>
        /// <remarks>This yields the same result as LocalFToTF(0)</remarks>
        public float TF
        {
            get
            {
                return mSpline.SegmentToTF(this);
            }
#if UNITY_2020_3_OR_NEWER
            [Obsolete("Setting a TF value is not allowed anymore")]
#endif
            internal set => Debug.LogError("[Curvy] CurvySplineSegment.TF: Setting a TF value is not allowed");
        }

        /// <summary>
        /// Gets whether this Control Point is the first IGNORING closed splines
        /// </summary>
        public bool IsFirstControlPoint
        {
            get
            {
                return (Spline.GetControlPointIndex(this) == 0);
            }
        }

        /// <summary>
        /// Gets whether this Control Point is the last IGNORING closed splines
        /// </summary>
        public bool IsLastControlPoint
        {
            get
            {
                return (Spline.GetControlPointIndex(this) == Spline.ControlPointCount - 1);
            }
        }

        /// <summary>
        /// The Metadata components added to this GameObject
        /// </summary>
        public HashSet<CurvyMetadataBase> Metadata
        {
            get
            {
                return mMetadata;
            }
        }

        /// <summary>
        /// Gets the parent spline
        /// </summary>
        public CurvySpline Spline
        {
            get
            {
                return mSpline;
            }
        }


        /// <summary>
        /// Returns true if the local position is different than the last one used in the segment approximations cache computation
        /// </summary>
        public bool HasUnprocessedLocalPosition { get { return cachedTransform.localPosition.Approximately(lastProcessedLocalPosition) == false; } }
        /// <summary>
        /// Returns true if the local orientation is different than the last one used in the segment approximations cache computation
        /// </summary>
        public bool HasUnprocessedLocalOrientation { get { return cachedTransform.localRotation.DifferentOrientation(lastProcessedLocalRotation); } }
        /// <summary>
        /// Returns wheter the orientation of this Control Point influences the orientation of its containing spline's approximation points.
        /// Returns false if control point is not part of a spline
        /// </summary>
        public bool OrientatinInfluencesSpline { get { return mSpline != null && (mSpline.Orientation == CurvyOrientation.Static || mSpline.IsControlPointAnOrientationAnchor(this)); } }

        #endregion

        #region ### Public Methods ###

        /// <summary>
        /// Sets Bezier HandleIn
        /// </summary>
        /// <param name="position">HandleIn position</param>
        /// <param name="space">The space (spline's local space or the world space) in which the <paramref name="position"/> is expressed</param>
        /// <param name="mode">Handle synchronization mode</param>
        public void SetBezierHandleIn(Vector3 position, Space space = Space.Self, CurvyBezierModeEnum mode = CurvyBezierModeEnum.None)
        {
            if (space == Space.Self)
                HandleIn = position;
            else
                HandleInPosition = position;

            bool syncDirections = (mode & CurvyBezierModeEnum.Direction) == CurvyBezierModeEnum.Direction;
            bool syncLengths = (mode & CurvyBezierModeEnum.Length) == CurvyBezierModeEnum.Length;
            bool syncConnectedCPs = (mode & CurvyBezierModeEnum.Connections) == CurvyBezierModeEnum.Connections;

            if (syncDirections)
                HandleOut = HandleOut.magnitude * (HandleIn.normalized * -1);
            if (syncLengths)
                HandleOut = HandleIn.magnitude * ((HandleOut == Vector3.zero) ? HandleIn.normalized * -1 : HandleOut.normalized);
            if (Connection && syncConnectedCPs && (syncDirections || syncLengths))
            {
                ReadOnlyCollection<CurvySplineSegment> connectionControlPoints = Connection.ControlPointsList;
                for (int index = 0; index < connectionControlPoints.Count; index++)
                {
                    CurvySplineSegment connectedCp = connectionControlPoints[index];
                    if (connectedCp == this)
                        continue;

                    if (connectedCp.HandleIn.magnitude == 0)
                        connectedCp.HandleIn = HandleIn;

                    if (syncDirections)
                        connectedCp.SetBezierHandleIn(connectedCp.HandleIn.magnitude * HandleIn.normalized * Mathf.Sign(Vector3.Dot(HandleIn, connectedCp.HandleIn)), Space.Self, CurvyBezierModeEnum.Direction);
                    if (syncLengths)
                        connectedCp.SetBezierHandleIn(connectedCp.HandleIn.normalized * HandleIn.magnitude, Space.Self, CurvyBezierModeEnum.Length);
                }
            }
        }

        /// <summary>
        /// Sets Bezier HandleOut
        /// </summary>
        /// <param name="position">HandleOut position</param>
        /// <param name="space">The space (spline's local space or the world space) in which the <paramref name="position"/> is expressed</param>
        /// <param name="mode">Handle synchronization mode</param>
        public void SetBezierHandleOut(Vector3 position, Space space = Space.Self, CurvyBezierModeEnum mode = CurvyBezierModeEnum.None)
        {
            if (space == Space.Self)
                HandleOut = position;
            else
                HandleOutPosition = position;

            bool syncDirections = (mode & CurvyBezierModeEnum.Direction) == CurvyBezierModeEnum.Direction;
            bool syncLengths = (mode & CurvyBezierModeEnum.Length) == CurvyBezierModeEnum.Length;
            bool syncConnectedCPs = (mode & CurvyBezierModeEnum.Connections) == CurvyBezierModeEnum.Connections;

            if (syncDirections)
                HandleIn = HandleIn.magnitude * (HandleOut.normalized * -1);
            if (syncLengths)
                HandleIn = HandleOut.magnitude * ((HandleIn == Vector3.zero) ? HandleOut.normalized * -1 : HandleIn.normalized);

            if (Connection && syncConnectedCPs && (syncDirections || syncLengths))
            {
                for (int index = 0; index < (Connection.ControlPointsList).Count; index++)
                {
                    CurvySplineSegment connectedCp = (Connection.ControlPointsList)[index];
                    if (connectedCp == this)
                        continue;

                    if (connectedCp.HandleOut.magnitude == 0)
                        connectedCp.HandleOut = HandleOut;

                    if (syncDirections)
                        connectedCp.SetBezierHandleOut(connectedCp.HandleOut.magnitude * HandleOut.normalized * Mathf.Sign(Vector3.Dot(HandleOut, connectedCp.HandleOut)), Space.Self, CurvyBezierModeEnum.Direction);
                    if (syncLengths)
                        connectedCp.SetBezierHandleOut(connectedCp.HandleOut.normalized * HandleOut.magnitude, Space.Self, CurvyBezierModeEnum.Length);
                }
            }
        }

        /// <summary>
        /// Automatically place Bezier handles relative to neighbour Control Points
        /// </summary>
        /// <param name="distanceFrag">how much % distance between neighbouring CPs are applied to the handle length?</param>
        /// <param name="setIn">Set HandleIn?</param>
        /// <param name="setOut">Set HandleOut?</param>
        /// <param name="noDirtying">If true, the Bezier handles will be modified without dirtying any spline</param>
        public void SetBezierHandles(float distanceFrag = -1, bool setIn = true, bool setOut = true, bool noDirtying = false)
        {
            Vector3 pIn = Vector3.zero;
            Vector3 pOut = Vector3.zero;
            if (distanceFrag == -1)
                distanceFrag = AutoHandleDistance;
            if (distanceFrag > 0)
            {
                CurvySpline spline = Spline;

                CurvySplineSegment nextControlPoint = spline.GetNextControlPoint(this);
                Transform nextTt = nextControlPoint
                    ? nextControlPoint.transform
                    : cachedTransform;
                CurvySplineSegment previousControlPoint = spline.GetPreviousControlPoint(this);
                Transform previousTt = previousControlPoint
                    ? previousControlPoint.transform
                    : cachedTransform;


                Vector3 c = cachedTransform.localPosition;
                Vector3 p = previousTt.localPosition - c;
                Vector3 n = nextTt.localPosition - c;
                SetBezierHandles(distanceFrag, p, n, setIn, setOut, noDirtying);
            }
            else
            {
                // Fallback to zero
                if (setIn)
                    if (noDirtying)
                        m_HandleIn = pIn;
                    else
                        HandleIn = pIn;

                if (setOut)
                    if (noDirtying)
                        m_HandleOut = pOut;
                    else
                        HandleOut = pOut;
            }

        }

        /// <summary>
        /// Automatically place Bezier handles
        /// </summary>
        /// <param name="distanceFrag">how much % distance between neighbouring CPs are applied to the handle length?</param>
        /// <param name="p">Position the In-Handle relates to</param>
        /// <param name="n">Position the Out-Handle relates to</param>
        /// <param name="setIn">Set HandleIn?</param>
        /// <param name="setOut">Set HandleOut?</param>
        /// <param name="noDirtying">If true, the Bezier handles will be modified without dirtying any spline</param>
        public void SetBezierHandles(float distanceFrag, Vector3 p, Vector3 n, bool setIn = true, bool setOut = true, bool noDirtying = false)
        {
            float pLen = p.magnitude;
            float nLen = n.magnitude;
            Vector3 pIn = Vector3.zero;
            Vector3 pOut = Vector3.zero;

            if (pLen != 0 || nLen != 0)
            {
                Vector3 dir = ((pLen / nLen) * n - p).normalized;
                pIn = -dir * (pLen * distanceFrag);
                pOut = dir * (nLen * distanceFrag);
            }

            // Fallback to zero
            if (setIn)
                if (noDirtying)
                    m_HandleIn = pIn;
                else
                    HandleIn = pIn;

            if (setOut)
                if (noDirtying)
                    m_HandleOut = pOut;
                else
                    HandleOut = pOut;
        }


        /// <summary>
        /// Sets Follow-Up of this Control Point
        /// </summary>
        /// <param name="target">the Control Point to follow to</param>
        /// <param name="heading">the Heading on the target's spline</param>
        public void SetFollowUp(CurvySplineSegment target, ConnectionHeadingEnum heading = ConnectionHeadingEnum.Auto)
        {
            if (target == null)
            {
                FollowUp = target;
                FollowUpHeading = heading;
            }
            else if (Spline.CanControlPointHaveFollowUp(this))
            {
                if (Connection == null || Connection != target.Connection)
                    DTLog.LogError("[Curvy] Trying to set as a Follow-Up a Control Point that is not part of the same connection", this);
                else
                {
                    FollowUp = target;
                    FollowUpHeading = heading;
                }
            }
            else
                DTLog.LogError("[Curvy] Setting a Follow-Up to a Control Point that can't have one", this);
        }

        /// <summary>
        /// Resets the connections related data (Connection, FollowUp, etc) while updating the Connection object and dirtying relevant splines.
        /// </summary>
        public void Disconnect()
        {
            Disconnect(true);
        }

        /// <summary>
        /// Resets the connections related data (Connection, FollowUp, etc) while updating the Connection object and dirtying relevant splines.
        /// </summary>
        /// <param name="destroyEmptyConnection">whether the related <see cref="Connection"/> should be destroyed if it becomes empty due to this Disconnect call</param>
        public void Disconnect(bool destroyEmptyConnection)
        {
            if (Connection)
                Connection.RemoveControlPoint(this, destroyEmptyConnection);

            //TODO make all this data part of the connection and not the CP
            //Reset connection related data
            FollowUp = null;
            FollowUpHeading = ConnectionHeadingEnum.Auto;
            ConnectionSyncPosition = false;
            ConnectionSyncRotation = false;
        }

        /// <summary>
        /// Gets the position of a point on the spline segment
        /// </summary>
        /// <param name="localF">localF stands for Local Fragment. It's a value ranging from 0 to 1 inclusive. 0 means the segment's start and 1 means the segment's end. This is the "time" parameter used in the splines' formulas. A point's localF is not proportional to its distance from the segment's start. Depending on the spline, a value of 0.5 does not always mean the middle, distance wise, of the segment</param>
        /// <param name="space">The space (spline's local space or the world space) in which the returned result is expressed</param>
        public Vector3 Interpolate(float localF, Space space = Space.Self)
        {
            CurvySpline spline = Spline;

#if CURVY_SANITY_CHECKS
            if (spline.Dirty)
                DTLog.LogWarning("Interpolate should not be called on segment of a dirty spline. Call CurvySpline.Refresh first", this);
            Assert.IsTrue(spline.IsControlPointASegment(this));
            Assert.IsTrue(spline.IsCpsRelationshipCacheValidINTERNAL);
#endif
            CurvyInterpolation curvyInterpolation = spline.Interpolation;

            Vector3 result;
            localF = Mathf.Clamp01(localF);

            //If you modify this, modify also the inlined version of this method in refreshCurveINTERNAL()
            switch (curvyInterpolation)
            {
                case CurvyInterpolation.BSpline:
                    result = BSpline(spline.ControlPointsList, spline.SegmentToTF(this, localF), spline.IsBSplineClamped, spline.Closed, spline.BSplineDegree, BSplineP0Array.Array);
                    break;
                case CurvyInterpolation.Bezier:
                    {
                        result = CurvySpline.Bezier(threadSafeLocalPosition.Addition(HandleOut),
                            threadSafeLocalPosition,
                            threadSafeNextCpLocalPosition,
                            threadSafeNextCpLocalPosition.Addition(cachedNextControlPoint.HandleIn),
                            localF);
                        break;
                    }
                case CurvyInterpolation.CatmullRom:
                case CurvyInterpolation.TCB:
                    {
                        if (curvyInterpolation == CurvyInterpolation.TCB)
                        {
                            float t0 = StartTension; float t1 = EndTension;
                            float c0 = StartContinuity; float c1 = EndContinuity;
                            float b0 = StartBias; float b1 = EndBias;

                            if (!OverrideGlobalTension)
                                t0 = t1 = mSpline.Tension;
                            if (!OverrideGlobalContinuity)
                                c0 = c1 = mSpline.Continuity;
                            if (!OverrideGlobalBias)
                                b0 = b1 = mSpline.Bias;

                            result = CurvySpline.TCB(threadSafePreviousCpLocalPosition,
                                threadSafeLocalPosition,
                                threadSafeNextCpLocalPosition,
                                cachedNextControlPoint.threadSafeNextCpLocalPosition,
                                localF, t0, c0, b0, t1, c1, b1);
                        }
                        else
                            result = CurvySpline.CatmullRom(threadSafePreviousCpLocalPosition,
                                threadSafeLocalPosition,
                                threadSafeNextCpLocalPosition,
                                cachedNextControlPoint.threadSafeNextCpLocalPosition,
                                localF);
                    }
                    break;
                case CurvyInterpolation.Linear:
                    result = OptimizedOperators.LerpUnclamped(
                        threadSafeLocalPosition,
                        threadSafeNextCpLocalPosition,
                        localF);
                    break;
                default:
                    DTLog.LogError("[Curvy] Invalid interpolation value " + curvyInterpolation, this);
                    return Vector3.zero;
            }

            if (space == Space.World)
                result = spline.ToWorldPosition(result);
            return result;
        }

        /// <summary>
        /// Gets the position of a point on the spline segment.
        /// Instead of computing the exact value, this method uses a linear interpolation between cached points for faster result
        /// </summary>
        /// <param name="localF">localF stands for Local Fragment. It's a value ranging from 0 to 1 inclusive. 0 means the segment's start and 1 means the segment's end. This is the "time" parameter used in the splines' formulas. A point's localF is not proportional to its distance from the segment's start. Depending on the spline, a value of 0.5 does not always mean the middle, distance wise, of the segment</param>
        /// <param name="space">The space (spline's local space or the world space) in which the returned result is expressed</param>
        public Vector3 InterpolateFast(float localF, Space space = Space.Self)
        {
            Vector3 result;
            if (Approximation.Length > 1)
            {
                float frag;
                int idx = getApproximationIndexINTERNAL(localF, out frag);
                result = OptimizedOperators.LerpUnclamped(Approximation[idx], Approximation[idx + 1], frag);
            }
            else
                result = Approximation[0];

            if (space == Space.World)
                result = Spline.ToWorldPosition(result);
            return result;
        }

        /// <summary>
        /// Gets the normalized tangent at a point on the spline segment
        /// </summary>
        /// <param name="localF">localF stands for Local Fragment. It's a value ranging from 0 to 1 inclusive. 0 means the segment's start and 1 means the segment's end. This is the "time" parameter used in the splines' formulas. A point's localF is not proportional to its distance from the segment's start. Depending on the spline, a value of 0.5 does not always mean the middle, distance wise, of the segment</param>
        /// <param name="space">The space (spline's local space or the world space) in which the returned result is expressed</param>
        public Vector3 GetTangent(float localF, Space space = Space.Self)
        {
            localF = Mathf.Clamp01(localF);
            Vector3 position = Interpolate(localF, space);
            return GetTangent(localF, position, space);
        }

        /// <summary>
        /// Gets the normalized tangent at a point on the spline segment.
        /// This method is faster than <see cref="GetTangent(float, Space)"/> if you have already the position of the point.
        /// Instead of computing the exact value, this method uses a linear interpolation between cached points for faster result
        /// </summary>
        /// <param name="localF">localF stands for Local Fragment. It's a value ranging from 0 to 1 inclusive. 0 means the segment's start and 1 means the segment's end. This is the "time" parameter used in the splines' formulas. A point's localF is not proportional to its distance from the segment's start. Depending on the spline, a value of 0.5 does not always mean the middle, distance wise, of the segment</param>
        /// <param name="position">the position of the point at localF. In other words, the result of <see cref="Interpolate(float, Space)"/></param>
        /// <param name="space">The space (spline's local space or the world space) in which the returned result is expressed</param>
        public Vector3 GetTangent(float localF, Vector3 position, Space space = Space.Self)
        {
            CurvySpline curvySpline = Spline;
#if CONTRACTS_FULL
            Contract.Requires(curvySpline != null);
#endif
            Vector3 p2;
            int leave = 2;
            const float fIncrement = 0.01f;
            do
            {
                float f2 = localF + fIncrement;
                if (f2 > 1)
                {
                    CurvySplineSegment nSeg = curvySpline.GetNextSegment(this);
                    if (nSeg)
                        p2 = nSeg.Interpolate(f2 - 1, space);//return (NextSegment.Interpolate(f2 - 1) - position).normalized;
                    else
                    {
                        f2 = localF - fIncrement;
                        return OptimizedOperators.Normalize(position.Subtraction(Interpolate(f2, space)));
                    }
                }
                else
                    p2 = Interpolate(f2, space); // return (Interpolate(f2) - position).normalized;

                localF += fIncrement;
            } while (p2 == position && --leave > 0);

            return OptimizedOperators.Normalize(p2.Subtraction(position));
        }

        /// <summary>
        /// Gets the normalized tangent at a point on the spline segment.
        /// Instead of computing the exact value, this method uses a linear interpolation between cached points for faster result
        /// </summary>
        /// <param name="localF">localF stands for Local Fragment. It's a value ranging from 0 to 1 inclusive. 0 means the segment's start and 1 means the segment's end. This is the "time" parameter used in the splines' formulas. A point's localF is not proportional to its distance from the segment's start. Depending on the spline, a value of 0.5 does not always mean the middle, distance wise, of the segment</param>
        /// <param name="space">The space (spline's local space or the world space) in which the returned result is expressed</param>
        public Vector3 GetTangentFast(float localF, Space space = Space.Self)
        {
            Vector3 result;
            if (ApproximationT.Length > 1)
            {
                float frag;
                int idx = getApproximationIndexINTERNAL(localF, out frag);
                result = Vector3.SlerpUnclamped(ApproximationT[idx], ApproximationT[idx + 1], frag);
            }
            else
                result = ApproximationT[0];

            if (space == Space.World)
                result = Spline.ToWorldDirection(result);
            return result;
        }

        /// <summary>
        /// Gets the position and normalized tangent at a point on the spline segment
        /// Is Faster than calling <see cref="Interpolate(float, Space)"/> and <see cref="Interpolate(float, Space)"/> separately
        /// </summary>
        /// <param name="localF">localF stands for Local Fragment. It's a value ranging from 0 to 1 inclusive. 0 means the segment's start and 1 means the segment's end. This is the "time" parameter used in the splines' formulas. A point's localF is not proportional to its distance from the segment's start. Depending on the spline, a value of 0.5 does not always mean the middle, distance wise, of the segment</param>
        /// <param name="position">the output position</param>
        /// <param name="tangent">the output tangent</param>
        /// <param name="space">The space (spline's local space or the world space) in which the returned result is expressed</param>
        public void InterpolateAndGetTangent(float localF, out Vector3 position, out Vector3 tangent, Space space = Space.Self)
        {
            localF = Mathf.Clamp01(localF);
            position = Interpolate(localF, space);
            tangent = GetTangent(localF, position, space);
        }

        /// <summary>
        /// Gets the position and normalized tangent at a point on the spline segment
        /// Is Faster than calling <see cref="Interpolate(float, Space)"/> and <see cref="Interpolate(float, Space)"/> separately
        /// Instead of computing the exact value, this method uses a linear interpolation between cached points for faster result
        /// </summary>
        /// <param name="localF">localF stands for Local Fragment. It's a value ranging from 0 to 1 inclusive. 0 means the segment's start and 1 means the segment's end. This is the "time" parameter used in the splines' formulas. A point's localF is not proportional to its distance from the segment's start. Depending on the spline, a value of 0.5 does not always mean the middle, distance wise, of the segment</param>
        /// <param name="position">the output position</param>
        /// <param name="tangent">the output tangent</param>
        /// <param name="space">The space (spline's local space or the world space) in which the returned result is expressed</param>
        public void InterpolateAndGetTangentFast(float localF, out Vector3 position, out Vector3 tangent, Space space = Space.Self)
        {
            if (Approximation.Length > 1)
            {
                float frag;
                int idx = getApproximationIndexINTERNAL(localF, out frag);
                int idx2 = idx + 1;
                position = OptimizedOperators.LerpUnclamped(Approximation[idx], Approximation[idx2], frag);
                tangent = Vector3.SlerpUnclamped(ApproximationT[idx], ApproximationT[idx2], frag);
            }
            else
            {
                position = Approximation[0];
                tangent = ApproximationT[0];
            }


            if (space == Space.World)
            {
                position = Spline.ToWorldPosition(position);
                tangent = Spline.ToWorldDirection(tangent);
            }
        }

        /// <summary>
        /// Gets the Up vector of a point on the spline segment.
        /// Instead of computing the exact value, this method uses a linear interpolation between cached points for faster result
        /// </summary>
        /// <param name="localF">localF stands for Local Fragment. It's a value ranging from 0 to 1 inclusive. 0 means the segment's start and 1 means the segment's end. This is the "time" parameter used in the splines' formulas. A point's localF is not proportional to its distance from the segment's start. Depending on the spline, a value of 0.5 does not always mean the middle, distance wise, of the segment</param>
        /// <param name="space">The space (spline's local space or the world space) in which the returned result is expressed</param>
        public Vector3 GetOrientationUpFast(float localF, Space space = Space.Self)
        {
            Vector3 result;
            if (ApproximationUp.Length > 1)
            {
                float frag;
                int idx = getApproximationIndexINTERNAL(localF, out frag);
                result = Vector3.SlerpUnclamped(ApproximationUp[idx], ApproximationUp[idx + 1], frag);
            }
            else
                result = ApproximationUp[0];

            if (space == Space.World)
                result = Spline.ToWorldDirection(result);
            return result;
        }

        /// <summary>
        /// Gets the rotation of a point on the spline segment. The rotation's forward is the segment's tangent, and it's up is the segment's orientation
        /// Instead of computing the exact value, this method uses a linear interpolation between cached points for faster result
        /// </summary>
        /// <param name="localF">localF stands for Local Fragment. It's a value ranging from 0 to 1 inclusive. 0 means the segment's start and 1 means the segment's end. This is the "time" parameter used in the splines' formulas. A point's localF is not proportional to its distance from the segment's start. Depending on the spline, a value of 0.5 does not always mean the middle, distance wise, of the segment</param>
        /// <param name="inverse">whether the orientation should look at the opposite direction of the tangent</param>
        /// <param name="space">The space (spline's local space or the world space) in which the returned result is expressed</param>
        public Quaternion GetOrientationFast(float localF, bool inverse = false, Space space = Space.Self)
        {
            Vector3 view = GetTangentFast(localF, space);

            Quaternion result;
            if (view != Vector3.zero)
            {
                if (inverse)
                    view *= -1;
                result = Quaternion.LookRotation(view, GetOrientationUpFast(localF, space));
            }
            else
            {
#if CURVY_SANITY_CHECKS
                Debug.LogError(string.Format("[Curvy] Invalid Orientation for segment {0} at localF {1}", this, localF));
#endif
                result = Quaternion.identity;
            }

            return result;

        }


        #region MetaData handling

        /// <summary>
        /// Rebuilds <see cref="Metadata"/>
        /// </summary>
        public void ReloadMetaData()
        {
            Metadata.Clear();
            CurvyMetadataBase[] metaDataComponents = GetComponents<CurvyMetadataBase>();
            foreach (CurvyMetadataBase metaData in metaDataComponents)
                Metadata.Add(metaData);

            CheckAgainstMetaDataDuplication();
        }

        /// <summary>
        /// Adds a MetaData instance to <see cref="Metadata"/>
        /// </summary>
        public void RegisterMetaData(CurvyMetadataBase metaData)
        {
            Metadata.Add(metaData);
            CheckAgainstMetaDataDuplication();
        }

        /// <summary>
        /// Removes a MetaData instance from <see cref="Metadata"/>
        /// </summary>
        public void UnregisterMetaData(CurvyMetadataBase metaData)
        {
            Metadata.Remove(metaData);
        }

        /// <summary>
        /// Gets Metadata of this ControlPoint
        /// </summary>
        /// <typeparam name="T">Metadata type</typeparam>
        /// <param name="autoCreate">whether to create the Metadata component if it's not present</param>
        /// <returns>the Metadata component or null</returns>
        public T GetMetadata<T>(bool autoCreate = false) where T : CurvyMetadataBase
        {
            Type type = typeof(T);
            T result = null;

            foreach (CurvyMetadataBase metaData in Metadata)
                if (metaData != null && metaData.GetType() == type)
                {
                    result = (T)metaData;
                    break;
                }

            if (autoCreate && result == null)
            {
                result = gameObject.AddComponent<T>();
                Metadata.Add(result);
            }
            return result;
        }

        /// <summary>
        /// Gets an interpolated Metadata value for a certain F
        /// </summary>
        /// <typeparam name="T">Metadata type inheriting from CurvyInterpolatableMetadataBase</typeparam>
        /// <typeparam name="U">Metadata's Value type</typeparam>
        /// <param name="f">a local F in the range 0..1</param>
        /// <returns>The interpolated value. If no Metadata of specified type is present at the given tf, the default value of type U is returned</returns>
        public U GetInterpolatedMetadata<T, U>(float f) where T : CurvyInterpolatableMetadataBase<U>
        {
            T metaData = GetMetadata<T>();
            if (metaData != null)
            {
                CurvySplineSegment nextCp = Spline.GetNextControlPointUsingFollowUp(this);
                CurvyInterpolatableMetadataBase<U> nextMetaData = null;
                if (nextCp)
                    nextMetaData = nextCp.GetMetadata<T>();
                return metaData.Interpolate(nextMetaData, f);
            }
            return default;
        }

        /// <summary>
        /// Removes all Metadata components of this Control Point
        /// </summary>
        public void DeleteMetadata()
        {
            List<CurvyMetadataBase> metaDataList = Metadata.ToList();
            for (int i = metaDataList.Count - 1; i >= 0; i--)
                metaDataList[i].Destroy(true, false);
        }
        #endregion

        /// <summary>
        /// Gets the localF of the point on the segment that is the nearest to a given position
        /// localF stands for Local Fragment. It's a value ranging from 0 to 1 inclusive. 0 means the segment's start and 1 means the segment's end. This is the "time" parameter used in the splines' formulas. A point's localF is not proportional to its distance from the segment's start. Depending on the spline, a value of 0.5 does not always mean the middle, distance wise, of the segment
        /// </summary>
        /// <param name="position">The point's position</param>
        /// <param name="space">The space (spline's local space or the world space) in which the returned result is expressed</param> <paramref name="position"/> is expressed</param>
        /// <remarks>This method's precision and speed depend on the <see cref="CurvySpline.CacheDensity"/></remarks>
        public float GetNearestPointF(Vector3 position, Space space = Space.Self)
        {
#if CURVY_SANITY_CHECKS
            Assert.IsTrue(CacheSize >= 0, "[Curvy] CurvySplineSegment has uninitialized cache. Call Refresh() on the CurvySpline it belongs to.");
#endif
            if (space == Space.World)
                position = Spline.ToLocalPosition(position);

            CurvyUtility.GetNearestPointIndex(position, Approximation, Approximation.Length, out int index, out float frag);

            // return the nearest collision
            return (index + frag) / (Approximation.Length - 1);

        }


        /// <summary>
        /// Gets the local F of a point given its local distance
        /// Local F stands for Local Fragment. It's a value ranging from 0 to 1 inclusive. 0 means the segment's start and 1 means the segment's end. This is the "time" parameter used in the splines' formulas. A point's localF is not proportional to its distance from the segment's start. Depending on the spline, a value of 0.5 does not always mean the middle, distance wise, of the segment
        /// </summary>
        /// <param name="localDistance">The distance between the segment's start and the point you are interested in. Value should be in the range from 0 to <see cref="Length"/> inclusive</param>>
        public float DistanceToLocalF(float localDistance)
        {
#if CURVY_SANITY_CHECKS
            Assert.IsTrue(localDistance >= 0);
            Assert.IsTrue(localDistance <= Length);
#endif
            int approximationDistancesLength = ApproximationDistances.Length;

            if (approximationDistancesLength <= 1 || localDistance == 0)
                return 0;

            int lowerIndex = CurvyUtility.InterpolationSearch(ApproximationDistances, ApproximationDistances.Length, localDistance);
            if (lowerIndex == approximationDistancesLength - 1)
                return 1;

            //BUG this basically interpolates linearly the F value between two cache points. This is an approximation that is not correct because F does not vary linearly between two points, unlike distance that does. The issue is not big as long as there is a lot of cache points to keep the difference between the correct answer and the approximate one small enough.
            float frag = (localDistance - ApproximationDistances[lowerIndex]) / (ApproximationDistances[lowerIndex + 1] - ApproximationDistances[lowerIndex]);
            return (lowerIndex + frag) / (approximationDistancesLength - 1);
        }

        /// <summary>
        /// Gets the local distance of a point at a certain localF.
        /// Local distance is the distance between a point and the start of its segment. Value ranges from 0 to the segment's <see cref="Length"/>, inclusive
        /// </summary>
        /// <param name="localF">localF stands for Local Fragment. It's a value ranging from 0 to 1 inclusive. 0 means the segment's start and 1 means the segment's end. This is the "time" parameter used in the splines' formulas. A point's localF is not proportional to its distance from the segment's start. Depending on the spline, a value of 0.5 does not always mean the middle, distance wise, of the segment</param>
        public float LocalFToDistance(float localF)
        {
#if CURVY_SANITY_CHECKS
            Assert.IsTrue(localF >= 0);
            Assert.IsTrue(localF <= 1);
#endif
            if (ApproximationDistances.Length <= 1 || localF == 0)
                return 0;

            if (localF == 1f)
                return Length;

            float frag;
            int idx = getApproximationIndexINTERNAL(localF, out frag);
#if CURVY_SANITY_CHECKS
            Assert.IsTrue(idx >= 0);
            Assert.IsTrue(idx < ApproximationDistances.Length - 1);
#endif
            float d = ApproximationDistances[idx + 1] - ApproximationDistances[idx];
            return ApproximationDistances[idx] + d * frag;
        }

        /// <summary>
        /// Gets TF for a certain local F
        /// TF stands for Total Fragment. It's a value ranging from 0 to 1 inclusive. 0 means the spline's start and 1 means the spline's end. This is the "time" parameter used in the splines' formulas. A point's TF is not proportional to its distance from the spline's start. Depending on the spline, a value of 0.5 does not always mean the middle, distance wise, of the spline
        /// </summary>
        /// <param name="localF">localF stands for Local Fragment. It's a value ranging from 0 to 1 inclusive. 0 means the segment's start and 1 means the segment's end. This is the "time" parameter used in the splines' formulas. A point's localF is not proportional to its distance from the segment's start. Depending on the spline, a value of 0.5 does not always mean the middle, distance wise, of the segment</param>
        /// <returns>a TF value</returns>
        public float LocalFToTF(float localF)
        {
            return Spline.SegmentToTF(this, localF);
        }

        public override string ToString()
        {
            if (Spline != null)
                return Spline.name + "." + name;
            else
                return base.ToString();
        }

        /// <summary>
        /// Modify the control point's local rotation to match the segment's orientation
        /// </summary>
        public void BakeOrientationToTransform()
        {
#if CURVY_SANITY_CHECKS
            Assert.IsTrue(ApproximationUp.Length > 0);
#endif

            Quaternion orientation = GetOrientationFast(0);
            if (cachedTransform.localRotation.DifferentOrientation(orientation))
                SetLocalRotation(orientation);
        }

        /// <summary>
        /// Internal, gets the index of mApproximation by F and the remaining fragment
        /// </summary>
        public int getApproximationIndexINTERNAL(float localF, out float frag)
        {
            int approximationLength = Approximation.Length;

            float f = localF * (approximationLength - 1);

            int tempIndex = (int)f;
            int index = tempIndex <= 0
                ? 0
                : tempIndex >= approximationLength - 2
                    ? approximationLength - 2
                    : tempIndex;

            float tempFrag = f - index;
            frag = tempFrag <= 0
                ? 0
                : tempFrag >= 1
                    ? 1
                    : tempFrag;

            return index;
        }

        public void LinkToSpline(CurvySpline spline)
        {
#if CURVY_SANITY_CHECKS
            //The following assertion is commented because, when dragging CPs from spline A to Spline B, we might have B's SyncSplineFromHierarchy executed before A's, and will call A's CP.LinkToSpline(B) while A's CP still has mSpline != null
            //Assert.IsTrue(mSpline == null, name);
#endif

            mSpline = spline;
        }

        public void UnlinkFromSpline()
        {
#if CURVY_SANITY_CHECKS
            Assert.IsTrue(mSpline != null);
#endif
            mSpline = null;
        }

#if CONTRACTS_FULL
        [ContractInvariantMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
            //Contract.Invariant(mSpline == null || transform.parent.GetComponent<CurvySpline>() == mSpline);
            //Contract.Invariant((mSpline == null) == (mControlPointIndex == -1));
            //Contract.Invariant(mSpline == null || mControlPointIndex < mSpline.ControlPointCount);
            //Contract.Invariant(mSpline == null || mSpline.ControlPoints.ElementAt(mControlPointIndex) == this);
            Contract.Invariant(Connection == null || Connection.ControlPoints.Contains(this));
            Contract.Invariant(Connection == null || Spline != null);
            Contract.Invariant(FollowUp == null || Connection != null);

            //TODO CONTRACT reactivate these if you find a way to call GetNextControlPoint and GetPreviousControlPoint without modifying the cache
            //Contract.Invariant(FollowUp == null || Spline.GetNextControlPoint(this) == null || Spline.GetPreviousControlPoint(this) == null);
        }
#endif

        #region Update position and rotation

        /// <summary>
        /// Sets the local position while dirtying the spline, dirtying the connected splines, and updating the connected control points' positions accordingly.
        /// </summary>
        /// <param name="newPosition"></param>
        public void SetLocalPosition(Vector3 newPosition)
        {
            if (cachedTransform.localPosition != newPosition)
            {
                cachedTransform.localPosition = newPosition;
                Spline.SetDirtyPartial(this, SplineDirtyingType.Everything);
                if ((ConnectionSyncPosition || ConnectionSyncRotation) && Connection != null)
                    Connection.SetSynchronisationPositionAndRotation(
                        ConnectionSyncPosition ? cachedTransform.position : Connection.transform.position,
                        ConnectionSyncRotation ? cachedTransform.rotation : Connection.transform.rotation);
            }
        }

        /// <summary>
        /// Sets the global position while dirtying the spline, dirtying the connected splines, and updating the connected control points' positions accordingly.
        /// </summary>
        /// <param name="value"></param>
        public void SetPosition(Vector3 value)
        {
            if (cachedTransform.position != value)
            {
                cachedTransform.position = value;
                Spline.SetDirtyPartial(this, SplineDirtyingType.Everything);
                if ((ConnectionSyncPosition || ConnectionSyncRotation) && Connection != null)
                    Connection.SetSynchronisationPositionAndRotation(
                        ConnectionSyncPosition ? cachedTransform.position : Connection.transform.position,
                        ConnectionSyncRotation ? cachedTransform.rotation : Connection.transform.rotation);
            }
        }

        /// <summary>
        /// Sets the local rotation while dirtying the spline, dirtying the connected splines, and updating the connected control points' rotations accordingly.
        /// </summary>
        /// <param name="value"></param>
        public void SetLocalRotation(Quaternion value)
        {
            if (cachedTransform.localRotation != value)
            {
                cachedTransform.localRotation = value;
                if (OrientatinInfluencesSpline)
                    Spline.SetDirtyPartial(this, SplineDirtyingType.OrientationOnly);
                if ((ConnectionSyncPosition || ConnectionSyncRotation) && Connection != null)
                    Connection.SetSynchronisationPositionAndRotation(
                        ConnectionSyncPosition ? cachedTransform.position : Connection.transform.position,
                        ConnectionSyncRotation ? cachedTransform.rotation : Connection.transform.rotation);
            }
        }

        /// <summary>
        /// Sets the global rotation while dirtying the spline, dirtying the connected splines, and updating the connected control points' rotations accordingly.
        /// </summary>
        /// <param name="value"></param>
        public void SetRotation(Quaternion value)
        {
            if (cachedTransform.rotation != value)
            {
                cachedTransform.rotation = value;
                if (OrientatinInfluencesSpline)
                    Spline.SetDirtyPartial(this, SplineDirtyingType.OrientationOnly);
                if ((ConnectionSyncPosition || ConnectionSyncRotation) && Connection != null)
                    Connection.SetSynchronisationPositionAndRotation(
                        ConnectionSyncPosition ? cachedTransform.position : Connection.transform.position,
                        ConnectionSyncRotation ? cachedTransform.rotation : Connection.transform.rotation);
            }
        }

        #endregion

        /// <summary>
        /// Returns true if followUp can be associated with a heading direction of <see cref="ConnectionHeadingEnum.Minus"/>
        /// </summary>
        public static bool CanFollowUpHeadToStart([NotNull] CurvySplineSegment followUp)
        {
            return followUp.Spline.GetPreviousControlPointIndex(followUp) != -1;
        }

        /// <summary>
        /// Returns true if followUp can be associated with a heading direction of <see cref="ConnectionHeadingEnum.Plus"/>
        /// </summary>
        public static bool CanFollowUpHeadToEnd([NotNull] CurvySplineSegment followUp)
        {
            return followUp.Spline.GetNextControlPointIndex(followUp) != -1;
        }

        /// <summary>
        /// Gets the position of a point on the B-Spline
        /// </summary>
        /// <param name="controlPoints">The spline's control points.</param>
        /// <param name="tf">A value between 0 and 1 defining where the point is on the spline</param>
        /// <param name="isClamped"><see cref="CurvySpline.IsBSplineClamped"/></param>
        /// <param name="isClosed"><see cref="CurvySpline.Closed"/></param>
        /// <param name="degree"><see cref="CurvySpline.BSplineDegree"/></param>
        /// <param name="p0Array">An array used in internal computations. This is to avoid excessive allocations.The length of the array should be greater or equal to <paramref name="degree"/> + 1. The content of the array does not matter, since it gets overwritten by the method</param>
        public static Vector3 BSpline([NotNull] ReadOnlyCollection<CurvySplineSegment> controlPoints, float tf, bool isClamped, bool isClosed, int degree, [NotNull] Vector3[] p0Array)
        {
#if CURVY_SANITY_CHECKS
            Assert.IsTrue(p0Array.Length >= degree + 1);
            Assert.IsTrue(tf.IsBetween0And1());
#endif
            int controlPointsCount = controlPoints.Count;
            int n = BSplineHelper.GetBSplineN(controlPointsCount, degree, isClosed);
            BSplineHelper.GetBSplineUAndK(tf, isClamped, degree, n, out float u, out int k);
            GetBSplineP0s(controlPoints, controlPointsCount, degree, k, p0Array);
            return isClamped
                ? BSplineHelper.DeBoorClamped(degree, k, u, n + 1, p0Array)
                : BSplineHelper.DeBoorUnclamped(degree, k, u, p0Array);
        }

        #endregion

        #region ### Interface Implementations ###

        //IPoolable
        public void OnBeforePush()
        {
            this.StripComponents();
            Disconnect();
            DeleteMetadata();
        }

        //IPoolable
        public void OnAfterPop()
        {
            Reset();
        }

        #endregion
    }
}