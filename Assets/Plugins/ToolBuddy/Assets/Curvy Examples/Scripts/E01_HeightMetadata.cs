// =====================================================================
// Copyright 2013-2022 ToolBuddy
// All rights reserved
// 
// http://www.toolbuddy.net
// =====================================================================

using UnityEngine;
using FluffyUnderware.DevTools;

namespace FluffyUnderware.Curvy.Examples
{
    public class E01_HeightMetadata : CurvyInterpolatableMetadataBase<float>
    {
        [SerializeField]
        [RangeEx(0, 1, Slider = true)]
#pragma warning disable 649
        float m_Height;
#pragma warning restore 649

        public override float MetaDataValue
        {
            get { return m_Height; }
        }

        public override float Interpolate(CurvyInterpolatableMetadataBase<float> nextMetadata, float interpolationTime)
        {
            return (nextMetadata != null) ? Mathf.Lerp(MetaDataValue, nextMetadata.MetaDataValue, interpolationTime) : MetaDataValue;
        }
    }
}