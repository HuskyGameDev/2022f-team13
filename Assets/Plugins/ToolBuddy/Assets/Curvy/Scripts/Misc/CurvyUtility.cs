// =====================================================================
// Copyright 2013-2022 ToolBuddy
// All rights reserved
// 
// http://www.toolbuddy.net
// =====================================================================

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using FluffyUnderware.Curvy.Generator;
using FluffyUnderware.Curvy.Pools;
using ToolBuddy.Pooling.Pools;
using FluffyUnderware.Curvy.ThirdParty.LibTessDotNet;
#if UNITY_EDITOR
using UnityEditor;
#endif
using FluffyUnderware.DevTools;
using ToolBuddy.Pooling;
using ToolBuddy.Pooling.Collections;


namespace FluffyUnderware.Curvy.Utils
{

    /// <summary>
    /// A workaround to the Unity Json's class not being able to serialize top level arrays.
    /// Including such arrays in another object avoids the issue.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SerializableArray<T>
    {
        public T[] Array;
    }

    /// <summary>
    /// Taken from my asset Frame Rate Booster
    /// https://assetstore.unity.com/packages/tools/utilities/frame-rate-booster-120660
    /// </summary>
    public static class OptimizedOperators
    {
        public static Vector3 Addition(this Vector3 a, Vector3 b)
        {
            a.x += b.x;
            a.y += b.y;
            a.z += b.z;
            return a;
        }

        public static Vector3 UnaryNegation(this Vector3 a)
        {
            Vector3 result;
            result.x = -a.x;
            result.y = -a.y;
            result.z = -a.z;
            return result;
        }

        public static Vector3 Subtraction(this Vector3 a, Vector3 b)
        {
            a.x -= b.x;
            a.y -= b.y;
            a.z -= b.z;
            return a;

        }

        public static Vector3 Multiply(this Vector3 a, float d)
        {
            a.x *= d;
            a.y *= d;
            a.z *= d;
            return a;
        }

        public static Vector3 Multiply(this float d, Vector3 a)
        {
            a.x *= d;
            a.y *= d;
            a.z *= d;
            return a;
        }

        public static Vector3 Division(this Vector3 a, float d)
        {
            float inversed = 1 / d;
            a.x *= inversed;
            a.y *= inversed;
            a.z *= inversed;
            return a;
        }

        public static Vector3 Normalize(this Vector3 value)
        {
            Vector3 result;
            float num = (float)Math.Sqrt(value.x * (double)value.x + value.y * (double)value.y + value.z * (double)value.z);
            if (num > 9.99999974737875E-06)
            {
                float inversed = 1 / num;
                result.x = value.x * inversed;
                result.y = value.y * inversed;
                result.z = value.z * inversed;
            }
            else
            {
                result.x = 0;
                result.y = 0;
                result.z = 0;
            }
            return result;
        }

        public static Vector3 LerpUnclamped(this Vector3 a, Vector3 b, float t)
        {
            a.x += (b.x - a.x) * t;
            a.y += (b.y - a.y) * t;
            a.z += (b.z - a.z) * t;
            return a;
        }

        static public Color Multiply(this Color a, float b)
        {
            a.r *= b;
            a.g *= b;
            a.b *= b;
            a.a *= b;
            return a;
        }

        static public Color Multiply(this float b, Color a)
        {
            a.r *= b;
            a.g *= b;
            a.b *= b;
            a.a *= b;
            return a;
        }

        public static Quaternion Multiply(this Quaternion lhs, Quaternion rhs)
        {
            Quaternion result;
            result.x = (lhs.w * rhs.x + lhs.x * rhs.w + lhs.y * rhs.z - lhs.z * rhs.y);
            result.y = (lhs.w * rhs.y + lhs.y * rhs.w + lhs.z * rhs.x - lhs.x * rhs.z);
            result.z = (lhs.w * rhs.z + lhs.z * rhs.w + lhs.x * rhs.y - lhs.y * rhs.x);
            result.w = (lhs.w * rhs.w - lhs.x * rhs.x - lhs.y * rhs.y - lhs.z * rhs.z);
            return result;
        }
    }


