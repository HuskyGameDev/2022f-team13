// =====================================================================
// Copyright 2013-2022 ToolBuddy
// All rights reserved
// 
// http://www.toolbuddy.net
// =====================================================================

using UnityEngine;
using UnityEditor;
using System.IO;
using FluffyUnderware.DevTools.Extensions;
using FluffyUnderware.Curvy.Utils;
using FluffyUnderware.DevTools;

namespace FluffyUnderware.CurvyEditor
{
    public class ShapeWizard : EditorWindow
    {

        string mShapeClassName = "CS";
        string mShapeMenuName = "Custom/";
        bool mShapeIs2D = true;
        bool mNeedFocus=true;

        string mShapeParent = "CurvyShape";
        readonly string mShapeScriptPath = CurvyProject.Instance.CustomizationRootPath + CurvyProject.RELPATH_SHAPEWIZARDSCRIPTS;

        string ScriptTemplate
        {
            get
            {
                return CurvyEditorUtility.GetPackagePathAbsolute("ClassTemplates/ShapeTemplate.txt"); 
            }
        }

        string ShapeFileName
        {
            get
            {
                return Application.dataPath + "/" + mShapeScriptPath.TrimEnd('/', '\\') + "/" + mShapeClassName + ".cs";
            }
        }

       
        public static void Open()
        {
            ShapeWizard win = EditorWindow.GetWindow<ShapeWizard>(true, "Create Shape");
            win.minSize = new Vector2(500, 60);
        }

        void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            GUI.SetNextControlName("ClassName");
            mShapeClassName = EditorGUILayout.TextField("Class Name", mShapeClassName);
            
            if (EditorGUI.EndChangeCheck())
            {
                mShapeMenuName = "Custom/"+ObjectNames.NicifyVariableName(mShapeClassName.TrimStart("CS"));
            }
            
            mShapeMenuName = EditorGUILayout.TextField("Menu Name", mShapeMenuName);
            mShapeIs2D = EditorGUILayout.Toggle("Is 2D", mShapeIs2D);

            GUI.enabled = !string.IsNullOrEmpty(mShapeScriptPath) &&
                          !string.IsNullOrEmpty(mShapeClassName) &&
                          !string.IsNullOrEmpty(mShapeMenuName);
            if (GUILayout.Button("Create"))
            {
                CreateShape();
            }
            GUI.enabled = true;


            if (mNeedFocus)
            {
                EditorGUI.FocusTextInControl("ClassName");
                mNeedFocus = false;
            }

        }

        void CreateShape()
        {
            if (!File.Exists(ScriptTemplate))
            {
                DTLog.LogError("[Curvy] Missing Shape Template file '" + ScriptTemplate + "'!");
                return;
            }

            mShapeParent = (mShapeIs2D) ? "CurvyShape2D" : "CurvyShape";

            // Script
            string template = File.ReadAllText(ScriptTemplate);
            if (!string.IsNullOrEmpty(template))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(ShapeFileName));
                StreamWriter stream = File.CreateText(ShapeFileName);
                stream.Write(replaceVars(template));
                stream.Close();

            }
            else
            {
                DTLog.LogError("[Curvy] Unable to load template file");
                return;
            }
           
            AssetDatabase.Refresh();
            Close();
            EditorUtility.DisplayDialog("Shape Script Wizard", "Script successfully created!", "OK");
            Selection.activeObject = AssetDatabase.LoadMainAssetAtPath("Assets/"+mShapeScriptPath + "/" + mShapeClassName + ".cs");
        }

        string replaceVars(string template)
        {
            return template.Replace("%MENUNAME%", mShapeMenuName)
                           .Replace("%CLASSNAME%", mShapeClassName)
                           .Replace("%PARENT%",mShapeParent)
                           .Replace("%IS2D%",mShapeIs2D.ToString().ToLower());
        }
    }
}