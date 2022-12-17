// =====================================================================
// Copyright 2013-2022 ToolBuddy
// All rights reserved
// 
// http://www.toolbuddy.net
// =====================================================================

using System;
using UnityEngine;
using UnityEditor;
using System.IO;
using FluffyUnderware.Curvy;
using FluffyUnderware.DevTools.Extensions;
using FluffyUnderware.DevToolsEditor;
using FluffyUnderware.DevTools;
using JetBrains.Annotations;
using Application = UnityEngine.Application;

namespace FluffyUnderware.CurvyEditor
{

    public static class CurvyEditorUtility
    {
        public static void SendBugReport()
        {
            string par = string.Format("@Operating System@={0}&@Unity Version@={1}&@Curvy Version@={2}", SystemInfo.operatingSystem, Application.unityVersion, CurvySpline.VERSION);
            Application.OpenURL(CurvySpline.WEBROOT + "bugreport?" + par.Replace(" ", "%20"));
        }

        public static void GenerateAssemblyDefinitions()
        {
            string curvyRootPath = GetCurvyRootPath();
            if (String.IsNullOrEmpty(curvyRootPath))
            {
                DTLog.LogError("[Curvy] Assembly Definitions generation aborted, couldn't locate the installation folder");
            }
            else
            {
                string curvyRootPathAbsolute = Application.dataPath + "/" + curvyRootPath;
                DirectoryInfo parentInfo = Directory.GetParent(curvyRootPathAbsolute).Parent;
                string assetsParentDirectory = parentInfo.FullName;
                string toolbuddyDirectory = parentInfo.Parent.FullName;

                GenerateAssemblyDefinition($"{assetsParentDirectory}/Arrays Pooling/ToolBuddy.ArraysPooling.asmdef",
                    @"
{
	""name"":""ToolBuddy.ArraysPooling""
}");

                GenerateAssemblyDefinition($"{toolbuddyDirectory}/Dependencies/Vector Graphics/ToolBuddy.Dependencies.VectorGraphics.asmdef",
                    @"
{
	""name"":""ToolBuddy.Dependencies.VectorGraphics""
}");

                GenerateAssemblyDefinition($"{toolbuddyDirectory}/Dependencies/DevTools/FluffyUnderware.DevTools.asmdef",
                    @"
{
	""name"":""FluffyUnderware.DevTools""
}");

                GenerateAssemblyDefinition($"{toolbuddyDirectory}/Dependencies/LibTessDotNet/LibTessDotNet.asmdef",
                @"
{
	""name"":""LibTessDotNet"",
    ""references"":[
        ""ToolBuddy.ArraysPooling""
    ],
    ""includePlatforms"":[],
    ""excludePlatforms"":[]
}");

                GenerateAssemblyDefinition($"{toolbuddyDirectory}/Dependencies/DevTools/Editor/FlufyUnderware.DevTools.Editor.asmdef",
                @"
{
    ""name"":""FluffyUnderware.DevTools.Editor"",
    ""references"":[
        ""ToolBuddy.ArraysPooling"",
        ""FluffyUnderware.DevTools""
    ],
    ""includePlatforms"":[
        ""Editor""
    ],
    ""excludePlatforms"":[]
}");

                GenerateAssemblyDefinition($"{assetsParentDirectory}/Curvy/ToolBuddy.Curvy.asmdef",
                @"
{
    ""name"":""ToolBuddy.Curvy"",
    ""references"":[
        ""ToolBuddy.ArraysPooling"",
        ""ToolBuddy.Dependencies.VectorGraphics"",
        ""FluffyUnderware.DevTools"",
        ""LibTessDotNet""
    ],
    ""includePlatforms"":[],
    ""excludePlatforms"":[]
}");

                GenerateAssemblyDefinition($"{assetsParentDirectory}/Curvy/Editor/ToolBuddy.Curvy.Editor.asmdef",
                @"
{
    ""name"":""ToolBuddy.Curvy.Editor"",
    ""references"":[
        ""ToolBuddy.ArraysPooling"",
        ""ToolBuddy.Curvy"",
        ""FluffyUnderware.DevTools"",
        ""FluffyUnderware.DevTools.Editor"",
        ""LibTessDotNet""
    ],
    ""includePlatforms"":[
        ""Editor""
    ],
    ""excludePlatforms"":[]
}");

                GenerateAssemblyDefinition($"{assetsParentDirectory}/Curvy Examples/ToolBuddy.Curvy.Examples.asmdef",
                @"
{
    ""name"":""ToolBuddy.Curvy.Examples"",
    ""references"":[
        ""ToolBuddy.ArraysPooling"",
        ""FluffyUnderware.DevTools"",
        ""ToolBuddy.Curvy""
    ],
    ""includePlatforms"":[],
    ""excludePlatforms"":[]
}");

                GenerateAssemblyDefinition($"{assetsParentDirectory}/Curvy Examples/Editor/ToolBuddy.Curvy.Examples.Editor.asmdef",
                @"
{
    ""name"":""ToolBuddy.Curvy.Examples.Editor"",
    ""references"":[
        ""ToolBuddy.ArraysPooling"",
        ""FluffyUnderware.DevTools"",
        ""FluffyUnderware.DevTools.Editor"",
        ""ToolBuddy.Curvy"",
        ""ToolBuddy.Curvy.Editor"",
        ""ToolBuddy.Curvy.Examples""
    ],
    ""includePlatforms"":[
        ""Editor""
    ],
    ""excludePlatforms"":[]
}");

                AssetDatabase.Refresh();
            }
        }

        private static void GenerateAssemblyDefinition(string filePath, string fileContent)
        {
            DirectoryInfo directory = Directory.GetParent(filePath);
            if (Directory.Exists(directory.FullName) == false)
                EditorUtility.DisplayDialog("Missing directory",
                    String.Format("Could not find the directory '{0}', file generation will be skipped", directory.FullName), "Continue");
            else if (!File.Exists(filePath) || EditorUtility.DisplayDialog("Replace File?", String.Format("The file '{0}' already exists! Replace it?", filePath), "Yes", "No"))
                using (StreamWriter streamWriter = File.CreateText(filePath))
                {
                    streamWriter.WriteLine(fileContent);
                }
        }


        /// <summary>
        /// Converts a path/file relative to Curvy's root path to the real path, e.g. "ReadMe.txt" gives "Curvy/ReadMe.txt"
        /// </summary>
        /// <param name="relativePath">a path/file inside the Curvy package, WITHOUT the leading Curvy</param>
        /// <returns>the real path, relative to Assets</returns>
        public static string GetPackagePath(string relativePath)
        {
            return GetCurvyRootPath() + relativePath.TrimStart('/', '\\');
        }
        /// <summary>
        /// Converts a path/file relative to Curvy's root path to the real absolute path
        /// </summary>
        /// <param name="relativePath">a path/file inside the Curvy package, WITHOUT the leading Curvy</param>
        /// <returns>the absolute system path</returns>
        public static string GetPackagePathAbsolute(string relativePath)
        {
            return Application.dataPath + "/" + GetPackagePath(relativePath);
        }

        /// <summary>
        /// Gets the Curvy folder relative path, e.g. "Plugins/Curvy/" by default
        /// </summary>
        /// <returns></returns>
        public static string GetCurvyRootPath()
        {
            // Quick check for the regular path
            if (File.Exists(Application.dataPath + "/Plugins/ToolBuddy/Assets/Curvy/Scripts/Splines/CurvySpline.cs"))
                return "Plugins/ToolBuddy/Assets/Curvy/";


            // Still no luck? Do a project search
            string[] guid = AssetDatabase.FindAssets("curvyspline_private"); //FindAssets("curvyspline") returns also files other than CurvySpline.cs
            if (guid.Length == 0)
            {
                DTLog.LogError("[Curvy] Unable to locate CurvySpline_private.cs in the project! Is the Curvy package fully imported?");
                return null;
            }
            else
                return AssetDatabase.GUIDToAssetPath(guid[0]).TrimStart("Assets/").TrimEnd("Scripts/Splines/CurvySpline_private.cs");
        }

        /// <summary>
        /// Gets the Curvy folder absolute path, i.e. Application.dataPath+"/"+CurvyEditorUtility.GetCurvyRootPath()
        /// </summary>
        /// <returns></returns>
        public static string GetCurvyRootPathAbsolute()
        {
            return Application.dataPath + "/" + GetCurvyRootPath();
        }
    }

    public static class CurvyGizmo
    {
        /// <summary>
        /// Displays a label next to a point. The relative position of the label compared to the point is defined by <paramref name="direction"/>
        /// </summary>
#if CURVY_SANITY_CHECKS_PRIVATE
        [Obsolete("Do not call this method from a method not having the DrawGizmo attribute until the issue with Unity 2021.2 is fixed")]
#endif
        public static void PointLabel(Vector3 pointPosition, String label, OrientationAxisEnum direction, float? handleSize = null, [CanBeNull] GUIStyle style = null)
        {
#if UNITY_2021_2_0 || UNITY_2021_2_1 || UNITY_2021_2_2 || UNITY_2021_2_3 || UNITY_2021_2_4 || UNITY_2021_2_5 || UNITY_2021_2_6 || UNITY_2021_2_7 || UNITY_2021_2_8 || UNITY_2021_2_9 || UNITY_2021_2_10 || UNITY_2021_2_11
            //workaround to this issue: https://issuetracker.unity3d.com/issues/handles-dot-label-does-not-appear-in-the-supposed-place
            //the issue seems to not happen when this method is called from a OnGui method.
            pointPosition = DTHandles.TranslateByPixel(pointPosition, -53, 23);
#endif
            //ugly shit to bypass the joke that is style.alignment. Tried to bypass the issue by using style.CalcSize(new GUIContent(label)) to manually place the labels. No luck with that
            while (label.Length <= 5)
                label = $" {label} ";

            if (handleSize.HasValue == false)
                handleSize = HandleUtility.GetHandleSize(pointPosition);

            style = style ?? CurvyStyles.GizmoText;

            pointPosition -= Camera.current.transform.right * handleSize.Value * 0.1f;
            pointPosition += Camera.current.transform.up * handleSize.Value * 0.1f;
            Vector3 labelPosition;
            switch (direction)
            {
                case OrientationAxisEnum.Up:
                    //style.alignment = TextAnchor.LowerCenter;
                    labelPosition = pointPosition;
                    labelPosition += Camera.current.transform.up * handleSize.Value * 0.3f;
                    break;
                case OrientationAxisEnum.Down:
                    //style.alignment = TextAnchor.UpperCenter;
                    labelPosition = pointPosition;
                    labelPosition -= Camera.current.transform.up * handleSize.Value * 0.3f;
                    break;
                case OrientationAxisEnum.Right:
                    //style.alignment = TextAnchor.MiddleLeft;
                    labelPosition = pointPosition;
                    labelPosition += Camera.current.transform.right * handleSize.Value * 0.4f;
                    break;
                case OrientationAxisEnum.Left:
                    //style.alignment = TextAnchor.MiddleRight;
                    labelPosition = pointPosition;
                    labelPosition -= Camera.current.transform.right * handleSize.Value * 0.45f;
                    break;
                case OrientationAxisEnum.Forward:
                case OrientationAxisEnum.Backward:
                    //style.alignment = TextAnchor.MiddleCenter;
                    labelPosition = pointPosition;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }

            Handles.Label(labelPosition, label, style);
        }
    }
    public static class CurvyGUI
    {

        #region ### GUI Controls ###

        public static bool Foldout(ref bool state, string text) { return Foldout(ref state, new GUIContent(text), null); }
        public static bool Foldout(ref bool state, string text, string helpURL) { return Foldout(ref state, new GUIContent(text), helpURL); }

        public static bool Foldout(ref bool state, GUIContent content, string helpURL, bool hierarchyMode = true)
        {
            Rect controlRect = GUILayoutUtility.GetRect(content, CurvyStyles.Foldout);
            bool isInsideInspector = DTInspectorNode.IsInsideInspector;
            int xOffset = isInsideInspector ? 12 : -2;
            controlRect.x -= xOffset;
            controlRect.width += (isInsideInspector ? 0 : 1);

            int indentLevel = DTInspectorNodeDefaultRenderer.RenderHeader(controlRect, xOffset, helpURL, content, ref state);

            EditorGUI.indentLevel = indentLevel;

            return state;
        }

        #endregion

    }
}