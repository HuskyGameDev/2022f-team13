// =====================================================================
// Copyright 2013-2022 ToolBuddy
// All rights reserved
// 
// http://www.toolbuddy.net
// =====================================================================

using UnityEngine;
using System.Collections;
using FluffyUnderware.Curvy;
using FluffyUnderware.DevTools.Extensions;
using FluffyUnderware.Curvy.Controllers;
using UnityEngine.UI;

/* 
 * In this example we let the user draw a spline on screen!
 * 
 */
namespace FluffyUnderware.Curvy.Examples
{
    public class E04_PaintSpline : MonoBehaviour
    {
        public float StepDistance = 30;
        public SplineController Controller;
        public Text InfoText;

        CurvySpline mSpline;
        Vector2 mLastControlPointPos;
        bool mResetSpline = true;

        void Awake()
        {
            // for this example we assume the component is attached to a GameObject holding a spline
            mSpline = GetComponent<CurvySpline>();
        }

        void OnGUI()
        {
            // before using the spline, ensure it's initialized and the Controller is available
            if (mSpline == null || !mSpline.IsInitialized || !Controller)
                return;

            Event e = Event.current;
            switch (e.type)
            {
                case EventType.MouseDrag:
                    // Start a new line?
                    if (mResetSpline)
                    {
                        mSpline.Clear(); // delete all Control Points
                        addCP(e.mousePosition); // add the first Control Point
                        Controller.gameObject.SetActive(true);
                        Controller.AbsolutePosition = 0;
                        mLastControlPointPos = e.mousePosition; // Store current mouse position
                        mResetSpline = false;
                    }
                    else
                    {
                        // only create a new Control Point if the minimum distance is reached
                        float dist = (e.mousePosition - mLastControlPointPos).magnitude;
                        if (dist >= StepDistance)
                        {
                            mLastControlPointPos = e.mousePosition;
                            addCP(e.mousePosition);
                            if (Controller.PlayState != CurvyController.CurvyControllerState.Playing)
                                Controller.Play();
                        }
                    }

                    break;
                case EventType.MouseUp:
                    mResetSpline = true;

                    break;
            }
        }

        // Add a Control Point and set it's position
        CurvySplineSegment addCP(Vector3 mousePos)
        {
            Vector3 p = Camera.main.ScreenToWorldPoint(mousePos);
            p.y *= -1; // flip Y to get the correct world position
            p.z += 100; //To move further than camera's plane. The value 100 comes from the Canvas' plane distance
            CurvySplineSegment cp = mSpline.InsertAfter(null, p, false);

            InfoText.text = "Control Points: " + mSpline.ControlPointCount.ToString(); // set info text

            return cp;
        }
    }
}