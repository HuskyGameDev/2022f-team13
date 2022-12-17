// =====================================================================
// Copyright 2013-2022 ToolBuddy
// All rights reserved
// 
// http://www.toolbuddy.net
// =====================================================================

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FluffyUnderware.Curvy.Shapes;
using FluffyUnderware.DevTools;

namespace FluffyUnderware.Curvy.Generator.Modules
{

    [ModuleInfo("Input/Spline Shape", ModuleName = "Input Spline Shape", Description = "Spline Shape")]
    [HelpURL(CurvySpline.DOCLINK + "cginputsplineshape")]
#pragma warning disable 618
    public class InputSplineShape : SplineInputModuleBase, IExternalInput, IOnRequestProcessing, IPathProvider
#pragma warning restore 618
    {
        [HideInInspector]
        [OutputSlotInfo(typeof(CGShape))]
        public CGModuleOutputSlot OutShape = new CGModuleOutputSlot();

        #region ### Serialized Fields ###

        [Tab("General", Sort = 0)]
        [SerializeField, CGResourceManager("Shape")]
        [FieldCondition(nameof(m_Shape), null, false, ActionAttribute.ActionEnum.ShowWarning, "Missing Shape input")]
        private CurvySpline m_Shape;

        #endregion

        #region ### Public Properties ###

        public CurvySpline Shape
        {
            get { return m_Shape; }
            set
            {
                if (m_Shape != value)
                {
                    m_Shape = value;
                    OnSplineAssigned();
                    ValidateStartAndEndCps();
                }
                Dirty = true;
            }
        }

        public bool SupportsIPE { get { return FreeForm; } }
        public bool FreeForm
        {
            get
            {
                return (Shape != null && Shape.GetComponent<CurvyShape>() == null);
            }
            set
            {
                if (Shape != null)
                {
                    CurvyShape sh = Shape.GetComponent<CurvyShape>();
                    if (value && sh != null)
                        sh.Delete();
                    else if (!value && sh == null)
                        Shape.gameObject.AddComponent<CSCircle>();
                }

            }
        }

        #endregion


        #region ### IOnRequestPath ###
        public CGData[] OnSlotDataRequest(CGModuleInputSlot requestedBy, CGModuleOutputSlot requestedSlot, params CGDataRequestParameter[] requests)
        {
            CGDataRequestRasterization raster = GetRequestParameter<CGDataRequestRasterization>(ref requests);
            CGDataRequestMetaCGOptions options = GetRequestParameter<CGDataRequestMetaCGOptions>(ref requests);

            if (!raster || raster.RasterizedRelativeLength == 0)
                return null;
            CGData data = GetSplineData(Shape, false, raster, options);

            return new CGData[1] { data };

        }

        #endregion

        #region ### Public Methods ###

        public T SetManagedShape<T>() where T : CurvyShape2D
        {
            if (!Shape)
                Shape = (CurvySpline)AddManagedResource("Shape");

            CurvyShape sh = Shape.GetComponent<CurvyShape>();

            if (sh != null)
                sh.Delete();
            return Shape.gameObject.AddComponent<T>();
        }

        public void RemoveManagedShape()
        {
            if (Shape)
                DeleteManagedResource("Shape", Shape);
        }

        #endregion

        #region ### Protected members ###

        protected override CurvySpline InputSpline
        {
            get { return Shape; }
            set { Shape = value; }
        }

        protected override void OnSplineAssigned()
        {
            base.OnSplineAssigned();
            if (Shape)
                Shape.RestrictTo2D = true;
        }

        #endregion
    }
}