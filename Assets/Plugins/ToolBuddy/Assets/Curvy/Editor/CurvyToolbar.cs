// =====================================================================
// Copyright 2013-2022 ToolBuddy
// All rights reserved
// 
// http://www.toolbuddy.net
// =====================================================================

using System;
using UnityEngine;
using System.Collections;
using FluffyUnderware.DevToolsEditor;
using UnityEditor;
using FluffyUnderware.Curvy;
using FluffyUnderware.DevTools.Extensions;
using System.Collections.Generic;
using FluffyUnderware.Curvy.Utils;
using FluffyUnderware.Curvy.Shapes;
using FluffyUnderware.Curvy.Generator;
using System.Linq;
using FluffyUnderware.Curvy.Controllers;
using FluffyUnderware.CurvyEditor.Generator;
using FluffyUnderware.Curvy.Generator.Modules;
using FluffyUnderware.DevTools;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

namespace FluffyUnderware.CurvyEditor.UI
{
    [ToolbarItem(10, "Curvy", "Options", "Curvy Options", "curvyicon_dark,24,24", "curvyicon_light,24,24")]
    public class TBOptions : DTToolbarToggleButton
    {

        public override string StatusBarInfo { get { return "Open Curvy Options menu"; } }


        public override void RenderClientArea(Rect r)
        {
            base.RenderClientArea(r);
            SetElementSize(ref r, 32, 32);
            if (GUI.Button(r, new GUIContent(CurvyStyles.IconPrefs, "Preferences")))
            {
                DT.OpenPreferencesWindow(CurvyProject.CurvySettingsProvider.GetPreferencesPath());
                On = false;
            }
            Advance(ref r);
            if (GUI.Button(r, new GUIContent(CurvyStyles.IconAsmdef, "Generate Assembly Definitions")))
            {
                CurvyEditorUtility.GenerateAssemblyDefinitions();
                On = false;
            }
            Advance(ref r);
            if (GUI.Button(r, new GUIContent(CurvyStyles.IconHelp, "Online Manual")))
            {
                AboutWindow.OpenDocs();
                On = false;
            }
            Advance(ref r);
            if (GUI.Button(r, new GUIContent(CurvyStyles.IconWWW, "Curvy Website")))
            {
                AboutWindow.OpenWeb();
                On = false;
            }
            Advance(ref r);
            if (GUI.Button(r, new GUIContent(CurvyStyles.IconBugReporter, "Report Bug")))
            {
                CurvyEditorUtility.SendBugReport();
                On = false;
            }
            Advance(ref r);
            if (GUI.Button(r, new GUIContent(CurvyStyles.IconAbout, "About Curvy")))
                AboutWindow.Open();



        }

        public override void OnSelectionChange()
        {
            Visible = CurvyProject.Instance.ShowGlobalToolbar || DTSelection.HasComponent<CurvySpline, CurvySplineSegment, CurvyController, CurvyGenerator>(true);
        }

    }

    [ToolbarItem(12, "Curvy", "View", "View Settings", "viewsettings,24,24")]
    public class TBViewSetttings : DTToolbarToggleButton
    {
        public override string StatusBarInfo { get { return "Set Curvy Scene View visibility"; } }



        public override void OnSelectionChange()
        {
            Visible = CurvyProject.Instance.ShowGlobalToolbar || DTSelection.HasComponent<CurvySpline, CurvySplineSegment, CurvyController, CurvyGenerator>(true);
        }

        public override void RenderClientArea(Rect r)
        {
            base.RenderClientArea(r);
            bool b;
            bool v;

            Background(r, 134, 230);
            SetElementSize(ref r, 134, 19);

            EditorGUI.BeginChangeCheck();
            b = (CurvyGlobalManager.Gizmos == CurvySplineGizmos.None);
            b = GUI.Toggle(r, b, "None");
            if (b)
                CurvyGlobalManager.Gizmos = CurvySplineGizmos.None;
            // Curve
            AdvanceBelow(ref r);
            b = CurvyGlobalManager.ShowCurveGizmo;
            v = GUI.Toggle(r, b, "Curve");
            if (b != v)
                CurvyGlobalManager.ShowCurveGizmo = v;
            // Connection
            AdvanceBelow(ref r);
            b = CurvyGlobalManager.ShowConnectionsGizmo;
            v = GUI.Toggle(r, b, "Connections");
            if (b != v)
                CurvyGlobalManager.ShowConnectionsGizmo = v;
            // Orientation Anchor
            AdvanceBelow(ref r);
            b = CurvyGlobalManager.ShowOrientationAnchorsGizmo;
            v = GUI.Toggle(r, b, "Orientation Anchors");
            if (b != v)
                CurvyGlobalManager.ShowOrientationAnchorsGizmo = v;
            // Approximation
            AdvanceBelow(ref r);
            b = CurvyGlobalManager.ShowApproximationGizmo;
            v = GUI.Toggle(r, b, "Approximations");
            if (b != v)
                CurvyGlobalManager.ShowApproximationGizmo = v;
            // Orientation
            AdvanceBelow(ref r);
            b = CurvyGlobalManager.ShowOrientationGizmo;
            v = GUI.Toggle(r, b, "Orientation");
            if (b != v)
                CurvyGlobalManager.ShowOrientationGizmo = v;
            // Tangents
            AdvanceBelow(ref r);
            b = CurvyGlobalManager.ShowTangentsGizmo;
            v = GUI.Toggle(r, b, "Tangents");
            if (b != v)
                CurvyGlobalManager.ShowTangentsGizmo = v;
            // TF
            AdvanceBelow(ref r);
            b = CurvyGlobalManager.ShowTFsGizmo;
            v = GUI.Toggle(r, b, "TF");
            if (b != v)
                CurvyGlobalManager.ShowTFsGizmo = v;
            // Distance
            AdvanceBelow(ref r);
            b = CurvyGlobalManager.ShowRelativeDistancesGizmo;
            v = GUI.Toggle(r, b, "Relative Distances");
            if (b != v)
                CurvyGlobalManager.ShowRelativeDistancesGizmo = v;
            // UserValues
            AdvanceBelow(ref r);
            b = CurvyGlobalManager.ShowMetadataGizmo;
            v = GUI.Toggle(r, b, "Metadata");
            if (b != v)
                CurvyGlobalManager.ShowMetadataGizmo = v;
            // Labels
            AdvanceBelow(ref r);
            b = CurvyGlobalManager.ShowLabelsGizmo;
            v = GUI.Toggle(r, b, "Labels");
            if (b != v)
                CurvyGlobalManager.ShowLabelsGizmo = v;
            // Bounds
            AdvanceBelow(ref r);
            b = CurvyGlobalManager.ShowBoundsGizmo;
            v = GUI.Toggle(r, b, "Bounds");
            if (b != v)
                CurvyGlobalManager.ShowBoundsGizmo = v;

            if (EditorGUI.EndChangeCheck())
                CurvyProject.Instance.SavePreferences();
        }

    }

    [ToolbarItem(30, "Curvy", "Create", "Create", "add,24,24")]
    public class TBNewMenu : DTToolbarToggleButton
    {
        public override string StatusBarInfo { get { return "Create"; } }


