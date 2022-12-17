// =====================================================================
// Copyright 2013-2022 ToolBuddy
// All rights reserved
// 
// http://www.toolbuddy.net
// =====================================================================

using UnityEngine;
using UnityEditor;
using FluffyUnderware.Curvy;
using FluffyUnderware.DevTools;
using FluffyUnderware.DevTools.Extensions;
using FluffyUnderware.DevToolsEditor;
using System.Collections.Generic;

namespace FluffyUnderware.CurvyEditor
{
    public class CurvyProject : DTProject
    {
        public const string NAME = "Curvy";
        public const string RELPATH_SHAPEWIZARDSCRIPTS = "/Shapes";
        public const string RELPATH_CGMODULEWIZARDSCRIPTS = "/Generator Modules";
        public const string RELPATH_CGMODULEWIZARDEDITORSCRIPTS = "/Generator Modules/Editor";
        public const string RELPATH_CGTEMPLATES = "/Generator Templates";

        public static CurvyProject Instance
        {
            get
            {
                return (CurvyProject)DT.Project(NAME);
            }
        }

        #region ### Persistent Settings ###
        // DESIGN those settings are a mess: some are public fields, other properties of which the setter updates the editor preferences, and finally others ae part of CurvyGlobalManager (see CurvyProject.LoadPreferences to have a listing of them all). Shouldn't all of those settings be treated similarly?

        // Settings from Preferences window not stored in CurvyGlobalManager

        public bool SnapValuePrecision = true;
        /// <summary>
        /// If enabled and the spline has Restrict To 2D enabled, dots are shown instead of the default move handles
        /// </summary>
        public bool UseTiny2DHandles = false;
        /// <summary>
        /// Should the spline's text gizmos fade if the spline is too small on the screen
        /// </summary>
        public bool AutoFadeLabels = true;
        public bool ShowGlobalToolbar = true;
        public bool ShowHints = true;
        public bool EnableMetrics = true;
        public bool EnableAnnouncements = true;

        // Settings made in the toolbar or somewhere else

        bool mCGAutoModuleDetails = false;
        public bool CGAutoModuleDetails
        {
            get { return mCGAutoModuleDetails; }
            set
            {
                if (mCGAutoModuleDetails != value)
                {
                    mCGAutoModuleDetails = value;
                    SetEditorPrefs("CGAutoModuleDetails", mCGAutoModuleDetails);
                }
            }
        }

        bool mCGSynchronizeSelection = true;
        public bool CGSynchronizeSelection
        {
            get { return mCGSynchronizeSelection; }
            set
            {
                if (mCGSynchronizeSelection != value)
                {
                    mCGSynchronizeSelection = value;
                    SetEditorPrefs("CGSynchronizeSelection", mCGSynchronizeSelection);
                }
            }
        }

        bool mCGShowHelp = true;
        public bool CGShowHelp
        {
            get { return mCGShowHelp; }
            set
            {
                if (mCGShowHelp != value)
                {
                    mCGShowHelp = value;
                    SetEditorPrefs("CGShowHelp", mCGShowHelp);
                }
            }
        }

        int mCGGraphSnapping = 5;
        /// <summary>
        /// The size of the grid used for snapping when dragging a module in Curvy Generator Graph
        /// </summary>
        public int CGGraphSnapping
        {
            get { return mCGGraphSnapping; }
            set
            {
                int v = Mathf.Max(1, value);
                if (mCGGraphSnapping != v)
                {
                    mCGGraphSnapping = v;
                    SetEditorPrefs("CGGraphSnapping", mCGGraphSnapping);
                }
            }
        }

        string mCustomizationRootPath = "Packages/Curvy Customization";
        public string CustomizationRootPath
        {
            get
            {
                return mCustomizationRootPath;
            }
            set
            {
                if (mCustomizationRootPath != value)
                {
                    mCustomizationRootPath = value;
                    SetEditorPrefs("CustomizationRootPath", mCustomizationRootPath);
                }
            }
        }

        CurvyBezierModeEnum mBezierMode = CurvyBezierModeEnum.Direction | CurvyBezierModeEnum.Length;
        public CurvyBezierModeEnum BezierMode
        {
            get { return mBezierMode; }
            set
            {
                if (mBezierMode != value)
                {
                    mBezierMode = value;
                    SetEditorPrefs("BezierMode", mBezierMode);
                }
            }
        }

