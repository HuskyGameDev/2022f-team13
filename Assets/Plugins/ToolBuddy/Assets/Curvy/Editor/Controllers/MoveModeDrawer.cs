// =====================================================================
// Copyright 2013-2022 ToolBuddy
// All rights reserved
// 
// http://www.toolbuddy.net
// =====================================================================

using FluffyUnderware.Curvy.Controllers;
using UnityEngine;
using UnityEditor;

namespace FluffyUnderware.CurvyEditor.Controllers
{
    [CustomPropertyDrawer(typeof(CurvyController.MoveModeEnum))]
    public class MoveModeDrawer : PropertyDrawer
    {
        readonly GUIContent[] options = new[] { new GUIContent("Relative", "Speed is expressed as spline lengths per second"), new GUIContent("Absolute", "Speed is expressed as world units per second") };
        readonly GUIStyle guiStyle = EditorStyles.popup;

        override public void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            property.intValue = EditorGUI.Popup(position, label, property.intValue, options, guiStyle);
            EditorGUI.EndProperty();
        }
    }
}