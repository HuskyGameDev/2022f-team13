// =====================================================================
// Copyright 2013-2022 ToolBuddy
// All rights reserved
// 
// http://www.toolbuddy.net
// =====================================================================

using UnityEngine;
using UnityEditor;
using System.IO;
using FluffyUnderware.DevTools;

namespace FluffyUnderware.CurvyEditor.Generator
{
    public class ModuleWizard : EditorWindow
    {
        
        string mModuleClassName = string.Empty;
        string mModuleMenuName = string.Empty;
        string mModuleName = string.Empty;
        string mModuleDescription = string.Empty;
        readonly string mModuleScriptPath = CurvyProject.Instance.CustomizationRootPath + CurvyProject.RELPATH_CGMODULEWIZARDSCRIPTS;
        readonly string mModuleEditorScriptPath = CurvyProject.Instance.CustomizationRootPath + CurvyProject.RELPATH_CGMODULEWIZARDEDITORSCRIPTS;

        bool mNeedFocus = true;

        string ScriptTemplate
        {
            get 
            {
                return CurvyEditorUtility.GetPackagePathAbsolute("ClassTemplates/CGModuleTemplate.txt"); 
            }

        }
        string EditorScriptTemplate
        {
            get
            {
                return CurvyEditorUtility.GetPackagePathAbsolute("ClassTemplates/CGModuleEditorTemplate.txt");
            }
        }

        string ModuleFileName
        {
            get
            {
                return Application.dataPath+"/"+mModuleScriptPath.TrimEnd('/','\\') + "/" + mModuleClassName + ".cs";
            }
        }

        string ModuleEditorFileName
        {
            get
            {
                return Application.dataPath + "/" + mModuleEditorScriptPath.TrimEnd('/', '\\') + "/" + mModuleClassName + "Editor.cs";
            }
        }

        public static void Open()
        {
            ModuleWizard win=EditorWindow.GetWindow<ModuleWizard>(true, "Create CG Module");
            win.minSize = new Vector2(500, 120);
            
        }

        void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            GUI.SetNextControlName("ClassName");
            mModuleClassName = EditorGUILayout.TextField(new GUIContent("Class Name","C# class name"), mModuleClassName);
            

            if (EditorGUI.EndChangeCheck())
            {
                mModuleName = ObjectNames.NicifyVariableName(mModuleClassName);
                mModuleMenuName = "Custom/" + mModuleName;
            }
            mModuleName = EditorGUILayout.TextField(new GUIContent("Module Name","The default module instance name"), mModuleName);
            mModuleMenuName = EditorGUILayout.TextField(new GUIContent("Menu Name","Name to show in the CG menu"), mModuleMenuName);
            
            EditorGUILayout.PrefixLabel("Description");
            mModuleDescription = EditorGUILayout.TextArea(mModuleDescription);

            GUI.enabled = !string.IsNullOrEmpty(mModuleScriptPath) &&
                          !string.IsNullOrEmpty(mModuleEditorScriptPath) &&
                          !string.IsNullOrEmpty(mModuleClassName) &&
                          !string.IsNullOrEmpty(mModuleMenuName) &&
                          !string.IsNullOrEmpty(mModuleName);
            if (GUILayout.Button("Create"))
                CreateModule();

            GUI.enabled = true;

            if (mNeedFocus)
            {
                EditorGUI.FocusTextInControl("ClassName");
                mNeedFocus = false;
            }
                
        }

        void CreateModule()
        {
            if (!File.Exists(ScriptTemplate)){
                DTLog.LogError("[Curvy] Missing Module Template file '"+ScriptTemplate+"'!");
                return;
            }
            if (!File.Exists(EditorScriptTemplate))
            {
                DTLog.LogError("[Curvy] Missing Module Template file '" + EditorScriptTemplate + "'!");
                return;
            }

            // Script
            string template = File.ReadAllText(ScriptTemplate);
            if (!string.IsNullOrEmpty(template))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(ModuleFileName));
                StreamWriter stream = File.CreateText(ModuleFileName);
                stream.Write(replaceVars(template));
                stream.Close();

            }
            else
            {
                DTLog.LogError("[Curvy] Unable to load template file");
                return;
            }
            // Editor Script
            template = File.ReadAllText(EditorScriptTemplate);
            if (!string.IsNullOrEmpty(template))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(ModuleEditorFileName));
                StreamWriter stream = File.CreateText(ModuleEditorFileName);
                stream.Write(replaceVars(template));
                stream.Close();
            }
            else
            {
                DTLog.LogError("[Curvy] Unable to load editor template file");
                return;
            }
            AssetDatabase.Refresh();
            Close();
            EditorUtility.DisplayDialog("CG Module Wizard", "Scripts successfully created!", "OK");
            
            Selection.objects = new Object[2]
            {
                AssetDatabase.LoadMainAssetAtPath(ModuleFileName.Replace(Application.dataPath,"Assets")),
                AssetDatabase.LoadMainAssetAtPath(ModuleEditorFileName.Replace(Application.dataPath,"Assets"))
            };
        }

        string replaceVars(string template)
        {
            return template.Replace("%MENUNAME%", mModuleMenuName)
                           .Replace("%MODULENAME%", mModuleName)
                           .Replace("%DESCRIPTION%", mModuleDescription)
                           .Replace("%CLASSNAME%", mModuleClassName);
        }
    }
}