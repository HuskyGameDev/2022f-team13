// =====================================================================
// Copyright 2013-2022 ToolBuddy
// All rights reserved
// 
// http://www.toolbuddy.net
// =====================================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using FluffyUnderware.Curvy.Pools;
using ToolBuddy.Pooling.Collections;
using ToolBuddy.Pooling.Pools;

#if CONTRACTS_FULL
using System.Diagnostics.Contracts;
#endif


namespace FluffyUnderware.Curvy.Generator
{
    /// <summary>
    /// Request Parameter base class
    /// </summary>
    public abstract class CGDataRequestParameter
    {
        public static implicit operator bool(CGDataRequestParameter a)
        {
            return !ReferenceEquals(a, null);
        }
    }

    /// <summary>
    /// Additional Spline Request parameters
    /// </summary>
    public class CGDataRequestMetaCGOptions : CGDataRequestParameter
    {
        /// <summary>
        /// Whether Hard Edges should produce extra samples
        /// </summary>
        /// <remarks>This may result in extra samples at affected Control Points</remarks>
        [Obsolete("This option is now always assumed to be true")]
        public bool CheckHardEdges;
        /// <summary>
        /// Whether MaterialID's should be stored
        /// </summary>
        /// <remarks>This may result in extra samples at affected Control Points</remarks>
        [Obsolete("This option is now always assumed to be true")]
        public bool CheckMaterialID;
        /// <summary>
        /// Whether all Control Points should be included
        /// </summary>
        public bool IncludeControlPoints;
        /// <summary>
        /// Whether UVEdge, ExplicitU and custom U settings should be included
        /// </summary>
        [Obsolete("This option is now always assumed to be true")]
        public bool CheckExtendedUV;


        public CGDataRequestMetaCGOptions(bool checkEdges, bool checkMaterials, bool includeCP, bool extendedUV)
        {
#pragma warning disable 618
            CheckHardEdges = checkEdges;
#pragma warning restore 618
#pragma warning disable 618
            CheckMaterialID = checkMaterials;
#pragma warning restore 618
            IncludeControlPoints = includeCP;
#pragma warning disable 618
            CheckExtendedUV = extendedUV;
#pragma warning restore 618
        }

        public override bool Equals(object obj)
        {
            CGDataRequestMetaCGOptions O = obj as CGDataRequestMetaCGOptions;
            if (O == null)
                return false;
#pragma warning disable 618
            return (CheckHardEdges == O.CheckHardEdges && CheckMaterialID == O.CheckMaterialID && IncludeControlPoints == O.IncludeControlPoints && CheckExtendedUV == O.CheckExtendedUV);
#pragma warning restore 618
        }

        public override int GetHashCode()
        {
#pragma warning disable 618
            return new { A = CheckHardEdges, B = CheckMaterialID, C = IncludeControlPoints, D = CheckExtendedUV }.GetHashCode(); //OPTIM avoid array creation
#pragma warning restore 618
        }

        public override string ToString()
        {
#pragma warning disable 618
            return $"{nameof(CheckHardEdges)}: {CheckHardEdges}, {nameof(CheckMaterialID)}: {CheckMaterialID}, {nameof(IncludeControlPoints)}: {IncludeControlPoints}, {nameof(CheckExtendedUV)}: {CheckExtendedUV}";
#pragma warning restore 618
        }
    }

    /// <summary>
    /// Shape Rasterization Request parameters
    /// </summary>
    public class CGDataRequestShapeRasterization : CGDataRequestRasterization
    {
        /// <summary>
        /// The <see cref="CGShape.RelativeDistances"/> array of the <see cref="CGPath"/> instance used for the shape extrusion that requests the current Shape rasterization
        /// </summary>
        /// <remarks>Setting a new <see cref="SubArray{T}"/> will <see cref="ArrayPool{T}.Free(ToolBuddy.Pooling.Collections.SubArray{T})"/> the current <see cref="SubArray{T}"/>  instance</remarks>
        public SubArray<float> RelativeDistances
        {
            get => relativeDistances;
            set => relativeDistances = value;
        }

        /// <summary>
        /// The <see cref="CGShape.RelativeDistances"/> array of the <see cref="CGPath"/> instance used for the shape extrusion that requests the current Shape rasterization
        /// </summary>
        /// <remarks>This getter returns a copy of the actual array. For performance reasons, use the equivalent getter returning a <see cref="SubArray{T}"/> instance, which allows you to directly access and modify the underlying array</remarks>
        [Obsolete("Use RelativeDistances instead")]
        public float[] PathF
        {
            get => RelativeDistances.CopyToArray(ArrayPools.Single);
            set => RelativeDistances = new SubArray<float>(value);
        }

