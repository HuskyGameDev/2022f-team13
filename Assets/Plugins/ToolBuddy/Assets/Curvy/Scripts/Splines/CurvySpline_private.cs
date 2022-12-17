// =====================================================================
// Copyright 2013-2022 ToolBuddy
// All rights reserved
// 
// http://www.toolbuddy.net
// =====================================================================

using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using FluffyUnderware.Curvy.Utils;
using FluffyUnderware.DevTools;
using UnityEngine.Serialization;
using FluffyUnderware.DevTools.Extensions;
using System.Reflection;
using JetBrains.Annotations;
#if UNITY_EDITOR
using UnityEditor;
#endif
#if CONTRACTS_FULL
using System.Diagnostics.Contracts;
#endif
using UnityEngine.Assertions;


namespace FluffyUnderware.Curvy
{
    /// <summary>
    /// Curvy Spline class
    /// </summary>
    public partial class CurvySpline : DTVersionedMonoBehaviour
    {
        #region ### Privates Fields ###

        #region ### Serialized fields ###

        #region --- General ---

        [Section("General", HelpURL = CurvySpline.DOCLINK + "curvyspline_general")]
        [Tooltip("Interpolation Method")]
        [SerializeField, FormerlySerializedAs("Interpolation")]
        private CurvyInterpolation m_Interpolation = CurvyGlobalManager.DefaultInterpolation;

        [Tooltip("Restrict Control Points to a local 2D plane")]
        [SerializeField]
        private bool m_RestrictTo2D;

        [Tooltip("The local 2D plane to restrict the spline's control points to")]
        [SerializeField]
        [FieldCondition(nameof(RestrictTo2D), true)]
        [FieldAction("CBCheck2DPlanar")]
        private CurvyPlane restricted2DPlane = CurvyPlane.XY;

        [SerializeField, FormerlySerializedAs("Closed")]
        private bool m_Closed;

        [FieldCondition(nameof(CanHaveManualEndCp), Action = ActionAttribute.ActionEnum.Enable)]
        [Tooltip("Handle End Control Points automatically?")]
        [SerializeField, FormerlySerializedAs("AutoEndTangents")]
        private bool m_AutoEndTangents = CurvySplineDefaultValues.AutoEndTangents;

        [Tooltip("Orientation Flow")]
        [SerializeField, FormerlySerializedAs("Orientation")]
        private CurvyOrientation m_Orientation = CurvySplineDefaultValues.Orientation;

        #endregion

        #region --- Bezier Options ---

        [Section("Global Bezier Options", HelpURL = CurvySpline.DOCLINK + "curvyspline_bezier")]
        [GroupCondition(nameof(m_Interpolation), CurvyInterpolation.Bezier)]
        [RangeEx(0, 1, "Default Distance %", "Handle length by distance to neighbours")]
        [SerializeField]
        private float m_AutoHandleDistance = CurvySplineDefaultValues.AutoHandleDistance;

        #endregion

        #region --- TCB Options ---

        [Section("Global TCB Options", HelpURL = CurvySpline.DOCLINK + "curvyspline_tcb")]
        [GroupCondition(nameof(m_Interpolation), CurvyInterpolation.TCB)]
        [GroupAction("TCBOptionsGUI", Position = ActionAttribute.ActionPositionEnum.Below)]
        [SerializeField, FormerlySerializedAs("Tension")]
        private float m_Tension;

        [SerializeField, FormerlySerializedAs("Continuity")]
        private float m_Continuity;

        [SerializeField, FormerlySerializedAs("Bias")]
        private float m_Bias;
        #endregion

        #region --- B-Spline Options ---

        [Section("B-Spline Options", HelpURL = CurvySpline.DOCLINK + "curvyspline_bspline")]
        [GroupCondition(nameof(m_Interpolation), CurvyInterpolation.BSpline)]
        [RangeEx(minBSplineDegree, nameof(MaxBSplineDegree), "Degree", "The degree of the piecewise polynomial functions.\nIs in the range [2; control points count - 1]")]
        [SerializeField]
        private int bSplineDegree = CurvySplineDefaultValues.BSplineDegree;


        [FieldCondition(nameof(CanBeClamped), Action = ActionAttribute.ActionEnum.Enable)]
        [Label("Clamped", "Make the curve pass through the first and last control points by increasing the multiplicity of the first and last knots.\n\nIn technical terms, when this parameter is true, the knot vector is [0, 0, ...,0, 1, 2, ..., N-1, N, N, ..., N]. When false, it is [0, 1, 2, ..., N-1, N]")]
        [SerializeField]
        private bool isBSplineClamped = CurvySplineDefaultValues.IsBSplineClamped;

        #endregion

        #region --- Advanced Settings ---

        [Section("Advanced Settings", HelpURL = CurvySpline.DOCLINK + "curvyspline_advanced")]
        [FieldAction("ShowGizmoGUI", Position = ActionAttribute.ActionPositionEnum.Above)]
        [Label("Color", "Gizmo color")]
        [SerializeField]
        private Color m_GizmoColor = CurvyGlobalManager.DefaultGizmoColor;

        [FieldAction("CheckGizmoColor", Position = ActionAttribute.ActionPositionEnum.Above)]
        [FieldAction("CheckGizmoSelectionColor", Position = ActionAttribute.ActionPositionEnum.Below)]
        [Label("Active Color", "Selected Gizmo color")]
        [SerializeField]
        private Color m_GizmoSelectionColor = CurvyGlobalManager.DefaultGizmoSelectionColor;

        [RangeEx(1, 100)]
        [SerializeField, FormerlySerializedAs("Granularity"), Tooltip("Defines how densely the cached points are. When the value is 100, the number of cached points per world distance unit is equal to the spline's MaxPointsPerUnit")]
        private int m_CacheDensity = CurvySplineDefaultValues.CacheDensity;
        [SerializeField, Tooltip("The maximum number of sampling points per world distance unit. Sampling is used in caching or shape extrusion for example")]
        private float m_MaxPointsPerUnit = CurvySplineDefaultValues.MaxPointsPerUnit;
        [SerializeField, Tooltip("Use a GameObject pool at runtime")]
        private bool m_UsePooling = CurvySplineDefaultValues.UsePooling;
        [SerializeField, Tooltip("Use threading where applicable. Threading is is currently not supported when targetting WebGL and Universal Windows Platform")]
        private bool m_UseThreading;
        [Tooltip("Refresh when Control Point position change?")]
        [SerializeField, FormerlySerializedAs("AutoRefresh")]
        private bool m_CheckTransform = CurvySplineDefaultValues.CheckTransform;
        [SerializeField] private CurvyUpdateMethod m_UpdateIn = CurvySplineDefaultValues.UpdateIn;
        #endregion

        #region --- Events ---

        /*! \cond PRIVATE */

        [Group("Events", Expanded = false, Sort = 1000, HelpURL = DOCLINK + "curvyspline_events")]
        [SortOrder(0)]
        [SerializeField]
        protected CurvySplineEvent m_OnRefresh = new CurvySplineEvent();
        [Group("Events", Sort = 1000)]
        [SortOrder(1)]
        [SerializeField]
        protected CurvySplineEvent m_OnAfterControlPointChanges = new CurvySplineEvent();
        [Group("Events", Sort = 1000)]
        [SortOrder(2)]
        [SerializeField]
        protected CurvyControlPointEvent m_OnBeforeControlPointAdd = new CurvyControlPointEvent();
        [Group("Events", Sort = 1000)]
        [SortOrder(3)]
        [SerializeField]
        protected CurvyControlPointEvent m_OnAfterControlPointAdd = new CurvyControlPointEvent();
        [Group("Events", Sort = 1000)]
        [SortOrder(4)]
        [SerializeField]
        protected CurvyControlPointEvent m_OnBeforeControlPointDelete = new CurvyControlPointEvent();

        /*! \endcond */

        #endregion

        #endregion

        private bool mIsInitialized;

        private bool isStarted;
        private bool sendOnRefreshEventNextUpdate;
        private readonly object controlPointsRelationshipCacheLock = new object();
#if UNITY_EDITOR
        private bool transformChildrenChanged;
        private bool syncHierarchyFromSplineNeeded;
#endif