    /// <summary>
    /// Curvy Utility class
    /// </summary>
    public static class CurvyUtility
    {
        #region ### Clamping Methods ###

        /// <summary>
        /// Clamps relative position
        /// </summary>
        public static float ClampTF(float tf, CurvyClamping clamping)
        {
            switch (clamping)
            {
                case CurvyClamping.Loop:
                    return Mathf.Repeat(tf, 1);
                case CurvyClamping.PingPong:
                    return Mathf.PingPong(tf, 1);
                case CurvyClamping.Clamp:
                    return Mathf.Clamp01(tf);
                default:
                    throw new InvalidEnumArgumentException();
            }
        }


        /// <summary>
        /// Clamps relative position and sets new direction
        /// </summary>
        public static float ClampTF(float tf, ref int dir, CurvyClamping clamping)
        {
            switch (clamping)
            {
                case CurvyClamping.Loop:
                    return Mathf.Repeat(tf, 1);
                case CurvyClamping.PingPong:
                    if (Mathf.FloorToInt(tf) % 2 != 0)
                        dir *= -1;
                    return Mathf.PingPong(tf, 1);
                case CurvyClamping.Clamp:
                    return Mathf.Clamp01(tf);
                default:
                    throw new InvalidEnumArgumentException();
            }
        }

        /// <summary>
        /// Clamps a float to a range
        /// </summary>
        public static float ClampValue(float tf, CurvyClamping clamping, float minTF, float maxTF)
        {

            switch (clamping)
            {
                case CurvyClamping.Loop:
                    float v1 = DTMath.MapValue(0, 1, tf, minTF, maxTF);
                    return DTMath.MapValue(minTF, maxTF, Mathf.Repeat(v1, 1), 0, 1);
                case CurvyClamping.PingPong:
                    float v2 = DTMath.MapValue(0, 1, tf, minTF, maxTF);
                    return DTMath.MapValue(minTF, maxTF, Mathf.PingPong(v2, 1), 0, 1);
                case CurvyClamping.Clamp:
                    return Mathf.Clamp(tf, minTF, maxTF);
                default:
                    throw new InvalidEnumArgumentException();
            }
        }

        /// <summary>
        /// Clamps absolute position
        /// </summary>
        public static float ClampDistance(float distance, CurvyClamping clamping, float length)
        {
            if (length == 0)
                return 0;
            switch (clamping)
            {
                case CurvyClamping.Loop:
                    return Mathf.Repeat(distance, length);
                case CurvyClamping.PingPong:
                    return Mathf.PingPong(distance, length);
                case CurvyClamping.Clamp:
                    return Mathf.Clamp(distance, 0, length);
                default:
                    throw new InvalidEnumArgumentException();
            }
        }

        /// <summary>
        /// Clamps absolute position
        /// </summary>
        public static float ClampDistance(float distance, CurvyClamping clamping, float length, float min, float max)
        {
            if (length == 0)
                return 0;
            min = Mathf.Clamp(min, 0, length);
            max = Mathf.Clamp(max, min, length);
            switch (clamping)
            {
                case CurvyClamping.Loop:
                    return min + Mathf.Repeat(distance, max - min);
                case CurvyClamping.PingPong:
                    return min + Mathf.PingPong(distance, max - min);
                case CurvyClamping.Clamp:
                    return Mathf.Clamp(distance, min, max);
                default:
                    throw new InvalidEnumArgumentException();
            }
        }

        /// <summary>
        /// Clamps absolute position and sets new direction
        /// </summary>
        public static float ClampDistance(float distance, ref int dir, CurvyClamping clamping, float length)
        {
            if (length == 0)
                return 0;
            switch (clamping)
            {
                case CurvyClamping.Loop:
                    return Mathf.Repeat(distance, length);
                case CurvyClamping.PingPong:
                    if (Mathf.FloorToInt(distance / length) % 2 != 0)
                        dir *= -1;
                    return Mathf.PingPong(distance, length);
                case CurvyClamping.Clamp:
                    return Mathf.Clamp(distance, 0, length);
                default:
                    throw new InvalidEnumArgumentException();
            }
        }

