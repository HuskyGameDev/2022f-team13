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
    /// Pie Shape (2D)
    /// </summary>
    [CurvyShapeInfo("2D/Pie")]
    [RequireComponent(typeof(CurvySpline))]
    [AddComponentMenu("Curvy/Shapes/Pie")]
    public class CSPie : CSCircle
    {

        [Range(0, 1)]
        [SerializeField]
        private float m_Roundness = 1f;
        public float Roundness
        {
            get { return m_Roundness; }
            set
            {
                float v = Mathf.Clamp01(value);
                if (m_Roundness != v)
                {
                    m_Roundness = v;
                    Dirty = true;
                }

            }
        }

        public enum EatModeEnum
        {
            Left,
            Right,
            Center
        }

        [SerializeField]
        [RangeEx(0, nameof(maxEmpty), "Empty", "Number of empty slices")]
        private int m_Empty = 1;
        public int Empty
        {
            get { return m_Empty; }
            set
            {
                int v = Mathf.Clamp(value, 0, maxEmpty);
                if (m_Empty != v)
                {
                    m_Empty = v;
                    Dirty = true;
                }
            }
        }

        private int maxEmpty { get { return Count; } }

        [Label(Tooltip = "Eat Mode")]
        [SerializeField]
        private EatModeEnum m_Eat = EatModeEnum.Right;
        public EatModeEnum Eat
        {
            get { return m_Eat; }
            set
            {
                if (m_Eat != value)
                {
                    m_Eat = value;
                    Dirty = true;
                }
            }
        }



#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            Empty = m_Empty;
            Eat = m_Eat;
            Roundness = m_Roundness;
        }
#endif

        protected override void Reset()
        {
            base.Reset();
            Roundness = 1f;
            Empty = 1;
            Eat = EatModeEnum.Right;
        }

        private Vector3 cpPosition(int i, int empty, float d)
        {
            switch (Eat)
            {
                case EatModeEnum.Left:
                    return new Vector3(Mathf.Sin(d * i) * Radius, Mathf.Cos(d * i) * Radius, 0);
                case EatModeEnum.Right:
                    return new Vector3(Mathf.Sin(d * (i + empty)) * Radius, Mathf.Cos(d * (i + empty)) * Radius, 0);
                default:
                    return new Vector3(Mathf.Sin(d * (i + empty * 0.5f)) * Radius, Mathf.Cos(d * (i + empty * 0.5f)) * Radius, 0);
            }
        }

        protected override void ApplyShape()
        {
            PrepareSpline(CurvyInterpolation.Bezier, CurvyOrientation.Dynamic);
            PrepareControlPoints(Count - Empty + 2);

            float d = 360f * Mathf.Deg2Rad / Count;
            float distPercent = Roundness * 0.39f;

            for (int i = 0; i < Spline.ControlPointCount - 1; i++)
            {
                Spline.ControlPointsList[i].AutoHandles = true;
                Spline.ControlPointsList[i].AutoHandleDistance = distPercent;
                SetPosition(i, cpPosition(i, Empty, d));
            }


            // Center
            SetPosition(Spline.ControlPointCount - 1, Vector3.zero);
            SetBezierHandles(Spline.ControlPointCount - 1, 0);

            // From Center
            Spline.ControlPointsList[0].AutoHandles = false;
            Spline.ControlPointsList[0].HandleIn = Vector3.zero;
            Spline.ControlPointsList[0].SetBezierHandles(distPercent,
                                                     cpPosition(Count - 1, Empty, d) - Spline.ControlPointsList[0].transform.localPosition,
                                                     cpPosition(1, Empty, d) - Spline.ControlPointsList[0].transform.localPosition, false, true);

            // To Center
            Spline.ControlPointsList[Spline.ControlPointCount - 2].AutoHandles = false;
            Spline.ControlPointsList[Spline.ControlPointCount - 2].HandleOut = Vector3.zero;
            Spline.ControlPointsList[Spline.ControlPointCount - 2].SetBezierHandles(distPercent,
                                                     cpPosition(Count - 1 - Empty, Empty, d) - Spline.ControlPointsList[Spline.ControlPointCount - 2].transform.localPosition,
                                                     cpPosition(Count + 1 - Empty, Empty, d) - Spline.ControlPointsList[Spline.ControlPointCount - 2].transform.localPosition, true, false);

        }

    }
}