        //OPTIM Instead of having a segments list, use the controlPointsList, while providing the methods to convert from a segment index to a control point index.
        /// <summary>
        /// Controlpoints that start a valid spline segment
        /// </summary>
        private readonly List<CurvySplineSegment> mSegments = new List<CurvySplineSegment>();
        /// <summary>
        /// Read-only version of <see cref="controlPoints"/>
        /// </summary>
        private ReadOnlyCollection<CurvySplineSegment> readOnlyControlPoints;

        private float length = -1;
        private int mCacheSize = -1;
        private Bounds? mBounds;
        private bool mDirtyCurve;
        private bool mDirtyOrientation;
        private readonly HashSet<CurvySplineSegment> dirtyControlPointsMinimalSet = new HashSet<CurvySplineSegment>();

        private readonly List<CurvySplineSegment> dirtyCpsExtendedList = new List<CurvySplineSegment>();
        //DESIGN I think allControlPointsAreDirty can be removed, and related code can just fill dirtyControlPointsMinimalSet with all control points instead. Check that perfs lose is not significant before doing so.
        private bool allControlPointsAreDirty;
        //TODO mThreadWorker is disposable. CurvySpline should dispose it. See rule CA1001
        //https://docs.microsoft.com/en-us/visualstudio/code-quality/ca1001-types-that-own-disposable-fields-should-be-disposable?view=vs-2019
        private readonly ThreadPoolWorker<CurvySplineSegment> mThreadWorker = new ThreadPoolWorker<CurvySplineSegment>();

        //reusable events
        private readonly CurvySplineEventArgs defaultSplineEventArgs;
        private readonly CurvyControlPointEventArgs defaultAddAfterEventArgs;
        private readonly CurvyControlPointEventArgs defaultDeleteEventArgs;

        /// <summary>
        /// ControlPointsDistances[i] is equal to ControlPoints[i].Distance. ControlPointsDistances exists only to make search time shorter when searching for a Cp based on its Distance
        /// </summary>
        private float[] controlPointsDistances = new float[0];

        private readonly Action<CurvySplineSegment, int, int> refreshCurveAction;

#if CURVY_SANITY_CHECKS
        private int sanityErrorLogsThisFrame;
        private int sanityWaringLogsThisFrame;
#endif

        #region Keeping track of transform's change

        /// <summary>
        /// The global position of the spline the last time it was checked. Checks are done at least once a frame.
        /// </summary>
        private Vector3 lastProcessedPosition;
        /// <summary>
        /// The global rotation of the spline the last time it was checked. Checks are done at least once a frame.
        /// </summary>
        private Quaternion lastProcessedRotation;
        /// <summary>
        /// The global scale of the spline the last time it was checked. Checks are done at least once a frame.
        /// </summary>
        private Vector3 lastProcessedScale;
        /// <summary>
        /// True if the global position, rotation or scale of the spline has changed this frame
        /// </summary>
        private bool globalCoordinatesChangedThisFrame;

        #endregion


        #region ControlPoints relastionship cache

        private bool isCpsRelationshipCacheValid;
        private CurvySplineSegment firstSegment;
        private CurvySplineSegment lastSegment;
        private CurvySplineSegment firstVisibleControlPoint;
        private CurvySplineSegment lastVisibleControlPoint;

        #endregion

        #endregion


        #region ### Unity Callbacks ###
        /*! \cond UNITY */
#if UNITY_EDITOR
        private void OnValidate()
        {
            //Debug.Log("OnValidate " + name);

            Closed = m_Closed;
            Interpolation = m_Interpolation;
            AutoEndTangents = m_AutoEndTangents;
            MaxPointsPerUnit = m_MaxPointsPerUnit;
            BSplineDegree = bSplineDegree;

            InvalidateControlPointsRelationshipCacheINTERNAL();
            SetDirtyAll(SplineDirtyingType.Everything, true);
        }
#endif

        private void Awake()
        {
            cachedTransform = transform;

            //Debug.Log("Awake " + name);

            if (UsePooling)
            {
                //Create the CurvyGlobalManager if not existing already
                _ = CurvyGlobalManager.Instance;
            }
        }

        private void OnEnable()
        {
            cachedTransform = transform;

            SyncSplineFromHierarchy();
#if UNITY_EDITOR
            EditorApplication.update += EditorUpdate;
#endif
            if (isStarted)
            {
                bool processedDirtyCps = Initialize();
                if (processedDirtyCps)
                    OnRefreshEvent(defaultSplineEventArgs);

            }
        }

        /// <summary>
        /// Initialize the spline. This is called automatically by Unity at the first frame.
        /// The only situation I see where you will need to call it manually is if you instantiate a CurvySpline via Unity API, and need to use it the same frame before Unity calls Start() on it the next frame.
        /// </summary>
        public void Start()
        {
            //Debug.Log("Start");
            if (isStarted == false)
            {
                bool processedDirtyCps = Initialize();
                isStarted = true;
                if (processedDirtyCps)
                    OnRefreshEvent(defaultSplineEventArgs);
            }
        }

        private void OnDisable()
        {
            //Debug.Log("OnDisable " + name);
            mIsInitialized = false;
#if UNITY_EDITOR
            EditorApplication.update -= EditorUpdate;
#endif
        }

        private void OnDestroy()
        {
            //BUG? Why do we have that realDestroy boolean? Why not always do the same thing? This might hide something bad
            //When asked about this jake said:
            //That was quite a dirty hack as far as I remember, to counter issues with Unity's serialization
            //TBH I'm not sure if those issues still are present, so you might want to see if it's working without it now.
            //Debug.Log("OnDestroy " + name);
            bool realDestroy = true;
#if UNITY_EDITOR
            if (EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying)
                realDestroy = false;
#endif
            if (realDestroy)
            {
                if (UsePooling && Application.isPlaying)
                {
                    CurvyGlobalManager curvyGlobalManager = CurvyGlobalManager.Instance;
                    if (curvyGlobalManager != null)
                        for (int i = 0; i < ControlPointCount; i++)
                            curvyGlobalManager.ControlPointPool.Push(ControlPoints[i]);
                }
                else
                    mThreadWorker.Dispose();
            }
            ClearControlPoints();
            isStarted = false;
        }

#if UNITY_EDITOR
        private void OnTransformChildrenChanged()
        {
            transformChildrenChanged = true;
        }
#endif


        virtual protected void Reset()
        {
            //Debug.Log("Reset " + name);

            Interpolation = CurvyGlobalManager.DefaultInterpolation;
            RestrictTo2D = false;
            AutoHandleDistance = 0.39f;
            Closed = false;
            AutoEndTangents = CurvySplineDefaultValues.AutoEndTangents;
            // Orientation
            Orientation = CurvySplineDefaultValues.Orientation;
            // Advanced
            GizmoColor = CurvyGlobalManager.DefaultGizmoColor;
            GizmoSelectionColor = CurvyGlobalManager.DefaultGizmoSelectionColor;
            CacheDensity = CurvySplineDefaultValues.CacheDensity;
            MaxPointsPerUnit = CurvySplineDefaultValues.MaxPointsPerUnit;
            CheckTransform = CurvySplineDefaultValues.CheckTransform;
            // TCB
            Tension = 0;
            Continuity = 0;
            Bias = 0;
            BSplineDegree = CurvySplineDefaultValues.BSplineDegree;
            IsBSplineClamped = CurvySplineDefaultValues.IsBSplineClamped;
            SyncSplineFromHierarchy();
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (syncHierarchyFromSplineNeeded)
            {
                syncHierarchyFromSplineNeeded = false;
                SyncHierarchyFromSpline();
            }
#endif
#if CURVY_SANITY_CHECKS
            if (Application.isPlaying)
            {
                sanityWaringLogsThisFrame = 0;
                sanityErrorLogsThisFrame = 0;
            }
#endif

            if (Application.isPlaying && UpdateIn == CurvyUpdateMethod.Update)
                DoUpdate();
        }

        private void LateUpdate()
        {
            if (Application.isPlaying && UpdateIn == CurvyUpdateMethod.LateUpdate)
                DoUpdate();
        }

        private void FixedUpdate()
        {
            if (Application.isPlaying && UpdateIn == CurvyUpdateMethod.FixedUpdate)
                DoUpdate();
        }
        /*! \endcond */
        #endregion

