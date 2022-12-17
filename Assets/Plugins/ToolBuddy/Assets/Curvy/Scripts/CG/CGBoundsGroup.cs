// =====================================================================
// Copyright 2013-2022 ToolBuddy
// All rights reserved
// 
// http://www.toolbuddy.net
// =====================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using FluffyUnderware.DevTools;
using UnityEngine;

namespace FluffyUnderware.Curvy.Generator
{
    /// <summary>
    /// Helper class used by VolumeSpots and others
    /// </summary>
    [System.Serializable]
    public class CGBoundsGroup : CGWeightedItem
    {
        /// <summary>
        /// How the rotation axes are defined related to the Volume's data
        /// </summary>
        public enum RotationModeEnum
        {
            /// <summary>
            /// Use Volume's direction and orientation
            /// </summary>
            Full,
            /// <summary>
            /// Use Volume's direction only
            /// </summary>
            Direction,
            /// <summary>
            /// Use Volume's direction only after projecting it on XZ plane
            /// </summary>
            Horizontal,
            /// <summary>
            /// Do not use Volume's data
            /// </summary>
            Independent
        }

        #region ### Serialized Fields ###
        [SerializeField] private string m_Name;
        [SerializeField]
        [Tooltip("When checked, the group will only be placed when all the group's items can be placed in the space left")]
        private bool m_KeepTogether;
        [SerializeField]
        [FloatRegion(RegionIsOptional = true, Options = AttributeOptionsFlags.Compact)]
        private FloatRegion m_SpaceBefore = new FloatRegion() { SimpleValue = true };
        [SerializeField]
        [FloatRegion(RegionIsOptional = true, Options = AttributeOptionsFlags.Compact)]
        private FloatRegion m_SpaceAfter = new FloatRegion() { SimpleValue = true };
        [SerializeField]
        //BUG Tooltip and FloatRegion are not compatible [Tooltip("Shifts the Cross origin for this group by a value in the defined range")]
        [FloatRegion(RegionIsOptional = true, RegionOptionsPropertyName = nameof(PositionRangeOptions), UseSlider = true, Precision = 3)]
        private FloatRegion m_CrossBase = new FloatRegion(0);
        [SerializeField]
        [Tooltip("If ticked, the Cross origin for this group will not take into consideration the Cross parameters in the General tab")]
        private bool m_IgnoreModuleCrossBase = false;

        [SerializeField]
        [Tooltip("When enabled, items will be selected randomly")]
        private bool m_RandomizeItems;

        [IntRegion(UseSlider = false, RegionOptionsPropertyName = nameof(RepeatingGroupsOptions), Options = AttributeOptionsFlags.Compact)]
        [SerializeField]
        [Tooltip("The randomized items are the the ones that have their indices inside this range")]
        private IntRegion m_RepeatingItems;

        //Translation
        [SerializeField]
        [Tooltip("If unchecked, translation will be done in the global/world space")]
        private bool m_RelativeTranslation = true;
        [SerializeField]
        [FloatRegion(RegionIsOptional = true, Options = AttributeOptionsFlags.Compact)]
        private FloatRegion m_TranslationX = new FloatRegion(0);
        [SerializeField]
        [FloatRegion(RegionIsOptional = true, Options = AttributeOptionsFlags.Compact)]
        private FloatRegion m_TranslationY = new FloatRegion(0);
        [SerializeField]
        [FloatRegion(RegionIsOptional = true, Options = AttributeOptionsFlags.Compact)]
        private FloatRegion m_TranslationZ = new FloatRegion(0);

        //Rotation
        [SerializeField]
        [Tooltip("How the rotation axes are defined related to the Volume's data\r\n  - Full : Use Volume's direction and orientation\r\n  - Direction : Use Volume's direction only\r\n  - Horizontal : Use Volume's direction only after projecting it on XZ plane\r\n  - Independent : Do not use Volume's data")]
        private RotationModeEnum m_RotationMode = RotationModeEnum.Full;

        [SerializeField]
        [FloatRegion(RegionIsOptional = true, Options = AttributeOptionsFlags.Compact)]
        private FloatRegion m_RotationX = new FloatRegion(0);
        [SerializeField]
        [FloatRegion(RegionIsOptional = true, Options = AttributeOptionsFlags.Compact)]
        private FloatRegion m_RotationY = new FloatRegion(0);
        [SerializeField]
        [FloatRegion(RegionIsOptional = true, Options = AttributeOptionsFlags.Compact)]
        private FloatRegion m_RotationZ = new FloatRegion(0);

        //Scale
        [SerializeField]
        [Tooltip("Whether the scaling is applied equally on all dimensions")]
        private bool m_UniformScaling = true;
        [SerializeField]
        [FloatRegion(RegionIsOptional = true, Options = AttributeOptionsFlags.Compact)]
        private FloatRegion m_ScaleX = new FloatRegion(1);
        [SerializeField]
        [FloatRegion(RegionIsOptional = true, Options = AttributeOptionsFlags.Compact)]
        private FloatRegion m_ScaleY = new FloatRegion(1);
        [SerializeField]
        [FloatRegion(RegionIsOptional = true, Options = AttributeOptionsFlags.Compact)]
        private FloatRegion m_ScaleZ = new FloatRegion(1);

