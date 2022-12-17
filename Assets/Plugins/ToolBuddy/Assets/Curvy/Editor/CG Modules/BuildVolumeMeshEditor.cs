// =====================================================================
// Copyright 2013-2022 ToolBuddy
// All rights reserved
// 
// http://www.toolbuddy.net
// =====================================================================

using UnityEngine;
using UnityEditor;
using FluffyUnderware.Curvy.Generator;
using System.Collections.Generic;
using FluffyUnderware.Curvy.Generator.Modules;
using System.Linq;
using FluffyUnderware.DevToolsEditor;

namespace FluffyUnderware.CurvyEditor.Generator.Modules
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(BuildVolumeMesh))]
    public class BuildVolumeMeshEditor : CGModuleEditor<BuildVolumeMesh>
    {
        bool showAddButton;
        int matcount;

        public override void OnModuleDebugGUI()
        {
            CGVMesh vmesh = Target.OutVMesh.GetData<CGVMesh>();
            if (vmesh)
            {
                EditorGUILayout.LabelField("Vertices: " + vmesh.Count.ToString());
                EditorGUILayout.LabelField("Triangles: " + vmesh.TriangleCount.ToString());
                EditorGUILayout.LabelField("SubMeshes: " + vmesh.SubMeshes.Length.ToString());
            }
        }

        protected override void OnReadNodes()
        {
            ensureMaterialTabs();
        }

        void ensureMaterialTabs()
        {
            DTGroupNode tabbar = Node.FindTabBarAt("Default");

            if (tabbar == null)
                return;

            tabbar.MaxItemsPerRow = 4;
            for (int i = 0; i < Target.MaterialCount; i++)
            {
                string tabName = string.Format("Mat {0}", i);
                if (tabbar.Count <= i + 1)
                    tabbar.AddTab(tabName, OnRenderTab);
                else
                {
                    tabbar[i + 1].Name = tabName;
                    tabbar[i + 1].GUIContent.text = tabName;
                }
            }
            while (tabbar.Count > Target.MaterialCount + 1)
                tabbar[tabbar.Count - 1].Delete();
            matcount = Target.MaterialCount;
        }

        void OnRenderTab(DTInspectorNode node)
        {
            int idx = node.Index - 1;

            if (idx >= 0 && idx < Target.MaterialCount)
            {
                CGMaterialSettingsEx mat = Target.MaterialSettings[idx];
                EditorGUI.BeginChangeCheck();

                bool matSwapUv = EditorGUILayout.Toggle("Swap UV", mat.SwapUV);
                if (matSwapUv != mat.SwapUV)
                {
                    Undo.RegisterCompleteObjectUndo(Target, "Modify Swap UV");
                    mat.SwapUV = matSwapUv;
                }

                CGKeepAspectMode cgKeepAspectMode = (CGKeepAspectMode)EditorGUILayout.EnumPopup("Keep Aspect", mat.KeepAspect);
                if (cgKeepAspectMode != mat.KeepAspect)
                {
                    Undo.RegisterCompleteObjectUndo(Target, "Modify Keep Aspect");
                    mat.KeepAspect = cgKeepAspectMode;
                }

                Vector2 matUvOffset = EditorGUILayout.Vector2Field("UV Offset", mat.UVOffset);
                if (matUvOffset != mat.UVOffset)
                {
                    Undo.RegisterCompleteObjectUndo(Target, "Modify UV Offset");
                    mat.UVOffset = matUvOffset;
                }

                Vector2 matUvScale = EditorGUILayout.Vector2Field("UV Scale", mat.UVScale);
                if (matUvScale != mat.UVScale)
                {
                    Undo.RegisterCompleteObjectUndo(Target, "Modify UV Scale");
                    mat.UVScale = matUvScale;
                }

                Target.SetMaterial(idx, EditorGUILayout.ObjectField("Material", Target.GetMaterial(idx), typeof(Material), true) as Material);

                if (Target.MaterialCount > 1 && GUILayout.Button("Remove"))
                {
                    Target.RemoveMaterial(idx);
                    node.Delete();
                    ensureMaterialTabs();
                    GUIUtility.ExitGUI();
                }
                if (EditorGUI.EndChangeCheck())
                {
                    Target.Dirty = true;
                    EditorUtility.SetDirty(Target);
                }
            }
        }

        void CBAddMaterial()
        {
            if (DTGUI.IsLayout)
                showAddButton = Node.FindTabBarAt("Default").SelectedIndex == 0;
            if (showAddButton)
            {
                if (GUILayout.Button("Add Material Group"))
                {
                    Target.AddMaterial();
                    ensureMaterialTabs();
                    GUIUtility.ExitGUI();
                }
            }

        }

        protected override void OnCustomInspectorGUI()
        {
            base.OnCustomInspectorGUI();
            if (matcount != Target.MaterialCount)
                ensureMaterialTabs();
        }
    }
}