        /// <summary>
        /// Clamps absolute position and sets new direction
        /// </summary>
        public static float ClampDistance(float distance, ref int dir, CurvyClamping clamping, float length, float min, float max)
        {
            if (length == 0)
                return 0;
            min = Mathf.Clamp(min, 0, length);
            max = Mathf.Clamp(max, min, length);
            switch (clamping)
            {
                case CurvyClamping.Loop:
                    return min + Mathf.Repeat(distance, max - min);
                case CurvyClamping.PingPong:
                    if (Mathf.FloorToInt(distance / (max - min)) % 2 != 0)
                        dir *= -1;
                    return min + Mathf.PingPong(distance, max - min);
                case CurvyClamping.Clamp:
                    return Mathf.Clamp(distance, min, max);
                default:
                    throw new InvalidEnumArgumentException();
            }
        }

        #endregion

        /// <summary>
        /// Gets the default material, i.e. Curvy/Resources/CurvyDefaultMaterial
        /// </summary>
        public static Material GetDefaultMaterial()
        {
            Material mat = Resources.Load("CurvyDefaultMaterial") as Material;
            if (mat == null)
            {
                Shader shader = Shader.Find("Standard");
                if (shader != null) //this can happen, for example in an HDRP build
                    mat = new Material(shader);
            }

            if (mat == null)
                DTLog.LogWarning("[Curvy] Couldn't find Curvy's default material. Please raise a bug report.");

            return mat;
        }


        /// <summary>
        /// Does the same things as Mathf.Approximately, but with different handling of case where one of the two values is 0
        /// Considering inputs of 0 and 1E-7, Mathf.Approximately will return false, while this method will return true.
        /// </summary>
        public static bool Approximately(this float x, float y)
        {
            bool result;
            const float zeroComparisionMargin = 0.000009f;

            float nearlyZero = Mathf.Epsilon * 8f;

            float absX = Math.Abs(x);
            float absY = Math.Abs(y);

            if (absY < nearlyZero)
                result = absX < zeroComparisionMargin;
            else if (absX < nearlyZero)
                result = absY < zeroComparisionMargin;
            else
                result = Mathf.Approximately(x, y);
            return result;
        }

        /// <summary>
        /// Finds the index of x in an array of sorted values (ascendant order). If x not found, the closest smaller value's index is returned if any, -1 otherwise
        /// </summary>
        ///  <param name="array">The array to search into</param>
        ///  <param name="x">The element to search for</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int InterpolationSearch(float[] array, float x)
        {
            return InterpolationSearch(array, array.Length, x);
        }

        ///  <summary>
        ///  Finds the index of x in an array of sorted values (ascendant order). If x not found, the closest smaller value's index is returned if any, -1 otherwise
        ///  </summary>
        ///  <param name="array">The array to search into</param>
        ///  <param name="elementsCount">The number of elements of the array to search into</param>
        ///  <param name="x">The element to search for</param>
        public static int InterpolationSearch(float[] array, int elementsCount, float x)
        {
            int low = 0, high = (elementsCount - 1);

            while (low <= high && array[low] <= x && x <= array[high])
            {
                if (low == high)
                {
                    if (array[low] == x)
                        return low;
                    break;
                }
                int index = low + (int)((((high - low) / (array[high] - array[low])) * (x - array[low])));
                if (array[index] == x)
                    return index;
                if (array[index] < x)
                    low = index + 1;
                else
                    high = index - 1;
            }

            if (low > high)
            {
                (low, high) = (high, low);
            }

            if (x <= array[low])
            {
                while (low >= 0)
                {
                    if (array[low] <= x)
                        return low;
                    low--;
                }

                return 0;
            }

            if (array[high] < x)
            {
                while (high < elementsCount)
                {
                    if (x < array[high])
                        return high - 1;
                    high++;
                }

                return elementsCount - 1;
            }

            return -1;
        }

