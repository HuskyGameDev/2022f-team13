// =====================================================================
// Copyright 2013-2022 ToolBuddy
// All rights reserved
// 
// http://www.toolbuddy.net
// =====================================================================

using FluffyUnderware.Curvy.Pools;
using UnityEngine;
using FluffyUnderware.DevTools;
using ToolBuddy.Pooling.Collections;
using UnityEngine.Assertions;

namespace FluffyUnderware.Curvy.Components
{
    /// <summary>
    /// Class to drive a LineRenderer with a CurvySpline
    /// </summary>
    [AddComponentMenu(ComponentPath)]
    [RequireComponent(typeof(LineRenderer))]
    [HelpURL(CurvySpline.DOCLINK + "curvylinerenderer")]
    public class CurvyLineRenderer : SplineProcessor
    {
        public const string ComponentPath = "Curvy/Converters/Curvy Line Renderer";

        private LineRenderer mRenderer;

        protected override void Awake()
        {
            mRenderer = GetComponent<LineRenderer>();
            base.Awake();
        }

        protected override void OnEnable()
        {
            mRenderer = GetComponent<LineRenderer>();
            base.OnEnable();
        }

        private void Update()
        {
            EnforceWorldSpaceUsage();
        }

        private void EnforceWorldSpaceUsage()
        {
            if (mRenderer.useWorldSpace == false)
            {
                mRenderer.useWorldSpace = true;
                DTLog.Log("[Curvy] CurvyLineRenderer: Line Renderer's Use World Space was overriden to true", this);
            }
        }

        /// <summary>
        /// Update the <see cref="LineRenderer"/>'s points with the cache points of the <see cref="CurvySpline"/>
        /// </summary>
        public override void Refresh()
        {
            if (Spline)
            {
                if (Spline.IsInitialized && Spline.Dirty == false)
                {
#if CURVY_SANITY_CHECKS
                    Assert.IsTrue(mRenderer != null);
#endif
                    EnforceWorldSpaceUsage();
                    SubArray<Vector3> positions = Spline.GetPositionsCache(Space.World);
                    mRenderer.positionCount = positions.Count;
                    mRenderer.SetPositions(positions.Array);
                    ArrayPools.Vector3.Free(positions);
                }
                else if (mRenderer != null)
                {
                    EnforceWorldSpaceUsage();
                    mRenderer.positionCount = 0;
                }
            }
        }
    }
}