        #region ### Privates & Internals ###
        /*! \cond PRIVATE */

        private const float MinimalMaxPointsPerUnit = 0.0001f;
        /// <summary>
        /// The maximal size of the cache of a spline's segment
        /// </summary>
        private const float MaxSegmentCacheSize = 1000000;

        private static readonly string InvalidCPErrorMessage = "[Curvy] Method called with a control point '{0}' that is not part of the current spline '{1}'";

#if CURVY_SANITY_CHECKS
        /// <summary>
        /// Returns isCpsRelationshipCacheValid. Getter was created just for the sake of some sanity checks
        /// </summary>
        internal bool IsCpsRelationshipCacheValidINTERNAL
        {
            get { return isCpsRelationshipCacheValid; }
        }
#endif

        //cachedTransform in CurvySplineSegment is subject to an issue in Unity that leads to a bug. Read its comment to understand what it is. The same issue applies to CurvySpline, but does not lead to a known bug. So just as a precaution, I am avoiding the issue here too
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
        /// Access the list of Segments
        /// </summary>
        /// <remarks>The returned list should not be modified</remarks>
        private List<CurvySplineSegment> Segments
        {
            get
            {
                if (isCpsRelationshipCacheValid == false)
                    RebuildControlPointsRelationshipCache(true);
                return mSegments;
            }
        }

        private const int minBSplineDegree = 2;//update CurvySplineDefaultValues.BSplineDegree and documentation/tooltip of IsBSplineClamped if you modify this
        private int MaxBSplineDegree => Mathf.Max(minBSplineDegree, ControlPoints.Count - 1);

#if UNITY_EDITOR
        public static int _newSelectionInstanceIDINTERNAL; // Editor Bridge helper to determine new selection after object deletion
#endif

#if CONTRACTS_FULL
        [ContractInvariantMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
            Contract.Invariant(MaxPointsPerUnit.IsANumber());
            Contract.Invariant(MaxPointsPerUnit > 0);

            //TODO CONTRACT reactivate these if you find a way to call GetSegmentIndex and IsSegment without modifying the cache
            //Contract.Invariant(Contract.ForAll(Segments, s => GetSegmentIndex(s) == Segments.IndexOf(s)));
            //Contract.Invariant(Contract.ForAll(Segments, s => IsSegment(s)));

            //TODO CONTRACT more code contracts
            Contract.Invariant(Contract.ForAll(ControlPoints, cp => cp.Spline == this));
        }
#endif

        private bool Initialize()
        {
            SetDirtyAll(SplineDirtyingType.Everything, false);
            bool processedDirtyCps = ProcessDirtyControlPoints();
            UpdatedLastProcessedGlobalCoordinates();
            mIsInitialized = true;
            return processedDirtyCps;
        }

#if CURVY_SANITY_CHECKS

        [System.Diagnostics.Conditional("UNITY_ASSERTIONS")]
        private void DoSanityChecks()
        {
            const int limit = 20;
            if (!IsInitialized)
            {
                if (sanityErrorLogsThisFrame < limit)
                {
                    if (sanityErrorLogsThisFrame == limit - 1)
                        DTLog.LogError("[Curvy] Too many errors to display.", this);
                    else
                        DTLog.LogError("[Curvy] Calling public method on non initialized spline.", this);
                    sanityErrorLogsThisFrame++;
                }
            }
            else if (Dirty)
            {
                if (sanityWaringLogsThisFrame < limit)
                {
                    if (sanityWaringLogsThisFrame == limit - 1)
                        DTLog.LogWarning("[Curvy] Too many warnings to display.", this);
                    else
                        DTLog.LogWarning(string.Format(System.Globalization.CultureInfo.InvariantCulture, "[Curvy] Calling public method on a dirty spline. The returned result will not be up to date. Either refresh the spline manually by calling Refresh(), or wait for it to be refreshed automatically at the next {0} call", UpdateIn.ToString()), this);
                    sanityWaringLogsThisFrame++;
                }
            }
        }
#endif

#if UNITY_EDITOR
        private void EditorUpdate()
        {
#if CURVY_SANITY_CHECKS
            if (Application.isPlaying == false)
            {
                sanityWaringLogsThisFrame = 0;
                sanityErrorLogsThisFrame = 0;
            }
#endif
            if (Application.isPlaying == false && IsInitialized)
            {
                if (syncHierarchyFromSplineNeeded)
                {
                    syncHierarchyFromSplineNeeded = false;
                    SyncHierarchyFromSpline();
                }
                DoUpdate();
            }
        }
#endif


        private void DoUpdate()
        {
#if UNITY_EDITOR
            if (transformChildrenChanged)
            {
                transformChildrenChanged = false;
                if (ControlPoints.Count != GetComponentsInChildren<CurvySplineSegment>().Length)
                {
                    //The SyncSplineFromHierarchy is meant only to handle the case where the user adds or removes a contol point from the editor hierarchy. The addition or removal of a control point through Curvy's API is handled efficiently elsewhere. I said efficiently because, contrary to SyncSplineFromHierarchy, it does not lead to rebuilding the whole spline.
                    //There is in fact another case where the following code would be useful, which is removing a CP's gameobject through Unity's API. In this case, even if UNITY_EDITOR == false, the syncing would be necessary. But, to not impact the performances of the common user case (using Curvy API to modify CPs), I decided to not handle this case. Removing cps, or adding them, via Unity API is not supported.
                    SyncSplineFromHierarchy();
                }
            }
#endif

            int controlPointCount = ControlPointCount;
            for (int index = 0; index < controlPointCount; index++)
            {
                CurvySplineSegment controlPoint = ControlPoints[index];
                if (controlPoint.AutoBakeOrientation && controlPoint.ApproximationUp.Length > 0)
                    controlPoint.BakeOrientationToTransform();
            }

            if (isCpsRelationshipCacheValid == false)
                RebuildControlPointsRelationshipCache(true);

            globalCoordinatesChangedThisFrame = false;
            if (cachedTransform.hasChanged)
            {
                cachedTransform.hasChanged = false;

                //This additional test is done since transform.hasChanged is true even when changing parent with no change in both local and global coordinates. And even a change in local coordinates doesn't interest us, since bounds computation only need global coordinates
                if (cachedTransform.position.NotApproximately(lastProcessedPosition) || cachedTransform.rotation.DifferentOrientation(lastProcessedRotation) || cachedTransform.lossyScale != lastProcessedScale)
                {
                    globalCoordinatesChangedThisFrame = true;
                    UpdatedLastProcessedGlobalCoordinates();

                    mBounds = null;
                    //OPTIM Right now, transform change lead to recomputing the bounds in world space. This can be avoided by computing the bounds in local space only when the spline is modified, and transform that to the world space here, where a spline transform has changed.
                    for (int i = 0; i < controlPointCount; i++)
                        ControlPoints[i].ClearBoundsINTERNAL();
                }

            }

            if ((CheckTransform || !Application.isPlaying) && (allControlPointsAreDirty == false))
                for (int i = 0; i < controlPointCount; i++)
                {
                    CurvySplineSegment currentControlPoint = ControlPoints[i];
                    bool dirtyCurve = currentControlPoint.HasUnprocessedLocalPosition;
                    if (dirtyCurve || currentControlPoint.HasUnprocessedLocalOrientation && currentControlPoint.OrientatinInfluencesSpline)
                        currentControlPoint.Spline.SetDirty(currentControlPoint, dirtyCurve == false ? SplineDirtyingType.OrientationOnly : SplineDirtyingType.Everything);
                }

            if (Dirty)
                Refresh();
            else if (sendOnRefreshEventNextUpdate)
                OnRefreshEvent(defaultSplineEventArgs);

            sendOnRefreshEventNextUpdate = false;

            if (globalCoordinatesChangedThisFrame && OnGlobalCoordinatesChanged != null)
                OnGlobalCoordinatesChanged.Invoke(this);
        }

        /// <summary>
        /// are manual start/end CP's allowed?
        /// </summary>
        private bool CanHaveManualEndCp()
        {
            return !Closed && (Interpolation == CurvyInterpolation.CatmullRom || Interpolation == CurvyInterpolation.TCB);
        }

