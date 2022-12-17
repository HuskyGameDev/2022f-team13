// =====================================================================
// Copyright 2013-2022 ToolBuddy
// All rights reserved
// 
// http://www.toolbuddy.net
// =====================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using FluffyUnderware.Curvy.Generator;
using FluffyUnderware.DevTools.Extensions;
using FluffyUnderware.DevToolsEditor;
using FluffyUnderware.DevToolsEditor.Data;

namespace FluffyUnderware.CurvyEditor.Generator
{
    internal class CanvasState
    {
        readonly CGGraph Parent;
        public CanvasSelection Sel { get { return Parent.Sel; } }
        public CanvasUI UI { get { return Parent.UI; } }
        Event EV { get { return Event.current; } }


        public CanvasState(CGGraph parent)
        {
            Parent = parent;
        }


        /// <summary>
        /// The module the mouse is hovering
        /// </summary>
        //todo this and IsMouseOverModule should be tied
        public CGModule MouseOverModule;


        /// <summary>
        /// Storing Event.current.mousePosition (in Canvasspace!)
        /// </summary>
        public Vector2 MousePosition;

        #region Scrolling



        /// <summary>
        /// Canvas scrolling state
        /// </summary>
        [Obsolete("Use ScrollValue, ScrollTarget, ScrollSpeed or IsScrollValueAnimating")]
        public AnimVector2 Scroll = new AnimVector2();

        private Vector2 scrollValue;

        /// <summary>
        /// Setting ScrollValue will stop any ongoing animation
        /// </summary>
        public Vector2 ScrollValue
        {
            get => scrollValue;
            set
            {
                value.x = Mathf.Max(0f, value.x);
                value.y = Mathf.Max(0f, value.y);

                scrollValue = value;
                ScrollTarget = value;
                ScrollSpeed = 0;
            }
        }

        public Vector2 ScrollTarget { get; private set; }
        public float ScrollSpeed { get; private set; }
        public bool IsScrollValueAnimating => ScrollTarget != ScrollValue;

        public void SetScrollTarget(Vector2 target, float speed)
        {
            target.x = Mathf.Max(0f, target.x);
            target.y = Mathf.Max(0f, target.y);

            ScrollTarget = target;
            ScrollSpeed = speed;
        }

        public void UpdateScrollingAnimation(double time)
        {
            scrollValue = Vector2.MoveTowards(scrollValue, ScrollTarget, (float)(ScrollSpeed * time));
        }

        #endregion

        /// <summary>
        /// Left/Top offset of Canvas from window
        /// </summary>
        public Vector2 ClientRectOffset;

        /// <summary>
        /// Whether the mouse is hovering over a module or not
        /// </summary>
        //todo this and MouseOverModule should be tied
        public bool IsMouseOverModule { get; private set; }

        public bool IsMouseOverCanvas
        {
            get
            {
                return ViewPort.Contains(MousePosition);
            }
        }
        /// <summary>
        /// Gets the canvas' scrollview size in window space
        /// </summary>
        public Rect ClientRect { get; private set; }
        /// <summary>
        /// Gets the total canvas size
        /// </summary>
        public Rect CanvasRect { get; private set; }

        /// <summary>
        /// Gets the visible rect of the canvas
        /// </summary>
        public Rect ViewPort
        {
            get
            {
                return new Rect(CanvasRect.x + ScrollValue.x, CanvasRect.y + ScrollValue.y, ClientRect.width, ClientRect.height);
            }
        }

        public Vector2 ViewPortMousePosition
        {
            get
            {
                return MousePosition + ClientRectOffset - ViewPort.min;
            }
        }

        public void SetClientRect(float xOffset, float yOffset, float xspace = 0, float yspace = 0)
        {
            ClientRectOffset = new Vector2(xOffset, yOffset);
            ClientRect = new Rect(ClientRectOffset.x, ClientRectOffset.y, (Parent.position.width - ClientRectOffset.x - xspace), (Parent.position.height - ClientRectOffset.y - yspace));

        }

