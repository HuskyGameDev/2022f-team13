// =====================================================================
// Copyright 2013-2022 ToolBuddy
// All rights reserved
// 
// http://www.toolbuddy.net
// =====================================================================

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FluffyUnderware.Curvy.Utils;
using FluffyUnderware.DevTools.Extensions;
using System;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Runtime.CompilerServices;
using FluffyUnderware.Curvy.Pools;
using ToolBuddy.Pooling.Pools;
using FluffyUnderware.DevTools;
using JetBrains.Annotations;
using ToolBuddy.Pooling.Collections;
using UnityEngine.Assertions;
using UnityEngine.Rendering;

#if CONTRACTS_FULL
using System.Diagnostics.Contracts;
#endif




namespace FluffyUnderware.Curvy.Generator
{
    //TODO replace all the misuse of the F concept here, where it should really be RelativeDistance 

    /// <summary>
    /// Additional properties for CGData based classes
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class CGDataInfoAttribute : Attribute
    {
        public readonly Color Color;

        public CGDataInfoAttribute(Color color)
        {
            Color = color;
        }

        public CGDataInfoAttribute(float r, float g, float b, float a = 1)
        {
            Color = new Color(r, g, b, a);
        }

        public CGDataInfoAttribute(string htmlColor)
        {
            Color = htmlColor.ColorFromHtml();
        }
    }

    /// <summary>
    /// Data Base class
    /// </summary>
    public class CGData : IDisposable
    {
        #region Dispose pattern

        private bool disposed = false;

        protected virtual bool Dispose(bool disposing)
        {
            if (disposed)
            {
                DTLog.LogWarning("[Curvy] Attempt to dispose a CGData twice. Please raise a bug report.");
                return false;
            }

            disposed = true;
            return true;
        }

        /// <summary>
        /// Disposes an instance that is no more used, allowing it to free its resources immediately.
        /// Dispose is called automatically when an instance is <see cref="Finalize"/>d
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~CGData()
        {
            Dispose(false);
        }

        #endregion

        public string Name;

        public virtual int Count
        {
            get { return 0; }
        }

        public static implicit operator bool(CGData a)
        {
            return !ReferenceEquals(a, null);
        }

        public virtual T Clone<T>() where T : CGData
        {
            return new CGData() as T;
        }

        /// <summary>
        /// Searches FMapArray and returns the index that covers the fValue as well as the percentage between index and index+1
        /// </summary>
        /// <param name="FMapArray">array of sorted values ranging from 0..1</param>
        /// <param name="fValue">a value 0..1</param>
        /// <param name="frag">fragment between the resulting and the next index (0..1)</param>
        /// <returns>the index where fValue lies in</returns>
        protected int getGenericFIndex(SubArray<float> FMapArray, float fValue, out float frag)
        {
            //WARNING this method is inlined in DeformMesh, if you modify something here modify it there too
            int index = CurvyUtility.InterpolationSearch(FMapArray.Array, FMapArray.Count, fValue);

            if (index == FMapArray.Count - 1)
            {
                index -= 1;
                frag = 1;
            }
            else
                frag = (fValue - FMapArray.Array[index]) / (FMapArray.Array[index + 1] - FMapArray.Array[index]);

            return index;
        }
    }

    /// <summary>
    /// Rasterized Shape Data (Polyline)
    /// </summary>
    [CGDataInfo(0.73f, 0.87f, 0.98f)]
    public class CGShape : CGData
    {
        /// <summary>
        /// The relative distance of each point.
        /// A relative distance is a value between 0 and 1 representing how far the point is in a shape.
        /// A value of 0 means the start of the shape, and a value of 1 means the end of it.
        /// It is defined as (the point's distance from the shape's start) / (the total length of the shape)
        /// This is unrelated to the notion of <seealso cref="CurvySplineSegment.TF"/> or F of a spline.
        /// Unfortunately, it is abusively called F in big parts of the the Curvy Generator related code, sorry for the confusion.
        /// </summary>
        /// <remarks>Setting a new <see cref="SubArray{T}"/> will <see cref="ArrayPool{T}.Free(ToolBuddy.Pooling.Collections.SubArray{T})"/> the current <see cref="SubArray{T}"/>  instance</remarks>
        public SubArray<float> RelativeDistances
        {
            get => relativeDistances;
            set
            {
                ArrayPools.Single.Free(relativeDistances);
                relativeDistances = value;
            }
        }

        /// <summary>
        /// The relative distance of each point relative to the source shape.
        /// A relative distance is a value between 0 and 1 representing how far the point is in a shape.
        /// A value of 0 means the start of the shape, and a value of 1 means the end of it.
        /// It is defined as (the point's distance from the shape's start) / (the total length of the shape)
        /// Contrary to <seealso cref="RelativeDistances"/> which is computed based on the actual shape, SourceRelativeDistances is computed based on the source shape.
        /// For example, if a Shape A is defined as the second quarter of a Shape B, A's first point will have a relative distance of 0, but a source relative distance of 0.25. A's last point will have a relative distance of 1, but a source relative distance of 0.5
        /// This is unrelated to the notion of <seealso cref="CurvySplineSegment.TF"/> or F of a spline.
        /// Unfortunately, it is abusively called F in big parts of the the Curvy Generator related code, sorry for the confusion.
        /// </summary>
        /// <remarks>Setting a new <see cref="SubArray{T}"/> will <see cref="ArrayPool{T}.Free(ToolBuddy.Pooling.Collections.SubArray{T})"/> the current <see cref="SubArray{T}"/>  instance</remarks>
        public SubArray<float> SourceRelativeDistances
        {
            get => sourceRelativeDistances;
            set
            {
                ArrayPools.Single.Free(sourceRelativeDistances);
                sourceRelativeDistances = value;
            }
        }

        /// <summary>
        /// Positions of the path's points, in the path's local space
        /// </summary>
        /// <remarks>Setting a new <see cref="SubArray{T}"/> will <see cref="ArrayPool{T}.Free(ToolBuddy.Pooling.Collections.SubArray{T})"/> the current <see cref="SubArray{T}"/>  instance</remarks>
        public SubArray<Vector3> Positions
        {
            get => positions;
            set
            {
                ArrayPools.Vector3.Free(positions);
                positions = value;
            }
        }

        /// <summary>
        /// Normals of the path's points, in the path's local space
        /// </summary>
        /// <remarks>Setting a new <see cref="SubArray{T}"/> will <see cref="ArrayPool{T}.Free(ToolBuddy.Pooling.Collections.SubArray{T})"/> the current <see cref="SubArray{T}"/>  instance</remarks>
        public SubArray<Vector3> Normals
        {
            get => normals;
            set
            {
                ArrayPools.Vector3.Free(normals);
                normals = value;
            }
        }

        /// <summary>
        /// Arbitrary mapped value to each point, usually U coordinate
        /// </summary>
        /// <remarks>Setting a new <see cref="SubArray{T}"/> will <see cref="ArrayPool{T}.Free(ToolBuddy.Pooling.Collections.SubArray{T})"/> the current <see cref="SubArray{T}"/>  instance</remarks>
        public SubArray<float> CustomValues
        {
            get => customValues;
            set
            {
                ArrayPools.Single.Free(customValues);
                customValues = value;
            }
        }

        /// <summary>
        /// The list of the shape's <see cref="DuplicatePoints"/>
        /// </summary>
        public List<DuplicateSamplePoint> DuplicatePoints { get; set; }

        #region Obsolete

        /// <summary>
        /// The relative distance of each point.
        /// A relative distance is a value between 0 and 1 representing how far the point is in a shape.
        /// A value of 0 means the start of the shape, and a value of 1 means the end of it.
        /// It is defined as (the point's distance from the shape's start) / (the total length of the shape)
        /// This is unrelated to the notion of <seealso cref="CurvySplineSegment.TF"/> or F of a spline.
        /// Unfortunately, it is abusively called F in big parts of the the Curvy Generator related code, sorry for the confusion.
        /// </summary>
        /// <remarks>This getter returns a copy of the actual array. For performance reasons, use the equivalent getter returning a <see cref="SubArray{T}"/> instance, which allows you to directly access and modify the underlying array</remarks>
        [Obsolete("Use RelativeDistances instead")]
        public float[] F
        {
            get => RelativeDistances.CopyToArray(ArrayPools.Single);
            set => RelativeDistances = new SubArray<float>(value);
        }

        /// <summary>
        /// The relative distance of each point relative to the source shape.
        /// A relative distance is a value between 0 and 1 representing how far the point is in a shape.
        /// A value of 0 means the start of the shape, and a value of 1 means the end of it.
        /// It is defined as (the point's distance from the shape's start) / (the total length of the shape)
        /// Contrary to <seealso cref="RelativeDistances"/> which is computed based on the actual shape, SourceRelativeDistances is computed based on the source shape.
        /// For example, if a Shape A is defined as the second quarter of a Shape B, A's first point will have a relative distance of 0, but a source relative distance of 0.25. A's last point will have a relative distance of 1, but a source relative distance of 0.5
        /// This is unrelated to the notion of <seealso cref="CurvySplineSegment.TF"/> or F of a spline.
        /// Unfortunately, it is abusively called F in big parts of the the Curvy Generator related code, sorry for the confusion.
        /// </summary>
        /// <remarks>This getter returns a copy of the actual array. For performance reasons, use the equivalent getter returning a <see cref="SubArray{T}"/> instance, which allows you to directly access and modify the underlying array</remarks>
        [Obsolete("Use SourceRelativeDistances instead")]
        public float[] SourceF
        {
            get => SourceRelativeDistances.CopyToArray(ArrayPools.Single);
            set => SourceRelativeDistances = new SubArray<float>(value);
        }

        /// <summary>
        /// Positions of the path's points, in the path's local space
        /// </summary>
        /// <remarks>This getter returns a copy of the actual array. For performance reasons, use the equivalent getter returning a <see cref="SubArray{T}"/> instance, which allows you to directly access and modify the underlying array</remarks>
        [Obsolete("Use Positions instead")]
        public Vector3[] Position
        {
            get => Positions.CopyToArray(ArrayPools.Vector3);
            set => Positions = new SubArray<Vector3>(value);
        }

        /// <summary>
        /// Normals of the path's points, in the path's local space
        /// </summary>
        /// <remarks>This getter returns a copy of the actual array. For performance reasons, use the equivalent getter returning a <see cref="SubArray{T}"/> instance, which allows you to directly access and modify the underlying array</remarks>
        [Obsolete("Use Normals instead")]
        public Vector3[] Normal
        {
            get => Normals.CopyToArray(ArrayPools.Vector3);
            set => Normals = new SubArray<Vector3>(value);
        }

        /// <summary>
        /// Arbitrary mapped value to each point, usually U coordinate
        /// </summary>
        /// <remarks>This getter returns a copy of the actual array. For performance reasons, use the equivalent getter returning a <see cref="SubArray{T}"/> instance, which allows you to directly access and modify the underlying array</remarks>
        [Obsolete("Use CustomValues instead")]
        public float[] Map
        {
            get => CustomValues.CopyToArray(ArrayPools.Single);
            set => CustomValues = new SubArray<float>(value);
        }

        #endregion

        /// <summary>
        /// Groups/Patches
        /// </summary>
        public List<SamplePointsMaterialGroup> MaterialGroups;
        /// <summary>
        /// Whether the source is managed or not
        /// </summary>
        /// <remarks>This could be used to determine if values needs to be transformed into generator space or not</remarks>
        public bool SourceIsManaged;
        /// <summary>
        /// Whether the base spline is closed or not
        /// </summary>
        public bool Closed;
        /// <summary>
        /// Whether the Shape/Path is seamless, i.e. Closed==true and the whole length is covered
        /// </summary>
        public bool Seamless;
        /// <summary>
        /// Length in world units
        /// </summary>
        public float Length;

