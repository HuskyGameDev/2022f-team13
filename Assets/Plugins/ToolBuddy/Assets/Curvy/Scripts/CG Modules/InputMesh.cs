// =====================================================================
// Copyright 2013-2022 ToolBuddy
// All rights reserved
// 
// http://www.toolbuddy.net
// =====================================================================

using UnityEngine;
using System.Collections;
using FluffyUnderware.DevTools;
using System.Collections.Generic;

namespace FluffyUnderware.Curvy.Generator.Modules
{
    [ModuleInfo("Input/Meshes", ModuleName = "Input Meshes", Description = "Create VMeshes")]
    [HelpURL(CurvySpline.DOCLINK + "cginputmesh")]
    public class InputMesh : CGModule, IExternalInput
    {

        [HideInInspector]
        [OutputSlotInfo(typeof(CGVMesh), Array = true)]
        public CGModuleOutputSlot OutVMesh = new CGModuleOutputSlot();

        #region ### Serialized Fields ###

        [SerializeField]
        [ArrayEx]
        private List<CGMeshProperties> m_Meshes = new List<CGMeshProperties>(new CGMeshProperties[] { new CGMeshProperties() });

        #endregion

        #region ### Public Properties ###

        public List<CGMeshProperties> Meshes
        {
            get { return m_Meshes; }
        }

        public bool SupportsIPE
        {
            get { return false; }
        }

        #endregion

        #region ### Private Fields & Properties ###
        #endregion

        #region ### Unity Callbacks ###
        /*! \cond UNITY */

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            foreach (CGMeshProperties m in Meshes)
                m.OnValidate();
            Dirty = true;
        }
#endif
        public override void Reset()
        {
            base.Reset();
            Meshes.Clear();
            Dirty = true;
        }

        /*! \endcond */
        #endregion

        #region ### Public Methods ###

        public override void Refresh()
        {
            base.Refresh();
            //OutVMesh
            if (OutVMesh.IsLinked)
            {
                CGVMesh[] data = new CGVMesh[Meshes.Count];
                int total = 0;
                for (int i = 0; i < Meshes.Count; i++)
                {
                    Mesh mesh = Meshes[i].Mesh;
                    if (mesh)
                    {
                        if (mesh.isReadable == false)
                        {
                            UIMessages.Add($"Input mesh '{mesh.name}' is not readable. Please set the 'Read/Write Enabled' parameter to true in the mesh model import settings");
                        }
                        data[total++] = new CGVMesh(Meshes[i]);
                    }
                }
                System.Array.Resize(ref data, total);
                OutVMesh.SetData(data);
            }

#if UNITY_EDITOR
            if (Meshes.Exists(m => m.Mesh == null))
                UIMessages.Add("Missing Mesh input");
#endif
        }



        public override void OnTemplateCreated()
        {
            base.OnTemplateCreated();
            Meshes.Clear();
        }

        #endregion

        #region ### Privates ###
        /*! \cond PRIVATE */


        /*! \endcond */
        #endregion


    }

}