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
using System.Reflection;
using FluffyUnderware.Curvy.Generator;
using FluffyUnderware.DevTools.Extensions;
using FluffyUnderware.Curvy.Utils;
using FluffyUnderware.Curvy;
using FluffyUnderware.Curvy.Generator.Modules;
using FluffyUnderware.DevTools;
using Object = UnityEngine.Object;

namespace FluffyUnderware.CurvyEditor.Generator
{


    public class CanvasUI
    {
        public static CGClipboard Clipboard = new CGClipboard();
        readonly CGGraph Parent;
        /// <summary>
        /// Gets ModuleInfo->Module Type mapping
        /// </summary>
        SortedDictionary<ModuleInfoAttribute, System.Type> TypeByModuleInfo = new SortedDictionary<ModuleInfoAttribute, System.Type>();
        /// <summary>
        /// Gets Modules that accept a certain input data type
        /// </summary>
        readonly Dictionary<System.Type, List<ModuleInfoAttribute>> ModuleInfoByInput = new Dictionary<System.Type, List<ModuleInfoAttribute>>();

        /// <summary>
        /// Used to get InputSlotInfo from (ModuleInfoAttribute,InputType) couples
        /// </summary>
        readonly Dictionary<ModuleInfoAttribute, Dictionary<Type, InputSlotInfo>> inputSlotInfoByModuleInfoAndInputType = new Dictionary<ModuleInfoAttribute, Dictionary<Type, InputSlotInfo>>();
        readonly SortedDictionary<string, string> TemplatesByMenuName = new SortedDictionary<string, string>();

        CanvasSelection Sel { get { return Parent.Sel; } }
        CanvasState Canvas { get { return Parent.Canvas; } }

        public CanvasUI(CGGraph parent)
        {
            Parent = parent;
            LoadData();
        }

        public void AddModuleQuickmenu(CGModuleOutputSlot forOutputSlot)
        {
            GenericMenu mnu = new GenericMenu();
            List<ModuleInfoAttribute> matches;
            System.Type outType = forOutputSlot.OutputInfo.DataType;
            while (typeof(CGData).IsAssignableFrom(outType) && outType != typeof(CGData))
            {

                if (ModuleInfoByInput.TryGetValue(outType, out matches))
                {
                    foreach (ModuleInfoAttribute mi in matches)
                    {
                        InputSlotInfo si = inputSlotInfoByModuleInfoAndInputType[mi][outType];
                        if (CGModuleInputSlot.AreInputAndOutputSlotsCompatible(si, typeof(IOnRequestProcessing).IsAssignableFrom(TypeByModuleInfo[mi]), forOutputSlot.OutputInfo, forOutputSlot.OnRequestModule != null))
                            mnu.AddItem(new GUIContent(mi.MenuName), false, CTXOnAddAndConnectModule, mi);
                    }
                    mnu.ShowAsContext();
                }
                outType = outType.BaseType;
            }
        }



        void AddMenuItem(GenericMenu mnu, string item, GenericMenu.MenuFunction2 func, object userData, bool enabled = true)
        {
            if (enabled)
                mnu.AddItem(new GUIContent(item), false, func, userData);
            else
                mnu.AddDisabledItem(new GUIContent(item));
        }

        void AddMenuItem(GenericMenu mnu, string item, GenericMenu.MenuFunction func, bool enabled = true)
        {
            if (enabled)
                mnu.AddItem(new GUIContent(item), false, func);
            else
                mnu.AddDisabledItem(new GUIContent(item));
        }

