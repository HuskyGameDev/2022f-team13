// =====================================================================
// Copyright 2013-2022 ToolBuddy
// All rights reserved
// 
// http://www.toolbuddy.net
// =====================================================================

using UnityEngine;
using UnityEditor;
using System.Text;
using FluffyUnderware.CurvyEditor;
using FluffyUnderware.Curvy.Generator;
using FluffyUnderware.DevToolsEditor.Extensions;
using FluffyUnderware.Curvy.Utils;
using FluffyUnderware.DevToolsEditor;

namespace FluffyUnderware.CurvyEditor
{
    public static class CurvyStyles
    {
        public static GUIStyle GizmoText
        {
            get
            {
                if (gizmoText == null)
                {
                    gizmoText = new GUIStyle();
                    gizmoText.fixedHeight = 15;
                    gizmoText.alignment = TextAnchor.MiddleCenter;
                    //gizmoText.fixedWidth = label.Length * 10;
                    //gizmoText.padding = new RectOffset(0,0,0,0);
                    //gizmoText.margin = new RectOffset(0,0,0,0);
                    //gizmoText.border = new RectOffset(0,0,0,0);
                    //gizmoText.overflow = new RectOffset(0,0,0,0);
                    //gizmoText.contentOffset = Vector2.zero;
                    Texture2D backgroundTexture = new Texture2D(1, 1);
                    backgroundTexture.SetPixel(0, 0, new Color(1, 1, 1, 0.3f));
                    backgroundTexture.Apply();
                    backgroundTexture.hideFlags = HideFlags.DontSave;
                    gizmoText.normal.background = backgroundTexture;
                }
                return gizmoText;
            }
        }
        static GUIStyle gizmoText;

        public static GUIStyle ControllerCustomEventStyle
        {
            get
            {
                if (controllerCustomEventStyle == null)
                {
                    controllerCustomEventStyle = new GUIStyle();
                    controllerCustomEventStyle.fixedHeight = 15;
                    controllerCustomEventStyle.alignment = TextAnchor.MiddleCenter;
                    //gizmoText.fixedWidth = label.Length * 10;
                    //gizmoText.padding = new RectOffset(0,0,0,0);
                    //gizmoText.margin = new RectOffset(0,0,0,0);
                    //gizmoText.border = new RectOffset(0,0,0,0);
                    //gizmoText.overflow = new RectOffset(0,0,0,0);
                    //gizmoText.contentOffset = Vector2.zero;
                    Texture2D backgroundTexture = new Texture2D(1, 1);
                    backgroundTexture.SetPixel(0, 0, new Color(1, 1, 1, 0.3f));
                    backgroundTexture.Apply();
                    backgroundTexture.hideFlags = HideFlags.DontSave;
                    controllerCustomEventStyle.normal.background = backgroundTexture;
                }
                return controllerCustomEventStyle;
            }
        }

        static GUIStyle controllerCustomEventStyle;

        #region ### Buttons ###
        public static GUIStyle BorderlessButton
        {
            get
            {
                if (mBorderlessButton == null)
                {
                    mBorderlessButton = new GUIStyle(GUI.skin.label);
                    mBorderlessButton.padding = new RectOffset(-1, 3, -1, -1);
                    mBorderlessButton.imagePosition = ImagePosition.ImageOnly;
                }
                return mBorderlessButton;
            }
        }
        static GUIStyle mBorderlessButton;

        public static GUIStyle SmallButton
        {
            get
            {
                if (mSmallButton == null)
                {
                    mSmallButton = new GUIStyle(EditorStyles.miniButton);
                    mSmallButton.margin = new RectOffset(0, 0, 0, 0);
                    mSmallButton.padding = new RectOffset(1, 1, -1, -1);
                    //mSmallButton.imagePosition = ImagePosition.ImageOnly;
                }
                return mSmallButton;
            }
        }
        static GUIStyle mSmallButton;

        public static GUIStyle ImageButton
        {
            get
            {
                if (mImageButton == null)
                {
                    mImageButton = new GUIStyle(GUI.skin.button);
                    mImageButton.padding = new RectOffset(-1, -1, -1, -1);
                    mImageButton.imagePosition = ImagePosition.ImageOnly;
                }
                return mImageButton;
            }
        }
        static GUIStyle mImageButton;

        #endregion

        #region ### Misc ###

