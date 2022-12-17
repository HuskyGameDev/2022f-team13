// =====================================================================
// Copyright 2013-2022 ToolBuddy
// All rights reserved
// 
// http://www.toolbuddy.net
// =====================================================================

using UnityEngine;
using System.Collections;
using FluffyUnderware.DevTools;


namespace FluffyUnderware.Curvy.Generator.Modules
{
    [ModuleInfo("Debug/Volume", ModuleName = "Debug Volume")]
    [HelpURL(CurvySpline.DOCLINK + "cgdebugvolume")]
    public class DebugVolume : CGModule
    {
        [HideInInspector]
        [InputSlotInfo(typeof(CGVolume), Name = "Volume")]
        public CGModuleInputSlot InData = new CGModuleInputSlot();

        #region ### Serialized Fields ###

        [Tab("General")]
        public bool ShowPathSamples = true;
        public bool ShowCrossSamples = true;
        [FieldCondition(nameof(ShowCrossSamples), true)]
        [IntRegion(RegionIsOptional = true)]
        public IntRegion LimitCross = new IntRegion(0, 0);
        public bool ShowNormals = false;
        public bool ShowIndex = false;
        public bool ShowMap = false;
        public Color PathColor = Color.white;
        public Color VolumeColor = Color.gray;
        public Color NormalColor = Color.yellow;
        [Tab("Interpolate")]
        public bool Interpolate;
        [RangeEx(-1, 1, "Path")]
        public float InterpolatePathF;
        [RangeEx(-1, 1, "Cross")]
        public float InterpolateCrossF;
        #endregion

        #region ### Public Properties ###
        #endregion

        #region ### Private Fields & Properties ###
        #endregion

        #region ### Unity Callbacks ###
        /*! \cond UNITY */

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            Dirty = true;
        }

        public override void Reset()
        {
            base.Reset();
            ShowPathSamples = true;
            ShowCrossSamples = true;
            LimitCross = new IntRegion(0, 0);
            ShowNormals = false;
            ShowIndex = false;
            ShowMap = false;
            PathColor = Color.white;
            VolumeColor = Color.gray;
            NormalColor = Color.yellow;
            Interpolate = false;
            InterpolatePathF = 0;
            InterpolateCrossF = 0;
        }
#endif

        /*! \endcond */
        #endregion

        #region ### Public Methods ###
        #endregion

        #region ### Privates ###
        /*! \cond PRIVATE */


        /*! \endcond */
        #endregion


    }
}