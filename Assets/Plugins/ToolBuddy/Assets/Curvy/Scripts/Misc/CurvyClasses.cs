// =====================================================================
// Copyright 2013-2022 ToolBuddy
// All rights reserved
// 
// http://www.toolbuddy.net
// =====================================================================

using System;

namespace FluffyUnderware.Curvy
{
    /// <summary>
    /// Orientation options
    /// </summary>
    public enum OrientationModeEnum
    {
        /// <summary>
        /// No Orientation. The initial orientation of the controller is kept.
        /// </summary>
        None,
        /// <summary>
        /// Use Orientation/Up-Vector
        /// </summary>
        Orientation,
        /// <summary>
        /// Use Direction/Tangent
        /// </summary>
        Tangent
    }

    /// <summary>
    /// Orientation axis to use
    /// </summary>
    public enum OrientationAxisEnum
    {
        Up,
        Down,
        Forward,
        Backward,
        Left,
        Right
    }

    /// <summary>
    /// Used by components to determine when updates should occur
    /// </summary>
    public enum CurvyUpdateMethod
    {
        Update,
        LateUpdate,
        FixedUpdate
    }

    public enum CurvyRepeatingOrderEnum
    {
        Random = 0,
        Row = 1
    }

    /// <summary>
    /// Plane definition
    /// </summary>
    public enum CurvyPlane
    {
        /// <summary>
        /// X/Y Plane (Z==0)
        /// </summary>
        XY,
        /// <summary>
        /// X/U Plane (Y==0)
        /// </summary>
        XZ,
        /// <summary>
        /// Y/Z Plane (X==)
        /// </summary>
        YZ
    }

    /// <summary>
    /// Position Mode 
    /// </summary>
    public enum CurvyPositionMode
    {
        /// <summary>
        /// Valid positions are from 0 (Start) to 1 (End)
        /// </summary>
        Relative = 0,
        /// <summary>
        /// Valid positions are from 0 (Start) to Length (End). Also know as Absolute.
        /// </summary>
        WorldUnits = 1,
    }

    /// <summary>
    /// Bezier Handles editing modes
    /// </summary>
    [Flags]
    public enum CurvyBezierModeEnum
    {
        /// <summary>
        /// Don't sync
        /// </summary>
        None = 0,
        /// <summary>
        /// Sync Direction
        /// </summary>
        Direction = 1,
        /// <summary>
        /// Sync Length
        /// </summary>
        Length = 2,
        /// <summary>
        /// Sync connected Control Points
        /// </summary>
        Connections = 4,
        /// <summary>
        /// Combine both Handles of a segment
        /// </summary>
        Combine = 8
    }

    /// <summary>
    /// Bezier Handles editing modes for AdvSplines
    /// </summary>
    public enum CurvyAdvBezierModeEnum
    {
        /// <summary>
        /// Don't sync
        /// </summary>
        None = 0,
        /// <summary>
        /// Sync Direction
        /// </summary>
        Direction = 1,
        /// <summary>
        /// Sync Length
        /// </summary>
        Length = 2,
        /// <summary>
        /// Combine both Handles of a segment
        /// </summary>
        Combine = 8
    }

    /// <summary>
    /// Determines the interpolation method
    /// </summary>
    public enum CurvyInterpolation
    {
        /// <summary>
        ///  Linear interpolation
        /// </summary>
        Linear = 0,
        /// <summary>
        /// Catmul-Rom splines
        /// </summary>
        CatmullRom = 1,
        /// <summary>
        /// Kochanek-Bartels (TCB)-Splines
        /// </summary>
        TCB = 2,
        /// <summary>
        /// Cubic Bezier-Splines
        /// </summary>
        Bezier = 3,
        /// <summary>
        /// B-Splines
        /// </summary>
        BSpline = 4
    }

    /// <summary>
    /// Determines the clamping method used by Move-methods
    /// </summary>
    public enum CurvyClamping
    {
        /// <summary>
        /// Stop at splines ends
        /// </summary>
        Clamp = 0,
        /// <summary>
        /// Start over
        /// </summary>
        Loop = 1,
        /// <summary>
        /// Switch direction
        /// </summary>
        PingPong = 2
    }

    /// <summary>
    /// Determines Orientation mode
    /// </summary>
    public enum CurvyOrientation
    {
        /// <summary>
        /// Ignore rotation
        /// </summary>
        None = 0,
        /// <summary>
        /// Use the splines' tangent and up vectors to create a look rotation 
        /// </summary>
        Dynamic = 1,
        /// <summary>
        /// Interpolate between the Control Point's rotation
        /// </summary>
        Static = 2,
    }

    /// <summary>
    /// Swirl mode
    /// </summary>
    public enum CurvyOrientationSwirl
    {
        /// <summary>
        /// No Swirl
        /// </summary>
        None = 0,
        /// <summary>
        /// Swirl over each segment of anchor group
        /// </summary>
        Segment = 1,
        /// <summary>
        /// Swirl equal over current anchor group's segments
        /// </summary>
        AnchorGroup = 2,
        /// <summary>
        /// Swirl equal over anchor group's length
        /// </summary>
        AnchorGroupAbs = 3
    }



    /// <summary>
    /// Sceneview viewing modes
    /// </summary>
    [Flags]
    public enum CurvySplineGizmos : int
    {
        None = 0,
        Connections = 1,
        Curve = 1 << 1,
        Approximation = 1 << 2,
        Tangents = 1 << 3,
        Orientation = 1 << 4,
        Labels = 1 << 5,
        Metadata = 1 << 6,
        Bounds = 1 << 7,
        TFs = 1 << 8,
        RelativeDistances = 1 << 9,
        OrientationAnchors = 1 << 10,
        All = 65535
    }
}