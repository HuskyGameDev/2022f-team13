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

namespace FluffyUnderware.CurvyEditor.Generator.Modules
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(CreatePathLineRenderer))]
    public class CreatePathLineRendererEditor : CGModuleEditor<CreatePathLineRenderer>
    {

        protected override void OnCustomInspectorGUIBefore()
        {
            base.OnCustomInspectorGUIBefore();
            EditorGUILayout.HelpBox("Please edit parameters in inspector!", MessageType.Info);
            if (GUILayout.Button("Select Inspector"))
                Selection.activeGameObject = Target.gameObject;
        }

    }

}