        /// <summary>
        /// Gets the number of sample points
        /// </summary>
        public override int Count
        {
            get { return relativeDistances.Count; }
        }

        #region ### Private fields ###

        //TODO Debug time checks that F arrays contain values between 0 and 1
        private SubArray<float> relativeDistances;
        //OPTIM can the storage of this array be avoided by storing only SourceF and the start and end Distance, and infer F values only when needed?
        //OPTIM can we just assign SourceF to F when start and end distances are equal to respectively 0 and 1? (which is the case most of the time)
        private SubArray<float> sourceRelativeDistances;
        private SubArray<Vector3> positions;
        private SubArray<Vector3> normals;
        /*TODO Map is defined in CGShape but:
        1- filling it inside an instance of CGPath (which inherits from CGShape) is useless, since Map is used only by CGVolume when it takes it from a CGShape, and not a CGPath. So an optimization would be to not fill Map for instances not consumed by CGVolume
        2- I hope that storing it might be not needed, and calculating it only when needed might be possible
       */
        private SubArray<float> customValues;

        // Caching
        //TODO DESIGN OPTIM are these still needed, now that GetFIndex was greatly optimized?
        private float mCacheLastF = float.MaxValue;
        private int mCacheLastIndex;
        private float mCacheLastFrag;

        #endregion

        public CGShape() : base()
        {
            sourceRelativeDistances = ArrayPools.Single.Allocate(0);
            relativeDistances = ArrayPools.Single.Allocate(0);
            positions = ArrayPools.Vector3.Allocate(0);
            normals = ArrayPools.Vector3.Allocate(0);
            customValues = ArrayPools.Single.Allocate(0);
            DuplicatePoints = new List<DuplicateSamplePoint>();
            MaterialGroups = new List<SamplePointsMaterialGroup>();
        }

        public CGShape(CGShape source) : base()
        {
            positions = ArrayPools.Vector3.Clone(source.positions);
            normals = ArrayPools.Vector3.Clone(source.normals);
            customValues = ArrayPools.Single.Clone(source.customValues);
            DuplicatePoints = new List<DuplicateSamplePoint>(source.DuplicatePoints);
            relativeDistances = ArrayPools.Single.Clone(source.relativeDistances);
            sourceRelativeDistances = ArrayPools.Single.Clone(source.sourceRelativeDistances);
            MaterialGroups = new List<SamplePointsMaterialGroup>(source.MaterialGroups.Count);
            foreach (SamplePointsMaterialGroup materialGroup in source.MaterialGroups)
                MaterialGroups.Add(materialGroup.Clone());
            Closed = source.Closed;
            Seamless = source.Seamless;
            Length = source.Length;
            SourceIsManaged = source.SourceIsManaged;
        }

        protected override bool Dispose(bool disposing)
        {
            bool result = base.Dispose(disposing);
            if (result)
            {
                ArrayPools.Single.Free(sourceRelativeDistances);
                ArrayPools.Single.Free(relativeDistances);
                ArrayPools.Vector3.Free(positions);
                ArrayPools.Vector3.Free(normals);
                ArrayPools.Single.Free(customValues);
            }

            return result;
        }

        public override T Clone<T>()
        {
            return new CGShape(this) as T;
        }

        public static void Copy(CGShape dest, CGShape source)
        {
            ArrayPools.Vector3.Resize(ref dest.positions, source.positions.Count);
            Array.Copy(source.positions.Array, 0, dest.positions.Array, 0, source.positions.Count);
            ArrayPools.Vector3.Resize(ref dest.normals, source.normals.Count);
            Array.Copy(source.normals.Array, 0, dest.normals.Array, 0, source.normals.Count);
            ArrayPools.Single.Resize(ref dest.customValues, source.customValues.Count);
            Array.Copy(source.customValues.Array, 0, dest.customValues.Array, 0, source.customValues.Count);
            ArrayPools.Single.Resize(ref dest.relativeDistances, source.relativeDistances.Count);
            Array.Copy(source.relativeDistances.Array, 0, dest.relativeDistances.Array, 0, source.relativeDistances.Count);
            ArrayPools.Single.Resize(ref dest.sourceRelativeDistances, source.sourceRelativeDistances.Count);
            Array.Copy(source.sourceRelativeDistances.Array, 0, dest.sourceRelativeDistances.Array, 0, source.sourceRelativeDistances.Count);
            dest.DuplicatePoints.Clear();
            dest.DuplicatePoints.AddRange(source.DuplicatePoints);
            dest.MaterialGroups = source.MaterialGroups.Select(g => g.Clone()).ToList();
            dest.Closed = source.Closed;
            dest.Seamless = source.Seamless;
            dest.Length = source.Length;
        }

        //TODO documentation and whatnot
        public void Copy(CGShape source) { Copy(this, source); }

        /// <summary>
        /// Converts absolute (World Units) to relative (F) distance
        /// </summary>
        /// <param name="distance">distance in world units</param>
        /// <returns>Relative distance (0..1)</returns>
        public float DistanceToF(float distance)
        {
            return Mathf.Clamp(distance, 0, Length) / Length;
        }

        /// <summary>
        /// Converts relative (F) to absolute distance (World Units)
        /// </summary>
        /// <param name="f">relative distance (0..1)</param>
        /// <returns>Distance in World Units</returns>
        public float FToDistance(float f)
        {
            return Mathf.Clamp01(f) * Length;
        }

        /// <summary>
        /// Gets the index of a certain F
        /// </summary>
        /// <param name="f">F (0..1)</param>
        /// <param name="frag">fragment between the resulting and the next index (0..1)</param>
        /// <returns>the resulting index</returns>
        public int GetFIndex(float f, out float frag)
        {
#if CURVY_SANITY_CHECKS_PRIVATE
            Assert.IsTrue(f >= 0);
            if (f > 1)
                Debug.LogWarning(f);
#endif
            if (mCacheLastF != f)
            {
                mCacheLastF = f;
                //OPTIM make sure f is a ratio, then remove the following line
                float fValue = f == 1 ? f : f % 1;
                mCacheLastIndex = getGenericFIndex(relativeDistances, fValue, out mCacheLastFrag);
            }
            frag = mCacheLastFrag;

            return mCacheLastIndex;
        }

        /*
        /// <summary>
        /// Gets the index of a certain SourceF
        /// </summary>
        /// <param name="sourceF">F (0..1)</param>
        /// <param name="frag">fragment between the resulting and the next index (0..1)</param>
        /// <returns>the resulting index</returns>
        public int GetSourceFIndex(float sourceF, out float frag)
        {
            if (mCacheLastSourceF != sourceF)
            {
                mCacheLastSourceF = sourceF;

                mCacheLastSourceIndex = getGenericFIndex(ref F, sourceF, out mCacheLastSourceFrag);
            }
            frag = mCacheLastSourceFrag;
            return mCacheLastSourceIndex;
        }
        */
        /// <summary>
        /// Interpolates Position by F
        /// </summary>
        /// <param name="f">0..1</param>
        /// <returns>the interpolated position</returns>
        public Vector3 InterpolatePosition(float f)
        {
            float frag;
            int idx = GetFIndex(f, out frag);
            return OptimizedOperators.LerpUnclamped(positions.Array[idx], positions.Array[idx + 1], frag);
        }

        /// <summary>
        /// Interpolates Normal by F
        /// </summary>
        /// <param name="f">0..1</param>
        /// <returns>the interpolated normal</returns>
        public Vector3 InterpolateUp(float f)
        {
            float frag;
            int idx = GetFIndex(f, out frag);
            return Vector3.SlerpUnclamped(normals.Array[idx], normals.Array[idx + 1], frag);
        }

        /// <summary>
        /// Interpolates Position and Normal by F
        /// </summary>
        /// <param name="f">0..1</param>
        /// <param name="position"></param>
        /// <param name="up">a.k.a normal</param>
        public void Interpolate(float f, out Vector3 position, out Vector3 up)
        {
            float frag;
            int idx = GetFIndex(f, out frag);
            position = OptimizedOperators.LerpUnclamped(positions.Array[idx], positions.Array[idx + 1], frag);
            up = Vector3.SlerpUnclamped(normals.Array[idx], normals.Array[idx + 1], frag);
        }

        public void Move(ref float f, ref int direction, float speed, CurvyClamping clamping)
        {
            f = CurvyUtility.ClampTF(f + speed * direction, ref direction, clamping);
        }

        public void MoveBy(ref float f, ref int direction, float speedDist, CurvyClamping clamping)
        {
            float dist = CurvyUtility.ClampDistance(FToDistance(f) + speedDist * direction, ref direction, clamping, Length);
            f = DistanceToF(dist);
        }

        /// <summary>
        /// Recalculate Length and RelativeDistances (by measuring a polyline built from all Position points)
        /// </summary>
        /// <remarks>Call this after TRS'ing a shape</remarks>
        public virtual void Recalculate()
        {
            Length = 0;
            SubArray<float> dist = ArrayPools.Single.Allocate(Count);

            for (int i = 1; i < Count; i++)
            {
                dist.Array[i] = dist.Array[i - 1] + (positions.Array[i].Subtraction(positions.Array[i - 1])).magnitude;

            }

            if (Count > 0)
            {
                Length = dist.Array[Count - 1];
                if (Length > 0)
                {

                    relativeDistances.Array[0] = 0;
                    float oneOnLength = 1 / Length;
                    for (int i = 1; i < Count - 1; i++)
                        relativeDistances.Array[i] = dist.Array[i] * oneOnLength;
                    relativeDistances.Array[Count - 1] = 1;
                }
                else
                {
                    ArrayPools.Single.ResizeAndClear(ref relativeDistances, Count);
                }
            }

            ArrayPools.Single.Free(dist);

            //for (int i = 1; i < Count; i++)
            //    Direction[i] = (Position[i] - Position[i - 1]).normalized;
        }

        [Obsolete("Use another overload of RecalculateNormals instead")]
        public void RecalculateNormals(List<int> softEdges)
        {
            //TODO this implementation works properly with 2D shapes, but creates invalid results with 3D paths. This is ok for now because the code calls it only on shapes, but it is a ticking bomb
            //TODO document the method after fixing it
            if (normals.Count != positions.Count)
            {
                ArrayPools.Vector3.Resize(ref normals, positions.Count);
            }

            for (int mg = 0; mg < MaterialGroups.Count; mg++)
            {
                for (int p = 0; p < MaterialGroups[mg].Patches.Count; p++)
                {
                    SamplePointsPatch patch = MaterialGroups[mg].Patches[p];
                    Vector3 t;
                    for (int vt = 0; vt < patch.Count; vt++)
                    {
                        int x = patch.Start + vt;
                        t = (positions.Array[x + 1] - positions.Array[x]).normalized;
                        normals.Array[x] = new Vector3(-t.y, t.x, 0);
#if CURVY_SANITY_CHECKS_PRIVATE
                        if (normals.Array[x].magnitude.Approximately(1f) == false)
                            Debug.LogError($"Normal is not normalized, length was {normals.Array[x].magnitude}");//happens if shape is not in the XY plane
#endif
                    }
                    t = (positions.Array[patch.End] - positions.Array[patch.End - 1]).normalized;
                    normals.Array[patch.End] = new Vector3(-t.y, t.x, 0);
#if CURVY_SANITY_CHECKS_PRIVATE
                    if (normals.Array[patch.End].magnitude.Approximately(1f) == false)
                        Debug.LogError("Normal is not normalized");//happens if shape is not in the XY plane
#endif
                }
            }

            // Handle soft edges
            for (int i = 0; i < softEdges.Count; i++)
            {
                int previous = softEdges.ToArray()[i] - 1;
                if (previous < 0)
                    previous = positions.Count - 1;

                int beforePrevious = previous - 1;
                if (beforePrevious < 0)
                    beforePrevious = positions.Count - 1;

                int next = softEdges.ToArray()[i] + 1;
                if (next == positions.Count)
                    next = 0;

                normals.Array[softEdges.ToArray()[i]] = Vector3.Slerp(normals.Array[beforePrevious], normals.Array[next], 0.5f);
                normals.Array[previous] = normals.Array[softEdges.ToArray()[i]];
            }
        }