        /// <summary>
        /// Returns a mesh which boundaries are the input spline, similarly to what the Spline To Mesh window does, but simpler and less configurable.
        /// </summary>
        public static Mesh SplineToMesh(this CurvySpline spline)
        {
            Mesh result;

            Spline2Mesh splineToMesh = new Spline2Mesh();
            splineToMesh.Lines.Add(new SplinePolyLine(spline));
            splineToMesh.Apply(out result);

            if (String.IsNullOrEmpty(splineToMesh.Error) == false)
                Debug.Log(splineToMesh.Error);

            return result;
        }


        /// <summary>
        /// Given an input point, gets the index of the point in the array that is closest to the input point.
        /// </summary>
        /// <param name="point">the input point</param>
        /// <param name="points">A list of points to test against</param>
        /// <param name="pointsCount">The number of points to test against</param>
        /// <param name="index">the index of the closest point</param>
        /// <param name="fragement">a value between 0 and 1 indicating how close the input point is close to the point of index: index + 1</param>
        public static void GetNearestPointIndex(Vector3 point, Vector3[] points, int pointsCount, out int index, out float fragement)
        {
            float nearestSquaredDistance = float.MaxValue;
            int nearestIndex = 0;
            // get the nearest index
            for (int i = 0; i < pointsCount; i++)
            {
                Vector3 delta;
                delta.x = points[i].x - point.x;
                delta.y = points[i].y - point.y;
                delta.z = points[i].z - point.z;
                float squaredDistance = (delta.x * delta.x + delta.y * delta.y + delta.z * delta.z);
                if (squaredDistance <= nearestSquaredDistance)
                {
                    nearestSquaredDistance = squaredDistance;
                    nearestIndex = i;
                }
            }

            // collide p against the lines build by the index
            int leftIdx = (nearestIndex > 0) ? nearestIndex - 1 : -1;
            int rightIdx = (nearestIndex < pointsCount - 1) ? nearestIndex + 1 : -1;

            float leftFrag = 0;
            float rightFrag = 0;
            float leftSquaredDistance = float.MaxValue;
            float rightSquareDistance = float.MaxValue;
            {
                if (leftIdx > -1)
                    leftSquaredDistance = DTMath.LinePointDistanceSqr(points[leftIdx], points[nearestIndex], point, out leftFrag);
                if (rightIdx > -1)
                    rightSquareDistance = DTMath.LinePointDistanceSqr(points[nearestIndex], points[rightIdx], point, out rightFrag);
            }

            if (leftSquaredDistance < rightSquareDistance)
            {
                fragement = leftFrag;
                index = leftIdx;
            }
            else
            {
                fragement = rightFrag;
                index = nearestIndex;
            }
        }
    }

    #region ### Spline2Mesh ###

    /// <summary>
    /// Class to create a Mesh from a set of splines
    /// </summary>
    public class Spline2Mesh
    {
        #region ### Public Fields & Properties ###
        /// <summary>
        /// A list of splines (X/Y only) forming the resulting mesh
        /// </summary>
        public List<SplinePolyLine> Lines = new List<SplinePolyLine>();
        /// <summary>
        /// Winding rule used by triangulator
        /// </summary>
        public WindingRule Winding = WindingRule.EvenOdd;
        public Vector2 UVTiling = Vector2.one;
        public Vector2 UVOffset = Vector2.zero;
        public bool SuppressUVMapping;
        /// <summary>
        /// Whether UV2 should be set
        /// </summary>
        public bool UV2;
        /// <summary>
        /// Name of the returned mesh
        /// </summary>
        public string MeshName = string.Empty;
        /// <summary>
        /// Whether only vertices of the outline spline should be created
        /// </summary>
        public bool VertexLineOnly;

        public string Error { get; private set; }

        #endregion

        #region ### Private Fields ###

        private Tess mTess;
        private Mesh mMesh;

        #endregion

        #region ### Public Methods ###

