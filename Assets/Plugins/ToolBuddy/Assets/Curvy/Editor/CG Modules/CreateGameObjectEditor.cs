// =====================================================================
// Copyright 2013-2022 ToolBuddy
// All rights reserved
// 
// http://www.toolbuddy.net
// =====================================================================

using UnityEngine;
using UnityEditor;
using FluffyUnderware.Curvy.Generator;
using System.Collections.Generic;
using FluffyUnderware.Curvy.Generator.Modules;
using FluffyUnderware.DevToolsEditor;

namespace FluffyUnderware.CurvyEditor.Generator.Modules
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(CreateGameObject))]
    public class CreateGameObjectEditor : CGModuleEditor<CreateGameObject>
    {
        protected override void OnReadNodes()
        {
            base.OnReadNodes();
            Node.FindTabBarAt("Default").AddTab("Export", OnExportTab);
        }

        void OnExportTab(DTInspectorNode node)
        {
            GUI.enabled = Target.GameObjects.Count > 0;
            if (GUILayout.Button("Save To Scene"))
                Target.SaveToScene();
            GUI.enabled = true;
        }
    }
}