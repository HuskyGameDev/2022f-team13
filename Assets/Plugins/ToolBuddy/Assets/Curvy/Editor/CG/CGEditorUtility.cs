// =====================================================================
// Copyright 2013-2022 ToolBuddy
// All rights reserved
// 
// http://www.toolbuddy.net
// =====================================================================

using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using FluffyUnderware.Curvy.Generator;
using FluffyUnderware.Curvy.Utils;
using System.Reflection;
using FluffyUnderware.DevToolsEditor;
using FluffyUnderware.DevToolsEditor.Extensions;
using FluffyUnderware.DevTools.Extensions;
using FluffyUnderware.DevTools;
using FluffyUnderware.Curvy;
using System.IO;

namespace FluffyUnderware.CurvyEditor.Generator
{
    public static class CGEditorUtility
    {

        static List<CGModuleOutputSlot> findOutputSlots(CurvyGenerator generator, bool includeOnRequestProcessing, System.Type filterSlotDataType = null)
        {
            List<CGModule> modules = generator.GetModules(includeOnRequestProcessing);
            List<CGModuleOutputSlot> res = new List<CGModuleOutputSlot>();
            foreach (CGModule mod in modules)
                res.AddRange(mod.GetOutputSlots(filterSlotDataType));
            return res;
        }

        public static void ShowOutputSlotsMenu(GenericMenu.MenuFunction2 func, System.Type filterSlotDataType = null)
        {
            GenericMenu mnu = new GenericMenu();
            CurvyGenerator[] generators = Component.FindObjectsOfType<CurvyGenerator>();

            mnu.AddItem(new GUIContent("none"), false, func, null);

            foreach (CurvyGenerator gen in generators)
            {
                List<CGModuleOutputSlot> slots = findOutputSlots(gen, false, filterSlotDataType);
                foreach (CGModuleOutputSlot slot in slots)
                    mnu.AddItem(new GUIContent(gen.name + "/" + slot.Module.ModuleName + "/" + slot.Info.DisplayName), false, func, slot);
            }

            mnu.ShowAsContext();
        }

        public static List<CGModule> CopyModules(IList<CGModule> sourceModules, CurvyGenerator target, Vector2 canvasPosition)
        {
            List<CGModule> res = new List<CGModule>();

            Dictionary<int, int> IDMapping = new Dictionary<int, int>();


            // I. Copy Module, store mapping from old to new ID
            foreach (CGModule mod in sourceModules)
            {
                int oldID = mod.UniqueID;
                // Duplicate module GameObject and parent it to the target Generator, also ensure a unique module name and module id
                CGModule newMod = mod.CopyTo(target);
                res.Add(newMod);
                IDMapping.Add(oldID, newMod.UniqueID);
                newMod.Properties.Dimensions.position += canvasPosition;

                // ! Handle managed Resources !
                /*
                var resourceFields = DTUtility.GetFieldsWithAttribute(mod.GetType(), typeof(CGResourceManagerAttribute), BindingFlags.Instance | BindingFlags.NonPublic);
                foreach (var fi in resourceFields)
                {
                    var v = fi.GetValue(mod) as Component;
                    // Managed?
                    if (v != null && v.transform.parent == mod.transform)
                    {
                        var newV = v.DuplicateGameObject(newMod.transform);
                        if (newV != null)
                            fi.SetValue(newMod, newV);
                    }
                }*/
                //newMod.renameManagedResourcesINTERNAL();

            }
            // II. Update Links to use the new IDs
            for (int m = 0; m < res.Count; m++)
            {
                CGModule mod = res[m];
                int newID = mod.UniqueID;

                for (int i = mod.InputLinks.Count - 1; i >= 0; i--)
                {
                    // if target module was copied as well, change both IDs
                    int newTargetID;
                    if (IDMapping.TryGetValue(mod.InputLinks[i].TargetModuleID, out newTargetID))
                    {
                        mod.InputLinks[i].SetModuleIDIINTERNAL(newID, newTargetID);
                    }
                    else // otherwise delete link
                        mod.InputLinks.RemoveAt(i);
                }
                for (int i = mod.OutputLinks.Count - 1; i >= 0; i--)
                {
                    // if target module was copied as well, change both IDs
                    int newTargetID;
                    if (IDMapping.TryGetValue(mod.OutputLinks[i].TargetModuleID, out newTargetID))
                    {
                        mod.OutputLinks[i].SetModuleIDIINTERNAL(newID, newTargetID);
                    }
                    else // otherwise delete link
                        mod.OutputLinks.RemoveAt(i);
                }
                mod.ReInitializeLinkedSlots();
            }


            /// III. Reinitialize target generator
            target.Initialize(true);

            return res;
        }

