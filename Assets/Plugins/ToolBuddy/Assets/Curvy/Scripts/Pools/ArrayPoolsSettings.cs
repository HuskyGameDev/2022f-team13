// =====================================================================
// Copyright 2013-2022 ToolBuddy
// All rights reserved
// 
// http://www.toolbuddy.net
// =====================================================================

using System;
using FluffyUnderware.DevTools;
using ToolBuddy.Pooling.Pools;
using UnityEngine;

namespace FluffyUnderware.Curvy.Pools
{
    /// <summary>
    /// A component that allows setting, via the editor, the settings of the used <see cref="ToolBuddy.Pooling.Pools.ArrayPool{T}"/>s
    /// </summary>
    [HelpURL(DTUtility.HelpUrlBase + "arraypoolsettings")]
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class ArrayPoolsSettings : DTVersionedMonoBehaviour
    {
        [SerializeField]
        [Tooltip("The maximal number of elements of type Vector2 allowed to be stored in the arrays' pool waiting to be reused")]
        private long vector2Capacity = 100_000;

        [SerializeField]
        [Tooltip("The maximal number of elements of type Vector3 allowed to be stored in the arrays' pool waiting to be reused")]
        private long vector3Capacity = 100_000;

        [SerializeField]
        [Tooltip("The maximal number of elements of type Vector4 allowed to be stored in the arrays' pool waiting to be reused")]
        private long vector4Capacity = 100_000;

        [SerializeField]
        [Tooltip("The maximal number of elements of type Int32 allowed to be stored in the arrays' pool waiting to be reused")]
        private long intCapacity = 100_000;

        [SerializeField]
        [Tooltip("The maximal number of elements of type Single (a.k.a float) allowed to be stored in the arrays' pool waiting to be reused")]
        private long floatCapacity = 10_000;

        [SerializeField]
        [Tooltip("The maximal number of elements of type CGSpots allowed to be stored in the arrays' pool waiting to be reused")]
        private long cgSpotCapacity = 10_000;
        
        [Tooltip("Log in the console each time an array pool allocates a new array in memory")]
        [SerializeField]
        private bool logAllocations = false;

        /// <summary>
        /// The maximal number of elements of type Vector2 allowed to be stored in the arrays' pool waiting to be reused
        /// </summary>
        public long Vector2Capacity
        {
            get { return vector2Capacity; }
            set
            {
                vector2Capacity = Math.Max(0, value);
                ArrayPools.Vector2.ElementsCapacity = vector2Capacity;

            }
        }

        /// <summary>
        /// The maximal number of elements of type Vector3 allowed to be stored in the arrays' pool waiting to be reused
        /// </summary>
        public long Vector3Capacity
        {
            get { return vector3Capacity; }
            set
            {
                vector3Capacity = Math.Max(0, value);
                ArrayPools.Vector3.ElementsCapacity = vector3Capacity;
            }
        }

        /// <summary>
        /// The maximal number of elements of type Vector4 allowed to be stored in the arrays' pool waiting to be reused
        /// </summary>
        public long Vector4Capacity
        {
            get { return vector4Capacity; }
            set
            {
                vector4Capacity = Math.Max(0, value);
                ArrayPools.Vector4.ElementsCapacity = vector4Capacity;
            }
        }

        /// <summary>
        /// The maximal number of elements of type Int32 allowed to be stored in the arrays' pool waiting to be reused
        /// </summary>
        public long IntCapacity
        {
            get { return intCapacity; }
            set
            {
                intCapacity = Math.Max(0, value);
                ArrayPools.Int32.ElementsCapacity = IntCapacity;
            }
        }

        /// <summary>
        /// The maximal number of elements of type Single allowed to be stored in the arrays' pool waiting to be reused
        /// </summary>
        public long FloatCapacity
        {
            get { return floatCapacity; }
            set
            {
                floatCapacity = Math.Max(0, value);
                ArrayPools.Single.ElementsCapacity = floatCapacity;
            }
        }

        /// <summary>
        /// The maximal number of elements of type CGSpot allowed to be stored in the arrays' pool waiting to be reused
        /// </summary>
        public long CGSpotCapacity
        {
            get { return cgSpotCapacity; }
            set
            {
                cgSpotCapacity = Math.Max(0, value);
                ArrayPools.CGSpot.ElementsCapacity = cgSpotCapacity;
            }
        }
        /// <summary>
        /// Log in the console each time an array pool allocates a new array in memory
        /// </summary>
        public bool LogAllocations
        {
            get { return logAllocations; }
            set
            {
                logAllocations = value;
                ArrayPools.CGSpot.LogAllocations = logAllocations;
                ArrayPools.Int32.LogAllocations = logAllocations;
                ArrayPools.Single.LogAllocations = logAllocations;
                ArrayPools.Vector2.LogAllocations = logAllocations;
                ArrayPools.Vector3.LogAllocations = logAllocations;
                ArrayPools.Vector4.LogAllocations = logAllocations;
            }
        }

        private void Reset() => ValidateAndApply();

        private void OnValidate() => ValidateAndApply();

        private void Awake() => ValidateAndApply();

        private void OnEnable()
        {
#if UNITY_EDITOR
            if (FindObjectsOfType<ArrayPoolsSettings>().Length > 1)
                DTLog.LogWarning("[Curvy] More than one instance of 'Array Pools Settings' detected. You should keep only one instance of this script.", this);
#endif
            ValidateAndApply();
        }

        private void Start() => ValidateAndApply();

        private void ValidateAndApply()
        {
            Vector2Capacity = vector2Capacity;
            Vector3Capacity = vector3Capacity;
            Vector4Capacity = vector4Capacity;
            IntCapacity = intCapacity;
            FloatCapacity = floatCapacity;
            CGSpotCapacity = cgSpotCapacity;
            LogAllocations = logAllocations;
        }
    }
}