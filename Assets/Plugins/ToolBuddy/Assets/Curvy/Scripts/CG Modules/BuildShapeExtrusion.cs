// =====================================================================
// Copyright 2013-2022 ToolBuddy
// All rights reserved
// 
// http://www.toolbuddy.net
// =====================================================================

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ToolBuddy.Pooling.Pools;
using FluffyUnderware.DevTools;
using FluffyUnderware.Curvy.Utils;
using FluffyUnderware.DevTools.Extensions;
using ToolBuddy.Pooling.Collections;
using UnityEngine.Assertions;

namespace FluffyUnderware.Curvy.Generator.Modules
{
    [ModuleInfo("Build/Shape Extrusion", ModuleName = "Shape Extrusion", Description = "Simple Shape Extrusion")]
    [HelpURL(CurvySpline.DOCLINK + "cgbuildshapeextrusion")]
    public class BuildShapeExtrusion : ScalingModule, IPathProvider
    {
        #region ### Enums ###

        /// <summary>
        /// <see cref="FluffyUnderware.Curvy.Generator.Modules.ScaleMode"/>
        /// </summary>
        [Obsolete("Use FluffyUnderware.Curvy.Generator.Modules.ScaleMode instead")]
        public enum ScaleModeEnum
        {
            Simple,
            Advanced
        }

        public enum CrossShiftModeEnum
        {
            /// <summary>
            /// The start of the Shape is used
            /// </summary>
            None,
            /// <summary>
            /// The starting point is shifted to the collision point of the Path's orientation with the cross shape
            /// </summary>
            ByOrientation,//TODO rename to ByPathOrientation?
            /// <summary>
            /// The starting point is shifted by a user defined value
            /// </summary>
            Custom
        }

        #endregion

        [HideInInspector]
        [InputSlotInfo(typeof(CGPath), RequestDataOnly = true)]
        public CGModuleInputSlot InPath = new CGModuleInputSlot();

        [HideInInspector]
        [InputSlotInfo(typeof(CGShape), Array = true, ArrayType = SlotInfo.SlotArrayType.Hidden, RequestDataOnly = true)]
        public CGModuleInputSlot InCross = new CGModuleInputSlot();

        [HideInInspector]
        [OutputSlotInfo(typeof(CGVolume))]
        public CGModuleOutputSlot OutVolume = new CGModuleOutputSlot();

        [HideInInspector]
        [OutputSlotInfo(typeof(CGVolume))]
        public CGModuleOutputSlot OutVolumeHollow = new CGModuleOutputSlot();

        #region ### Serialized Fields ###

        #region TAB: Path
        [Tab("Path")]
        [FloatRegion(UseSlider = true, RegionOptionsPropertyName = nameof(RangeOptions), Precision = 4)]
        [SerializeField]
        private FloatRegion m_Range = FloatRegion.ZeroOne;
        [SerializeField, RangeEx(1, 100, "Resolution", "Defines how densely the path spline's sampling points are. When the value is 100, the number of sampling points per world distance unit is equal to the spline's Max Points Per Unit")]
        private int m_Resolution = 50;
        [SerializeField] private bool m_Optimize = true;
        [FieldCondition(nameof(m_Optimize), true)]
        [SerializeField, RangeEx(0.1f, 120, Tooltip = "Max angle")]
        private float m_AngleThreshold = 10;

        #endregion

        #region TAB: Cross
        [Tab("Cross")]
        [FieldAction("CBEditCrossButton", Position = ActionAttribute.ActionPositionEnum.Above)]
        [FloatRegion(UseSlider = true, RegionOptionsPropertyName = nameof(CrossRangeOptions), Precision = 4)]
        [SerializeField]
        private FloatRegion m_CrossRange = FloatRegion.ZeroOne;
        [SerializeField, RangeEx(1, 100, "Resolution", Tooltip = "Defines how densely the cross spline's sampling points are. When the value is 100, the number of sampling points per world distance unit is equal to the spline's Max Points Per Unit")]
        private int m_CrossResolution = 50;
        [SerializeField, Label("Optimize")] private bool m_CrossOptimize = true;
        [FieldCondition(nameof(m_CrossOptimize), true)]
        [SerializeField, RangeEx(0.1f, 120, "Angle Threshold", Tooltip = "Max angle")]
        private float m_CrossAngleThreshold = 10;

