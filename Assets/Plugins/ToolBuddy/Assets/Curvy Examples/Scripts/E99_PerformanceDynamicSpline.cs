// =====================================================================
// Copyright 2013-2022 ToolBuddy
// All rights reserved
// 
// http://www.toolbuddy.net
// =====================================================================

using UnityEngine;
using System.Collections;
using FluffyUnderware.DevTools;
using FluffyUnderware.Curvy.Generator;

namespace FluffyUnderware.Curvy.Examples
{
    public class E99_PerformanceDynamicSpline : MonoBehaviour
    {

        CurvySpline mSpline;

        public CurvyGenerator Generator;
        [Positive]
        public int UpdateInterval = 200;
        [RangeEx(2, 2000)]
        public int CPCount = 100;
        [Positive]
        public float Radius = 20;

        public bool AlwaysClear;
        public bool UpdateCG;

        float mAngleStep;
        float mCurrentAngle;
        float mLastUpdateTime;
        readonly TimeMeasure ExecTimes = new TimeMeasure(10);

        void Awake()
        {
            mSpline = GetComponent<CurvySpline>();
        }

        // Use this for initialization
        void Start()
        {
            for (int i = 0; i < CPCount; i++)
                addCP();

            mSpline.Refresh();
            mLastUpdateTime = Time.timeSinceLevelLoad + 0.1f;
        }

        // Update is called once per frame
        void Update()
        {
            if (Time.timeSinceLevelLoad - UpdateInterval * 0.001f > mLastUpdateTime)
            {
                mLastUpdateTime = Time.timeSinceLevelLoad;
                ExecTimes.Start();
                if (AlwaysClear)
                    mSpline.Clear();
                // Remove old CP
                while (mSpline.ControlPointCount > CPCount)
                    mSpline.Delete(mSpline.ControlPointsList[0], true);
                // Add new CP(s)
                while (mSpline.ControlPointCount <= CPCount)
                    addCP();
                mSpline.Refresh();
                ExecTimes.Stop();
            }
        }

        void addCP()
        {
            mAngleStep = Mathf.PI * 2 / (CPCount + CPCount * 0.25f);
            Vector3 cpPosition = transform.localToWorldMatrix.MultiplyPoint3x4(
                new Vector3(
                    Mathf.Sin(mCurrentAngle) * Radius,
                    Mathf.Cos(mCurrentAngle) * Radius,
                    0));
            mSpline.InsertAfter(null, cpPosition, true);
            mCurrentAngle = Mathf.Repeat(mCurrentAngle + mAngleStep, Mathf.PI * 2);
        }

        void OnGUI()
        {
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Interval", GUILayout.Width(130));
            UpdateInterval = (int)GUILayout.HorizontalSlider(UpdateInterval, 0, 5000, GUILayout.Width(200));
            GUILayout.Label(UpdateInterval.ToString());
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("# of Control Points", GUILayout.Width(130));
            CPCount = (int)GUILayout.HorizontalSlider(CPCount, 2, 200, GUILayout.Width(200));
            GUILayout.Label(CPCount.ToString());
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Radius", GUILayout.Width(130));
            Radius = GUILayout.HorizontalSlider(Radius, 10, 100, GUILayout.Width(200));
            GUILayout.Label(Radius.ToString("0.00"));
            GUILayout.EndHorizontal();
            AlwaysClear = GUILayout.Toggle(AlwaysClear, "Always clear");
            bool state = UpdateCG;
            UpdateCG = GUILayout.Toggle(UpdateCG, "Use Curvy Generator");
            if (state != UpdateCG)
                Generator.gameObject.SetActive(UpdateCG);
            GUILayout.Label("Avg. Execution Time (ms): " + ExecTimes.AverageMS.ToString("0.000"));
            GUILayout.EndVertical();
        }

    }
}