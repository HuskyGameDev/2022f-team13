// =====================================================================
// Copyright 2013-2022 ToolBuddy
// All rights reserved
// 
// http://www.toolbuddy.net
// =====================================================================

using System;
using UnityEngine;
using UnityEditor;
using FluffyUnderware.Curvy.Generator;
using System.Collections.Generic;
using System.Linq;
using FluffyUnderware.DevToolsEditor;
using JetBrains.Annotations;

namespace FluffyUnderware.CurvyEditor.Generator
{
    public class CanvasSelection
    {
        public List<CGModule> SelectedModules = new List<CGModule>();

        public CGModuleLink SelectedLink { get; private set; }
        public CGModule SelectedModule
        {
            get { return (SelectedModules.Count > 0) ? SelectedModules[0] : null; }
        }

        //todo unused, remove it
        public CGGraph Parent;

        /// <summary>
        /// Returns a new array with the <see cref="SelectedLink"/> if any, otherwise <see cref="SelectedModules"/>
        /// </summary>
        public object[] SelectedObjects
        {
            get
            {
                if (SelectedLink != null)
                    return new object[1] { SelectedLink };
                return SelectedModules.ToArray();
            }
        }

        /// <summary>
        /// Empties list and adds into it the <see cref="SelectedLink"/> if any, otherwise <see cref="SelectedModules"/>
        /// </summary>
        public void FillWithSelectedObjects(List<object> list)
        {
            list.Clear();
            if (SelectedLink != null)
                list.Add(SelectedLink);
            else
                list.AddRange(SelectedModules);
        }

        public CanvasSelection(CGGraph parent)
        {
            Parent = parent;
        }

        public void Clear()
        {
            SelectedLink = null;
            SelectedModules.Clear();
            if (CurvyProject.Instance.CGSynchronizeSelection)
                DTSelection.Clear();
        }

        /// <summary>
        /// Selects nothing (null), a link or one or more modules
        /// </summary>
        /// <param name="mod"></param>
        [Obsolete("Use SetSelectionTo, or Clear, depending on your needs")]
        public void Select(params object[] objects)
        {
            Clear();
            if (objects == null || objects.Length == 0)
                return;
            if (objects[0] is List<CGModule>)
                objects = ((List<CGModule>)objects[0]).ToArray();
            if (objects[0] is CGModuleLink)
                SelectedLink = (CGModuleLink)objects[0];
            else
            {
                List<Component> cmp = new List<Component>();
                foreach (object o in objects)
                    if (o is CGModule)
                    {
                        SelectedModules.Add((CGModule)o);
                        cmp.Add((CGModule)o);
                    }

                if (CurvyProject.Instance.CGSynchronizeSelection)
                    DTSelection.SetGameObjects(cmp.ToArray());
            }

        }

        public void SetSelectionTo([NotNull] CGModuleLink link)
        {
            Clear();
            SelectedLink = link;
        }

        public void SetSelectionTo([NotNull] CGModule module)
        {
            SetSelectionTo(new[] { module });
        }

        public void SetSelectionTo([NotNull] IEnumerable<CGModule> modules)
        {
            bool modulesSelectionChanged = modules.SequenceEqual(SelectedModules) == false;

            Clear();

            SelectedModules.AddRange(modules);

            if (modulesSelectionChanged && CurvyProject.Instance.CGSynchronizeSelection)
                DTSelection.SetGameObjects(modules.Select(m => m as Component).ToArray());
        }

        /// <summary>
        /// Adds or removes a module from the selection
        /// </summary>
        // Todo this code does not handle selection synchronisation. fix this before rehabilitating the method if needed
        [Obsolete("Use SetSelectionTo, or Clear, depending on your needs")]
        public void MultiSelectModule(CGModule mod)
        {
            if (mod == null)
                return;
            if (SelectedModules.Contains(mod))
                SelectedModules.Remove(mod);
            else
                SelectedModules.Add(mod);

            //todo why is this nt handling  if (CurvyProject.Instance.CGSynchronizeSelection)

        }
    }
}