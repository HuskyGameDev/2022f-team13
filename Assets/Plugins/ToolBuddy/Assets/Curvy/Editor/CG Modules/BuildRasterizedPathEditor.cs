// =====================================================================
// Copyright 2013-2022 ToolBuddy
// All rights reserved
// 
// http://www.toolbuddy.net
// =====================================================================

using UnityEngine;
using UnityEditor;
using FluffyUnderware.Curvy.Generator.Modules;
using System.Collections.Generic;
using FluffyUnderware.Curvy.Generator;

namespace FluffyUnderware.CurvyEditor.Generator.Modules
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(BuildRasterizedPath))]
    public class BuildRasterizedPathEditor : CGModuleEditor<BuildRasterizedPath>
    {
        protected override void OnEnable()
        {
            base.OnEnable();
            HasDebugVisuals = true;
        }

        public override void OnModuleSceneDebugGUI()
        {
            base.OnModuleSceneDebugGUI();
            CGPath data = Target.OutPath.GetData<CGPath>();
            if (data)
            {
                Handles.matrix = Target.Generator.transform.localToWorldMatrix;
                CGEditorUtility.SceneGUIPlot(data.Positions.Array, data.Positions.Count, 0.1f, Color.white);
                Handles.matrix = Matrix4x4.identity;
            }
        }

        public override void OnModuleDebugGUI()
        {
            CGPath data = Target.OutPath.GetData<CGPath>();
            if (data)
            {
                EditorGUILayout.LabelField("Samples: " + data.Count.ToString());
            }
        }
    }




}