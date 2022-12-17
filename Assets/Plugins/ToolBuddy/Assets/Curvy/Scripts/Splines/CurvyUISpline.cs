// =====================================================================
// Copyright 2013-2022 ToolBuddy
// All rights reserved
// 
// http://www.toolbuddy.net
// =====================================================================

using UnityEngine;

namespace FluffyUnderware.Curvy
{
    /// <summary>
    /// Spline component that fits perfectly to uGUI Canvas
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    [AddComponentMenu("Curvy/Curvy UI Spline")]
    [HelpURL(CurvySpline.DOCLINK + "curvyuispline")]
    public class CurvyUISpline : CurvySpline
    {
        //DESIGN Isn't there a better way to create UI splines? The current code is set in a way that will not set up the spline properly if you don't go through the CreateUISpline method. This is an issue.
        //Jake explains here that CurvyUISpline just to have a different PPU https://forum.curvyeditor.com/thread-431-post-1672.html#pid1672

        /// <summary>
        /// Creates a GameObject with a CurvyUISpline attached
        /// </summary>
        /// <returns>the component</returns>
        public static CurvyUISpline CreateUISpline(string gameObjectName = "Curvy UI Spline")
        {
            CurvyUISpline spl = new GameObject(gameObjectName, typeof(CurvyUISpline)).GetComponent<CurvyUISpline>();
            spl.SetupUISpline();
            return spl;
        }

        override protected void Reset()
        {
            base.Reset();
            SetupUISpline();
        }

        private void SetupUISpline()
        {
            RestrictTo2D = true;
            MaxPointsPerUnit = 1;
            Orientation = CurvyOrientation.None;
        }
    }
}