        public void ContextMenu()
        {
            GenericMenu mnu = new GenericMenu();
            // Add/<Modules>
            List<ModuleInfoAttribute> miNames = new List<ModuleInfoAttribute>(TypeByModuleInfo.Keys);

            foreach (ModuleInfoAttribute mi in miNames)
                AddMenuItem(mnu, "Add/" + mi.MenuName, CTXOnAddModule, mi);
            // Add/<Templates>


            foreach (string tplName in TemplatesByMenuName.Keys)
                AddMenuItem(mnu, "Add Template/" + tplName, CTXOnAddTemplate, tplName);

            mnu.AddSeparator("");
            AddMenuItem(mnu, "Reset", CTXOnReset, Sel.SelectedModules.Count > 0);
            mnu.AddSeparator("");
            AddMenuItem(mnu, "Cut", () => CutSelection(this), Sel.SelectedModules.Count > 0);
            AddMenuItem(mnu, "Copy", () => CopySelection(this), Sel.SelectedModules.Count > 0);
            AddMenuItem(mnu, "Paste", () => PastSelection(this), !Clipboard.Empty);
            mnu.AddSeparator("");
            AddMenuItem(mnu, "Delete", () => DeleteSelection(this), Sel.SelectedModules.Count > 0 || Sel.SelectedLink != null);
            mnu.AddSeparator("");
            AddMenuItem(mnu, "Select all", () => SelectAll(this));
            mnu.ShowAsContext();
        }

        void CTXOnReset()
        {
            foreach (CGModule mod in Sel.SelectedModules)
                mod.Reset();
        }

        void CTXOnAddModule(object userData)
        {
            ModuleInfoAttribute mi = (ModuleInfoAttribute)userData;
            CGModule mod = AddModule(TypeByModuleInfo[mi]);
            mod.Properties.Dimensions = mod.Properties.Dimensions.SetPosition(Canvas.MousePosition);
        }

        void CTXOnAddAndConnectModule(object userData)
        {
            if (!Canvas.AutoConnectFrom)
                return;

            ModuleInfoAttribute mi = (ModuleInfoAttribute)userData;
            CGModule mod = AddModule(TypeByModuleInfo[mi]);

            mod.Properties.Dimensions = mod.Properties.Dimensions.SetPosition(Canvas.MousePosition);

            foreach (CGModuleInputSlot inputSlot in mod.Input)
                if (inputSlot.CanLinkTo(Canvas.AutoConnectFrom))
                {
                    Canvas.AutoConnectFrom.LinkTo(inputSlot);
                    return;
                }

        }

        void CTXOnAddTemplate(object userData)
        {
            string tplPath;
            if (TemplatesByMenuName.TryGetValue((string)userData, out tplPath))
                CGEditorUtility.LoadTemplate(Parent.Generator, tplPath, Canvas.MousePosition);
        }

        public CGModule AddModule(System.Type type)
        {
            CGModule mod = Parent.Generator.AddModule(type);
            Undo.RegisterCreatedObjectUndo(mod, "Create Module");
            return mod;
        }

        /// <summary>
        /// Deletes a link or one or more modules (Undo-Aware!)
        /// </summary>
        /// <param name="objects"></param>
        public void Delete(params object[] objects)
        {
            if (objects == null || objects.Length == 0)
                return;
            if (objects[0] is CGModuleLink)
                DeleteLink((CGModuleLink)objects[0]);
            else
                foreach (CGModule m in objects)
                    m.Delete();
        }

        public void DeleteLink(CGModuleLink link)
        {
            CGModuleOutputSlot sOut = Parent.Generator.GetModule(link.ModuleID, true).OutputByName[link.SlotName];
            CGModuleInputSlot sIn = Parent.Generator.GetModule(link.TargetModuleID, true).InputByName[link.TargetSlotName];
            sOut.UnlinkFrom(sIn);
        }

