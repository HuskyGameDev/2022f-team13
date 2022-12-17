// =====================================================================
// Copyright 2013-2017 Fluffy Underware
// All rights reserved
// 
// http://www.fluffyunderware.com
// =====================================================================

using UnityEngine;
using UnityEditor;

namespace FluffyUnderware.DevToolsEditor
{
    public class DTDebugWindow : EditorWindow
    {
        string Label;
        public string Text
        {
            get { return string.Join("", texts); }
            set
            {
                int size = Mathf.FloorToInt(value.Length / 16382);
                texts = new string[size];
                for (int i = 0; i < size; i++)
                {
                    texts[i] = value.Substring(i * 16382, 16382);
                }
            }
        }
        Vector2 scroll;

        string[] texts;

        GUIStyle stHtmlArea;

        public static void Open(string label, string text, string windowName="Debug Dump")
        {
            DTDebugWindow win = EditorWindow.GetWindow<DTDebugWindow>(true, windowName);
            win.Label = label;
            win.Text = text;
        }

        void OnGUI()
        {
            if (stHtmlArea == null)
            {
                stHtmlArea = new GUIStyle(EditorStyles.textArea);
                stHtmlArea.richText = true;
            }
            GUILayout.Label(Label, EditorStyles.boldLabel);
            scroll = GUILayout.BeginScrollView(scroll);
            for (int i=0;i<texts.Length;i++)
                if (i==texts.Length-1)
                    GUILayout.TextArea(texts[i],stHtmlArea,GUILayout.ExpandHeight(true));
                else
                    GUILayout.TextArea(texts[i], stHtmlArea);
            GUILayout.EndScrollView();
        }
    }
}