        /// <summary>
        /// Create the Mesh using the current settings
        /// </summary>
        /// <param name="result">the resulting Mesh</param>
        /// <returns>true on success. If false, check the Error property!</returns>
        public bool Apply(out Mesh result)
        {
            ArrayPool<Vector3> pool = ArrayPoolsProvider.GetPool<Vector3>();

            mTess = null;
            mMesh = null;
            Error = string.Empty;
            bool triangulationSucceeded = triangulate();
            if (triangulationSucceeded)
            {
                mMesh = new Mesh();
                mMesh.name = MeshName;

                if (VertexLineOnly && Lines.Count > 0 && Lines[0] != null)
                {
                    SubArray<Vector3> vertices = Lines[0].GetVertexList();
                    mMesh.SetVertices(vertices.Array, 0, vertices.Count);
                    pool.Free(vertices);
                }
                else
                {
                    ContourVertex[] vertices = mTess.Vertices;
                    SubArray<Vector3> vector3s = pool.Allocate(vertices.Length);
                    UnityLibTessUtility.FromContourVertex(vertices, vector3s);
                    mMesh.SetVertices(vector3s.Array, 0, vector3s.Count);
                    mMesh.SetTriangles(mTess.ElementsArray.Value.Array, 0, mTess.ElementsArray.Value.Count, 0);
                    pool.Free(vector3s);
                }

                mMesh.RecalculateBounds();
                mMesh.RecalculateNormals();
                if (!SuppressUVMapping && !VertexLineOnly)
                {
                    Vector3 boundsSize = mMesh.bounds.size;
                    Vector3 boundsMin = mMesh.bounds.min;

                    float minSize = Mathf.Min(boundsSize.x, Mathf.Min(boundsSize.y, boundsSize.z));

                    bool minSizeIsX = minSize == boundsSize.x;
                    bool minSizeIsY = minSize == boundsSize.y;
                    bool minSizeIsZ = minSize == boundsSize.z;

                    Vector3[] vertices = mMesh.vertices;
                    int vertexCount = vertices.Length;

                    //set uv and uv2
                    SubArray<Vector2> uv;
                    SubArray<Vector2> uv2;
                    {
                        uv = ArrayPools.Vector2.Allocate(vertexCount);
                        Vector2[] uvArray = uv.Array;

                        uv2 = ArrayPools.Vector2.Allocate(UV2 ? vertexCount : 0);
                        Vector2[] uv2Array = uv2.Array;

                        for (int i = 0; i < vertexCount; i++)
                        {
                            float u;
                            float v;
                            Vector3 vertex = vertices[i];

                            if (minSizeIsX)
                            {
                                u = (vertex.y - boundsMin.y) / boundsSize.y;
                                v = (vertex.z - boundsMin.z) / boundsSize.z;
                            }
                            else if (minSizeIsY)
                            {
                                u = (vertex.z - boundsMin.z) / boundsSize.z;
                                v = (vertex.x - boundsMin.x) / boundsSize.x;
                            }
                            else if (minSizeIsZ)
                            {
                                u = (vertex.x - boundsMin.x) / boundsSize.x;
                                v = (vertex.y - boundsMin.y) / boundsSize.y;
                            }
                            else
                                throw new InvalidOperationException("Couldn't find the minimal bound dimension");

                            if (UV2)
                            {
                                uv2Array[i].x = u;
                                uv2Array[i].y = v;
                            }

                            u += UVOffset.x;
                            v += UVOffset.y;

                            u *= UVTiling.x;
                            v *= UVTiling.y;
                            uvArray[i].x = u;
                            uvArray[i].y = v;
                        }
                        mMesh.SetUVs(0, uv.Array, 0, uv.Count);
                        mMesh.SetUVs(1, uv2.Array, 0, uv2.Count);
                    }

                    ArrayPools.Vector2.Free(uv);
                    ArrayPools.Vector2.Free(uv2);
                    ArrayPools.Vector3.Free(vertices);
                }
            }
            result = mMesh;
            return triangulationSucceeded;
        }

        #endregion

        #region ### Privates ###
        /*! \cond PRIVATE */

