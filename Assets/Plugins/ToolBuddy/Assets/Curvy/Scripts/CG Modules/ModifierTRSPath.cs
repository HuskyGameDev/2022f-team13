// =====================================================================
// Copyright 2013-2022 ToolBuddy
// All rights reserved
// 
// http://www.toolbuddy.net
// =====================================================================

using System;
using UnityEngine;
using System.Collections;
using FluffyUnderware.DevTools;
using UnityEngine.Assertions;

namespace FluffyUnderware.Curvy.Generator.Modules
{
    [ModuleInfo("Modifier/TRS Path", ModuleName = "TRS Path", Description = "Transform,Rotate,Scale a Path")]
    [HelpURL(CurvySpline.DOCLINK + "cgtrspath")]
#pragma warning disable 618
    public class ModifierTRSPath : TRSModuleBase, IOnRequestProcessing, IPathProvider
#pragma warning restore 618
    {
        [HideInInspector]
        [InputSlotInfo(typeof(CGPath), Name = "Path A", ModifiesData = true)]
        public CGModuleInputSlot InPath = new CGModuleInputSlot();

        [HideInInspector]
        [OutputSlotInfo(typeof(CGPath))]
        public CGModuleOutputSlot OutPath = new CGModuleOutputSlot();



        #region ### Public Properties ###

        public bool PathIsClosed
        {
            get
            {
                return (IsConfigured) && InPath.SourceSlot().PathProvider.PathIsClosed;
            }
        }

        #endregion


        #region ### IOnRequestProcessing ###

        public CGData[] OnSlotDataRequest(CGModuleInputSlot requestedBy, CGModuleOutputSlot requestedSlot, params CGDataRequestParameter[] requests)
        {
            CGData[] result;
            if (requestedSlot == OutPath)
            {
                CGPath data = InPath.GetData<CGPath>(out bool isDisposable, requests);
#if CURVY_SANITY_CHECKS
                // I forgot why I added this assertion, but I trust my past self
                Assert.IsTrue(data == null || isDisposable);
#endif

                if (data)
                {
                    var scaleLessMatrix = ApplyTrsOnShape(data);
                    for (int i = 0; i < data.Count; i++)
                        data.Directions.Array[i] = scaleLessMatrix.MultiplyVector(data.Directions.Array[i]);
                }
                result = new CGData[1] { data };
            }
            else
                result = null;

            return result;
        }
    }

    #endregion




}