        /// <summary>
        /// Recalculate the shape's <see cref="Normals"/> based on the spline the shape was rasterized from
        /// </summary>
        public void RecalculateNormals([NotNull] CurvySpline spline)
        {
#if CURVY_SANITY_CHECKS
            Assert.IsFalse(spline.Orientation == CurvyOrientation.None);
#endif
            if (normals.Count != positions.Count)
            {
                ArrayPools.Vector3.Resize(ref normals, positions.Count);
            }

            Vector3[] normalsArray = normals.Array;
            float[] floats = SourceRelativeDistances.Array;

            for (int mg = 0; mg < MaterialGroups.Count; mg++)
            {
                for (int p = 0; p < MaterialGroups[mg].Patches.Count; p++)
                {
                    SamplePointsPatch patch = MaterialGroups[mg].Patches[p];
                    for (int vt = 0; vt < patch.Count; vt++)
                    {
                        int x = patch.Start + vt;
                        normalsArray[x] = spline.GetOrientationUpFast(spline.DistanceToTF(spline.Length * floats[x]), Space.Self);
#if CURVY_SANITY_CHECKS_PRIVATE
                        if (normalsArray[x].magnitude.Approximately(1f) == false)
                            Debug.LogError($"Normal is not normalized, length was {normalsArray[x].magnitude}");//happens if shape is not in the XY plane
#endif
                    }

                    normalsArray[patch.End] = spline.GetOrientationUpFast(spline.DistanceToTF(spline.Length * floats[patch.End]), Space.Self);
#if CURVY_SANITY_CHECKS_PRIVATE
                    if (normalsArray[patch.End].magnitude.Approximately(1f) == false)
                        Debug.LogError("Normal is not normalized");//happens if shape is not in the XY plane
#endif
                }
            }

            // Handle soft edges
            foreach (DuplicateSamplePoint duplicateSamplePoint in DuplicatePoints)
            {
                if (duplicateSamplePoint.IsHardEdge)
                {
                    int index = duplicateSamplePoint.StartIndex;
                    normalsArray[index] = normalsArray[Math.Max(0, index - 1)];
                }
            }
        }

        /// <summary>
        /// Recalculate the shape's <see cref="Normals"/> based on shape's rasterized <see cref="Positions"/>
        /// </summary>
        public void RecalculateNormals()
        {
            //TODO this implementation works properly with 2D shapes, but creates invalid results with 3D paths. This is ok for now because the code calls it only on shapes, but it is a ticking bomb
            //TODO document the method after fixing it
            if (normals.Count != positions.Count)
            {
                ArrayPools.Vector3.Resize(ref normals, positions.Count);
            }

            Vector3[] positionsArray = positions.Array;
            Vector3[] normalsArray = normals.Array;

            for (int mg = 0; mg < MaterialGroups.Count; mg++)
            {
                for (int p = 0; p < MaterialGroups[mg].Patches.Count; p++)
                {
                    SamplePointsPatch patch = MaterialGroups[mg].Patches[p];
                    Vector3 t;
                    int x;
                    for (int vt = 0; vt < patch.Count; vt++)
                    {
                        x = patch.Start + vt;
                        t = (positionsArray[x + 1] - positionsArray[x]).normalized;
                        //todo handle case where t = 0
                        normalsArray[x] = new Vector3(-t.y, t.x, 0);
#if CURVY_SANITY_CHECKS_PRIVATE
                        if (normalsArray[x].magnitude.Approximately(1f) == false)
                            Debug.LogError($"Normal is not normalized, length was {normalsArray[x].magnitude}");//happens if shape is not in the XY plane or if length is 0
#endif
                    }
                    t = (positionsArray[patch.End] - positionsArray[patch.End - 1]).normalized;
                    normalsArray[patch.End] = new Vector3(-t.y, t.x, 0);
#if CURVY_SANITY_CHECKS_PRIVATE
                    if (normalsArray[patch.End].magnitude.Approximately(1f) == false)
                        Debug.LogError("Normal is not normalized");//happens if shape is not in the XY plane
#endif
                }
            }

            // Handle soft edges
            foreach (DuplicateSamplePoint duplicateSamplePoint in DuplicatePoints)
            {
                if (duplicateSamplePoint.IsHardEdge == false)
                {
                    int previous = duplicateSamplePoint.EndIndex - 1;
                    if (previous < 0)
                        previous = positions.Count - 1;

                    int beforePrevious = previous - 1;
                    if (beforePrevious < 0)
                        beforePrevious = positions.Count - 1;

                    int next = duplicateSamplePoint.EndIndex + 1;
                    if (next == positions.Count)
                        next = 0;

                    normalsArray[duplicateSamplePoint.EndIndex] = Vector3.Slerp(normalsArray[beforePrevious], normalsArray[next], 0.5f);
                    normalsArray[previous] = normalsArray[duplicateSamplePoint.EndIndex];

                }
            }
        }
    }

    /// <summary>
    /// Path Data (Shape + Direction (Spline Tangents) + Orientation/Up)
    /// </summary>
    [CGDataInfo(0.13f, 0.59f, 0.95f)]
    public class CGPath : CGShape
    {
        /// <summary>
        /// Tangents of the path's points, in the path's local space
        /// </summary>
        /// <remarks>Setting a new <see cref="SubArray{T}"/> will <see cref="ArrayPool{T}.Free(ToolBuddy.Pooling.Collections.SubArray{T})"/> the current <see cref="SubArray{T}"/>  instance</remarks>
        public SubArray<Vector3> Directions
        {
            get => directions;
            set
            {
                ArrayPools.Vector3.Free(directions);
                directions = value;
            }
        }

        /// <summary>
        /// Tangents of the path's points, in the path's local space
        /// </summary>
        /// <remarks>This getter returns a copy of the actual array. For performance reasons, use the equivalent getter returning a <see cref="SubArray{T}"/> instance, which allows you to directly access and modify the underlying array</remarks>
        [Obsolete("Use Directions instead")]
        public Vector3[] Direction
        {
            get => Directions.CopyToArray(ArrayPools.Vector3);
            set => Directions = new SubArray<Vector3>(value);
        }

        private SubArray<Vector3> directions;

        public CGPath() : base()
        {
            directions = ArrayPools.Vector3.Allocate(0);
        }
        public CGPath(CGPath source) : base(source)
        {
            directions = ArrayPools.Vector3.Clone(source.directions);
        }

        protected override bool Dispose(bool disposing)
        {
            bool result = base.Dispose(disposing);
            if (result)
                ArrayPools.Vector3.Free(directions);
            return result;
        }

        public override T Clone<T>()
        {
            return new CGPath(this) as T;
        }

        public static void Copy(CGPath dest, CGPath source)
        {
            CGShape.Copy(dest, source);
            ArrayPools.Vector3.Resize(ref dest.directions, source.directions.Count);
            Array.Copy(source.directions.Array, 0, dest.directions.Array, 0, source.directions.Count);
        }

        /// <summary>
        /// Interpolates Position, Direction and Normal by F
        /// </summary>
        /// <param name="f">0..1</param>
        /// <param name="position"></param>
        /// <param name="direction">a.k.a tangent</param>
        /// <param name="up">a.k.a normal</param>
        public void Interpolate(float f, out Vector3 position, out Vector3 direction, out Vector3 up)
        {
            float frag;
            int idx = GetFIndex(f, out frag);
            position = OptimizedOperators.LerpUnclamped(Positions.Array[idx], Positions.Array[idx + 1], frag);
            direction = Vector3.SlerpUnclamped(directions.Array[idx], directions.Array[idx + 1], frag);
            up = Vector3.SlerpUnclamped(Normals.Array[idx], Normals.Array[idx + 1], frag);
        }

        [Obsolete("Method is no more used by Curvy and will get removed. Copy its content if you still need it")]
        public void Interpolate(float f, float angleF, out Vector3 pos, out Vector3 dir, out Vector3 up)
        {
            Interpolate(f, out pos, out dir, out up);
            if (angleF != 0)
            {
                Quaternion R = Quaternion.AngleAxis(angleF * -360, dir);
                up = R * up;
            }
        }

        /// <summary>
        /// Interpolates Direction by F
        /// </summary>
        /// <param name="f">0..1</param>
        public Vector3 InterpolateDirection(float f)
        {
            float frag;
            int idx = GetFIndex(f, out frag);
            return Vector3.SlerpUnclamped(directions.Array[idx], directions.Array[idx + 1], frag);
        }
    }

    /// <summary>
    /// Volume Data (Path + Vertex, VertexNormal, Cross)
    /// </summary>
    [CGDataInfo(0.08f, 0.4f, 0.75f)]
    public class CGVolume : CGPath
    {
        /// <summary>
        /// Positions of the points, in the local space
        /// </summary>
        /// <remarks>Setting a new <see cref="SubArray{T}"/> will <see cref="ArrayPool{T}.Free(ToolBuddy.Pooling.Collections.SubArray{T})"/> the current <see cref="SubArray{T}"/>  instance</remarks>
        public SubArray<Vector3> Vertices
        {
            get => vertices;
            set
            {
                ArrayPools.Vector3.Free(vertices);
                vertices = value;
            }
        }

        /// <summary>
        /// Notmals of the points, in the local space
        /// </summary>
        /// <remarks>Setting a new <see cref="SubArray{T}"/> will <see cref="ArrayPool{T}.Free(ToolBuddy.Pooling.Collections.SubArray{T})"/> the current <see cref="SubArray{T}"/>  instance</remarks>
        public SubArray<Vector3> VertexNormals
        {
            get => vertexNormals;
            set
            {
                ArrayPools.Vector3.Free(vertexNormals);
                vertexNormals = value;
            }
        }

        /// <summary>
        /// The <see cref="CGShape.F"/> of the <see cref="CGShape"/> used in the extrusion of this volume
        /// </summary>
        /// <remarks>Setting a new <see cref="SubArray{T}"/> will <see cref="ArrayPool{T}.Free(ToolBuddy.Pooling.Collections.SubArray{T})"/> the current <see cref="SubArray{T}"/>  instance</remarks>
        public SubArray<float> CrossRelativeDistances
        {
            get => crossRelativeDistances;
            set
            {
                ArrayPools.Single.Free(crossRelativeDistances);
                crossRelativeDistances = value;
            }
        }

        /// <summary>
        /// The <see cref="CGShape.CustomValues"/> of the <see cref="CGShape"/> used in the extrusion of this volume
        /// </summary>
        /// <remarks>Setting a new <see cref="SubArray{T}"/> will <see cref="ArrayPool{T}.Free(ToolBuddy.Pooling.Collections.SubArray{T})"/> the current <see cref="SubArray{T}"/>  instance</remarks>
        public SubArray<float> CrossCustomValues
        {
            get => crossCustomValues;
            set
            {
                ArrayPools.Single.Free(crossCustomValues);
                crossCustomValues = value;
            }
        }

        /// <summary>
        /// The 2D scale of the mesh at each sample point of the volume's path
        /// </summary>
        public SubArray<Vector2> Scales
        {
            get => scales;
            set
            {
                ArrayPools.Vector2.Free(scales);
                scales = value;
            }
        }

        #region Obsolete

        /// <summary>
        /// Positions of the points, in the local space
        /// </summary>
        /// <remarks>This getter returns a copy of the actual array. For performance reasons, use the equivalent getter returning a <see cref="SubArray{T}"/> instance, which allows you to directly access and modify the underlying array</remarks>
        [Obsolete("Use Vertices instead")]
        public Vector3[] Vertex
        {
            get => Vertices.CopyToArray(ArrayPools.Vector3);
            set => Vertices = new SubArray<Vector3>(value);
        }

