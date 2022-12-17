// =====================================================================
// Copyright 2013-2017 Fluffy Underware
// All rights reserved
// 
// http://www.fluffyunderware.com
// =====================================================================

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using FluffyUnderware.DevTools.Extensions;
using System;

namespace FluffyUnderware.DevToolsEditor
{
#if UNITY_2021_2_OR_NEWER
    [Obsolete("Now that SceneView has a OnSceneGUI method, there is seemingly no need to use DTSceneView which implements its own OnSceneGUI. It might even conflict with SceneView.OnSceneGUI")]
#endif
    public class DTSceneView : SceneView
    {

        #region ### Serialized fields ###
        #endregion

        #region ### Public Properties ###

        public bool In2DMode
        {
            get { return in2DMode; }
            set
            {
                in2DMode = value;
            }
        }

        public SceneViewState State
        {
            get
            {
                return mStateField.GetValue(this) as SceneViewState;
            }
            set
            {
                mStateField.SetValue(this, value);
            }
        }

        #endregion

        #region ### Privates Fields ###

        FieldInfo mStateField;

        #endregion

        #region ### Unity Callbacks ###
        /*! \cond UNITY */

        public override void OnEnable()
        {
            base.OnEnable();
            getInternals();
            SceneView.duringSceneGui += onScene;
        }

        public override void OnDisable()
        {
            SceneView.duringSceneGui -= onScene;
            base.OnDisable();
        }

        /*! \endcond */
        #endregion

        #region ### Public Static Methods ###
        #endregion

        #region ### Public Methods ###

#if UNITY_2021_2_OR_NEWER
        protected new virtual void OnSceneGUI()
#else
        protected virtual void OnSceneGUI()
#endif
        {
        }

        #endregion

        #region ### Privates & Internals ###
        /*! \cond PRIVATE */

        void onScene(SceneView view)
        {
            if (EditorApplication.isCompiling)
            {
                SceneView.duringSceneGui -= onScene;
                Close();
                GUIUtility.ExitGUI();
            }
            if (view == this)
                OnSceneGUI();
        }

        void getInternals()
        {
            mStateField = GetType().FieldByName("m_SceneViewState", true, true);
        }



        /*! \endcond */
        #endregion

    }
}