        private bool CanBeClamped()
        {
            return !Closed && (Interpolation == CurvyInterpolation.BSpline);
        }

#if UNITY_EDITOR


        /// <summary>
        /// Get the correct control point name that should be displayed in the hierarchy
        /// </summary>
        /// <param name="controlPointIndex"></param>
        static private string GetControlPointName(short controlPointIndex)
        {
            if (controlPointIndex < CachedControlPointsNameCount)
                return controlPointNames[controlPointIndex];

            return MakeControlPointName(controlPointIndex);
        }

        private static string[] GetControlPointNames()
        {
            string[] names = new string[CachedControlPointsNameCount];
            for (short i = 0; i < CachedControlPointsNameCount; i++)
                names[i] = MakeControlPointName(i);
            return names;
        }

        static private string MakeControlPointName(short controlPointIndex)
        {
            return "CP" + controlPointIndex.ToString("D4", System.Globalization.CultureInfo.InvariantCulture);
        }
#endif

        /// <summary>
        /// Marks a Control Point to get recalculated on next call to Refresh(). Will also mark connected control points if dirtyConnection is set to true.  Will also mark control points that depend on the current one through the Follow-Up feature.
        /// </summary>
        /// <param name="controlPoint">the Control Point to refresh</param>
        /// <param name="dirtyingType">Defines what aspect should be dirtied</param>
        /// <param name="previousControlPoint"></param>
        /// <param name="nextControlPoint"></param>
        /// <param name="ignoreConnectionOfInputControlPoint">If true, this method will not mark as dirty the control points connected to the "controlPoint" parameter</param>
        private void SetDirty(CurvySplineSegment controlPoint, SplineDirtyingType dirtyingType, CurvySplineSegment previousControlPoint, CurvySplineSegment nextControlPoint, bool ignoreConnectionOfInputControlPoint)
        {
            if (ReferenceEquals(this, controlPoint.Spline) == false)
                throw new ArgumentException(String.Format(InvalidCPErrorMessage, controlPoint, name));

#if CURVY_LOG_DIRTYING
            Debug.Log("Set Dirty CP " + dirtyingType);
#endif
            if (ignoreConnectionOfInputControlPoint == false && controlPoint.Connection)
            {
                //Setting all connected CPs is a bit overkill, but at least, you are sure to avoid the multitude of Connections related bugs, plus simplifies the code a lot. You might try to OPTIM by dirtying only the relevant connected CPs, and only in the relevant scenarios, but (seeing the old code that I removed) it is a very dangerous optimization, and you can surely optimize other stuff that will take less time to optimize, and can generate less bugs
                ReadOnlyCollection<CurvySplineSegment> connectionControlPoints = controlPoint.Connection.ControlPointsList;
                for (int index = 0; index < connectionControlPoints.Count; index++)
                {
                    CurvySplineSegment connectedControlPoint = connectionControlPoints[index];
                    CurvySpline connectedSpline = connectedControlPoint.Spline;
                    if (connectedSpline)
                    {
                        connectedSpline.dirtyControlPointsMinimalSet.Add(connectedControlPoint);
                        connectedSpline.SetDirtyingFlags(dirtyingType);
                    }
                }
#if CURVY_SANITY_CHECKS
                if (connectionControlPoints.Contains(controlPoint) == false)
                    DTLog.LogError("[Curvy] SetDirty couldn't find the dirtying control point in the connection.", this);
#endif
            }
            else
            {
                dirtyControlPointsMinimalSet.Add(controlPoint);
                SetDirtyingFlags(dirtyingType);
            }

            //Dirty CPs that could depend on the current CP through the Follow-Up feature
            {
                if (previousControlPoint && previousControlPoint.Connection)
                {
                    ReadOnlyCollection<CurvySplineSegment> connectionControlPoints = previousControlPoint.Connection.ControlPointsList;
                    for (int index = 0; index < connectionControlPoints.Count; index++)
                    {
                        CurvySplineSegment connectedControlPoint = connectionControlPoints[index];
                        CurvySpline connectedSpline = connectedControlPoint.Spline;
                        if (connectedSpline && connectedControlPoint.FollowUp == previousControlPoint)
                        {
                            connectedSpline.dirtyControlPointsMinimalSet.Add(connectedControlPoint);
                            connectedSpline.SetDirtyingFlags(dirtyingType);
                        }
                    }
                }

                if (nextControlPoint && nextControlPoint.Connection)
                {
                    ReadOnlyCollection<CurvySplineSegment> connectionControlPoints = nextControlPoint.Connection.ControlPointsList;
                    for (int index = 0; index < connectionControlPoints.Count; index++)
                    {
                        CurvySplineSegment connectedControlPoint = connectionControlPoints[index];
                        CurvySpline connectedSpline = connectedControlPoint.Spline;
                        if (connectedSpline && connectedControlPoint.FollowUp == nextControlPoint)
                        {
                            connectedSpline.dirtyControlPointsMinimalSet.Add(connectedControlPoint);
                            connectedSpline.SetDirtyingFlags(dirtyingType);
                        }
                    }
                }
            }
        }

        private void SetDirtyingFlags(SplineDirtyingType dirtyingType)
        {
            mDirtyCurve = mDirtyCurve || dirtyingType == SplineDirtyingType.Everything;
            mDirtyOrientation = true;

            if (mDirtyCurve)
            {
                mCacheSize = -1;
                length = -1;
                mBounds = null;
            }
        }

        private void ReverseControlPoints()
        {
            ControlPoints.Reverse();
            InvalidateControlPointsRelationshipCacheINTERNAL();
            SetDirtyAll(SplineDirtyingType.Everything, true);
        }

        static private short GetNextControlPointIndex(short controlPointIndex, bool isSplineClosed, int controlPointsCount)
        {
            if (isSplineClosed && controlPointsCount <= 1)
                return -1;
            if (controlPointIndex + 1 < controlPointsCount)
                return (short)(controlPointIndex + 1);
            return (short)(isSplineClosed ? 0 : -1);
        }

        static private short GetPreviousControlPointIndex(short controlPointIndex, bool isSplineClosed, int controlPointsCount)
        {
            if (isSplineClosed && controlPointsCount <= 1)
                return -1;
            if (controlPointIndex - 1 >= 0)
                return (short)(controlPointIndex - 1);
            return (short)(isSplineClosed ? controlPointsCount - 1 : -1);
        }

        //OPTIM should you use this instead of the isSegment poperties in ControlPointExtrinsicProperties?
        private static bool IsControlPointASegment(int controlPointIndex, int controlPointCount, bool isClosed, bool notAutoEndTangentsAndIsCatmullRomOrTCB, bool isBSpline, int bSplineDegree)
        {
#if CONTRACTS_FULL
            Contract.Requires(controlPointIndex >= 0 && controlPointIndex < ControlPointCount);
            Contract.Requires(bSplineDegree >= 0);
#endif

            return (isBSpline == false || bSplineDegree < controlPointCount)
                   &&
                   (isClosed && controlPointCount > 1
                     || (notAutoEndTangentsAndIsCatmullRomOrTCB
                         ? controlPointIndex > 0 && controlPointIndex < controlPointCount - 2
                         : controlPointIndex < controlPointCount - 1));
        }

        #region Modifying control points list
        private void AddControlPoint(CurvySplineSegment item)
        {
            ControlPoints.Add(item);
            item.LinkToSpline(this);
            InvalidateControlPointsRelationshipCacheINTERNAL();
            short previousControlPointIndex = GetPreviousControlPointIndex((short)(ControlPoints.Count - 1), Closed, ControlPoints.Count);
            short nextControlPointIndex = GetNextControlPointIndex((short)(ControlPoints.Count - 1), Closed, ControlPoints.Count);
            SetDirty(item, SplineDirtyingType.Everything,
                previousControlPointIndex != -1 ? ControlPoints[previousControlPointIndex] : null,
                nextControlPointIndex != -1 ? ControlPoints[nextControlPointIndex] : null, false);
        }