        /// <summary>
        /// Normals of the points, in the local space
        /// </summary>
        /// <remarks>This getter returns a copy of the actual array. For performance reasons, use the equivalent getter returning a <see cref="SubArray{T}"/> instance, which allows you to directly access and modify the underlying array</remarks>
        [Obsolete("Use VertexNormals instead")]
        public Vector3[] VertexNormal
        {
            get => VertexNormals.CopyToArray(ArrayPools.Vector3);
            set => VertexNormals = new SubArray<Vector3>(value);
        }

        /// <summary>
        /// The <see cref="CGShape.F"/> of the <see cref="CGShape"/> used in the extrusion of this volume
        /// </summary>
        /// <remarks>This getter returns a copy of the actual array. For performance reasons, use the equivalent getter returning a <see cref="SubArray{T}"/> instance, which allows you to directly access and modify the underlying array</remarks>
        [Obsolete("Use CrossRelativeDistances instead")]
        public float[] CrossF
        {
            get => CrossRelativeDistances.CopyToArray(ArrayPools.Single);
            set => CrossRelativeDistances = new SubArray<float>(value);
        }

        /// <summary>
        /// The <see cref="CGShape.CustomValues"/> of the <see cref="CGShape"/> used in the extrusion of this volume
        /// </summary>
        /// <remarks>This getter returns a copy of the actual array. For performance reasons, use the equivalent getter returning a <see cref="SubArray{T}"/> instance, which allows you to directly access and modify the underlying array</remarks>
        [Obsolete("Use CrossCustomValues instead")]
        public float[] CrossMap
        {
            get => CrossCustomValues.CopyToArray(ArrayPools.Single);
            set => CrossCustomValues = new SubArray<float>(value);
        }

        #endregion

        /// <summary>
        /// Length of a given cross segment. Will be calculated on demand only!
        /// </summary>
        [Obsolete("Do not use this. Use the GetCrossLength method instead")]
        public float[] SegmentLength
        {
            get
            {
                if (_segmentLength == null)
                    _segmentLength = new float[Count];
                return _segmentLength;
            }
            set => _segmentLength = value;
        }

        /// <summary>
        /// Gets the number of cross shape's sample points
        /// </summary>
        public int CrossSize { get { return crossRelativeDistances.Count; } }
        /// <summary>
        /// Whether the Cross base spline is closed or not
        /// </summary>
        public bool CrossClosed;//TODO make obsolete then remove this, it is not needed by Curvy
        /// <summary>
        /// Whether the Cross shape covers the whole length of the base spline
        /// </summary>
        public bool CrossSeamless;
        /// <summary>
        /// A shift of the <see cref="CrossRelativeDistances"/> value that is applied when using the interpolation methods on the volume, like <see cref="InterpolateVolume"/>
        /// </summary>
        public float CrossFShift;

        public SamplePointsMaterialGroupCollection CrossMaterialGroups;

        public int VertexCount { get { return vertices.Count; } }

        #region private fields

        private SubArray<Vector3> vertices;
        private SubArray<Vector3> vertexNormals;
        private SubArray<float> crossRelativeDistances;
        private SubArray<float> crossCustomValues;
        private SubArray<Vector2> scales;
        [Obsolete("Do not use this. Use the GetCrossLength method instead")]
        private float[] _segmentLength;

        #endregion

        #region ### Constructors ###

        [Obsolete("Use one of the other constructors")]
        public CGVolume() : base() { }

        public CGVolume(int samplePoints, CGShape crossShape) : base()
        {
            crossRelativeDistances = ArrayPools.Single.Clone(crossShape.RelativeDistances);
            crossCustomValues = ArrayPools.Single.Clone(crossShape.CustomValues);
            scales = ArrayPools.Vector2.Allocate(samplePoints);
            CrossClosed = crossShape.Closed;
            CrossSeamless = crossShape.Seamless;
            CrossMaterialGroups = new SamplePointsMaterialGroupCollection(crossShape.MaterialGroups);
            vertices = ArrayPools.Vector3.Allocate(CrossSize * samplePoints);
            vertexNormals = ArrayPools.Vector3.Allocate(vertices.Count);
        }

        public CGVolume(CGPath path, CGShape crossShape)
            : base(path)
        {
            crossRelativeDistances = ArrayPools.Single.Clone(crossShape.RelativeDistances);
            crossCustomValues = ArrayPools.Single.Clone(crossShape.CustomValues);
            scales = ArrayPools.Vector2.Allocate(Count);
            CrossClosed = crossShape.Closed;
            CrossSeamless = crossShape.Seamless;
            CrossMaterialGroups = new SamplePointsMaterialGroupCollection(crossShape.MaterialGroups);
            vertices = ArrayPools.Vector3.Allocate(CrossSize * Count);
            vertexNormals = ArrayPools.Vector3.Allocate(vertices.Count);
        }

        public CGVolume(CGVolume source)
            : base(source)
        {
            vertices = ArrayPools.Vector3.Clone(source.vertices);
            vertexNormals = ArrayPools.Vector3.Clone(source.vertexNormals);
            crossRelativeDistances = ArrayPools.Single.Clone(source.crossRelativeDistances);
            crossCustomValues = ArrayPools.Single.Clone(source.crossCustomValues);
            scales = ArrayPools.Vector2.Clone(source.scales);
            CrossClosed = source.Closed;
            CrossSeamless = source.CrossSeamless;
            CrossFShift = source.CrossFShift;
            CrossMaterialGroups = new SamplePointsMaterialGroupCollection(source.CrossMaterialGroups);
        }

        #endregion

        protected override bool Dispose(bool disposing)
        {
            bool result = base.Dispose(disposing);
            if (result)
            {
                ArrayPools.Vector3.Free(vertices);
                ArrayPools.Vector3.Free(vertexNormals);
                ArrayPools.Single.Free(crossRelativeDistances);
                ArrayPools.Single.Free(crossCustomValues);
                ArrayPools.Vector2.Free(scales);
#pragma warning disable 618
                if (SegmentLength != null)
                    ArrayPools.Single.Free(SegmentLength);
#pragma warning restore 618
            }

            return result;
        }

        /// <summary>
        /// Returns a CGVolume made from the given CGPath and CGShape
        /// </summary>
        /// <param name="data">If not null, the returned instance will be the one but with its fields updated. If null, a new instance will be created</param>
        /// <param name="path">The path used in the creation of the volume</param>
        /// <param name="crossShape">The shape used in the creation of the volume</param>
        /// <returns></returns>
        public static CGVolume Get(CGVolume data, CGPath path, CGShape crossShape)
        {
            if (data == null)
                return new CGVolume(path, crossShape);

            Copy(data, path);

#pragma warning disable 618
            if (data._segmentLength != null)
                data.SegmentLength = new float[data.Count];
#pragma warning restore 618

            // Volume
            ArrayPools.Single.Resize(ref data.crossRelativeDistances, crossShape.RelativeDistances.Count, false);
            Array.Copy(crossShape.RelativeDistances.Array, 0, data.crossRelativeDistances.Array, 0, crossShape.RelativeDistances.Count);

            ArrayPools.Single.Resize(ref data.crossCustomValues, crossShape.CustomValues.Count, false);
            Array.Copy(crossShape.CustomValues.Array, 0, data.crossCustomValues.Array, 0, crossShape.CustomValues.Count);

            ArrayPools.Vector2.Resize(ref data.scales, path.Count, false);

            data.CrossClosed = crossShape.Closed;
            data.CrossSeamless = crossShape.Seamless;
            data.CrossMaterialGroups = new SamplePointsMaterialGroupCollection(crossShape.MaterialGroups);
            ArrayPools.Vector3.Resize(ref data.vertices, data.CrossSize * data.Positions.Count, false);
            ArrayPools.Vector3.Resize(ref data.vertexNormals, data.vertices.Count, false);
            return data;
        }


        public override T Clone<T>()
        {
            return new CGVolume(this) as T;
        }



        public void InterpolateVolume(float f, float crossF, out Vector3 pos, out Vector3 dir, out Vector3 up)
        {
            float frag;
            float cfrag;
            int v0Idx = GetVertexIndex(f, crossF, out frag, out cfrag);

            // (2)-(3)
            //  | \ |
            // (0)-(1)
            Vector3 xd, zd;
            Vector3 v0 = vertices.Array[v0Idx];
            Vector3 v1 = vertices.Array[v0Idx + 1];
            Vector3 v2 = vertices.Array[v0Idx + CrossSize];

            if (frag + cfrag > 1)
            {
                Vector3 v3 = vertices.Array[v0Idx + CrossSize + 1];
                xd = v3 - v2;
                zd = v3 - v1;
                pos = v2 - zd * (1 - frag) + xd * (cfrag);
            }
            else
            {
                xd = v1 - v0;
                zd = v2 - v0;
                pos = v0 + zd * frag + xd * cfrag;
            }

            dir = zd.normalized;
            up = Vector3.Cross(zd, xd);
        }

        public Vector3 InterpolateVolumePosition(float f, float crossF)
        {
            float frag;
            float cfrag;
            int v0Idx = GetVertexIndex(f, crossF, out frag, out cfrag);
            // (2)-(3)
            //  | \ |
            // (0)-(1)
            Vector3 xd, zd;
            Vector3 v0 = vertices.Array[v0Idx];
            Vector3 v1 = vertices.Array[v0Idx + 1];
            Vector3 v2 = vertices.Array[v0Idx + CrossSize];

            if (frag + cfrag > 1)
            {
                Vector3 v3 = vertices.Array[v0Idx + CrossSize + 1];
                xd = v3 - v2;
                zd = v3 - v1;
                return v2 - zd * (1 - frag) + xd * (cfrag);
            }
            else
            {
                xd = v1 - v0;
                zd = v2 - v0;
                return v0 + zd * frag + xd * cfrag;
            }
        }

        public Vector3 InterpolateVolumeDirection(float f, float crossF)
        {
            float frag;
            float cfrag;
            int v0Idx = GetVertexIndex(f, crossF, out frag, out cfrag);

            // (2)-(3)
            //  | \ |
            // (0)-(1)
            if (frag + cfrag > 1)
            {
                Vector3 v1 = vertices.Array[v0Idx + 1];
                Vector3 v3 = vertices.Array[v0Idx + CrossSize + 1];
                return (v3 - v1).normalized;
            }
            else
            {
                Vector3 v0 = vertices.Array[v0Idx];
                Vector3 v2 = vertices.Array[v0Idx + CrossSize];
                return (v2 - v0).normalized;
            }

        }

        public Vector3 InterpolateVolumeUp(float f, float crossF)
        {
            float frag;
            float cfrag;
            int v0Idx = GetVertexIndex(f, crossF, out frag, out cfrag);

            // (2)-(3)
            //  | \ |
            // (0)-(1)
            Vector3 xd, zd;

            Vector3 v1 = vertices.Array[v0Idx + 1];
            Vector3 v2 = vertices.Array[v0Idx + CrossSize];

            if (frag + cfrag > 1)
            {
                Vector3 v3 = vertices.Array[v0Idx + CrossSize + 1];
                xd = v3 - v2;
                zd = v3 - v1;
            }
            else
            {
                Vector3 v0 = vertices.Array[v0Idx];
                xd = v1 - v0;
                zd = v2 - v0;
            }
            return Vector3.Cross(zd, xd);
        }

        public float GetCrossLength(float pathF)
        {
            int s0;
            int s1;
            float frag;
#pragma warning disable 618
            GetSegmentIndices(pathF, out s0, out s1, out frag);
#pragma warning restore 618

#pragma warning disable 618
            if (SegmentLength[s0] == 0)
                SegmentLength[s0] = calcSegmentLength(s0);
            if (SegmentLength[s1] == 0)
                SegmentLength[s1] = calcSegmentLength(s1);

            return Mathf.LerpUnclamped(SegmentLength[s0], SegmentLength[s1], frag);
#pragma warning restore 618
        }