        private bool triangulate()
        {
            if (Lines.Count == 0)
            {
                Error = "Missing splines to triangulate";
                return false;
            }

            if (VertexLineOnly)
                return true;

            mTess = new Tess();

            for (int i = 0; i < Lines.Count; i++)
            {
                if (Lines[i].Spline == null)
                {
                    Error = "Missing Spline";
                    return false;
                }
                if (!polyLineIsValid(Lines[i]))
                {
                    Error = Lines[i].Spline.name + ": Angle must be >0";
                    return false;
                }
                SubArray<Vector3> vertices = Lines[i].GetVertexList();
                if (vertices.Count < 3)
                {
                    Error = Lines[i].Spline.name + ": At least 3 Vertices needed!";
                    return false;
                }
                mTess.AddContour(UnityLibTessUtility.ToContourVertex(vertices, false), Lines[i].Orientation);
                ArrayPoolsProvider.GetPool<Vector3>().Free(vertices);
            }
            try
            {
                mTess.Tessellate(Winding, ElementType.Polygons, 3);
                return true;
            }
            catch (System.Exception e)
            {
                Error = e.Message;
            }

            return false;
        }

        private static bool polyLineIsValid(SplinePolyLine pl)
        {
            return (pl != null && pl.VertexMode == SplinePolyLine.VertexCalculation.ByApproximation ||
                    !Mathf.Approximately(0, pl.Angle));
        }

        /*! \endcond */
        #endregion
    }

    /// <summary>
    /// Spline Triangulation Helper Class
    /// </summary>
    [System.Serializable]
    public class SplinePolyLine
    {
        /// <summary>
        /// How to calculate vertices
        /// </summary>
        public enum VertexCalculation
        {
            /// <summary>
            /// Use Approximation points
            /// </summary>
            ByApproximation,
            /// <summary>
            /// By curvation angle
            /// </summary>
            ByAngle
        }

        /// <summary>
        /// Orientation order
        /// </summary>
        public ContourOrientation Orientation = ContourOrientation.Original;

        /// <summary>
        /// Base Spline
        /// </summary>
        public CurvySpline Spline;
        /// <summary>
        /// Vertex Calculation Mode
        /// </summary>
        public VertexCalculation VertexMode;
        /// <summary>
        /// Angle, used by VertexMode.ByAngle only
        /// </summary>
        public float Angle;
        /// <summary>
        /// Minimum distance, used by VertexMode.ByAngle only
        /// </summary>
        public float Distance;
        public Space Space;

        /// <summary>
        /// Creates a Spline2MeshCurve class using Spline2MeshCurve.VertexMode.ByApproximation
        /// </summary>
        public SplinePolyLine(CurvySpline spline) : this(spline, VertexCalculation.ByApproximation, 0, 0) { }
        /// <summary>
        /// Creates a Spline2MeshCurve class using Spline2MeshCurve.VertexMode.ByAngle
        /// </summary>
        public SplinePolyLine(CurvySpline spline, float angle, float distance) : this(spline, VertexCalculation.ByAngle, angle, distance) { }

        private SplinePolyLine(CurvySpline spline, VertexCalculation vertexMode, float angle, float distance, Space space = Space.World)
        {
            Spline = spline;
            VertexMode = vertexMode;
            Angle = angle;
            Distance = distance;
            Space = space;
        }
        /// <summary>
        /// Gets whether the spline is closed
        /// </summary>
        public bool IsClosed
        {
            get
            {
                return (Spline && Spline.Closed);
            }
        }

        /// <summary>
        /// Get vertices calculated using the current VertexMode
        /// </summary>
        /// <returns>an array of vertices</returns>
        [Obsolete("No more used in Curvy. Will get removed. Copy it if you still need it")]
        public Vector3[] GetVertices()
        {
            SubArray<Vector3> vertexList = GetVertexList();
            Vector3[] result = vertexList.CopyToArray(ArrayPools.Vector3);
            ArrayPools.Vector3.Free(vertexList);
            return result;
        }

