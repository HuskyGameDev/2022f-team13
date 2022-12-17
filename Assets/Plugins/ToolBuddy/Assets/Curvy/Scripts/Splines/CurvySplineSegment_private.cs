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
using System.Runtime.CompilerServices;
using FluffyUnderware.Curvy.Pools;
#if CONTRACTS_FULL
using System.Diagnostics.Contracts;
#endif
using JetBrains.Annotations;
using ToolBuddy.Pooling.Collections;
using ToolBuddy.Pooling.Pools;
using UnityEngine.Assertions;


namespace FluffyUnderware.Curvy
{
    /// <summary>
    /// Class covering a Curvy Spline Segment / ControlPoint
    /// </summary>
    public partial class CurvySplineSegment : DTVersionedMonoBehaviour, IPoolable
    {

        #region ### Serialized Fields ###

        #region --- General ---

        [Group("General")]
        [FieldAction("CBBakeOrientation", Position = ActionAttribute.ActionPositionEnum.Below)]
        [Label("Bake Orientation", "Automatically apply orientation to CP transforms?")]
        [SerializeField]
        private bool m_AutoBakeOrientation;

        [Group("General")]
        [Tooltip("Check to use this transform's rotation")]
        [FieldCondition(nameof(IsOrientationAnchorEditable), true)]
        [SerializeField]
        private bool m_OrientationAnchor;

        [Label("Swirl", "Add Swirl to orientation?")]
        [Group("General")]
        [FieldCondition(nameof(canHaveSwirl), true)]
        [SerializeField]
        private CurvyOrientationSwirl m_Swirl = CurvySplineSegmentDefaultValues.Swirl;

        [Label("Turns", "Number of swirl turns")]
        [Group("General")]
        [FieldCondition(nameof(canHaveSwirl), true, false, ConditionalAttribute.OperatorEnum.AND, "m_Swirl", CurvyOrientationSwirl.None, true)]
        [SerializeField]
        private float m_SwirlTurns;

        #endregion

        #region --- Bezier ---

        [Section("Bezier Options", Sort = 1, HelpURL = CurvySpline.DOCLINK + "curvysplinesegment_bezier")]
        [GroupCondition(nameof(interpolation), CurvyInterpolation.Bezier)]
        [SerializeField]
        private bool m_AutoHandles = CurvySplineSegmentDefaultValues.AutoHandles;

        [RangeEx(0, 1, "Distance %", "Handle length by distance to neighbours")]
        [FieldCondition(nameof(m_AutoHandles), true, Action = ConditionalAttribute.ActionEnum.Enable)]
        [SerializeField]
        private float m_AutoHandleDistance = CurvySplineSegmentDefaultValues.AutoHandleDistance;

        [VectorEx(Precision = 3, Options = AttributeOptionsFlags.Clipboard | AttributeOptionsFlags.Negate, Color = "#FFFF00")]
        [SerializeField, FormerlySerializedAs("HandleIn")]
        private Vector3 m_HandleIn = CurvySplineSegmentDefaultValues.HandleIn;

        [VectorEx(Precision = 3, Options = AttributeOptionsFlags.Clipboard | AttributeOptionsFlags.Negate, Color = "#00FF00")]
        [SerializeField, FormerlySerializedAs("HandleOut")]
        private Vector3 m_HandleOut = CurvySplineSegmentDefaultValues.HandleOut;

        #endregion

        #region --- TCB ---

        [Section("TCB Options", Sort = 1, HelpURL = CurvySpline.DOCLINK + "curvysplinesegment_tcb")]
        [GroupCondition(nameof(interpolation), CurvyInterpolation.TCB)]
        [GroupAction("TCBOptionsGUI", Position = ActionAttribute.ActionPositionEnum.Below)]

        [Label("Local Tension", "Override Spline Tension?")]
        [SerializeField, FormerlySerializedAs("OverrideGlobalTension")]
        private bool m_OverrideGlobalTension;

        [Label("Local Continuity", "Override Spline Continuity?")]
        [SerializeField, FormerlySerializedAs("OverrideGlobalContinuity")]
        private bool m_OverrideGlobalContinuity;

        [Label("Local Bias", "Override Spline Bias?")]
        [SerializeField, FormerlySerializedAs("OverrideGlobalBias")]
        private bool m_OverrideGlobalBias;
        [Tooltip("Synchronize Start and End Values")]
        [SerializeField, FormerlySerializedAs("SynchronizeTCB")]
        private bool m_SynchronizeTCB = CurvySplineSegmentDefaultValues.SynchronizeTCB;
        [Label("Tension"), FieldCondition("m_OverrideGlobalTension", true)]
        [SerializeField, FormerlySerializedAs("StartTension")]
        private float m_StartTension;

        [Label("Tension (End)"), FieldCondition("m_OverrideGlobalTension", true, false, ConditionalAttribute.OperatorEnum.AND, "m_SynchronizeTCB", false, false)]
        [SerializeField, FormerlySerializedAs("EndTension")]
        private float m_EndTension;

        [Label("Continuity"), FieldCondition("m_OverrideGlobalContinuity", true)]
        [SerializeField, FormerlySerializedAs("StartContinuity")]
        private float m_StartContinuity;

        [Label("Continuity (End)"), FieldCondition("m_OverrideGlobalContinuity", true, false, ConditionalAttribute.OperatorEnum.AND, "m_SynchronizeTCB", false, false)]
        [SerializeField, FormerlySerializedAs("EndContinuity")]
        private float m_EndContinuity;

        [Label("Bias"), FieldCondition("m_OverrideGlobalBias", true)]
        [SerializeField, FormerlySerializedAs("StartBias")]
        private float m_StartBias;

        [Label("Bias (End)"), FieldCondition("m_OverrideGlobalBias", true, false, ConditionalAttribute.OperatorEnum.AND, "m_SynchronizeTCB", false, false)]
        [SerializeField, FormerlySerializedAs("EndBias")]
        private float m_EndBias;

        #endregion
        /*
#region --- CG Options ---
        
        /// <summary>
        /// Material ID (used by CG)
        /// </summary>
        [Section("Generator Options", true, Sort = 5, HelpURL = CurvySpline.DOCLINK + "curvysplinesegment_cg")]
        [Positive(Label="Material ID")]
        [SerializeField]
        int m_CGMaterialID;

        /// <summary>
        /// Whether to create a hard edge or not (used by PCG)
        /// </summary>
        [Label("Hard Edge")]
        [SerializeField]
        bool m_CGHardEdge;
        /// <summary>
        /// Maximum vertex distance when using optimization (0=infinite)
        /// </summary>
        [Positive(Label="Max Step Size",Tooltip="Max step distance when using optimization")]
        [SerializeField]
        float m_CGMaxStepDistance;
#endregion
        */
        #region --- Connections ---

        [SerializeField, HideInInspector] private CurvySplineSegment m_FollowUp;
        [SerializeField, HideInInspector] private ConnectionHeadingEnum m_FollowUpHeading = ConnectionHeadingEnum.Auto;
        //DESIGN: shouldn't these two be part of Connection? By spreading them on the ControlPoints, we risk a desynchronisation between m_ConnectionSyncPosition's value of a CP and the one of the connected CP
        [SerializeField, HideInInspector] private bool m_ConnectionSyncPosition;
        [SerializeField, HideInInspector] private bool m_ConnectionSyncRotation;

        [SerializeField, HideInInspector] private CurvyConnection m_Connection;

        #endregion

        #endregion

        #region ### Private Fields ###

