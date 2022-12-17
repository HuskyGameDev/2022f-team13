// =====================================================================
// Copyright 2013-2022 ToolBuddy
// All rights reserved
// 
// http://www.toolbuddy.net
// =====================================================================

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Assertions;

namespace FluffyUnderware.Curvy.Generator.Modules
{
    [ModuleInfo("Modifier/TRS Mesh", ModuleName = "TRS Mesh", Description = "Transform,Rotate,Scale a VMesh")]
    [HelpURL(CurvySpline.DOCLINK + "cgtrsmesh")]
    public class ModifierTRSMesh : TRSModuleBase
    {
        [HideInInspector]
        [InputSlotInfo(typeof(CGVMesh), Array = true, ModifiesData = true)]
        public CGModuleInputSlot InVMesh = new CGModuleInputSlot();

        [HideInInspector]
        [OutputSlotInfo(typeof(CGVMesh), Array = true)]
        public CGModuleOutputSlot OutVMesh = new CGModuleOutputSlot();



        #region ### Public Methods ###

        public override void Refresh()
        {
            base.Refresh();
            if (OutVMesh.IsLinked)
            {
                List<CGVMesh> vMesh = InVMesh.GetAllData<CGVMesh>(out bool isDisposable);
                Matrix4x4 mat = Matrix;
                for (int i = 0; i < vMesh.Count; i++)
                    vMesh[i].TRS(mat);
#if CURVY_SANITY_CHECKS
                Assert.IsTrue(isDisposable);
#endif
                OutVMesh.SetData(vMesh);
            }

        }

        #endregion







    }
}