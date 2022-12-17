// =====================================================================
// Copyright 2013-2022 ToolBuddy
// All rights reserved
// 
// http://www.toolbuddy.net
// =====================================================================

using FluffyUnderware.DevTools;
using System;
using System.Threading.Tasks;
using FluffyUnderware.DevTools.Extensions;
using UnityEngine;
using UnityEngine.Assertions;

namespace FluffyUnderware.Curvy.Generator.Modules
{
    /// <summary>
    /// Translates a path relatively to it's direction, instead of relatively to the world as does the TRS Path module.
    /// </summary>
    [ModuleInfo("Modifier/Path Relative Translation", ModuleName = "Path Relative Translation", Description = "Translates a path relatively to it's direction, instead of relatively to the world as does the TRS Path module.")]
    [HelpURL(CurvySpline.DOCLINK + "cgpathrelativetranslation")]
#pragma warning disable 618
    public class ModifierPathRelativeTranslation : CGModule, IOnRequestProcessing, IPathProvider
#pragma warning restore 618
    {
        [HideInInspector]
        [InputSlotInfo(typeof(CGPath), Name = "Path A", ModifiesData = true)]
        public CGModuleInputSlot InPath = new CGModuleInputSlot();

        [HideInInspector]
        [OutputSlotInfo(typeof(CGPath))]
        public CGModuleOutputSlot OutPath = new CGModuleOutputSlot();


        #region ### Serialized Fields ###

        /// <summary>
        /// The translation distance
        /// </summary>
        [SerializeField]
        [Label("Translation")]
        [Tooltip("The (base) translation distance")]
        private float lateralTranslation;

        [SerializeField]
        [Tooltip("Defines translation multiplier, depending on the Relative Distance (between 0 and 1) of a point on the path")]
        [AnimationCurveEx("    Multiplier")]
        private AnimationCurve multiplier = AnimationCurve.Linear(0, 1, 1, 1);

        /// <summary>
        /// The translation angle, in degrees
        /// </summary>
        [SerializeField]
        [Tooltip("The translation angle, in degrees")]
        private float angle;

        #endregion

        #region ### Public Properties ###

        /// <summary>
        /// The translation distance
        /// </summary>
        public float LateralTranslation
        {
            get { return lateralTranslation; }
            set
            {
                if (lateralTranslation != value)
                {
                    lateralTranslation = value;
                    Dirty = true;
                }
            }
        }

        /// <summary>
        /// The translation angle, in degrees
        /// </summary>
        public float Angle
        {
            get { return angle; }
            set
            {

                if (Single.IsNaN(value))
                {
#if CURVY_SANITY_CHECKS
                    DTLog.LogWarning($"[Curvy] Invalid Angle value: {value}. 0 will be assigned instead", this);
#endif
                    value = 0;
                }

                if (angle != value)
                {
                    angle = value;
                    Dirty = true;
                }
            }
        }

        /// <summary>
        /// Defines a translation multiplier relatively to the Relative Distance of a point on the path.
        /// </summary>
        /// <remarks>You will need to set this module's Dirty to true yourself if you modify the AnimationCurve without setting a new one</remarks>
        public AnimationCurve Multiplier
        {
            get { return multiplier; }
            set
            {
                if (multiplier != value)
                    multiplier = value;
                Dirty = true;
            }
        }

        public bool PathIsClosed
        {
            get
            {
                return (IsConfigured) && InPath.SourceSlot().PathProvider.PathIsClosed;
            }
        }

        #endregion

        #region ### IOnRequestProcessing ###

        public CGData[] OnSlotDataRequest(CGModuleInputSlot requestedBy, CGModuleOutputSlot requestedSlot, params CGDataRequestParameter[] requests)
        {
            CGData[] result;
            if (requestedSlot == OutPath)
            {
                CGPath data = InPath.GetData<CGPath>(out bool isDisposable, requests);
#if CURVY_SANITY_CHECKS
                // I forgot why I added this assertion, but I trust my past self
                Assert.IsTrue(data == null || isDisposable);
#endif
                if (data)
                {
                    bool evaluateTranslationMultiplier = Multiplier.ValueIsOne() == false;

                    if (evaluateTranslationMultiplier)
                        //no parallelization if we are going to evaluate TranslationMultiplier, since evaluating Animation Curves is not thread safe 
                        for (int i = 0; i < data.Count; i++)
                            TranslatePoint(i, data, evaluateTranslationMultiplier, 
                                lateralTranslation, multiplier, angle);
                    else
                        Parallel.For(0,
                            data.Count,
                            i => TranslatePoint(i, data, evaluateTranslationMultiplier, 
                                lateralTranslation, multiplier, angle)
                            );

                    data.Recalculate();
                }

                //TODO after fixing Directions computation in ConformPath, do the same here

                result = new CGData[1] { data };
            }
            else
                result = null;

            return result;
        }

        private static void TranslatePoint(int index, CGPath data, bool evaluateTranslationMultiplier, float translation, AnimationCurve translationMultiplier, float angle)
        {

            float translationMagnitude;
            {
                if (evaluateTranslationMultiplier)
                    translationMagnitude = translation * translationMultiplier.Evaluate(data.RelativeDistances.Array[index]);
                else
                    translationMagnitude = translation;
            }

            Vector3 translationVector;
            {
                Vector3 direction = data.Directions.Array[index];
                Vector3 normal = data.Normals.Array[index];

                if (angle != 0f)
                    translationVector = Quaternion.AngleAxis(angle, direction) * Vector3.Cross(normal, direction) * translationMagnitude;
                else
                    translationVector = Vector3.Cross(normal, direction) * translationMagnitude;
            }

            Vector3[] positions = data.Positions.Array;
            positions[index].x = positions[index].x + translationVector.x;
            positions[index].y = positions[index].y + translationVector.y;
            positions[index].z = positions[index].z + translationVector.z;
        }

        #endregion


        #region ### Unity Callbacks ###
        /*! \cond UNITY */

        protected override void OnEnable()
        {
            base.OnEnable();
            Properties.MinWidth = 250;
            Properties.LabelWidth = 165;
        }

        public override void Reset()
        {
            base.Reset();
            LateralTranslation = 0;
            Angle = 0;
            Multiplier = AnimationCurve.Linear(0, 1, 1, 1);
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            LateralTranslation = lateralTranslation;
            Angle = angle;
            Multiplier = multiplier;
            Dirty = true;
        }
#endif

        /*! \endcond */
        #endregion
    }
}