        /// <summary>
        /// Grows the canvas to include the rect
        /// </summary>
        /// <param name="r">a rect in canvas space</param>
        public void EnlargeCanvasFor(Rect r)
        {
            //if (CurvyGUI.IsLayout)
            CanvasRect = CanvasRect.Include(r);
        }

        public void BeginGUI()
        {
            if (EV.isMouse || EV.type == EventType.DragUpdated || EV.type == EventType.DragExited)
                MousePosition = EV.mousePosition;

            //if (EV.type != EventType.Repaint && EV.type != EventType.Layout)
            //    Debug.Log("BeginGUI()  " + EV.type + "  " + IsMouseOverCanvas);

            if (IsMouseOverCanvas)
                //mouse events
                switch (EV.type)
                {
                    case EventType.MouseDrag:
                        if (!IsDragging)
                        {
                            if (IsMouseOverModule)
                                StartModuleDrag();
                            else
                                StartSelectionRectangleDrag();
                        }
                        break;
                    case EventType.MouseMove:
                    case EventType.MouseUp:
                        //ending drag is done in EventType.MouseMove to handle cases where the drag is finished while the mouse is out of the window, meaning that the MouseUp event is not catched
                        if (IsModuleDrag)
                            EndModuleDrag();
                        if (EV.type == EventType.MouseUp)
                        {
                            if (EV.button == 1)
                                UI.ContextMenu();
                        }
                        break;
                    case EventType.MouseDown:
                        if (IsCanvasDrag == false && EV.button == 2)
                            StartCanvasDrag(false);
                        break;
                }

            //keyboard events
            switch (EV.type)
            {
                case EventType.KeyDown:
                    if (IsCanvasDrag == false && EV.keyCode == KeyCode.Space)
                        StartCanvasDrag(true);
                    break;
                case EventType.KeyUp:
                    if (IsCanvasDrag && isKeyboardCanvasDrag && EV.keyCode == KeyCode.Space)
                        EndCanvasDrag();
                    break;
            }

            if (EV.type != EventType.Layout)
                IsMouseOverModule = false;

            if (IsCanvasDrag)
                EditorGUIUtility.AddCursorRect(ViewPort, MouseCursor.Pan);
        }

        /// <summary>
        /// Processing of Events AFTER Module Window drawing (Beware! Window dragging eats up most events!)
        /// </summary>
        public void EndGUI()
        {
            //if (EV.type != EventType.Repaint && EV.type != EventType.Layout)
            //    Debug.Log("EndGUI()  " + EV.type + "  " + IsMouseOverCanvas);

            if (IsMouseOverCanvas)
                //mouse events
                switch (EV.type)
                {
                    case EventType.MouseDrag:
                        // Drag canvas (i.e. change scroll offset)
                        if (IsCanvasDrag)
                        {
                            ScrollValue -= EV.delta;
                        }
                        break;
                    case EventType.MouseMove:
                    case EventType.MouseUp:
                        if (EV.type == EventType.MouseUp)
                        {
                            if (EV.button == 0 && !IsDragging)
                            {
                                if (Sel.SelectedLink && !MouseOverLink(Sel.SelectedLink)
                                    || (Sel.SelectedModule && !MouseOverModule))
                                    Sel.Clear();
                            }

                            // Multi Selection
                            if (IsSelectionRectDrag)
                                EndSelectionRectangleDrag();

                            Parent.StatusBar.Clear();
                        }

                        //ending drag is done in EventType.MouseMove to handle cases where the drag is finished while the mouse is out of the window, meaning that the MouseUp event is not caught
                        {
                            if (IsLinkDrag)
                                EndLinkDrag();

                            // Multi Selection
                            if (IsSelectionRectDrag)
                                CancelSelectionRectangleDrag();

                            if (IsCanvasDrag && isKeyboardCanvasDrag == false)
                                EndCanvasDrag();
                        }
                        break;
                    case EventType.DragUpdated:
                        UI.HandleDragDropProgress();
                        break;
                    case EventType.DragPerform:
                        UI.HandleDragDropDone();
                        break;
                }

            //keyboard events
            switch (EV.type)
            {
                case EventType.KeyDown:
                    switch (EV.keyCode)
                    {
                        case KeyCode.Delete:
                            CanvasUI.DeleteSelection(UI);
                            break;
                        case KeyCode.A:
                            if (EV.control)
                                CanvasUI.SelectAll(UI);
                            break;
                        case KeyCode.C:
                            if (EV.control)
                                CanvasUI.CopySelection(UI);
                            break;
                        case KeyCode.V:
                            if (EV.control)
                                CanvasUI.PastSelection(UI);
                            break;
                        case KeyCode.X:
                            if (EV.control)
                                CanvasUI.CutSelection(UI);
                            break;
                        case KeyCode.D:
                            if (EV.control)
                            {
                                CanvasUI.CopySelection(UI);
                                CanvasUI.PastSelection(UI);
                            }
                            break;
                    }
                    break;
            }
        }