        /// <summary>
        /// Adds a control point at a specific index
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        private void InsertControlPoint(int index, CurvySplineSegment item)
        {
            ControlPoints.Insert(index, item);
            item.LinkToSpline(this);
            InvalidateControlPointsRelationshipCacheINTERNAL();
            //Dirtying
            {
                short previousControlPointIndex = GetPreviousControlPointIndex((short)index, Closed, ControlPoints.Count);
                short nextControlPointIndex = GetNextControlPointIndex((short)index, Closed, ControlPoints.Count);
                SetDirty(item, SplineDirtyingType.Everything,
                    previousControlPointIndex == -1 ? null : ControlPoints[previousControlPointIndex],
                    nextControlPointIndex == -1 ? null : ControlPoints[nextControlPointIndex], false);
            }
        }

        private void RemoveControlPoint(CurvySplineSegment item)
        {
            int indexOftItem = GetControlPointIndex(item);
            //Dirtying
            if (ControlPoints.Count == 1)//Removing the last CP
                SetDirtyAll(SplineDirtyingType.Everything, true);
            else
            {
                short previousControlPointIndex = GetPreviousControlPointIndex((short)indexOftItem, Closed, ControlPoints.Count);
                short nextControlPointIndex = GetNextControlPointIndex((short)indexOftItem, Closed, ControlPoints.Count);
                if (previousControlPointIndex != -1)
                    SetDirty(ControlPoints[previousControlPointIndex], SplineDirtyingType.Everything);
                if (nextControlPointIndex != -1)
                    SetDirty(ControlPoints[nextControlPointIndex], SplineDirtyingType.Everything);
            }
            ControlPoints.RemoveAt(indexOftItem);
            dirtyControlPointsMinimalSet.Remove(item);
            if (item.Spline == this)
                item.UnlinkFromSpline();
            InvalidateControlPointsRelationshipCacheINTERNAL();
        }

        private void ClearControlPoints()
        {
            SetDirtyAll(SplineDirtyingType.Everything, true);
            for (int index = 0; index < ControlPoints.Count; index++)
            {
                CurvySplineSegment controlPoint = ControlPoints[index];
                if (controlPoint && //controlPoint can be null if you create a spline via the pen tool, and then undo it
                    controlPoint.Spline == this) //This if is to avoid the case where the code, executed because of a change in the number of children, will unlink a CP that has been moved to another spline through the hierarchy editor.
                    controlPoint.UnlinkFromSpline();
            }
            ControlPoints.Clear();
            dirtyControlPointsMinimalSet.Clear();
            InvalidateControlPointsRelationshipCacheINTERNAL();
        }
        #endregion

        #region ControlPoints relastionship cache

        internal void InvalidateControlPointsRelationshipCacheINTERNAL()
        {
            if (isCpsRelationshipCacheValid)
            {
                lock (controlPointsRelationshipCacheLock)
                {
                    isCpsRelationshipCacheValid = false;
                    firstSegment = lastSegment = firstVisibleControlPoint = lastVisibleControlPoint = null;
                }
            }
        }

        /// <summary>
        /// Is not thread safe
        /// </summary>
        /// <param name="fixNonCoherentControlPoints">If true, control points with properties that are no more coherent with their position in the spline will get modified</param>
        /// <remarks>Is not thread safe</remarks>
        private void RebuildControlPointsRelationshipCache(bool fixNonCoherentControlPoints)
        {
            lock (controlPointsRelationshipCacheLock)
            {
                if (isCpsRelationshipCacheValid)
                    return;

                //force clamping of B-spline's degree, in case control points got removed
                BSplineDegree = bSplineDegree;

                //TODO Try to do elsewhere the work done here when fixNonCoherentControlPoints, so it is always true, and not only true when Relationship cache is build
                int controlPointsCount = ControlPoints.Count;
                mSegments.Clear();
                mSegments.Capacity = controlPointsCount;
                if (controlPointsCount > 0)
                {
                    CurvySplineSegment firsAssignedSegment = null;
                    bool firstSegmentFound = false;
                    CurvySplineSegment lastAssignedSegment = null;

                    CurvySplineSegment.ControlPointExtrinsicProperties previousCpInfo = new CurvySplineSegment.ControlPointExtrinsicProperties(false, -1, -1, -1, -1, -1, false, false, false, -1);

                    bool isSplineClosed = Closed;
                    bool isCatmullRomOrTcb = (Interpolation == CurvyInterpolation.CatmullRom || Interpolation == CurvyInterpolation.TCB);
                    bool notAutoEndTangentsAndIsCatmullRomOrTcb = AutoEndTangents == false && isCatmullRomOrTcb;
                    bool isBSpline = Interpolation == CurvyInterpolation.BSpline;


                    float tfInverseDenominator;
                    {
                        if (notAutoEndTangentsAndIsCatmullRomOrTcb)
                            tfInverseDenominator = 1f / (controlPointsCount > 3 ? controlPointsCount - 3 : 1);
                        else if (isSplineClosed)
                            tfInverseDenominator = 1f / controlPointsCount;
                        else
                            tfInverseDenominator = 1f / (controlPointsCount > 1 ? controlPointsCount - 1 : 1);
                    }

                    short segmentIndex = 0;
                    short lastProcessedOrientationAnchorIndex = -1;
                    for (short index = 0; index < controlPointsCount; index++)
                    {
                        CurvySplineSegment controlPoint = ControlPoints[index];

                        short previousControlPointIndex = GetPreviousControlPointIndex(index, isSplineClosed, controlPointsCount);
                        short nextControlPointIndex = GetNextControlPointIndex(index, isSplineClosed, controlPointsCount);

                        bool isSegment = IsControlPointASegment(index, controlPointsCount, isSplineClosed, notAutoEndTangentsAndIsCatmullRomOrTcb, isBSpline, bSplineDegree);
                        bool isVisible = isSegment || previousCpInfo.IsSegment;

                        if (isVisible
                            && (lastProcessedOrientationAnchorIndex == -1//is first segment
                                || controlPoint.SerializedOrientationAnchor// is anchor
                                || !isSegment))// is last visible CP
                            lastProcessedOrientationAnchorIndex = index;

                        bool canHaveFollowUp = isVisible && (nextControlPointIndex == -1 || previousControlPointIndex == -1);

                        float tf;
                        {
                            if (notAutoEndTangentsAndIsCatmullRomOrTcb)
                                tf = tfInverseDenominator
                                     * (index == 0
                                        ? 0
                                        : index == controlPointsCount - 1
                                            ? Math.Max(0, index - 2)
                                            : index - 1);
                            else
                                tf = tfInverseDenominator * index;
                        }

                        previousCpInfo = new CurvySplineSegment.ControlPointExtrinsicProperties(
                            isVisible,
                            tf,
                            isSegment ? segmentIndex : (short)-1,
                            index,
                            previousControlPointIndex,
                            nextControlPointIndex,
                            previousControlPointIndex != -1
                                && IsControlPointASegment(previousControlPointIndex, controlPointsCount, isSplineClosed, notAutoEndTangentsAndIsCatmullRomOrTcb, isBSpline, bSplineDegree),
                            nextControlPointIndex != -1
                                && IsControlPointASegment(nextControlPointIndex, controlPointsCount, isSplineClosed, notAutoEndTangentsAndIsCatmullRomOrTcb, isBSpline, bSplineDegree),
                            canHaveFollowUp,
                            isVisible ? lastProcessedOrientationAnchorIndex : (short)-1);
                        controlPoint.SetExtrinsicPropertiesINTERNAL(previousCpInfo);

                        if (isSegment)
                        {
                            mSegments.Add(controlPoint);
                            segmentIndex++;
                            if (firstSegmentFound == false)
                            {
                                firstSegmentFound = true;
                                firsAssignedSegment = controlPoint;
                            }
                            lastAssignedSegment = controlPoint;
                        }

                        if (fixNonCoherentControlPoints && canHaveFollowUp == false)
                            controlPoint.UnsetFollowUpWithoutDirtyingINTERNAL();
                    }

                    firstSegment = firsAssignedSegment;
                    lastSegment = lastAssignedSegment;
                    firstVisibleControlPoint = firstSegment;
                    lastVisibleControlPoint = ReferenceEquals(lastSegment, null) == false
                        ? ControlPoints[lastSegment.GetExtrinsicPropertiesINTERNAL().NextControlPointIndex]
                        : null;
                }
                else
                {
                    firstSegment = lastSegment = firstVisibleControlPoint = lastVisibleControlPoint = null;
                }

                isCpsRelationshipCacheValid = true;

#if UNITY_EDITOR
                if (fixNonCoherentControlPoints)
                    syncHierarchyFromSplineNeeded = true;
#endif
            }
        }