        public void LoadData()
        {
            // Build TypeByModuleInfo and ModuleInfoByInput dictionaries
            TypeByModuleInfo.Clear();
            ModuleInfoByInput.Clear();
            inputSlotInfoByModuleInfoAndInputType.Clear();
            TypeByModuleInfo = new SortedDictionary<ModuleInfoAttribute, System.Type>(typeof(CGModule).GetAllTypesWithAttribute<ModuleInfoAttribute>());

            foreach (KeyValuePair<ModuleInfoAttribute, Type> kv in TypeByModuleInfo)
            {
                Type moduleType = kv.Value;
                ModuleInfoAttribute moduleInfoAttribute = kv.Key;

                FieldInfo[] fields = moduleType.GetFields(BindingFlags.Public | BindingFlags.Instance);
                foreach (FieldInfo fieldInfo in fields)
                    if (fieldInfo.FieldType == typeof(CGModuleInputSlot))
                    {
                        object[] slotAttrib = fieldInfo.GetCustomAttributes(typeof(InputSlotInfo), true);
                        if (slotAttrib.Length > 0)
                        {
                            InputSlotInfo inputSlotInfo = (InputSlotInfo)slotAttrib[0];
                            List<ModuleInfoAttribute> moduleInfoAttributes;
                            for (int x = 0; x < inputSlotInfo.DataTypes.Length; x++)
                            {
                                Type dataType = inputSlotInfo.DataTypes[x];
                                if (!ModuleInfoByInput.TryGetValue(dataType, out moduleInfoAttributes))
                                {
                                    moduleInfoAttributes = new List<ModuleInfoAttribute>();
                                    ModuleInfoByInput.Add(dataType, moduleInfoAttributes);
                                }

                                moduleInfoAttributes.Add(moduleInfoAttribute);

                                if (inputSlotInfoByModuleInfoAndInputType.ContainsKey(moduleInfoAttribute) == false)
                                    inputSlotInfoByModuleInfoAndInputType[moduleInfoAttribute] = new Dictionary<Type, InputSlotInfo>();
                                if (inputSlotInfoByModuleInfoAndInputType[moduleInfoAttribute].ContainsKey(dataType) == false)
                                    inputSlotInfoByModuleInfoAndInputType[moduleInfoAttribute][dataType] = inputSlotInfo;
                            }
                        }
                    }
            }
            // load Templates
            ReloadTemplates();
        }

        /// <summary>
        /// Reloads the available templates from the prefabs in the Templates folder
        /// </summary>
        public void ReloadTemplates()
        {
            TemplatesByMenuName.Clear();
            string[] baseFolders;
            if (AssetDatabase.IsValidFolder("Assets/" + CurvyProject.Instance.CustomizationRootPath + CurvyProject.RELPATH_CGTEMPLATES))
                baseFolders = new string[2] { "Assets/" + CurvyEditorUtility.GetPackagePath("CG Templates"), "Assets/" + CurvyProject.Instance.CustomizationRootPath + CurvyProject.RELPATH_CGTEMPLATES };
            else
                baseFolders = new string[1] { "Assets/" + CurvyEditorUtility.GetPackagePath("CG Templates") };

            string[] prefabs = AssetDatabase.FindAssets("t:gameobject", baseFolders);

            foreach (string guid in prefabs)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                // Store under a unique menu name
                string name = AssetDatabase.LoadAssetAtPath(path, typeof(Transform)).name;
                string menuPath = System.IO.Path.GetDirectoryName(path).Replace(System.IO.Path.DirectorySeparatorChar.ToString(), "/");
                foreach (string s in baseFolders)
                    menuPath = menuPath.TrimStart(s);
                menuPath = menuPath.TrimStart('/');

                string menuName = string.IsNullOrEmpty(menuPath)
                    ? name
                    : menuPath + "/" + name;
                int i = 0;
                while (TemplatesByMenuName.ContainsKey((i == 0)
                    ? menuName
                    : menuName + i.ToString()))
                    i++;
                TemplatesByMenuName.Add((i == 0)
                    ? menuName
                    : menuName + i.ToString(), path);
            }
        }