        public float CrossFToDistance(float f, float crossF, CurvyClamping crossClamping = CurvyClamping.Clamp)
        {
            return GetCrossLength(f) * CurvyUtility.ClampTF(crossF, crossClamping);
        }

        public float CrossDistanceToF(float f, float distance, CurvyClamping crossClamping = CurvyClamping.Clamp)
        {
            float cl = GetCrossLength(f);
            return CurvyUtility.ClampDistance(distance, crossClamping, cl) / cl;
        }

        /// <summary>
        /// Get the indices of the two points on the path that are surrounding the point at pathF
        /// </summary>
        /// <param name="pathF">The relative distance of the input point on the path</param>
        /// <param name="segment0Index">Index of the path point just before the input point </param>
        /// <param name="segment1Index">Index of the path point just after the input point</param>
        /// <param name="frag">The interpolation value between segment0Index and segment1Index, defining the exact position of the input point between those two points</param>
        [Obsolete("Method will get removed. Copy its content if you still need it")]
        public void GetSegmentIndices(float pathF, out int segment0Index, out int segment1Index, out float frag)
        {
            segment0Index = GetFIndex(Mathf.Repeat(pathF, 1), out frag);
            segment1Index = segment0Index + 1;
        }

        public int GetSegmentIndex(int segment)
        {
            return segment * CrossSize;
        }

        public int GetCrossFIndex(float crossF, out float frag)
        {
            float f = crossF + CrossFShift;
            //OPTIM if f is always positive, replace repeat with %. Right now crossF can be negative
            f = f == 1 ? f : Mathf.Repeat(f, 1);
            int index = getGenericFIndex(crossRelativeDistances, f, out frag);

            return index;
        }

        /// <summary>
        /// Get the index of the first vertex belonging to the segment a certain F is part of
        /// </summary>
        /// <param name="pathF">position on the path (0..1)</param>
        /// <param name="pathFrag">remainder between the returned segment and the next segment</param>
        /// <returns>a vertex index</returns>
        public int GetVertexIndex(float pathF, out float pathFrag)
        {
            int pIdx = GetFIndex(pathF, out pathFrag);
            return pIdx * CrossSize;
        }

        /// <summary>
        /// Get the index of the first vertex of the edge a certain F and CrossF is part of
        /// </summary>
        /// <param name="pathF">position on the path (0..1)</param>
        /// <param name="crossF">position on the cross (0..1)</param>
        /// <param name="pathFrag">remainder between the segment and the next segment</param>
        /// <param name="crossFrag">remainder between the returned vertex and the next vertex</param>
        /// <returns>a vertex index</returns>
        public int GetVertexIndex(float pathF, float crossF, out float pathFrag, out float crossFrag)
        {
            int pIdx = GetVertexIndex(pathF, out pathFrag);
            int cIdx = GetCrossFIndex(crossF, out crossFrag);
            return pIdx + cIdx;
        }

        /// <summary>
        /// Gets all vertices belonging to one or more extruded shape segments
        /// </summary>
        /// <param name="segmentIndices">indices of segments in question</param>
        public Vector3[] GetSegmentVertices(params int[] segmentIndices)
        {
            SubArray<Vector3> verts = ArrayPools.Vector3.Allocate(CrossSize * segmentIndices.Length);
            for (int i = 0; i < segmentIndices.Length; i++)
            {
                int sourceIndex = segmentIndices[i] * CrossSize;
                int destinationIndex = i * CrossSize;
                Array.Copy(vertices.Array, sourceIndex, verts.Array, destinationIndex, CrossSize);
            }

            return verts.CopyToArray(ArrayPools.Vector3);
        }


        private float calcSegmentLength(int segmentIndex)
        {
            int vstart = segmentIndex * CrossSize;
            int vend = vstart + CrossSize - 1;
            float l = 0;
            for (int i = vstart; i < vend; i++)
                l += (vertices.Array[i + 1] - vertices.Array[i]).magnitude;

            return l;
        }

    }

    /// <summary>
    /// Bounds data class
    /// </summary>
    [CGDataInfo(1, 0.8f, 0.5f)]
    public class CGBounds : CGData
    {
        protected Bounds? mBounds;
        public Bounds Bounds
        {
            get
            {
                if (!mBounds.HasValue)
                    RecalculateBounds();
                return mBounds.Value;
            }
            set
            {
                if (mBounds != value)
                    mBounds = value;
            }
        }

        public float Depth
        {
            get
            {
                //OPTIM just do the delta between max z and min z, and get rid of bounds
                return Bounds.size.z;
            }
        }

        public CGBounds() : base() { }

        public CGBounds(Bounds bounds) : base()
        {
            Bounds = bounds;
        }

        public CGBounds(CGBounds source)
        {
            Name = source.Name;
            if (source.mBounds.HasValue) //Do not copy bounds if they are not computed yet
                Bounds = source.Bounds;
        }


        public virtual void RecalculateBounds()
        {
            Bounds = new Bounds();
        }

        public override T Clone<T>()
        {
            return new CGBounds(this) as T;
        }

        public static void Copy(CGBounds dest, CGBounds source)
        {
            if (source.mBounds.HasValue) //Do not copy bounds if they are not computed yet
                dest.Bounds = source.Bounds;
        }
    }

    /// <summary>
    /// SubMesh data (triangles, material)
    /// </summary>
    public class CGVSubMesh : CGData
    {
        /// <summary>
        /// Vertex indices constituting the mesh's triangles
        /// </summary>
        /// <remarks>Setting a new <see cref="SubArray{T}"/> will <see cref="ArrayPool{T}.Free(ToolBuddy.Pooling.Collections.SubArray{T})"/> the current <see cref="SubArray{T}"/>  instance</remarks>
        public SubArray<int> TrianglesList
        {
            get => triangles;
            set
            {
                ArrayPools.Int32.Free(triangles);
                triangles = value;
            }
        }

        /// <summary>
        /// Vertex indices constituting the mesh's triangles
        /// </summary>
        /// <remarks>This getter returns a copy of the actual array. For performance reasons, use the equivalent getter returning a <see cref="SubArray{T}"/> instance, which allows you to directly access and modify the underlying array</remarks>
        [Obsolete("Use TrianglesList instead")]
        public int[] Triangles
        {
            get => TrianglesList.CopyToArray(ArrayPools.Int32);
            set => TrianglesList = new SubArray<int>(value);
        }

        public Material Material;
        private SubArray<int> triangles;

        public override int Count
        {
            get
            {
                return triangles.Count;
            }
        }

        public CGVSubMesh(Material material = null) : base()
        {
            Material = material;
            triangles = ArrayPools.Int32.Allocate(0);
        }

        public CGVSubMesh(int[] triangles, Material material = null) : base()
        {
            Material = material;
            this.triangles = new SubArray<int>(triangles);
        }

        public CGVSubMesh(SubArray<int> triangles, Material material = null) : base()
        {
            Material = material;
            this.triangles = triangles;
        }

        public CGVSubMesh(int triangleCount, Material material = null) : base()
        {
            Material = material;
            triangles = ArrayPools.Int32.Allocate(triangleCount);
        }

        public CGVSubMesh(CGVSubMesh source) : base()
        {
            Material = source.Material;
            triangles = ArrayPools.Int32.Clone(source.triangles);
        }

        protected override bool Dispose(bool disposing)
        {
            bool result = base.Dispose(disposing);
            if (result)
                ArrayPools.Int32.Free(triangles);
            return result;
        }

        public override T Clone<T>()
        {
            return new CGVSubMesh(this) as T;
        }

        public static CGVSubMesh Get(CGVSubMesh data, int triangleCount, Material material = null)
        {

            if (data == null)
                return new CGVSubMesh(triangleCount, material);

            ArrayPools.Int32.Resize(ref data.triangles, triangleCount);
            data.Material = material;
            return data;
        }

        public void ShiftIndices(int offset, int startIndex = 0)
        {
            for (int i = startIndex; i < triangles.Count; i++)
                triangles.Array[i] += offset;
        }

        public void Add(CGVSubMesh other, int shiftIndexOffset = 0)
        {
            int trianglesLength = triangles.Count;
            int otherTriangleLength = other.triangles.Count;

            if (otherTriangleLength == 0)
                return;

            ArrayPools.Int32.Resize(ref triangles, trianglesLength + otherTriangleLength);

            Array.Copy(other.triangles.Array, 0, triangles.Array, trianglesLength, otherTriangleLength);

            if (shiftIndexOffset != 0)
                ShiftIndices(shiftIndexOffset, trianglesLength);
        }
    }

    /// <summary>
    /// Mesh Data (Bounds + Vertex,UV,UV2,Normal,Tangents,SubMehes)
    /// </summary>
    [CGDataInfo(0.98f, 0.5f, 0)]
    public class CGVMesh : CGBounds
    {

#if CONTRACTS_FULL
        [ContractInvariantMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
            Contract.Invariant(Vertex != null);
            Contract.Invariant(UV != null);
            Contract.Invariant(UV2 != null);
            Contract.Invariant(Normal != null);
            Contract.Invariant(Tangents != null);
            Contract.Invariant(SubMeshes != null);

            Contract.Invariant(UV.Length == 0 || UV.Length == Vertex.Length);
            Contract.Invariant(UV2.Length == 0 || UV2.Length == Vertex.Length);
            Contract.Invariant(Normal.Length == 0 || Normal.Length == Vertex.Length);
            Contract.Invariant(Tangents.Length == 0 || Tangents.Length == Vertex.Length);
        }
#endif

        /// <summary>
        /// Positions of the points, in the local space
        /// </summary>
        /// <remarks>Setting a new <see cref="SubArray{T}"/> will <see cref="ArrayPool{T}.Free(ToolBuddy.Pooling.Collections.SubArray{T})"/> the current <see cref="SubArray{T}"/>  instance</remarks>
        /// <remarks>If you modify the content of the returned array, make sure to call <see cref="ClearCachedSortedVertexIndices"/> before calling <see cref="GetCachedSortedVertexIndices"/></remarks>
        public SubArray<Vector3> Vertices
        {
            get => vertices;
            set
            {
                ArrayPools.Vector3.Free(vertices);
                vertices = value;
                OnVerticesChanged();
            }
        }

        /// <summary>
        /// UVs of the points
        /// </summary>
        /// <remarks>Setting a new <see cref="SubArray{T}"/> will <see cref="ArrayPool{T}.Free(ToolBuddy.Pooling.Collections.SubArray{T})"/> the current <see cref="SubArray{T}"/>  instance</remarks>
        public SubArray<Vector2> UVs
        {
            get => uvs;
            set
            {
                ArrayPools.Vector2.Free(uvs);
                uvs = value;
            }
        }

        /// <summary>
        /// UV2s of the points
        /// </summary>
        /// <remarks>Setting a new <see cref="SubArray{T}"/> will <see cref="ArrayPool{T}.Free(ToolBuddy.Pooling.Collections.SubArray{T})"/> the current <see cref="SubArray{T}"/>  instance</remarks>
        public SubArray<Vector2> UV2s
        {
            get => uv2s;
            set
            {
                ArrayPools.Vector2.Free(uv2s);
                uv2s = value;
            }
        }

        /// <summary>
        /// Normals of the points, in the local space
        /// </summary>
        /// <remarks>Setting a new <see cref="SubArray{T}"/> will <see cref="ArrayPool{T}.Free(ToolBuddy.Pooling.Collections.SubArray{T})"/> the current <see cref="SubArray{T}"/>  instance</remarks>
        public SubArray<Vector3> NormalsList
        {
            get => normals;
            set
            {
                ArrayPools.Vector3.Free(normals);
                normals = value;
            }
        }