        //[Header("Options")]
        [SerializeField, Label("Include CPs")]
        [Tooltip("If enabled, vertices are guaranteed to be created for all the Cross shape's Control Points.")]
        private bool m_CrossIncludeControlpoints;

        [SerializeField, Label("Hard Edges"), HideInInspector]
        [Obsolete("This option is now always assumed to be true")]
        private bool m_CrossHardEdges;

        [SerializeField, Label("Materials"), HideInInspector]
        [Obsolete("This option is now always assumed to be true")]
        private bool m_CrossMaterials;

        [SerializeField, Label("Extended UV"), HideInInspector]
        [Obsolete("This option is now always assumed to be true")]
        private bool m_CrossExtendedUV;

        [SerializeField, Label("Shift", Tooltip = "Defines a shift to be applied on the output volume's cross.\r\nThis shift is used when interpolating values (position, normal, ...) along the volume's surface.")]
        private CrossShiftModeEnum m_CrossShiftMode = CrossShiftModeEnum.ByOrientation;
        [SerializeField]
        [RangeEx(0, 1, "Value", "Shift By", Slider = true)]
        [FieldCondition(nameof(m_CrossShiftMode), CrossShiftModeEnum.Custom)]
        private float m_CrossShiftValue;
        [Label("Reverse Normal", "Reverse Vertex Normals?")]
        [SerializeField]
        private bool m_CrossReverseNormals;
        #endregion

        #region TAB: Hollow
        [Tab("Hollow", Sort = 102)]
        [RangeEx(0, 1, Slider = true, Label = "Inset")]
        [SerializeField]
        private float m_HollowInset;
        [Label("Reverse Normal", "Reverse Vertex Normals?")]
        [SerializeField]
        private bool m_HollowReverseNormals;
        #endregion


        #endregion

        #region ### Public Properties ###

        #region TAB: Path

        //TODO DESIGN this code should be unified with BuildRasterizedPath's

        public float From
        {
            get { return m_Range.From; }
            set
            {
                float v = Mathf.Repeat(value, 1);
                if (m_Range.From != v)
                    m_Range.From = v;

                Dirty = true;
            }
        }

        public float To
        {
            get { return m_Range.To; }
            set
            {
                float v = Mathf.Max(From, value);
                if (ClampPath)
                    v = DTMath.Repeat(value, 1);
                if (m_Range.To != v)
                    m_Range.To = v;

                Dirty = true;
            }
        }

        public float Length
        {
            get
            {
                return (ClampPath) ? m_Range.To - m_Range.From : m_Range.To;
            }
            set
            {
                float v = (ClampPath) ? value - m_Range.To : value;
                if (m_Range.To != v)
                    m_Range.To = v;
                Dirty = true;
            }
        }

        /// <summary>
        /// Defines how densely the path spline's sampling points are. When the value is 100, the number of sampling points per world distance unit is equal to the spline's MaxPointsPerUnit
        /// </summary>
        public int Resolution
        {
            get { return m_Resolution; }
            set
            {
                int v = Mathf.Clamp(value, 1, 100);
                if (m_Resolution != v)
                    m_Resolution = v;
                Dirty = true;
            }
        }

        public bool Optimize
        {
            get { return m_Optimize; }
            set
            {
                if (m_Optimize != value)
                    m_Optimize = value;
                Dirty = true;
            }
        }

        public float AngleThreshold
        {
            get { return m_AngleThreshold; }
            set
            {
                float v = Mathf.Clamp(value, 0.1f, 120);
                if (m_AngleThreshold != v)
                    m_AngleThreshold = v;
                Dirty = true;
            }
        }


        #endregion
        #region TAB: Cross
        public float CrossFrom
        {
            get { return m_CrossRange.From; }
            set
            {
                float v = Mathf.Repeat(value, 1);
                if (m_CrossRange.From != v)
                    m_CrossRange.From = v;

                Dirty = true;
            }
        }

