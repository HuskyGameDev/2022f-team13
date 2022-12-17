// =====================================================================
// Copyright 2013-2022 ToolBuddy
// All rights reserved
// 
// http://www.toolbuddy.net
// =====================================================================

using System;
using FluffyUnderware.Curvy.Pools;
using UnityEngine;
using ToolBuddy.Pooling.Collections;
using UnityEngine.Assertions;

namespace FluffyUnderware.Curvy
{
    /// <summary>
    /// Converts a <see cref="CurvySpline"/> to an <see cref="EdgeCollider2D"/> 
    /// </summary>
    [AddComponentMenu(ComponentPath)]
    [RequireComponent(typeof(EdgeCollider2D))]
    [HelpURL(CurvySpline.DOCLINK + "edgecollider2d")]
    public class CurvySplineToEdgeCollider2D : SplineProcessor
    {
        public const string ComponentPath = "Curvy/Converters/Curvy Spline To Edge Collider 2D";

        private EdgeCollider2D edgeCollider2D;

        protected override void Awake()
        {
            edgeCollider2D = GetComponent<EdgeCollider2D>();
            base.Awake();
        }

        protected override void OnEnable()
        {
            edgeCollider2D = GetComponent<EdgeCollider2D>();
            base.OnEnable();
        }

        /// <summary>
        /// Update the <see cref="EdgeCollider2D.points"/> with the cache points of the <see cref="CurvySpline"/>
        /// </summary>
        public override void Refresh()
        {
            if (Spline)
            {
                if (Spline.IsInitialized && Spline.Dirty == false)
                {
#if CURVY_SANITY_CHECKS
                    Assert.IsTrue(edgeCollider2D != null);
#endif
                    SubArray<Vector3> positions = Spline.GetPositionsCache(Space.Self);
                    SubArray<Vector2> positions2D = ArrayPools.Vector2.AllocateExactSize(positions.Count);
                    Vector3[] positionsArray = positions.Array;
                    Vector2[] positions2DArray = positions2D.Array;
                    for (var i = 0; i < positions.Count; i++)
                    {
                        positions2DArray[i].x = positionsArray[i].x;
                        positions2DArray[i].y = positionsArray[i].y;
                    }
                    edgeCollider2D.points = positions2DArray;
                    ArrayPools.Vector2.Free(positions2D);
                    ArrayPools.Vector3.Free(positions);
                }
                else if (edgeCollider2D != null)
                {
                    edgeCollider2D.points = Array.Empty<Vector2>();
                }
            }
        }
    }
}