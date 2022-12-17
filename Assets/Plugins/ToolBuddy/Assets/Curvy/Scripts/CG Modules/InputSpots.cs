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

namespace FluffyUnderware.Curvy.Generator.Modules
{
    [ModuleInfo("Input/Spots", ModuleName = "Input Spots", Description = "Defines an array of placement spots")]
    [HelpURL(CurvySpline.DOCLINK + "cginputspots")]
    public class InputSpots : CGModule
    {

        [HideInInspector]
        [OutputSlotInfo(typeof(CGSpots))]
        public CGModuleOutputSlot OutSpots = new CGModuleOutputSlot();

        #region ### Serialized Fields ###

        [ArrayEx]
        [SerializeField]
        private List<CGSpot> m_Spots = new List<CGSpot>();

        #endregion

        #region ### Public Properties ###

        public List<CGSpot> Spots
        {
            get { return m_Spots; }
            set
            {
                if (m_Spots != value)
                    m_Spots = value;
                Dirty = true;
            }
        }

        #endregion

        #region ### Private Fields & Properties ###
        #endregion

        #region ### Unity Callbacks ###
        /*! \cond UNITY */

        protected override void OnEnable()
        {
            base.OnEnable();
            Properties.MinWidth = 250;
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
        }
#endif

        public override void Reset()
        {
            base.Reset();
            Spots.Clear();
            Dirty = true;
        }

        /*! \endcond */
        #endregion

        #region ### Public Methods ###

        public override void Refresh()
        {
            base.Refresh();

            if (OutSpots.IsLinked)
            {
                OutSpots.SetData(new CGSpots(Spots));
            }
        }

        #endregion

        #region ### Privates ###
        /*! \cond PRIVATE */


        /*! \endcond */
        #endregion

    }
}