        #endregion

        private bool ProcessDirtyControlPoints()
        {
            if (isCpsRelationshipCacheValid == false)
                RebuildControlPointsRelationshipCache(true);

            FillDirtyCpsExtendedList();

            bool thereAreDirtyControlPoints = allControlPointsAreDirty || dirtyCpsExtendedList.Count > 0;

            dirtyControlPointsMinimalSet.Clear();
            allControlPointsAreDirty = false;

            //OPTIM: the current implementation will refresh all dirty CP's orientations, even if one of them needed it, and the others needed only position related refresh. This is because the mDirtyCurve and mDirtyOrientation are spline wide, and not per CP. This can be improved
            //OPTIM: make all the per CP work threadable, and multi thread everything
            if (thereAreDirtyControlPoints)
            {
                if (!(mDirtyOrientation || mDirtyCurve))
                    Debug.LogError("Invalid dirtying flags");

                PrepareThreadCompatibleData();

                int controlPointsCount = ControlPointCount;

                if (mDirtyCurve)
                {
                    #region --- Curve ---

                    // Update Bezier Handles
                    if (Interpolation == CurvyInterpolation.Bezier)
                    {
                        for (int i = 0; i < dirtyCpsExtendedList.Count; i++)
                        {
                            CurvySplineSegment dirtyControlPoint = dirtyCpsExtendedList[i];
                            if (dirtyControlPoint.AutoHandles)
                                dirtyControlPoint.SetBezierHandles(-1f, true, true, true);
                        }
                    }

                    // Iterate through all changed for threadable tasks (cache Approximation, ApproximationT, ApproximationDistance)
                    if (UseThreading)
                        mThreadWorker.ParallelFor(refreshCurveAction, dirtyCpsExtendedList);
                    else
                        for (int i = 0; i < dirtyCpsExtendedList.Count; i++)
                            dirtyCpsExtendedList[i].refreshCurveINTERNAL();

                    // Iterate through all ControlPoints for some basic actions
                    if (controlPointsCount > 0)
                    {

                        List<CurvySplineSegment> segments = Segments;
                        int segmentsCount = segments.Count;

                        Array.Resize(ref controlPointsDistances, controlPointsCount);

                        //// Distances
                        controlPointsDistances[0] = ControlPoints[0].Distance = 0;
                        for (int i = 1; i < controlPointsCount; i++)
                            controlPointsDistances[i] = ControlPoints[i].Distance = ControlPoints[i - 1].Distance + ControlPoints[i - 1].Length;

                        for (int index = 0; index < segmentsCount; index++)
                        {
                            CurvySplineSegment segment = segments[index];
                            CurvySplineSegment nextSegment = GetNextSegment(segment);
                            if (nextSegment)
                                //enforce tangents continuity
                                segment.ApproximationT[segment.CacheSize] = nextSegment.ApproximationT[0];
                            else
                            {
                                //handles tangent of last visible control point
                                GetNextControlPoint(segment).ApproximationT[0] = segment.ApproximationT[segment.CacheSize];
                            }
                        }
                    }

                    #endregion
                }

                if (mDirtyOrientation && Count > 0)
                {
                    #region --- Orientation ---

                    switch (Orientation)
                    {
                        case CurvyOrientation.None:

                            #region --- None ---

                            //No threading here since the operation is too quick to have any benefice in multithreading it
                            for (int i = 0; i < dirtyCpsExtendedList.Count; i++)
                                dirtyCpsExtendedList[i].refreshOrientationNoneINTERNAL();
                            break;

                        #endregion

                        case CurvyOrientation.Static:

                            #region --- Static ---

                            if (UseThreading)
                            {
                                Action<CurvySplineSegment, int, int> action = (controlPoint, cpIndex, cpsCount) => controlPoint.refreshOrientationStaticINTERNAL();
                                mThreadWorker.ParallelFor(action, dirtyCpsExtendedList);
                            }
                            else
                            {
                                for (int i = 0; i < dirtyCpsExtendedList.Count; i++)
                                    dirtyCpsExtendedList[i].refreshOrientationStaticINTERNAL();
                            }

                            break;

                        #endregion

                        case CurvyOrientation.Dynamic:

                            #region --- Dynamic ---

                            // process PTF and smoothing for all anchor groups of dirty CPs
                            int dead = controlPointsCount + 1;
                            do
                            {
                                CurvySplineSegment currentDirtyControlPoint = dirtyCpsExtendedList[0];
                                if (IsControlPointASegment(currentDirtyControlPoint) == false)
                                {
                                    currentDirtyControlPoint.refreshOrientationDynamicINTERNAL(currentDirtyControlPoint.getOrthoUp0INTERNAL());
                                    dirtyCpsExtendedList.RemoveAt(0);
                                }
                                else
                                {
                                    short currentOrientationAnchorIndex = GetControlPointOrientationAnchorIndex(currentDirtyControlPoint);
                                    CurvySplineSegment currentOrientationAnchor = ControlPoints[currentOrientationAnchorIndex];

                                    float swirlPerSegment;
                                    float smoothingAngleStep;
                                    int sampleCount = 0;
                                    short firstCpOutsideAnchorGroupIndex;
                                    {
                                        short anchorGroupCurrentCpIndex = currentOrientationAnchorIndex;
                                        CurvySplineSegment anchorGroupCurrentCp = currentOrientationAnchor;
                                        int anchorgroupSegmentCount = 0;
                                        float anchorgroupLength = 0;
                                        Vector3 nextControlPointInitialUp = currentOrientationAnchor.getOrthoUp0INTERNAL();
                                        do
                                        {
                                            sampleCount += anchorGroupCurrentCp.CacheSize;
                                            anchorgroupSegmentCount++;
                                            anchorgroupLength += anchorGroupCurrentCp.Length;

                                            anchorGroupCurrentCp.refreshOrientationDynamicINTERNAL(nextControlPointInitialUp);
                                            nextControlPointInitialUp = anchorGroupCurrentCp.ApproximationUp[anchorGroupCurrentCp.ApproximationUp.Length - 1];

                                            anchorGroupCurrentCpIndex = GetNextControlPointIndex(anchorGroupCurrentCpIndex, m_Closed, controlPointsCount);
                                            anchorGroupCurrentCp = ControlPoints[anchorGroupCurrentCpIndex];
                                        } while (!IsControlPointAnOrientationAnchor(anchorGroupCurrentCp));
                                        firstCpOutsideAnchorGroupIndex = anchorGroupCurrentCpIndex;
                                        smoothingAngleStep = nextControlPointInitialUp.AngleSigned(anchorGroupCurrentCp.getOrthoUp0INTERNAL(), anchorGroupCurrentCp.ApproximationT[0]) / sampleCount;

                                        // Apply swirl
                                        {
                                            switch (currentOrientationAnchor.Swirl)
                                            {
                                                case CurvyOrientationSwirl.Segment:
                                                    swirlPerSegment = currentOrientationAnchor.SwirlTurns * 360;
                                                    break;
                                                case CurvyOrientationSwirl.AnchorGroup:
                                                    swirlPerSegment = (currentOrientationAnchor.SwirlTurns * 360 / anchorgroupSegmentCount);
                                                    break;
                                                case CurvyOrientationSwirl.AnchorGroupAbs:
                                                    swirlPerSegment = (currentOrientationAnchor.SwirlTurns * 360) / anchorgroupLength;
                                                    break;
                                                case CurvyOrientationSwirl.None:
                                                    swirlPerSegment = 0;
                                                    break;
                                                default:
                                                    swirlPerSegment = 0;
                                                    DTLog.LogError("[Curvy] Invalid Swirl value " + currentOrientationAnchor.Swirl, this);
                                                    break;
                                            }
                                        }
                                    }


                                    {
                                        float angleAccumulator = smoothingAngleStep;
                                        short anchorGroupCurrentCpIndex = currentOrientationAnchorIndex;
                                        bool isSwirlAnchorGroupAbs = currentOrientationAnchor.Swirl == CurvyOrientationSwirl.AnchorGroupAbs;
                                        Vector3 nextControlPointInitialUp = currentOrientationAnchor.ApproximationUp[0];
                                        do
                                        {
                                            CurvySplineSegment anchorGroupCurrentCp = ControlPoints[anchorGroupCurrentCpIndex];
                                            float swirlAngleStep = isSwirlAnchorGroupAbs
                                                ? smoothingAngleStep + swirlPerSegment * anchorGroupCurrentCp.Length / anchorGroupCurrentCp.CacheSize
                                                : smoothingAngleStep + swirlPerSegment / anchorGroupCurrentCp.CacheSize;

                                            //rotate Ups around tangents
                                            Vector3[] tangents = anchorGroupCurrentCp.ApproximationT;
                                            Vector3[] ups = anchorGroupCurrentCp.ApproximationUp;
                                            int upsLength = ups.Length;
                                            ups[0] = nextControlPointInitialUp;

                                            //OPTIM: I thought that the commented version of the code will be faster because of SIMD, but it doesn't seem so. Maybe further work on parallelisation will indded make it faster, but a quick test with Parallel.For didn't show any increase in perfs, but the opposite. Maybe parallelising the ups on a single CP is too little work for multi-threading. Maybe if all the cps' ups were computed in the same loop, multithreading will be worth it.
                                            /*
                                            float angleAccumulatorLoopStart = angleAccumulator;
                                            for (int i = 1; i < upsLength; i++)
                                                ups[i] = Quaternion.AngleAxis(angleAccumulatorLoopStart + (i - 1) * swirlAngleStep, tangents[i]) * ups[i];
                                            angleAccumulator = angleAccumulatorLoopStart + (upsLength - 1) * swirlAngleStep;
                                            */
                                            for (int i = 1; i < upsLength; i++)
                                            {
                                                ups[i] = Quaternion.AngleAxis(angleAccumulator, tangents[i]) * ups[i];
                                                angleAccumulator += swirlAngleStep;
                                            }
                                            nextControlPointInitialUp = ups[upsLength - 1];
                                            dirtyCpsExtendedList.Remove(anchorGroupCurrentCp);

                                            anchorGroupCurrentCpIndex = GetNextControlPointIndex(anchorGroupCurrentCpIndex, m_Closed, controlPointsCount);
                                        } while (anchorGroupCurrentCpIndex != firstCpOutsideAnchorGroupIndex);
                                    }
                                }
                            } while (dirtyCpsExtendedList.Count > 0 && dead-- > 0);
                            if (dead <= 0)
                                DTLog.LogWarning("[Curvy] Deadloop in CurvySpline.Refresh! Please raise a bugreport!", this);
                            break;
                        default:
                            DTLog.LogError("[Curvy] Invalid Orientation value " + Orientation, this);
                            break;
                            #endregion
                    }

                    // Handle very last CP
                    if (!Closed)
                    {
                        CurvySplineSegment beforLastVisibleCp = GetPreviousControlPoint(LastVisibleControlPoint);
                        LastVisibleControlPoint.ApproximationUp[0] = beforLastVisibleCp.ApproximationUp[beforLastVisibleCp.CacheSize];
                    }

                    #endregion
                }
            }

#if CURVY_SANITY_CHECKS
            //These asserts are to make sure that the Refresh code doesn't modify the dirtiness state, which was the case before and could create bugs or unecessary calculations
            Assert.IsTrue(dirtyControlPointsMinimalSet.Count == 0);
            Assert.IsTrue(allControlPointsAreDirty == false);
#endif
            mDirtyCurve = false;
            mDirtyOrientation = false;

            return thereAreDirtyControlPoints;
        }