        //Because Unity pre 2019 doesn't act like it is supposed to, I have to make two different codes for cachedTransform. Here is the issue:
        //cachedTransform is used as an optim. The idea is to get transform once at script's start, and then use it later. Execution order says that CurvySplineSegment runs before CurvySpline. So all CSS's OnEnable methods should run before CS's ones. But this is not the case in pre 2019. So you end up with CS's OnEnable accessing (through public members) to CSS's cachedTransform, which is still set to null because its OnEnable was not called yet.
#if (UNITY_2019_1_OR_NEWER)
        private Transform cachedTransform;
#else
        private Transform _cachedTransform;
        private Transform cachedTransform
        {
            get
            {
                if (ReferenceEquals(_cachedTransform, null))
                    _cachedTransform = transform;
                return _cachedTransform;
            }
            set
            {
                _cachedTransform = value;
            }
        }
#endif


        /// <summary>
        /// This exists because Transform can not be accessed in non main threads. So before refreshing the spline, we store the local position here so it can be accessed in multithread spline refreshing code
        /// </summary>
        /// <remarks>Warning: Make sure it is set with valid value before using it</remarks>
        private Vector3 threadSafeLocalPosition;
        /// <summary>
        /// Same as <see cref="threadSafeLocalPosition"/>, but for the next CP. Is equal to <see cref="threadSafeLocalPosition"/> if no next cp. Takes into consideration Follow-Ups if spline uses them to define its shape
        /// </summary>
        private Vector3 threadSafeNextCpLocalPosition;
        /// <summary>
        /// Same as <see cref="threadSafeLocalPosition"/>, but for the next CP. Is equal to <see cref="threadSafeLocalPosition"/> if no previous cp. Takes into consideration Follow-Ups if spline uses them to define its shape
        /// </summary>
        private Vector3 threadSafePreviousCpLocalPosition;
        /// <summary>
        /// This exists because Transform can not be accesed in non main threads. So before refreshing the spline, we store the local rotation here so it can be accessed in multithread spline refreshing code
        /// </summary>
        /// <remarks>Warning: Make sure it is set with valid value before using it</remarks>
        private Quaternion threadSafeLocalRotation;
        /// <summary>
        /// The cached result of Spline.GetNextControlPoint(this)
        /// OPTIM: use this more often?
        /// </summary>
        private CurvySplineSegment cachedNextControlPoint;

        private CurvySpline mSpline;
        private Bounds? mBounds;

        /// <summary>
        /// The Metadata components added to this GameObject
        /// </summary>
        private readonly HashSet<CurvyMetadataBase> mMetadata = new HashSet<CurvyMetadataBase>();
        /// <summary>
        /// The local position used in the segment approximations cache latest computation
        /// </summary>
        private Vector3 lastProcessedLocalPosition;
        /// <summary>
        /// The local rotation used in the segment approximations cache latest computation
        /// </summary>
        private Quaternion lastProcessedLocalRotation;

        #endregion

        #region ### Unity Callbacks ###
        /*! \cond UNITY */


        private void Awake()
        {
            //Happens when duplicating a spline that has a connection. This can be avoided
            if (Connection && Connection.ControlPointsList.Contains(this) == false)
                SetConnection(null);

            cachedTransform = transform;
            ReloadMetaData();
        }

        private void OnEnable()
        {
            Awake();
        }

#if UNITY_EDITOR

