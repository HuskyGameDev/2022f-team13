// =====================================================================
// Copyright 2013-2017 Fluffy Underware
// All rights reserved
// 
// http://www.fluffyunderware.com
// =====================================================================

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Reflection;
using FluffyUnderware.DevTools.Extensions;

namespace FluffyUnderware.DevToolsEditor
{
    public static class DTToolbar
    {
        const int ITEMSPACE = 10;
        const int PROJECTSPACE = 20;

        public static bool RecalcItemSize = true;
        public static bool MouseOverToolbarElement { get; private set; }

        static Vector2[] _MaxItemDimension = new Vector2[4]; // each Orientation has it's own max. Dimension
        /// <summary>
        /// Starting position for each side
        /// </summary>
        static Rect[] _InitialRects;
        /// <summary>
        /// Last item position for each side
        /// </summary>
        static Rect[] _ItemRect;

        static Event _handleEvent;

        internal static void Initialize()
        {
            RecalcItemSize = true;
            _handleEvent = null;
            loadItems();
            DTSelection.OnSelectionChange -= OnSelectionChange;
            DTSelection.OnSelectionChange += OnSelectionChange;
            SceneView.duringSceneGui -= Render;
            SceneView.duringSceneGui += Render;
            EditorApplication.hierarchyWindowItemOnGUI -= onHierarchy;
            EditorApplication.hierarchyWindowItemOnGUI += onHierarchy;
            EditorApplication.update -= onUpdate;
            EditorApplication.update += onUpdate;

        }


        static void loadItems()
        {
            foreach (DTProject prj in DT.Projects)
                prj.ToolbarItems.Clear();

            TypeCache.TypeCollection toolbarItemTypes = TypeCache.GetTypesDerivedFrom(typeof(DTToolbarItem));
            toolbarItemTypes.Intersect(TypeCache.GetTypesWithAttribute<ToolbarItemAttribute>()).ForEach(type => Activator.CreateInstance(type));

            foreach (DTProject prj in DT.Projects)
                prj.ToolbarItems.Sort();
        }

        static void OnSelectionChange()
        {
            foreach (DTProject project in DT.Projects)
                foreach (DTToolbarItem item in project.ToolbarItems)
                    item.OnSelectionChange();

            RecalcItemSize = true;
        }

        static void onHierarchy(int instanceID, Rect selectionRect)
        {
            if (Selection.instanceIDs.Contains(instanceID))
                _handleEvent = new Event(Event.current);
        }

        static void onUpdate()
        {
            if (Event.current != null || _handleEvent != null)
            {
                DTSelection.MuteEvents = true;
                foreach (DTProject project in DT.Projects)
                    foreach (DTToolbarItem item in project.ToolbarItems)
                    {
                        if (_handleEvent != null && item.Visible && item.Enabled)
                            item.HandleEvents(_handleEvent);
                        if (Event.current != null && item.Visible && item.Enabled)
                            item.HandleEvents(Event.current);
                    }
                DTSelection.MuteEvents = false;
            }
            _handleEvent = null;
        }


        static Rect getStatusBarRect()
        {
            Vector2 v = GUIUtility.GUIToScreenPoint(Vector2.zero);
            Rect r = SceneView.currentDrawingSceneView.position;
            // If SceneView is on another monitor, r.x doesn't start at 0, but GUIToScreenPoint gives the offset
            r.x -= v.x;
            r.y = _ItemRect[(int)DTToolbarOrientation.Bottom].y - 25;
            r.height = 20;

            return r;
        }

        public static bool detailOpen;

