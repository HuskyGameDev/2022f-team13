// =====================================================================
// Copyright 2013-2022 ToolBuddy
// All rights reserved
// 
// http://www.toolbuddy.net
// =====================================================================

using System;
using UnityEngine;
using System.Collections;
using FluffyUnderware.DevTools;

namespace FluffyUnderware.Curvy
{

    /// <summary>
    /// Curvy Generator options Metadata class
    /// </summary>
    [HelpURL(CurvySpline.DOCLINK + "metacgoptions")]
    public class MetaCGOptions : CurvyMetadataBase
    {

        #region ### Serialized Fields ###

        [Positive]
        [SerializeField]
        private int m_MaterialID = DefaultMaterialId;


        [SerializeField]
        [FieldCondition(nameof(ShowUvEdgeOrHardEdge), true)]
        private bool m_HardEdge;
        [Positive(Tooltip = "Max step distance when using optimization")]
        [SerializeField]
        private float m_MaxStepDistance;
        [Section("Extended UV", HelpURL = CurvySpline.DOCLINK + "metacgoptions_extendeduv")]
        [FieldCondition(nameof(ShowUvEdgeOrHardEdge), true)]
        [SerializeField]
        private bool m_UVEdge;

        [Positive]
        [FieldCondition(nameof(showExplicitU), true)]
        [SerializeField]
        private bool m_ExplicitU;
        [FieldCondition(nameof(showFirstU), true)]
        [FieldAction("CBSetFirstU")]
        [Positive]
        [SerializeField]
        private float m_FirstU;
        [FieldCondition(nameof(showSecondU), true)]
        [Positive]
        [SerializeField]
        private float m_SecondU;

        /// <summary>
        /// Whether  or not the conversion of the UVEdge value to the new "system" (starting from Curvy 8) was done. See the commentary on the private method EnsureUVEdgeUpdate to know more.
        /// </summary>
        [SerializeField, HideInInspector] private bool uVEdgeUpdated = false;

        #endregion

        #region ### Public Properties ###

        /// <summary>
        /// Gets or sets Material ID
        /// </summary>
        public int MaterialID
        {
            get
            {
                return m_MaterialID;
            }
            set
            {
                int v = Mathf.Max(0, value);
                if (m_MaterialID != v)
                {
                    m_MaterialID = v;
                    NotifyModification();
                }
            }
        }

        /// <summary>
        /// Gets or sets whether to create a hard edge or not
        /// This is the raw serialized value. In opposition, <see cref="CorrectedHardEdge"/> takes other considerations into account
        /// </summary>
        public bool HardEdge
        {
            get { return m_HardEdge; }
            set
            {
                if (m_HardEdge != value)
                {
                    m_HardEdge = value;
                    NotifyModification();
                }
            }
        }

        /// <summary>
        /// <see cref="HardEdge"/> is ignored for first and last CPs of an open spline. This method takes that into consideration. In opposition, <see cref="HardEdge"/> is the raw serialized value.
        /// </summary>
        /// <value></value>
        public bool CorrectedHardEdge
        {
            get
            {
                //this one is to handle the case of a cp (of an open spline) that was in the middle of the spline, and has HardEdge, then we delete all its following cps, so it becomes the last cp. This means it has HardEdge to true, but the value is ignored
                return CanHaveUvEdgeOrHadrdEdge() && HardEdge;
            }
        }

        /// <summary>
        /// Gets or sets whether to create an UV edge or not
        /// This is the raw serialized value. In opposition, <see cref="CorrectedUVEdge"/> takes other considerations into account
        /// </summary>
        public bool UVEdge
        {
            get { return m_UVEdge; }
            set
            {
                if (m_UVEdge != value)
                {
                    m_UVEdge = value;
                    NotifyModification();
                }
            }
        }

        /// <summary>
        /// <see cref="UVEdge"/> is ignored for first and last CPs of an open spline. This method takes that into consideration. In opposition, <see cref="UVEdge"/> is the raw serialized value.
        /// </summary>
        /// <value></value>
        public bool CorrectedUVEdge
        {
            get
            {
                //this one is to handle the case of a cp (of an open spline) that was in the middle of the spline, and has UVEdge, then we delete all its following cps, so it becomes the last cp. This means it has UVEdge to true, but the value is ignored
                return CanHaveUvEdgeOrHadrdEdge() && UVEdge;
            }
        }

        /// <summary>
        /// Gets or sets whether to define explicit U values
        /// </summary>
        public bool ExplicitU
        {
            get { return m_ExplicitU; }
            set
            {
                if (m_ExplicitU != value)
                {
                    m_ExplicitU = value;
                    NotifyModification();
                }
            }
        }