        CurvyAdvBezierModeEnum mAdvBezierMode = CurvyAdvBezierModeEnum.Direction | CurvyAdvBezierModeEnum.Length;
        public CurvyAdvBezierModeEnum AdvBezierMode
        {
            get { return mAdvBezierMode; }
            set
            {
                if (mAdvBezierMode != value)
                {
                    mAdvBezierMode = value;
                    SetEditorPrefs("AdvBezierMode", mAdvBezierMode);
                }
            }
        }

        bool mShowAboutOnLoad = true;
        public bool ShowAboutOnLoad
        {
            get
            {
                return mShowAboutOnLoad;
            }
            set
            {
                if (mShowAboutOnLoad != value)
                    mShowAboutOnLoad = value;
                SetEditorPrefs("ShowAboutOnLoad", mShowAboutOnLoad);
            }
        }

        #endregion





        static Vector2 scroll;
        static readonly bool[] foldouts = new bool[4] { true, true, true, true };
        readonly List<int> mShowConIconObjects = new List<int>();


        public CurvyProject()
            : base(NAME, CurvySpline.VERSION)
        {
            Resource = CurvyResource.Instance;
            Undo.undoRedoPerformed -= OnUndoRedo;
            Undo.undoRedoPerformed += OnUndoRedo;
            EditorApplication.update -= OnUpdate;
            EditorApplication.update += OnUpdate;
            EditorApplication.update += checkLaunch;
            EditorApplication.hierarchyChanged -= ScanConnections;
            EditorApplication.hierarchyChanged += ScanConnections;
            EditorApplication.hierarchyWindowItemOnGUI -= OnHierarchyWindowItemOnGUI;
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemOnGUI;
            ScanConnections();
        }

        /// <summary>
        /// Rebuilds the list of GameObject that needs to show a connection icon in the hierarchy window
        /// </summary>
        /// <remarks>Usually there is no need to call this manually</remarks>
        public void ScanConnections()
        {
            int old = mShowConIconObjects.Count;
            mShowConIconObjects.Clear();

            CurvyConnection[] o = GameObject.FindObjectsOfType<CurvyConnection>();
            foreach (CurvyConnection con in o)
            {
                foreach (CurvySplineSegment cp in con.ControlPointsList)
                {
                    if (cp != null && cp.gameObject != null)
                        // see comment in CurvyConnection.DoUpdate to know more about when cp.gameObject can be null
                        mShowConIconObjects.Add(cp.gameObject.GetInstanceID());
                }
            }

            if (old != mShowConIconObjects.Count)
                EditorApplication.RepaintHierarchyWindow();
        }

        void OnHierarchyWindowItemOnGUI(int instanceid, Rect selectionrect)
        {
            if (mShowConIconObjects.Contains(instanceid))
            {
                GUI.DrawTexture(new Rect(selectionrect.xMax - 14, selectionrect.yMin + 4, 10, 10), CurvyStyles.HierarchyConnectionTexture);
            }
        }

        void checkLaunch()
        {
            EditorApplication.update -= checkLaunch;
            if (ShowAboutOnLoad)
                AboutWindow.Open();
        }

        void OnUpdate()
        {

            // check if a deleted Curvy object defines a new object to select
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                CurvySpline._newSelectionInstanceIDINTERNAL = 0;

            if (CurvySpline._newSelectionInstanceIDINTERNAL != 0)
            {
                Object o = EditorUtility.InstanceIDToObject(CurvySpline._newSelectionInstanceIDINTERNAL);
                if (o != null && o is Component)
                    DTSelection.SetGameObjects((Component)o);
                CurvySpline._newSelectionInstanceIDINTERNAL = 0;
            }
        }

        void OnUndoRedo()
        {
            List<CurvySpline> splines = DTSelection.GetAllAs<CurvySpline>();
            List<CurvySplineSegment> cps = DTSelection.GetAllAs<CurvySplineSegment>();
            foreach (CurvySplineSegment cp in cps)
            {
                CurvySpline curvySpline = cp.transform.parent
                    ? cp.transform.parent.GetComponent<CurvySpline>()
                    : cp.Spline;
                if (curvySpline && !splines.Contains(curvySpline))
                    splines.Add(curvySpline);
            }

            foreach (CurvySpline spl in splines)
            {
                spl.SyncSplineFromHierarchy();
                //spl.SetDirtyAll(SplineDirtyingType.Everything, true); is already done in spl.SyncSplineFromHierarchy();
                spl.Refresh();
            }
        }