        public static GUIStyle Foldout
        {
            get
            {
                if (mFoldout == null)
                {
                    mFoldout = new GUIStyle(EditorStyles.foldout);
                    mFoldout.fontStyle = FontStyle.Bold;
                    mFoldout.margin.top += 2;
                    mFoldout.margin.bottom += 4;

                }
                return mFoldout;
            }
        }
        static GUIStyle mFoldout;

        public static GUIStyle HelpBox
        {
            get
            {
                if (mHelpBox == null)
                {
                    mHelpBox = new GUIStyle(GUI.skin.GetStyle("HelpBox"));
                    mHelpBox.richText = true;
                }
                return mHelpBox;
            }
        }
        static GUIStyle mHelpBox;

        public static GUIStyle Toolbar
        {
            get
            {
                if (mToolbar == null)
                {
                    mToolbar = new GUIStyle(EditorStyles.toolbar);
                    mToolbar.fixedHeight = 0;
                    mToolbar.padding = new RectOffset(6, 6, 4, 4);
                }
                return mToolbar;
            }
        }
        public static GUIStyle mToolbar;

        public static GUIStyle RoundRectangle
        {
            get
            {
                if (mRoundRectangle == null)
                {
                    mRoundRectangle = new GUIStyle();
                    mRoundRectangle.normal.background = CurvyResource.Load("roundrectangle,16,16");
                    mRoundRectangle.border = new RectOffset(6, 6, 6, 6);
                    mRoundRectangle.overflow = new RectOffset(1, 0, 0, 1);
                }
                return mRoundRectangle;
            }
        }
        static GUIStyle mRoundRectangle;

        public static GUIStyle ToolbarItem
        {
            get
            {
                if (mToolbarItem == null)
                {
                    mToolbarItem = new GUIStyle(GUI.skin.button);
                    mToolbarItem.alignment = TextAnchor.MiddleLeft;
                    mToolbarItem.padding.top = 4;
                    mToolbarItem.padding.bottom = 2;
                }
                return mToolbarItem;
            }
        }
        static GUIStyle mToolbarItem;

        #endregion

        #region ### CG Colors ###

        public static Color IOnRequestProcessingTitleColor
        {
            get
            {
                if (EditorGUIUtility.isProSkin) return new Color(0.2f, 0.7f, 0.2f);
                else return new Color(0.1f, 0.5f, 0.1f);
            }
        }

        public static Color IOnRequestProcessingSlotColor
        {
            get
            {
                if (EditorGUIUtility.isProSkin) return new Color(0.2f, 0.7f, 0.2f);
                else return new Color(0.1f, 0.5f, 0.1f);
            }
        }

        #endregion

        #region ### CG Module Window ###
        public static int ModuleWindowTitleHeight = 26;

        public static GUIStyle ModuleWindow
        {
            get
            {
                if (mModuleWindow == null)
                {
                    mModuleWindow = new GUIStyle(GUI.skin.window);
                    mModuleWindow.normal.background = TexModuleWindow;
                    mModuleWindow.onNormal.background = TexModuleWindow1;
                    mModuleWindow.border = new RectOffset(10, 12, 24, 13);
                    mModuleWindow.padding = new RectOffset(0, 0, 24, 6);
                    mModuleWindow.contentOffset = new Vector2(0, -18);
#pragma warning disable 162
                    mModuleWindow.overflow = new RectOffset(10, 11, 8, 11);
#pragma warning restore 162
                    mModuleWindow.richText = true;
                }
                return mModuleWindow;
            }
        }
        static GUIStyle mModuleWindow;

        public static GUIStyle ModuleWindowSlotBackground
        {
            get
            {
                if (mModuleWindowSlotBackground == null)
                {
                    mModuleWindowSlotBackground = new GUIStyle(GUI.skin.box);
                    mModuleWindowSlotBackground.padding = new RectOffset(1, 1, 1, 1);
                    mModuleWindowSlotBackground.margin = new RectOffset(1, 1, 0, 0);
                }
                return mModuleWindowSlotBackground;
            }
        }
        static GUIStyle mModuleWindowSlotBackground;

        public static GUIStyle ModuleWindowBackground
        {
            get
            {
                if (mModuleWindowBackground == null)
                {
                    mModuleWindowBackground = new GUIStyle(GUI.skin.box);
                    mModuleWindowBackground.padding = new RectOffset(1, 1, 1, 1);
                    mModuleWindowBackground.margin = new RectOffset(1, 1, 5, 0);

                }
                return mModuleWindowBackground;
            }
        }
        static GUIStyle mModuleWindowBackground;


