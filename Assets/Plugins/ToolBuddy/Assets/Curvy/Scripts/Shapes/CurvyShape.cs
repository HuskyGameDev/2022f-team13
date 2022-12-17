// =====================================================================
// Copyright 2013-2022 ToolBuddy
// All rights reserved
// 
// http://www.toolbuddy.net
// =====================================================================

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using FluffyUnderware.DevTools;
using System;
#if CONTRACTS_FULL
using System.Diagnostics.Contracts;
#endif
using FluffyUnderware.Curvy.Utils;
using FluffyUnderware.DevTools.Extensions;


namespace FluffyUnderware.Curvy
{
    /// <summary>
    /// Base class for CurvyShape components
    /// </summary>
    [RequireComponent(typeof(CurvySpline))]
    //[DisallowMultipleComponent]
    [ExecuteAlways]
    [HelpURL(CurvySpline.DOCLINK + "curvyshape")]
    public class CurvyShape : DTVersionedMonoBehaviour
    {
        #region ### Serialized Fields ###

        [SerializeField, Label("Plane")] private CurvyPlane m_Plane = CurvyPlane.XY;

        #endregion

        #region ### Public Properties ###

        /// <summary>
        /// Gets or sets the plane to create the shape in
        /// </summary>
        public CurvyPlane Plane
        {
            get { return m_Plane; }
            set
            {
                Spline.Restricted2DPlane = value;
                if (m_Plane != value)
                {
                    m_Plane = value;
                    Dirty = true;
                }
            }
        }

        /// <summary>
        /// Gets the attached spline
        /// </summary>
        public CurvySpline Spline
        {
            get
            {
                if (!mSpline)
                    mSpline = GetComponent<CurvySpline>();
                return mSpline;
            }
        }

        #endregion

        #region ### Private fields ###

        private static Dictionary<CurvyShapeInfo, Type> mShapeDefs = new Dictionary<CurvyShapeInfo, System.Type>();

        private CurvySpline mSpline;
        [System.NonSerialized]
        public bool Dirty;
#if UNITY_EDITOR
        private bool isEnabled;
#endif
        #endregion

        #region ### Unity Callbacks ###
        /*! \cond UNITY */
        private void Update()
        {
            // Prevent updating while dragging prefab
#if UNITY_EDITOR
            //if (Selection.activeGameObject==gameObject)
#endif
            Refresh();
        }

#if UNITY_EDITOR

        private void OnEnable()
        {
            isEnabled = true;
        }

        private void OnDisable()
        {
            isEnabled = false;
        }

        protected virtual void OnValidate()
        {
            if (isEnabled)
            {
                Plane = m_Plane;
                Dirty = true;
            }
        }
#endif

        protected virtual void Reset()
        {
            Plane = CurvyPlane.XY;
            Dirty = true;
        }
        /*! \endcond */

        #endregion

        #region ### Public Methods ###

        /// <summary>
        /// Remove the CurvyShape component from it's GameObject
        /// </summary>
        public void Delete()
        {
            this.Destroy(true, false);
        }

        /// <summary>
        /// Called to refresh the shape. Please call base.Refresh() or RefreshSpline() after your custom code!
        /// </summary>
        /// <remarks>Warning: Only work with ControlPoints, not with segments</remarks>
        public void Refresh()
        {
            if (Dirty)
            {
                //#if UNITY_EDITOR
                //                if (!Application.isPlaying)
                //                    Undo.RecordObject(Spline, "Apply Shape");
                //#endif
                ApplyShape();
                applyPlane();
                Spline.SetDirtyAll();
                Spline.Refresh();
            }
            Dirty = false;
        }


        /// <summary>
        /// Replace the current script with another shape's script
        /// </summary>
        /// <returns>the new shape script</returns>
        public CurvyShape Replace(string menuName)
        {
            Type shapeType = GetShapeType(menuName);
            if (shapeType != null)
            {
                GameObject go = this.gameObject;

                Delete();
                CurvyShape shape;

#if UNITY_EDITOR
                shape = (CurvyShape)Undo.AddComponent(go, shapeType);
#else
                shape = (CurvyShape)go.AddComponent(shapeType);
#endif

                shape.Dirty = true;
                return shape;
            }
            return null;
        }


        #endregion

        #region ### Protected Methods ###

        /// <summary>
        /// Sets basic spline parameters
        /// </summary>
        protected void PrepareSpline(CurvyInterpolation interpolation, CurvyOrientation orientation = CurvyOrientation.Dynamic, int cachedensity = 50, bool closed = true)
        {
            Spline.Interpolation = interpolation;
            Spline.Orientation = orientation;
            Spline.CacheDensity = cachedensity;
            Spline.Closed = closed;
            Spline.RestrictTo2D = this is CurvyShape2D;
        }

        /// <summary>
        /// Sets a Control Point's position by index
        /// </summary>
        /// <param name="no">Control point index</param>
        /// <param name="position">local position</param>
        protected void SetPosition(int no, Vector3 position)
        {
            Spline.ControlPointsList[no].SetLocalPosition(position);
        }