        public float CrossTo
        {
            get { return m_CrossRange.To; }
            set
            {
                float v = Mathf.Max(CrossFrom, value);
                if (ClampCross)
                    v = DTMath.Repeat(value, 1);
                if (m_CrossRange.To != v)
                    m_CrossRange.To = v;

                Dirty = true;
            }
        }

        public float CrossLength
        {
            get
            {
                return (ClampCross) ? m_CrossRange.To - m_CrossRange.From : m_CrossRange.To;
            }
            set
            {
                float v = (ClampCross) ? value - m_CrossRange.To : value;
                if (m_CrossRange.To != v)
                    m_CrossRange.To = v;
                Dirty = true;
            }
        }

        /// <summary>
        /// Defines how densely the cross spline's sampling points are. When the value is 100, the number of sampling points per world distance unit is equal to the spline's MaxPointsPerUnit
        /// </summary>
        public int CrossResolution
        {
            get { return m_CrossResolution; }
            set
            {
                int v = Mathf.Clamp(value, 1, 100);
                if (m_CrossResolution != v)
                    m_CrossResolution = v;
                Dirty = true;
            }
        }

        public bool CrossOptimize
        {
            get { return m_CrossOptimize; }
            set
            {
                if (m_CrossOptimize != value)
                    m_CrossOptimize = value;
                Dirty = true;
            }
        }

        public float CrossAngleThreshold
        {
            get { return m_CrossAngleThreshold; }
            set
            {
                float v = Mathf.Clamp(value, 0.1f, 120);
                if (m_CrossAngleThreshold != v)
                    m_CrossAngleThreshold = v;
                Dirty = true;
            }
        }


        public bool CrossIncludeControlPoints
        {
            get { return m_CrossIncludeControlpoints; }
            set
            {
                if (m_CrossIncludeControlpoints != value)
                    m_CrossIncludeControlpoints = value;
                Dirty = true;
            }
        }

        [Obsolete("This option is now always assumed to be true")]
        public bool CrossHardEdges
        {
            get { return m_CrossHardEdges; }
            set
            {
                if (m_CrossHardEdges != value)
                    m_CrossHardEdges = value;
                Dirty = true;
            }
        }

        [Obsolete("This option is now always assumed to be true")]
        public bool CrossMaterials
        {
            get { return m_CrossMaterials; }
            set
            {
                if (m_CrossMaterials != value)
                    m_CrossMaterials = value;
                Dirty = true;
            }
        }

        [Obsolete("This option is now always assumed to be true")]
        public bool CrossExtendedUV
        {
            get { return m_CrossExtendedUV; }
            set
            {
                if (m_CrossExtendedUV != value)
                    m_CrossExtendedUV = value;
                Dirty = true;
            }
        }

        /// <summary>
        /// Defines how the <see cref="CGVolume.CrossFShift"/> value is defined.
        /// </summary>
        public CrossShiftModeEnum CrossShiftMode
        {
            get { return m_CrossShiftMode; }
            set
            {
                if (m_CrossShiftMode != value)
                    m_CrossShiftMode = value;
                Dirty = true;
            }
        }

        public float CrossShiftValue
        {
            get { return m_CrossShiftValue; }
            set
            {
                float v = Mathf.Repeat(value, 1);
                if (m_CrossShiftValue != v)
                    m_CrossShiftValue = v;
                Dirty = true;
            }
        }

        public bool CrossReverseNormals
        {
            get { return m_CrossReverseNormals; }
            set
            {
                if (m_CrossReverseNormals != value)
                    m_CrossReverseNormals = value;
                Dirty = true;
            }
        }


        #endregion

        #region TAB: Scale

        /// <summary>
        /// <see cref="ScalingModule.ScaleMode"/>
        /// </summary>
        [Obsolete("Use parent class ScalingModule's ScaleMode instead")]
        public new ScaleModeEnum ScaleMode
        {
            get
            {
                return base.ScaleMode == Modules.ScaleMode.Simple ? ScaleModeEnum.Simple : ScaleModeEnum.Advanced;
            }
            set
            {
                if (value == ScaleModeEnum.Simple)
                    base.ScaleMode = Modules.ScaleMode.Simple;
                else
                    base.ScaleMode = Modules.ScaleMode.Advanced;
            }
        }
        #endregion