        public static Texture2D HelpTexture
        {
            get
            {
                if (mHelpTexture == null)
                    // mHelpTexture=(Texture2D)EditorGUIUtility.Load("icons/_Help.png");
                    // mHelpTexture = CurvyResource.Load("help12,12,12");
                    mHelpTexture = CurvyResource.Load(GetTextureFilename("help12", 12, 12));
                return mHelpTexture;
            }
        }
        static Texture2D mHelpTexture;

        public static Texture2D EditTexture
        {
            get
            {
                if (mEditTexture == null)
                    // mEditTexture = CurvyResource.Load("editsmall,12,12");
                    mEditTexture = CurvyResource.Load(GetTextureFilename("editsmall", 12, 12));
                return mEditTexture;
            }
        }
        static Texture2D mEditTexture;


        public static GUIStyle GlowBox
        {
            get
            {
                if (mGlowBox == null)
                {
                    mGlowBox = new GUIStyle();
                    mGlowBox.normal.background = CurvyResource.Load("glowbox,26,26");
                    mGlowBox.border = new RectOffset(11, 11, 11, 11);
                    mGlowBox.overflow = new RectOffset(1, 0, 0, 1);
                }
                return mGlowBox;
            }
        }
        static GUIStyle mGlowBox;


        public static GUIStyle ShowDetailsButton
        {
            get
            {
                if (showDetailsButton == null)
                {
                    showDetailsButton = new GUIStyle(EditorStyles.toolbarButton);
                    showDetailsButton.margin.left = 2;
                    showDetailsButton.margin.right = 1;
                }
                return showDetailsButton;
            }
        }
        static GUIStyle showDetailsButton;

        #endregion

        #region ### CG Slots ###

        public static GUIStyle Slot
        {
            get
            {
                if (mSlot == null)
                {
                    mSlot = new GUIStyle();
                    mSlot.normal.background = EditorGUIUtility.whiteTexture;
                    mSlot.fixedHeight = 17;
                    mSlot.fixedWidth = 17;
                    mSlot.normal.textColor = new Color(0, 0, 0, 0.6f);
                    mSlot.alignment = TextAnchor.MiddleCenter;
                    mSlot.contentOffset = new Vector2(-1f, -1f);
                }
                return mSlot;
            }
        }
        static GUIStyle mSlot;

        public static GUIStyle GetSlotLabelStyle(CGModuleSlot slot)
        {
            GUIStyle st = new GUIStyle();
            st.fixedHeight = 18;
            // Linked Slots => Bold
            // OnRequestProcessing => Green
            // Optional => Italic
            st.fontStyle = (slot.IsLinked) ? FontStyle.Bold : FontStyle.Normal;
            st.normal.textColor = new Color(1, 1, 1, 0.6f).SkinAwareColor();

            CGModuleInputSlot inputSlot = (slot as CGModuleInputSlot);

            if (inputSlot)
            {
                InputSlotInfo myInfo = inputSlot.InputInfo;
                if (myInfo.Optional)
                {
                    st.fontStyle = (st.fontStyle == FontStyle.Bold) ? st.fontStyle = FontStyle.BoldAndItalic : FontStyle.Italic;
                }
                if (inputSlot.Module is IOnRequestProcessing || myInfo.RequestDataOnly)
                    st.normal.textColor = IOnRequestProcessingSlotColor.SkinAwareColor();
                st.alignment = TextAnchor.MiddleLeft;
                st.margin.left = 2;
            }
            else
            {
                if (slot.Module is IOnRequestProcessing)
                    st.normal.textColor = IOnRequestProcessingSlotColor.SkinAwareColor();
                st.alignment = TextAnchor.MiddleRight;
                st.margin.right = 2;

            }

            st.padding.bottom = 3;

            return st;
        }



        #endregion

        #region ### Textures ###

        public static string GetTextureFilename(string name, int width, int height, string darkskinPostfix = "_dark", string ligthskinPostfix = "_light")
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(name);
            sb.Append(EditorGUIUtility.isProSkin ? darkskinPostfix : ligthskinPostfix);
            sb.Append(",");
            sb.Append(width);
            sb.Append(",");
            sb.Append(height);

            string filename = sb.ToString();
            return filename;
        }

        public static Texture2D TexModuleWindow
        {
            get
            {
                if (mTexModuleWindow == null)
                {
                    mTexModuleWindow = CurvyResource.Load((EditorGUIUtility.isProSkin) ? "cgwindowdark,64,64" : "cgwindowlight,64,64");
                    //mTexModuleWindow.hideFlags = HideFlags.DontSave;
                }
                return mTexModuleWindow;
            }
        }
        static Texture2D mTexModuleWindow;