        public override void ResetPreferences()
        {
            //reset only settings that are settable through the Preferences window

            base.ResetPreferences();
            CurvyGlobalManager.DefaultInterpolation = CurvyInterpolation.CatmullRom;
            CurvyGlobalManager.DefaultGizmoColor = CurvyGlobalManager.DefaultDefaultGizmoColor;
            CurvyGlobalManager.DefaultGizmoSelectionColor = CurvyGlobalManager.DefaultDefaultGizmoSelectionColor;
            CurvyGlobalManager.GizmoControlPointSize = 0.15f;
            CurvyGlobalManager.GizmoOrientationLength = 1f;
            CurvyGlobalManager.GizmoOrientationColor = CurvyGlobalManager.DefaultGizmoOrientationColor;
            CurvyGlobalManager.SceneViewResolution = 0.5f;
            CurvyGlobalManager.HideManager = false;
            CurvyGlobalManager.SplineLayer = 0;
            CurvyGlobalManager.SaveGeneratorOutputs = true;

            CustomizationRootPath = "Packages/Curvy Customization";
            SnapValuePrecision = true;
            UseTiny2DHandles = false;
            AutoFadeLabels = true;
            ShowGlobalToolbar = true;
            EnableMetrics = true;
            EnableAnnouncements = true;
            ShowHints = true;

            ToolbarMode = DTToolbarMode.Full;
            ToolbarOrientation = DTToolbarOrientation.Left;

            DT._UseSnapValuePrecision = SnapValuePrecision;
            DTToolbarItem._StatusBar.Visible = ShowHints;
        }

        public override void LoadPreferences()
        {
            if (GetEditorPrefs("Version", "PreDT") == "PreDT")
            {
                DeletePreDTSettings();
                SavePreferences();
            }
            base.LoadPreferences();
            CurvyGlobalManager.DefaultInterpolation = GetEditorPrefs("DefaultInterpolation", CurvyGlobalManager.DefaultInterpolation);
            CurvyGlobalManager.DefaultGizmoColor = GetEditorPrefs("GizmoColor", CurvyGlobalManager.DefaultGizmoColor);
            CurvyGlobalManager.DefaultGizmoSelectionColor = GetEditorPrefs("GizmoSelectionColor", CurvyGlobalManager.DefaultGizmoSelectionColor);
            CurvyGlobalManager.GizmoControlPointSize = GetEditorPrefs("GizmoControlPointSize", CurvyGlobalManager.GizmoControlPointSize);
            CurvyGlobalManager.GizmoOrientationLength = GetEditorPrefs("GizmoOrientationLength", CurvyGlobalManager.GizmoOrientationLength);
            CurvyGlobalManager.GizmoOrientationColor = GetEditorPrefs("GizmoOrientationColor", CurvyGlobalManager.GizmoOrientationColor);
            CurvyGlobalManager.Gizmos = GetEditorPrefs("Gizmos", CurvyGlobalManager.Gizmos);
            CurvyGlobalManager.SceneViewResolution = Mathf.Clamp01(GetEditorPrefs("SceneViewResolution", CurvyGlobalManager.SceneViewResolution));
            CurvyGlobalManager.HideManager = GetEditorPrefs("HideManager", CurvyGlobalManager.HideManager);
            CurvyGlobalManager.SplineLayer = GetEditorPrefs("SplineLayer", CurvyGlobalManager.SplineLayer);
            CurvyGlobalManager.SaveGeneratorOutputs = GetEditorPrefs("SaveGeneratorOutputs", CurvyGlobalManager.SaveGeneratorOutputs);

            mCustomizationRootPath = GetEditorPrefs("CustomizationRootPath", mCustomizationRootPath);
            SnapValuePrecision = GetEditorPrefs("SnapValuePrecision", true);
            UseTiny2DHandles = GetEditorPrefs("UseTiny2DHandles", UseTiny2DHandles);
            AutoFadeLabels = GetEditorPrefs("AutoFadeLabels", AutoFadeLabels);
            ShowGlobalToolbar = GetEditorPrefs("ShowGlobalToolbar", ShowGlobalToolbar);
            EnableMetrics = GetEditorPrefs("EnableMetrics", true);
            EnableAnnouncements = GetEditorPrefs("EnableAnnouncements", true);
            ShowHints = GetEditorPrefs("ShowHints", ShowHints);

            CurvyGlobalManager.SaveRuntimeSettings();

            mCGAutoModuleDetails = GetEditorPrefs("CGAutoModuleDetails", mCGAutoModuleDetails);
            mCGSynchronizeSelection = GetEditorPrefs("CGSynchronizeSelection", mCGSynchronizeSelection);
            mCGShowHelp = GetEditorPrefs("CGShowHelp", mCGShowHelp);
            mCGGraphSnapping = GetEditorPrefs("CGGraphSnapping", mCGGraphSnapping);
            mBezierMode = GetEditorPrefs("BezierMode", mBezierMode);
            mAdvBezierMode = GetEditorPrefs("AdvBezierMode", mAdvBezierMode);
            mShowAboutOnLoad = GetEditorPrefs("ShowAboutOnLoad", mShowAboutOnLoad);

            DT._UseSnapValuePrecision = SnapValuePrecision;
            DTToolbarItem._StatusBar.Visible = ShowHints;
        }