        #region TAB: Hollow

        public float HollowInset
        {
            get { return m_HollowInset; }
            set
            {
                float v = Mathf.Clamp01(value);
                if (m_HollowInset != v)
                    m_HollowInset = v;
                Dirty = true;
            }
        }

        public bool HollowReverseNormals
        {
            get { return m_HollowReverseNormals; }
            set
            {
                if (m_HollowReverseNormals != value)
                    m_HollowReverseNormals = value;
                Dirty = true;
            }
        }

        #endregion

        public int PathSamples
        {
            get;
            private set;
        }

        public int CrossSamples
        {
            get;
            private set;
        }

        public int CrossGroups { get; private set; }

        public IExternalInput Cross
        {
            get
            {
                return (IsConfigured) ? InCross.SourceSlot().ExternalInput : null;
            }
        }

        public Vector3 CrossPosition { get; protected set; }

        public Quaternion CrossRotation { get; protected set; }

        public bool PathIsClosed
        {
            get { return InPath.SourceSlot().PathProvider.PathIsClosed; }
        }

        #endregion

        #region ### Private Fields & Properties ###

        private bool ClampPath { get { return !InPath.IsLinked || !InPath.SourceSlot().PathProvider.PathIsClosed; } }
        private bool ClampCross { get { return !InCross.IsLinked || !InCross.SourceSlot().PathProvider.PathIsClosed; } }

        private RegionOptions<float> RangeOptions
        {
            get
            {

                if (ClampPath)
                {
                    return RegionOptions<float>.MinMax(0, 1);
                }
                else
                {
                    return new RegionOptions<float>()
                    {
                        LabelFrom = "Start",
                        ClampFrom = DTValueClamping.Min,
                        FromMin = 0,
                        LabelTo = "Length",
                        ClampTo = DTValueClamping.Range,
                        ToMin = 0,
                        ToMax = 1
                    };
                }
            }
        }

        private RegionOptions<float> CrossRangeOptions
        {
            get
            {

                if (ClampCross)
                {
                    return RegionOptions<float>.MinMax(0, 1);
                }
                else
                {
                    return new RegionOptions<float>()
                    {
                        LabelFrom = "Start",
                        ClampFrom = DTValueClamping.Min,
                        FromMin = 0,
                        LabelTo = "Length",
                        ClampTo = DTValueClamping.Range,
                        ToMin = 0,
                        ToMax = 1
                    };
                }
            }
        }

        #endregion

        #region ### Unity Callbacks ###
        /*! \cond UNITY */

        protected override void OnEnable()
        {
            base.OnEnable();
            Properties.MinWidth = 270;
            Properties.LabelWidth = 100;
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            //TODO OPTIM each one of the following properties setting will set Dirty to true, and trigger a lot of work. You can avoid that by Dirty to true only once per OnValidate call
            Resolution = m_Resolution;
            Optimize = m_Optimize;

            CrossResolution = m_CrossResolution;
            CrossOptimize = m_CrossOptimize;

            CrossIncludeControlPoints = m_CrossIncludeControlpoints;
#pragma warning disable 618
            CrossHardEdges = m_CrossHardEdges;
#pragma warning restore 618
        }
#endif

        public override void Reset()
        {
            base.Reset();
            From = 0;
            To = 1;
            Resolution = 50;
            AngleThreshold = 10;
            Optimize = true;
            CrossFrom = 0;
            CrossTo = 1;
            CrossResolution = 50;
            CrossAngleThreshold = 10;
            CrossOptimize = true;
            CrossIncludeControlPoints = false;
#pragma warning disable 618
            CrossHardEdges = false;
#pragma warning restore 618
#pragma warning disable 618
            CrossMaterials = false;
#pragma warning restore 618
            CrossShiftMode = CrossShiftModeEnum.ByOrientation;
            HollowInset = 0;
#pragma warning disable 618
            CrossExtendedUV = false;
#pragma warning restore 618
            CrossReverseNormals = false;
            HollowReverseNormals = false;
        }


        /*! \endcond */
        #endregion

        #region ### Public Methods ###

