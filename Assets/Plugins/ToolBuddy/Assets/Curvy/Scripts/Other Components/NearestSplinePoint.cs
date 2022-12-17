// =====================================================================
// Copyright 2013-2022 ToolBuddy
// All rights reserved
// 
// http://www.toolbuddy.net
// =====================================================================

using FluffyUnderware.DevTools;
using UnityEngine;
using UnityEditor;
using UnityEngine.Events;

namespace FluffyUnderware.Curvy
{

    /// <summary>
    /// Given a <see cref="CurvySpline"/> and an input position, this class will provide you with the nearest point to that position on the given spline.
    /// The nearest position is provided either in an <see cref="OnUpdated"/> event, or as an override of the position of the <see cref="TargetPosition"/>
    /// </summary>
    /// <remarks>This script simply calls the <see cref="CurvySpline.GetNearestPoint"/> method. If you are a programmer, you don't need to go through this script, just call <see cref="CurvySpline.GetNearestPoint"/></remarks>
    [HelpURL(CurvySpline.DOCLINK + "nearestsplinepoint")]
    [AddComponentMenu("Curvy/Misc/Nearest Spline Point")]
    [ExecuteAlways]
    public class NearestSplinePoint : MonoBehaviour
    {
        /// <summary>
        /// The <see cref="CurvySpline"/> on which the nearest position is searched for
        /// </summary>
        [Tooltip("The spline on which the nearest position is searched for")]
        public CurvySpline Spline;
        /// <summary>
        /// A transform which position will be used as the input position for the lookup
        /// </summary>
        [Tooltip("A transform which position will be used as the input position for the lookup")]
        public Transform SourcePosition;
        /// <summary>
        /// A transform which position will be updated with the nearest point on Spline to Source Position
        /// </summary>
        [Tooltip("A transform which position will be updated with the nearest point on Spline to Source Position")]
        public Transform TargetPosition;
        /// <summary>
        /// When to run the lookup
        /// </summary>
        [Tooltip("When to run the lookup")]
        public CurvyUpdateMethod UpdateIn;
        /// <summary>
        /// At each update, this event is called with the result of the lookup
        /// </summary>
        [Tooltip("At each update, this event is called with the result of the lookup")]
        public UnityEventEx<Vector3> OnUpdated = new UnityEventEx<Vector3>();

        private void Process()
        {
            if (SourcePosition && Spline && Spline.IsInitialized && Spline.Dirty == false)
            {
                Vector3 destinationPosition = Spline.GetNearestPoint(SourcePosition.position, Space.Self);
                if (TargetPosition)
                    TargetPosition.position = destinationPosition;
                OnUpdated?.Invoke(destinationPosition);
            }
        }

        #region Updating methods

#if UNITY_EDITOR
        private void Update()
        {
            if (UpdateIn == CurvyUpdateMethod.Update)
                Process();
        }

        private void LateUpdate()
        {
            if (UpdateIn == CurvyUpdateMethod.LateUpdate ||
                (Application.isPlaying == false && UpdateIn == CurvyUpdateMethod.FixedUpdate)) // In edit mode, fixed updates are not called, so we update here instead
                Process();
        }

        private void FixedUpdate()
        {
            if (UpdateIn == CurvyUpdateMethod.FixedUpdate)
                Process();
        }

        protected virtual void OnEnable()
        {
            EditorApplication.update += OnEditorUpdate;
        }

        protected virtual void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
        }

        private void OnEditorUpdate()
        {
            if (Application.isPlaying == false)
                Process();
        }
#endif

        #endregion

    }
}