        /// <summary>
        /// Sets a Control Point's rotation by index
        /// </summary>
        /// <param name="no">Control point index</param>
        /// <param name="rotation">local rotation</param>
        protected void SetRotation(int no, Quaternion rotation)
        {
            Spline.ControlPointsList[no].SetLocalRotation(rotation);
        }

        /// <summary>
        /// Sets a Control Point's Bezier Handles by index
        /// </summary>
        /// <param name="no">Control point index</param>
        /// <param name="distanceFrag">distance in percent</param>
        protected void SetBezierHandles(int no, float distanceFrag)
        {
            SetBezierHandles(no, distanceFrag, distanceFrag);
        }

        /// <summary>
        /// Sets a Control Point's Bezier Handles by index
        /// </summary>
        /// <param name="no">Control point index</param>
        /// <param name="inDistanceFrag">distance in percent for HandleIn</param>
        /// /// <param name="outDistanceFrag">distance in percent for HandleOut</param>
        protected void SetBezierHandles(int no, float inDistanceFrag, float outDistanceFrag)
        {
            CurvySplineSegment curvySplineSegment = Spline.ControlPointsList[no];

            if (no >= 0 && no < Spline.ControlPointCount)
            {
                if (inDistanceFrag == outDistanceFrag)
                {
                    curvySplineSegment.AutoHandles = true;
                    curvySplineSegment.AutoHandleDistance = inDistanceFrag;
                }
                else
                {
                    curvySplineSegment.AutoHandles = false;
                    curvySplineSegment.AutoHandleDistance = (inDistanceFrag + outDistanceFrag) / 2f;
                    SetBezierHandles(inDistanceFrag, true, false, curvySplineSegment);
                    SetBezierHandles(outDistanceFrag, false, true, curvySplineSegment);
                }


            }
        }

        /// <summary>
        /// Sets a Control Point's Bezier Handles position
        /// </summary>
        /// <param name="no">the ControlPoint</param>
        /// <param name="i">HandlInPosition</param>
        /// <param name="o">HandleOutPosition</param>
        /// <param name="space">World or local space</param>
        protected void SetBezierHandles(int no, Vector3 i, Vector3 o, Space space = Space.World)
        {
            if (no >= 0 && no < Spline.ControlPointCount)
            {
                CurvySplineSegment curvySplineSegment = Spline.ControlPointsList[no];

                curvySplineSegment.AutoHandles = false;
                if (space == Space.World)
                {
                    curvySplineSegment.HandleInPosition = i;
                    curvySplineSegment.HandleOutPosition = o;
                }
                else
                {
                    curvySplineSegment.HandleIn = i;
                    curvySplineSegment.HandleOut = o;
                }
            }
        }

        /// <summary>
        /// Automatically place Bezier handles for a set of Control Points
        /// </summary>
        /// <param name="distanceFrag">how much % distance between neighbouring CPs are applied to the handle length?</param>
        /// <param name="setIn">Set HandleIn?</param>
        /// <param name="setOut">Set HandleOut?</param>
        /// <param name="controlPoints">one or more Control Points to set</param>
        public static void SetBezierHandles(float distanceFrag, bool setIn, bool setOut, params CurvySplineSegment[] controlPoints)
        {
            if (controlPoints.Length == 0)
                return;

            foreach (CurvySplineSegment cp in controlPoints)
                cp.SetBezierHandles(distanceFrag, setIn, setOut);
        }

        /// <summary>
        /// Enables CGHardEdge for a set of Control Points
        /// </summary>
        /// <param name="controlPoints">list of Control Point indices</param>
        protected void SetCGHardEdges(params int[] controlPoints)
        {
            if (controlPoints.Length == 0)
            {
                for (int i = 0; i < Spline.ControlPointCount; i++)
                    Spline.ControlPointsList[i].GetMetadata<MetaCGOptions>(true).HardEdge = true;
            }
            else
            {
                for (int i = 0; i < controlPoints.Length; i++)
                    if (i >= 0 && i < Spline.ControlPointCount)
                        Spline.ControlPointsList[i].GetMetadata<MetaCGOptions>(true).HardEdge = true;
            }
        }

        /// <summary>
        /// Override this to add custom code
        /// </summary>
        protected virtual void ApplyShape()
        {
        }

        /// <summary>
        /// Resizes the spline to have a certain number of Control Points
        /// </summary>
        /// <param name="count">number of Control Points</param>
        protected void PrepareControlPoints(int count)
        {
#if CONTRACTS_FULL
            Contract.Requires(Spline.LastVisibleControlPoint != null);
#endif

            /*
            Spline.Clear();
            while (count-- > 0)
                Spline.Add();
            Spline.Refresh();
            */
            int delta = count - Spline.ControlPointCount;
            bool upd = delta != 0;

            while (delta > 0)
            {
                Spline.InsertAfter(null, true);
                delta--;
            }

            while (delta < 0)
            {
                Spline.Delete(Spline.LastVisibleControlPoint, true, false);
                delta++;
            }
            // Revert to default settings
            for (int i = 0; i < Spline.ControlPointsList.Count; i++)
            {
                CurvySplineSegment controlPoint = Spline.ControlPointsList[i];
                controlPoint.transform.localPosition = Vector3.zero;
                controlPoint.transform.localRotation = Quaternion.identity;
                controlPoint.transform.localScale = Vector3.one;
                controlPoint.Reset();
                controlPoint.Disconnect();
                MetaCGOptions mcg = controlPoint.GetMetadata<MetaCGOptions>();
                if (mcg)
                    mcg.Reset();
            }

            if (upd)
                Spline.Refresh();
        }