        static void Render(SceneView view)
        {
            Event ev = Event.current;
            MouseOverToolbarElement = false;
            // Only let certain hotkeys pass
            if (!EditorGUIUtility.editingTextField)
            {
                GUIUtility.keyboardControl = GUIUtility.GetControlID(FocusType.Passive);
            }

            List<DTProject> projects = DT.Projects;
            projects.Sort();

            Handles.BeginGUI();
            GUI.skin = null; // to ensure light-skin is used if set in preferences (or not Pro)
            // Get largest item for each side
            if (RecalcItemSize)
                calcMaxItemDimension();

            // Get starting position for each side
            _InitialRects = getInitialItemRect();
            _ItemRect = new Rect[4];
            _InitialRects.CopyTo(_ItemRect, 0);

            DTToolbarItem lastItem = null;
            DTSelection.MuteEvents = true;

            DTToolbarItem hovering = null;

            foreach (DTProject project in projects)
            {
                int side = (int)project.ToolbarOrientation;
                if (lastItem && lastItem.Project.ToolbarOrientation != project.ToolbarOrientation)
                    lastItem = null;
                List<DTToolbarItem> items = project.ToolbarItems;

                // Render items
                for (int i = 0; i < items.Count; i++)
                {
                    if (items[i].Visible)
                    {
                        Vector2 itemSize = items[i].GetItemSize(); // size of current item
                        _ItemRect[side] = advanceItemRect(lastItem, items[i], itemSize); // advance by using the last itemRect and the new(current) size

                        items[i].mItemRect = _ItemRect[side]; // Store current item rect


                        if (items[i].Enabled)
                        {
                            Handles.EndGUI();
                            EditorKeyBinding.BindingsEnabled = false;
                            if (items[i].mItemRect.Contains(DTGUI.MousePosition))
                            {
                                hovering = items[i];
                                MouseOverToolbarElement = true;
                            }
                            items[i].OnSceneGUI();

                            EditorKeyBinding.BindingsEnabled = true;
                            Handles.BeginGUI();
                        }
                        GUI.enabled = items[i].Enabled;
                        items[i].Render(_ItemRect[side]);

                        GUI.enabled = true;
                        if (ev != null && items[i].Enabled && (DTToolbarItem.FocusedItem == null || DTToolbarItem.FocusedItem == items[i]))
                            items[i].HandleEvents(ev);

                        lastItem = items[i];
                    }
                }
            }
            DTSelection.MuteEvents = false;
            detailOpen = false;
            // Render items client area (Note: itemRect contains the last rendered item, a.k.a. a way to get the number of rows/cols needed
            foreach (DTProject project in projects)
            {
                List<DTToolbarItem> items = project.ToolbarItems;
                for (int i = 0; i < items.Count; i++)
                {
                    if (items[i].Visible && items[i].ShowClientArea)
                    {
                        detailOpen = true;
                        Rect clientRect = items[i].mItemRect;
                        int side = (int)project.ToolbarOrientation;
                        switch (project.ToolbarOrientation)
                        {
                            case DTToolbarOrientation.Left:
                                clientRect.x = _ItemRect[side].x + _MaxItemDimension[side].x + 5;
                                break;
                            case DTToolbarOrientation.Right:
                                clientRect.x = _ItemRect[side].x - 5;
                                break;
                            case DTToolbarOrientation.Top:
                                clientRect.y = _ItemRect[side].y + _MaxItemDimension[side].y + 5;
                                break;
                            case DTToolbarOrientation.Bottom:
                                clientRect.y = _ItemRect[side].y - 5;
                                break;
                        }
                        if (clientRect.width > 0 && clientRect.height > 0)
                        {
                            items[i].mBackgroundRects.Clear();
                            MouseOverToolbarElement = MouseOverToolbarElement || clientRect.Contains(DTGUI.MousePosition);
                            EditorKeyBinding.BindingsEnabled = false;
                            items[i].RenderClientArea(clientRect);
                            EditorKeyBinding.BindingsEnabled = true;
                            if (DTGUI.IsClick)
                                foreach (Rect r in items[i].mBackgroundRects)
                                    if (r.Contains(ev.mousePosition))
                                        DTGUI.UseEvent(items[i].GetHashCode(), ev);
                        }
                    }
                }
            }

            // Handle statusbar info when hovering over an item
            if (hovering != null)
                DTToolbarItem._StatusBar.Set(hovering.StatusBarInfo, "Info");
            else
                DTToolbarItem._StatusBar.Clear("Info");

            // Render Statusbar
            DTToolbarItem._StatusBar.Render(getStatusBarRect(), null, true);

            Handles.EndGUI();

        }

        static Rect[] getInitialItemRect()
        {
            Rect[] res = new Rect[4];
            for (int side = 0; side < 4; side++)
            {
                Rect r = new Rect();
                switch (side)
                {
                    case (int)DTToolbarOrientation.Left:
                        r.x = 5; r.y = 10;
                        break;
                    case (int)DTToolbarOrientation.Right:
                        r.x = SceneView.currentDrawingSceneView.position.width - 10 - _MaxItemDimension[side].x;
                        r.y = 115;
                        break;
                    case (int)DTToolbarOrientation.Top:
                        float lft = _MaxItemDimension[(int)DTToolbarOrientation.Left].x;
                        r.x = 5; r.y = 10;
                        if (lft > 0)
                            r.x += 10 + lft;
                        break;
                    default: // Bottom
                        lft = _MaxItemDimension[(int)DTToolbarOrientation.Left].x;
                        r.x = 5; r.y = SceneView.currentDrawingSceneView.position.height - _MaxItemDimension[side].y - 30;
                        if (lft > 0)
                            r.x += 10 + lft;
                        break;
                }
                res[side] = r;
            }

            return res;
        }