        public override void Refresh()
        {
            base.Refresh();
            if (Length == 0)
            {
                OutVolume.SetData(null);
                OutVolumeHollow.SetData(null);
            }
            else
            {
                //OPTIM make it an array
                List<CGDataRequestParameter> req = new List<CGDataRequestParameter>();

                CGPath path;
                bool isPathDisposable;
                {
                    req.Add(new CGDataRequestRasterization(
                        this.From, this.Length,
                        Resolution,
                        AngleThreshold, Optimize ? CGDataRequestRasterization.ModeEnum.Optimized : CGDataRequestRasterization.ModeEnum.Even));
                    path = InPath.GetData<CGPath>(out isPathDisposable, req.ToArray());
                    req.Clear();
                }

                List<CGShape> crosses;
                bool isCrossesDisposable;
                {
                    CGDataRequestRasterization rasterizationRequest;
                    {
                        bool useVariableShape = InCross.LinkedSlots.Count == 1 && InCross.LinkedSlots[0].Info is ShapeOutputSlotInfo && (InCross.LinkedSlots[0].Info as ShapeOutputSlotInfo).OutputsVariableShape;

                        if (useVariableShape && path)
                            rasterizationRequest = new CGDataRequestShapeRasterization(path.RelativeDistances,
                                this.CrossFrom, this.CrossLength,
                                CrossResolution,
                                CrossAngleThreshold, CrossOptimize ? CGDataRequestRasterization.ModeEnum.Optimized : CGDataRequestRasterization.ModeEnum.Even);
                        else
                            rasterizationRequest = new CGDataRequestRasterization(
                                this.CrossFrom, this.CrossLength,
                                CrossResolution,
                                CrossAngleThreshold, CrossOptimize ? CGDataRequestRasterization.ModeEnum.Optimized : CGDataRequestRasterization.ModeEnum.Even);
                    }

                    req.Add(rasterizationRequest);

#pragma warning disable 618
                    req.Add(new CGDataRequestMetaCGOptions(CrossHardEdges, CrossMaterials, CrossIncludeControlPoints, CrossExtendedUV));
#pragma warning restore 618

                    crosses = InCross.GetAllData<CGShape>(out isCrossesDisposable, req.ToArray());
                }


                bool isPathInvalid = !path || path.Count == 0;

                bool areCrossesInvalid;
                {
                    List<int> distinctCrossCounts = crosses.Select(c => c == null ? 0 : c.Count).Distinct().ToList();
                    if (distinctCrossCounts.Count() != 1 || distinctCrossCounts.First() == 0)
                    {
                        areCrossesInvalid = true;
                        UIMessages.Add("Shape Extrusion: All input Crosses are expected to have the same non zero number of sample points.");
                    }
                    else
                        areCrossesInvalid = false;
                }

                if (isPathInvalid || areCrossesInvalid)
                {
                    OutVolume.ClearData();
                    OutVolumeHollow.ClearData();
                    return;
                }

                CGShape initialCross = crosses[0];

#if UNITY_EDITOR
                //TODO move these warnings, and modify them, inside the raterizing modules themselves. Because the way it is done now, if there is a module between the InputSplineShape module and the current module, the warning will not happen

                //TODO add warnings to the "Rasterize Path" module similar to those below
                //Warning messages
                {
                    for (int index = 0; index < InPath.LinkedSlots.Count; index++)
                    {
                        CGModuleSlot linkedSlot = InPath.LinkedSlots[index];
                        if (linkedSlot.Module is InputSplinePath)
                        {
                            InputSplinePath inputSplineModule = linkedSlot.Module as InputSplinePath;
                            if (inputSplineModule
                                && inputSplineModule.UseCache
                                && path.Count > inputSplineModule.Spline.CacheSize * 1.30f
                                && path.Count > inputSplineModule.Spline.CacheSize + 30)
                                UIMessages.Add(String.Format(System.Globalization.CultureInfo.InvariantCulture, "The Cache Density of \"{0}\" might be too small for this module's Path Resolution. To get a more detailed extruded volume, you might need to increase Cache Density, or set the input module's Use Cache to false", inputSplineModule.Spline.gameObject.name));
                        }
                    }

                    for (int index = 0; index < InCross.LinkedSlots.Count; index++)
                    {
                        CGModuleSlot linkedSlot = InCross.LinkedSlots[index];
                        if (linkedSlot.Module is InputSplineShape)
                        {
                            InputSplineShape inputSplineModule = linkedSlot.Module as InputSplineShape;
                            if (inputSplineModule
                                && inputSplineModule.UseCache
                                && initialCross.Count > inputSplineModule.Shape.CacheSize * 1.30f
                                && initialCross.Count > inputSplineModule.Shape.CacheSize + 30)
                                UIMessages.Add(String.Format(System.Globalization.CultureInfo.InvariantCulture, "The Cache Density of \"{0}\" might be too small for this module's Cross Resolution. To get a more detailed extruded volume, you might need to increase Cache Density, or set the input module's Use Cache to false", inputSplineModule.Shape.gameObject.name));
                        }
                    }
                }
#endif
                CGVolume vol = CGVolume.Get(OutVolume.GetData<CGVolume>(), path, initialCross);
                CGVolume volHollow = OutVolumeHollow.IsLinked ? CGVolume.Get(OutVolumeHollow.GetData<CGVolume>(), path, initialCross) : null;
                bool hasHollowVolume = volHollow;
                PathSamples = path.Count;
                CrossSamples = initialCross.Count;
                CrossGroups = initialCross.MaterialGroups.Count;
                CrossPosition = vol.Positions.Array[0];
                CrossRotation = Quaternion.LookRotation(vol.Directions.Array[0], vol.Normals.Array[0]);

                int vtIdx = 0;

                Vector2[] scalesArray = vol.Scales.Array;

                float crossNormalMul = CrossReverseNormals ? -1 : 1;
                float hollowNormalMul = HollowReverseNormals ? -1 : 1;

                bool hasSingleCross = crosses.Count == 1;

                int samplesCount = path.Count;
                for (int sample = 0; sample < samplesCount; sample++)
                {
                    CGShape currentCross;
                    if (hasSingleCross)
                        currentCross = initialCross;
                    else
                    {
                        int crossIndex = Mathf.RoundToInt((crosses.Count - 1) * path.RelativeDistances.Array[sample]);
#if CURVY_SANITY_CHECKS
                        Assert.IsTrue(path.RelativeDistances.Array[sample] >= 0);
                        Assert.IsTrue(path.RelativeDistances.Array[sample] <= 1);
                        Assert.IsTrue(crossIndex >= 0);
                        Assert.IsTrue(crossIndex < crosses.Count);
#endif
                        currentCross = crosses[crossIndex];
                    }

                    SubArray<Vector3> crossPositions = currentCross.Positions;
                    SubArray<Vector3> crossNormals = currentCross.Normals;

                    Quaternion pathRotation = Quaternion.LookRotation(path.Directions.Array[sample], path.Normals.Array[sample]);

                    //Numbers used in the quaternion multiplication formula
                    float forumlaNumber4;
                    float forumlaNumber5;
                    float forumlaNumber6;
                    float forumlaNumber7;
                    float forumlaNumber8;
                    float forumlaNumber9;
                    float forumlaNumber10;
                    float forumlaNumber11;
                    float forumlaNumber12;
                    {
                        float num1 = pathRotation.x * 2f;
                        float num2 = pathRotation.y * 2f;
                        float num3 = pathRotation.z * 2f;
                        forumlaNumber4 = pathRotation.x * num1;
                        forumlaNumber5 = pathRotation.y * num2;
                        forumlaNumber6 = pathRotation.z * num3;
                        forumlaNumber7 = pathRotation.x * num2;
                        forumlaNumber8 = pathRotation.x * num3;
                        forumlaNumber9 = pathRotation.y * num3;
                        forumlaNumber10 = pathRotation.w * num1;
                        forumlaNumber11 = pathRotation.w * num2;
                        forumlaNumber12 = pathRotation.w * num3;
                    }

                    Vector2 scale = GetScale(sample, path.RelativeDistances, path.SourceRelativeDistances);
                    Matrix4x4 mat = Matrix4x4.TRS(path.Positions.Array[sample], pathRotation, scale);
                    Matrix4x4 matHollow = hasHollowVolume
                        ? Matrix4x4.TRS(path.Positions.Array[sample], pathRotation, scale * (1 - HollowInset))
                        : default;

                    scalesArray[sample].x = scale.x;
                    scalesArray[sample].y = scale.y;

                    int currentCrossCount = currentCross.Count;
                    for (int c = 0; c < currentCrossCount; c++)
                    {
                        vol.Vertices.Array[vtIdx] = mat.MultiplyPoint3x4(crossPositions.Array[c]);

                        Vector3 crossNormal = crossNormals.Array[c];

                        // inlined version of  Vector3 rotatedCrossNormal = pathRotation * crossNormal;
                        float rotatedCrossNormalX = (1.0f - (forumlaNumber5 + forumlaNumber6)) * crossNormal.x + (forumlaNumber7 - forumlaNumber12) * crossNormal.y + (forumlaNumber8 + forumlaNumber11) * crossNormal.z;
                        float rotatedCrossNormalY = (forumlaNumber7 + forumlaNumber12) * crossNormal.x + (1.0f - (forumlaNumber4 + forumlaNumber6)) * crossNormal.y + (forumlaNumber9 - forumlaNumber10) * crossNormal.z;
                        float rotatedCrossNormalZ = (forumlaNumber8 - forumlaNumber11) * crossNormal.x + (forumlaNumber9 + forumlaNumber10) * crossNormal.y + (1.0f - (forumlaNumber4 + forumlaNumber5)) * crossNormal.z;

                        vol.VertexNormals.Array[vtIdx].x = rotatedCrossNormalX * crossNormalMul;
                        vol.VertexNormals.Array[vtIdx].y = rotatedCrossNormalY * crossNormalMul;
                        vol.VertexNormals.Array[vtIdx].z = rotatedCrossNormalZ * crossNormalMul;

                        if (hasHollowVolume)
                        {
                            volHollow.Vertices.Array[vtIdx] = matHollow.MultiplyPoint3x4(crossPositions.Array[c]);
                            volHollow.VertexNormals.Array[vtIdx].x = rotatedCrossNormalX * hollowNormalMul;
                            volHollow.VertexNormals.Array[vtIdx].y = rotatedCrossNormalY * hollowNormalMul;
                            volHollow.VertexNormals.Array[vtIdx].z = rotatedCrossNormalZ * hollowNormalMul;
                        }

                        vtIdx++;
                    }
                }

                switch (CrossShiftMode)
                {
                    case CrossShiftModeEnum.ByOrientation:
                        vol.CrossFShift = 0;
                        // shift CrossF to match Path Orientation
                        Vector2 hit;
                        float frag;
                        for (int i = 0; i < initialCross.Count - 1; i++)
                            if (DTMath.RayLineSegmentIntersection(vol.Positions.Array[0], vol.Normals.Array[0], vol.Vertices.Array[i], vol.Vertices.Array[i + 1], out hit, out frag))
                            {
                                vol.CrossFShift = DTMath.SnapPrecision(vol.CrossRelativeDistances.Array[i] + (vol.CrossRelativeDistances.Array[i + 1] - vol.CrossRelativeDistances.Array[i]) * frag, 2);
                                break;
                            }

                        break;
                    case CrossShiftModeEnum.Custom:
                        vol.CrossFShift = CrossShiftValue;
                        break;
                    case CrossShiftModeEnum.None:
                        vol.CrossFShift = 0;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("CrossShiftMode");
                }

                if (volHollow != null)
                    volHollow.CrossFShift = vol.CrossFShift;

                OutVolume.SetData(vol);
                OutVolumeHollow.SetData(volHollow);

                if (isPathDisposable)
                {
                    path.Dispose();
                }

                if (isCrossesDisposable)
                {
                    crosses.ForEach(c => c.Dispose());
                }
            }
        }

        /// <summary>
        /// <see cref="ScalingModule.GetScale"/>
        /// </summary>
        [Obsolete("Use parent class ScalingModule's GetScale instead")]
        public new Vector3 GetScale(float relativeDistance)
        {
            Vector2 scaleVector = base.GetScale(relativeDistance);
            return new Vector3(scaleVector.x, scaleVector.y, 1);
        }

        #endregion

    }
}