        public void HandleDragDropProgress()
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
        }

        public void HandleDragDropDone()
        {
            Vector2 mousePosition = Event.current.mousePosition;

            foreach (Object @object in DragAndDrop.objectReferences)
            {
                CGModule module = null;
                if (@object is GameObject gameObject)
                {
                    CurvySpline spline = gameObject.GetComponent<CurvySpline>();
                    if (spline)
                    {
                        CurvyShape shape = gameObject.GetComponent<CurvyShape>();
                        if (shape)
                        {
                            InputSplineShape inputModule = Parent.Generator.AddModule<InputSplineShape>();
                            inputModule.Shape = spline;
                            module = inputModule;
                        }
                        else
                        {
                            InputSplinePath inputModule = Parent.Generator.AddModule<InputSplinePath>();
                            inputModule.Spline = spline;
                            module = inputModule;
                        }
                    }
                    else
                    {
                        InputGameObject inputModule = Parent.Generator.AddModule<InputGameObject>();
                        inputModule.GameObjects.Add(new CGGameObjectProperties(gameObject));
                        module = inputModule;
                    }

                }
                else if (@object is Mesh mesh)
                {
                    InputMesh inputModule = Parent.Generator.AddModule<InputMesh>();
                    inputModule.Meshes.Add(new CGMeshProperties(mesh));
                    module = inputModule;
                }

                if (module)
                {
                    module.Properties.Dimensions.position = mousePosition;
                    module.Properties.Dimensions.xMin -= module.Properties.MinWidth / 2;
                    mousePosition.y += module.Properties.Dimensions.height;
                }
            }

            DragAndDrop.AcceptDrag();
        }

        #region shortcut/contextual menu shared commands

        public static void SelectAll(CanvasUI ui)
        {
            ui.Sel.SetSelectionTo(ui.Parent.Modules);
        }

        public static void DeleteSelection(CanvasUI ui)
        {
            ui.Delete(ui.Sel.SelectedObjects);
            ui.Sel.Clear();
        }

        public static void CopySelection(CanvasUI ui)
        {
            Clipboard.CopyModules(ui.Sel.SelectedModules);
        }
        public static void CutSelection(CanvasUI ui)
        {
            Clipboard.CutModules(ui.Sel.SelectedModules);
        }
        public static void PastSelection(CanvasUI ui)
        {
            // relative position between modules were kept, but take current mouse position as reference!
            Vector2 off = ui.Canvas.MousePosition - Clipboard.Modules[0].Properties.Dimensions.position;
            ui.Sel.SetSelectionTo(Clipboard.PasteModules(ui.Parent.Generator, off));
        }

        #endregion
    }

    public class CGClipboard
    {
        public enum ClipboardMode
        {
            Cut,
            Copy
        }

        public ClipboardMode Mode = ClipboardMode.Copy;

        public List<CGModule> Modules = new List<CGModule>();


        public bool Empty { get { return Modules.Count == 0; } }

        public CurvyGenerator ParentGenerator
        {
            get
            {
                return (Modules.Count > 0) ? Modules[0].Generator : null;
            }
        }

        public void CutModules(IList<CGModule> modules)
        {
            Mode = ClipboardMode.Cut;
            copyInternal(modules);
        }

        public void CopyModules(IList<CGModule> modules)
        {
            Mode = ClipboardMode.Copy;
            copyInternal(modules);
        }

        public void Clear()
        {
            Modules.Clear();

        }

        /// <summary>
        /// Paste all Clipboard modules
        /// </summary>
        /// <param name="target">the generator to paste to</param>
        /// <param name="positionOffset">Canvas offset to use</param>
        /// <returns>the new modules</returns>
        public List<CGModule> PasteModules(CurvyGenerator target, Vector2 positionOffset)
        {
            List<CGModule> res = CGEditorUtility.CopyModules(Modules, target, positionOffset);
            if (Mode == ClipboardMode.Cut)
                foreach (CGModule mod in Modules)
                    ParentGenerator.DeleteModule(mod);
            Clear();

            return res;
        }

        void copyInternal(IList<CGModule> modules)
        {
            Modules.Clear();
            Modules.AddRange(modules);
        }


    }
}