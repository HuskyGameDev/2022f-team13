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
using FluffyUnderware.DevTools;
using JetBrains.Annotations;

namespace FluffyUnderware.Curvy.Generator
{
    /// <summary>
    /// For modules that don't process anything
    /// </summary>
    public interface INoProcessing
    {
    }

    /// <summary>
    /// For modules that rely on external input (Splines, Meshes etc..)
    /// </summary>
    public interface IExternalInput
    {
        /// <summary>
        /// Whether the module currently supports an IPE session
        /// </summary>
        bool SupportsIPE { get; }
    }

    /// <summary>
    /// For modules that process data on demand
    /// </summary>
    public interface IOnRequestProcessing
    {
        CGData[] OnSlotDataRequest(CGModuleInputSlot requestedBy, CGModuleOutputSlot requestedSlot, params CGDataRequestParameter[] requests);
    }

    /// <summary>
    /// For modules that output instances of <see cref="CGPath"/>
    /// </summary>
    public interface IPathProvider
    {
        bool PathIsClosed { get; }
    }

    /// <summary>
    /// Resource Loader Interface
    /// </summary>
    public interface ICGResourceLoader
    {
        Component Create(CGModule cgModule, [NotNull] string context);
        void Destroy(CGModule cgModule, Component obj, [NotNull] string context, bool kill);
    }

    /// <summary>
    /// Resource Collection interface
    /// </summary>
    public interface ICGResourceCollection
    {
        int Count { get; }
        Component[] ItemsArray { get; }
    }
}