        /// <summary>
        /// Tangents of the points, in the local space
        /// </summary>
        /// <remarks>Setting a new <see cref="SubArray{T}"/> will <see cref="ArrayPool{T}.Free(ToolBuddy.Pooling.Collections.SubArray{T})"/> the current <see cref="SubArray{T}"/>  instance</remarks>
        public SubArray<Vector4> TangentsList
        {
            get => tangents;
            set
            {
                ArrayPools.Vector4.Free(tangents);
                tangents = value;
            }
        }

        #region Obsolete

        /// <summary>
        /// Positions of the points, in the local space
        /// </summary>
        /// <remarks>This getter returns a copy of the actual array. For performance reasons, use the equivalent getter returning a <see cref="SubArray{T}"/> instance, which allows you to directly access and modify the underlying array</remarks>
        [Obsolete("Use Vertices instead")]
        public Vector3[] Vertex
        {
            get => Vertices.CopyToArray(ArrayPools.Vector3);
            set => Vertices = new SubArray<Vector3>(value);
        }

        /// <summary>
        /// UVs of the points
        /// </summary>
        /// <remarks>This getter returns a copy of the actual array. For performance reasons, use the equivalent getter returning a <see cref="SubArray{T}"/> instance, which allows you to directly access and modify the underlying array</remarks>
        [Obsolete("Use UVs instead")]
        public Vector2[] UV
        {
            get => UVs.CopyToArray(ArrayPools.Vector2);
            set => UVs = new SubArray<Vector2>(value);
        }

        /// <summary>
        /// UV2s of the points
        /// </summary>
        /// <remarks>This getter returns a copy of the actual array. For performance reasons, use the equivalent getter returning a <see cref="SubArray{T}"/> instance, which allows you to directly access and modify the underlying array</remarks>
        [Obsolete("Use UV2s instead")]
        public Vector2[] UV2
        {
            get => UV2s.CopyToArray(ArrayPools.Vector2);
            set => UV2s = new SubArray<Vector2>(value);
        }


        /// <summary>
        /// Normals of the points, in the local space
        /// </summary>
        /// <remarks>This getter returns a copy of the actual array. For performance reasons, use the equivalent getter returning a <see cref="SubArray{T}"/> instance, which allows you to directly access and modify the underlying array</remarks>
        [Obsolete("Use NormalList instead")]
        public Vector3[] Normals
        {
            get => NormalsList.CopyToArray(ArrayPools.Vector3);
            set => NormalsList = new SubArray<Vector3>(value);
        }

        /// <summary>
        /// Tangents of the points, in the local space
        /// </summary>
        /// <remarks>This getter returns a copy of the actual array. For performance reasons, use the equivalent getter returning a <see cref="SubArray{T}"/> instance, which allows you to directly access and modify the underlying array</remarks>
        [Obsolete("Use TangentsList instead")]
        public Vector4[] Tangents
        {
            get => TangentsList.CopyToArray(ArrayPools.Vector4);
            set => TangentsList = new SubArray<Vector4>(value);
        }


        #endregion
        public CGVSubMesh[] SubMeshes;
        /// <summary>
        /// Gets the number of vertices
        /// </summary>
        public override int Count
        {
            get
            {
                return vertices.Count;
            }
        }

        public bool HasUV { get { return uvs.Count > 0; } }
        public bool HasUV2 { get { return uv2s.Count > 0; } }
        /// <summary>
        /// True if at least one vertex has a normal
        /// </summary>
        public bool HasNormals { get { return normals.Count > 0; } }

        /// <summary>
        /// True if <see cref="HasNormals"/> but not all vertices have normals
        /// </summary>
        public bool HasPartialNormals
        {
            get
            {
#if CURVY_SANITY_CHECKS
                Assert.IsTrue(hasPartialNormals == false || HasNormals);
#endif
                return hasPartialNormals;
            }
            private set => hasPartialNormals = value;
        }

        /// <summary>
        /// True if at least one vertex has a tangent
        /// </summary>
        public bool HasTangents { get { return tangents.Count > 0; } }

        /// <summary>
        /// True if <see cref="HasTangents"/> but not all vertices have tangents
        /// </summary>
        public bool HasPartialTangents
        {
            get
            {
#if CURVY_SANITY_CHECKS
                Assert.IsTrue(hasPartialTangents == false || HasTangents);
#endif
                return hasPartialTangents;
            }
            private set => hasPartialTangents = value;
        }

        public int TriangleCount
        {
            get
            {
                int cnt = 0;
                for (int i = 0; i < SubMeshes.Length; i++)
                    cnt += SubMeshes[i].TrianglesList.Count;
                return cnt / 3;
            }
        }

        #region Private fields

        /// <summary>
        /// An array of the index of vertices when sorted by Z coordinate, from smaller to bigger
        /// </summary>
        private SubArray<int>? sortedVertexIndices;
        /// <summary>
        /// Lock used when generating <see cref="sortedVertexIndices"/>
        /// </summary>
        private readonly object vertexIndicesLock = new object();

        private SubArray<Vector3> vertices;

        private SubArray<Vector2> uvs;

        private SubArray<Vector2> uv2s;

        private SubArray<Vector3> normals;

        private SubArray<Vector4> tangents;

        private bool hasPartialNormals;

        private bool hasPartialTangents;

        #endregion
        public CGVMesh() : this(0) { }
        public CGVMesh(int vertexCount, bool addUV = false, bool addUV2 = false, bool addNormals = false, bool addTangents = false) : base()
        {
            vertices = ArrayPools.Vector3.Allocate(vertexCount);
            uvs = addUV
                ? ArrayPools.Vector2.Allocate(vertexCount)
                : ArrayPools.Vector2.Allocate(0);
            uv2s = addUV2
                ? ArrayPools.Vector2.Allocate(vertexCount)
                : ArrayPools.Vector2.Allocate(0);
            normals = addNormals
                ? ArrayPools.Vector3.Allocate(vertexCount)
                : ArrayPools.Vector3.Allocate(0);
            tangents = addTangents
                ? ArrayPools.Vector4.Allocate(vertexCount)
                : ArrayPools.Vector4.Allocate(0);
            hasPartialNormals = false;
            hasPartialTangents = false;
            SubMeshes = new CGVSubMesh[0];
        }
        public CGVMesh(CGVolume volume) : this(volume.Vertices.Count)
        {
            Array.Copy(volume.Vertices.Array, 0, vertices.Array, 0, volume.Vertices.Count);
        }

        public CGVMesh(CGVolume volume, IntRegion subset)
            : this((subset.LengthPositive + 1) * volume.CrossSize, false, false, true)
        {
            int start = subset.Low * volume.CrossSize;
            Array.Copy(volume.Vertices.Array, start, vertices.Array, 0, vertices.Count);
            Array.Copy(volume.VertexNormals.Array, start, normals.Array, 0, normals.Count);
        }

        public CGVMesh(CGVMesh source) : base(source)
        {
            vertices = ArrayPools.Vector3.Clone(source.vertices);
            uvs = ArrayPools.Vector2.Clone(source.uvs);
            uv2s = ArrayPools.Vector2.Clone(source.uv2s);
            normals = ArrayPools.Vector3.Clone(source.normals);
            tangents = ArrayPools.Vector4.Clone(source.tangents);
            hasPartialNormals = source.HasPartialNormals;
            hasPartialTangents = source.HasPartialTangents;
            SubMeshes = new CGVSubMesh[source.SubMeshes.Length];
            for (int i = 0; i < source.SubMeshes.Length; i++)
                SubMeshes[i] = new CGVSubMesh(source.SubMeshes[i]);
        }

        public CGVMesh(CGMeshProperties meshProperties) : this(meshProperties.Mesh, meshProperties.Material, meshProperties.Matrix) { }

        public CGVMesh(Mesh source, Material[] materials, Matrix4x4 trsMatrix) : base()
        {
            Name = source.name;
            vertices = new SubArray<Vector3>(source.vertices);
            normals = new SubArray<Vector3>(source.normals);
            tangents = new SubArray<Vector4>(source.tangents);
            hasPartialNormals = false;
            hasPartialTangents = false;
            uvs = new SubArray<Vector2>(source.uv);
            uv2s = new SubArray<Vector2>(source.uv2);
            SubMeshes = new CGVSubMesh[source.subMeshCount];
            for (int s = 0; s < source.subMeshCount; s++)
                SubMeshes[s] = new CGVSubMesh(source.GetTriangles(s), (materials.Length > s) ? materials[s] : null);

            Bounds = source.bounds;

            if (!trsMatrix.isIdentity)
                TRS(trsMatrix);

        }

        protected override bool Dispose(bool disposing)
        {
            bool result = base.Dispose(disposing);
            if (result)
            {
                if (sortedVertexIndices != null)
                    ArrayPools.Int32.Free(sortedVertexIndices.Value);
                ArrayPools.Vector3.Free(vertices);
                ArrayPools.Vector2.Free(uvs);
                ArrayPools.Vector2.Free(uv2s);
                ArrayPools.Vector3.Free(normals);
                ArrayPools.Vector4.Free(tangents);

                //Do not dispose SubMeshes if the call is due to finalization, since submeshes are disposable by themselves.
                if (disposing)
                    for (var i = 0; i < SubMeshes.Length; i++)
                        SubMeshes[i].Dispose();
            }
            return result;
        }

        public override T Clone<T>()
        {
            return new CGVMesh(this) as T;
        }

        [Obsolete("Member not used by Curvy, will get removed next major version. Use another overload of this method")]
        public static CGVMesh Get(CGVMesh data, CGVolume source, bool addUV, bool reverseNormals)
        {
            return Get(data, source, new IntRegion(0, source.Count - 1), addUV, reverseNormals);
        }

        [Obsolete("Member not used by Curvy, will get removed next major version. Use another overload of this method")]
        public static CGVMesh Get(CGVMesh data, CGVolume source, IntRegion subset, bool addUV, bool reverseNormals)
        {
            return Get(data, source, subset, addUV, false, reverseNormals);
        }

        public static CGVMesh Get(CGVMesh data, CGVolume source, IntRegion subset, bool addUV, bool addUV2, bool reverseNormals)
        {
            int start = subset.Low * source.CrossSize;
            int size = (subset.LengthPositive + 1) * source.CrossSize;

            if (data == null)
                data = new CGVMesh(size, addUV, addUV2, true);
            else
            {
                if (data.vertices.Count != size)
                    ArrayPools.Vector3.Resize(ref data.vertices, size, false);

                if (data.normals.Count != size)
                    ArrayPools.Vector3.Resize(ref data.normals, size, false);

                int uvSize = (addUV) ? size : 0;
                if (data.uvs.Count != uvSize)
                    ArrayPools.Vector2.ResizeAndClear(ref data.uvs, uvSize);

                int uv2Size = (addUV2) ? size : 0;
                if (data.uv2s.Count != uv2Size)
                    ArrayPools.Vector2.ResizeAndClear(ref data.uv2s, uv2Size);

                //data.SubMeshes = new CGVSubMesh[0];//BUG? why is this commented?

                if (data.tangents.Count != 0)
                    ArrayPools.Vector4.Resize(ref data.tangents, 0);
                data.HasPartialTangents = false;
            }

            Array.Copy(source.Vertices.Array, start, data.vertices.Array, 0, size);
            Array.Copy(source.VertexNormals.Array, start, data.normals.Array, 0, size);
            data.hasPartialNormals = false;

            if (reverseNormals)
            {
                Vector3[] normalsArray = data.normals.Array;

                //OPTIM merge loop with normals copy
                for (int n = 0; n < data.normals.Count; n++)
                {
                    normalsArray[n].x = -normalsArray[n].x;
                    normalsArray[n].y = -normalsArray[n].y;
                    normalsArray[n].z = -normalsArray[n].z;
                }
            }

            data.OnVerticesChanged();

            return data;
        }


        public void SetSubMeshCount(int count)
        {
            Array.Resize(ref SubMeshes, count);
        }

