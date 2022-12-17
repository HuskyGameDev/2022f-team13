// =====================================================================
// Copyright 2013-2022 ToolBuddy
// All rights reserved
// 
// http://www.toolbuddy.net
// =====================================================================

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FluffyUnderware.DevTools;
using UnityEngine.Assertions;

namespace FluffyUnderware.Curvy.Generator.Modules
{
    [ModuleInfo("Build/Rasterize Path", ModuleName = "Rasterize Path", Description = "Rasterizes a virtual path")]
    [HelpURL(CurvySpline.DOCLINK + "cgbuildrasterizedpath")]
    public class BuildRasterizedPath : CGModule, IPathProvider
    {
        [HideInInspector]
        [InputSlotInfo(typeof(CGPath), Name = "Path", RequestDataOnly = true)]
        public CGModuleInputSlot InPath = new CGModuleInputSlot();
        [HideInInspector]
        [OutputSlotInfo(typeof(CGPath), Name = "Path", DisplayName = "Rasterized Path")]
        public CGModuleOutputSlot OutPath = new CGModuleOutputSlot();

        #region ### Serialized Fields ###

        [FloatRegion(UseSlider = true, RegionOptionsPropertyName = nameof(RangeOptions), Precision = 4)]
        [SerializeField]
        private FloatRegion m_Range = FloatRegion.ZeroOne;
        [SerializeField, RangeEx(1, 100, "Resolution", "Defines how densely the path spline's sampling points are. When the value is 100, the number of sampling points per world distance unit is equal to the spline's Max Points Per Unit")]
        private int m_Resolution = 50;
        [SerializeField] private bool m_Optimize;
        [FieldCondition(nameof(m_Optimize), true)]
        [SerializeField, RangeEx(0.1f, 120)]
        private float m_AngleTreshold = 10;

        [SerializeField]
        [Section("Backward Compatibility", false)]
        [Tooltip("Curvy versions prior to 8.0.0 had a bug in the computation of the rasterization range for closed splines. Enable this value to keep that bugged behaviour if your project depends on it")]
        private bool useBuggedRange;
        #endregion

        #region ### Public Properties ###

        //TODO DESIGN this code should be unified with BuildShapeExtrusion's

        public float From
        {
            get { return m_Range.From; }
            set
            {
                float v = DTMath.Repeat(value, 1);
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
                    v = Mathf.Repeat(value, 1);
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
            get { return m_AngleTreshold; }
            set
            {
                float v = Mathf.Clamp(value, 0.1f, 120);
                if (m_AngleTreshold != v)
                    m_AngleTreshold = v;
                Dirty = true;
            }
        }

        public CGPath Path
        {
            get
            {
                return OutPath.GetData<CGPath>();
            }
        }

        public bool PathIsClosed
        {
            get
            {
                return !IsConfigured || InPath.SourceSlot().PathProvider.PathIsClosed;
            }
        }

        /// <summary>
        /// Curvy versions prior to 8.0.0 had a bug in the computation of the rasterization range for closed splines. Enable this value to keep that bugged behaviour if your project depends on it
        /// </summary>
        public bool UseBuggedRange
        {
            get { return useBuggedRange; }
            set
            {
                if (useBuggedRange != value)
                {
                    useBuggedRange = value;
                    Dirty = true;
                }
            }
        }

        #endregion

        #region ### Private Fields & Properties ###

        private bool ClampPath { get { return UseBuggedRange ? PathIsClosed : !PathIsClosed; } }

        private RegionOptions<float> RangeOptions
        {
            get
            {

                if (!PathIsClosed)
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
            Properties.MinWidth = 250;
            Properties.LabelWidth = 112;
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            //From = m_Range.From;
            //To = m_Range.To;
            Resolution = m_Resolution;
            Optimize = m_Optimize;
        }
#endif

        public override void Reset()
        {
            base.Reset();
            m_Range = FloatRegion.ZeroOne;
            Resolution = 50;
            AngleThreshold = 10;
            OutPath.ClearData();
            Optimize = false;
        }

        /*! \endcond */
        #endregion

        #region ### Public Methods ###

        public override void Refresh()
        {
            base.Refresh();
            if (Length == 0)
            {
                OutPath.ClearData();
            }
            else
            {
                List<CGDataRequestParameter> req = new List<CGDataRequestParameter>();
                req.Add(new CGDataRequestRasterization(
                    From, Length, Resolution,
                    AngleThreshold, (Optimize) ? CGDataRequestRasterization.ModeEnum.Optimized : CGDataRequestRasterization.ModeEnum.Even));
                CGPath path = InPath.GetData<CGPath>(out bool isDisposable, req.ToArray());
#if CURVY_SANITY_CHECKS
                // I forgot why I added this assertion, but I trust my past self
                Assert.IsTrue(path == null || isDisposable);
#endif
                OutPath.SetData(path);
            }
        }

        #endregion

        #region ### Privates ###
        /*! \cond PRIVATE */


        /*! \endcond */
        #endregion


    }
}