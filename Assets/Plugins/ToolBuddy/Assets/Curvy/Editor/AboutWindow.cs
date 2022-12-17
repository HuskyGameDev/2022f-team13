// =====================================================================
// Copyright 2013-2022 ToolBuddy
// All rights reserved
// 
// http://www.toolbuddy.net
// =====================================================================

using UnityEngine;
using UnityEditor;
using FluffyUnderware.Curvy;
using FluffyUnderware.DevToolsEditor;
using System.Linq;
using FluffyUnderware.DevTools;

namespace FluffyUnderware.CurvyEditor
{

    public class AboutWindow : EditorWindow
    {
        static bool heightHasBeenSet = false;


        public static void Open()
        {
            EditorWindow.GetWindow<AboutWindow>(true, "About Curvy");
        }

        void OnEnable()
        {
            CurvyProject.Instance.ShowAboutOnLoad = false;
        }

        void OnGUI()
        {
            GUILayout.BeginVertical();
            GUILayout.Label(new GUIContent(CurvyStyles.TexLogoBig));
            DTGUI.PushContentColor(Color.black);

            GUIStyle labelStyle = new GUIStyle(EditorStyles.label);
            labelStyle.alignment = TextAnchor.UpperLeft;
            labelStyle.fontSize = 18;
            labelStyle.richText = true;

            GUI.Label(new Rect(300, 70, 215, 40), "<b>v " + CurvySpline.VERSION + "</b>", labelStyle);
            labelStyle.fontSize = 14;
            GUI.Label(new Rect(300, 95, 215, 40), "© 2013 ToolBuddy", labelStyle);
            DTGUI.PopContentColor();


            //head("Links");
            if (buttonCol("Release notes", "View release notes and upgrade instructions"))
                OpenReleaseNotes();
            if (buttonCol("Leave a review", "We've got to feed the Asset Store's algorithm"))
                Application.OpenURL("https://assetstore.unity.com/packages/tools/level-design/curvy-splines-7038");
            if (buttonCol("Custom development", "We can provide custom modifications for Curvy"))
                Application.OpenURL("mailto:admin@curvyeditor.com?subject=Curvy custom development request");
            if (buttonCol("Curvy Website", "Visit Curvy Splines' website"))
                OpenWeb();
            if (buttonCol("Our other assets", "Find our other assets on the Asset Store"))
                Application.OpenURL("https://assetstore.unity.com/publishers/304");
            if (buttonCol("Submit a bug report", "Found a bug? Please issue a bug report"))
                CurvyEditorUtility.SendBugReport();
            foot();

            GUILayout.Space(10);

            head("Learning Resources");
            if (buttonCol("View Examples", "Show examples folder in the Project window"))
                ShowExamples();
            if (buttonCol("Tutorials", "Watch some tutorials"))
                OpenTutorials();
            if (buttonCol("Documentation", "Manuals! That magic source of wisdom"))
                OpenDocs();
            if (buttonCol("API Reference", "Browse the API reference"))
                OpenAPIDocs();
            if (buttonCol("Support Forum", "Visit Support forum"))
                OpenForum();
            foot();

            GUILayout.EndVertical();

            if (!heightHasBeenSet && Event.current.type == EventType.Repaint)
                setHeightToContent();
        }

        private void setHeightToContent()
        {
            int w = 500;
            float height = GUILayoutUtility.GetLastRect().height + 10f;
            position.Set(position.x, position.y, w, height);
            minSize = new Vector2(w, height);
            maxSize = new Vector2(w, height + 1);
            heightHasBeenSet = true;
        }

        void head(string text)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(35);
            GUILayout.Label(text, EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(3);
        }

        void foot()
        {
            GUILayout.Space(5);
        }

        bool buttonCol(string btnText, string text)
        {
            return buttonCol(new GUIContent(btnText), text);
        }

        bool buttonCol(GUIContent btn, string text)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            bool res = GUILayout.Button(btn, GUILayout.Width(150));
            GUILayout.Space(20);
            EditorGUILayout.LabelField("<i>" + text + "</i>", DTStyles.HtmlLabel);
            GUILayout.EndHorizontal();
            return res;
        }

        public static void ShowExamples()
        {
            string searchString;
            searchString = "t:Folder Curvy Examples";
            string[] assetsGuids = AssetDatabase.FindAssets(searchString);
            if (assetsGuids.Any())
                EditorGUIUtility.PingObject(AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(assetsGuids.First())));
            else
                DTLog.LogError("[Curvy] Could not find the \"Curvy Examples\" folder");
        }

        public static void OpenTutorials()
        {
            Application.OpenURL(CurvySpline.DOCLINK + "tutorials");
        }
        public static void OpenReleaseNotes()
        {
            Application.OpenURL(CurvySpline.DOCLINK + "releasenotes");
        }

        public static void OpenDocs()
        {
            Application.OpenURL(CurvySpline.WEBROOT + "documentation/");
        }

        public static void OpenAPIDocs()
        {
            Application.OpenURL("https://api.curvyeditor.com/" + CurvySpline.APIVERSION + "/");
        }

        public static void OpenWeb()
        {
            Application.OpenURL(CurvySpline.WEBROOT);
        }

        public static void OpenForum()
        {
            Application.OpenURL("https://forum.curvyeditor.com");
        }


    }
}