        /// <summary>
        /// Set the correct values to the thread compatible local positions and rotation
        /// When multithreading, you can't access Transform in the not main threads. Here we cache that data so it is available for threads
        /// </summary>
        private void PrepareThreadCompatibleData()
        {
#if CURVY_SANITY_CHECKS
            Assert.IsTrue(isCpsRelationshipCacheValid);
#endif
            int controlPointsCount = ControlPointCount;
            bool useFollowUp = Interpolation == CurvyInterpolation.CatmullRom || Interpolation == CurvyInterpolation.TCB;

            //prepare the TTransform for all needed control points, which are ....
            // OPTIM: preparing the TTransform of all those CPs is overkill. Restrict the following the prepared CPs to only the CPs being related to the dirtied CPs
            //... all the spline's control points, and ...
            for (int i = 0; i < controlPointsCount; i++)
            {
                CurvySplineSegment controlPoint = ControlPoints[i];
                controlPoint.PrepareThreadCompatibleDataINTERNAL(useFollowUp);
            }
            //... possible other splines' control points because of the followup feature ...
            if (Count > 0)
            {
                CurvySplineSegment beforeFirst = GetPreviousControlPointUsingFollowUp(FirstVisibleControlPoint);
                //before first can be contorlPoints[0] in the case of a spline with AutoEndTangent set to false
                if (ReferenceEquals(beforeFirst, null) == false && beforeFirst.Spline != this)
                    beforeFirst.PrepareThreadCompatibleDataINTERNAL(useFollowUp);
                CurvySplineSegment afterLast = GetNextControlPointUsingFollowUp(LastVisibleControlPoint);
                //afterLast first can be contorlPoints[controlPoints.Count - 1] in the case of a spline with AutoEndTangent set to false
                if (ReferenceEquals(afterLast, null) == false && afterLast.Spline != this)
                    afterLast.PrepareThreadCompatibleDataINTERNAL(useFollowUp);
            }
        }

