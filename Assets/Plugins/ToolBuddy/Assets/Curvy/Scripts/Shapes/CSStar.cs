// =====================================================================
// Copyright 2013-2022 ToolBuddy
// All rights reserved
// 
// http://www.toolbuddy.net
// =====================================================================

using UnityEngine;
using System.Collections;
using FluffyUnderware.DevTools;

namespace FluffyUnderware.Curvy.Shapes
{
    /// <summary>
    /// Star Shape (2D)
    /// </summary>
    [CurvyShapeInfo("2D/Star")]
    [RequireComponent(typeof(CurvySpline))]
    [AddComponentMenu("Curvy/Shapes/Star")]
    public class CSStar : CurvyShape2D
    {
        
        [SerializeField]
        [Positive(Tooltip = "Number of Sides", MinValue = 2)]
        private int m_Sides = 5;
        public int Sides
        {
            get { return m_Sides; }
            set
            {
                int v = Mathf.Max(0, value);
                if (m_Sides != v)
                {
                    m_Sides = v;
                    Dirty = true;
                }
            }
        }

        
        [SerializeField]
        [Positive]
        private float m_OuterRadius = 2;
        public float OuterRadius
        {
            get { return m_OuterRadius; }
            set
            {
                float v = Mathf.Max(InnerRadius, value);
                if (m_OuterRadius != v)
                {
                    m_OuterRadius = v;
                    Dirty = true;
                }
                
            }
        }

        
        [SerializeField]
        [RangeEx(0, 1)]
        private float m_OuterRoundness = 0;
        public float OuterRoundness
        {
            get { return m_OuterRoundness; }
            set
            {
                float v = Mathf.Max(0, value);
                if (m_OuterRoundness != v)
                {
                    m_OuterRoundness = v;
                    Dirty = true;
                }
            }
        }
        
        
        [SerializeField]
        [Positive]
        private float m_InnerRadius = 1;
        public float InnerRadius
        {
            get { return m_InnerRadius; }
            set
            {
                float v = Mathf.Max(0, value);
                if (m_InnerRadius != v)
                {
                    m_InnerRadius = v;
                    Dirty = true;
                }
            }
        }

        [SerializeField]
        [RangeEx(0, 1)]
        private float m_InnerRoundness = 0;
        public float InnerRoundness
        {
            get { return m_InnerRoundness; }
            set
            {
                float v = Mathf.Max(0, value);
                if (m_InnerRoundness != v)
                {
                    m_InnerRoundness = v;
                    Dirty = true;
                }
            }
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            Sides=m_Sides;
            OuterRadius = m_OuterRadius;
            InnerRadius=m_InnerRadius;
            OuterRoundness = m_OuterRoundness;
            InnerRoundness = m_InnerRoundness;
        }
#endif

        protected override void Reset()
        {
 	         base.Reset();
            Sides=5;
            OuterRadius=2;
            OuterRoundness=0;
            InnerRadius=1;
            InnerRoundness=0;
        }

        protected override void ApplyShape()
        {
            PrepareSpline(CurvyInterpolation.Bezier);
            PrepareControlPoints(Sides*2);
            float d = 360f * Mathf.Deg2Rad / Spline.ControlPointCount;
            for (int i = 0; i < Spline.ControlPointCount; i += 2)
            {
                Vector3 dir = new Vector3(Mathf.Sin(d * i), Mathf.Cos(d * i), 0);

                SetPosition(i, dir * OuterRadius);
                //SetBezierHandles(i,new Vector3(-dir.y, dir.x, 0),new Vector3(dir.y, -dir.x, 0),Space.Self);
                Spline.ControlPointsList[i].AutoHandleDistance = OuterRoundness;
                dir=new Vector3(Mathf.Sin(d*(i+1)),Mathf.Cos(d*(i+1)),0);
                SetPosition(i+1,dir * InnerRadius);
                //SetBezierHandles(i+1,new Vector3(-dir.y, dir.x, 0),new Vector3(dir.y, -dir.x, 0),Space.Self);
                Spline.ControlPointsList[i + 1].AutoHandleDistance = InnerRoundness;
            }
        }

    }
}