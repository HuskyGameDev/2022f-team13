// =====================================================================
// Copyright 2013-2022 ToolBuddy
// All rights reserved
// 
// http://www.toolbuddy.net
// =====================================================================

using UnityEngine;
using UnityEditor;
using FluffyUnderware.Curvy.Controllers;

namespace FluffyUnderware.CurvyEditor.Controllers
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(VolumeController), true)]
    public class VolumeControllerEditor : CurvyControllerEditor<VolumeController>
    {
    }
}