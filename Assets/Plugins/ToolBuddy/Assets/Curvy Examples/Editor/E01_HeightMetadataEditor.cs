// =====================================================================
// Copyright 2013-2022 ToolBuddy
// All rights reserved
// 
// http://www.toolbuddy.net
// =====================================================================

using UnityEngine;
using UnityEditor;
using System.Collections;
using FluffyUnderware.DevToolsEditor;
using FluffyUnderware.Curvy.Examples;
using FluffyUnderware.CurvyEditor;

namespace FluffyUnderware.Curvy.ExamplesEditor
{

    [CanEditMultipleObjects]
    [CustomEditor(typeof(E01_HeightMetadata))]
    public class E01_HeightMetadataEditor : DTEditor<E01_HeightMetadata>
    {

        [DrawGizmo(GizmoType.Active | GizmoType.NonSelected | GizmoType.InSelectionHierarchy)]
        static void GizmoDrawer(E01_HeightMetadata data, GizmoType context)
        {
            if (CurvyGlobalManager.ShowMetadataGizmo && data.Spline.ShowGizmos)
            {
                Vector3 position = data.ControlPoint.transform.position;
#pragma warning disable CS0618
                CurvyGizmo.PointLabel(position, data.MetaDataValue.ToString(), OrientationAxisEnum.Down);
#pragma warning restore CS0618
            }
        }
    }
}