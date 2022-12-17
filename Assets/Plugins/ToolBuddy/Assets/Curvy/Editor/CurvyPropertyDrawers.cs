// =====================================================================
// Copyright 2013-2022 ToolBuddy
// All rights reserved
// 
// http://www.toolbuddy.net
// =====================================================================

using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Reflection;
using System;
using System.Collections.Generic;
using FluffyUnderware.CurvyEditor;
using FluffyUnderware.Curvy;
using FluffyUnderware.Curvy.Generator;
using FluffyUnderware.CurvyEditor.Generator;
using FluffyUnderware.Curvy.Utils;
using FluffyUnderware.DevToolsEditor;
using FluffyUnderware.DevTools;

namespace FluffyUnderware.CurvyEditor
{

    #region ### CG related ###


    //[CustomPropertyDrawer(typeof(CGSpot))]
    public class CGSpotPropertyDrawer : PropertyDrawer
    {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            base.OnGUI(position, property, label);
            //  property.f
        }
    }

    [CustomPropertyDrawer(typeof(CGResourceCollectionManagerAttribute))]
    public class CGResourceCollectionManagerPropertyDrawer : DTPropertyDrawer<CGResourceCollectionManagerAttribute>
    {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect mControlRect = position;
            CGResourceManagerAttribute A = (CGResourceManagerAttribute)attribute;
            ICGResourceCollection lst = this.GetPropertySourceField<ICGResourceCollection>(property);

            label = EditorGUI.BeginProperty(position, label, property);

            if (lst != null)
            {
                if (lst.Count > 0)
                    label.text += string.Format("[{0}]", lst.Count);
                EditorGUI.PrefixLabel(mControlRect, label);
                mControlRect.x = (A.ReadOnly) ? mControlRect.xMax - 60 : mControlRect.xMax - 82;
                mControlRect.width = 60;

                if (GUI.Button(mControlRect, new GUIContent("Select", CurvyStyles.SelectTexture, "Select")))
                    DTSelection.SetGameObjects(lst.ItemsArray);
            }
        }
    }

    [CustomPropertyDrawer(typeof(CGResourceManagerAttribute), true)]
    public class CGResourceManagerPropertyDrawer : DTPropertyDrawer<CGResourceManagerAttribute>
    {

        CGResourceEditor ResourceEditor;



        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect mControlRect = position;
            label = EditorGUI.BeginProperty(position, label, property);
            CGResourceManagerAttribute A = (CGResourceManagerAttribute)attribute;
            CGModule module = (CGModule)property.serializedObject.targetObject;
            Component res = (Component)property.objectReferenceValue;
            if (res)
            {
                Transform parent = res.transform.parent;
                bool managed = (parent != null && parent.transform == module.transform);
                if (managed)
                {
                    EditorGUI.PrefixLabel(mControlRect, label);
                    mControlRect.x = (A.ReadOnly) ? mControlRect.xMax - 60 : mControlRect.xMax - 82;
                    mControlRect.width = 60;
                    if (GUI.Button(mControlRect, new GUIContent("Select", CurvyStyles.SelectTexture, "Select"), CurvyStyles.SmallButton))
                        Selection.activeObject = property.objectReferenceValue;
                    if (!A.ReadOnly)
                    {
                        mControlRect.x += mControlRect.width + 2;
                        mControlRect.width = 20;
                        if (GUI.Button(mControlRect, new GUIContent(CurvyStyles.DeleteSmallTexture, "Delete resource"), CurvyStyles.SmallButton))
                        {
                            if (EditorUtility.DisplayDialog("Delete resource", "This will permanently delete the resource! This operation cannot be undone. Proceed?", "Yes", "No"))
                            {
                                if (DTUtility.DoesPrefabStatusAllowDeletion(res.gameObject, out string errorMessage))
                                {
                                    module.DeleteManagedResource(A.ResourceName, res);
                                    property.objectReferenceValue = null;
                                    ResourceEditor = null;
                                }
                                else
                                {
                                    EditorUtility.DisplayDialog($"Cannot delete Game Object '{res.name}'", errorMessage, "Ok");
                                }
                            }
                        }
                    }

                    if (property.objectReferenceValue != null)
                    {
                        //if (!ResourceEditor)
                        ResourceEditor = CGResourceEditorHandler.GetEditor(A.ResourceName, res);

                        if (ResourceEditor && ResourceEditor.OnGUI())
                        {
                            // TODO: Refresh using new value not always working!
                            module.Invoke("OnValidate", 0);
                            module.Dirty = true;
                            module.Generator.Invoke("Update", 1f);
                        }
                    }
                }
                else
                {
                    mControlRect.width -= 20;
                    EditorGUI.PropertyField(mControlRect, property, label);
                    mControlRect.x += mControlRect.width + 2;
                    mControlRect.width = 20;
                    if (GUI.Button(mControlRect, new GUIContent(CurvyStyles.ClearSmallTexture, "Unset")))
                    {
                        property.objectReferenceValue = null;
                        ResourceEditor = null;
                    }


                }
            }
            else
            {
                mControlRect.width -= 20;
                EditorGUI.PropertyField(mControlRect, property, label);
                mControlRect.x = mControlRect.xMax + 2;
                mControlRect.width = 20;
                if (GUI.Button(mControlRect, new GUIContent(CurvyStyles.AddSmallTexture, "Add Managed")))
                {
                    // Call AddResource to create and name the resource
                    property.objectReferenceValue = module.AddManagedResource(A.ResourceName);
                }
            }


            EditorGUI.EndProperty();
        }


    }

    [CustomPropertyDrawer(typeof(CGDataReferenceSelectorAttribute))]
    public class CGDataReferenceSelectorPropertyDrawer : DTPropertyDrawer<CGDataReferenceSelectorAttribute>
    {
        SerializedProperty CurrentProp;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return (GetPropertySourceField<CGDataReference>(property).HasValue) ? base.GetPropertyHeight(property, label) : base.GetPropertyHeight(property, label) * 2;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            CGDataReferenceSelectorAttribute attrib = (CGDataReferenceSelectorAttribute)attribute;
            CurrentProp = property;
            CGDataReference field = GetPropertySourceField<CGDataReference>(property);

            EditorGUI.PrefixLabel(position, label);

            position.x += EditorGUIUtility.labelWidth;
            position.width -= EditorGUIUtility.labelWidth;

            Rect r = new Rect(position);
            if (field.Module != null)
                r.width -= 30;

            string btnLabel = (field.Module) ? string.Format("{0}.{1}", field.Module.ModuleName, field.SlotName) : "None";
            string btnTT = (field.Module && field.Module.Generator) ? string.Format("{0}.{1}.{2}", field.Module.Generator.name, field.Module.ModuleName, field.SlotName) : "Click to choose";
            if (GUI.Button(r, new GUIContent(btnLabel, btnTT)))
            {
                CGEditorUtility.ShowOutputSlotsMenu(OnMenu, attrib.DataType);
            }
            if (field.Module != null)
            {
                r.width = 30;
                r.x = position.xMax - 30;
                if (GUI.Button(r, new GUIContent(CurvyStyles.SelectTexture, "Select")))
                    EditorGUIUtility.PingObject(field.Module);
            }
            else
            {
                EditorGUILayout.HelpBox(string.Format("Missing source of type {0}", attrib.DataType.Name), MessageType.Error);
            }

        }

        void OnMenu(object userData)
        {
            CGModuleOutputSlot slot = userData as CGModuleOutputSlot;
            CGDataReference field = GetPropertySourceField<CGDataReference>(CurrentProp);
            if (slot == null)
                field.Clear();
            else
                field.setINTERNAL(slot.Module, slot.Info.Name);

            CurrentProp.serializedObject.ApplyModifiedProperties();
        }
    }
    #endregion
}