        public void AddSubMesh(CGVSubMesh submesh = null)
        {
            SubMeshes = SubMeshes.Add(submesh);
        }

        /// <summary>
        /// Combine/Merge another VMesh into this
        /// </summary>
        /// <param name="source"></param>
        public void MergeVMesh(CGVMesh source) => MergeVMesh(source, Matrix4x4.identity);

        /// <summary>
        /// Combine/Merge another VMesh into this, applying a matrix
        /// </summary>
        /// <param name="source"></param>
        /// <param name="matrix"></param>
        public void MergeVMesh(CGVMesh source, Matrix4x4 matrix)
        {
            //TODO Design: unify implementation with MergeVMeshes
            int preMergeVertexCount = Count;
            // Add base data
            if (source.Count != 0)
            {
                int postMergeVertexCount = preMergeVertexCount + source.Count;
                ArrayPools.Vector3.Resize(ref vertices, postMergeVertexCount);
                if (matrix == Matrix4x4.identity)
                    Array.Copy(source.vertices.Array, 0, vertices.Array, preMergeVertexCount, source.Count);
                else
                    for (int v = preMergeVertexCount; v < postMergeVertexCount; v++)
                        vertices.Array[v] = matrix.MultiplyPoint3x4(source.vertices.Array[v - preMergeVertexCount]);

                MergeUVsNormalsAndTangents(source, preMergeVertexCount);

                // Add Submeshes
                for (int sm = 0; sm < source.SubMeshes.Length; sm++)
                    GetMaterialSubMesh(source.SubMeshes[sm].Material).Add(source.SubMeshes[sm], preMergeVertexCount);

                OnVerticesChanged();
            }
        }

        /// <summary>
        /// Combine/Merge multiple CGVMeshes into this
        /// </summary>
        /// <param name="vMeshes">list of CGVMeshes</param>
        /// <param name="startIndex">Index of the first element of the list to merge</param>
        /// <param name="endIndex">Index of the last element of the list to merge</param>
        public void MergeVMeshes(List<CGVMesh> vMeshes, int startIndex, int endIndex)
        {
            Assert.IsTrue(endIndex < vMeshes.Count);
            int totalVertexCount = 0;
            bool hasNormals = false;
            bool partialNormals = false;
            bool hasTangents = false;
            bool partialTangents = false;
            bool hasUV = false;
            bool hasUV2 = false;
            Dictionary<Material, List<SubArray<int>>> submeshesByMaterial = new Dictionary<Material, List<SubArray<int>>>();
            Dictionary<Material, int> trianglesIndexPerMaterial = new Dictionary<Material, int>();
            //dictionaries can't have null as a key, so to handle null materials, here are the fields equivalent to these dictionaries for null
            List<SubArray<int>> noMaterialSubmeshes = null;
            int noMaterialTrianglesIndex = 0;

            for (int i = startIndex; i <= endIndex; i++)
            {
                CGVMesh cgvMesh = vMeshes[i];
                totalVertexCount += cgvMesh.Count;
                hasNormals |= cgvMesh.HasNormals;
                partialNormals |= cgvMesh.HasNormals == false || cgvMesh.HasPartialNormals;
                hasTangents |= cgvMesh.HasTangents;
                partialTangents |= cgvMesh.HasTangents == false || cgvMesh.hasPartialTangents;
                hasUV |= cgvMesh.HasUV;
                hasUV2 |= cgvMesh.HasUV2;

                //initialize per material data
                for (int sm = 0; sm < cgvMesh.SubMeshes.Length; sm++)
                {
                    CGVSubMesh subMesh = cgvMesh.SubMeshes[sm];
                    if (subMesh.Material != null)
                    {
                        Material subMeshMaterial = subMesh.Material;
                        if (submeshesByMaterial.ContainsKey(subMeshMaterial) == false)
                        {
                            submeshesByMaterial[subMeshMaterial] = new List<SubArray<int>>(1);
                            trianglesIndexPerMaterial[subMeshMaterial] = 0;
                        }
                        submeshesByMaterial[subMeshMaterial].Add(subMesh.TrianglesList);
                    }
                    else
                    {
                        if(noMaterialSubmeshes == null)
                        {
                            noMaterialSubmeshes = new List<SubArray<int>>(1);
                            noMaterialTrianglesIndex = 0;
                        }
                        noMaterialSubmeshes.Add(subMesh.TrianglesList);
                    }

                }
            }

            ArrayPools.Vector3.Resize(ref vertices, totalVertexCount);
            if (hasNormals)
                ArrayPools.Vector3.Resize(ref normals, totalVertexCount);
            hasPartialNormals = partialNormals;

            if (hasTangents)
                ArrayPools.Vector4.Resize(ref tangents, totalVertexCount);
            hasPartialTangents = partialTangents;

            if (hasUV)
                ArrayPools.Vector2.Resize(ref uvs, totalVertexCount);

            if (hasUV2)
                ArrayPools.Vector2.Resize(ref uv2s, totalVertexCount);

            foreach (KeyValuePair<Material, List<SubArray<int>>> pair in submeshesByMaterial)
                ProcessTriangleArrays(pair.Value, pair.Key);
            if(noMaterialSubmeshes != null)
                ProcessTriangleArrays(noMaterialSubmeshes, null);

            int currentVertexCount = 0;
            for (int i = startIndex; i <= endIndex; i++)
            {
                CGVMesh source = vMeshes[i];

                Array.Copy(source.vertices.Array, 0, vertices.Array, currentVertexCount, source.vertices.Count);
                if (hasNormals)
                {
                    if (source.HasNormals)
                        Array.Copy(source.normals.Array, 0, normals.Array, currentVertexCount, source.normals.Count);
                    else
                        Array.Clear(normals.Array, currentVertexCount, source.vertices.Count);
                }

                if (hasTangents)
                {
                    if (source.HasTangents)
                        Array.Copy(source.tangents.Array, 0, tangents.Array, currentVertexCount, source.tangents.Count);
                    else
                        Array.Clear(tangents.Array, currentVertexCount, source.vertices.Count);
                }

                if (hasUV)
                {
                    if (source.HasUV)
                        Array.Copy(source.uvs.Array, 0, uvs.Array, currentVertexCount, source.uvs.Count);
                    else
                        Array.Clear(uvs.Array, currentVertexCount, source.vertices.Count);
                }

                if (hasUV2)
                {
                    if (source.HasUV2)
                        Array.Copy(source.uv2s.Array, 0, uv2s.Array, currentVertexCount, source.uv2s.Count);
                    else
                        Array.Clear(uv2s.Array, currentVertexCount, source.vertices.Count);
                }

                // Add Submeshes
                for (int subMeshIndex = 0; subMeshIndex < source.SubMeshes.Length; subMeshIndex++)
                {
                    CGVSubMesh sourceSubMesh = source.SubMeshes[subMeshIndex];
                    Material sourceMaterial = sourceSubMesh.Material;
                    SubArray<int> sourceTriangles = sourceSubMesh.TrianglesList;
                    int sourceTrianglesLength = sourceTriangles.Count;

                    SubArray<int> destinationTriangles = GetMaterialSubMesh(sourceMaterial).TrianglesList;

                    int trianglesIndex = sourceMaterial == null ? noMaterialTrianglesIndex: trianglesIndexPerMaterial[sourceMaterial];

                    if (sourceTrianglesLength != 0)
                    {
                        if (currentVertexCount == 0)
                            Array.Copy(sourceTriangles.Array, 0, destinationTriangles.Array, trianglesIndex, sourceTrianglesLength);
                        else
                            for (int j = 0; j < sourceTrianglesLength; j++)
                                destinationTriangles.Array[trianglesIndex + j] = sourceTriangles.Array[j] + currentVertexCount;

                        int materialTrianglesIndex = trianglesIndex + sourceTrianglesLength;

                        if (sourceMaterial == null)
                            noMaterialTrianglesIndex = materialTrianglesIndex;
                        else
                            trianglesIndexPerMaterial[sourceMaterial] = materialTrianglesIndex;
                    }
                }
                currentVertexCount += source.vertices.Count;
            }

            OnVerticesChanged();

            void ProcessTriangleArrays(List<SubArray<int>> subArrays, Material material1)
            {
                int totalTrianglesCount = 0;
                for (int arraysIndex = 0; arraysIndex < subArrays.Count; arraysIndex++)
                    totalTrianglesCount += subArrays[arraysIndex].Count;

                AddSubMesh(new CGVSubMesh(totalTrianglesCount, material1));
            }
        }

        private void MergeUVsNormalsAndTangents(CGVMesh source, int preMergeVertexCount)
        {
            int sourceLength = source.Count;
            if (sourceLength == 0)
                return;

            int postMergeVetexCount = preMergeVertexCount + sourceLength;
            if (HasUV || source.HasUV)
            {
                SubArray<Vector2> newUVs = ArrayPools.Vector2.Allocate(postMergeVetexCount, false);

                if (HasUV)
                    Array.Copy(uvs.Array, 0, newUVs.Array, 0, preMergeVertexCount);
                else
                    Array.Clear(newUVs.Array, 0, preMergeVertexCount);

                if (source.HasUV)
                    Array.Copy(source.uvs.Array, 0, newUVs.Array, preMergeVertexCount, sourceLength);
                else
                    Array.Clear(newUVs.Array, preMergeVertexCount, sourceLength);

                UVs = newUVs;

            }

            if (HasUV2 || source.HasUV2)
            {
                SubArray<Vector2> newUV2s = ArrayPools.Vector2.Allocate(postMergeVetexCount, false);

                if (HasUV2)
                    Array.Copy(uv2s.Array, 0, newUV2s.Array, 0, preMergeVertexCount);
                else
                    Array.Clear(newUV2s.Array, 0, preMergeVertexCount);

                if (source.HasUV2)
                    Array.Copy(source.uv2s.Array, 0, newUV2s.Array, preMergeVertexCount, sourceLength);
                else
                    Array.Clear(newUV2s.Array, preMergeVertexCount, sourceLength);

                UV2s = newUV2s;

            }

            if (HasNormals || source.HasNormals)
            {
                HasPartialNormals = HasNormals ^ source.HasNormals;

                SubArray<Vector3> newNormals = ArrayPools.Vector3.Allocate(postMergeVetexCount, false);

                if (HasNormals)
                    Array.Copy(normals.Array, 0, newNormals.Array, 0, preMergeVertexCount);
                else
                    Array.Clear(newNormals.Array, 0, preMergeVertexCount);

                if (source.HasNormals)
                    Array.Copy(source.normals.Array, 0, newNormals.Array, preMergeVertexCount, sourceLength);
                else
                    Array.Clear(newNormals.Array, preMergeVertexCount, sourceLength);

                NormalsList = newNormals;
            }

            if (HasTangents || source.HasTangents)
            {
                HasPartialTangents = HasTangents ^ source.HasTangents;

                SubArray<Vector4> newTangents = ArrayPools.Vector4.Allocate(postMergeVetexCount, false);

                if (HasTangents)
                    Array.Copy(tangents.Array, 0, newTangents.Array, 0, preMergeVertexCount);
                else
                    Array.Clear(newTangents.Array, 0, preMergeVertexCount);

                if (source.HasTangents)
                    Array.Copy(source.tangents.Array, 0, newTangents.Array, preMergeVertexCount, sourceLength);
                else
                    Array.Clear(newTangents.Array, preMergeVertexCount, sourceLength);

                TangentsList = newTangents;
            }
        }

        /// <summary>
        /// Gets the submesh using a certain material
        /// </summary>
        /// <param name="mat">the material the submesh should use</param>
        /// <param name="createIfMissing">whether to create the submesh if no existing one matches</param>
        /// <returns>a submesh using the given material</returns>
        public CGVSubMesh GetMaterialSubMesh(Material mat, bool createIfMissing = true)
        {
            // already having submesh with matching material?
            for (int sm = 0; sm < SubMeshes.Length; sm++)
                if (SubMeshes[sm].Material == mat)
                    return SubMeshes[sm];

            // else create new
            if (createIfMissing)
            {
                CGVSubMesh sm = new CGVSubMesh(mat);
                AddSubMesh(sm);
                return sm;
            }
            else
                return null;
        }