        public void ViewPortRegisterWindow(CGModule module)
        {
            Rect winRect = module.Properties.Dimensions;
            EnlargeCanvasFor(winRect);

            if (!IsMouseOverModule && EV.type != EventType.Layout)
            {
                IsMouseOverModule = winRect.Contains(EV.mousePosition);
                MouseOverModule = (IsMouseOverModule) ? module : null;
            }
        }

        /// <summary>
        /// Sets the Canvas Rectangle so that it includes all the given modules
        /// </summary>
        public void ComputeCanvasRectangle(List<CGModule> modules)
        {
            CanvasRect = new Rect(0, 0, 0, 0);
            foreach (CGModule mod in modules)
                if (mod != null)
                    ViewPortRegisterWindow(mod);
        }

        public void FocusSelection()
        {
            CGModule selectedModule = Sel.SelectedModule;
            if (selectedModule)
            {
                Vector2 scrollDelta = GetFocusDelta(selectedModule);
                SetScrollTarget(ScrollValue + scrollDelta, Mathf.Max(60f, scrollDelta.magnitude * 4f));
            }
        }

        /// <summary>
        /// Returns the translation the viewport needs to do to focus on the given module
        /// </summary>
        /// <param name="targetModule">The module to focus on</param>

        public Vector2 GetFocusDelta(CGModule targetModule)
        {
            Rect focusedDimensions = targetModule.Properties.Dimensions;

            Rect viewPort = ViewPort;

            //add a margin, to include the module's highlight in the screen
            const float xMargin = 5;
            const float yMargin = 5;
            focusedDimensions.x -= xMargin;
            focusedDimensions.y -= yMargin;
            //the + 17 is to take into account the pixels taken by the scroll bar. I am always assuming the scroll bar is visible. An improvement is to check if they are actually there or not.
            focusedDimensions.width += 2 * xMargin + 17;
            focusedDimensions.height += 2 * yMargin + 17;

            //clamp the focused rectangle to what can be possibly seen within the viewport
            focusedDimensions.width = Mathf.Min(focusedDimensions.width, viewPort.width);
            focusedDimensions.height = Mathf.Min(focusedDimensions.height, viewPort.height);

            //clamp the focused rectangle to the canvas dimensions
            focusedDimensions.xMax = Math.Min(focusedDimensions.xMax, CanvasRect.width);
            focusedDimensions.yMax = Math.Min(focusedDimensions.yMax, CanvasRect.height);
            focusedDimensions.xMin = Math.Max(focusedDimensions.xMin, CanvasRect.x);
            focusedDimensions.yMin = Math.Max(focusedDimensions.yMin, CanvasRect.y);


            Vector2 delta = Vector2.zero;
            if (focusedDimensions.xMax > viewPort.xMax)
                delta.x = focusedDimensions.xMax - viewPort.xMax;
            else if (focusedDimensions.xMin < viewPort.xMin)
                delta.x = focusedDimensions.xMin - viewPort.xMin;

            if (focusedDimensions.yMax > viewPort.yMax)
                delta.y = focusedDimensions.yMax - viewPort.yMax;
            else if (focusedDimensions.yMin < viewPort.yMin)
                delta.y = focusedDimensions.yMin - viewPort.yMin;
            return delta;
        }