        #endregion

        #region ### Public Static Methods & Properties ###

        /// <summary>
        /// Dictionary of Shape definitions and their types
        /// </summary>
        //TODO should be private
        public static Dictionary<CurvyShapeInfo, System.Type> ShapeDefinitions
        {
            get
            {
                if (mShapeDefs.Count == 0)
                    mShapeDefs = typeof(CurvyShape).GetAllTypesWithAttribute<CurvyShapeInfo>();

                return mShapeDefs;
            }
        }

        /// <summary>
        /// Gets a list of Menu Names of available shapes
        /// </summary>
        /// <param name="only2D">whether to skip 3D shapes or not</param>
        /// <returns>a list of Menu Names</returns>
        public static List<string> GetShapesMenuNames(bool only2D = false)
        {
            List<string> res = new List<string>();
            foreach (CurvyShapeInfo shapeInfo in ShapeDefinitions.Keys)
                if (!only2D || shapeInfo.Is2D)
                    res.Add(shapeInfo.Name);

            return res;
        }

        /// <summary>
        /// Gets a list of Menu Names of available shapes
        /// </summary>
        /// <param name="currentShapeType">the current shape type</param>
        /// <param name="currentIndex">returns the index of the current shape type</param>
        /// <param name="only2D">whether only to show 2D shapes</param>
        /// <returns>a list of Menu Names</returns>
        public static List<string> GetShapesMenuNames(System.Type currentShapeType, out int currentIndex, bool only2D = false)
        {
            currentIndex = 0;
            if (currentShapeType == null)
                return GetShapesMenuNames(only2D);
            List<string> lst = new List<string>();
            foreach (KeyValuePair<CurvyShapeInfo, Type> kv in ShapeDefinitions)
            {
                if (!only2D || kv.Key.Is2D)
                    lst.Add(kv.Key.Name);
                if (kv.Value == currentShapeType)
                    currentIndex = lst.Count - 1;

            }
            return lst;
        }

        /// <summary>
        /// Gets Shape Menu Name from a CurvyShape subclass type
        /// </summary>
        /// <param name="shapeType"></param>
        /// <returns></returns>
        public static string GetShapeName(System.Type shapeType)
        {
            foreach (KeyValuePair<CurvyShapeInfo, Type> kv in ShapeDefinitions)
                if (kv.Value == shapeType)
                    return kv.Key.Name;
            return null;
        }

        /// <summary>
        /// Gets a CurvyShape subclass type from a Shape's MenuName
        /// </summary>
        /// <param name="menuName"></param>
        /// <returns></returns>
        public static Type GetShapeType(string menuName)
        {
            foreach (CurvyShapeInfo shapeInfo in ShapeDefinitions.Keys)
                if (shapeInfo.Name == menuName)
                    return ShapeDefinitions[shapeInfo];

            return null;
        }

        #endregion

        #region ### Privates ###

        private void applyPlane()
        {
            switch (Plane)
            {
                case CurvyPlane.XZ:
                    applyRotation(Quaternion.Euler(90, 0, 0));
                    break;
                case CurvyPlane.YZ:
                    applyRotation(Quaternion.Euler(0, 90, 0));
                    break;
                default:
                    applyRotation(Quaternion.Euler(0, 0, 0));
                    break;
            }
        }

        private void applyRotation(Quaternion q)
        {
            Spline.transform.localRotation = Quaternion.identity;

            for (int i = 0; i < Spline.ControlPointCount; i++)
            {
                CurvySplineSegment curvySplineSegment = Spline.ControlPointsList[i];
                curvySplineSegment.SetLocalRotation(q * curvySplineSegment.transform.localRotation);
                curvySplineSegment.SetLocalPosition(q * curvySplineSegment.transform.localPosition);
                if (Spline.Interpolation == CurvyInterpolation.Bezier)
                {
                    curvySplineSegment.HandleIn = q * curvySplineSegment.HandleIn;
                    curvySplineSegment.HandleOut = q * curvySplineSegment.HandleOut;
                }
            }
        }

        #endregion


    }

    /// <summary>
    /// CurvyShape Info Attribute
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class CurvyShapeInfo : System.Attribute
    {
        public readonly string Name;
        public readonly bool Is2D;

        public CurvyShapeInfo(string name, bool is2D = true)
        {
            Name = name;
            Is2D = is2D;
        }
    }
}