        /// <summary>
        /// Creates a Mesh from the data
        /// </summary>
        public Mesh AsMesh()
        {
            Mesh msh = new Mesh();
            ToMesh(ref msh);
            return msh;
        }

        /// <summary>
        /// Copies the data into an existing Mesh
        /// </summary>
        /// <param name="mesh">The mesh to copy the data from this CGVMesh into</param>
        /// <param name="includeNormals">should normals be copied or set to empty?</param>
        /// <param name="includeTangents">should tangents be copied or set to empty?</param>
        public void ToMesh(ref Mesh mesh, bool includeNormals = true, bool includeTangents = true)
        {
            mesh.indexFormat = Count >= UInt16.MaxValue ? IndexFormat.UInt32 : IndexFormat.UInt16;

            mesh.SetVertices(vertices.Array, 0, vertices.Count);
            mesh.SetUVs(0, uvs.Array, 0, HasUV ? uvs.Count : 0);
            mesh.SetUVs(1, uv2s.Array, 0, HasUV2 ? uv2s.Count : 0);
            mesh.SetNormals(normals.Array, 0, (includeNormals && HasNormals) ? normals.Count : 0);
            mesh.SetTangents(tangents.Array, 0, (includeTangents && HasTangents) ? tangents.Count : 0);

            mesh.subMeshCount = SubMeshes.Length;
            for (int s = 0; s < SubMeshes.Length; s++)
            {
                SubArray<int> subArray = SubMeshes[s].TrianglesList;
                mesh.SetTriangles(subArray.Array, 0, subArray.Count, s);
            }
        }

        /// <summary>
        /// Gets a list of all Materials used
        /// </summary>
        public Material[] GetMaterials()
        {
            List<Material> mats = new List<Material>();
            for (int s = 0; s < SubMeshes.Length; s++)
                mats.Add(SubMeshes[s].Material);
            return mats.ToArray();
        }

        public override void RecalculateBounds()
        {
            if (Count == 0)
            {
                mBounds = new Bounds(Vector3.zero, Vector3.zero);
            }
            else
            {
                int vertexCount = vertices.Count;
                Vector3 min = vertices.Array[0], max = vertices.Array[0];
                for (int i = 1; i < vertexCount; i++)
                {
                    Vector3 vertex = vertices.Array[i];

                    if (vertex.x < min.x)
                        min.x = vertex.x;
                    else if (vertex.x > max.x)
                        max.x = vertex.x;

                    if (vertex.y < min.y)
                        min.y = vertex.y;
                    else if (vertex.y > max.y)
                        max.y = vertex.y;

                    if (vertex.z < min.z)
                        min.z = vertex.z;
                    else if (vertex.z > max.z)
                        max.z = vertex.z;
                }

                Bounds bounds = new Bounds();
                bounds.SetMinMax(min, max);
                mBounds = bounds;
            }
        }

        [Obsolete("Method will get remove in next major update. Copy its content if you need it")]
        public void RecalculateUV2()
        {
            ArrayPools.Vector2.Resize(ref uv2s, UVs.Count);
            CGUtility.CalculateUV2(uvs.Array, uv2s.Array, uvs.Count);
        }

        /// <summary>
        /// Applies the translation, rotation and scale defined by the given matrix
        /// </summary>
        public void TRS(Matrix4x4 matrix)
        {
            int count = Count;
            for (int vertexIndex = 0; vertexIndex < count; vertexIndex++)
                vertices.Array[vertexIndex] = matrix.MultiplyPoint3x4(vertices.Array[vertexIndex]);

            count = normals.Count;
            for (int vertexIndex = 0; vertexIndex < count; vertexIndex++)
                normals.Array[vertexIndex] = matrix.MultiplyVector(normals.Array[vertexIndex]);

            count = tangents.Count;
            for (int vertexIndex = 0; vertexIndex < count; vertexIndex++)
            {
                //Keep in mind that Tangents is a Vector4 array
                Vector4 tangent4 = tangents.Array[vertexIndex];
                Vector3 tangent3;
                tangent3.x = tangent4.x;
                tangent3.y = tangent4.y;
                tangent3.z = tangent4.z;
                tangents.Array[vertexIndex] = matrix.MultiplyVector(tangent3);
            }

            OnVerticesChanged();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OnVerticesChanged()
        {
            mBounds = null;
            ClearCachedSortedVertexIndices();
        }

        /// <summary>
        /// Gets an array of the index of vertices when sorted by Z coordinate, from smaller to bigger.
        /// This array is cached. Curvy Splines does clear this cache if it modifies <see cref="Vertices"/>, but if you modify <see cref="Vertices"/> through its getter, you will have to clear the cached value by calling <see cref="ClearCachedSortedVertexIndices"/>
        /// </summary>
        /// <remarks>Is thread safe</remarks>
        public SubArray<int> GetCachedSortedVertexIndices()
        {
            if (sortedVertexIndices == null)
            {
                lock (vertexIndicesLock)
                {
                    if (sortedVertexIndices == null)
                    {
                        int verticesCount = vertices.Count;

                        SubArray<int> result = ArrayPools.Int32.Allocate(verticesCount);
                        SubArray<float> verticesZ = ArrayPools.Single.Allocate(verticesCount);
                        for (int k = 0; k < verticesCount; k++)
                        {
                            result.Array[k] = k;
                            verticesZ.Array[k] = vertices.Array[k].z;
                        }

                        Array.Sort(verticesZ.Array, result.Array, 0, verticesCount);
                        ArrayPools.Single.Free(verticesZ);

                        sortedVertexIndices = result;
                    }
                }
            }

#if CURVY_SANITY_CHECKS
            Assert.IsTrue(sortedVertexIndices.Value.Count == Vertices.Count);
#endif
            return sortedVertexIndices.Value;
        }

        /// <summary>
        /// Clears the cached value computed by <see cref="GetCachedSortedVertexIndices"/>
        /// </summary>
        /// <remarks>Is thread safe</remarks>
        private void ClearCachedSortedVertexIndices()
        {
            if (sortedVertexIndices != null)
                lock (vertexIndicesLock)
                {
                    if (sortedVertexIndices != null)
                    {
                        ArrayPools.Int32.Free(sortedVertexIndices.Value);
                        sortedVertexIndices = null;
                    }
                }
        }
    }

    /// <summary>
    /// GameObject data (Bounds + Object)
    /// </summary>
    [CGDataInfo("#FFF59D")]
    public class CGGameObject : CGBounds
    {
        public GameObject Object;
        public Vector3 Translate;
        public Vector3 Rotate;
        public Vector3 Scale = Vector3.one;

        public Matrix4x4 Matrix
        {
            get { return Matrix4x4.TRS(Translate, Quaternion.Euler(Rotate), Scale); }
        }

        public CGGameObject() : base() { }

        public CGGameObject(CGGameObjectProperties properties) : this(properties.Object, properties.Translation, properties.Rotation, properties.Scale) { }

        public CGGameObject(GameObject obj) : this(obj, Vector3.zero, Vector3.zero, Vector3.one) { }

        public CGGameObject(GameObject obj, Vector3 translate, Vector3 rotate, Vector3 scale)
            : base()
        {
            Object = obj;
            Translate = translate;
            Rotate = rotate;
            Scale = scale;
            if (Object)
                Name = Object.name;
        }

        public CGGameObject(CGGameObject source) : base(source)
        {
            Object = source.Object;
            Translate = source.Translate;
            Rotate = source.Rotate;
            Scale = source.Scale;
        }

        public override T Clone<T>()
        {
            return new CGGameObject(this) as T;
        }

        public override void RecalculateBounds()
        {
            if (Object == null)
            {
                mBounds = new Bounds();
            }
            else
            {
                Renderer[] renderer = Object.GetComponentsInChildren<Renderer>(true);
                Collider[] collider = Object.GetComponentsInChildren<Collider>(true);
                Bounds bounds;
                if (renderer.Length > 0)
                {
                    bounds = renderer[0].bounds;
                    for (int i = 1; i < renderer.Length; i++)
                        bounds.Encapsulate(renderer[i].bounds);
                    for (int i = 0; i < collider.Length; i++)
                        bounds.Encapsulate(collider[i].bounds);
                }
                else if (collider.Length > 0)
                {
                    bounds = collider[0].bounds;
                    for (int i = 1; i < collider.Length; i++)
                        bounds.Encapsulate(collider[i].bounds);
                }
                else
                    bounds = new Bounds();

                Vector3 rotationlessBoundsSize = (Quaternion.Inverse(Object.transform.localRotation) * bounds.size);
                bounds.size = new Vector3(
                    rotationlessBoundsSize.x * Scale.x,
                    rotationlessBoundsSize.y * Scale.y,
                    rotationlessBoundsSize.z * Scale.z);

                mBounds = bounds;
            }
        }
    }

    /// <summary>
    /// A collection of <see cref="CGSpot"/>
    /// </summary>
    [CGDataInfo(0.96f, 0.96f, 0.96f)]
    public class CGSpots : CGData
    {
        //DESIGN what is the use of this class? Seems to me like a complicated way to represent an array

        /// <summary>
        /// List of spots
        /// </summary>
        /// <remarks>Setting a new <see cref="SubArray{T}"/> will <see cref="ArrayPool{T}.Free(ToolBuddy.Pooling.Collections.SubArray{T})"/> the current <see cref="SubArray{T}"/>  instance</remarks>

        public SubArray<CGSpot> Spots
        {
            get => spots;
            set
            {
                ArrayPools.CGSpot.Free(spots);
                spots = value;
            }
        }

        /// <summary>
        /// List of spots
        /// </summary>
        /// <remarks>This getter returns a copy of the actual array. For performance reasons, use the equivalent getter returning a <see cref="SubArray{T}"/> instance, which allows you to directly access and modify the underlying array</remarks>
        [Obsolete("Use Spots instead")]
        public CGSpot[] Points
        {
            get => Spots.CopyToArray(ArrayPools.CGSpot);
            set => Spots = new SubArray<CGSpot>(value);
        }

        private SubArray<CGSpot> spots;

        public override int Count
        {
            get
            {
                return spots.Count;
            }
        }

        public CGSpots() : base()
        {
            spots = ArrayPools.CGSpot.Allocate(0);
        }

        public CGSpots(params CGSpot[] points) : base()
        {
            spots = new SubArray<CGSpot>(points);
        }

        public CGSpots(SubArray<CGSpot> spots) : base()
        {
            this.spots = spots;
        }

        public CGSpots(List<CGSpot> spots) : base()
        {
            this.spots = ArrayPools.CGSpot.Allocate(spots.Count);
            spots.CopyTo(0, this.spots.Array, 0, spots.Count);
        }

        public CGSpots(params List<CGSpot>[] spots) : base()
        {
            int c = 0;
            for (int i = 0; i < spots.Length; i++)
                c += spots[i].Count;
            this.spots = ArrayPools.CGSpot.Allocate(c);
            c = 0;
            for (int i = 0; i < spots.Length; i++)
            {
                List<CGSpot> cgSpots = spots[i];
                cgSpots.CopyTo(0, this.spots.Array, c, cgSpots.Count);
                c += cgSpots.Count;
            }
        }

        public CGSpots(CGSpots source) : base()
        {
            spots = ArrayPools.CGSpot.Clone(source.spots);
        }
        protected override bool Dispose(bool disposing)
        {
            bool result = base.Dispose(disposing);
            if (result)
                ArrayPools.CGSpot.Free(spots);
            return result;
        }

        public override T Clone<T>()
        {
            return new CGSpots(this) as T;
        }
    }
}