        public static Texture2D TexModuleWindow1
        {
            get
            {
                if (mTexModuleWindow1 == null)
                {
                    mTexModuleWindow1 = CurvyResource.Load((EditorGUIUtility.isProSkin) ? "cgwindowdark1,64,64" : "cgwindowlight1,64,64");
                    //mTexModuleWindow1.hideFlags = HideFlags.DontSave;
                }
                return mTexModuleWindow1;
            }
        }
        static Texture2D mTexModuleWindow1;

        public static Texture2D TexGridSnap
        {
            get
            {
                if (mTexGridSnap == null)
                {
                    mTexGridSnap = CurvyResource.Load(GetTextureFilename("cggridstep", 16, 16));
                    //mTexGridSnap.hideFlags = HideFlags.DontSave;
                }
                return mTexGridSnap;
            }
        }
        static Texture2D mTexGridSnap;

        public static Texture2D TexPlay
        {
            get
            {
                if (mTexPlay == null)
                {
                    mTexPlay = CurvyResource.Load("play,24,24");
                    //mTexPlay.hideFlags=HideFlags.DontSave;
                }
                return mTexPlay;
            }
        }
        static Texture2D mTexPlay;

        public static Texture2D TexStop
        {
            get
            {
                if (mTexStop == null)
                {
                    mTexStop = CurvyResource.Load("stop,24,24");
                    //mTexStop.hideFlags=HideFlags.DontSave;
                }
                return mTexStop;
            }
        }
        static Texture2D mTexStop;

        public static Texture2D TexLogoBig
        {
            get
            {
                if (mTexLogoBig == null)
                {
                    // mTexLogoBig = CurvyResource.Load( (EditorGUIUtility.isProSkin) ? "curvylogo_light,436,160" : "curvylogo_dark,436,160");
                    mTexLogoBig = CurvyResource.Load(GetTextureFilename("curvylogo", 436, 160));
                    //mTexLogoBig.hideFlags = HideFlags.DontSave;
                }
                return mTexLogoBig;
            }
        }
        static Texture2D mTexLogoBig;

        public static Texture2D TexLogoSmall
        {
            get
            {
                if (mTexLogoSmall == null)
                {
                    mTexLogoSmall = CurvyResource.Load(GetTextureFilename("curvylogo_small", 178, 124));
                }
                return mTexLogoSmall;
            }
        }
        static Texture2D mTexLogoSmall;

        public static Texture2D TexConnection
        {
            get
            {
                if (mTexConnection == null)
                {
                    // mTexConnection = CurvyResource.Load("connection,24,24");
                    mTexConnection = CurvyResource.Load(GetTextureFilename("connection", 24, 24));
                    //mTexConnection.hideFlags = HideFlags.DontSave;
                }
                return mTexConnection;
            }
        }
        static Texture2D mTexConnection;

        public static Texture2D TexConnectionPos
        {
            get
            {
                if (mTexConnectionPos == null)
                {
                    // mTexConnectionPos = CurvyResource.Load("connectionpos,24,24");
                    mTexConnectionPos = CurvyResource.Load(GetTextureFilename("connectionpos", 24, 24));
                    //mTexConnectionPos.hideFlags = HideFlags.DontSave;
                }
                return mTexConnectionPos;
            }
        }
        static Texture2D mTexConnectionPos;

        public static Texture2D TexConnectionRot
        {
            get
            {
                if (mTexConnectionRot == null)
                {
                    // mTexConnectionRot = CurvyResource.Load("connectionrot,24,24");
                    mTexConnectionRot = CurvyResource.Load(GetTextureFilename("connectionrot", 24, 24));
                    //mTexConnectionRot.hideFlags = HideFlags.DontSave;
                }
                return mTexConnectionRot;
            }
        }
        static Texture2D mTexConnectionRot;

        public static Texture2D TexConnectionFull
        {
            get
            {
                if (mTexConnectionFull == null)
                {
                    // mTexConnectionFull = CurvyResource.Load("connectionfull,24,24");
                    mTexConnectionFull = CurvyResource.Load(GetTextureFilename("connectionfull", 24, 24));
                    //mTexConnectionFull.hideFlags = HideFlags.DontSave;
                }
                return mTexConnectionFull;
            }
        }
        static Texture2D mTexConnectionFull;