        public override void SavePreferences()
        {
            base.SavePreferences();
            SetEditorPrefs("DefaultInterpolation", CurvyGlobalManager.DefaultInterpolation);
            SetEditorPrefs("GizmoColor", CurvyGlobalManager.DefaultGizmoColor);
            SetEditorPrefs("GizmoSelectionColor", CurvyGlobalManager.DefaultGizmoSelectionColor);
            SetEditorPrefs("GizmoControlPointSize", CurvyGlobalManager.GizmoControlPointSize);
            SetEditorPrefs("GizmoOrientationLength", CurvyGlobalManager.GizmoOrientationLength);
            SetEditorPrefs("GizmoOrientationColor", CurvyGlobalManager.GizmoOrientationColor);
            SetEditorPrefs("Gizmos", CurvyGlobalManager.Gizmos);
            SetEditorPrefs("SnapValuePrecision", SnapValuePrecision);
            SetEditorPrefs("EnableAnnouncements", EnableAnnouncements);
            SetEditorPrefs("EnableMetrics", EnableMetrics);
            SetEditorPrefs("SceneViewResolution", CurvyGlobalManager.SceneViewResolution);
            SetEditorPrefs("HideManager", CurvyGlobalManager.HideManager);
            SetEditorPrefs("UseTiny2DHandles", UseTiny2DHandles);
            SetEditorPrefs("AutoFadeLabels", AutoFadeLabels);
            SetEditorPrefs("ShowGlobalToolbar", ShowGlobalToolbar);
            SetEditorPrefs("ShowHints", ShowHints);
            SetEditorPrefs("SplineLayer", CurvyGlobalManager.SplineLayer);
            SetEditorPrefs("SaveGeneratorOutputs", CurvyGlobalManager.SaveGeneratorOutputs);
            SetEditorPrefs("CustomizationRootPath", mCustomizationRootPath);

            CurvyGlobalManager.SaveRuntimeSettings();
            DT._UseSnapValuePrecision = SnapValuePrecision;
            DTToolbarItem._StatusBar.Visible = ShowHints;
        }

        protected override void UpgradePreferences(string oldVersion)
        {
            base.UpgradePreferences(oldVersion);
            // Ensure that About Window will be shown after upgrade
            DeleteEditorPrefs("ShowAboutOnLoad");
            if (oldVersion == "2.0.0")
            {
                if (GetEditorPrefs("GizmoOrientationLength", CurvyGlobalManager.GizmoOrientationLength) == 4)
                    DeleteEditorPrefs("GizmoOrientationLength");
            }

        }