        static Rect advanceItemRect(DTToolbarItem lastItem, DTToolbarItem newItem, Vector2 newItemSize)
        {
            Rect itemRect;
            float space = 0;
            int side = (int)newItem.Project.ToolbarOrientation;

            if (lastItem != null)
            {
                itemRect = lastItem.mItemRect;
                if (lastItem.Project != newItem.Project)
                    space = PROJECTSPACE;
                else
                    space = (newItem.Order - lastItem.Order >= 10) ? ITEMSPACE : 0;
            }
            else
                itemRect = _InitialRects[side];

            switch (newItem.Project.ToolbarOrientation)
            {
                case DTToolbarOrientation.Left:
                    itemRect.y += itemRect.height + 3 + space;
                    if (itemRect.y + newItemSize.y > SceneView.currentDrawingSceneView.position.height - 30)
                    {
                        itemRect.y = 10;
                        itemRect.x += _MaxItemDimension[(int)DTToolbarOrientation.Left].x + 5;
                    }
                    itemRect.width = _MaxItemDimension[(int)DTToolbarOrientation.Left].x;
                    itemRect.height = newItemSize.y;
                    break;
                case DTToolbarOrientation.Right:
                    itemRect.y += itemRect.height + 3 + space;
                    if (itemRect.y + newItemSize.y > SceneView.currentDrawingSceneView.position.height - 30)
                    {
                        itemRect.y = 10;
                        itemRect.x -= _MaxItemDimension[(int)DTToolbarOrientation.Right].x + 5;
                    }
                    itemRect.width = _MaxItemDimension[(int)DTToolbarOrientation.Right].x;
                    itemRect.height = newItemSize.y;
                    break;
                case DTToolbarOrientation.Top:
                    itemRect.x += 3 + itemRect.width + space;
                    if (itemRect.x + newItemSize.x > SceneView.currentDrawingSceneView.position.width - _MaxItemDimension[(int)DTToolbarOrientation.Left].x - _MaxItemDimension[(int)DTToolbarOrientation.Right].x)
                    {
                        itemRect.x = _InitialRects[(int)DTToolbarOrientation.Top].x;
                        itemRect.y += _MaxItemDimension[(int)DTToolbarOrientation.Top].y + 5;
                    }
                    itemRect.width = _MaxItemDimension[(int)DTToolbarOrientation.Top].x;
                    itemRect.height = newItemSize.y;
                    break;
                case DTToolbarOrientation.Bottom:
                    itemRect.x += 3 + itemRect.width + space;
                    if (itemRect.x + newItemSize.x > SceneView.currentDrawingSceneView.position.width - _MaxItemDimension[(int)DTToolbarOrientation.Left].x - _MaxItemDimension[(int)DTToolbarOrientation.Right].x)
                    {
                        itemRect.x = _InitialRects[(int)DTToolbarOrientation.Top].x;
                        itemRect.y -= _MaxItemDimension[(int)DTToolbarOrientation.Bottom].y + 5;
                    }
                    itemRect.width = _MaxItemDimension[(int)DTToolbarOrientation.Bottom].x;
                    itemRect.height = newItemSize.y;
                    break;

            }
            return itemRect;
        }

        /// <summary>
        /// For each side, find the item with the largest dimensions
        /// </summary>
        static void calcMaxItemDimension()
        {
            RecalcItemSize = false;
            _MaxItemDimension = new Vector2[4];
            int side;
            foreach (DTProject prj in DT.Projects)
            {
                side = (int)prj.ToolbarOrientation;
                foreach (DTToolbarItem item in prj.ToolbarItems)
                    if (item.Visible)
                        _MaxItemDimension[side] = Vector2.Max(_MaxItemDimension[side], item.GetItemSize());
            }
        }


        internal static void SetRadioGroupState(DTToolbarRadioButton active)
        {
            active.Project.SetRadioGroupState(active);
        }
    }

    public enum DTToolbarMode : int
    {
        Text = 1,
        Icon = 2,
        Full = 15,
    }

    public enum DTToolbarOrientation : int
    {
        Left = 0,
        Right = 1,
        Top = 2,
        Bottom = 3
    }

    public class DTToolbarStatus : DTStatusbar
    {

        protected override void GetColors()
        {
            GUI.contentColor = new Color(0, 0, 0, 0.75f);
        }
        protected override GUIStyle GetStyle()
        {
            GUIStyle style = base.GetStyle();
            style.alignment = TextAnchor.MiddleCenter;
            Texture2D bgTex = new Texture2D(1, 1);
            bgTex.SetPixel(0, 0, new Color(1, 1, 1, 0.5f));
            bgTex.Apply();
            bgTex.hideFlags = HideFlags.DontSave;
            style.normal.background = bgTex;
            return style;
        }
    }
}