        public static Texture2D HierarchyConnectionTexture
        {
            get
            {
                if (mHierarchyConnectionTexture == null)
                {
                    mHierarchyConnectionTexture = CurvyResource.Load("connectionsmall,12,12");
                    // mHierarchyConnectionTexture = CurvyResource.Load (GetTextureFilename ("connectionsmall", 12, 12));
                    //mHierarchyConnectionTexture.hideFlags = HideFlags.DontSave;
                }
                return mHierarchyConnectionTexture;
            }
        }
        static Texture2D mHierarchyConnectionTexture;

        public static Texture2D RndSeedTexture
        {
            get
            {
                if (mRndSeedTexture == null)
                {
                    mRndSeedTexture = CurvyResource.Load("rndseed,12,12");
                    // mRndSeedTexture = CurvyResource.Load (GetTextureFilename ("rndseed", 12, 12));
                    // mRndSeedTexture.hideFlags = HideFlags.DontSave;
                }
                return mRndSeedTexture;
            }
        }
        static Texture2D mRndSeedTexture;

        public static Texture2D DeleteTexture
        {
            get
            {
                if (mDeleteTexture == null)
                {
                    // mDeleteTexture = CurvyResource.Load("delete16,16,16");
                    mDeleteTexture = CurvyResource.Load(GetTextureFilename("delete16", 16, 16));
                    //mDeleteTexture.hideFlags = HideFlags.DontSave;
                }
                return mDeleteTexture;
            }
        }
        static Texture2D mDeleteTexture;

        public static Texture2D DeleteBTexture
        {
            get
            {
                if (deleteBTexture == null)
                {
                    // mDeleteTexture = CurvyResource.Load("delete16,16,16");
                    deleteBTexture = CurvyResource.Load(GetTextureFilename("deleteB16", 16, 16));
                    //mDeleteTexture.hideFlags = HideFlags.DontSave;
                }
                return deleteBTexture;
            }
        }
        static Texture2D deleteBTexture;

        public static Texture2D SaveResourcesTexture
        {
            get
            {
                if (saveResourcesTexture == null)
                {
                    saveResourcesTexture = CurvyResource.Load(GetTextureFilename("save_resources", 16, 16));
                }
                return saveResourcesTexture;
            }
        }
        static Texture2D saveResourcesTexture;

        public static Texture2D RefreshTexture
        {
            get
            {
                if (mRefreshTexture == null)
                {
                    // mRefreshTexture = CurvyResource.Load("reload,16,16");
                    mRefreshTexture = CurvyResource.Load(GetTextureFilename("reload", 16, 16));
                    //mRefreshTexture.hideFlags = HideFlags.DontSave;
                }
                return mRefreshTexture;
            }
        }
        static Texture2D mRefreshTexture;

        public static Texture2D ReorderTexture
        {
            get
            {
                if (mReorderTexture == null)
                {
                    mReorderTexture = CurvyResource.Load(GetTextureFilename("reorder", 16, 16));
                }
                return mReorderTexture;
            }
        }
        static Texture2D mReorderTexture;

        public static Texture2D CGAutoFoldTexture
        {
            get
            {
                if (mCGAutoFoldTexture == null)
                {
                    // mCGAutoFoldTexture = CurvyResource.Load("autofold,16,16");
                    mCGAutoFoldTexture = CurvyResource.Load(GetTextureFilename("autofold", 16, 16));
                    //mCGAutoFoldTexture.hideFlags = HideFlags.DontSave;
                }
                return mCGAutoFoldTexture;
            }
        }
        static Texture2D mCGAutoFoldTexture;

        public static Texture2D AddTemplateTexture
        {
            get
            {
                if (mAddTemplateTexture == null)
                {
                    // mAddTemplateTexture = CurvyResource.Load("addCGTemplate,16,16");
                    mAddTemplateTexture = CurvyResource.Load(GetTextureFilename("addCGTemplate", 16, 16));
                    //mAddTemplateTexture.hideFlags = HideFlags.DontSave;
                }
                return mAddTemplateTexture;
            }
        }
        static Texture2D mAddTemplateTexture;

        public static Texture2D DebugTexture
        {
            get
            {
                if (mDebugTexture == null)
                {
                    // mDebugTexture = CurvyResource.Load("debug,16,16");
                    mDebugTexture = CurvyResource.Load(GetTextureFilename("debug", 16, 16));
                    //mDebugTexture.hideFlags = HideFlags.DontSave;
                }
                return mDebugTexture;
            }
        }
        static Texture2D mDebugTexture;

