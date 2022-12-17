// =====================================================================
// Copyright 2013-2022 ToolBuddy
// All rights reserved
// 
// http://www.toolbuddy.net
// =====================================================================

using UnityEngine;
using UnityEditor;
using FluffyUnderware.Curvy.Generator;
using FluffyUnderware.Curvy.Generator.Modules;
using FluffyUnderware.DevToolsEditor;
using FluffyUnderware.Curvy;

namespace FluffyUnderware.CurvyEditor.Generator.Modules
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(InputSplineShape))]
    public class InputSplineShapeEditor : CGModuleEditor<InputSplineShape>
    {

        protected override void OnEnable()
        {
            base.OnEnable();
            EndIPE();
        }

        internal override void BeginIPE()
        {
            Target.Shape.ShowGizmos = true;

            if (Target.Shape.ControlPointCount > 0)
            {
                Selection.activeObject = Target.Shape.ControlPointsList[0];


                SceneView scn = SceneView.lastActiveSceneView;
                if (scn != null)
                {
                    Transform t = Target.Shape.transform;
                    scn.size = Target.Shape.Bounds.extents.magnitude * 1.5f;
                    scn.FixNegativeSize();
                    scn.LookAt(t.position + t.forward, Quaternion.LookRotation(t.forward, Vector3.up));
                }

                SceneView.RepaintAll();
            }
        }


        /// <summary>
        /// Called for the IPE Target when the module should TRS it's IPE editor to the given values
        /// </summary>
        internal override void OnIPESetTRS(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            if (Target && Target.Shape)
            {
                Target.Shape.transform.localPosition = position;
                Target.Shape.transform.localRotation = rotation;
                Target.Shape.transform.localScale = scale;
            }
        }

        internal override void EndIPE()
        {

        }



    }


}