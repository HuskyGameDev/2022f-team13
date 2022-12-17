// =====================================================================
// Copyright 2013-2022 ToolBuddy
// All rights reserved
// 
// http://www.toolbuddy.net
// =====================================================================

using UnityEngine;
using System.Collections;
using System;
using System.Reflection;
using FluffyUnderware.DevTools;

namespace FluffyUnderware.Curvy
{

    #region ### CG related ###

    /// <summary>
    /// CG Resource Attribute
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class CGResourceManagerAttribute : DTPropertyAttribute
    {
        public readonly string ResourceName;
        public bool ReadOnly;

        public CGResourceManagerAttribute(string resourceName)
        {
            ResourceName = resourceName;
        }
    }

    /// <summary>
    /// CG Resource Collection Attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public sealed class CGResourceCollectionManagerAttribute : CGResourceManagerAttribute
    {
        public bool ShowCount;

        public CGResourceCollectionManagerAttribute(string resourceName)
            : base(resourceName)
        {
            ReadOnly = true;
        }
    }

    /// <summary>
    /// CG Data Reference Selector Attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public sealed class CGDataReferenceSelectorAttribute : DTPropertyAttribute
    {
        public readonly System.Type DataType;

        public CGDataReferenceSelectorAttribute(System.Type dataType)
        {
            DataType = dataType;
        }
    }

    #endregion



}