        public static Texture2D DebugSceneViewTexture
        {
            get
            {
                return (EditorGUIUtility.isProSkin) ?
                    EditorGUIUtility.IconContent("d_UnityEditor.SceneView").image as Texture2D :
                    EditorGUIUtility.IconContent("UnityEditor.SceneView").image as Texture2D;
            }
        }

        public static Texture2D LineTexture
        {
            get
            {
                if (mLineTexture == null)
                {
                    mLineTexture = new Texture2D(1, 2);
                    Color c = Color.white;//.SkinAwareColor();
                    Color ca = new Color(c.r, c.g, c.b, 0);
                    mLineTexture.SetPixels(new Color[] { ca, c });
                    //mLineTexture.hideFlags = HideFlags.DontSave;
                    mLineTexture.Apply();
                }
                return mLineTexture;
            }
        }
        static Texture2D mLineTexture;

        public static Texture2D RequestLineTexture
        {
            get
            {
                if (mRequestLineTexture == null)
                {
                    mRequestLineTexture = new Texture2D(2, 2);
                    Color c = Color.white;
                    Color ca = new Color(c.r, c.g, c.b, 0);
                    mRequestLineTexture.SetPixels(new Color[] { ca, Color.black, c, Color.black });
                    //mRequestLineTexture.hideFlags = HideFlags.DontSave;
                    mRequestLineTexture.Apply();
                }
                return mRequestLineTexture;
            }
        }
        static Texture2D mRequestLineTexture;

        public static Texture2D InspectorTexture
        {
            get
            {
                return EditorGUIUtility.IconContent("d_UnityEditor.InspectorWindow").image as Texture2D;
            }
        }

        public static Texture2D ExpandTexture
        {
            get
            {
                if (mExpandTexture == null)
                {
                    // mExpandTexture = CurvyResource.Load("expand16,16,16");
                    mExpandTexture = CurvyResource.Load(GetTextureFilename("expand16", 16, 16));
                    //mExpandTexture.hideFlags = HideFlags.DontSave;
                }
                return mExpandTexture;
            }
        }
        static Texture2D mExpandTexture;

        public static Texture2D SynchronizeTexture
        {
            get
            {
                if (mSynchronizeTexture == null)
                {
                    // mSynchronizeTexture = CurvyResource.Load("synchronize,16,16");
                    mSynchronizeTexture = CurvyResource.Load(GetTextureFilename("synchronize", 16, 16));
                    //mSynchronizeTexture.hideFlags = HideFlags.DontSave;
                }
                return mSynchronizeTexture;
            }
        }
        static Texture2D mSynchronizeTexture;

        public static Texture2D CollapseTexture
        {
            get
            {
                if (mCollapseTexture == null)
                {
                    // mCollapseTexture = CurvyResource.Load("collapse16,16,16");
                    mCollapseTexture = CurvyResource.Load(GetTextureFilename("collapse16", 16, 16));
                    //mCollapseTexture.hideFlags = HideFlags.DontSave;
                }
                return mCollapseTexture;
            }
        }
        static Texture2D mCollapseTexture;

        public static Texture2D OpenGraphTexture
        {
            get
            {
                if (mOpenGraphTexture == null)
                {
                    // mOpenGraphTexture = CurvyResource.Load("opengraph,24,24");
                    mOpenGraphTexture = CurvyResource.Load(GetTextureFilename("opengraph", 24, 24));
                    //mOpenGraphTexture.hideFlags = HideFlags.DontSave;
                }
                return mOpenGraphTexture;
            }
        }
        static Texture2D mOpenGraphTexture;

        public static Texture2D DeleteSmallTexture
        {
            get
            {
                if (mDeleteSmallTexture == null)
                {
                    // mDeleteSmallTexture = CurvyResource.Load ("deletesmall,12,12");
                    mDeleteSmallTexture = CurvyResource.Load(GetTextureFilename("deletesmall", 12, 12));
                    //mDeleteSmallTexture.hideFlags = HideFlags.DontSave;
                }
                return mDeleteSmallTexture;
            }
        }
        static Texture2D mDeleteSmallTexture;

        public static Texture2D ClearSmallTexture
        {
            get
            {
                if (mClearSmallTexture == null)
                {
                    // mClearSmallTexture = CurvyResource.Load ("clearsmall,12,12");
                    mClearSmallTexture = CurvyResource.Load(GetTextureFilename("clearsmall", 12, 12));
                    //mClearSmallTexture.hideFlags = HideFlags.DontSave;
                }
                return mClearSmallTexture;
            }
        }
        static Texture2D mClearSmallTexture;