        void DeletePreDTSettings()
        {
            DTLog.Log("[Curvy] Removing old preferences");
            EditorPrefs.DeleteKey("Curvy_GizmoColor");
            EditorPrefs.DeleteKey("Curvy_GizmoSelectionColor");
            EditorPrefs.DeleteKey("Curvy_ControlPointSize");
            EditorPrefs.DeleteKey("Curvy_OrientationLength");
            EditorPrefs.DeleteKey("Curvy_Gizmos");
            EditorPrefs.DeleteKey("Curvy_ToolbarLabels");
            EditorPrefs.DeleteKey("Curvy_ToolbarOrientation");
            EditorPrefs.DeleteKey("Curvy_ShowShapeWizardUndoWarning");
            EditorPrefs.DeleteKey("Curvy_KeyBindings");
        }

        #region Settings window

        /// <summary>
        /// The name of the settings entry of Curvy
        /// </summary>
        const string SettingsEntryName = "Curvy";

        /// <summary>
        /// The class used by Unity 2018.3 and newer to provide Curvy's preferences window
        /// </summary>
        public class CurvySettingsProvider : SettingsProvider
        {

            public CurvySettingsProvider(SettingsScope scopes, IEnumerable<string> keywords = null)
                : base(GetPreferencesPath(), scopes, keywords)
            { }

            public override void OnGUI(string searchContext)
            {
                PreferencesGUI();
            }

            /// <summary>
            /// The settings path for Curvy's Settings
            /// </summary>
            public static string GetPreferencesPath()
            {
                return "Preferences/" + SettingsEntryName;
            }
        }

        [SettingsProvider]
        static SettingsProvider MyNewPrefCode()
        {
            return new CurvySettingsProvider(SettingsScope.User);
        }

        public static void PreferencesGUI()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);
            CurvyGlobalManager.DefaultInterpolation = (CurvyInterpolation)EditorGUILayout.EnumPopup("Default Spline Type", CurvyGlobalManager.DefaultInterpolation);
            CurvyGlobalManager.SplineLayer = EditorGUILayout.LayerField(new GUIContent("Default Spline Layer", "Layer to use for splines and Control Points"), CurvyGlobalManager.SplineLayer);

            CurvyGlobalManager.SaveGeneratorOutputs = EditorGUILayout.Toggle(new GUIContent("Save Generator Outputs", "Whether the output of Curvy Generators should be saved in the scene file.\nDisable this option to reduce the size of scene files. This might increase the saving time for complex scenes.\nThis option applies only on generators that are enabled and have Auto Refresh set to true"), CurvyGlobalManager.SaveGeneratorOutputs);

            Instance.SnapValuePrecision = EditorGUILayout.Toggle(new GUIContent("Snap Value Precision", "Round inspector values"), Instance.SnapValuePrecision);

            CurvyGlobalManager.HideManager = EditorGUILayout.Toggle(new GUIContent("Hide _CurvyGlobal_", "Hide the global manager in Hierarchy?"), CurvyGlobalManager.HideManager);

            Instance.EnableAnnouncements = EditorGUILayout.Toggle(new GUIContent("Enable Announcements", "Display announcements from Curvy's developers"), Instance.EnableAnnouncements);