        public override void RenderClientArea(Rect r)
        {
            base.RenderClientArea(r);
            SetElementSize(ref r, 32, 32);
            if (GUI.Button(r, new GUIContent(CurvyStyles.IconNewShape, "Shape")))
            {
                CurvyMenu.CreateCurvySpline(new MenuCommand(Selection.activeGameObject));
                Project.FindItem<TBSplineSetShape>().OnClick();

                On = false;
            }
            Advance(ref r);
            if (GUI.Button(r, new GUIContent(CurvyStyles.IconNewCG, "Generator")))
            {
                CurvyGenerator cg = new GameObject("Curvy Generator", typeof(CurvyGenerator)).GetComponent<CurvyGenerator>();
                CurvyMenu.ApplyIncrementalNameToGenerator(cg);
                Undo.RegisterCreatedObjectUndo(cg.gameObject, "Create Generator");
                if (cg)
                {
                    GameObject parent = DTSelection.GetGameObject(false);
                    if (parent != null)
                    {
                        Undo.SetTransformParent(cg.transform, parent.transform, "Create Generator");
                        cg.transform.localPosition = Vector3.zero;
                        cg.transform.localRotation = Quaternion.identity;
                        cg.transform.localScale = Vector3.one;
                    }
                    // if a spline is selected, create an Input module
                    if (DTSelection.HasComponent<CurvySpline>())
                    {
                        InputSplinePath mod = cg.AddModule<InputSplinePath>();
                        mod.Spline = DTSelection.GetAs<CurvySpline>();
                    }
                    DTSelection.SetGameObjects(cg);
                    CGGraph.Open(cg);
                }
                On = false;
            }
        }

        public override void OnSelectionChange()
        {
            Visible = CurvyProject.Instance.ShowGlobalToolbar || DTSelection.HasComponent<CurvySpline, CurvySplineSegment, CurvyController, CurvyGenerator>(true);
        }

    }

    [ToolbarItem(32, "Curvy", "Draw Spline", "Draw Splines", "draw,24,24")]
    public class TBDrawControlPoints : DTToolbarToggleButton
    {
        private enum RayCastPlane
        {
            Undefinded,
            LocalXY,
            GlobalXY,
            LocalYZ,
            GlobalYZ,
            LocalXZ,
            GlobalXZ,
            ViewPlane
        }

        private RayCastPlane rayCastPlane = RayCastPlane.LocalXZ;

        public override string StatusBarInfo { get { return "Create splines and Control Points"; } }

        enum ModeEnum
        {
            None = 0,
            Add = 1,
            Pick = 2,

        }

        ModeEnum Mode = ModeEnum.None;

        CurvySplineSegment selectedCP;
        CurvySpline selectedSpline;

        public TBDrawControlPoints()
        {
            KeyBindings.Add(new EditorKeyBinding("Toggle Draw Mode", "", KeyCode.Space));
        }



        public override void HandleEvents(Event e)
        {
            base.HandleEvents(e);
            if (On && DTHandles.MouseOverSceneView)
            {
                Mode = ModeEnum.None;
                if (e.control && !e.alt) // Prevent that Panning (CTRL+ALT+LMB) creates CP'S
                {
                    Mode = ModeEnum.Add;
                    if (e.shift)
                        Mode |= ModeEnum.Pick;
                }

                if (e.type == EventType.MouseDown)
                {
                    if (Mode.HasFlag(ModeEnum.Add))
                    {
                        addCP(e.mousePosition, Mode.HasFlag(ModeEnum.Pick), e.button == 1);
                        DTGUI.UseEvent(GetHashCode(), e);
                    }

                }
                if (Mode.HasFlag(ModeEnum.Add))
                {
                    string bLmbBAddControlPointBRmbBAddSmartConnect =
                        "<b>[LMB] on empty space:</b> Add Control Point\n" +
                        "<b>[LMB] on first Control Point:</b> Close/Open the spline\n" +
                        "<b>[RMB] on empty space:</b> Add & Connect to new spline\n" +
                        "<b>[RMB] on segment:</b> Add & Connect to existing spline";

                    if (Mode.HasFlag(ModeEnum.Pick))
                    {
                        //<b>[LMB On first Control Point]</b> Close/Open the spline
                        _StatusBar.Set(bLmbBAddControlPointBRmbBAddSmartConnect, "DrawMode");
                    }
                    else
                        _StatusBar.Set($"{bLmbBAddControlPointBRmbBAddSmartConnect}\n<b>[Shift]</b> Raycast", "DrawMode");
                }
                else
                    _StatusBar.Set("Hold <b>[Ctrl]</b> to add Control Points", "DrawMode");
            }
            else
                _StatusBar.Clear("DrawMode");
        }