        public static Texture2D SelectTexture
        {
            get
            {
                if (mSelectTexture == null)
                {
                    // mSelectTexture = CurvyResource.Load("selectsmall,12,12");
                    mSelectTexture = CurvyResource.Load(GetTextureFilename("selectsmall", 12, 12));
                    //mSelectTexture.hideFlags = HideFlags.DontSave;
                }
                return mSelectTexture;
            }
        }
        static Texture2D mSelectTexture;

        public static Texture2D AddSmallTexture
        {
            get
            {
                if (mAddSmallTexture == null)
                {
                    // mAddSmallTexture = CurvyResource.Load("addsmall,12,12");
                    mAddSmallTexture = CurvyResource.Load(GetTextureFilename("addsmall", 12, 12));
                    // mAddSmallTexture.hideFlags = HideFlags.DontSave;
                }
                return mAddSmallTexture;
            }
        }
        static Texture2D mAddSmallTexture;

        #region --- Toolbar Icons ---



        public static Texture2D IconPrefs
        {
            get
            {
                if (mIconPrefs == null)
                {
                    mIconPrefs = CurvyResource.Load("prefs,24,24");
                    mIconPrefs.hideFlags = HideFlags.DontSave;
                }
                return mIconPrefs;
            }
        }
        static Texture2D mIconPrefs;

        public static Texture2D IconAbout
        {
            get
            {
                if (mIconAbout == null)
                {
                    mIconAbout = CurvyResource.Load("about,24,24");
                    mIconAbout.hideFlags = HideFlags.DontSave;
                }
                return mIconAbout;
            }
        }
        static Texture2D mIconAbout;

        public static Texture2D IconAsmdef
        {
            get
            {
                if (mIconAsmdef == null)
                {
                    mIconAsmdef = CurvyResource.Load("asmdef,24,24");
                    mIconAsmdef.hideFlags = HideFlags.DontSave;
                }
                return mIconAsmdef;
            }
        }
        static Texture2D mIconAsmdef;

        public static Texture2D IconHelp
        {
            get
            {
                if (mIconHelp == null)
                {
                    mIconHelp = CurvyResource.Load("help,24,24");
                    mIconHelp.hideFlags = HideFlags.DontSave;
                }
                return mIconHelp;
            }
        }
        static Texture2D mIconHelp;

        public static Texture2D IconWWW
        {
            get
            {
                if (mIconWWW == null)
                {
                    mIconWWW = CurvyResource.Load("web,24,24");
                    mIconWWW.hideFlags = HideFlags.DontSave;
                }
                return mIconWWW;
            }
        }
        static Texture2D mIconWWW;

        public static Texture2D IconBugReporter
        {
            get
            {
                if (mIconBugReporter == null)
                {
                    mIconBugReporter = CurvyResource.Load("bugreport,24,24");
                    mIconBugReporter.hideFlags = HideFlags.DontSave;
                }
                return mIconBugReporter;
            }
        }
        static Texture2D mIconBugReporter;

        public static Texture2D IconNewShape
        {
            get
            {
                if (mIconNewShape == null)
                {
                    mIconNewShape = CurvyResource.Load("shapewizard,24,24");
                    mIconNewShape.hideFlags = HideFlags.DontSave;
                }
                return mIconNewShape;
            }
        }
        static Texture2D mIconNewShape;

        public static Texture2D IconNewGroup
        {
            get
            {
                if (mIconNewGroup == null)
                {
                    mIconNewGroup = CurvyResource.Load("group,24,24");
                    mIconNewGroup.hideFlags = HideFlags.DontSave;
                }
                return mIconNewGroup;
            }
        }
        static Texture2D mIconNewGroup;

        public static Texture2D IconNewCG
        {
            get
            {
                if (mIconNewCG == null)
                {
                    mIconNewCG = CurvyResource.Load(GetTextureFilename("opengraph", 24, 24));
                    mIconNewCG.hideFlags = HideFlags.DontSave;
                }
                return mIconNewCG;
            }
        }
        static Texture2D mIconNewCG;


        public static Texture2D IconCP
        {
            get
            {
                if (mIconCP == null)
                {
                    mIconCP = CurvyResource.Load("singlecp,24,24");
                    mIconCP.hideFlags = HideFlags.DontSave;
                }
                return mIconCP;
            }
        }
        static Texture2D mIconCP;

        public static Texture2D IconCPOff
        {
            get
            {
                if (mIconCPOff == null)
                {
                    mIconCPOff = CurvyResource.Load("singlecp_off,24,24");
                    mIconCPOff.hideFlags = HideFlags.DontSave;
                }
                return mIconCPOff;
            }
        }
        static Texture2D mIconCPOff;