        [SerializeField] private List<CGBoundsGroupItem> m_Items = new List<CGBoundsGroupItem>();

        #endregion

        #region ### Public Members ###

        public string Name
        {
            get { return m_Name; }
            set
            {
                if (m_Name != value)
                    m_Name = value;
            }
        }

        /// <summary>
        /// When true, the group will only be placed when all items can be placed in the space left. 
        /// </summary>
        public bool KeepTogether
        {
            get { return m_KeepTogether; }
            set
            {
                if (m_KeepTogether != value)
                    m_KeepTogether = value;
            }
        }

        public FloatRegion SpaceBefore
        {
            get { return m_SpaceBefore; }
            set
            {
                if (m_SpaceBefore != value)
                    m_SpaceBefore = value;
            }
        }

        public FloatRegion SpaceAfter
        {
            get { return m_SpaceAfter; }
            set
            {
                if (m_SpaceAfter != value)
                    m_SpaceAfter = value;
            }
        }

        /// <summary>
        /// When enabled, items in groups will be selected randomly.
        /// <seealso cref="RepeatingItems"/>
        /// </summary>
        public bool RandomizeItems
        {
            get { return m_RandomizeItems; }
            set
            {
                if (m_RandomizeItems != value)
                    m_RandomizeItems = value;
            }
        }

        /// <summary>
        /// When <seealso cref="RandomizeItems"/> is set to true, the randomized items are the the ones that have their indices inside the RepeatingItems range
        /// </summary>
        public IntRegion RepeatingItems
        {
            get { return m_RepeatingItems; }
            set
            {
                if (m_RepeatingItems != value)
                    m_RepeatingItems = value;
            }
        }

        /// <summary>
        /// Shifts the Cross origin for this group by a value in the defined range
        /// </summary>
        public FloatRegion CrossBase
        {
            get { return m_CrossBase; }
            set
            {
                if (m_CrossBase != value)
                    m_CrossBase = value;
            }
        }

        /// <summary>
        /// If true, the Cross origin for this group will not take into consideration the Cross parameters of the BuildVolumeSpots class/>
        /// </summary>
        public bool IgnoreModuleCrossBase
        {
            get { return m_IgnoreModuleCrossBase; }
            set
            {
                if (m_IgnoreModuleCrossBase != value)
                    m_IgnoreModuleCrossBase = value;
            }
        }

        /// <summary>
        /// How the rotation axes are defined related to the Volume's data
        /// </summary>
        public RotationModeEnum RotationMode
        {
            get { return m_RotationMode; }
            set
            {
                if (m_RotationMode != value)
                    m_RotationMode = value;
            }
        }

        public FloatRegion RotationX
        {
            get { return m_RotationX; }
            set
            {
                if (m_RotationX != value)
                    m_RotationX = value;
            }
        }
        public FloatRegion RotationY
        {
            get { return m_RotationY; }
            set
            {
                if (m_RotationY != value)
                    m_RotationY = value;
            }
        }
        public FloatRegion RotationZ
        {
            get { return m_RotationZ; }
            set
            {
                if (m_RotationZ != value)
                    m_RotationZ = value;
            }
        }

        /// <summary>
        /// When true, the scaling vector is (ScaleX, ScaleX, ScaleX) instead of (ScaleX, ScaleY, ScaleZ)
        /// </summary>
        public bool UniformScaling
        {
            get { return m_UniformScaling; }
            set
            {
                if (m_UniformScaling != value)
                    m_UniformScaling = value;
            }
        }
        public FloatRegion ScaleX
        {
            get { return m_ScaleX; }
            set
            {
                if (m_ScaleX != value)
                    m_ScaleX = value;
            }
        }
        public FloatRegion ScaleY
        {
            get { return m_ScaleY; }
            set
            {
                if (m_ScaleY != value)
                    m_ScaleY = value;
            }
        }
        public FloatRegion ScaleZ
        {
            get { return m_ScaleZ; }
            set
            {
                if (m_ScaleZ != value)
                    m_ScaleZ = value;
            }
        }
        /// <summary>
        /// When true, the translation of an item is done in the relative frame defined by the tangent and orientation (up vector) of the volume at the item's position
        /// </summary>
        public bool RelativeTranslation
        {
            get { return m_RelativeTranslation; }
            set
            {
                if (m_RelativeTranslation != value)
                    m_RelativeTranslation = value;
            }
        }
        public FloatRegion TranslationX
        {
            get { return m_TranslationX; }
            set
            {
                if (m_TranslationX != value)
                    m_TranslationX = value;
            }
        }
        public FloatRegion TranslationY
        {
            get { return m_TranslationY; }
            set
            {
                if (m_TranslationY != value)
                    m_TranslationY = value;
            }
        }
        public FloatRegion TranslationZ
        {
            get { return m_TranslationZ; }
            set
            {
                if (m_TranslationZ != value)
                    m_TranslationZ = value;
            }
        }

