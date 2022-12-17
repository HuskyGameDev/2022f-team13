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
    [ModuleInfo("Debug/VMesh", ModuleName = "Debug VMesh")]
    [HelpURL(CurvySpline.DOCLINK + "cgdebugvmesh")]
    public class DebugVMesh : CGModule
    {
        [HideInInspector]
        [InputSlotInfo(typeof(CGVMesh), Name = "VMesh")]
        public CGModuleInputSlot InData = new CGModuleInputSlot();

        #region ### Serialized Fields ###

        [Tab("General")]
        public bool ShowVertices;
        public bool ShowVertexID;
        public bool ShowUV;

        #endregion

        #region ### Public Properties ###
        #endregion

        #region ### Private Fields & Properties ###
        #endregion

        #region ### Unity Callbacks ###
        /*! \cond UNITY */
        public override void Reset()
        {
            base.Reset();
            ShowVertices = false;
            ShowVertexID = false;
            ShowUV = false;
        }

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