        public override void RenderClientArea(Rect r)
        {
            base.RenderClientArea(r);

            bool raycastAgainstGeometry = Mode.HasFlag(ModeEnum.Pick);

            SetElementSize(ref r, 32, 32);

            if (raycastAgainstGeometry)
            {

            }
            else if (SceneView.currentDrawingSceneView.in2DMode)
            {
                rayCastPlane = RayCastPlane.GlobalXY;
            }
            else if (selectedSpline != null && selectedSpline.RestrictTo2D)
            {
                switch (selectedSpline.Restricted2DPlane)
                {
                    case CurvyPlane.XY:
                        rayCastPlane = RayCastPlane.LocalXY;
                        break;
                    case CurvyPlane.XZ:
                        rayCastPlane = RayCastPlane.LocalXZ;
                        break;
                    case CurvyPlane.YZ:
                        rayCastPlane = RayCastPlane.LocalYZ;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                if (GUI.Toggle(r,
                    rayCastPlane == RayCastPlane.ViewPlane,
                    new GUIContent(CurvyStyles.IconView, "Use view plane"), GUI.skin.button))
                    rayCastPlane = RayCastPlane.ViewPlane;
                Advance(ref r);

                if (GUI.Toggle(r,
                    rayCastPlane == RayCastPlane.GlobalXY || rayCastPlane == RayCastPlane.LocalXY,
                    new GUIContent(CurvyStyles.IconAxisXY, "Use XY plane"), GUI.skin.button))
                    rayCastPlane = Tools.pivotRotation == PivotRotation.Local ? RayCastPlane.LocalXY : RayCastPlane.GlobalXY;
                Advance(ref r);

                if (GUI.Toggle(r,
                    rayCastPlane == RayCastPlane.GlobalXZ || rayCastPlane == RayCastPlane.LocalXZ,
                    new GUIContent(CurvyStyles.IconAxisXZ, "Use XZ plane"), GUI.skin.button))
                    rayCastPlane = Tools.pivotRotation == PivotRotation.Local ? RayCastPlane.LocalXZ : RayCastPlane.GlobalXZ;
                Advance(ref r);

                if (GUI.Toggle(r,
                    rayCastPlane == RayCastPlane.GlobalYZ || rayCastPlane == RayCastPlane.LocalYZ,
                    new GUIContent(CurvyStyles.IconAxisYZ, "Use YZ plane"), GUI.skin.button))
                    rayCastPlane = Tools.pivotRotation == PivotRotation.Local ? RayCastPlane.LocalYZ : RayCastPlane.GlobalYZ;
                Advance(ref r);
            }

            SetElementSize(ref r, 24, 24);
            r.y += 4;
            GUI.DrawTexture(r, Mode.HasFlag(ModeEnum.Add) ? CurvyStyles.IconCP : CurvyStyles.IconCPOff);

            Advance(ref r);
            GUI.DrawTexture(r, raycastAgainstGeometry ? CurvyStyles.IconRaycast : CurvyStyles.IconRaycastOff);
        }

        bool pickScenePoint(Vector2 mousePos, bool castAgainstGeometry, CurvySpline referenceSpline, CurvySplineSegment referenceCP, out Vector3 pickedPoint)
        {
            Ray mouseRay = HandleUtility.GUIPointToWorldRay(mousePos);
            Vector3 referencePosition = ((referenceCP != null) ? referenceCP.transform : referenceSpline.transform).position;

            pickedPoint = Vector3.zero;
            bool didPickAPoint = false;

            if (castAgainstGeometry && Physics.Raycast(mouseRay, out RaycastHit hit))
            {
                pickedPoint = hit.point;
                didPickAPoint = true;
            }
            else
            {
                Plane plane;
                switch (rayCastPlane)
                {
                    case RayCastPlane.LocalXY:
                        plane = new Plane(referenceSpline.transform.forward, referencePosition);
                        break;
                    case RayCastPlane.GlobalXY:
                        plane = new Plane(Vector3.forward, referencePosition);
                        break;
                    case RayCastPlane.LocalYZ:
                        plane = new Plane(referenceSpline.transform.right, referencePosition);
                        break;
                    case RayCastPlane.GlobalYZ:
                        plane = new Plane(Vector3.right, referencePosition);
                        break;
                    case RayCastPlane.LocalXZ:
                        plane = new Plane(referenceSpline.transform.up, referencePosition);
                        break;
                    case RayCastPlane.GlobalXZ:
                        plane = new Plane(Vector3.up, referencePosition);
                        break;
                    case RayCastPlane.ViewPlane:
                        plane = new Plane(SceneView.currentDrawingSceneView.camera.transform.forward, referencePosition);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }


                float hitDistance;
                if (plane.Raycast(mouseRay, out hitDistance))
                {
                    pickedPoint = mouseRay.GetPoint(hitDistance);
                    didPickAPoint = true;
                }
                //fallback: this can happen if you select xy plane but scene view is orthogonal and set to yz. Meaning that all rays are parallel to xy plane. We use the same logic than in RayCastPlane.ViewPlane as a fallback
                else if (new Plane(SceneView.currentDrawingSceneView.camera.transform.forward, referencePosition).Raycast(mouseRay, out hitDistance))
                {
                    pickedPoint = mouseRay.GetPoint(hitDistance);
                    didPickAPoint = true;
                }
            }

            if (didPickAPoint == false)
                DTLog.LogWarning("[Curvy] Draw Splines tool: could not pick a valid position. Please raise a bug report.");

            return didPickAPoint;
        }

        void addCP(Vector2 cursor, bool castRay, bool connectNew)
        {
            const string undoingOperationLabel = "Add ControlPoint";

            Func<CurvySpline, CurvySplineSegment, Vector3, CurvySplineSegment> insertControlPoint =
                (spline, current, worldPos) =>
                {
                    CurvySplineSegment seg = spline.InsertAfter(current, worldPos, false, Space.World);
                    Undo.RegisterCreatedObjectUndo(seg.gameObject, undoingOperationLabel);
                    return seg;
                };


            if (selectedSpline) //selecp the last cp if none selected
            {
                if (!selectedCP && selectedSpline.ControlPointCount > 0)
                    selectedCP = selectedSpline.ControlPointsList[selectedSpline.ControlPointCount - 1];
            }
            else //create a spline if none selected
            {
                selectedSpline = CurvySpline.Create();

                //Restricted2DPlane
                switch (rayCastPlane)
                {
                    case RayCastPlane.ViewPlane:
                    case RayCastPlane.LocalXY:
                    case RayCastPlane.GlobalXY:
                        selectedSpline.Restricted2DPlane = CurvyPlane.XY;
                        break;
                    case RayCastPlane.LocalYZ:
                    case RayCastPlane.GlobalYZ:
                        selectedSpline.Restricted2DPlane = CurvyPlane.YZ;
                        break;
                    case RayCastPlane.LocalXZ:
                    case RayCastPlane.GlobalXZ:
                        selectedSpline.Restricted2DPlane = CurvyPlane.XZ;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                CurvyMenu.ApplyIncrementalNameToSpline(selectedSpline);

                Undo.RegisterCreatedObjectUndo(selectedSpline.gameObject, undoingOperationLabel);

                Transform parent = DTSelection.GetAs<Transform>();
                selectedSpline.transform.SetParent(parent);
                if (parent == null)
                    selectedSpline.transform.position = HandleUtility.GUIPointToWorldRay(cursor).GetPoint(10);
            }

            // Pick a point to add the CP at
            Vector3 pickedPoint;
            if (!pickScenePoint(cursor, castRay, selectedSpline, selectedCP, out pickedPoint))
                return;

            CurvySplineSegment pickedControlPoint;
            {
                GameObject pickedGameObject = HandleUtility.PickGameObject(cursor, false);
                pickedControlPoint = pickedGameObject ? pickedGameObject.GetComponent<CurvySplineSegment>() : null;
            }

            CurvySplineSegment newCP;
            // Connect by creating a new spline with 2 CP, the first "over" selectedCP, the second at the desired new position
            // OR connect to existing CP
            if (connectNew && selectedCP)
            {
                CurvySplineSegment cpToConnect;//To connect with the selected cp

                // if mouse is over an existing CP, connect to this (if possible)
                // if we picked a target cp, it may be a pick on it's segment, so check distance to CP
                if (pickedControlPoint)
                {
                    Plane plane = new Plane(SceneView.currentDrawingSceneView.camera.transform.forward, pickedControlPoint.transform.position);
                    Ray ray = HandleUtility.GUIPointToWorldRay(cursor);
                    float dist;
                    if (plane.Raycast(ray, out dist))
                    {
                        //Setting connectedCp
                        {
                            Vector3 hit = ray.GetPoint(dist);
                            if ((hit - pickedControlPoint.transform.position).magnitude <= HandleUtility.GetHandleSize(hit) * CurvyGlobalManager.GizmoControlPointSize)
                            {
                                cpToConnect = pickedControlPoint;
                            }
                            else
                            {
                                if (pickedControlPoint.Spline.Dirty)
                                    pickedControlPoint.Spline.Refresh();

                                Vector3 position = pickedControlPoint.Interpolate(pickedControlPoint.GetNearestPointF(hit, Space.World), Space.World);
                                cpToConnect = insertControlPoint(pickedControlPoint.Spline, pickedControlPoint, position);
                            }
                        }
                        newCP = insertControlPoint(selectedSpline, selectedCP, cpToConnect.transform.position);
                        selectedCP = newCP;
                    }
                    else
                        newCP = cpToConnect = null;
                }
                else
                    newCP = cpToConnect = null;

                if (!cpToConnect)
                {
                    CurvySpline newSpline = CurvySpline.Create(selectedSpline);
                    CurvyMenu.ApplyIncrementalNameToSpline(newSpline);

                    Undo.RegisterCreatedObjectUndo(newSpline.gameObject, undoingOperationLabel);

                    newSpline.Closed = false;
                    cpToConnect = insertControlPoint(newSpline, null, selectedCP.transform.position);
                    newCP = insertControlPoint(newSpline, cpToConnect, pickedPoint);
                    selectedSpline = newSpline;
                }

                {
#if CURVY_SANITY_CHECKS
                    Assert.IsFalse((selectedCP.Connection != cpToConnect.Connection) && (selectedCP.Connection != null && cpToConnect.Connection != null), "Both Control Points should not have different non null connections");
#endif
                    CurvySplineSegment connectionSourceCp;
                    CurvySplineSegment connectionDestinationCp;
                    if (selectedCP.Connection != null)
                    {
                        connectionSourceCp = selectedCP;
                        connectionDestinationCp = cpToConnect;
                    }
                    else
                    {
                        if (cpToConnect.Connection == null)
                        {
                            CurvyConnection.Create(cpToConnect, selectedCP);

                            cpToConnect.Connection.SetSynchronisationPositionAndRotation(cpToConnect.transform.position, cpToConnect.transform.rotation);
                            cpToConnect.ConnectionSyncPosition = true;
                            cpToConnect.ConnectionSyncRotation = true;
                            cpToConnect.FollowUpHeading = ConnectionHeadingEnum.Auto;
                        }

                        connectionSourceCp = cpToConnect;
                        connectionDestinationCp = selectedCP;
                    }

                    CurvyConnection connection = connectionSourceCp.Connection;
                    Undo.RecordObject(connection, undoingOperationLabel);
                    if (connection.ControlPointsList.Contains(connectionDestinationCp) == false)
                        connection.AddControlPoints(connectionDestinationCp);
                    connectionDestinationCp.ConnectionSyncPosition = connectionSourceCp.ConnectionSyncPosition;
                    connectionDestinationCp.ConnectionSyncRotation = connectionSourceCp.ConnectionSyncRotation;
                    connectionDestinationCp.FollowUpHeading = ConnectionHeadingEnum.Auto;
                    connection.SetSynchronisationPositionAndRotation(connection.transform.position, connection.transform.rotation);
                }
            }
            else
            {
                if (selectedSpline.Count >= 1 && pickedControlPoint == selectedSpline.FirstSegment)
                {
                    Undo.RecordObject(selectedSpline, undoingOperationLabel);
                    selectedSpline.Closed = !selectedSpline.Closed;
                    newCP = selectedSpline.Closed ? selectedSpline.LastSegment : selectedSpline.LastVisibleControlPoint;
                }
                else
                    newCP = insertControlPoint(selectedSpline, selectedCP, pickedPoint);
            }

            DTSelection.SetGameObjects(newCP);
        }

        public override void OnSelectionChange()
        {
            Visible = CurvyProject.Instance.ShowGlobalToolbar || DTSelection.HasComponent<CurvySpline, CurvySplineSegment, CurvyController, CurvyGenerator>(true);
            // Ensure we have a spline and a CP. If a spline is selected, choose the last CP
            selectedCP = DTSelection.GetAs<CurvySplineSegment>();
            selectedSpline = (selectedCP) ? selectedCP.Spline : DTSelection.GetAs<CurvySpline>();

        }
    }

    [ToolbarItem(35, "Curvy", "Import/Export", "Import or export splines", "importexport_dark,24,24", "importexport_light,24,24")]
    public class TBImportExport : DTToolbarButton
    {
        public override string StatusBarInfo { get { return "Import or export splines to/from various formats"; } }

        public TBImportExport()
        {
            KeyBindings.Add(new EditorKeyBinding("Import/Export", ""));
        }

        public override void OnClick()
        {
            ImportExportWizard.Open();
        }

        public override void OnSelectionChange()
        {
            Visible = CurvyProject.Instance.ShowGlobalToolbar;
        }
    }

    [ToolbarItem(100, "Curvy", "Select Parent", "", "selectparent,24,24")]
    public class TBSelectParent : DTToolbarButton
    {
        public override string StatusBarInfo { get { return "Select parent spline(s)"; } }

        public TBSelectParent()
        {
            KeyBindings.Add(new EditorKeyBinding("Select Parent", "", KeyCode.Backslash));
        }

        public override void OnClick()
        {
            base.OnClick();
            List<CurvySplineSegment> cps = DTSelection.GetAllAs<CurvySplineSegment>();
            List<CurvySpline> parents = new List<CurvySpline>();
            foreach (CurvySplineSegment cp in cps)
                if (cp.Spline && !parents.Contains(cp.Spline))
                    parents.Add(cp.Spline);

            DTSelection.SetGameObjects(parents.ToArray());
        }

        public override void OnSelectionChange()
        {
            Visible = DTSelection.HasComponent<CurvySplineSegment>(true);
        }
    }

    [ToolbarItem(101, "Curvy", "Select Children", "", "selectchilds,24,24")]
    public class TBSelectAllChildren : DTToolbarButton
    {
        public override string StatusBarInfo { get { return "Select Control Points"; } }

        public TBSelectAllChildren()
        {
            KeyBindings.Add(new EditorKeyBinding("Select Children", "", KeyCode.Backslash, true));
        }

        public override void OnClick()
        {
            base.OnClick();
            List<CurvySpline> splines = DTSelection.GetAllAs<CurvySpline>();
            List<CurvySplineSegment> cps = DTSelection.GetAllAs<CurvySplineSegment>();
            foreach (CurvySplineSegment cp in cps)
                if (cp.Spline && !splines.Contains(cp.Spline))
                    splines.Add(cp.Spline);
            List<CurvySplineSegment> res = new List<CurvySplineSegment>();
            foreach (CurvySpline spl in splines)
                res.AddRange(spl.ControlPointsList);

            DTSelection.SetGameObjects(res.ToArray());
        }

        public override void OnSelectionChange()
        {
            base.OnSelectionChange();
            Visible = DTSelection.HasComponent<CurvySpline, CurvySplineSegment>(true);
        }


    }

    [ToolbarItem(105, "Curvy", "Previous", "Select Previous", "prev,24,24")]
    public class TBCPPrevious : DTToolbarButton
    {
        public override string StatusBarInfo { get { return "Select previous Control Point"; } }

        public TBCPPrevious()
        {
            KeyBindings.Add(new EditorKeyBinding("Select Previous", "", KeyCode.Tab, true));
        }

        public override void OnClick()
        {
            base.OnClick();
            CurvySplineSegment cp = DTSelection.GetAs<CurvySplineSegment>();
            if (cp && cp.Spline)
                DTSelection.SetGameObjects(cp.Spline.ControlPointsList[(int)Mathf.Repeat(cp.Spline.GetControlPointIndex(cp) - 1, cp.Spline.ControlPointCount)]);
            else
            {
                CurvySpline spl = DTSelection.GetAs<CurvySpline>();
                if (spl && spl.ControlPointCount > 0)
                    DTSelection.SetGameObjects(spl.ControlPointsList[spl.ControlPointCount - 1]);
            }

        }

        public override void OnSelectionChange()
        {
            Visible = DTSelection.HasComponent<CurvySplineSegment>() || DTSelection.HasComponent<CurvySpline>();
        }
    }

    [ToolbarItem(106, "Curvy", "Next", "Select Next", "next,24,24")]
    public class TBCPNext : DTToolbarButton
    {
        public override string StatusBarInfo { get { return "Select next Control Point"; } }

        public TBCPNext()
        {
            KeyBindings.Add(new EditorKeyBinding("Select Next", "", KeyCode.Tab));
        }

        public override void OnClick()
        {
            base.OnClick();
            CurvySplineSegment cp = DTSelection.GetAs<CurvySplineSegment>(false);
            if (cp && cp.Spline)
                DTSelection.SetGameObjects(cp.Spline.ControlPointsList[(int)Mathf.Repeat(cp.Spline.GetControlPointIndex(cp) + 1, cp.Spline.ControlPointCount)]);
            else
            {
                CurvySpline spl = DTSelection.GetAs<CurvySpline>();
                if (spl && spl.ControlPointCount > 0)
                    DTSelection.SetGameObjects(spl.ControlPointsList[0]);
            }

        }

        public override void OnSelectionChange()
        {
            Visible = DTSelection.HasComponent<CurvySplineSegment>() || DTSelection.HasComponent<CurvySpline>();
        }
    }

    [ToolbarItem(120, "Curvy", "Next connected", "Toggle between connected CP", "nextcon,24,24")]
    public class TBCPNextConnected : DTToolbarButton
    {
        public override string StatusBarInfo { get { return "Select next Control Point being part of this connection"; } }

        public TBCPNextConnected()
        {
            KeyBindings.Add(new EditorKeyBinding("Toggle Connection", "", KeyCode.C));
        }

        public override void OnClick()
        {
            base.OnClick();
            CurvySplineSegment cp = DTSelection.GetAs<CurvySplineSegment>();
            if (cp)
            {
                int idx = (int)Mathf.Repeat(cp.Connection.ControlPointsList.IndexOf(cp) + 1, cp.Connection.ControlPointsList.Count);
                DTSelection.SetGameObjects(cp.Connection.ControlPointsList[idx]);
            }

        }

        public override void OnSelectionChange()
        {
            CurvySplineSegment cp = DTSelection.GetAs<CurvySplineSegment>();
            Visible = (cp != null && cp.Connection != null && cp.Connection.ControlPointsList.Count > 1);
        }


    }

    [ToolbarItem(140, "Curvy", "Sync Direction", "Synchronise direction of Bezier handles", "beziersyncdir,24,24")]
    public class TBCPBezierModeDirection : DTToolbarToggleButton
    {
        public override string StatusBarInfo { get { return "Mirror Bezier Handles Direction"; } }

        public TBCPBezierModeDirection()
        {
            KeyBindings.Add(new EditorKeyBinding("Bezier: Sync Dir", "Sync Handles Direction", KeyCode.B));
        }

        public override bool On
        {
            get
            {
                return ((CurvyProject)Project).BezierMode.HasFlag(CurvyBezierModeEnum.Direction);
            }
            set
            {
                ((CurvyProject)Project).BezierMode = ((CurvyProject)Project).BezierMode.Set(CurvyBezierModeEnum.Direction, value);
            }
        }


        public override void OnOtherItemClicked(DTToolbarItem other) { } // IMPORTANT!


        public override void OnSelectionChange()
        {
            base.OnSelectionChange();
            CurvySplineSegment cp = DTSelection.GetAs<CurvySplineSegment>();
            Visible = (cp && cp.Spline && cp.Spline.Interpolation == CurvyInterpolation.Bezier);
        }
    }

    [ToolbarItem(141, "Curvy", "Sync Length", "Synchronise length of Bezier handles", "beziersynclen,24,24")]
    public class TBCPBezierModeLength : DTToolbarToggleButton
    {
        public override string StatusBarInfo { get { return "Mirror Bezier Handles Size"; } }

        public TBCPBezierModeLength()
        {
            KeyBindings.Add(new EditorKeyBinding("Bezier: Sync Len", "Sync Handles Length", KeyCode.N));
        }

        public override bool On
        {
            get
            {
                return ((CurvyProject)Project).BezierMode.HasFlag(CurvyBezierModeEnum.Length);
            }
            set
            {
                ((CurvyProject)Project).BezierMode = ((CurvyProject)Project).BezierMode.Set(CurvyBezierModeEnum.Length, value);
            }
        }

        public override void OnOtherItemClicked(DTToolbarItem other) { } // IMPORTANT!

        public override void OnSelectionChange()
        {
            base.OnSelectionChange();
            CurvySplineSegment cp = DTSelection.GetAs<CurvySplineSegment>();
            Visible = (cp && cp.Spline && cp.Spline.Interpolation == CurvyInterpolation.Bezier);
        }
    }

    [ToolbarItem(142, "Curvy", "Sync Connection", "Synchronise Bezier handles in a Connection", "beziersynccon,24,24")]
    public class TBCPBezierModeConnections : DTToolbarToggleButton
    {
        public override string StatusBarInfo { get { return "Apply 'Sync Handles Length' and 'Sync Handles Direction' on connected Control Points as well"; } }

        public TBCPBezierModeConnections()
        {
            KeyBindings.Add(new EditorKeyBinding("Bezier: Sync Con", "Sync connected CP' handles", KeyCode.M));
        }

        public override bool On
        {
            get
            {
                return ((CurvyProject)Project).BezierMode.HasFlag(CurvyBezierModeEnum.Connections);
            }
            set
            {
                ((CurvyProject)Project).BezierMode = ((CurvyProject)Project).BezierMode.Set(CurvyBezierModeEnum.Connections, value);
            }
        }

        public override void OnOtherItemClicked(DTToolbarItem other) { } // IMPORTANT!

        public override void OnSelectionChange()
        {
            base.OnSelectionChange();
            CurvySplineSegment cp = DTSelection.GetAs<CurvySplineSegment>();
            Visible = (cp && cp.Spline && cp.Spline.Interpolation == CurvyInterpolation.Bezier);
        }
    }

    [ToolbarItem(160, "Curvy", "Shift", "Shift on curve", "shiftcp,24,24")]
    public class TBCPShift : DTToolbarToggleButton
    {
        CurvySplineSegment selCP;

        float mMin;
        float mMax;
        float mShift;

        public override string StatusBarInfo
        {
            get
            {
                return "Shifts the Control Point toward the previous or next Control Point";
            }
        }

        Vector3 getLocalPos()
        {
            CurvySpline curvySpline = selCP.Spline;
            Vector3 result;
            if (mShift >= 0)
            {
                if (curvySpline.IsControlPointASegment(selCP))
                    result = selCP.Interpolate(mShift);
                else
                {
                    CurvySplineSegment previousSegment = curvySpline.GetPreviousSegment(selCP);
                    result = previousSegment ? previousSegment.Interpolate(1) : selCP.transform.localPosition;
                }
            }
            else
            {
                CurvySplineSegment previousSegment = curvySpline.GetPreviousSegment(selCP);
                result = previousSegment ? previousSegment.Interpolate(1 + mShift) : selCP.transform.localPosition;
            }

            return result;
        }

        public override void OnSceneGUI()
        {
            if (On && selCP && selCP.Spline)
            {
                Vector3 pos = selCP.Spline.transform.TransformPoint(getLocalPos());
                DTHandles.PushHandlesColor(CurvyGlobalManager.DefaultGizmoSelectionColor);
#if UNITY_5_6_OR_NEWER
                Handles.SphereHandleCap(0, pos, Quaternion.identity, HandleUtility.GetHandleSize(pos) * CurvyGlobalManager.GizmoControlPointSize, EventType.Repaint);
#else
                Handles.SphereCap(0, pos, Quaternion.identity, HandleUtility.GetHandleSize(pos)*CurvyGlobalManager.GizmoControlPointSize);
#endif
                DTHandles.PopHandlesColor();
            }
        }

        public override void RenderClientArea(Rect r)
        {
            base.RenderClientArea(r);

            Background(r, 80, 32);
            SetElementSize(ref r, 80, 32);


            //Slider
            r.y += 8;
            mShift = GUI.HorizontalSlider(r, mShift, mMin, mMax);

            //Ok button
            Advance(ref r);
            r.width = 32;
            r.y -= 8;
            if (GUI.Button(r, "Ok"))
            {
                Undo.RecordObject(selCP.transform, "Shift Control Point");
                selCP.SetLocalPosition(getLocalPos());
                mShift = 0;
                On = false;
            }
        }



        public override void OnSelectionChange()
        {
            base.OnSelectionChange();
            selCP = DTSelection.GetAs<CurvySplineSegment>(false);
            Visible = selCP != null && selCP.Spline && selCP.Spline.IsControlPointVisible(selCP);
            if (Visible)
            {
                CurvySpline curvySpline = selCP.Spline;
                mMin = curvySpline.GetPreviousSegment(selCP) ? -0.9f : 0;
                mMax = curvySpline.IsControlPointASegment(selCP) ? 0.9f : 0;
                mShift = 0;
            }
        }
    }

    [ToolbarItem(161, "Curvy", "Set 1.", "Set as first Control Point", "setfirstcp,24,24")]
    public class TBCPSetFirst : DTToolbarButton
    {
        public override string StatusBarInfo { get { return "Make this Control Point the first of the spline"; } }

        public TBCPSetFirst()
        {
            KeyBindings.Add(new EditorKeyBinding("Set 1. CP", ""));
        }

        public override void OnClick()
        {
            base.OnClick();
            CurvySplineSegment cp = DTSelection.GetAs<CurvySplineSegment>();
            if (cp && cp.Spline)
            {
                Undo.RegisterFullObjectHierarchyUndo(cp.Spline, "Set first CP");
                cp.Spline.SetFirstControlPoint(cp);
            }
        }

        public override void OnSelectionChange()
        {
            base.OnSelectionChange();
            CurvySplineSegment cp = DTSelection.GetAs<CurvySplineSegment>();
            Visible = (cp != null);
            Enabled = Visible && cp.Spline && cp.Spline.GetControlPointIndex(cp) > 0;
        }
    }

    [ToolbarItem(162, "Curvy", "Join", "Join Splines", "join,24,24")]
    public class TBCPJoin : DTToolbarButton
    {
        public override string StatusBarInfo
        {
            get
            {
                return mInfo;
            }
        }

        string mInfo;

        public TBCPJoin()
        {
            KeyBindings.Add(new EditorKeyBinding("Join Spline", "Join two splines"));
        }


        public override void OnClick()
        {
            base.OnClick();
            CurvySpline source = DTSelection.GetAs<CurvySpline>();
            CurvySplineSegment destCP = DTSelection.GetAs<CurvySplineSegment>();
            int selIdx = destCP.Spline.GetControlPointIndex(destCP) + source.ControlPointCount + 1;
            source.JoinWith(destCP);
            DTSelection.SetGameObjects(destCP.Spline.ControlPointsList[Mathf.Min(destCP.Spline.ControlPointCount - 1, selIdx)]);
        }

        public override void OnSelectionChange()
        {
            CurvySpline source = DTSelection.GetAs<CurvySpline>();
            CurvySplineSegment destCP = DTSelection.GetAs<CurvySplineSegment>();
            Visible = source && destCP && destCP.Spline && source != destCP.Spline;
            mInfo = (Visible) ? string.Format("Insert all Control Points of <b>{0}</b> after <b>{1}</b>", source.name, destCP.ToString()) : "";
        }


    }

    [ToolbarItem(163, "Curvy", "Split", "Split spline at selection", "split,24,24")]
    public class TBCPSplit : DTToolbarButton
    {
        public override string StatusBarInfo { get { return "Split current Spline and make this Control Point the first of a new spline"; } }

        public TBCPSplit()
        {
            KeyBindings.Add(new EditorKeyBinding("Split Spline", "Split spline at selection"));
        }

        public override void OnClick()
        {
            base.OnClick();
            CurvySplineSegment cp = DTSelection.GetAs<CurvySplineSegment>();
            DTSelection.SetGameObjects(cp.Spline.Split(cp));
        }

        public override void OnSelectionChange()
        {
            CurvySplineSegment cp = DTSelection.GetAs<CurvySplineSegment>();
            Visible = cp && cp.Spline && cp.Spline.IsControlPointASegment(cp) && (cp.Spline.FirstSegment != cp);
        }
    }

    [ToolbarItem(165, "Curvy", "Connect", "Create a connection", "connectionpos_dark,24,24", "connectionpos_light,24,24")]
    public class TBCPConnect : DTToolbarButton
    {
        public override string StatusBarInfo { get { return "Add a connection"; } }

        public TBCPConnect()
        {
            KeyBindings.Add(new EditorKeyBinding("Connect", "Create connection"));
        }

        public override void OnClick()
        {
            List<CurvySplineSegment> selected = DTSelection.GetAllAs<CurvySplineSegment>();
            CurvySplineSegment[] unconnected = (from cp in selected
                                                where !cp.Connection
                                                select cp).ToArray();

            if (unconnected.Length > 0)
            {
                CurvyConnection con = (from cp in selected
                                       where cp.Connection != null
                                       select cp.Connection).FirstOrDefault();

                if (con == null)
                {
                    con = CurvyConnection.Create(unconnected); // Undo inside
                    //con.AddControlPoints(unconnected); // Undo inside
                    //con.AutoSetFollowUp();
                }
                else
                    con.AddControlPoints(unconnected); // Undo inside
            }
            /*
            if (unconnected.Length == 2)
            {
                var source = unconnected[1];
                var dest = unconnected[0];
                source.ConnectTo(dest, (source.transform.position == dest.transform.position), false);
            }
            else
            {
                if (con == null)
                {
                    con = CurvyConnection.Create(); // Undo inside
                }
                con.AddControlPoints(unconnected); // Undo inside
            }
            */
            foreach (CurvySplineSegment cp in unconnected)
                EditorUtility.SetDirty(cp);

            CurvyProject.Instance.ScanConnections();

            //EditorApplication.RepaintHierarchyWindow();
        }

        public override void OnSelectionChange()
        {
            List<CurvySplineSegment> selected = DTSelection.GetAllAs<CurvySplineSegment>();
            List<CurvySplineSegment> unconnected = (from cp in selected
                                                    where !cp.Connection
                                                    select cp).ToList();

            Visible = (unconnected.Count > 0);
            /*
                      (unconnected.Count==1 ||
                      unconnected.Count>2 ||
                      (selected.Count == 2 && selected[0].CanConnectTo(selected[1])));
              */
        }
    }
    /*
    [ToolbarItem(180, "Curvy", "Limit Len", "Constraint max. Spline length", "constraintlength,24,24")]
    public class TBCPLengthConstraint : DTToolbarToggleButton
    {
        public float MaxSplineLength;
        CurvySpline Spline;

        public TBCPLengthConstraint()
        {
            KeyBindings.Add(new EditorKeyBinding("Constraint Length", "Spline: Constraint Length"));
        }
        Vector3[] storedPosPrev = new Vector3[0];
        Vector3[] storedPos = new Vector3[0];


        void StorePos()
        {
            storedPosPrev = storedPos;
            storedPos = new Vector3[Selection.transforms.Length];
            for (int i = 0; i < storedPos.Length; i++)
                storedPos[i] = Selection.transforms[i].position;
        }
        void RestorePos()
        {
            Debug.Log("Restore");
            for (int i = 0; i < storedPosPrev.Length; i++)
                Selection.transforms[i].position = storedPosPrev[i];
        }

        public override void RenderClientArea(Rect r)
        {
            base.RenderClientArea(r);
            SetElementSize(ref r, 84, 22);
            Background(r, 84, 22);
            r.width = 60;
            MaxSplineLength = EditorGUI.FloatField(r, MaxSplineLength);
            r.x += 62;
            r.width = 22;
            if (GUI.Button(r, "<"))
            {
                var cp = DTSelection.GetAs<CurvySplineSegment>();
                if (cp)
                    MaxSplineLength = cp.Spline.Length;
            }
        }

        public override void OnSelectionChange()
        {
            var cp = DTSelection.GetAs<CurvySplineSegment>();
            Visible = cp != null;
            Spline = (cp) ? cp.Spline : null;
        }

        public override void OnSceneGUI()
        {
            base.OnSceneGUI();

            if (On && Spline)
            {
                if (Spline.Length > MaxSplineLength)
                {
                    RestorePos();
                    Spline.SetDirtyAll();
                    Spline.Refresh();
                }
                else
                    StorePos();
            }

        }
    }
    */
    [ToolbarItem(190, "Curvy", "Camera Project", "Project camera", "camproject,24,24")]
    public class TBCPCameraProject : DTToolbarToggleButton
    {
        public override string StatusBarInfo { get { return "Raycast and move Control Points"; } }

        List<CurvySplineSegment> mCPSelection;

        public TBCPCameraProject()
        {
        }

        public override void RenderClientArea(Rect r)
        {
            base.RenderClientArea(r);
            SetElementSize(ref r, 32, 32);

            if (GUI.Button(r, "OK"))
            {
                foreach (CurvySplineSegment cp in mCPSelection)
                {
                    RaycastHit hit;
                    if (Physics.Raycast(new Ray(cp.transform.position, SceneView.currentDrawingSceneView.camera.transform.forward), out hit))
                    {
                        Undo.RecordObject(cp.transform, "Project Control Points");
                        cp.transform.position = hit.point;
                    }
                }

                On = false;
            }
        }

        public override void OnSceneGUI()
        {
            base.OnSceneGUI();
            if (On && SceneView.currentDrawingSceneView != null)
            {
                DTHandles.PushHandlesColor(Color.red);
                foreach (CurvySplineSegment cp in mCPSelection)
                {
                    RaycastHit hit;
                    if (Physics.Raycast(new Ray(cp.transform.position, SceneView.currentDrawingSceneView.camera.transform.forward), out hit))
                    {
                        Handles.DrawDottedLine(cp.transform.position, hit.point, 2);
#if UNITY_5_6_OR_NEWER
                        Handles.SphereHandleCap(0, hit.point, Quaternion.identity, HandleUtility.GetHandleSize(hit.point) * 0.1f, EventType.Repaint);
#else
                        Handles.SphereCap(0, hit.point, Quaternion.identity, HandleUtility.GetHandleSize(hit.point)*0.1f);
#endif
                    }
                }
                DTHandles.PopHandlesColor();
            }
        }

        public override void OnSelectionChange()
        {
            mCPSelection = DTSelection.GetAllAs<CurvySplineSegment>();
            Visible = mCPSelection.Count > 0;
            if (!Visible)
                On = false;
        }

        public override void HandleEvents(Event e)
        {
            base.HandleEvents(e);
            if (On)
                _StatusBar.Set("Click <b>OK</b> to apply the preview changes", "CameraProject");
            else
                _StatusBar.Clear("CameraProject");
        }
    }

    [ToolbarItem(200, "Curvy", "CPTools", "Control Point Tools", "tools,24,24")]
    public class TBCPTools : DTToolbarToggleButton
    {
        public override string StatusBarInfo { get { return "Open Control Point Tools menu"; } }

        public class SplineRange
        {
            public CurvySpline Spline;
            public CurvySplineSegment Low;
            public CurvySplineSegment High;

            public bool CanSubdivide
            {
                get { return Low && High && (High.Spline.GetControlPointIndex(High) - Low.Spline.GetControlPointIndex(Low) > 0); }
            }

            public bool CanSimplify
            {
                get { return Low && High && (High.Spline.GetControlPointIndex(High) - Low.Spline.GetControlPointIndex(Low) > 1); }
            }

            public SplineRange(CurvySpline spline)
            {
                Spline = spline;
                Low = null;
                High = null;
            }

            public void AddCP(CurvySplineSegment cp)
            {
                if (Low == null || Low.Spline.GetControlPointIndex(Low) > cp.Spline.GetControlPointIndex(cp))
                    Low = cp;
                if (High == null || High.Spline.GetControlPointIndex(High) < cp.Spline.GetControlPointIndex(cp))
                    High = cp;
            }
        }

        List<CurvySplineSegment> mCPSelection;
        readonly Dictionary<CurvySpline, SplineRange> mSplineRanges = new Dictionary<CurvySpline, SplineRange>();

        public bool CanSubdivide
        {
            get
            {
                foreach (SplineRange sr in mSplineRanges.Values)
                    if (sr.CanSubdivide)
                        return true;
                return false;
            }
        }

        public bool CanSimplify
        {
            get
            {
                foreach (SplineRange sr in mSplineRanges.Values)
                    if (sr.CanSimplify)
                        return true;
                return false;
            }
        }



        public override void RenderClientArea(Rect r)
        {
            base.RenderClientArea(r);
            SetElementSize(ref r, 32, 32);
            GUI.enabled = CanSubdivide;
            if (GUI.Button(r, new GUIContent(CurvyStyles.IconSubdivide, "Subdivide")))
                Subdivide();
            Advance(ref r);

            GUI.enabled = CanSimplify;
            if (GUI.Button(r, new GUIContent(CurvyStyles.IconSimplify, "Simplify")))
                Simplify();
            Advance(ref r);
            if (GUI.Button(r, new GUIContent(CurvyStyles.IconEqualize, "Equalize")))
                Equalize();
            GUI.enabled = true;
        }

        public override void OnSelectionChange()
        {
            mCPSelection = DTSelection.GetAllAs<CurvySplineSegment>().Where(cp => cp.Spline != null).ToList();
            getRange();
            Visible = mCPSelection.Count > 1;
            if (!Visible)
                On = false;
        }

        void Subdivide()
        {
            foreach (SplineRange sr in mSplineRanges.Values)
                if (sr.CanSubdivide)
                    sr.Spline.Subdivide(sr.Low, sr.High);
        }

        void Simplify()
        {
            foreach (SplineRange sr in mSplineRanges.Values)
                if (sr.CanSimplify)
                    sr.Spline.Simplify(sr.Low, sr.High);
        }

        void Equalize()
        {
            foreach (SplineRange sr in mSplineRanges.Values)
                if (sr.CanSimplify)
                    sr.Spline.Equalize(sr.Low, sr.High);
        }

        void getRange()
        {
            mSplineRanges.Clear();
            foreach (CurvySplineSegment cp in mCPSelection)
            {
                SplineRange sr;
                if (!mSplineRanges.TryGetValue(cp.Spline, out sr))
                {
                    sr = new SplineRange(cp.Spline);
                    mSplineRanges.Add(cp.Spline, sr);
                }

                sr.AddCP(cp);
            }
        }
    }

    [ToolbarItem(120, "Curvy", "Set Pivot", "", "centerpivot,24,24")]
    public class TBSplineSetPivot : DTToolbarToggleButton
    {
        public override string StatusBarInfo { get { return "Set center/pivot point"; } }

        float pivotX;
        float pivotY;
        float pivotZ;

        public override void OnSelectionChange()
        {
            Visible = DTSelection.HasComponent<CurvySpline>(true);
        }

        public override void RenderClientArea(Rect r)
        {
            base.RenderClientArea(r);

            Background(r, 182, 187);
            SetElementSize(ref r, 180, 185);

            EditorGUIUtility.labelWidth = 20;
            GUILayout.BeginArea(new Rect(r));

            GUILayout.Label("X/Y", EditorStyles.boldLabel);
            for (int y = -1; y <= 1; y++)
            {
                GUILayout.BeginHorizontal();
                for (int x = -1; x <= 1; x++)
                {
                    DTGUI.PushBackgroundColor((x == pivotX && y == pivotY) ? Color.red : GUI.backgroundColor);
                    if (GUILayout.Button("", GUILayout.Width(20)))
                    {
                        pivotX = x;
                        pivotY = y;
                    }

                    DTGUI.PopBackgroundColor();
                }

                if (y == -1)
                {
                    GUILayout.Space(20);
                    pivotX = EditorGUILayout.FloatField("X", pivotX);
                }
                else if (y == 0)
                {
                    GUILayout.Space(20);
                    pivotY = EditorGUILayout.FloatField("Y", pivotY);
                }

                GUILayout.EndVertical();
            }

            GUILayout.Label("Z/Y", EditorStyles.boldLabel);
            for (int y = -1; y <= 1; y++)
            {
                GUILayout.BeginHorizontal();
                for (int z = -1; z <= 1; z++)
                {
                    DTGUI.PushBackgroundColor((y == pivotY && z == pivotZ) ? Color.red : GUI.backgroundColor);
                    if (GUILayout.Button("", GUILayout.Width(20)))
                    {
                        pivotY = y;
                        pivotZ = z;
                    }

                    DTGUI.PopBackgroundColor();
                }

                if (y == -1)
                {
                    GUILayout.Space(20);
                    pivotZ = EditorGUILayout.FloatField("Z", pivotZ);
                }
                else if (y == 0)
                {
                    GUILayout.Space(20);
                    pivotY = EditorGUILayout.FloatField("Y", pivotY);
                }

                GUILayout.EndVertical();
            }

            if (GUILayout.Button("Apply"))
            {
                SetPivot();
                On = false;
            }
            GUILayout.EndArea();
        }

        public override void OnSceneGUI()
        {
            if (On)
            {
                List<CurvySpline> splines = DTSelection.GetAllAs<CurvySpline>();
                foreach (CurvySpline spl in splines)
                {

                    Vector3 p = spl.SetPivot(pivotX, pivotY, pivotZ, true);
                    DTHandles.PushHandlesColor(new Color(0.3f, 0, 0));
                    DTHandles.BoundsCap(spl.Bounds);
                    Handles.SphereHandleCap(0, p, Quaternion.identity, HandleUtility.GetHandleSize(p) * .1f, EventType.Repaint);
                    DTHandles.PopHandlesColor();
                }
            }
        }



        void SetPivot()
        {
            List<CurvySpline> splines = DTSelection.GetAllAs<CurvySpline>();
            foreach (CurvySpline spl in splines)
                spl.SetPivot(pivotX, pivotY, pivotZ);
        }

    }

    [ToolbarItem(122, "Curvy", "Flip", "Flip spline direction", "flip,24,24")]
    public class TBSplineFlip : DTToolbarButton
    {
        public override string StatusBarInfo { get { return "Invert all Control Points, making the spline direction flip"; } }

        public TBSplineFlip()
        {
            KeyBindings.Add(new EditorKeyBinding("Flip", "Flip spline direction"));
        }

        public override void OnClick()
        {
            List<CurvySpline> splines = DTSelection.GetAllAs<CurvySpline>();
            foreach (CurvySpline spline in splines)
            {
                spline.Flip();
            }
        }

        public override void OnSelectionChange()
        {
            Visible = DTSelection.HasComponent<CurvySpline>(true);
        }
    }

    [ToolbarItem(124, "Curvy", "Normalize", "Normalize scale", "normalize,24,24")]
    public class TBSplineNormalize : DTToolbarButton
    {
        public override string StatusBarInfo { get { return "Apply transform scale to Control Points and reset scale to 1"; } }

        public TBSplineNormalize()
        {
            KeyBindings.Add(new EditorKeyBinding("Normalize", "Normalize spline"));
        }

        public override void OnClick()
        {
            List<CurvySpline> splines = DTSelection.GetAllAs<CurvySpline>();
            foreach (CurvySpline spline in splines)
            {
                spline.Normalize();
            }
        }

        public override void OnSelectionChange()
        {
            Visible = DTSelection.HasComponent<CurvySpline>(true);
        }
    }

    [ToolbarItem(124, "Curvy", "Shape", "Apply a shape", "shapewizard,24,24")]
    public class TBSplineSetShape : DTToolbarToggleButton
    {
        public override string StatusBarInfo { get { return "Apply a shape. <b><color=#ff0000>WARNING: THIS CAN'T BE UNDONE!</color></b>"; } }

        Vector2 scroll;
        readonly float winHeight = 120;

        public override void RenderClientArea(Rect r)
        {
            base.RenderClientArea(r);
            CurvySpline spline;
            CurvyShape shape = DTSelection.GetAs<CurvyShape>();
            if (shape == null && (spline = DTSelection.GetAs<CurvySpline>()))
            {
                shape = spline.gameObject.AddComponent<CSCircle>();
                shape.Dirty = true;
                shape.Refresh();
            }

            if (shape != null)
            {
                CurvyShapeEditor ShapeEditor = Editor.CreateEditor(shape, typeof(CurvyShapeEditor)) as CurvyShapeEditor;
                if (ShapeEditor != null)
                {
                    FocusedItem = this;
                    ShapeEditor.ShowOnly2DShapes = false;
                    ShapeEditor.ShowPersistent = true;

                    Background(r, 300, winHeight);
                    SetElementSize(ref r, 300, winHeight);

                    GUILayout.BeginArea(r);
                    scroll = GUILayout.BeginScrollView(scroll, GUILayout.Height(winHeight - 25));

                    ShapeEditor.OnEmbeddedGUI();

                    GUILayout.EndScrollView();
                    GUILayout.EndArea();

                    r.y += winHeight - 20;
                    r.height = 20;

                    if (GUI.Button(r, "Close"))
                    {
                        On = false;
                    }

                    Editor.DestroyImmediate(ShapeEditor);
                }
            }
        }

        public override void OnSelectionChange()
        {
            Visible = DTSelection.HasComponent<CurvySpline>();
            scroll = Vector2.zero;
        }
    }

    [ToolbarItem(200, "Curvy", "Tools", "Spline Tools", "tools,24,24")]
    public class TBSplineTools : DTToolbarToggleButton
    {

        public override string StatusBarInfo { get { return "Open Spline Tools menu"; } }


        public override void RenderClientArea(Rect r)
        {
            base.RenderClientArea(r);
            SetElementSize(ref r, 32, 32);
            if (GUI.Button(r, new GUIContent(CurvyStyles.IconMeshExport, "Spline to Mesh")))
            {
                CurvySplineExportWizard.Create();
                On = false;
            }
            Advance(ref r);
            if (GUI.Button(r, new GUIContent(CurvyStyles.IconSyncFromHierarchy, "Sync from Hierarchy")))
            {
                List<CurvySpline> sel = DTSelection.GetAllAs<CurvySpline>();
                foreach (CurvySpline spl in sel)
                {
                    spl.SyncSplineFromHierarchy();
                    spl.ApplyControlPointsNames();
                    spl.Refresh();
                    On = false;
                }
            }
            Advance(ref r);
            if (GUI.Button(r, new GUIContent(CurvyStyles.IconSelectContainingConnections, "Select connections connecting only CPs within the selected spline(s)")))
            {
                List<CurvySpline> sel = DTSelection.GetAllAs<CurvySpline>();
                DTSelection.SetGameObjects(CurvyGlobalManager.Instance.GetContainingConnections(sel.ToArray()));
            }
        }

        public override void OnSelectionChange()
        {
            Visible = DTSelection.HasComponent<CurvySpline>(true);
        }
    }

    [ToolbarItem(190, "Curvy", "Edit", "Open CG Editor", "opengraph_dark,24,24", "opengraph_light,24,24")]
    public class TBPCGOpenGraph : DTToolbarButton
    {
        public override string StatusBarInfo { get { return "Open Curvy Generator Editor"; } }

        public override void OnClick()
        {
            base.OnClick();
            CurvyGenerator pcg = DTSelection.GetAs<CurvyGenerator>();
            if (pcg)
                FluffyUnderware.CurvyEditor.Generator.CGGraph.Open(pcg);

        }

        public override void OnSelectionChange()
        {
            Visible = DTSelection.HasComponent<CurvyGenerator>();
        }
    }



}