        public List<CGBoundsGroupItem> Items
        {
            get { return m_Items; }
        }

        /// <summary>
        /// First index of the <see cref="RepeatingItems"/> range
        /// </summary>
        public int FirstRepeating
        {
            get { return m_RepeatingItems.From; }
            set
            {
                int v = Mathf.Clamp(value, 0, Mathf.Max(0, ItemCount - 1));
                if (m_RepeatingItems.From != v)
                    m_RepeatingItems.From = v;
            }
        }

        /// <summary>
        /// Last index of the <see cref="RepeatingItems"/> range
        /// </summary>
        public int LastRepeating
        {
            get { return m_RepeatingItems.To; }
            set
            {
                int v = Mathf.Clamp(value, FirstRepeating, Mathf.Max(0, ItemCount - 1));
                if (m_RepeatingItems.To != v)
                    m_RepeatingItems.To = v;
            }
        }

        public int ItemCount
        {
            get { return Items.Count; }
        }

        public CGBoundsGroup(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Fill an item bag with items based on their weights
        /// </summary>
        public static void FillItemBag(WeightedRandom<int> bag, IEnumerable<CGWeightedItem> itemsWeights, int firstItem, int lastItem)
        {
            for (int g = firstItem; g <= lastItem; g++)
                bag.Add(g, (int)(itemsWeights.ElementAt(g).Weight * 10));

            if (bag.Size == 0)
                bag.Add(firstItem, 1);
        }

        #endregion

        #region Non Public Members

        private RegionOptions<int> RepeatingGroupsOptions
        {
            get
            {
                return RegionOptions<int>.MinMax(0, Mathf.Max(0, ItemCount - 1));
            }
        }

        private RegionOptions<float> PositionRangeOptions
        {
            get
            {
                return RegionOptions<float>.MinMax(-1f, 1f);
            }
        }
        #endregion

        #region Obsolete code kept for retrocompatibility
        [Obsolete("Enum no more used by Curvy. This enum is kept for retro compatibility reasons")]
        private enum DistributionModeEnum
        {
            Parent,
            Self
        }

#pragma warning disable 649
        [SerializeField, HideInInspector]
        [Obsolete("Use IgnoreModuleCrossBase instead. This field is kept for retro compatibility reasons")]
        private DistributionModeEnum m_DistributionMode;

        [SerializeField, HideInInspector]
        [Obsolete("Use CrossBase instead. This field is kept for retro compatibility reasons")]
        [FloatRegion(RegionIsOptional = true, RegionOptionsPropertyName = nameof(PositionRangeOptions), UseSlider = true, Precision = 3)]
        private FloatRegion m_PositionOffset = new FloatRegion(0);

        [SerializeField, HideInInspector]
        [Obsolete("Use TranslationY instead, while setting RelativeTranslation to true. This field is kept for retro compatibility reasons")]
        [FloatRegion(RegionIsOptional = true, Options = AttributeOptionsFlags.Compact)]
        private FloatRegion m_Height = new FloatRegion(0);

        [SerializeField, HideInInspector]
        [Obsolete("Use RandomizeItems instead. This field is kept for retro compatibility reasons")]
        private CurvyRepeatingOrderEnum m_RepeatingOrder = CurvyRepeatingOrderEnum.Row;

        [SerializeField, HideInInspector]
        [Obsolete("Use RotationX, RotationY and RotationZ instead. This field is kept for retro compatibility reasons")]
        [VectorEx]
        private Vector3 m_RotationOffset;

        [SerializeField, HideInInspector]
        [Obsolete("Use RotationX, RotationY and RotationZ instead. This field is kept for retro compatibility reasons")]
        [VectorEx]
        private Vector3 m_RotationScatter;
#pragma warning restore 649

        /// <summary>
        /// Converts the obsolete data to the new format
        /// </summary>
        [Obsolete("Method will get removed once the obsolete data will get removed")]
        public void ConvertObsoleteData()
        {
            RandomizeItems = m_RepeatingOrder == CurvyRepeatingOrderEnum.Random;
            IgnoreModuleCrossBase = m_DistributionMode == DistributionModeEnum.Self;
            CrossBase = m_PositionOffset;
            if (m_Height.From != 0f || (m_Height.SimpleValue == false && m_Height.To != 0f))
            {
                TranslationY = m_Height;
                RelativeTranslation = true;
            }

            {
                float from = m_RotationOffset.x - m_RotationScatter.x;
                float to = m_RotationOffset.x + m_RotationScatter.x;
                RotationX = from == to ? new FloatRegion(from) : new FloatRegion(from, to);
            }

            {
                float from = m_RotationOffset.y - m_RotationScatter.y;
                float to = m_RotationOffset.y + m_RotationScatter.y;
                RotationY = from == to ? new FloatRegion(from) : new FloatRegion(from, to);
            }

            {
                float from = m_RotationOffset.z - m_RotationScatter.z;
                float to = m_RotationOffset.z + m_RotationScatter.z;
                RotationZ = from == to ? new FloatRegion(from) : new FloatRegion(from, to);
            }
        }
        #endregion
    }
}