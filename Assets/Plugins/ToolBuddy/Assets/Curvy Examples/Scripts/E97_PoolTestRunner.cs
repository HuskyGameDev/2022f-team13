// =====================================================================
// Copyright 2013-2022 ToolBuddy
// All rights reserved
// 
// http://www.toolbuddy.net
// =====================================================================

using UnityEngine;
using FluffyUnderware.Curvy.Components;
using FluffyUnderware.DevTools.Extensions;
using UnityEngine.UI;

namespace FluffyUnderware.Curvy.Examples
{
    public class E97_PoolTestRunner : MonoBehaviour
    {
        public CurvySpline Spline;
        public Text PoolCountInfo;

        void Start()
        {
            checkForSpline();
        }

        void Update()
        {


            PoolCountInfo.text = string.Format("Control Points in Pool: {0}", CurvyGlobalManager.Instance.ControlPointPool.Count);
        }

        void checkForSpline()
        {
            if (Spline == null)
            {
                Spline = CurvySpline.Create();
                Camera.main.GetComponent<CurvyGLRenderer>().Add(Spline);
                for (int i = 0; i < 4; i++)
                    AddCP();
            }
        }

        public void AddCP()
        {
            checkForSpline();
            Spline.Add(Random.insideUnitCircle * 50);
            Spline.Refresh();
        }

        public void DeleteCP()
        {
            if (Spline && Spline.ControlPointCount > 0)
            {
                int idx = Random.Range(0, Spline.ControlPointCount - 1);
                Spline.Delete(Spline.ControlPointsList[idx], false);
            }
        }

        public void ClearSpline()
        {
            if (Spline)
                Spline.Clear();
        }

        public void DeleteSpline()
        {
            if (Spline)
                Spline.gameObject.Destroy(false, true);
        }
    }
}