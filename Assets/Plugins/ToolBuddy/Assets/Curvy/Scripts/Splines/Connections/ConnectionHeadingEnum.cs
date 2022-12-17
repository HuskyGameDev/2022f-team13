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
    /// Connection's Follow-Up heading direction
    /// </summary>
    public enum ConnectionHeadingEnum
    {
        /// <summary>
        /// Head towards the targets start (negative F)
        /// </summary>
        Minus = -1,
        /// <summary>
        /// Do not head anywhere, stay still
        /// </summary>
        Sharp = 0,
        /// <summary>
        /// Head towards the targets end (positive F)
        /// </summary>
        Plus = 1,
        /// <summary>
        /// Automatically choose the appropriate value
        /// </summary>
        Auto = 2
    }

    /// <summary>
    /// Extension methods of <see cref="ConnectionHeadingEnum"/>
    /// </summary>
    public static class ConnectionHeadingEnumMethods
    {
        /// <summary>
        /// If heading is Auto, this method will translate it to a Plus, Minus or Sharp value depending on the Follow-Up control point.
        /// </summary>
        /// <param name="heading">the value to resolve</param>
        /// <param name="followUp">the related followUp control point</param>
        /// <returns></returns>
        static public ConnectionHeadingEnum ResolveAuto(this ConnectionHeadingEnum heading, CurvySplineSegment followUp)
        {
            if (heading == ConnectionHeadingEnum.Auto)
            {
                if (CurvySplineSegment.CanFollowUpHeadToEnd(followUp))
                    heading = ConnectionHeadingEnum.Plus;
                else if (CurvySplineSegment.CanFollowUpHeadToStart(followUp))
                    heading = ConnectionHeadingEnum.Minus;
                else
                    heading = ConnectionHeadingEnum.Sharp;
            }
            return heading;
        }
    }
}