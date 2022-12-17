// =====================================================================
// Copyright 2013-2022 ToolBuddy
// All rights reserved
// 
// http://www.toolbuddy.net
// =====================================================================

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FluffyUnderware.DevTools;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace FluffyUnderware.Curvy.Generator.Modules
{
    [ModuleInfo("Input/Transform Spots", ModuleName = "Input Transform Spots", Description = "Defines an array of placement spots taken from existing Transforms")]
    [HelpURL(CurvySpline.DOCLINK + "cginputtransformspots")]
    public class InputTransformSpots : CGModule
    {
        [HideInInspector]
        [OutputSlotInfo(typeof(CGSpots))]
        public CGModuleOutputSlot OutSpots = new CGModuleOutputSlot();

        #region ### Serialized Fields ###

        [ArrayEx]
        [SerializeField]
        private List<TransformSpot> transformSpots = new List<TransformSpot>();

        #endregion

        #region ### Public Properties ###

        /// <summary>
        /// The input <see cref="TransformSpots"/>
        /// </summary>
        public List<TransformSpot> TransformSpots
        {
            get { return transformSpots; }
            set
            {
                if (transformSpots != value)
                    transformSpots = value;
                Dirty = true;
            }
        }

        #endregion

        #region ### Unity Callbacks ###
        /*! \cond UNITY */

        protected override void OnEnable()
        {
            base.OnEnable();
            Properties.MinWidth = 250;
#if UNITY_EDITOR
            EditorApplication.update += EditorUpdate;
#endif
        }

        protected override void OnDisable()
        {
            base.OnDisable();
#if UNITY_EDITOR
            EditorApplication.update -= EditorUpdate;
#endif
        }

        public override void Reset()
        {
            base.Reset();
            TransformSpots.Clear();
            Dirty = true;
        }

        private void Update()
        {
            if (Dirty == false && OutSpots.Data != null && OutSpots.Data.Length != 0)
            {
                foreach (var keyValuePair in outputToInputDictionary)
                {
                    CGSpot cgSpot = keyValuePair.Key;
                    TransformSpot transformSpot = keyValuePair.Value;
                    if (cgSpot.Position != transformSpot.Transform.position)
                    {
                        Dirty = true;
                        return;
                    }
                }
            }
        }

        /*! \endcond */
        #endregion

        #region ### Public Methods ###

        public override void Refresh()
        {
            base.Refresh();

            if (OutSpots.IsLinked)
            {
                outputToInputDictionary.Clear();

                List<CGSpot> spots = TransformSpots.Where(s => s.Transform != null).Select(s =>
                {
                    CGSpot cgSpot = new CGSpot(s.Index, s.Transform.position, s.Transform.rotation, s.Transform.lossyScale);
                    outputToInputDictionary[cgSpot] = s;
                    return cgSpot;
                }).ToList();

                OutSpots.SetData(new CGSpots(spots));
            }

#if UNITY_EDITOR
            if (TransformSpots.Exists(s => s.Transform == null))
                UIMessages.Add("Missing Transform input");
#endif
        }

        #endregion

        #region ### Privates ###
        /*! \cond PRIVATE */

        private readonly Dictionary<CGSpot, TransformSpot> outputToInputDictionary = new Dictionary<CGSpot, TransformSpot>();

#if UNITY_EDITOR
        private void EditorUpdate()
        {
            if (Application.isPlaying == false)
                Update();
        }

#endif
        /*! \endcond */
        #endregion

        /// <summary>
        /// Similar to <see cref="CGSpot"/>, but instead of having a constant position/rotation/scale, it is taken from a Transform
        /// </summary>
        [System.Serializable]
        public struct TransformSpot : IEquatable<TransformSpot>
        {
            [SerializeField]
#pragma warning disable 649 //field is modified through InputTransformSpotsEditor, through Unity's serialization API
            private int index;
#pragma warning restore 649

            [SerializeField]
#pragma warning disable 649 //field is modified through InputTransformSpotsEditor, through Unity's serialization API
            private Transform transform;
#pragma warning restore 649

            /// <summary>
            /// The index of the object to place
            /// </summary>
            public int Index => index;

            /// <summary>
            /// The Transform from which the spot's position/rotation/scale should be taken
            /// </summary>
            public Transform Transform => transform;

            public bool Equals(TransformSpot other)
            {
                return index == other.index && Equals(transform, other.transform);
            }

            public override bool Equals(object obj)
            {
                return obj is TransformSpot other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (index * 397) ^ (transform != null ? transform.GetHashCode() : 0);
                }
            }

            public static bool operator ==(TransformSpot left, TransformSpot right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(TransformSpot left, TransformSpot right)
            {
                return !left.Equals(right);
            }
        }
    }
}