        /// <summary>
        /// Fills dirtyCpsExtendedList from dirtyControlPointsMinimalSet
        /// </summary>
        private void FillDirtyCpsExtendedList()
        {
            int controlPointsCount = ControlPoints.Count;
            dirtyCpsExtendedList.Clear();
            if (allControlPointsAreDirty)
                for (int i = 0; i < controlPointsCount; i++)
                    dirtyCpsExtendedList.Add(ControlPoints[i]);
            else
            {
                //OPTIM use cps indexes in dirtyControlPointsMinimalSet instead of cps references, will reduce the time passed in getHash and ==

                int minimalDirtyCpsCount = dirtyControlPointsMinimalSet.Count;
                //We expend dirtyControlPointsMinimalSet to include the extended list of dirty control points
                for (int index = 0; index < minimalDirtyCpsCount; index++)
                {
                    //OPTIM ElementAt allocates enumerator, avoid this. Maybe a way to avoid it is to use dirtyControlPointsMinimalSet.Copyto(array) to copy the content of the hashset into an array, and then iterate on that array instead of the hashset. The array needs to be a member of the CurvySpline class, so it is allocated only one
                    CurvySplineSegment dirtyCp = dirtyControlPointsMinimalSet.ElementAt(index);

                    {
                        switch (Interpolation)
                        {
                            case CurvyInterpolation.Linear:
                                {
                                    CurvySplineSegment previousCp = GetPreviousControlPoint(dirtyCp);
                                    if (previousCp)
                                        dirtyControlPointsMinimalSet.Add(previousCp);
                                }
                                break;
                            case CurvyInterpolation.CatmullRom:
                            case CurvyInterpolation.TCB:
                            case CurvyInterpolation.Bezier:
                                {
                                    CurvySplineSegment previousCp = GetPreviousControlPoint(dirtyCp);
                                    if (previousCp)
                                        dirtyControlPointsMinimalSet.Add(previousCp);

                                    //Add other segments to reflect the effect of Bezier handles (and Auto Handles) and Catmull-Rom and TCB's extended influence of CPs.
                                    //OPTIM in the bezier case, always including this extended set of CPs is overkill, but at least it avoids bugs and the complicated dirtying logic associated with the Bezier handles handling code.
                                    if (previousCp)
                                    {
                                        //OPTIM you can get dirtyCp's index, then use GetPreviousControlPointIndex to get previousCp and previousPreviousCp
                                        CurvySplineSegment previousPreviousCp = GetPreviousControlPoint(previousCp);
                                        if (previousPreviousCp)
                                            dirtyControlPointsMinimalSet.Add(previousPreviousCp);
                                    }

                                    CurvySplineSegment nextCp = GetNextControlPoint(dirtyCp);
                                    if (nextCp)
                                        dirtyControlPointsMinimalSet.Add(nextCp);
                                }
                                break;
                            case CurvyInterpolation.BSpline:
                                {
                                    int degree = BSplineDegree;
                                    bool closed = Closed;
                                    bool isClamped = IsBSplineClamped;
                                    int n = BSplineHelper.GetBSplineN(controlPointsCount, degree, closed);
                                    int dirtyCpIndex = GetControlPointIndex(dirtyCp);
                                    for (int testedCpIndex = 0; testedCpIndex < controlPointsCount; testedCpIndex++)
                                    {
                                        CurvySplineSegment testedCP = ControlPoints[testedCpIndex];

                                        int startK;
                                        int endK;

                                        BSplineHelper.GetBSplineUAndK(
                                            SegmentToTF(testedCP), isClamped, degree, n,
                                            out _, out startK);

                                        if (dirtyCpIndex >= startK - degree && dirtyCpIndex <= startK)
                                            dirtyCpsExtendedList.Add(testedCP);
                                        else
                                        {
                                            BSplineHelper.GetBSplineUAndK(
                                                SegmentToTF(testedCP, 1), isClamped, degree, n,
                                                out _, out endK);
                                            if (dirtyCpIndex >= endK - degree && dirtyCpIndex <= endK)
                                                dirtyCpsExtendedList.Add(testedCP);
                                            else if (closed)
                                            {
                                                int loopedCpIndex = dirtyCpIndex + controlPointsCount;
                                                if (loopedCpIndex >= startK - degree && loopedCpIndex <= startK)
                                                    dirtyCpsExtendedList.Add(testedCP);
                                                else
                                                {
                                                    BSplineHelper.GetBSplineUAndK(
                                                        SegmentToTF(testedCP, 1), isClamped, degree, n,
                                                        out _, out endK);
                                                    if (loopedCpIndex >= endK - degree && loopedCpIndex <= endK)
                                                        dirtyCpsExtendedList.Add(testedCP);
                                                }
                                            }
                                        }
                                    }
                                }
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }

#if CURVY_SANITY_CHECKS
                Assert.IsTrue(isCpsRelationshipCacheValid);
#endif
                dirtyCpsExtendedList.AddRange(dirtyControlPointsMinimalSet);
            }
        }

        /// <summary>
        /// Call this to make the spline send an event to notify its listeners of the change in the spline data.
        /// </summary>
        internal void NotifyMetaDataModification()
        {
            //DESIGN until 2.2.3, meta data change triggered OnRefresh event by dirtying its associated control point. I think spline should have different events (or at least a param in the event) to distinguish between the event coming from an actual change in the spline's geometry, and a change in its meta data.
            sendOnRefreshEventNextUpdate = true;
        }

        /// <summary>
        /// Rebuilds the hierarchy from the ControlPoints list
        /// </summary>
        private void SyncHierarchyFromSpline(bool renameControlPoints = true)
        {
#if UNITY_EDITOR
            // rename them and set their order based on ControlPoint list
            int count = ControlPoints.Count;
            for (short i = 0; i < count; i++)
            {
                CurvySplineSegment curvySplineSegment = ControlPoints[i];
                if (curvySplineSegment)
                //curvySplineSegment was null in the following case:
                //In edit mode, using the pen tool, added a new spline with a cp (CTRL + Left click on empty spot), then added a connected spline (CTRL + Right click on empty spot), and then hit ctrl+Z, which undone the creation of the connected spline, and in the next update this code is called with ControlPoints containing destroyed CPs 
                {
                    curvySplineSegment.transform.SetSiblingIndex(i);

                    if (renameControlPoints)
                        curvySplineSegment.name = GetControlPointName(i);
                }
            }
#endif
        }

        private void UpdatedLastProcessedGlobalCoordinates()
        {
            lastProcessedPosition = cachedTransform.position;
            lastProcessedRotation = cachedTransform.rotation;
            lastProcessedScale = cachedTransform.lossyScale;
        }

        /// <summary>
        /// Inserts a Control Point, trigger events and refresh spline
        /// </summary>
        /// <param name="controlPoint">A control point used as a param of the OnBeforeControlPointAddEvent</param>
        /// <param name="position">The position of the control point at its creation</param>
        /// <param name="insertionIndex">Index at which the newly created control point will be inserted in the spline.</param>
        /// <param name="insertionMode">Used as a param of send events</param>
        /// <param name="skipRefreshingAndEvents">If true, the spline's <see cref="Refresh"/> method will not be called, and the relevant events will not be triggered</param>
        /// <param name="space">Whether the positions are in the local or global space</param>
        /// <returns>The created Control Point</returns>
        private CurvySplineSegment InsertAt(CurvySplineSegment controlPoint, Vector3 position, int insertionIndex, CurvyControlPointEventArgs.ModeEnum insertionMode, bool skipRefreshingAndEvents, Space space)
        {
#if CONTRACTS_FULL
            Contract.Requires(controlPoint.Spline == this);
            Contract.Requires(controlPoints.Contains(controlPoint));
#endif

            if (skipRefreshingAndEvents == false)
                OnBeforeControlPointAddEvent(new CurvyControlPointEventArgs(this, this, controlPoint, insertionMode));

            GameObject go;
            CurvySplineSegment insertedControlPoint;

            if (UsePooling && Application.isPlaying)
            {
                CurvyGlobalManager curvyGlobalManager = CurvyGlobalManager.Instance;
                if (curvyGlobalManager != null)
                {
                    insertedControlPoint = curvyGlobalManager.ControlPointPool.Pop<CurvySplineSegment>(cachedTransform);//TODO should this be callse with "null" instead of "transform", to be coherent with the other branches, and knowing that the parent is set to "transform" anyway in subsequent calls?
                    go = insertedControlPoint.gameObject;
                }
                else
                {
                    DTLog.LogError("[Curvy] Couldn't find Curvy Global Manager. Please raise a bug report.", this);
                    go = new GameObject("NewCP", typeof(CurvySplineSegment));
                    insertedControlPoint = go.GetComponent<CurvySplineSegment>();
                }
            }
            else
            {
                go = new GameObject("NewCP", typeof(CurvySplineSegment));
                insertedControlPoint = go.GetComponent<CurvySplineSegment>();
            }

            go.layer = gameObject.layer;
            go.transform.SetParent(cachedTransform);

            InsertControlPoint(insertionIndex, insertedControlPoint);
            insertedControlPoint.AutoHandleDistance = AutoHandleDistance;
            if (space == Space.World)
                insertedControlPoint.transform.position = position;
            else
                insertedControlPoint.transform.localPosition = position;
            insertedControlPoint.transform.localRotation = Quaternion.identity;
            insertedControlPoint.transform.localScale = Vector3.one;

            if (skipRefreshingAndEvents == false)
            {
                Refresh();
                OnAfterControlPointAddEvent(new CurvyControlPointEventArgs(this, this, insertedControlPoint, insertionMode));
                OnAfterControlPointChangesEvent(defaultSplineEventArgs);
            }

            return insertedControlPoint;
        }

        #region Events

        private CurvySplineEventArgs OnRefreshEvent(CurvySplineEventArgs e)
        {
            if (OnRefresh != null)
                OnRefresh.Invoke(e);
            return e;
        }

        private CurvyControlPointEventArgs OnBeforeControlPointAddEvent(CurvyControlPointEventArgs e)
        {
            if (OnBeforeControlPointAdd != null)
                OnBeforeControlPointAdd.Invoke(e);
            return e;
        }

        private CurvyControlPointEventArgs OnAfterControlPointAddEvent(CurvyControlPointEventArgs e)
        {
            if (OnAfterControlPointAdd != null)
                OnAfterControlPointAdd.Invoke(e);
            return e;
        }

        private CurvyControlPointEventArgs OnBeforeControlPointDeleteEvent(CurvyControlPointEventArgs e)
        {
            if (OnBeforeControlPointDelete != null)
                OnBeforeControlPointDelete.Invoke(e);
            return e;
        }

        private CurvySplineEventArgs OnAfterControlPointChangesEvent(CurvySplineEventArgs e)
        {
            if (OnAfterControlPointChanges != null)
                OnAfterControlPointChanges.Invoke(e);
            return e;
        }

        #endregion


        /*! \endcond */

        #endregion
    }
}