        public static bool CreateTemplate(IList<CGModule> modules, string absFilePath)
        {
            if (!Directory.Exists(Path.GetDirectoryName(absFilePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(absFilePath));

            }
            // Convert absolute to relative path
            absFilePath = absFilePath.Replace(Application.dataPath, "Assets");
            if (modules.Count == 0 || string.IsNullOrEmpty(absFilePath))
                return false;

            CurvyGenerator assetGenerator = CurvyGenerator.Create();
            assetGenerator.name = Path.GetFileNameWithoutExtension(absFilePath);
            CopyModules(modules, assetGenerator, Vector2.zero);
            foreach (CGModule mod in assetGenerator.Modules)
                mod.OnTemplateCreated();
            assetGenerator.ArrangeModules();
            PrefabUtility.SaveAsPrefabAsset(assetGenerator.gameObject, absFilePath);
            GameObject.DestroyImmediate(assetGenerator.gameObject);
            AssetDatabase.Refresh();
            return true;

        }

        public static List<CGModule> LoadTemplate(CurvyGenerator generator, string path, Vector2 canvasPosition)
        {
            CurvyGenerator srcGen = AssetDatabase.LoadAssetAtPath(path, typeof(CurvyGenerator)) as CurvyGenerator;
            if (srcGen)
                return CGEditorUtility.CopyModules(srcGen.Modules, generator, canvasPosition);
            else
                return null;
        }

        public static void SetModulesExpandedState(bool expanded, params CGModule[] modules)
        {
            foreach (CGModule mod in modules)
                mod.Properties.Expanded.target = expanded;
        }

        public static void SceneGUIPlot(Vector3[] vertices, int verticesCount, float size, Color col)
        {
            DTHandles.PushHandlesColor(col);
            for (int index = 0; index < verticesCount; index++)
            {
                Vector3 v = vertices[index];
                Handles.CubeHandleCap(0, v, Quaternion.identity, size * HandleUtility.GetHandleSize(v), EventType.Repaint);
            }

            DTHandles.PopHandlesColor();
        }
        
        [Obsolete("Use the other overload or make a copy of this method")]
        public static void SceneGUIPlot(IList<Vector3> vertices, float size, Color col)
        {
            DTHandles.PushHandlesColor(col);
            for (int index = 0; index < vertices.Count; index++)
            {
                Vector3 v = vertices[index];
                Handles.CubeHandleCap(0, v, Quaternion.identity, size * HandleUtility.GetHandleSize(v), EventType.Repaint);
            }

            DTHandles.PopHandlesColor();
        }

        public static void SceneGUILabels(Vector3[] vertices, int verticesCount, IList<string> labels, Color col, Vector2 offset)
        {
            Dictionary<Vector3, string> labelsByPos = new Dictionary<Vector3, string>();
            int ub = Mathf.Min(verticesCount, labels.Count);

            for (int i = 0; i < ub; i++)
            {
                string val;
                if (labelsByPos.TryGetValue(vertices[i], out val))
                    labelsByPos[vertices[i]] = val + "," + labels[i];
                else
                    labelsByPos.Add(vertices[i], labels[i]);
            }

            GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
            style.normal.textColor = col;
            foreach (KeyValuePair<Vector3, string> kv in labelsByPos)
                Handles.Label(DTHandles.TranslateByPixel(kv.Key, offset), kv.Value, style);

        }
        
        [Obsolete("Use the other overload or make a copy of this method")]
        public static void SceneGUILabels(IList<Vector3> vertices, IList<string> labels, Color col, Vector2 offset)
        {
            Dictionary<Vector3, string> labelsByPos = new Dictionary<Vector3, string>();
            int ub = Mathf.Min(vertices.Count, labels.Count);

            for (int i = 0; i < ub; i++)
            {
                string val;
                if (labelsByPos.TryGetValue(vertices[i], out val))
                    labelsByPos[vertices[i]] = val + "," + labels[i];
                else
                    labelsByPos.Add(vertices[i], labels[i]);
            }

            GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
            style.normal.textColor = col;
            foreach (KeyValuePair<Vector3, string> kv in labelsByPos)
                Handles.Label(DTHandles.TranslateByPixel(kv.Key, offset), kv.Value, style);

        }

        public static void SceneGUIPoly(IEnumerable<Vector3> vertices, Color col)
        {
            DTHandles.PushHandlesColor(col);

            Handles.DrawPolyLine(vertices as Vector3[]);
            DTHandles.PopHandlesColor();
        }


    }


}