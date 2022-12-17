// =====================================================================
// Copyright 2013-2022 ToolBuddy
// All rights reserved
// 
// http://www.toolbuddy.net
// =====================================================================

using System;
using UnityEngine;
using System.Collections;
using FluffyUnderware.Curvy.Utils;
using FluffyUnderware.DevTools;
using FluffyUnderware.DevTools.Extensions;
using System.Reflection;
using JetBrains.Annotations;
#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace FluffyUnderware.Curvy.Controllers
{
    /// <summary>
    /// Controller base class
    /// </summary>
    [ExecuteAlways]
    public abstract class CurvyController : DTVersionedMonoBehaviour, ISerializationCallbackReceiver
    {
        #region ### Enums ###
        /// <summary>
        /// Movement method options
        /// </summary>
        public enum MoveModeEnum
        {
            /// <summary>
            /// Move by Percentage or TF (SplineController only)
            /// </summary>
            Relative = 0,
            /// <summary>
            /// Move by calculated distance
            /// </summary>
            AbsolutePrecise = 1,
        }

        /// <summary>
        /// The play state of the controller
        /// </summary>
        public enum CurvyControllerState
        {
            Stopped,
            Playing,
            Paused
        }

        #endregion

        #region ### Events ###

        /// <summary>
        /// Invoked each time the controller finishes initialization
        /// </summary>
        public ControllerEvent OnInitialized
        {
            get { return onInitialized; }
        }

        #endregion

        #region ### Serialized Fields ###
        //TODO tooltips
        [Section("General", Sort = 0, HelpURL = CurvySpline.DOCLINK + "curvycontroller_general")]
        [Label(Tooltip = "Determines when to update")]
        public CurvyUpdateMethod UpdateIn = CurvyUpdateMethod.Update; // when to update?

        [SerializeField]
        [FieldCondition(nameof(IsNeededRigidbodyMissing), true, false, ActionAttribute.ActionEnum.ShowError,
            "Missing Rigidbody component. Its 'Is Kinematic' setting should be set to true")]
        [FieldCondition(nameof(IsNeeded2DRigidbodyMissing), true, false, ActionAttribute.ActionEnum.ShowError,
            "Missing Rigidbody 2D component. Its 'Body Type' setting should be set to 'Kinematic'")]
        [FieldCondition(nameof(IsNeededRigidbodyNotKinematic), true, false, ActionAttribute.ActionEnum.ShowError,
            "Rigidbody's 'Is Kinematic' setting should be set to true")]
        [FieldCondition(nameof(IsNeeded2DRigidbodyNotKinematic), true, false, ActionAttribute.ActionEnum.ShowError,
            "Rigidbody 2Ds 'Body Type' setting should be set to 'Kinematic'")]
        [FieldCondition(nameof(targetComponent), TargetComponent.Transform, false, ActionAttribute.ActionEnum.ShowInfo,
            "The transform's position and rotation are updated at the selected 'Update In' method.")]
        [FieldCondition(nameof(targetComponent), TargetComponent.Transform, true, ActionAttribute.ActionEnum.ShowInfo,
            "The rigidbody's position and rotation are updated at the physics simulation, and not at the selected 'Update In' method. Please consider this if getting the position or rotation via script.")]
        [Tooltip("The component controlled by the controller")]
        private TargetComponent targetComponent = TargetComponent.Transform;

        [Section("Position", Sort = 100, HelpURL = CurvySpline.DOCLINK + "curvycontroller_position")]
        [SerializeField]
        private CurvyPositionMode m_PositionMode = CurvyPositionMode.WorldUnits;

        /*! \cond PRIVATE */

        [RangeEx(0, nameof(maxPosition))]
        [SerializeField]
        [FormerlySerializedAs("m_InitialPosition")]
        [FieldCondition(nameof(ShouldDisablePositionSlider), true, false, ActionAttribute.ActionEnum.Disable)]
        protected float m_Position;

        /*! \endcond */

        [Section("Motion", Sort = 200, HelpURL = CurvySpline.DOCLINK + "curvycontroller_move")]
        [SerializeField]
        private MoveModeEnum m_MoveMode = MoveModeEnum.AbsolutePrecise;

        [Positive]
        [SerializeField]
        private float m_Speed = 0;

        [SerializeField] private MovementDirection m_Direction = MovementDirection.Forward;

        [SerializeField] private CurvyClamping m_Clamping = CurvyClamping.Loop;

        [Label("Constraints")]
        [Tooltip("Defines what motions are to be frozen")]
        [FieldCondition(nameof(AreConstraintsConflicting), true, false, ActionAttribute.ActionEnum.ShowWarning,
            "The controller targets a Rididbody that has constraints on it. This can creates conflicts with the controller's constraints")]
        [SerializeField] private MotionConstraints motionConstraints = MotionConstraints.None;

        [SerializeField, Tooltip("Start playing automatically when entering play mode")]
        private bool m_PlayAutomatically = true;

        [Section("Orientation", Sort = 300, HelpURL = CurvySpline.DOCLINK + "curvycontroller_orientation")]

        [Label("Source", "Source Vector")]
        [FieldCondition(nameof(ShowOrientationSection), false, false, Action = ActionAttribute.ActionEnum.Hide)]
        [SerializeField]
        private OrientationModeEnum m_OrientationMode = OrientationModeEnum.Orientation;

        [Label("Lock Rotation", "When set, the controller will enforce the rotation to not change")]
#if UNITY_EDITOR //Conditional to avoid WebGL build failure when using Unity 5.5.3
        [FieldCondition(nameof(m_OrientationMode), OrientationModeEnum.None, true, ConditionalAttribute.OperatorEnum.OR, "ShowOrientationSection", false, false, Action = ActionAttribute.ActionEnum.Hide)]
#endif
        [SerializeField]
        private bool m_LockRotation = true;

        [Label("Target", "Target Vector3")]
        [FieldCondition(nameof(m_OrientationMode), OrientationModeEnum.None, false, ConditionalAttribute.OperatorEnum.OR, "ShowOrientationSection", false, false, Action = ActionAttribute.ActionEnum.Hide)]
        [SerializeField]
        private OrientationAxisEnum m_OrientationAxis = OrientationAxisEnum.Up;

        [Tooltip("Should the orientation ignore the movement direction?")]
        [FieldCondition(nameof(m_OrientationMode), OrientationModeEnum.None, false, ConditionalAttribute.OperatorEnum.OR, "ShowOrientationSection", false, false, Action = ActionAttribute.ActionEnum.Hide)]
        [SerializeField]
        private bool m_IgnoreDirection;

        [DevTools.Min(0, "Direction Damping Time", "If non zero, the direction vector will not be updated instantly, but using a damping effect that will last the specified amount of time.")]
        [FieldCondition(nameof(ShowOrientationSection), false, false, Action = ActionAttribute.ActionEnum.Hide)]
        [SerializeField]
        private float m_DampingDirection;

        [DevTools.Min(0, "Up Damping Time", "If non zero, the up vector will not be updated instantly, but using a damping effect that will last the specified amount of time.")]
        [FieldCondition(nameof(ShowOrientationSection), false, false, Action = ActionAttribute.ActionEnum.Hide)]
        [SerializeField]
        private float m_DampingUp;

        [Section("Offset", Sort = 400, HelpURL = CurvySpline.DOCLINK + "curvycontroller_orientation")]
        [FieldCondition(nameof(ShowOffsetSection), false, false, Action = ActionAttribute.ActionEnum.Hide)]
        [RangeEx(-180f, 180f)]
        [SerializeField]
        private float m_OffsetAngle;

        [FieldCondition(nameof(ShowOffsetSection), false, false, Action = ActionAttribute.ActionEnum.Hide)]
        [SerializeField]
        private float m_OffsetRadius;

        [FieldCondition(nameof(ShowOffsetSection), false, false, Action = ActionAttribute.ActionEnum.Hide)]
        [Label("Compensate Offset")]
        [SerializeField]
        private bool m_OffsetCompensation = true;

        [Section("Events", Sort = 500)]
        [SerializeField]
#pragma warning disable 649
        protected ControllerEvent onInitialized = new ControllerEvent();
#pragma warning restore 649

#if UNITY_EDITOR
        [Section("Advanced Settings", Sort = 2000, HelpURL = CurvySpline.DOCLINK + "curvycontroller_general", Expanded = false)]
        [Label(Tooltip = "Force this script to update in Edit mode as often as in Play mode. Most users don't need that.")]
        [SerializeField]
        private bool m_ForceFrequentUpdates;
#endif
        #endregion

        #region ### Public Properties ###


        /// <summary>
        /// The component controlled by the controller
        /// </summary>
        public TargetComponent TargetComponent
        {
            get { return targetComponent; }
            set
            {
                if (targetComponent != value)
                    targetComponent = value;
            }
        }

        /// <summary>
        /// Gets or sets the position mode to use
        /// </summary>
        public CurvyPositionMode PositionMode
        {
            get { return m_PositionMode; }
            set
            {
                m_PositionMode = value;
            }
        }

        /// <summary>
        /// Gets or sets the movement mode to use
        /// </summary>
        public MoveModeEnum MoveMode
        {
            get { return m_MoveMode; }
            set
            {
                if (m_MoveMode != value)
                    m_MoveMode = value;
            }
        }

        /// <summary>
        /// Gets or sets whether to start playing automatically
        /// </summary>
        public bool PlayAutomatically
        {
            get { return m_PlayAutomatically; }
            set
            {
                if (m_PlayAutomatically != value)
                    m_PlayAutomatically = value;
            }
        }

        /// <summary>
        /// Gets or sets what to do when the source's end is reached
        /// </summary>
        public CurvyClamping Clamping
        {
            get { return m_Clamping; }
            set
            {
                if (m_Clamping != value)
                    m_Clamping = value;
            }
        }

        /// <summary>
        /// Defines what motions are to be frozen
        /// </summary>
        public MotionConstraints MotionConstraints
        {
            get { return motionConstraints; }
            set
            {
                if (motionConstraints != value)
                    motionConstraints = value;
            }
        }

        /// <summary>
        /// Gets or sets how to apply rotation
        /// </summary>
        public OrientationModeEnum OrientationMode
        {
            get { return m_OrientationMode; }
            set
            {
                if (m_OrientationMode != value)
                    m_OrientationMode = value;
            }
        }

        /// <summary>
        /// Used only when OrientationMode is equal to None
        /// When true, the controller will enforce the rotation to not change
        /// </summary>
        public bool LockRotation
        {
            get { return m_LockRotation; }
            set
            {
                if (m_LockRotation != value)
                    m_LockRotation = value;

                if (m_LockRotation)
                {
                    GetPositionAndRotation(out _, out Quaternion rotation);
                    LockedRotation = rotation;
                }
            }
        }

        /// <summary>
        /// Gets or sets the axis to apply the rotation to
        /// </summary>
        public OrientationAxisEnum OrientationAxis
        {
            get { return m_OrientationAxis; }
            set
            {
                if (m_OrientationAxis != value)
                    m_OrientationAxis = value;

            }
        }

        /// <summary>
        /// If non zero, the direction vector will not be updated instantly, but using a damping effect that will last the specified amount of time.
        /// </summary>
        public float DirectionDampingTime
        {
            get { return m_DampingDirection; }
            set
            {
                float v = Mathf.Max(0, value);
                if (m_DampingDirection != v)
                    m_DampingDirection = v;
            }
        }

        /// <summary>
        /// If non zero, the up vector will not be updated instantly, but using a damping effect that will last the specified amount of time.
        /// </summary>
        public float UpDampingTime
        {
            get { return m_DampingUp; }
            set
            {
                float v = Mathf.Max(0, value);
                if (m_DampingUp != v)
                    m_DampingUp = v;
            }
        }



        /// <summary>
        /// Should the controller's orientation ignore the movement direction?
        /// </summary>
        public bool IgnoreDirection
        {
            get { return m_IgnoreDirection; }
            set
            {
                if (m_IgnoreDirection != value)
                    m_IgnoreDirection = value;
            }
        }

        /// <summary>
        /// Gets or sets the angle to offset (-180° to 180° off Orientation)
        /// </summary>
        public float OffsetAngle
        {
            get { return m_OffsetAngle; }
            set
            {
                if (m_OffsetAngle != value)
                    m_OffsetAngle = value;
            }
        }
        /// <summary>
        /// Gets or sets the offset radius
        /// </summary>
        public float OffsetRadius
        {
            get { return m_OffsetRadius; }
            set
            {
                if (m_OffsetRadius != value)
                    m_OffsetRadius = value;
            }
        }

        /// <summary>
        /// Gets or sets whether to compensate offset distances in curvy paths
        /// </summary>
        public bool OffsetCompensation
        {
            get { return m_OffsetCompensation; }
            set { m_OffsetCompensation = value; }
        }

        /// <summary>
        /// Gets or sets the speed either in world units or relative, depending on MoveMode
        /// </summary>
        public float Speed
        {
            get { return m_Speed; }
            set
            {
                if (value < 0)
                {
#if CURVY_SANITY_CHECKS

                    DTLog.LogWarning("[Curvy] Trying to assign a negative value of " + value.ToString() + " to Speed. Speed should always be positive. To set direction, use the Direction property", this);
#endif
                    value = -value;
                }
                m_Speed = value;
            }
        }

        /// <summary>
        /// Gets or sets the relative position on the source, respecting Clamping
        /// </summary>
        public float RelativePosition
        {
            get
            {
                float relativePosition;
                switch (PositionMode)
                {
                    case CurvyPositionMode.Relative:
                        relativePosition = GetClampedPosition(m_Position, CurvyPositionMode.Relative, Clamping, Length);
                        break;
                    case CurvyPositionMode.WorldUnits:
                        relativePosition = AbsoluteToRelative(GetClampedPosition(m_Position, CurvyPositionMode.WorldUnits, Clamping, Length));
                        break;
                    default:
                        throw new NotSupportedException();
                }

                return relativePosition;
            }
            set
            {
                float clampedRelativePosition = GetClampedPosition(value, CurvyPositionMode.Relative, Clamping, Length);
                switch (PositionMode)
                {
                    case CurvyPositionMode.Relative:
                        m_Position = clampedRelativePosition;
                        break;
                    case CurvyPositionMode.WorldUnits:
                        m_Position = RelativeToAbsolute(clampedRelativePosition);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        /// <summary>
        /// Gets or sets the absolute position on the source, respecting Clamping
        /// </summary>
        public float AbsolutePosition
        {
            get
            {
                float absolutePosition;
                switch (PositionMode)
                {
                    case CurvyPositionMode.Relative:
                        absolutePosition = RelativeToAbsolute(GetClampedPosition(m_Position, CurvyPositionMode.Relative, Clamping, Length));
                        break;
                    case CurvyPositionMode.WorldUnits:
                        absolutePosition = GetClampedPosition(m_Position, CurvyPositionMode.WorldUnits, Clamping, Length);
                        break;
                    default:
                        throw new NotSupportedException();
                }

                return absolutePosition;
            }
            set
            {
                float clampedAbsolutePosition = GetClampedPosition(value, CurvyPositionMode.WorldUnits, Clamping, Length);
                switch (PositionMode)
                {
                    case CurvyPositionMode.Relative:
                        m_Position = AbsoluteToRelative(clampedAbsolutePosition);
                        break;
                    case CurvyPositionMode.WorldUnits:
                        m_Position = clampedAbsolutePosition;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        /// <summary>
        /// Gets or sets the position on the source (relative or absolute, depending on MoveMode), respecting Clamping
        /// </summary>
        public float Position
        {
            get
            {
                float result;
                switch (PositionMode)
                {
                    case CurvyPositionMode.Relative:
                        result = RelativePosition;
                        break;
                    case CurvyPositionMode.WorldUnits:
                        result = AbsolutePosition;
                        break;
                    default:
                        throw new NotSupportedException();
                }
                return result;
            }
            set
            {
                switch (PositionMode)
                {
                    case CurvyPositionMode.Relative:
                        RelativePosition = value;
                        break;
                    case CurvyPositionMode.WorldUnits:
                        AbsolutePosition = value;
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }
        }

        /// <summary>
        /// Gets or sets the movement direction
        /// </summary>
        public MovementDirection MovementDirection
        {
            get { return m_Direction; }
            set { m_Direction = value; }
        }


        /// <summary>
        /// The state (Playing, paused or stopped) of the controller
        /// </summary>
        public CurvyControllerState PlayState { get { return State; } }

        /// <summary>
        /// Returns true if the controller has all it dependencies ready.
        /// </summary>
        /// <remarks>A controller that is not initialized and has IsReady true, will be initialized at the next update call (automatically each frame or manually through <see cref="Refresh"/>.</remarks>
        abstract public bool IsReady { get; }

#if UNITY_EDITOR
        /// <summary>
        /// By default Unity calls scripts' update less frequently in Edit mode. ForceFrequentUpdates forces this script to update in Edit mode as often as in Play mode. Most users don't need that, but that was helpful for a user working with cameras controlled by Unity in Edit mode
        /// </summary>
        public bool ForceFrequentUpdates
        {
            get { return m_ForceFrequentUpdates; }
            set { m_ForceFrequentUpdates = value; }
        }
#endif

        #endregion

        #region ### Private & Protected Fields ###

        /// <summary>
        /// The position slider is disabled in the inspector when this property returns true
        /// </summary>
        protected virtual bool ShouldDisablePositionSlider => PositionMode == CurvyPositionMode.WorldUnits && IsReady == false;

        /// <summary>
        /// An error message used in various assertions
        /// </summary>
        protected const string ControllerNotReadyMessage = "The controller is not yet ready";

        /// <summary>
        /// The state (Playing, paused or stopped) of the controller
        /// <seealso cref="CurvyControllerState"/>
        /// </summary>
        protected CurvyControllerState State = CurvyControllerState.Stopped;

        /// <summary>
        /// The damping velocity used in the Direction damping
        /// <seealso cref="DirectionDampingTime"/>
        /// <seealso cref="Vector3.SmoothDamp(Vector3, Vector3, ref Vector3, float, float, float)"/>
        /// </summary>
        protected Vector3 DirectionDampingVelocity;//TODO should this value be reinitialized when DirectionDampingTime is set to non strictly positive value or any other moment?

        /// <summary>
        /// The damping velocity used in the Up damping
        /// <seealso cref="UpDampingTime"/>
        /// <seealso cref="Vector3.SmoothDamp(Vector3, Vector3, ref Vector3, float, float, float)"/>
        /// </summary>
        protected Vector3 UpDampingVelocity;//TODO should this value be reinitialized when UpDampingTime is set to non strictly positive value or any other moment?

        /// <summary>
        /// The position of the controller when started playing
        /// </summary>
        protected float PrePlayPosition;

        /// <summary>
        /// The <see cref="MovementDirection"/> of the controller when started playing
        /// </summary>
        protected MovementDirection PrePlayDirection;

        /// <summary>
        /// When <see cref="OrientationMode"/> is None, and <see cref="LockRotation"/> is true, this field is the value of the locked rotation, the one that will be assigned all the time to the controller
        /// </summary>
        protected Quaternion LockedRotation;


#if UNITY_EDITOR
        /// <summary>
        /// The last time the controller was updated while in Edit Mode
        /// </summary>
        protected float EditModeLastUpdate;
#endif

        #endregion

        #region ### Unity Callbacks ###
        /*! \cond UNITY */
        protected virtual void OnEnable()
        {
            if (isInitialized == false && IsReady)
            {
                Initialize();
                InitializedApplyDeltaTime(0);
            }

#if UNITY_EDITOR
            EditorApplication.update += editorUpdate;
#endif
        }

        protected virtual void Start()
        {
            if (isInitialized == false && IsReady)
            {
                Initialize();
                InitializedApplyDeltaTime(0);
            }

            if (PlayAutomatically && Application.isPlaying)
                Play();
        }

        protected virtual void OnDisable()
        {
#if UNITY_EDITOR
            EditorApplication.update -= editorUpdate;
#endif
            if (isInitialized)
                Deinitialize();
        }

        protected virtual void Update()
        {
            if (UpdateIn == CurvyUpdateMethod.Update)
                ApplyDeltaTime(TimeSinceLastUpdate);
        }

        protected virtual void LateUpdate()
        {
            if (UpdateIn == CurvyUpdateMethod.LateUpdate ||
                (Application.isPlaying == false && UpdateIn == CurvyUpdateMethod.FixedUpdate)) // In edit mode, fixed updates are not called, so we update the controller here instead
                ApplyDeltaTime(TimeSinceLastUpdate);
        }

        protected virtual void FixedUpdate()
        {
            if (UpdateIn == CurvyUpdateMethod.FixedUpdate)
                ApplyDeltaTime(TimeSinceLastUpdate);
        }

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            Speed = m_Speed;
            LockRotation = m_LockRotation;
        }
#endif

        protected virtual void Reset()
        {
            UpdateIn = CurvyUpdateMethod.Update;
            TargetComponent = TargetComponent.Transform;
            PositionMode = CurvyPositionMode.WorldUnits;
            m_Position = 0;
            PlayAutomatically = true;
            MoveMode = MoveModeEnum.AbsolutePrecise;
            Speed = 0;
            LockRotation = true;
            MotionConstraints = MotionConstraints.None;
            Clamping = CurvyClamping.Loop;
            OrientationMode = OrientationModeEnum.Orientation;
            OrientationAxis = OrientationAxisEnum.Up;
            IgnoreDirection = false;
        }
        /*! \endcond */
        #endregion

        #region ### Virtual Properties & Methods  ###

        /// <summary>
        /// Gets the transform being controlled by this controller.
        /// </summary>
        /// <seealso cref="TargetComponent"/>
        public virtual Transform Transform
        {
            get
            {
                return transform;
            }
        }

        /// <summary>
        /// Gets the rigidbody being controlled by this controller.
        /// </summary>
        /// <seealso cref="TargetComponent"/>
        public virtual Rigidbody Rigidbody
        {
            [CanBeNull]
            get
            {
                return transform.GetComponent<Rigidbody>();
            }
        }

        /// <summary>
        /// Gets the 2d rigidbody being controlled by this controller.
        /// </summary>
        /// <seealso cref="TargetComponent"/>
        public virtual Rigidbody2D Rigidbody2D
        {
            [CanBeNull]
            get
            {
                return transform.GetComponent<Rigidbody2D>();
            }
        }

        /// <summary>
        /// Advances the controller state by deltaTime seconds. Is called only for initialized controllers
        /// </summary>
        protected virtual void InitializedApplyDeltaTime(float deltaTime)
        {
#if UNITY_EDITOR
            EditModeLastUpdate = Time.realtimeSinceStartup;
#endif
            if (State == CurvyControllerState.Playing && Speed * deltaTime != 0)
            {
                float speed = UseOffset && OffsetCompensation && OffsetRadius != 0f
                    ? ComputeOffsetCompensatedSpeed(deltaTime)
                    : Speed;

                if (speed * deltaTime != 0)
                    Advance(speed, deltaTime);
            }

            Vector3 preRefreshPosition;
            Quaternion preRefreshOrientation;
            GetPositionAndRotation(out preRefreshPosition, out preRefreshOrientation);

            Vector3 newPosition;
            Vector3 newForward;
            Vector3 newUp;
            ComputeTargetPositionAndRotation(out newPosition, out newUp, out newForward);

            Vector3 postDampingForward;
            if (DirectionDampingTime > 0 && State == CurvyControllerState.Playing)
            {
                postDampingForward = deltaTime > 0
                    ? Vector3.SmoothDamp(preRefreshOrientation * Vector3.forward, newForward, ref DirectionDampingVelocity, DirectionDampingTime, float.PositiveInfinity, deltaTime)
                    : preRefreshOrientation * Vector3.forward;
            }
            else
                postDampingForward = newForward;

            Vector3 postDampingUp;
            if (UpDampingTime > 0 && State == CurvyControllerState.Playing)
            {
                postDampingUp = deltaTime > 0
                    ? Vector3.SmoothDamp(preRefreshOrientation * Vector3.up, newUp, ref UpDampingVelocity, UpDampingTime, float.PositiveInfinity, deltaTime)
                    : preRefreshOrientation * Vector3.up;
            }
            else
                postDampingUp = newUp;

            Quaternion newRotation = Quaternion.LookRotation(postDampingForward, postDampingUp);

            SetPositionAndRotation(newPosition, newRotation);

            if (preRefreshPosition.NotApproximately(newPosition) || preRefreshOrientation.DifferentOrientation(newRotation))
                UserAfterUpdate();
        }

        /// <summary>
        /// Gets the position and rotation of the controller, ignoring any damping or other interpolations
        /// </summary>
        /// <param name="targetPosition"></param>
        /// <param name="targetUp"></param>
        /// <param name="targetForward"></param>
        protected virtual void ComputeTargetPositionAndRotation(out Vector3 targetPosition, out Vector3 targetUp, out Vector3 targetForward)
        {
            Vector3 pos;
            Vector3 tangent;
            Vector3 orientation;
            GetInterpolatedSourcePosition(RelativePosition, out pos, out tangent, out orientation);

            if (tangent == Vector3.zero || orientation == Vector3.zero)
                GetOrientationNoneUpAndForward(out targetUp, out targetForward);
            else
            {
                switch (OrientationMode)
                {
                    case OrientationModeEnum.None:
                        GetOrientationNoneUpAndForward(out targetUp, out targetForward);
                        break;
                    case OrientationModeEnum.Orientation:
                        {
                            Vector3 signedTangent = (m_Direction == MovementDirection.Backward && IgnoreDirection == false) ? -tangent : tangent;
                            switch (OrientationAxis)
                            {
                                case OrientationAxisEnum.Up:
                                    targetUp = orientation;
                                    targetForward = signedTangent;
                                    break;
                                case OrientationAxisEnum.Down:
                                    targetUp = -orientation;
                                    targetForward = signedTangent;
                                    break;
                                case OrientationAxisEnum.Forward:
                                    targetUp = -signedTangent;
                                    targetForward = orientation;
                                    break;
                                case OrientationAxisEnum.Backward:
                                    targetUp = signedTangent;
                                    targetForward = -orientation;
                                    break;
                                case OrientationAxisEnum.Left:
                                    targetUp = Vector3.Cross(orientation, signedTangent);
                                    targetForward = signedTangent;
                                    break;
                                case OrientationAxisEnum.Right:
                                    targetUp = Vector3.Cross(signedTangent, orientation);
                                    targetForward = signedTangent;
                                    break;
                                default:
                                    throw new NotSupportedException();
                            }
                        }
                        break;
                    case OrientationModeEnum.Tangent:
                        {

                            Vector3 signedTangent = (m_Direction == MovementDirection.Backward && IgnoreDirection == false) ? -tangent : tangent;
                            switch (OrientationAxis)
                            {
                                case OrientationAxisEnum.Up:
                                    targetUp = signedTangent;
                                    targetForward = -orientation;
                                    break;
                                case OrientationAxisEnum.Down:
                                    targetUp = -signedTangent;
                                    targetForward = orientation;
                                    break;
                                case OrientationAxisEnum.Forward:
                                    targetUp = orientation;
                                    targetForward = signedTangent;
                                    break;
                                case OrientationAxisEnum.Backward:
                                    targetUp = orientation;
                                    targetForward = -signedTangent;
                                    break;
                                case OrientationAxisEnum.Left:
                                    targetUp = orientation;
                                    targetForward = Vector3.Cross(orientation, signedTangent);
                                    break;
                                case OrientationAxisEnum.Right:
                                    targetUp = orientation;
                                    targetForward = Vector3.Cross(signedTangent, orientation);
                                    break;
                                default:
                                    throw new NotSupportedException();
                            }
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            targetPosition = (UseOffset && OffsetRadius != 0f)
                ? ApplyOffset(pos, tangent, orientation, OffsetAngle, OffsetRadius)
                : pos;
        }


        virtual protected void Initialize()
        {
            isInitialized = true;
            GetPositionAndRotation(out _, out Quaternion rotation);
            LockedRotation = rotation;
            DirectionDampingVelocity = UpDampingVelocity = Vector3.zero;

            BindEvents();
            UserAfterInit();
            onInitialized.Invoke(this);
        }

        virtual protected void Deinitialize()
        {
            UnbindEvents();
            isInitialized = false;
        }

        /// <summary>
        /// Binds any external events
        /// </summary>
        protected virtual void BindEvents()
        {
#if CURVY_SANITY_CHECKS
            Assert.IsTrue(isInitialized);
#endif
        }
        /// <summary>
        /// Unbinds any external events
        /// </summary>
        protected virtual void UnbindEvents()
        {
#if CURVY_SANITY_CHECKS
            Assert.IsTrue(isInitialized);
#endif
        }

        protected virtual void SavePrePlayState()
        {
            PrePlayPosition = m_Position;
            PrePlayDirection = m_Direction;
        }

        protected virtual void RestorePrePlayState()
        {
            m_Position = PrePlayPosition;
            m_Direction = PrePlayDirection;
        }

        /// <summary>
        /// Gets the current position and rotation of the target component
        /// </summary>
        /// <seealso cref="TargetComponent"/>
        protected virtual void GetPositionAndRotation(out Vector3 position, out Quaternion rotation)
        {
            switch (TargetComponent)
            {
                case TargetComponent.Transform:
                    {
                        Transform cachedTransform = Transform;
                        position = cachedTransform.position;
                        rotation = cachedTransform.rotation;
                    }
                    break;
                case TargetComponent.KinematicRigidbody:
                    {
                        Rigidbody cachedRigidBody = Rigidbody;
                        if (cachedRigidBody == null || Application.isPlaying == false)
                        {
                            Transform cachedTransform = Transform;
                            position = cachedTransform.position;
                            rotation = cachedTransform.rotation;
                        }
                        else
                        {
                            position = cachedRigidBody.position;
                            rotation = cachedRigidBody.rotation;
                        }
                    }
                    break;
                case TargetComponent.KinematicRigidbody2D:
                    {
                        Rigidbody2D cachedRigidBody = Rigidbody2D;
                        if (cachedRigidBody == null || Application.isPlaying == false)
                        {
                            Transform cachedTransform = Transform;
                            position = cachedTransform.position;
                            rotation = cachedTransform.rotation;
                        }
                        else
                        {
                            position = cachedRigidBody.position;
                            rotation = Quaternion.AngleAxis(Rigidbody2D.rotation, cachedRigidBody.transform.rotation * new Vector3(0, 0, 1));
                        }
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Sets a new position and rotation to the target component
        /// </summary>
        /// <seealso cref="TargetComponent"/>
        protected virtual void SetPositionAndRotation(Vector3 position, Quaternion rotation)
        {
            Vector3 constrainedPosition;
            Quaternion constrainedRotation;
            if (MotionConstraints == MotionConstraints.None)
            {
                constrainedPosition = position;
                constrainedRotation = rotation;
            }
            else
            {
                GetPositionAndRotation(out var oldPosition, out var oldRotation);

                //position
                {
                    constrainedPosition.x =
                        (MotionConstraints & MotionConstraints.FreezePositionX) == 0
                            ? position.x
                            : oldPosition.x;

                    constrainedPosition.y =
                        (MotionConstraints & MotionConstraints.FreezePositionY) == 0
                            ? position.y
                            : oldPosition.y;

                    constrainedPosition.z =
                        (MotionConstraints & MotionConstraints.FreezePositionZ) == 0
                            ? position.z
                            : oldPosition.z;
                }

                //rotation
                {

                    Vector3 constrainedRotationEuler;
                    {
                        Vector3 rotationEuler = rotation.eulerAngles;
                        Vector3 oldRotationEuler = oldRotation.eulerAngles;

                        constrainedRotationEuler.x =
                            (MotionConstraints & MotionConstraints.FreezeRotationX) == 0
                                ? rotationEuler.x
                                : oldRotationEuler.x;

                        constrainedRotationEuler.y =
                            (MotionConstraints & MotionConstraints.FreezeRotationY) == 0
                                ? rotationEuler.y
                                : oldRotationEuler.y;

                        constrainedRotationEuler.z =
                            (MotionConstraints & MotionConstraints.FreezeRotationZ) == 0
                                ? rotationEuler.z
                                : oldRotationEuler.z;
                    }

                    constrainedRotation = Quaternion.Euler(constrainedRotationEuler);
                }
            }

            switch (TargetComponent)
            {
                case TargetComponent.Transform:
                    {
                        Transform.SetPositionAndRotation(constrainedPosition, constrainedRotation);
                    }
                    break;
                case TargetComponent.KinematicRigidbody:
                    {
                        Rigidbody cachedRigidBody = Rigidbody;
                        if (cachedRigidBody == null || Application.isPlaying == false)
                        {
                            Transform.SetPositionAndRotation(constrainedPosition, constrainedRotation);
                        }
                        else
                        {
                            cachedRigidBody.MovePosition(constrainedPosition); //cachedRigidbody.position is not yet updated. Will be done after physics simulation.
                            cachedRigidBody.MoveRotation(constrainedRotation); //cachedRigidbody.roation is not yet updated. Will be done after physics simulation.
                        }
                    }
                    break;
                case TargetComponent.KinematicRigidbody2D:
                    {
                        Rigidbody2D cachedRigidBody = Rigidbody2D;
                        if (cachedRigidBody == null || Application.isPlaying == false)
                        {
                            Transform.SetPositionAndRotation(constrainedPosition, constrainedRotation);
                        }
                        else
                        {
                            cachedRigidBody.MovePosition(constrainedPosition); //cachedRigidbody.position is not yet updated. Will be done after physics simulation.
                            cachedRigidBody.MoveRotation(constrainedRotation); //cachedRigidbody.roation is not yet updated. Will be done after physics simulation.
                        }
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #region ### Virtual Methods for inherited custom controllers (Easy mode) ###

        /// <summary>
        /// Called after the controller is initialized
        /// </summary>
        protected virtual void UserAfterInit() { }
        /// <summary>
        /// Called after the controller has updated it's position or rotation
        /// </summary>
        protected virtual void UserAfterUpdate() { }

        #endregion

        #region Conditional display in the inspector of CurvyController properties

        /// <summary>
        /// Whether the controller should display the CurvyController properties under the Orientation section or not.
        /// </summary>
        protected virtual bool ShowOrientationSection
        {
            get { return true; }
        }
        /// <summary>
        /// Whether the controller should display the CurvyController properties under the Offset section or not.
        /// </summary>
        protected virtual bool ShowOffsetSection
        {
            get { return OrientationMode != OrientationModeEnum.None; }
        }

        #endregion

        #endregion

        #region ### Abstract Properties and Methods ###

        /// <summary>
        /// Gets the source's length
        /// </summary>
        public abstract float Length { get; }

        /// <summary>
        /// Advance the controller and return the new position. This method will do side effect operations if needed, like updating some internal state, or trigerring events.
        /// </summary>
        /// <param name="speed">controller's speed. Should be strictely positive</param>
        /// <param name="deltaTime">the time that the controller should advance with. Should be strictely positive</param>
        abstract protected void Advance(float speed, float deltaTime);

        /// <summary>
        /// Advance the controller and return the new position. Contrary to <see cref="Advance"/>, this method will not do any side effect operations, like updating some internal state, or trigerring events
        /// 
        /// </summary>
        /// <param name="tf">the current virtual position (either TF or World Units) </param>
        /// <param name="curyDirection">the current direction</param>
        /// <param name="speed">controller's speed. Should be strictely positive</param>
        /// <param name="deltaTime">the time that the controller should advance with. Should be strictely positive</param>
        abstract protected void SimulateAdvance(ref float tf, ref MovementDirection curyDirection, float speed, float deltaTime);

        /// <summary>
        /// Converts distance on source from absolute to relative position.
        /// </summary>
        /// <param name="worldUnitDistance">distance in world units from the source start. Should be already clamped</param>
        /// <returns>relative distance in the range 0..1</returns>
        abstract protected float AbsoluteToRelative(float worldUnitDistance);

        /// <summary>
        /// Converts distance on source from relative to absolute position.
        /// </summary>
        /// <param name="relativeDistance">relative distance from the source start. Should be already clamped</param>
        /// <returns>distance in world units from the source start</returns>
        abstract protected float RelativeToAbsolute(float relativeDistance);

        /// <summary>
        /// Retrieve the source global position for a given relative position (TF)
        /// </summary>
        abstract protected Vector3 GetInterpolatedSourcePosition(float tf);

        /// <summary>
        /// Retrieve the source global position, tangent and orientation for a given relative position (TF)
        /// </summary>
        abstract protected void GetInterpolatedSourcePosition(float tf, out Vector3 interpolatedPosition, out Vector3 tangent, out Vector3 up);

        /// <summary>
        /// Retrieve the source global Orientation/Up-Vector for a given relative position
        /// </summary>
        abstract protected Vector3 GetOrientation(float tf);

        /// <summary>
        /// Gets global tangent for a given relative position
        /// </summary>
        abstract protected Vector3 GetTangent(float tf);


        #endregion


        #region Non virtual public methods 

        /// <summary>
        /// Plays the controller. Calling this method while the controller is playing will have no effect.
        /// </summary>
        public void Play()
        {
            if (PlayState == CurvyControllerState.Stopped)
                SavePrePlayState();
            State = CurvyControllerState.Playing;
        }

        /// <summary>
        /// Stops the controller, and restore its position (and other relevant states) to its state when starting playing
        /// </summary>
        public void Stop()
        {
            if (PlayState != CurvyControllerState.Stopped)
                RestorePrePlayState();
            State = CurvyControllerState.Stopped;
        }

        /// <summary>
        /// Pauses the controller. To unpause it call Play()
        /// </summary>
        public void Pause()
        {
            if (PlayState == CurvyControllerState.Playing)
                State = CurvyControllerState.Paused;
        }

        /// <summary>
        /// Forces the controller to update its state, without waiting for the automatic per frame update.
        /// Can initialize or deinitialize the controller if the right conditions are met.
        /// </summary>
        public void Refresh()
        {
            ApplyDeltaTime(0);
        }

        /// <summary>
        /// Advances the controller state by deltaTime seconds, without waiting for the automatic per frame update.
        /// Can initialize or deinitialize the controller if the right conditions are met.
        /// </summary>
        public void ApplyDeltaTime(float deltaTime)
        {
            if (isInitialized == false && IsReady)
                Initialize();
            else if (isInitialized && IsReady == false)
                Deinitialize();

            if (isInitialized)
                InitializedApplyDeltaTime(deltaTime);
        }

        /// <summary>
        /// Teleports the controller to a specific position, while handling events triggering and connections.
        /// </summary>
        /// <remarks> Internally, the teleport is handled as a movement of high speed on small time (0.001s). This will call <see cref="ApplyDeltaTime"/> with that small amount of time.</remarks>
        public void TeleportTo(float newPosition)
        {
            float distance = Mathf.Abs(Position - newPosition);
            MovementDirection direction = Position < newPosition
                ? MovementDirection.Forward
                : MovementDirection.Backward;
            TeleportBy(distance, direction);
        }

        /// <summary>
        /// Teleports the controller to by a specific distance, while handling events triggering and connections.
        /// </summary>
        /// <param name="distance"> A positive distance</param>
        /// <param name="direction"> Direction of teleportation</param>
        /// <remarks> Internally, the teleport is handled as a movement of high speed on small time (0.001s). This will call <see cref="ApplyDeltaTime"/> with that small amount of time.</remarks>
        public void TeleportBy(float distance, MovementDirection direction)
        {
#if CURVY_SANITY_CHECKS
            Assert.IsTrue(distance >= 0);
#endif
            if (PlayState != CurvyControllerState.Playing)
                DTLog.LogError("[Curvy] Calling TeleportBy on a controller that is stopped. Please make the controller play first", this);

            float preWrapSpeed = Speed;
            MovementDirection preWrapDirection = MovementDirection;

            const float timeFraction = 1000;
            Speed = Mathf.Abs(distance) * timeFraction;
            MovementDirection = direction;

            ApplyDeltaTime(1 / timeFraction);

            Speed = preWrapSpeed;
            MovementDirection = preWrapDirection;
        }



        /// <summary>
        /// Event-friedly helper that sets a field or property value
        /// </summary>
        /// <param name="fieldAndValue">e.g. "MyValue=42"</param>
        public void SetFromString(string fieldAndValue)
        {
            string[] f = fieldAndValue.Split('=');
            if (f.Length != 2)
                return;

            FieldInfo fi = this.GetType().FieldByName(f[0], true, false);
            if (fi != null)
            {
                try
                {
                    if (fi.FieldType.IsEnum)
                        fi.SetValue(this, System.Enum.Parse(fi.FieldType, f[1]));
                    else
                        fi.SetValue(this, System.Convert.ChangeType(f[1], fi.FieldType, System.Globalization.CultureInfo.InvariantCulture));
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning(this.name + ".SetFromString(): " + e.ToString());
                }
            }
            else
            {
                PropertyInfo pi = this.GetType().PropertyByName(f[0], true, false);
                if (pi != null)
                {
                    try
                    {
                        if (pi.PropertyType.IsEnum)
                            pi.SetValue(this, System.Enum.Parse(pi.PropertyType, f[1]), null);
                        else
                            pi.SetValue(this, System.Convert.ChangeType(f[1], pi.PropertyType, System.Globalization.CultureInfo.InvariantCulture), null);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning(this.name + ".SetFromString(): " + e.ToString());
                    }
                }
            }
        }
        #endregion



        #region ### Privates & Protected Methods & Properties ###

        /// <summary>
        /// Whether or not the controller is initialized. Initialization happens before first usage
        /// </summary>
        protected bool isInitialized { get; private set; }

        /// <summary>
        /// When in Play mode, the controller update happens only in Update or Late Update of Fixed Update, so the time since last update is always equal to Time.deltaTime
        /// When in Edit mode, the controller update happens at various points, including the editor's update, so we compute the time since last update using a time stamp
        /// </summary>
        protected float TimeSinceLastUpdate
        {
            get
            {
#if UNITY_EDITOR
                return (Application.isPlaying) ?
                    Time.deltaTime :
                    Time.realtimeSinceStartup - EditModeLastUpdate;
#else
                    return  Time.deltaTime;
#endif
            }
        }

        /// <summary>
        /// Whether this controller uses Offsetting or not
        /// </summary>
        protected bool UseOffset
        {
            get { return ShowOffsetSection; }
        }

#if UNITY_EDITOR
        private void editorUpdate()
        {
            if (Application.isPlaying == false)
            {
                if (ForceFrequentUpdates)
                    EditorApplication.QueuePlayerLoopUpdate();
                else
                    ApplyDeltaTime(TimeSinceLastUpdate);
            }
        }
#endif

        /// <summary>
        /// Returns the position of the controller after applying an offset
        /// </summary>
        /// <param name="position">The controller's position</param>
        /// <param name="tangent">The tangent at the controller's position</param>
        /// <param name="up">The Up direction at the controller's position</param>
        /// <param name="offsetAngle"><see cref="OffsetAngle"/></param>
        /// <param name="offsetRadius"><see cref="OffsetRadius"/></param>
        protected static Vector3 ApplyOffset(Vector3 position, Vector3 tangent, Vector3 up, float offsetAngle, float offsetRadius)
        {
            Quaternion offsetRotation = Quaternion.AngleAxis(offsetAngle, tangent);
            return position.Addition((offsetRotation * up).Multiply(offsetRadius));
        }

        /// <summary>
        /// Return the clamped position
        /// </summary>
        protected static float GetClampedPosition(float position, CurvyPositionMode positionMode, CurvyClamping clampingMode, float length)
        {
            float clampedPosition;
            {
                switch (positionMode)
                {
                    case CurvyPositionMode.Relative:
                        if (position == 1)
                            clampedPosition = 1;
                        else
                            clampedPosition = CurvyUtility.ClampTF(position, clampingMode);
                        break;
                    case CurvyPositionMode.WorldUnits:
                        if (position == length)
                            clampedPosition = length;
                        else
                            clampedPosition = CurvyUtility.ClampDistance(position, clampingMode, length);
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }
            return clampedPosition;
        }

        private float maxPosition
        {
            get
            {
                return GetMaxPosition(PositionMode);
            }
        }

        /// <summary>
        /// Returns the maximal valid position value using the given <see cref="CurvyPositionMode"/>
        /// </summary>
        /// <param name="positionMode"></param>
        protected float GetMaxPosition(CurvyPositionMode positionMode)
        {
            float result;
            switch (positionMode)
            {
                case CurvyPositionMode.Relative:
                    result = 1;
                    break;
                case CurvyPositionMode.WorldUnits:
                    result = IsReady
                        ? Length
                        : 0;
                    break;
                default:
                    throw new NotSupportedException();
            }

            return result;
        }

        /// <summary>
        /// Returns the Speed after applying Offset Compensation <see cref="OffsetCompensation"/>
        /// </summary>
        /// <param name="deltaTime">Time since last update</param>
        /// <returns></returns>
        protected float ComputeOffsetCompensatedSpeed(float deltaTime)
        {
#if CURVY_SANITY_CHECKS
            Assert.IsTrue(deltaTime > 0);
            Assert.IsTrue(UseOffset);
#endif

            if (OffsetRadius == 0)
                return Speed;

            Vector3 previousOffsetlesPosition;
            Vector3 previousOffsetPosition;
            {
                Vector3 previousTangent;
                Vector3 previousUp;
                GetInterpolatedSourcePosition(RelativePosition, out previousOffsetlesPosition, out previousTangent, out previousUp);

                previousOffsetPosition = ApplyOffset(previousOffsetlesPosition, previousTangent, previousUp, OffsetAngle, OffsetRadius);
            }

            Vector3 offsetlesPosition;
            Vector3 offsetPosition;
            {
                float offsetlesRelativePosition;
                {
                    offsetlesRelativePosition = RelativePosition;
                    MovementDirection curvyDirection = m_Direction;
                    SimulateAdvance(ref offsetlesRelativePosition, ref curvyDirection, Speed, deltaTime);
                }

                Vector3 offsetlesTangent;
                Vector3 offsetlesUp;
                GetInterpolatedSourcePosition(offsetlesRelativePosition, out offsetlesPosition, out offsetlesTangent, out offsetlesUp);

                offsetPosition = ApplyOffset(offsetlesPosition, offsetlesTangent, offsetlesUp, OffsetAngle, OffsetRadius);
            }

            float deltaPosition = (offsetlesPosition - previousOffsetlesPosition).magnitude;
            float deltaOffsetPosition = (previousOffsetPosition - offsetPosition).magnitude;
            float ratio = (deltaPosition / deltaOffsetPosition);
            return Speed * (float.IsNaN(ratio) ? 1 : ratio);
        }

        //TODO This should be a local method when all supported unity versions will handle C#7
        /// <summary>
        /// Gets the Up and Forward of the orientation when the <see cref="OrientationMode"/> is set to <see cref="OrientationModeEnum.None"/>
        /// </summary>
        private void GetOrientationNoneUpAndForward(out Vector3 targetUp, out Vector3 targetForward)
        {
            if (LockRotation)
            {
                targetUp = LockedRotation * Vector3.up;
                targetForward = LockedRotation * Vector3.forward;
            }
            else
            {
                GetPositionAndRotation(out _, out Quaternion rotation);

                targetUp = rotation * Vector3.up;
                targetForward = rotation * Vector3.forward;
            }
        }
        #region Rigibody target handling
        private bool IsNeededRigidbodyMissing
        {
            get
            {
                return targetComponent == TargetComponent.KinematicRigidbody && Rigidbody == null;
            }
        }

        private bool IsNeeded2DRigidbodyMissing
        {
            get
            {
                return targetComponent == TargetComponent.KinematicRigidbody2D && Rigidbody2D == null;
            }
        }

        private bool IsNeededRigidbodyNotKinematic
        {
            get
            {
                Rigidbody localRigidBody = Rigidbody;
                return targetComponent == TargetComponent.KinematicRigidbody && localRigidBody != null && localRigidBody.isKinematic == false;
            }
        }

        private bool IsNeeded2DRigidbodyNotKinematic
        {
            get
            {
                Rigidbody2D localRigidBody = Rigidbody2D;
                return targetComponent == TargetComponent.KinematicRigidbody2D && localRigidBody != null && localRigidBody.isKinematic == false;
            }
        }

        private bool AreConstraintsConflicting
        {
            get
            {
                Rigidbody localRigidBody;
                Rigidbody2D localRigidBody2D;
                bool isConflicting;
                switch (TargetComponent)
                {
                    case TargetComponent.KinematicRigidbody when (localRigidBody = Rigidbody) != null:
                        isConflicting = localRigidBody.constraints != RigidbodyConstraints.None;
                        break;
                    case TargetComponent.KinematicRigidbody2D when (localRigidBody2D = Rigidbody2D) != null:
                        isConflicting = localRigidBody2D.constraints != RigidbodyConstraints2D.None;
                        break;
                    default:
                        isConflicting = false;
                        break;
                }

                return isConflicting;
            }
        }

        #endregion

        #endregion

        #region ISerializationCallbackReceiver
        /*! \cond PRIVATE */
        /// <summary>
        /// Implementation of UnityEngine.ISerializationCallbackReceiver
        /// Called automatically by Unity, is not meant to be called by Curvy's users
        /// </summary>
        public void OnBeforeSerialize()
        {
        }

        /// <summary>
        /// Implementation of UnityEngine.ISerializationCallbackReceiver
        /// Called automatically by Unity, is not meant to be called by Curvy's users
        /// </summary>
        public virtual void OnAfterDeserialize()
        {
            if (m_Speed < 0)
            {
                m_Speed = Mathf.Abs(m_Speed);
                m_Direction = MovementDirection.Backward;
            }

            //Merged AbsolutePrecise and AbsoluteExtrapolate into one value
            if ((short)MoveMode == 2)
                MoveMode = MoveModeEnum.AbsolutePrecise;
        }
        /*! \endcond */
        #endregion
    }
}