        private SubArray<float> relativeDistances;

        public CGDataRequestShapeRasterization(SubArray<float> relativeDistance, float start, float rasterizedRelativeLength, int resolution, float angle, ModeEnum mode = ModeEnum.Even) : base(start, rasterizedRelativeLength, resolution, angle, mode)
        {
            relativeDistances = ArrayPools.Single.Clone(relativeDistance);
        }

        [Obsolete("Use another constructor instead")]
        public CGDataRequestShapeRasterization(float[] pathF, float start, float rasterizedRelativeLength, int resolution, float angle, ModeEnum mode = ModeEnum.Even) : base(start, rasterizedRelativeLength, resolution, angle, mode)
        {
            relativeDistances = ArrayPools.Single.Clone(pathF);
        }

        public override bool Equals(object obj)
        {
            CGDataRequestShapeRasterization other = obj as CGDataRequestShapeRasterization;
            if (other == null)
                return false;

            if (!base.Equals(obj) || other.relativeDistances.Count != relativeDistances.Count)
                return false;

            for (var i = 0; i < relativeDistances.Count; i++)
            {
                if (other.relativeDistances.Array[i].Equals(relativeDistances.Array[i]) == false)
                    return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ (relativeDistances != null
                           ? relativeDistances.GetHashCode()
                           : 0);
            }
        }

        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(RelativeDistances)}: {relativeDistances}";
        }
    }

    /// <summary>
    /// Rasterization Request parameters
    /// </summary>
    public class CGDataRequestRasterization : CGDataRequestParameter
    {
#if CONTRACTS_FULL
        [ContractInvariantMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
            Contract.Invariant(Start.IsRatio());
            Contract.Invariant(RasterizedRelativeLength.IsRatio());
            Contract.Invariant(Resolution > 0);
            Contract.Invariant(Resolution <= 100);
            Contract.Invariant(SplineAbsoluteLength.IsPositiveNumber());
            Contract.Invariant(AngleThreshold.IsIn0To180Range());
        }
#endif


        public enum ModeEnum
        {
            /// <summary>
            /// Distribute sample points evenly spread
            /// </summary>
            Even,
            /// <summary>
            /// Use Source' curvation to optimize the result
            /// </summary>
            Optimized
        }

        /// <summary>
        /// Relative Start Position (0..1)
        /// </summary>
        public float Start;

        /// <summary>
        /// Relative Length. A value of 1 means the full spline length
        /// </summary>
        public float RasterizedRelativeLength;

        /// <summary>
        /// Maximum number of samplepoints
        /// </summary>
        public int Resolution;

        /// <summary>
        /// Angle resolution (0..100) for optimized mode
        /// </summary>
        public float AngleThreshold;

        /// <summary>
        /// Rasterization mode
        /// </summary>
        public ModeEnum Mode;

        public CGDataRequestRasterization(float start, float rasterizedRelativeLength, int resolution, float angle, ModeEnum mode = ModeEnum.Even)
        {
#if CONTRACTS_FULL
            Contract.Requires(rasterizedRelativeLength.IsRatio());
#endif
            Start = Mathf.Repeat(start, 1);
            RasterizedRelativeLength = Mathf.Clamp01(rasterizedRelativeLength);
            Resolution = resolution;
            AngleThreshold = angle;
            Mode = mode;
        }

        public CGDataRequestRasterization(CGDataRequestRasterization source) : this(source.Start, source.RasterizedRelativeLength, source.Resolution, source.AngleThreshold, source.Mode)
        {
        }

        public override bool Equals(object obj)
        {
            CGDataRequestRasterization O = obj as CGDataRequestRasterization;
            if (O == null)
                return false;
            return (Start == O.Start && RasterizedRelativeLength == O.RasterizedRelativeLength && Resolution == O.Resolution && AngleThreshold == O.AngleThreshold && Mode == O.Mode);
        }

        public override int GetHashCode()
        {
            return new { A = Start, B = RasterizedRelativeLength, C = Resolution, D = AngleThreshold, E = Mode }.GetHashCode(); //OPTIM avoid array creation
        }

        public override string ToString()
        {
            return $"{nameof(Start)}: {Start}, {nameof(RasterizedRelativeLength)}: {RasterizedRelativeLength}, {nameof(Resolution)}: {Resolution}, {nameof(AngleThreshold)}: {AngleThreshold}, {nameof(Mode)}: {Mode}";
        }
    }
}