        private void OnDrawGizmos()
        {
            if (Spline && Spline.IsInitialized && Spline.ShowGizmos)
            {
                bool isSelected = Selection.Contains(gameObject.GetInstanceID());
                bool isThisOrParentSelected;
                {
                    if (isSelected)
                    {
                        isThisOrParentSelected = true;
                    }
                    else
                    {
                        Transform testedTransform = gameObject.transform.parent;
                        do
                        {
                            isThisOrParentSelected = Selection.Contains(testedTransform.gameObject.GetInstanceID());
                            testedTransform = testedTransform.parent;
                        }
                        while (!isThisOrParentSelected && ReferenceEquals(testedTransform, null) == false);
                    }
                }

                bool willOnDrawGizmosSelectedGetCalled;
                {
#if UNITY_2022_1_OR_NEWER
                    willOnDrawGizmosSelectedGetCalled = isSelected;
#else
                    willOnDrawGizmosSelectedGetCalled = isThisOrParentSelected;
#endif
                }

                if (willOnDrawGizmosSelectedGetCalled == false)
                {
#if UNITY_2022_1_OR_NEWER
                    doGizmos(isThisOrParentSelected);
#else
                    doGizmos(false);
#endif
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (Spline && Spline.IsInitialized)
                doGizmos(true);
        }
#endif

        private void OnDestroy()
        {
            //BUG? Why do we have that realDestroy boolean? Why not always do the same thing? This might hide something bad
            //When asked about this jake said:
            //That was quite a dirty hack as far as I remember, to counter issues with Unity's serialization
            //TBH I'm not sure if those issues still are present, so you might want to see if it's working without it now.
            bool realDestroy = true;
#if UNITY_EDITOR
            if (EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying)
                realDestroy = false;
#endif
            //Debug.Log("realDestroy " + realDestroy);
#if UNITY_EDITOR
            //mSpline is non null when the user delete only this CP. mSpline is null when the user deletes the spline, which then leads to this method to be called
            if (mSpline != null)
            {
                if (!Application.isPlaying &&
                    (CurvySpline._newSelectionInstanceIDINTERNAL == 0 || CurvySpline._newSelectionInstanceIDINTERNAL == GetInstanceID())
                    )
                {
                    if (Spline.GetPreviousControlPoint(this))
                        CurvySpline._newSelectionInstanceIDINTERNAL = Spline.GetPreviousControlPoint(this).GetInstanceID();
                    else if (Spline.GetNextControlPoint(this))
                        CurvySpline._newSelectionInstanceIDINTERNAL = Spline.GetNextControlPoint(this).GetInstanceID();
                    else
                        CurvySpline._newSelectionInstanceIDINTERNAL = mSpline.GetInstanceID();
                }
            }
#endif
            if (realDestroy)
            {
                Disconnect();
                if (bSplineP0Array.Count > 0)
                    ArrayPools.Vector3.Free(bSplineP0Array);
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            //Debug.Log("    OnValidate " + name);
            SetAutoHandles(m_AutoHandles);
            SetConnection(m_Connection);
            if (mSpline != null)
            {
                Spline.SetDirtyAll(SplineDirtyingType.Everything, true);
                Spline.InvalidateControlPointsRelationshipCacheINTERNAL();
            }
        }

#endif

        /// <summary>
        /// Resets the properties of this control point, but will not remove its Connection if it has any.
        /// </summary>
        public void Reset()
        {
            m_OrientationAnchor = false;
            m_Swirl = CurvySplineSegmentDefaultValues.Swirl;
            m_SwirlTurns = 0;
            // Bezier
            m_AutoHandles = CurvySplineSegmentDefaultValues.AutoHandles;
            m_AutoHandleDistance = CurvySplineSegmentDefaultValues.AutoHandleDistance;
            m_HandleIn = CurvySplineSegmentDefaultValues.HandleIn;
            m_HandleOut = CurvySplineSegmentDefaultValues.HandleOut;
            // TCB
            m_SynchronizeTCB = CurvySplineSegmentDefaultValues.SynchronizeTCB;
            m_OverrideGlobalTension = false;
            m_OverrideGlobalContinuity = false;
            m_OverrideGlobalBias = false;
            m_StartTension = 0;
            m_EndTension = 0;
            m_StartContinuity = 0;
            m_EndContinuity = 0;
            m_StartBias = 0;
            m_EndBias = 0;
            if (mSpline)
            {
                Spline.SetDirty(this, SplineDirtyingType.Everything);
                Spline.InvalidateControlPointsRelationshipCacheINTERNAL();
            }
        }
        /*! \endcond */
        #endregion

        #region ### Privates & Internals ###
        /*! \cond PRIVATE */

        #region Properties used in inspector's field condition and group condition

        // Used as a group condition
        private CurvyInterpolation interpolation
        {
            get { return Spline ? Spline.Interpolation : CurvyInterpolation.Linear; }
        }

        // Used as a field condition
        private bool isDynamicOrientation
        {
            get { return Spline && Spline.Orientation == CurvyOrientation.Dynamic; }
        }

        // Used as a field condition
        private bool IsOrientationAnchorEditable
        {
            get
            {
                CurvySpline curvySpline = Spline;
                return isDynamicOrientation && curvySpline.IsControlPointVisible(this) && curvySpline.FirstVisibleControlPoint != this && curvySpline.LastVisibleControlPoint != this;
            }
        }

        // Used as a field condition
        private bool canHaveSwirl
        {
            get
            {
                CurvySpline curvySpline = Spline;
                return isDynamicOrientation && curvySpline && curvySpline.IsControlPointAnOrientationAnchor(this) && (curvySpline.Closed || curvySpline.LastVisibleControlPoint != this);
            }
        }

        #endregion

        #region BSplines

        /// <summary>
        /// A subArray used in the computation of B-Splines, to avoid arrays computation at each computation
        /// </summary>
        private SubArray<Vector3> bSplineP0Array;

        /// <summary>
        /// A subArray used in the computation of B-Splines, to avoid arrays computation at each computation
        /// </summary>
        private SubArray<Vector3> BSplineP0Array
        {
            get
            {
                if (bSplineP0Array.Count != mSpline.BSplineDegree + 1)
                {
                    ArrayPool<Vector3> arrayPool = ArrayPools.Vector3;
                    if (bSplineP0Array.Count > 0)
                        arrayPool.Free((SubArray<Vector3>)bSplineP0Array);
                    bSplineP0Array = arrayPool.Allocate(Spline.BSplineDegree + 1, false);
                }
                return bSplineP0Array;
            }
        }

        /// <summary>
        /// Fills <paramref name="pArray"/> with the P0s numbers as defined in the B-Spline section, De Boor's algorithm, here: https://pages.mtu.edu/~shene/COURSES/cs3621/NOTES/
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void GetBSplineP0s([NotNull] ReadOnlyCollection<CurvySplineSegment> controlPoints, int controlPointsCount, int degree, int k, [NotNull] Vector3[] pArray)
        {
#if CURVY_SANITY_CHECKS
            Assert.IsTrue(pArray.Length >= degree + 1);
#endif
            for (int j = 0; j <= degree; j++)
            {
                int index = j + k - degree;
                pArray[j] = controlPoints[
                        index < controlPointsCount
                            ? index
                            : (index - controlPointsCount)
                    ]
                    .threadSafeLocalPosition;
            }
        }

        #endregion

        #region Extrinsic properties

        private ControlPointExtrinsicProperties extrinsicPropertiesINTERNAL;

        /// <summary>
        /// Properties describing the relationship between this CurvySplineSegment and its containing CurvySpline.
        /// </summary>
        internal void SetExtrinsicPropertiesINTERNAL(ControlPointExtrinsicProperties value)
        {
            extrinsicPropertiesINTERNAL = value;
        }
        internal ref readonly ControlPointExtrinsicProperties GetExtrinsicPropertiesINTERNAL()
        {
            return ref extrinsicPropertiesINTERNAL;
        }

        #endregion

        private void CheckAgainstMetaDataDuplication()
        {
            if (Metadata.Count > 1)
            {
                HashSet<Type> metaDataTypes = new HashSet<Type>();
                foreach (CurvyMetadataBase metaData in Metadata)
                {
                    Type componentType = metaData.GetType();
                    if (metaDataTypes.Contains(componentType))
                        DTLog.LogWarning(String.Format("[Curvy] Game object '{0}' has multiple Components of type '{1}'. Control Points should have no more than one Component instance for each MetaData type.", this.ToString(), componentType), this);
                    else
                        metaDataTypes.Add(componentType);
                }
            }
        }

        /// <summary>
        /// Sets the connection handler this Control Point is using
        /// </summary>
        /// <param name="newConnection"></param>
        /// <returns>Whether a modification was done or not</returns>
        /// <remarks>If set to null, FollowUp wil be set to null to</remarks>
        private bool SetConnection(CurvyConnection newConnection)
        {
            bool modificationDone = false;
            if (m_Connection != newConnection)
            {
                modificationDone = true;
                m_Connection = newConnection;
            }
            if (m_Connection == null && m_FollowUp != null)
            {
                modificationDone = true;
                m_FollowUp = null;
            }
            return modificationDone;
        }

        /// <summary>
        /// Returns a different ConnectionHeadingEnum value when connectionHeading has a value that is no more valid in the context of this spline. For example, heading to start (Minus) when there is no previous CP
        /// </summary>
        private static ConnectionHeadingEnum GetValidateConnectionHeading(ConnectionHeadingEnum connectionHeading, [CanBeNull] CurvySplineSegment followUp)
        {
            if (followUp == null)
                return connectionHeading;

            if ((connectionHeading == ConnectionHeadingEnum.Minus && CanFollowUpHeadToStart(followUp) == false)
                || (connectionHeading == ConnectionHeadingEnum.Plus && CanFollowUpHeadToEnd(followUp) == false))
                return ConnectionHeadingEnum.Auto;

            return connectionHeading;
        }

        /// <summary>
        /// Sets Auto Handles. When setting it the value of connected control points is also updated
        /// </summary>
        /// <param name="newValue"></param>
        /// <returns>Whether a modifcation was done or not</returns>
        private bool SetAutoHandles(bool newValue)
        {
            bool modificationDone = false;
            if (Connection)
            {
                ReadOnlyCollection<CurvySplineSegment> controlPoints = Connection.ControlPointsList;
                for (int index = 0; index < controlPoints.Count; index++)
                {
                    CurvySplineSegment controlPoint = controlPoints[index];
                    modificationDone = modificationDone || controlPoint.m_AutoHandles != newValue;
                    controlPoint.m_AutoHandles = newValue;
                }
            }
            else
            {
                modificationDone = m_AutoHandles != newValue;
                m_AutoHandles = newValue;
            }
            return modificationDone;
        }

        #region approximations cache computation

        internal void refreshCurveINTERNAL()
        {
            CurvySpline spline = Spline;
            bool isControlPointASegment = spline.IsControlPointASegment(this);
            int newCacheSize;
            if (isControlPointASegment)
            {
#if CURVY_SANITY_CHECKS
                Assert.IsNotNull(cachedNextControlPoint);
#endif
                newCacheSize = CurvySpline.CalculateCacheSize(
                    spline.CacheDensity,
                    (cachedNextControlPoint.threadSafeLocalPosition.Subtraction(threadSafeLocalPosition)).magnitude,
                    spline.MaxPointsPerUnit);
            }
            else
                newCacheSize = 0;

            Array.Resize(ref Approximation, newCacheSize + 1);
            Array.Resize(ref ApproximationT, newCacheSize + 1);
            Array.Resize(ref ApproximationDistances, newCacheSize + 1);
            Array.Resize(ref ApproximationUp, newCacheSize + 1);

            bool hasNextControlPoint = ReferenceEquals(cachedNextControlPoint, null) == false;

            //set Approximation[0] and Approximation[newCacheSize]
            switch (interpolation)
            {
                case CurvyInterpolation.Linear:
                case CurvyInterpolation.CatmullRom:
                case CurvyInterpolation.TCB:
                case CurvyInterpolation.Bezier:
                    Approximation[0] = threadSafeLocalPosition;
                    if (newCacheSize != 0)
                        Approximation[newCacheSize] = hasNextControlPoint
                            ? cachedNextControlPoint.threadSafeLocalPosition
                            : threadSafeLocalPosition;
                    break;
                case CurvyInterpolation.BSpline:
                    if (isControlPointASegment)
                    {
                        Approximation[0] = BSpline(spline.ControlPointsList, spline.SegmentToTF(this), spline.IsBSplineClamped, spline.Closed, spline.BSplineDegree, BSplineP0Array.Array);
                        Approximation[newCacheSize] = BSpline(spline.ControlPointsList, spline.SegmentToTF(this, 1), spline.IsBSplineClamped, spline.Closed, spline.BSplineDegree, BSplineP0Array.Array);
                    }
                    else
                        //staying coherent with other interpolation types. TODO check why Approximation should have the element 0 when not a segment. What happens if we simply keep the array empty?
                        Approximation[0] = threadSafeLocalPosition;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            ApproximationDistances[0] = 0;
            //  ApproximationT[0] and ApproximationUp[0] are handled later
            mBounds = null;
            Length = 0;

            if (isControlPointASegment)
            {
                float segmentLength = 0;
                switch (spline.Interpolation)
                {
                    case CurvyInterpolation.BSpline:
                        segmentLength = InterpolateBSplineSegment(newCacheSize);
                        break;
                    case CurvyInterpolation.Bezier:
                        segmentLength = InterpolateBezierSegment(cachedNextControlPoint, newCacheSize);
                        break;
                    case CurvyInterpolation.CatmullRom:
                        segmentLength = InterpolateCatmullSegment(cachedNextControlPoint, newCacheSize);
                        break;
                    case CurvyInterpolation.TCB:
                        segmentLength = InterpolateTCBSegment(cachedNextControlPoint, newCacheSize, spline.Tension, spline.Continuity, spline.Bias);
                        break;
                    case CurvyInterpolation.Linear:
                        segmentLength = InterpolateLinearSegment(cachedNextControlPoint, newCacheSize);
                        break;
                    default:
                        DTLog.LogError("[Curvy] Invalid interpolation value " + spline.Interpolation, this);
                        break;
                }
                Length = segmentLength;

                Vector3 tangent = Approximation[newCacheSize].Subtraction(Approximation[newCacheSize - 1]);
                Length += tangent.magnitude;
                ApproximationDistances[newCacheSize] = Length;
                ApproximationT[newCacheSize - 1] = tangent.normalized;
                // ApproximationT[cacheSize] is set in Spline's Refresh method
                ApproximationT[newCacheSize] = ApproximationT[newCacheSize - 1];
            }
            else
            {
                if (hasNextControlPoint)
                    ApproximationT[0] = (cachedNextControlPoint.threadSafeLocalPosition.Subtraction(Approximation[0])).normalized;
                else
                {
                    short previousControlPointIndex = spline.GetPreviousControlPointIndex(this);
                    if (previousControlPointIndex != -1)
                        ApproximationT[0] = (Approximation[0].Subtraction(spline.ControlPointsList[previousControlPointIndex].threadSafeLocalPosition)).normalized;
                    else
                        ApproximationT[0] = threadSafeLocalRotation * Vector3.forward;
                }

                //Last visible control point gets the last tangent from the previous segment. This is done in Spline's Refresh method 

            }

            lastProcessedLocalPosition = threadSafeLocalPosition;
        }


        #region Inlined code for optimization


        private float InterpolateBSplineSegment(int newCacheSize)
        {
            CurvySpline spline = Spline;

            float mStepSize = 1f / newCacheSize;
            float lengthAccumulator = 0;

            bool isClamped = spline.IsBSplineClamped;
            ReadOnlyCollection<CurvySplineSegment> controlPoints = spline.ControlPointsList;
            int degree = spline.BSplineDegree;
            float segmentTF = spline.SegmentToTF(this);
            float tfIncrement = mStepSize / spline.Count;

            int controlPointsCount = controlPoints.Count;
            int n = BSplineHelper.GetBSplineN(controlPointsCount, degree, spline.Closed);
            int previousK = int.MinValue;

            SubArray<Vector3> splinePsVector = BSplineP0Array;
            Vector3[] ps = splinePsVector.Array;
            int psCount = splinePsVector.Count;

            SubArray<Vector3> psCopySubArray = ArrayPools.Vector3.Allocate(psCount);
            Vector3[] psCopy = psCopySubArray.Array;

            int nPlus1 = n + 1;
            for (int i = 1; i < newCacheSize; i++)
            {
                float tf = segmentTF + tfIncrement * i;
                BSplineHelper.GetBSplineUAndK(tf, isClamped, degree, n, out float u, out int k);

                if (k != previousK)
                {
                    GetBSplineP0s(controlPoints, controlPointsCount, degree, k, ps);
                    previousK = k;
                }

                Array.Copy(ps, 0, psCopy, 0, psCount);

                Approximation[i] = isClamped ? BSplineHelper.DeBoorClamped(degree, k, u, nPlus1, psCopy) : BSplineHelper.DeBoorUnclamped(degree, k, u, psCopy);

                Vector3 delta = Approximation[i].Subtraction(Approximation[i - 1]);
                lengthAccumulator += delta.magnitude;
                ApproximationDistances[i] = lengthAccumulator;
                ApproximationT[i - 1] = OptimizedOperators.Normalize(delta);
            }

            ArrayPools.Vector3.Free(psCopySubArray);

            return lengthAccumulator;
        }

        private float InterpolateBezierSegment(CurvySplineSegment nextControlPoint, int newCacheSize)
        {
            //The following code is the inlined version of this code:
            //        for (int i = 1; i < CacheSize; i++)
            //        {
            //            Approximation[i] = interpolateBezier(i * mStepSize);
            //            t = (Approximation[i] - Approximation[i - 1]);
            //            Length += t.magnitude;
            //            ApproximationDistances[i] = Length;
            //            ApproximationT[i - 1] = t.normalized;
            //        }

            float mStepSize = 1f / newCacheSize;

            float lengthAccumulator = 0;
            CurvySplineSegment ncp = nextControlPoint;
            Vector3 p0 = threadSafeLocalPosition;
            Vector3 t0 = p0 + HandleOut;
            Vector3 p1 = ncp.threadSafeLocalPosition;
            Vector3 t1 = p1 + ncp.HandleIn;

            const double Ft2 = 3;
            const double Ft3 = -3;
            const double Fu1 = 3;
            const double Fu2 = -6;
            const double Fu3 = 3;
            const double Fv1 = -3;
            const double Fv2 = 3;

            double FAX = -p0.x + Ft2 * t0.x + Ft3 * t1.x + p1.x;
            double FBX = Fu1 * p0.x + Fu2 * t0.x + Fu3 * t1.x;
            double FCX = Fv1 * p0.x + Fv2 * t0.x;
            double FDX = p0.x;

            double FAY = -p0.y + Ft2 * t0.y + Ft3 * t1.y + p1.y;
            double FBY = Fu1 * p0.y + Fu2 * t0.y + Fu3 * t1.y;
            double FCY = Fv1 * p0.y + Fv2 * t0.y;
            double FDY = p0.y;

            double FAZ = -p0.z + Ft2 * t0.z + Ft3 * t1.z + p1.z;
            double FBZ = Fu1 * p0.z + Fu2 * t0.z + Fu3 * t1.z;
            double FCZ = Fv1 * p0.z + Fv2 * t0.z;
            double FDZ = p0.z;
            Vector3 tangent;

            for (int i = 1; i < newCacheSize; i++)
            {
                float localF = i * mStepSize;

                Approximation[i].x = (float)(((FAX * localF + FBX) * localF + FCX) * localF + FDX);
                Approximation[i].y = (float)(((FAY * localF + FBY) * localF + FCY) * localF + FDY);
                Approximation[i].z = (float)(((FAZ * localF + FBZ) * localF + FCZ) * localF + FDZ);

                tangent.x = (Approximation[i].x - Approximation[i - 1].x);
                tangent.y = (Approximation[i].y - Approximation[i - 1].y);
                tangent.z = (Approximation[i].z - Approximation[i - 1].z);

                float tMagnitude = Mathf.Sqrt((float)(tangent.x * tangent.x + tangent.y * tangent.y + tangent.z * tangent.z));
                lengthAccumulator += tMagnitude;
                ApproximationDistances[i] = lengthAccumulator;
                if ((double)tMagnitude > 9.99999974737875E-06)
                {
                    float oneOnMagnitude = 1 / tMagnitude;
                    ApproximationT[i - 1].x = tangent.x * oneOnMagnitude;
                    ApproximationT[i - 1].y = tangent.y * oneOnMagnitude;
                    ApproximationT[i - 1].z = tangent.z * oneOnMagnitude;
                }
                else
                {
                    ApproximationT[i - 1].x = 0;
                    ApproximationT[i - 1].y = 0;
                    ApproximationT[i - 1].z = 0;
                }
            }
            return lengthAccumulator;
        }


        private float InterpolateTCBSegment(CurvySplineSegment nextControlPoint, int newCacheSize, float splineTension, float splineContinuity, float splineBias)
        {
            //The following code is the inlined version of this code:
            //        for (int i = 1; i < CacheSize; i++)
            //        {
            //            Approximation[i] = interpolateCatmull(i * mStepSize);
            //            t = (Approximation[i] - Approximation[i - 1]);
            //            Length += t.magnitude;
            //            ApproximationDistances[i] = Length;
            //            ApproximationT[i - 1] = t.normalized;
            //        }

            float mStepSize = 1f / newCacheSize;

            float lengthAccumulator = 0;

            float ft0 = StartTension;
            float ft1 = EndTension;
            float fc0 = StartContinuity;
            float fc1 = EndContinuity;
            float fb0 = StartBias;
            float fb1 = EndBias;

            if (!OverrideGlobalTension)
                ft0 = ft1 = splineTension;
            if (!OverrideGlobalContinuity)
                fc0 = fc1 = splineContinuity;
            if (!OverrideGlobalBias)
                fb0 = fb1 = splineBias;

            Vector3 p0 = threadSafeLocalPosition;
            Vector3 p1 = threadSafeNextCpLocalPosition;
            Vector3 t0 = threadSafePreviousCpLocalPosition;
            Vector3 t1 = nextControlPoint.threadSafeNextCpLocalPosition;

            double FFA = (1 - ft0) * (1 + fc0) * (1 + fb0);
            double FFB = (1 - ft0) * (1 - fc0) * (1 - fb0);
            double FFC = (1 - ft1) * (1 - fc1) * (1 + fb1);
            double FFD = (1 - ft1) * (1 + fc1) * (1 - fb1);

            double DD = 2;
            double Ft1 = -FFA / DD;
            double Ft2 = (+4 + FFA - FFB - FFC) / DD;
            double Ft3 = (-4 + FFB + FFC - FFD) / DD;
            double Ft4 = FFD / DD;
            double Fu1 = +2 * FFA / DD;
            double Fu2 = (-6 - 2 * FFA + 2 * FFB + FFC) / DD;
            double Fu3 = (+6 - 2 * FFB - FFC + FFD) / DD;
            double Fu4 = -FFD / DD;
            double Fv1 = -FFA / DD;
            double Fv2 = (FFA - FFB) / DD;
            double Fv3 = FFB / DD;
            double Fw2 = +2 / DD;

            double FAX = Ft1 * t0.x + Ft2 * p0.x + Ft3 * p1.x + Ft4 * t1.x;
            double FBX = Fu1 * t0.x + Fu2 * p0.x + Fu3 * p1.x + Fu4 * t1.x;
            double FCX = Fv1 * t0.x + Fv2 * p0.x + Fv3 * p1.x;
            double FDX = Fw2 * p0.x;

            double FAY = Ft1 * t0.y + Ft2 * p0.y + Ft3 * p1.y + Ft4 * t1.y;
            double FBY = Fu1 * t0.y + Fu2 * p0.y + Fu3 * p1.y + Fu4 * t1.y;
            double FCY = Fv1 * t0.y + Fv2 * p0.y + Fv3 * p1.y;
            double FDY = Fw2 * p0.y;

            double FAZ = Ft1 * t0.z + Ft2 * p0.z + Ft3 * p1.z + Ft4 * t1.z;
            double FBZ = Fu1 * t0.z + Fu2 * p0.z + Fu3 * p1.z + Fu4 * t1.z;
            double FCZ = Fv1 * t0.z + Fv2 * p0.z + Fv3 * p1.z;
            double FDZ = Fw2 * p0.z;
            Vector3 tangent;
            for (int i = 1; i < newCacheSize; i++)
            {
                float localF = i * mStepSize;

                Approximation[i].x = (float)(((FAX * localF + FBX) * localF + FCX) * localF + FDX);
                Approximation[i].y = (float)(((FAY * localF + FBY) * localF + FCY) * localF + FDY);
                Approximation[i].z = (float)(((FAZ * localF + FBZ) * localF + FCZ) * localF + FDZ);

                tangent.x = (Approximation[i].x - Approximation[i - 1].x);
                tangent.y = (Approximation[i].y - Approximation[i - 1].y);
                tangent.z = (Approximation[i].z - Approximation[i - 1].z);

                float tMagnitude = Mathf.Sqrt((float)(tangent.x * tangent.x + tangent.y * tangent.y + tangent.z * tangent.z));
                lengthAccumulator += tMagnitude;
                ApproximationDistances[i] = lengthAccumulator;
                if ((double)tMagnitude > 9.99999974737875E-06)
                {
                    float oneOnMagnitude = 1 / tMagnitude;
                    ApproximationT[i - 1].x = tangent.x * oneOnMagnitude;
                    ApproximationT[i - 1].y = tangent.y * oneOnMagnitude;
                    ApproximationT[i - 1].z = tangent.z * oneOnMagnitude;
                }
                else
                {
                    ApproximationT[i - 1].x = 0;
                    ApproximationT[i - 1].y = 0;
                    ApproximationT[i - 1].z = 0;
                }
            }
            return lengthAccumulator;
        }

        private float InterpolateCatmullSegment(CurvySplineSegment nextControlPoint, int newCacheSize)
        {
            //The following code is the inlined version of this code:
            //        for (int i = 1; i < CacheSize; i++)
            //        {
            //            Approximation[i] = interpolateTCB(i * mStepSize);
            //            t = (Approximation[i] - Approximation[i - 1]);
            //            Length += t.magnitude;
            //            ApproximationDistances[i] = Length;
            //            ApproximationT[i - 1] = t.normalized;
            //        }

            float mStepSize = 1f / newCacheSize;

            float lengthAccumulator = 0;

            Vector3 p0 = threadSafeLocalPosition;
            Vector3 p1 = threadSafeNextCpLocalPosition;
            Vector3 t0 = threadSafePreviousCpLocalPosition;
            Vector3 t1 = nextControlPoint.threadSafeNextCpLocalPosition;

            const double Ft1 = -0.5;
            const double Ft2 = 1.5;
            const double Ft3 = -1.5;
            const double Ft4 = 0.5;
            const double Fu2 = -2.5;
            const double Fu3 = 2;
            const double Fu4 = -0.5;
            const double Fv1 = -0.5;
            const double Fv3 = 0.5;

            double FAX = Ft1 * t0.x + Ft2 * p0.x + Ft3 * p1.x + Ft4 * t1.x;
            double FBX = t0.x + Fu2 * p0.x + Fu3 * p1.x + Fu4 * t1.x;
            double FCX = Fv1 * t0.x + Fv3 * p1.x;
            double FDX = p0.x;

            double FAY = Ft1 * t0.y + Ft2 * p0.y + Ft3 * p1.y + Ft4 * t1.y;
            double FBY = t0.y + Fu2 * p0.y + Fu3 * p1.y + Fu4 * t1.y;
            double FCY = Fv1 * t0.y + Fv3 * p1.y;
            double FDY = p0.y;

            double FAZ = Ft1 * t0.z + Ft2 * p0.z + Ft3 * p1.z + Ft4 * t1.z;
            double FBZ = t0.z + Fu2 * p0.z + Fu3 * p1.z + Fu4 * t1.z;
            double FCZ = Fv1 * t0.z + Fv3 * p1.z;
            double FDZ = p0.z;
            Vector3 tangent;
            for (int i = 1; i < newCacheSize; i++)
            {
                float localF = i * mStepSize;

                Approximation[i].x = (float)(((FAX * localF + FBX) * localF + FCX) * localF + FDX);
                Approximation[i].y = (float)(((FAY * localF + FBY) * localF + FCY) * localF + FDY);
                Approximation[i].z = (float)(((FAZ * localF + FBZ) * localF + FCZ) * localF + FDZ);

                tangent.x = (Approximation[i].x - Approximation[i - 1].x);
                tangent.y = (Approximation[i].y - Approximation[i - 1].y);
                tangent.z = (Approximation[i].z - Approximation[i - 1].z);

                float tMagnitude = Mathf.Sqrt((float)(tangent.x * tangent.x + tangent.y * tangent.y + tangent.z * tangent.z));
                lengthAccumulator += tMagnitude;
                ApproximationDistances[i] = lengthAccumulator;
                if ((double)tMagnitude > 9.99999974737875E-06)
                {
                    float oneOnMagnitude = 1 / tMagnitude;
                    ApproximationT[i - 1].x = tangent.x * oneOnMagnitude;
                    ApproximationT[i - 1].y = tangent.y * oneOnMagnitude;
                    ApproximationT[i - 1].z = tangent.z * oneOnMagnitude;
                }
                else
                {
                    ApproximationT[i - 1].x = 0;
                    ApproximationT[i - 1].y = 0;
                    ApproximationT[i - 1].z = 0;
                }
            }
            return lengthAccumulator;
        }

        private float InterpolateLinearSegment(CurvySplineSegment nextControlPoint, int newCacheSize)
        {
            //The following code is the inlined version of this code:
            //        for (int i = 1; i < CacheSize; i++)
            //        {
            //            Approximation[i] = interpolateLinear(i * mStepSize);
            //            t = (Approximation[i] - Approximation[i - 1]);
            //            Length += t.magnitude;
            //            ApproximationDistances[i] = Length;
            //            ApproximationT[i - 1] = t.normalized;
            //        }

            float mStepSize = 1f / newCacheSize;

            float lengthAccumulator = 0;
            Vector3 pStart = threadSafeLocalPosition;
            Vector3 pEnd = nextControlPoint.threadSafeLocalPosition;
            Vector3 tangent;
            for (int i = 1; i < newCacheSize; i++)
            {
                float localF = i * mStepSize;
                Approximation[i] = OptimizedOperators.LerpUnclamped(pStart, pEnd, localF);

                tangent.x = (Approximation[i].x - Approximation[i - 1].x);
                tangent.y = (Approximation[i].y - Approximation[i - 1].y);
                tangent.z = (Approximation[i].z - Approximation[i - 1].z);

                float tMagnitude = Mathf.Sqrt((float)(tangent.x * tangent.x + tangent.y * tangent.y + tangent.z * tangent.z));
                lengthAccumulator += tMagnitude;
                ApproximationDistances[i] = lengthAccumulator;
                if ((double)tMagnitude > 9.99999974737875E-06)
                {
                    float oneOnMagnitude = 1 / tMagnitude;
                    ApproximationT[i - 1].x = tangent.x * oneOnMagnitude;
                    ApproximationT[i - 1].y = tangent.y * oneOnMagnitude;
                    ApproximationT[i - 1].z = tangent.z * oneOnMagnitude;
                }
                else
                {
                    ApproximationT[i - 1].x = 0;
                    ApproximationT[i - 1].y = 0;
                    ApproximationT[i - 1].z = 0;
                }
            }
            return lengthAccumulator;
        }

        #endregion

        internal void refreshOrientationNoneINTERNAL()
        {
            Array.Clear(ApproximationUp, 0, ApproximationUp.Length);
            lastProcessedLocalRotation = threadSafeLocalRotation;
        }

        internal void refreshOrientationStaticINTERNAL()
        {
            Vector3 firstUp = ApproximationUp[0] = getOrthoUp0INTERNAL();
            if (Approximation.Length > 1)
            {
                int cachedCachesize = CacheSize;
                Vector3 lastUp = ApproximationUp[cachedCachesize] = getOrthoUp1INTERNAL();
                float oneOnCachSize = 1f / cachedCachesize;
                for (int i = 1; i < cachedCachesize; i++)
                    ApproximationUp[i] = Vector3.SlerpUnclamped(firstUp, lastUp, i * oneOnCachSize);
            }

            lastProcessedLocalRotation = threadSafeLocalRotation;
        }

        /// <summary>
        /// Set each point's up as the initialUp rotated by the same rotation than the one that rotates initial tangent to the point's tangent
        /// </summary>
        /// <remarks>Does not handle swirl</remarks>
        /// <param name="initialUp"></param>
        internal void refreshOrientationDynamicINTERNAL(Vector3 initialUp)
        {
            int upsLength = ApproximationUp.Length;
            ApproximationUp[0] = initialUp;
            for (int i = 1; i < upsLength; i++)
            {
                //Inlined version of ups[i] = DTMath.ParallelTransportFrame(ups[i-1], tangents[i - 1], tangents[i]) and with less checks for performance reasons
                Vector3 tan0 = ApproximationT[i - 1];
                Vector3 tan1 = ApproximationT[i];
                //Inlined version of Vector3 A = Vector3.Cross(tan0, tan1);
                Vector3 A;
                {
                    A.x = tan0.y * tan1.z - tan0.z * tan1.y;
                    A.y = tan0.z * tan1.x - tan0.x * tan1.z;
                    A.z = tan0.x * tan1.y - tan0.y * tan1.x;
                }
                //Inlined version of float a = (float)Math.Atan2(A.magnitude, Vector3.Dot(tan0, tan1));
                float a = (float)Math.Atan2(
                    Math.Sqrt(A.x * A.x + A.y * A.y + A.z * A.z),
                    tan0.x * tan1.x + tan0.y * tan1.y + tan0.z * tan1.z);
                ApproximationUp[i] = Quaternion.AngleAxis(Mathf.Rad2Deg * a, A) * ApproximationUp[i - 1];
            }

            lastProcessedLocalRotation = threadSafeLocalRotation;
        }

        #endregion

        internal void ClearBoundsINTERNAL()
        {
            mBounds = null;
        }

        /// <summary>
        /// Gets Transform.up orthogonal to ApproximationT[0]
        /// </summary>
        internal Vector3 getOrthoUp0INTERNAL()
        {
            Vector3 u = threadSafeLocalRotation * Vector3.up;
            Vector3.OrthoNormalize(ref ApproximationT[0], ref u);
            return u;
        }

        private Vector3 getOrthoUp1INTERNAL()
        {
            CurvySplineSegment nextControlPoint = Spline.GetNextControlPoint(this);
            Quaternion nextRotation = nextControlPoint
                ? nextControlPoint.threadSafeLocalRotation
                : threadSafeLocalRotation;
            Vector3 u = nextRotation * Vector3.up;
            Vector3.OrthoNormalize(ref ApproximationT[CacheSize], ref u);
            return u;
        }

        internal void UnsetFollowUpWithoutDirtyingINTERNAL()
        {
            m_FollowUp = null;
            m_FollowUpHeading = ConnectionHeadingEnum.Auto;
        }

#if UNITY_EDITOR

#endif

        #region Gizmo drawing

        private static readonly Plane[] gizomTestCameraPlanes = new Plane[6];
        private static Vector3 gizomTestCamearPosition;
        private static Vector3 gizomTestCameraForward;
        private static float gizomTestFov;
        private static float gizomTestPixelWidth;
        private static float gizomTestPixelHeight;


        private void doGizmos(bool selected)
        {
            //OPTIM try multithreading some of the loops in this method. All loops have in them operations that unity forbids the usage of outside the main thread (drawline, worldtoscreen, ...). Maybe using Unity's jobs?
            if (CurvyGlobalManager.Gizmos == CurvySplineGizmos.None)
                return;

            Camera currentCamera = Camera.current;
            int cameraPixelWidth = currentCamera.pixelWidth;
            int cameraPixelHeight = currentCamera.pixelHeight;
            Transform cameraTransform = currentCamera.transform;
            Vector3 cameraPosition = cameraTransform.position;
            Vector3 cameraZDirection;
            Vector3 cameraXDirection;
            {
                Quaternion cameraRotation = cameraTransform.rotation;
                Vector3 direction;
                {
                    direction.x = 0;
                    direction.y = 0;
                    direction.z = 1;
                }
                cameraZDirection = cameraRotation * direction;

                {
                    direction.x = 1;
                    direction.y = 0;
                    direction.z = 0;
                }
                cameraXDirection = cameraRotation * direction;
            }
            Bounds bounds = Bounds;


            //Update gizomTestCameraPlanes if camera changed
            if (gizomTestCamearPosition != cameraPosition ||
                gizomTestCameraForward != cameraZDirection ||
                gizomTestPixelWidth != cameraPixelWidth ||
                gizomTestPixelHeight != cameraPixelHeight ||
                gizomTestFov != currentCamera.fieldOfView)
            {
                //Design Reading and writing static fields can be dangerous if this code is multi-threaded
                gizomTestCamearPosition = cameraPosition;
                gizomTestCameraForward = cameraZDirection;
                gizomTestPixelWidth = cameraPixelWidth;
                gizomTestPixelHeight = cameraPixelHeight;
                gizomTestFov = currentCamera.fieldOfView;
                GeometryUtility.CalculateFrustumPlanes(currentCamera, gizomTestCameraPlanes);
            }

            // Skip if the segment isn't in view
            if (!GeometryUtility.TestPlanesAABB(gizomTestCameraPlanes, bounds))
                return;

            CurvySpline spline = Spline;
            Transform splineTransform = spline.transform;
            Vector3 splineTransformLocalScale = splineTransform.localScale;
            Vector3 scale;
            {
                scale.x = 1 / splineTransformLocalScale.x;
                scale.y = 1 / splineTransformLocalScale.y;
                scale.z = 1 / splineTransformLocalScale.z;
            }
            Color splineGizmoColor = (selected) ? spline.GizmoSelectionColor : spline.GizmoColor;
            Vector3 transformPosition = transform.position;
            float cameraCenterWidth = cameraPixelWidth * 0.5f;
            float cameraCenterHeight = cameraPixelHeight * 0.5f;

            bool viewCurve = CurvyGlobalManager.ShowCurveGizmo;

            // Control Point
            if (viewCurve)
            {
                Gizmos.color = splineGizmoColor;
                float handleSize = DTUtility.GetHandleSize(transformPosition, currentCamera, cameraCenterWidth, cameraCenterHeight, cameraPosition, cameraZDirection, cameraXDirection);
                float cpGizmoSize = handleSize * (selected ? 1 : 0.7f) * CurvyGlobalManager.GizmoControlPointSize;

                if (spline.RestrictTo2D)
                    Gizmos.DrawCube(transformPosition, OptimizedOperators.Multiply(Vector3.one, cpGizmoSize));
                else
                    Gizmos.DrawSphere(transformPosition, cpGizmoSize);
            }

            //Remaining
            if (spline.IsControlPointASegment(this))
            {
                if (spline.Dirty)
                    spline.Refresh();

                Matrix4x4 initialGizmoMatrix = Gizmos.matrix;
                Matrix4x4 currentGizmoMatrix = Gizmos.matrix = splineTransform.localToWorldMatrix;

                //Spline lines
                if (viewCurve)
                {
                    float steps;
                    {
                        float camDistance = (cameraPosition.Subtraction(bounds.ClosestPoint(cameraPosition))).magnitude;

                        float df = Mathf.Clamp(camDistance, 1, 3000) / 3000;
                        df = (df < 0.01f) ? DTTween.SineOut(df, 0, 1) : DTTween.QuintOut(df, 0, 1);

                        steps = Mathf.Clamp((Length * CurvyGlobalManager.SceneViewResolution * 0.1f) / df, 1, 10000);
                    }
                    DrawGizmoLines(1 / steps);
                }

                //Approximations
                if (Approximation.Length > 0 && CurvyGlobalManager.ShowApproximationGizmo)
                {
                    Gizmos.color = spline.GizmoColor.Multiply(0.8f);
                    Vector3 size = OptimizedOperators.Multiply(0.1f, scale);
                    for (int i = 0; i < Approximation.Length; i++)
                    {
                        float handleSize = DTUtility.GetHandleSize(currentGizmoMatrix.MultiplyPoint3x4(Approximation[i]), currentCamera, cameraCenterWidth, cameraCenterHeight, cameraPosition, cameraZDirection, cameraXDirection);

                        Gizmos.DrawCube(Approximation[i], handleSize.Multiply(size));
                    }
                }

                //Orientation
                if (spline.Orientation != CurvyOrientation.None && ApproximationUp.Length > 0 && CurvyGlobalManager.ShowOrientationGizmo)
                {
                    Gizmos.color = CurvyGlobalManager.GizmoOrientationColor;
                    Vector3 orientationGizmoSize = scale.Multiply(CurvyGlobalManager.GizmoOrientationLength);

                    for (int i = 0; i < ApproximationUp.Length; i++)
                    {
                        Vector3 lineEnd;
                        lineEnd.x = Approximation[i].x + ApproximationUp[i].x * orientationGizmoSize.x;
                        lineEnd.y = Approximation[i].y + ApproximationUp[i].y * orientationGizmoSize.y;
                        lineEnd.z = Approximation[i].z + ApproximationUp[i].z * orientationGizmoSize.z;

                        Gizmos.DrawLine(Approximation[i], lineEnd);
                    }


                    if (spline.IsControlPointAnOrientationAnchor(this) && spline.Orientation == CurvyOrientation.Dynamic)
                    {
                        if (ApproximationUp.Length != 0)
                        {
                            Gizmos.color = CurvyGlobalManager.GizmoOrientationColor;
                            Vector3 u = ApproximationUp[0];
                            u.Set(u.x * scale.x, u.y * scale.y, u.z * scale.z);
                            Gizmos.DrawRay(Approximation[0],
                                CurvyGlobalManager.GizmoOrientationLength * 1.75f * u);
                        }
                    }
                }

                //Tangent
                if (ApproximationT.Length > 0 && CurvyGlobalManager.ShowTangentsGizmo)
                {
                    int segmentCacheSize = CacheSize;
                    float tangentSize = CurvyGlobalManager.GizmoOrientationLength;
                    for (int i = 0; i < ApproximationT.Length; i++)
                    {
                        //updating gizmo color
                        if (i == 0)
                            Gizmos.color = Color.blue;
                        else if (i == 1)
                            Gizmos.color = GizmoTangentColor;
                        else if (i == segmentCacheSize)
                            Gizmos.color = Color.black;

                        Vector3 lineEnd;
                        lineEnd.y = Approximation[i].y + ApproximationT[i].y * tangentSize;
                        lineEnd.z = Approximation[i].z + ApproximationT[i].z * tangentSize;
                        lineEnd.x = Approximation[i].x + ApproximationT[i].x * tangentSize;

                        Gizmos.DrawLine(Approximation[i], lineEnd);
                    }
                }
                Gizmos.matrix = initialGizmoMatrix;
            }

        }

        /// <summary>
        /// Draw gizmo lines representing the spline segment
        /// </summary>
        /// <param name="stepSize">The relative distance between the start and end of each line. Must be exclusively between 0 and 1</param>
        private void DrawGizmoLines(float stepSize)
        {
            CurvySpline spline = Spline;
            CurvyInterpolation splineInterpolation = spline.Interpolation;

#if CURVY_SANITY_CHECKS
            if (spline.Dirty)
                DTLog.LogWarning("Interpolate should not be called on segment of a dirty spline. Call CurvySpline.Refresh first", this);
            Assert.IsTrue(spline.IsControlPointASegment(this));
            Assert.IsTrue(spline.IsCpsRelationshipCacheValidINTERNAL);
            Assert.IsTrue(stepSize > 0);
            Assert.IsTrue(stepSize <= 1);
#endif
            if (splineInterpolation == CurvyInterpolation.Linear)
                Gizmos.DrawLine(Interpolate(0), Interpolate(1));
            else
            {
                Vector3 startPoint;
                if (splineInterpolation == CurvyInterpolation.BSpline)
                    startPoint = BSpline(spline.ControlPointsList, spline.SegmentToTF(this), spline.IsBSplineClamped, spline.Closed, spline.BSplineDegree, BSplineP0Array.Array);
                else
                    startPoint = threadSafeLocalPosition;

                //used only in BSplines for performance reasons
                bool isBSplineClamped = default;
                int bSplineDegree = default;
                ReadOnlyCollection<CurvySplineSegment> controlPoints = default;
                int controlPointsCount = default;
                float segmentTF = default;
                int n = default;
                int nPlus1 = default;
                int previousK = default;
                Vector3[] ps = default;
                int psCount = default;
                Vector3[] psCopy = default;
                SubArray<Vector3> psCopySubArray = default;
                if (splineInterpolation == CurvyInterpolation.BSpline)
                {
                    isBSplineClamped = spline.IsBSplineClamped;
                    bSplineDegree = spline.BSplineDegree;
                    controlPoints = spline.ControlPointsList;
                    segmentTF = spline.SegmentToTF(this);
                    controlPointsCount = controlPoints.Count;
                    n = BSplineHelper.GetBSplineN(controlPointsCount, bSplineDegree, spline.Closed);
                    nPlus1 = n + 1;
                    previousK = int.MinValue;
                    SubArray<Vector3> splinePsVector = BSplineP0Array;
                    ps = splinePsVector.Array;
                    psCount = splinePsVector.Count;
                    psCopySubArray = ArrayPools.Vector3.Allocate(psCount);
                    psCopy = psCopySubArray.Array;
                }

                for (float localF = 0; localF < 1; localF += stepSize)
                {
                    Vector3 interpolatedPoint;
                    {
                        Vector3 result;
                        //Inlined version of Interpolate, stripped from some code for performance reasons
                        //If you modify this, modify also the inlined version of this method in refreshCurveINTERNAL()
                        switch (splineInterpolation)
                        {
                            case CurvyInterpolation.BSpline:
                                {

                                    float tf = segmentTF + localF / spline.Count;
                                    BSplineHelper.GetBSplineUAndK(tf, isBSplineClamped, bSplineDegree, n, out float u, out int k);
                                    if (k != previousK)
                                    {
                                        GetBSplineP0s(controlPoints, controlPointsCount, bSplineDegree, k, ps);
                                        previousK = k;
                                    }
                                    Array.Copy(ps, 0, psCopy, 0, psCount);
                                    result = isBSplineClamped ? BSplineHelper.DeBoorClamped(bSplineDegree, k, u, nPlus1, psCopy) : BSplineHelper.DeBoorUnclamped(bSplineDegree, k, u, psCopy);
                                    break;
                                }
                            case CurvyInterpolation.CatmullRom:
                                {
                                    result = CurvySpline.CatmullRom(threadSafePreviousCpLocalPosition,
                                        threadSafeLocalPosition,
                                        threadSafeNextCpLocalPosition,
                                        cachedNextControlPoint.threadSafeNextCpLocalPosition,
                                        localF);
                                }
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
                            case CurvyInterpolation.TCB:
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
                                break;
                            default:
                                DTLog.LogError("[Curvy] Invalid interpolation value " + splineInterpolation, this);
                                result = startPoint;
                                break;
                        }

                        interpolatedPoint = result;
                    }

                    Gizmos.DrawLine(startPoint, interpolatedPoint);
                    startPoint = interpolatedPoint;
                }

                if (interpolation == CurvyInterpolation.BSpline)
                    ArrayPools.Vector3.Free(psCopySubArray);

                Vector3 endPoint;
                if (splineInterpolation == CurvyInterpolation.BSpline)
                    endPoint = BSpline(spline.ControlPointsList, spline.SegmentToTF(this, 1), spline.IsBSplineClamped, spline.Closed, spline.BSplineDegree, BSplineP0Array.Array);
                else
                    endPoint = threadSafeNextCpLocalPosition;
                ;
                Gizmos.DrawLine(startPoint, endPoint);
            }
        }
        #endregion

        /// <summary>
        /// Set the correct values to the thread safe local positions and rotation
        /// When multithreading, you can't access Transform in the not main threads. Here we cache that data so it is available for threads
        /// </summary>
        internal void PrepareThreadCompatibleDataINTERNAL(bool useFollowUp)
        {
            CurvySpline spline = Spline;
            CurvySplineSegment previousCP = spline.GetPreviousControlPoint(this);
            CurvySplineSegment nextCP = spline.GetNextControlPoint(this);

            //TODO: get rid of this the day you will be able to access transforms in threads
            threadSafeLocalPosition = cachedTransform.localPosition;
            threadSafeLocalRotation = cachedTransform.localRotation;

            //This isn't cached for thread compatibility, but for performance
            cachedNextControlPoint = nextCP;

            if (useFollowUp)
            {
                CurvySplineSegment followUpPreviousCP;
                bool hasFollowUp = FollowUp != null;
                if (hasFollowUp && ReferenceEquals(spline.FirstVisibleControlPoint, this))
                    followUpPreviousCP = CurvySpline.GetFollowUpHeadingControlPoint(FollowUp, this.FollowUpHeading);
                else
                    followUpPreviousCP = previousCP;
                CurvySplineSegment followUpNextCP;
                if (hasFollowUp && ReferenceEquals(spline.LastVisibleControlPoint, this))
                    followUpNextCP = CurvySpline.GetFollowUpHeadingControlPoint(FollowUp, this.FollowUpHeading);
                else
                    followUpNextCP = nextCP;

                if (followUpPreviousCP != null)
                {
                    threadSafePreviousCpLocalPosition = ReferenceEquals(followUpPreviousCP.Spline, spline) ?
                        followUpPreviousCP.cachedTransform.localPosition :
                        spline.transform.InverseTransformPoint(followUpPreviousCP.cachedTransform.position);
                }
                else
                    threadSafePreviousCpLocalPosition = threadSafeLocalPosition;

                if (followUpNextCP != null)
                {
                    threadSafeNextCpLocalPosition = ReferenceEquals(followUpNextCP.Spline, spline) ?
                        followUpNextCP.cachedTransform.localPosition :
                        spline.transform.InverseTransformPoint(followUpNextCP.cachedTransform.position);
                }
                else
                    threadSafeNextCpLocalPosition = threadSafeLocalPosition;
            }
            else
            {
                threadSafePreviousCpLocalPosition = ReferenceEquals(previousCP, null) == false ? previousCP.cachedTransform.localPosition :
                    threadSafeLocalPosition;

                threadSafeNextCpLocalPosition = ReferenceEquals(nextCP, null) == false ? nextCP.cachedTransform.localPosition :
                    threadSafeLocalPosition;
            }
        }

        /*! \endcond */
        #endregion
    }
}