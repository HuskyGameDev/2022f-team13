// =====================================================================
// Copyright 2013-2022 ToolBuddy
// All rights reserved
// 
// http://www.toolbuddy.net
// =====================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluffyUnderware.DevTools;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

namespace FluffyUnderware.Curvy.Controllers
{
    #region ### Controller Events ###

    [System.Serializable]
    public class ControllerEvent : UnityEventEx<CurvyController> { }

    /// <summary>
    /// EventArgs used by spline controller movements
    /// </summary>
    [System.Serializable]
    public class CurvySplineMoveEvent : UnityEventEx<CurvySplineMoveEventArgs> { }

    /// <summary>
    /// EventArgs used by spline controller movements
    /// </summary>
    public class CurvySplineMoveEventArgs : System.ComponentModel.CancelEventArgs
    {
        /// <summary>
        /// The Spline Controller raising the event
        /// </summary>
        public SplineController Sender { get; private set; }
        /// <summary>
        /// The related spline
        /// </summary>
        public CurvySpline Spline { get; private set; }
        /// <summary>
        /// The control point which reaching triggered this event
        /// </summary>
        public CurvySplineSegment ControlPoint { get; private set; }
        /// <summary>
        /// Are <see cref="Delta"/> and <see cref="Position"/> in world units (in opposition to relative units)?
        /// </summary>
        public bool WorldUnits { get; private set; }

        /// <summary>
        /// The movement direction the controller had when sending the event
        /// </summary>
        public MovementDirection MovementDirection { get; private set; }
        /// <summary>
        /// The left distance yet to move.
        /// </summary>
        public float Delta { get; private set; }
        /// <summary>
        /// Controller Position on Spline
        /// </summary>
        public float Position { get; private set; }


        public CurvySplineMoveEventArgs(SplineController sender, CurvySpline spline, CurvySplineSegment controlPoint, float position, bool usingWorldUnits, float delta, MovementDirection direction)
        {
            Set_INTERNAL(sender, spline, controlPoint, position, delta, direction, usingWorldUnits);
        }

        /// <summary>
        /// Set all the properties values. Is not meant to be used by code outside of Curvy's code.
        /// </summary>
        internal void Set_INTERNAL(SplineController sender, CurvySpline spline, CurvySplineSegment controlPoint, float position, float delta, MovementDirection direction, bool usingWorldUnits)
        {

            Sender = sender;
            Spline = spline;
            ControlPoint = controlPoint;

#if CURVY_SANITY_CHECKS
            Assert.IsTrue(Sender != null);
            Assert.IsTrue(controlPoint == null || controlPoint.Spline == spline);
#endif
            MovementDirection = direction;
            Delta = delta;
            Position = position;
            WorldUnits = usingWorldUnits;
            Cancel = false;
        }
    }


    //TODO Use CurvyControllerSwitchEvent
    //public class CurvyControllerSwitchEvent : UnityEventEx<CurvyControllerSwitchEventArgs> { }

    //public class CurvyControllerSwitchEventArgs : EventArgs
    //{
    //    /// <summary>
    //    /// The controller raising the event
    //    /// </summary>
    //    public CurvyController Controller { get; private set; }
    //    public CurvySpline SourceSpline { get; private set; }
    //    public CurvySpline DestinationSpline { get; private set; }
    //    public float TFOnSource { get; private set; }
    //    public float TFOnDestination { get; private set; }
    //    public CurvyControllerDirection DirectionOnSource { get; private set; }
    //    public CurvyControllerDirection DirectionOnDestination { get; private set; }
    //    public float SwitchTimeStart { get; private set; }
    //    public float SwitchDuration { get; private set; }
    //    public float SwitchProgression { get; private set; }


    //    public CurvyControllerSwitchEventArgs()
    //    {
    //    }

    //    public void Set(CurvyController controller, float switchTimeStart, float switchDuration, float switchProgression, CurvySpline sourceSpline, CurvySpline destinationSpline, float tfOnSource, float tfOnDestination, CurvyControllerDirection directionOnSource, CurvyControllerDirection directionOnDestination)
    //    {
    //        SwitchDuration = switchDuration;
    //        SwitchProgression = switchProgression;
    //        Controller = controller;
    //        SourceSpline = sourceSpline;
    //        DestinationSpline = destinationSpline;
    //        TFOnSource = tfOnSource;
    //        TFOnDestination = tfOnDestination;
    //        SwitchTimeStart = switchTimeStart;
    //        DirectionOnSource = directionOnSource;
    //        DirectionOnDestination = directionOnDestination;
    //    }
    //}

    #endregion
}