        /// <summary>
        /// Gets or sets UV0
        /// </summary>
        public float FirstU
        {
            get { return m_FirstU; }
            set
            {
                if (m_FirstU != value)
                {
                    m_FirstU = value;
                    NotifyModification();
                }
            }
        }

        /// <summary>
        /// Gets or sets UV0
        /// </summary>
        public float SecondU
        {
            get { return m_SecondU; }
            set
            {
                if (m_SecondU != value)
                {
                    m_SecondU = value;
                    NotifyModification();
                }
            }
        }

        /// <summary>
        /// Gets or sets maximum vertex distance when using optimization (0=infinite)
        /// </summary>
        public float MaxStepDistance
        {
            get
            {
                return m_MaxStepDistance;
            }
            set
            {
                float v = Mathf.Max(0, value);
                if (m_MaxStepDistance != v)
                {
                    m_MaxStepDistance = v;
                    NotifyModification();
                }
            }
        }

        public bool HasDifferentMaterial
        {
            get
            {
                MetaCGOptions previousMetaCGOptions = GetPreviousData<MetaCGOptions>(false);
                int previousMaterialId = previousMetaCGOptions == null ? DefaultMaterialId : previousMetaCGOptions.MaterialID;
                return previousMaterialId != MaterialID;
            }
        }

        #endregion

        #region ### Private Fields & Properties ###

        private const int DefaultMaterialId = 0;

        private bool ShowUvEdgeOrHardEdge
        {
            get
            {
                return ControlPoint && CanHaveUvEdgeOrHadrdEdge();
            }
        }

        private bool showExplicitU
        {
            get
            {
                return (ControlPoint && !showSecondU);
            }
        }

        private bool showFirstU
        {
            get
            {
                return ExplicitU || CorrectedUVEdge;
            }
        }

        private bool showSecondU
        {
            get
            {
                return CorrectedUVEdge;
            }
        }

        #endregion

        #region ### Unity Callbacks ###
        /*! \cond UNITY */


#if UNITY_EDITOR
        private void OnValidate()
        {
            NotifyModification();
        }

        protected override void Awake()
        {
            base.Awake();
            EnsureUVEdgeUpdate();
        }

#endif

        public void Reset()
        {
            MaterialID = DefaultMaterialId;
            HardEdge = false;
            MaxStepDistance = 0;
            UVEdge = false;
            ExplicitU = false;
            FirstU = 0;
            SecondU = 0;
        }

        /*! \endcond */
        #endregion

        #region ### Public Methods ###

        public float GetDefinedFirstU(float defaultValue)
        {
            return (CorrectedUVEdge || ExplicitU) ? FirstU : defaultValue;
        }

        public float GetDefinedSecondU(float defaultValue)
        {
            return (CorrectedUVEdge) ? SecondU : GetDefinedFirstU(defaultValue);
        }

        #endregion

        #region ### Privates ###
        /*! \cond PRIVATES */

        /// <summary>
        ///Until Curvy 7 included, a change in material id (compared to the one of the previous CP) was automatically considered to be an UVEdge, so in the case users wanted to just change a material id, they had to figure out the right value of first and second U, which is annoying at best. True, they had the option to deactivate Extended UV in the Shape Extrusion module, but that's not an option if you want to use extended UV on CPs other than the one with a material change.
        ///Starting from Curvy 8, the Extended UV option in the Shape Extrusion module is not visible anymore (which solves another problem: people confused about why the UV options they enter are not taken into consideration). The module will act as if Extended UV is always true. With Extended UV not available, a solution had to be implemented to allow for material id change that does not modify the UV. The solution found is to dissociate a material id change from UVEdge. Now you can have both true, false, or having different values.
        ///To keep things backward compatible, I am setting UVEdge to true when I detect a material id change while both U values are different from their default value of 0. This is done when an instance is processed for the first time under Curvy 8. This is not perfect, since you can still have someone who purposefully set both U values to 0.
        /// </summary>
        private void EnsureUVEdgeUpdate()
        {
            if (uVEdgeUpdated == false)
            {
                m_UVEdge = m_UVEdge || (HasDifferentMaterial && false == (FirstU == 0 && SecondU == 0));
                uVEdgeUpdated = true;
            }
        }

        private bool CanHaveUvEdgeOrHadrdEdge()
        {
            return Spline.Closed || (Spline.FirstVisibleControlPoint != ControlPoint && Spline.LastVisibleControlPoint != ControlPoint);
        }

        /*! \endcond */
        #endregion
    }
}