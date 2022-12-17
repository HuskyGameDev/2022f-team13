// =====================================================================
// Copyright 2013-2022 ToolBuddy
// All rights reserved
// 
// http://www.toolbuddy.net
// =====================================================================

using System;
using UnityEngine;
using UnityEditor;
using FluffyUnderware.CurvyEditor;
using System.Collections.Generic;
using FluffyUnderware.Curvy.Generator;
using FluffyUnderware.CurvyEditor.Generator;

namespace FluffyUnderware.CurvyEditor.Generator
{
    public class TemplateWizard : EditorWindow
    {
        string mName;
        List<CGModule> mModules;
        private CanvasUI canvasUI;

        public static void Open(List<CGModule> modules, CanvasUI canvasUI)
        {
            if (modules == null || modules.Count == 0)
                return;
            TemplateWizard win = EditorWindow.GetWindow<TemplateWizard>(true, "Save Template");
            win.minSize = new Vector2(300, 90);
            win.maxSize = win.minSize;
            win.mName = "";
            win.mModules = modules;
            win.canvasUI = canvasUI;
        }

        void OnGUI()
        {
            EditorGUILayout.HelpBox("Only Managed Resources will be saved!", MessageType.Warning);
            GUI.SetNextControlName("TPLWIZ_txtName");
            mName = EditorGUILayout.TextField("Template Menu Name", mName).TrimStart('/');

            

            EditorGUILayout.BeginHorizontal();
            GUI.enabled = (!string.IsNullOrEmpty(mName));
            if (GUILayout.Button("Save"))
            {
                Save();
                Close();
            }
            GUI.enabled = true;
            if (GUILayout.Button("Cancel"))
                Close();
            EditorGUILayout.EndHorizontal();

            if (GUI.GetNameOfFocusedControl() == "")
                EditorGUI.FocusTextInControl("TPLWIZ_txtName");
        }

        void Save()
        {
            string absFolder = Application.dataPath + "/" + CurvyProject.Instance.CustomizationRootPath + CurvyProject.RELPATH_CGTEMPLATES;
            string file = absFolder + "/" + mName + ".prefab";
            if (!System.IO.File.Exists(file) || EditorUtility.DisplayDialog("Replace File?", "The file already exists! Replace it?", "Yes", "No"))
            {
                if (CGEditorUtility.CreateTemplate(mModules, file))
                {
                    EditorUtility.DisplayDialog("Save Generator Template", "Template successfully saved!", "Ok");
                    if (canvasUI != null)
                        canvasUI.ReloadTemplates();
                }
            }

        }
    }
}