// =====================================================================
// Copyright 2013-2022 ToolBuddy
// All rights reserved
// 
// http://www.toolbuddy.net
// =====================================================================

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using FluffyUnderware.Curvy.Generator;
using FluffyUnderware.Curvy.Generator.Modules;
using FluffyUnderware.DevToolsEditor;

namespace FluffyUnderware.CurvyEditor.Generator.Modules
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(BuildVolumeCaps))]
    public class BuildVolumeCapsEditor : CGModuleEditor<BuildVolumeCaps>
    {


        public override void OnModuleDebugGUI()
        {
            CGVMesh vmesh = Target.OutVMesh.GetData<CGVMesh>();
            if (vmesh)
            {
                EditorGUILayout.LabelField("Vertices: " + vmesh.Count.ToString());
                EditorGUILayout.LabelField("Triangles: " + vmesh.TriangleCount.ToString());
                EditorGUILayout.LabelField("SubMeshes: " + vmesh.SubMeshes.Length.ToString());
            }
        }


    }

}