        public bool MouseOverLink(CGModuleLink link)
        {
            if (link == null)
                return false;

            CGModuleOutputSlot outSlot = Parent.Generator.ModulesByID[link.ModuleID].GetOutputSlot(link.SlotName);
            CGModuleInputSlot inSlot = Parent.Generator.ModulesByID[link.TargetModuleID].GetInputSlot(link.TargetSlotName);

            Vector3 startPosition = outSlot.Origin;
            Vector3 endPosition = inSlot.Origin;

            CGGraph.GetLinkBezierTangents(
                startPosition,
                endPosition,
                out Vector2 startTangent,
                out Vector2 endTangent);

            return HandleUtility.DistancePointBezier(EV.mousePosition, startPosition, endPosition, startTangent, endTangent) <= CGGraph.LinkSelectionDistance;
        }


        #region Drag handling

        /// <summary>
        /// Gets whether the user is currently dragging anything (Canvas, Module, Link, etc..)
        /// </summary>
        public bool IsDragging
        {
            get { return IsCanvasDrag || IsModuleDrag || IsLinkDrag || IsSelectionRectDrag; }
        }

        #region CanvasDrag

        public bool IsCanvasDrag;
        /// <summary>
        /// Was the Canvas drag initiated by the keyboard or the mouse?
        /// </summary>
        private bool isKeyboardCanvasDrag;

        private void StartCanvasDrag(bool isKeyboardInitiated)
        {
            IsCanvasDrag = true;
            isKeyboardCanvasDrag = isKeyboardInitiated;
        }

        private void EndCanvasDrag()
        {
            IsCanvasDrag = false;
        }

        #endregion
        #region Module Drag

        public bool IsModuleDrag;

        private void StartModuleDrag()
        {
            IsModuleDrag = true;
        }

        private void EndModuleDrag()
        {
            IsModuleDrag = false;
        }

        #endregion

        //The rest of the code handling links dragging is in CGGraph.cs
        #region Link Drag


        public CGModuleOutputSlot LinkDragFrom;

        /// <summary>
        /// Gets whether a link is currently dragged
        /// </summary>
        public bool IsLinkDrag
        {
            get { return LinkDragFrom != null; }
        }

        public CGModuleOutputSlot AutoConnectFrom;

        private void EndLinkDrag()
        {
            if (EV.control)
            {
                AutoConnectFrom = LinkDragFrom;
                UI.AddModuleQuickmenu(LinkDragFrom);
            }

            LinkDragFrom = null;
        }

        //The rest of the code handling links dragging is in CGGraph.cs


        #endregion

        #region Selection Rectangle Drag

        public bool IsSelectionRectDrag;

        /// <summary>
        /// Starting position of selection drag
        /// </summary>
        public Vector2 SelectionRectStart;

        private void StartSelectionRectangleDrag()
        {
            IsSelectionRectDrag = true;
            SelectionRectStart = ViewPortMousePosition;
        }

        public void HandleMultiSelection()
        {
            Rect selectionRect = new Rect().SetBetween(SelectionRectStart, ViewPortMousePosition);
            selectionRect.position -= ClientRectOffset - ViewPort.position;

            Sel.SetSelectionTo(Parent.Modules.Where(m => selectionRect.Overlaps(m.Properties.Dimensions, true)).ToList());
        }

        private void EndSelectionRectangleDrag()
        {
            HandleMultiSelection();
            IsSelectionRectDrag = false;
        }
        private void CancelSelectionRectangleDrag()
        {
            IsSelectionRectDrag = false;
        }

        #endregion

        #endregion
    }
}