            Instance.EnableMetrics = EditorGUILayout.Toggle(new GUIContent("Enable Metrics", "Send metrics to Curvy's developers. This includes data such as the Unity version, Curvy version, etc... Keeping this enabled helps us a lot maintaining Curvy"), Instance.EnableMetrics);

            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.TextField(new GUIContent("Customization Root Path", "Base Path for custom Curvy extensions"), Instance.CustomizationRootPath);
                if (GUILayout.Button(new GUIContent("<", "Select"), GUILayout.ExpandWidth(false)))
                {
                    string path = EditorUtility.OpenFolderPanel("Customization Root Path", Application.dataPath, "");
                    if (!string.IsNullOrEmpty(path))
                        Instance.CustomizationRootPath = path.Replace(Application.dataPath + "/", "");
                }
                EditorGUILayout.EndHorizontal();
            }
            CurvyGlobalManager.SceneViewResolution = EditorGUILayout.Slider(new GUIContent("SceneView Resolution", "Lower values results in faster SceneView drawing"), CurvyGlobalManager.SceneViewResolution, 0, 1);

            foldouts[0] = EditorGUILayout.Foldout(foldouts[0], "Gizmo", CurvyStyles.Foldout);
            if (foldouts[0])
            {
                CurvyGlobalManager.DefaultGizmoColor = EditorGUILayout.ColorField("Spline color", CurvyGlobalManager.DefaultGizmoColor);
                CurvyGlobalManager.DefaultGizmoSelectionColor = EditorGUILayout.ColorField("Spline Selection color", CurvyGlobalManager.DefaultGizmoSelectionColor);
                CurvyGlobalManager.GizmoControlPointSize = EditorGUILayout.FloatField("Control Point Size", CurvyGlobalManager.GizmoControlPointSize);
                CurvyGlobalManager.GizmoOrientationLength = EditorGUILayout.FloatField(new GUIContent("Orientation Length", "Orientation gizmo size"), CurvyGlobalManager.GizmoOrientationLength);
                CurvyGlobalManager.GizmoOrientationColor = EditorGUILayout.ColorField(new GUIContent("Orientation Color", "Orientation gizmo color"), CurvyGlobalManager.GizmoOrientationColor);
                Instance.UseTiny2DHandles = EditorGUILayout.Toggle(new GUIContent("Use Tiny 2D Handles", "If enabled and the spline has Restrict To 2D enabled, dots are shown instead of the default move handles"), Instance.UseTiny2DHandles);
                Instance.AutoFadeLabels = EditorGUILayout.Toggle(new GUIContent("Auto Fade Labels", "Should the spline's text gizmos fade if the spline is too small on the screen"), Instance.AutoFadeLabels);
            }

            foldouts[1] = EditorGUILayout.Foldout(foldouts[1], "UI", CurvyStyles.Foldout);
            if (foldouts[1])
            {
                Instance.ShowGlobalToolbar = EditorGUILayout.Toggle(new GUIContent("Show Global Toolbar", "Always show Curvy Toolbar"), Instance.ShowGlobalToolbar);
                Instance.ToolbarMode = (DTToolbarMode)EditorGUILayout.EnumPopup(new GUIContent("Toolbar Labels", "Defines Toolbar Display Mode"), Instance.ToolbarMode);
                Instance.ToolbarOrientation = (DTToolbarOrientation)EditorGUILayout.EnumPopup(new GUIContent("Toolbar Orientation", "Defines Toolbar Position"), Instance.ToolbarOrientation);
                Instance.ShowHints = EditorGUILayout.Toggle(new GUIContent("Show Hints", "Show hints, at the bottom of scene view, about the usage of some Curvy editor tools"), Instance.ShowHints);
            }

            foldouts[2] = EditorGUILayout.Foldout(foldouts[2], "Shortcuts", CurvyStyles.Foldout);
            if (foldouts[2])
            {
                List<EditorKeyBinding> keys = Instance.GetProjectBindings();
                foreach (EditorKeyBinding binding in keys)
                {
                    if (binding.OnPreferencesGUI()) // save changed bindings
                    {
                        Instance.SetEditorPrefs(binding.Name, binding.ToPrefsString());
                    }
                    GUILayout.Space(2);
                    GUILayout.Box("", GUILayout.Height(1), GUILayout.ExpandWidth(true));
                    GUILayout.Space(2);
                }
            }
            if (GUILayout.Button("Reset to defaults"))
            {
                Instance.ResetPreferences();

                List<EditorKeyBinding> keys = Instance.GetProjectBindings();
                foreach (EditorKeyBinding binding in keys)
                    Instance.DeleteEditorPrefs(binding.Name);
            }

            EditorGUILayout.EndScrollView();

            if (GUI.changed)
            {
                Instance.SavePreferences();
                DT.ReInitialize(false);
            }

        }

        #endregion
    }

    /// <summary>
    /// Class for loading image resources
    /// </summary>
    public class CurvyResource : DTResource
    {
        static CurvyResource _Instance;
        public static CurvyResource Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new CurvyResource();
                return _Instance;
            }
        }

        public CurvyResource()
        {
            ResourceDLL = FindResourceDLL("CurvyEditorIcons");
            ResourceNamespace = "";//Assets.Curvy.Editor.Resources.";
        }

        private const string fallbackPackedString = "missing,16,16";

        public static Texture2D Load(string packedString)
        {
            Texture2D tex = Instance.LoadPacked(packedString);
            if (tex == null)
            {
                DTLog.LogError("Loading texture from packed string failed: " + packedString);
                return Instance.LoadPacked(fallbackPackedString);
            }

            return tex;
        }




    }
}