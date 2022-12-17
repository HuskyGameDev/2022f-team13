// =====================================================================
// Copyright 2013-2022 ToolBuddy
// All rights reserved
// 
// http://www.toolbuddy.net
// =====================================================================

using UnityEngine;
using System.Collections;
using FluffyUnderware.Curvy;
using FluffyUnderware.DevTools;

namespace FluffyUnderware.Curvy.Shapes
{
    /// <summary>
    /// Rectangle Shape (2D)
    /// </summary>
    [CurvyShapeInfo("2D/Rectangle")]
    [RequireComponent(typeof(CurvySpline))]
    [AddComponentMenu("Curvy/Shapes/Rectangle")]
    public class CSRectangle : CurvyShape2D
    {
        [Positive]
        [SerializeField]
        private float m_Width = 1;
        public float Width
        {
            get { return m_Width; }
            set 
            {
                float v = Mathf.Max(0, value);
                if (m_Width != v)
                {
                    m_Width = v;
                    Dirty = true;
                }
            }
        }
        [Positive]
        [SerializeField]
        private float m_Height = 1;
        public float Height
        {
            get { return m_Height; }
            set
            {
                float v = Mathf.Max(0, value);
                if (m_Height != v)
                {
                    m_Height = v;
                    Dirty = true;
                }
            }
        }

        protected override void Reset()
        {
            base.Reset();
            Width = 1;
            Height = 1;
        }

        protected override void ApplyShape()
        {
            base.ApplyShape();
            PrepareSpline(CurvyInterpolation.Linear, CurvyOrientation.Dynamic,1, true);
            PrepareControlPoints(4);
            float hw = Width / 2;
            float hh = Height / 2;
            SetCGHardEdges();
            
            SetPosition(0,new Vector3(-hw, -hh));
            SetPosition(1,new Vector3(-hw, hh));
            SetPosition(2,new Vector3(hw, hh));
            SetPosition(3,new Vector3(hw, -hh));
        }


#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            Width = m_Width;
            Height = m_Height;
        }
#endif

      
      
    }
}