        public static Texture2D IconRaycast
        {
            get
            {
                if (mIconRaycast == null)
                {
                    mIconRaycast = CurvyResource.Load("raycast,24,24");
                    mIconRaycast.hideFlags = HideFlags.DontSave;
                }
                return mIconRaycast;
            }
        }
        static Texture2D mIconRaycast;

        public static Texture2D IconRaycastOff
        {
            get
            {
                if (mIconRaycastOff == null)
                {
                    mIconRaycastOff = CurvyResource.Load("raycast_off,24,24");
                    mIconRaycastOff.hideFlags = HideFlags.DontSave;
                }
                return mIconRaycastOff;
            }
        }
        static Texture2D mIconRaycastOff;

        public static Texture2D IconSubdivide
        {
            get
            {
                if (mIconSubdivide == null)
                {
                    mIconSubdivide = CurvyResource.Load("subdivide,24,24");
                    mIconSubdivide.hideFlags = HideFlags.DontSave;
                }
                return mIconSubdivide;
            }
        }
        static Texture2D mIconSubdivide;

        public static Texture2D IconSimplify
        {
            get
            {
                if (mIconSimplify == null)
                {
                    mIconSimplify = CurvyResource.Load("simplify,24,24");
                    mIconSimplify.hideFlags = HideFlags.DontSave;
                }
                return mIconSimplify;
            }
        }
        static Texture2D mIconSimplify;

        public static Texture2D IconEqualize
        {
            get
            {
                if (mIconEqualize == null)
                {
                    mIconEqualize = CurvyResource.Load("equalize,24,24");
                    mIconEqualize.hideFlags = HideFlags.DontSave;
                }
                return mIconEqualize;
            }
        }
        static Texture2D mIconEqualize;

        public static Texture2D IconMeshExport
        {
            get
            {
                if (mIconMeshExport == null)
                {
                    mIconMeshExport = CurvyResource.Load("exportmesh,24,24");
                    mIconMeshExport.hideFlags = HideFlags.DontSave;
                }
                return mIconMeshExport;
            }
        }
        static Texture2D mIconMeshExport;

        public static Texture2D IconSyncFromHierarchy
        {
            get
            {
                if (mIconSyncFromHierarchy == null)
                {
                    mIconSyncFromHierarchy = CurvyResource.Load("syncfromhierarchy,24,24");
                    mIconSyncFromHierarchy.hideFlags = HideFlags.DontSave;
                }
                return mIconSyncFromHierarchy;
            }
        }
        static Texture2D mIconSyncFromHierarchy;

        public static Texture2D IconSelectContainingConnections
        {
            get
            {
                if (mIconSelectContainingConnections == null)
                {
                    mIconSelectContainingConnections = CurvyResource.Load("containingcon,24,24");
                    mIconSelectContainingConnections.hideFlags = HideFlags.DontSave;
                }
                return mIconSelectContainingConnections;
            }
        }
        static Texture2D mIconSelectContainingConnections;

        public static Texture2D IconAxisXY
        {
            get
            {
                if (mIconAxisXY == null)
                {
                    mIconAxisXY = CurvyResource.Load("axisxy,24,24");
                    mIconAxisXY.hideFlags = HideFlags.DontSave;
                }
                return mIconAxisXY;
            }
        }
        static Texture2D mIconAxisXY;

        public static Texture2D IconAxisXZ
        {
            get
            {
                if (mIconAxisXZ == null)
                {
                    mIconAxisXZ = CurvyResource.Load("axisxz,24,24");
                    mIconAxisXZ.hideFlags = HideFlags.DontSave;
                }
                return mIconAxisXZ;
            }
        }
        static Texture2D mIconAxisXZ;

        public static Texture2D IconAxisYZ
        {
            get
            {
                if (mIconAxisYZ == null)
                {
                    mIconAxisYZ = CurvyResource.Load("axisyz,24,24");
                    mIconAxisYZ.hideFlags = HideFlags.DontSave;
                }
                return mIconAxisYZ;
            }
        }
        static Texture2D mIconAxisYZ;

        public static Texture2D IconView
        {
            get
            {
                if (iconView == null)
                {
                    iconView = CurvyResource.Load("view,24,24");
                    iconView.hideFlags = HideFlags.DontSave;
                }
                return iconView;
            }
        }
        static Texture2D iconView;

        #endregion

        #endregion
    }
}