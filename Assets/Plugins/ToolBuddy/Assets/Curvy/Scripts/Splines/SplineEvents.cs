// =====================================================================
// Copyright 2013-2022 ToolBuddy
// All rights reserved
// 
// http://www.toolbuddy.net
// =====================================================================

using UnityEngine;
using FluffyUnderware.DevTools;
using UnityEngine.Assertions;

#if CONTRACTS_FULL
using System.Diagnostics.Contracts;
#endif
namespace FluffyUnderware.Curvy
{
    /// <summary>
    /// Class used by spline related events
    /// </summary>
    [System.Serializable]
    public class CurvySplineEvent : UnityEventEx<CurvySplineEventArgs> { }

    /// <summary>
    /// Class used by spline related events
    /// </summary>
    [System.Serializable]
    public class CurvyControlPointEvent : UnityEventEx<CurvyControlPointEventArgs> { }

    /// <summary>
    /// EventArgs used by CurvyControlPointEvent events
    /// </summary>
    public class CurvyControlPointEventArgs : CurvySplineEventArgs
    {
        /// <summary>
        /// Event Mode
        /// </summary>
        public enum ModeEnum
        {
            /// <summary>
            /// Send for events that are not related to control points adding or removal
            /// </summary>
            None,
            /// <summary>
            /// Send when a Control point is added before an existing one
            /// </summary>
            AddBefore,
            /// <summary>
            /// Send when a Control point is added after an existing one
            /// </summary>
            AddAfter,
            /// <summary>
            /// Send when a Control point is deleted
            /// </summary>
            Delete
        }

        /// <summary>
        /// Determines the action this event was raised for
        /// </summary>
        public readonly ModeEnum Mode;
        /// <summary>
        /// Related Control Point
        /// </summary>
        public readonly CurvySplineSegment ControlPoint;

        public CurvyControlPointEventArgs(MonoBehaviour sender, CurvySpline spline, CurvySplineSegment cp, ModeEnum mode = ModeEnum.None, object data = null) : base(sender, spline, data)
        {
            ControlPoint = cp;
            Mode = mode;
        }
    }



    /// <summary>
    /// EventArgs used by CurvySplineEvent events
    /// </summary>
    public class CurvySplineEventArgs : CurvyEventArgs
    {
        /// <summary>
        /// The related spline
        /// </summary>
        public readonly CurvySpline Spline;

        public CurvySplineEventArgs(MonoBehaviour sender, CurvySpline spline, object data = null) : base(sender, data)
        {
            Spline = spline;


#if CURVY_SANITY_CHECKS
            Assert.IsTrue(System.Object.ReferenceEquals(Spline,null) == false);
#endif
        }
    }
}