        /// <summary>
        /// Get vertices calculated using the current VertexMode
        /// </summary>
        /// <returns>an array of vertices</returns>
        public SubArray<Vector3> GetVertexList()
        {
            SubArray<Vector3> points;
            switch (VertexMode)
            {
                case VertexCalculation.ByAngle:
                    points = GetPolygon(Spline, 0, 1, Angle, Distance, -1, false).ToSubArray();
                    break;
                default:
                    points = Spline.GetPositionsCache(Space.Self);
                    break;
            }

            if (Space == Space.World)
            {
                Vector3[] pointsArray = points.Array;
                int pointsCount = points.Count;
                for (int i = 0; i < pointsCount; i++)
                    pointsArray[i] = Spline.transform.TransformPoint(pointsArray[i]);
            }

            return points;
        }

        /// <summary>
        /// Gets an array of sampled points that follow some restrictions on the distance between two consecutive points, and the angle of tangents between those points
        /// </summary>
        /// <param name="fromTF">start TF</param>
        /// <param name="toTF">end TF</param>
        /// <param name="maxAngle">maximum angle in degrees between tangents</param>
        /// <param name="minDistance">minimum distance between two points</param>
        /// <param name="maxDistance">maximum distance between two points</param>
        /// <param name="vertexTF">Stores the TF of the resulting points</param>
        /// <param name="vertexTangents">Stores the Tangents of the resulting points</param>
        /// <param name="includeEndPoint">Whether the end position should be included</param>
        /// <param name="stepSize">the stepsize to use</param>
        /// <returns>an array of interpolated positions</returns>
        private static SubArrayList<Vector3> GetPolygon(CurvySpline spline, float fromTF, float toTF, float maxAngle, float minDistance, float maxDistance, bool includeEndPoint = true, float stepSize = 0.01f)
        {
            stepSize = Mathf.Clamp(stepSize, 0.002f, 1);
            maxDistance = (maxDistance == -1) ? spline.Length : Mathf.Clamp(maxDistance, 0, spline.Length);
            minDistance = Mathf.Clamp(minDistance, 0, maxDistance);
            if (!spline.Closed)
            {
                toTF = Mathf.Clamp01(toTF);
                fromTF = Mathf.Clamp(fromTF, 0, toTF);
            }
            SubArrayList<Vector3> vPos = new SubArrayList<Vector3>(50, ArrayPools.Vector3);

            int linearSteps = 0;
            float angleFromLast = 0;
            float distAccu = 0;
            Vector3 curPos = spline.Interpolate(fromTF);
            Vector3 curTangent = spline.GetTangent(fromTF);
            Vector3 lastPos = curPos;
            Vector3 lastTangent = curTangent;

            Action<Vector3> addPoint = ((position) =>
            {
                vPos.Add(position);
                angleFromLast = 0;
                distAccu = 0;

                linearSteps = 0;
            });

            addPoint(curPos);

            float tf = fromTF + stepSize;
            while (tf < toTF)
            {
                // Get Point Pos & Tangent
                spline.InterpolateAndGetTangent(tf % 1, out curPos, out curTangent);
                if (curTangent == Vector3.zero)
                {
                    Debug.Log("zero Tangent! Oh no!");
                }
                distAccu += (curPos - lastPos).magnitude;
                if (curTangent == lastTangent)
                    linearSteps++;
                if (distAccu >= minDistance)
                {
                    // Exceeding distance?
                    if (distAccu >= maxDistance)
                        addPoint(curPos);
                    else // Check angle
                    {
                        angleFromLast += Vector3.Angle(lastTangent, curTangent);
                        // Max angle reached or entering/leaving a linear zone
                        if (angleFromLast >= maxAngle || (linearSteps > 0 && angleFromLast > 0))
                            addPoint(curPos);
                    }
                }
                tf += stepSize;
                lastPos = curPos;
                lastTangent = curTangent;
            }
            if (includeEndPoint)
            {
                curPos = spline.Interpolate(toTF % 1);
                vPos.Add(curPos);
            }

            return vPos;
        }
    }
    #endregion
}