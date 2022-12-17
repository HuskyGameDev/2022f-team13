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
    [CustomEditor(typeof(GameObjectToMesh))]
    public class GameObjectToMeshEditor : CGModuleEditor<GameObjectToMesh>
    {

        // Scene View GUI - Called only if the module is initialized and configured
        //public override void OnModuleSceneGUI() {}

        // Scene View Debug GUI - Called only when Show Debug Visuals is activated
        //public override void OnModuleSceneDebugGUI() {}

        // Inspector Debug GUI - Called only when Show Debug Values is activated
        //public override void OnModuleDebugGUI() {}

    }

}