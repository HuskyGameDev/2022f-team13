// =====================================================================
// Copyright 2013-2022 ToolBuddy
// All rights reserved
// 
// http://www.toolbuddy.net
// =====================================================================

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Reflection;
using FluffyUnderware.DevTools;
using ToolBuddy.Pooling.Pools;

namespace FluffyUnderware.Curvy.Generator
{
    /// <summary>
    /// Class defining a module slot
    /// </summary>
    public class CGModuleSlot
    {
        /// <summary>
        /// The Module this Slot belongs to
        /// </summary>
        public CGModule Module { get; internal set; }
        /// <summary>
        /// Gets the SlotInfo Attribute
        /// </summary>
        public SlotInfo Info { get; internal set; }

        /// <summary>
        /// Origin of Link-Wire
        /// </summary>
        public Vector2 Origin { get; set; }
        /// <summary>
        /// Mouse-Hotzone
        /// </summary>
        public Rect DropZone { get; set; }

        /// <summary>
        /// Whether the link is wired or not
        /// </summary>
        public bool IsLinked { get { return LinkedSlots != null && LinkedSlots.Count > 0; } }
        /// <summary>
        /// Whether the link is wired and all connected modules are configured
        /// </summary>
        public bool IsLinkedAndConfigured
        {
            get
            {
                if (!IsLinked)
                    return false;
                for (int i = 0; i < LinkedSlots.Count; i++)
                    if (!LinkedSlots[i].Module.IsConfigured)
                        return false;
                return true;
            }
        }
        /// <summary>
        /// Gets the module as an <see cref="IOnRequestProcessing"/>
        /// </summary>
        public IOnRequestProcessing OnRequestModule { get { return Module as IOnRequestProcessing; } }
        /// <summary>
        /// Gets the module as an <see cref="IPathProvider"/>
        /// </summary>
        public IPathProvider PathProvider { get { return Module as IPathProvider; } }
        /// <summary>
        /// Gets the module as an <see cref="IExternalInput"/>
        /// </summary>
        public IExternalInput ExternalInput { get { return Module as IExternalInput; } }
        /// <summary>
        /// All slots of linked modules
        /// </summary>
        public List<CGModuleSlot> LinkedSlots
        {
            get
            {
                if (mLinkedSlots == null)
                    LoadLinkedSlots();
                return mLinkedSlots ?? new List<CGModuleSlot>();
            }
        }
        /// <summary>
        /// Gets the number of connected links, i.e. shortcut to this.Links.Count
        /// </summary>
        public int Count
        {
            get { return LinkedSlots.Count; }
        }

        public string Name
        {
            get { return (Info != null) ? Info.Name : ""; }
        }

        protected List<CGModuleSlot> mLinkedSlots = null;

        public CGModuleSlot()
        {

        }

        public bool HasLinkTo(CGModuleSlot other)
        {
            for (int i = 0; i < LinkedSlots.Count; i++)
                if (LinkedSlots[i] == other)
                    return true;

            return false;
        }

        /// <summary>
        /// Gets a list of all Links' modules
        /// </summary>
        public List<CGModule> GetLinkedModules()
        {
            List<CGModule> res = new List<CGModule>();
            for (int i = 0; i < LinkedSlots.Count; i++)
                res.Add(LinkedSlots[i].Module);
            return res;
        }

        public virtual void LinkTo(CGModuleSlot other)
        {
            if (Module)
            {
                Module.Generator.sortModulesINTERNAL();
                Module.Dirty = true;
            }
            if (other.Module)
                other.Module.Dirty = true;
        }

        protected static void LinkInputAndOutput(CGModuleSlot inputSlot, CGModuleSlot outputSlot)
        {
            if ((!inputSlot.Info.Array || inputSlot.Info.ArrayType == SlotInfo.SlotArrayType.Hidden) && inputSlot.IsLinked)
                inputSlot.UnlinkAll();

            outputSlot.Module.OutputLinks.Add(new CGModuleLink(outputSlot, inputSlot));
            inputSlot.Module.InputLinks.Add(new CGModuleLink(inputSlot, outputSlot));
            if (!outputSlot.LinkedSlots.Contains(inputSlot))
                outputSlot.LinkedSlots.Add(inputSlot);
            if (!inputSlot.LinkedSlots.Contains(outputSlot))
                inputSlot.LinkedSlots.Add(outputSlot);
        }

        public virtual void UnlinkFrom(CGModuleSlot other)
        {
            if (Module)
            {
                Module.Generator.sortModulesINTERNAL();
                Module.Dirty = true;
            }
            if (other.Module)
                other.Module.Dirty = true;
        }

        public virtual void UnlinkAll()
        {
        }

        public void ReInitializeLinkedSlots()
        {
            mLinkedSlots = null;
        }

        protected virtual void LoadLinkedSlots()
        {
        }

        public static implicit operator bool(CGModuleSlot a)
        {
            return !object.ReferenceEquals(a, null);
        }

        public override string ToString()
        {
            return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}: {1}.{2}", GetType().Name, Module.name, Name);
        }

    }

    /// <summary>
    /// Class defining a module's input slot
    /// </summary>
    [System.Serializable]
    public class CGModuleInputSlot : CGModuleSlot
    {
        public InputSlotInfo InputInfo { get { return Info as InputSlotInfo; } }
#if UNITY_EDITOR
        public int LastDataCountINTERNAL { get; set; }
#endif
        public CGModuleInputSlot() : base() { }


        protected override void LoadLinkedSlots()
        {
            if (!Module.Generator.IsInitialized)
                return;
            base.LoadLinkedSlots();
            mLinkedSlots = new List<CGModuleSlot>();
            List<CGModuleLink> lnks = Module.GetInputLinks(this);
            foreach (CGModuleLink l in lnks)
            {
                CGModule mod = Module.Generator.GetModule(l.TargetModuleID, true);
                if (mod)
                {
                    CGModuleOutputSlot slot = mod.OutputByName[l.TargetSlotName];
                    // Sanitize missing links
                    if (!slot.Module.GetOutputLink(slot, this))
                    {
                        slot.Module.OutputLinks.Add(new CGModuleLink(slot, this));
                        slot.ReInitializeLinkedSlots();
                    }

                    if (!mLinkedSlots.Contains(slot))
                        mLinkedSlots.Add(slot);
                }
                else
                {
                    Module.InputLinks.Remove(l);
                }
            }
        }

        public override void UnlinkAll()
        {
            List<CGModuleSlot> ls = new List<CGModuleSlot>(LinkedSlots);
            foreach (CGModuleSlot l in ls)
            {
                UnlinkFrom(l);
            }
        }

        public override void LinkTo(CGModuleSlot outputSlot)
        {
            if (!HasLinkTo(outputSlot))
            {
                LinkInputAndOutput(this, outputSlot);
                base.LinkTo(outputSlot);
            }
        }

        public override void UnlinkFrom(CGModuleSlot outputSlot)
        {
            if (HasLinkTo(outputSlot))
            {
                CGModuleOutputSlot cgModuleOutputSlot = (CGModuleOutputSlot)outputSlot;
                CGModuleLink l1 = Module.GetInputLink(this, cgModuleOutputSlot);
                Module.InputLinks.Remove(l1);
                CGModuleLink l2 = outputSlot.Module.GetOutputLink(cgModuleOutputSlot, this);
                outputSlot.Module.OutputLinks.Remove(l2);

                LinkedSlots.Remove(outputSlot);
                outputSlot.LinkedSlots.Remove(this);

                base.UnlinkFrom(outputSlot);
            }
        }



        /// <summary>
        /// Gets a linked Output slot
        /// </summary>
        public CGModuleOutputSlot SourceSlot(int index = 0)
        {
            return (index < Count && index >= 0) ? (CGModuleOutputSlot)LinkedSlots[index] : null;
        }

        /// <summary>
        /// Determines if a particular output slot of another module can link to this slot
        /// </summary>
        /// <param name="source">the slot of the other module that'd like to link to this input slot</param>
        /// <returns>whether linking is allowed or not</returns>
        public bool CanLinkTo(CGModuleOutputSlot source)
        {
            return source.Module != Module && AreInputAndOutputSlotsCompatible(InputInfo, OnRequestModule != null, source.OutputInfo, source.OnRequestModule != null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputSlotInfo"></param>
        /// <param name="inputSlotModuleIsOnRequest">Does the module owning the input slot implement the IOnRequestProcessing interface</param>
        /// <param name="outputSlotInfo"></param>
        /// <param name="outputSlotModuleIsOnRequest">Does the module owning the output slot implement the IOnRequestProcessing interface</param>
        /// <returns></returns>
        public static bool AreInputAndOutputSlotsCompatible(InputSlotInfo inputSlotInfo, bool inputSlotModuleIsOnRequest, OutputSlotInfo outputSlotInfo, bool outputSlotModuleIsOnRequest)
        {
            return inputSlotInfo.IsValidFrom(outputSlotInfo.DataType) &&
                    ((outputSlotModuleIsOnRequest && (inputSlotInfo.RequestDataOnly || inputSlotModuleIsOnRequest)) || (outputSlotModuleIsOnRequest == false && !inputSlotInfo.RequestDataOnly));
        }

        /// <summary>
        /// Gets the module connected to the link
        /// </summary>
        /// <param name="index">the link index</param>
        /// <returns>a module</returns>
        private CGModule SourceModule(int index)
        {
            return (index < Count && index >= 0) ? LinkedSlots[index].Module : null;
        }

        /// <summary>
        /// Gets the data from the module connected to a certain input slot. If more than one module is connected, the first module's data is returned
        /// </summary>
        /// <typeparam name="T">type of requested data</typeparam>
        /// <param name="requests">request parameters</param>
        /// <returns>the data</returns>
        public T GetData<T>(params CGDataRequestParameter[] requests) where T : CGData
        {
            return GetData<T>(out _, requests);
        }

        /// <summary>
        /// Gets the data from the module connected to a certain input slot. If more than one module is connected, the first module's data is returned
        /// </summary>
        /// <typeparam name="T">type of requested data</typeparam>
        /// <param name="isDataDisposable">Whether the returned data can be disposed safely after using it, in order to make its resources available for future use, and thus reducing garbage collection.
        /// It is set to false when returned data is a direct reference to data stored by the module, and not a copy of it
        /// <seealso cref="CGData.Dispose(bool)"/>
        /// <seealso cref="ArrayPool{U}"/>
        /// </param>
        /// <param name="requests">request parameters</param>
        /// <returns>the data</returns>
        public T GetData<T>(out bool isDataDisposable, params CGDataRequestParameter[] requests) where T : CGData
        {
            CGData[] data = GetData<T>(0, out isDataDisposable, requests);
#if UNITY_EDITOR
            LastDataCountINTERNAL = (data == null || data.Length == 0) ? 0 : data.Length;
#endif
            if (data == null || data.Length == 0 || data[0] == null)
            {
                isDataDisposable = false;
                return null;
            }

            return data[0] as T;
        }

        /// <summary>
        /// Gets the data from all modules connected to a certain input slot.
        /// </summary>
        /// <typeparam name="T">type of requested data</typeparam>
        /// <param name="requests">request parameters</param>
        /// <returns>the data</returns>
        public List<T> GetAllData<T>(params CGDataRequestParameter[] requests) where T : CGData
        {
            return GetAllData<T>(out _, requests);
        }

        /// <summary>
        /// Gets the data from all modules connected to a certain input slot.
        /// </summary>
        /// <typeparam name="T">type of requested data</typeparam>
        /// <param name="isDataDisposable">Whether the returned data can be disposed safely after using it, in order to make its resources available for future use, and thus reducing garbage collection.
        /// It is set to false when returned data is a direct reference to data stored by the module, and not a copy of it
        /// <seealso cref="CGData.Dispose(bool)"/>
        /// <seealso cref="ArrayPool{U}"/>
        /// </param>
        /// <param name="requests">request parameters</param>
        /// <returns>the data</returns>
        public List<T> GetAllData<T>(out bool isDataDisposable, params CGDataRequestParameter[] requests) where T : CGData
        {
            isDataDisposable = true;
            List<T> res = new List<T>();
            for (int i = 0; i < Count; i++)
            {
                CGData[] data = GetData<T>(i, out bool isDisposable, requests);
                if (data != null)
                {
                    //If in the returned there are multiple CGData, some needing to be disposed, others not, then we consider that none should be disposed
                    //This means that some data might not be flagged as disposable, which means it will be dispose once Finalize is called. Missed opportunity to instantly reuse its data. Not good for reducing garbage collection
                    isDataDisposable &= isDisposable;
#if CURVY_SANITY_CHECKS
                    //TODO right now there are no graph that I am aware of that can trigger the situation explained above, but in the future, if there are modules that take multiple paths as inputs, one of them can be from Input Path module (isDisposable true) and one from Shape Extrusion (output Volume, which is also a path, isDisposable false)
                    if (isDisposable && isDataDisposable == false)
                        DTLog.LogWarning("[Curvy] A disposable data was treated as not disposable");
#endif
                    if (!Info.Array)
                    {
                        res.Add(data[0] as T);
                        break;
                    }

                    for (int a = 0; a < data.Length; a++)
                        res.Add(data[a] as T);
                }
            }

#if UNITY_EDITOR
            LastDataCountINTERNAL = res.Count;
#endif
            return res;
        }

        /// <summary>
        /// Gets the data from the module connected to a certain input slot
        /// </summary>
        /// <typeparam name="T">type of requested data</typeparam>
        /// <param name="slotIndex">slot index (if the slot supports multiple inputs)</param>
        /// <param name="isDataDisposable">Whether the returned data can be disposed safely after using it, in order to make its resources available for future use, and thus reducing garbage collection.
        /// It is set to false when returned data is a direct reference to data stored by the module, and not a copy of it
        /// <seealso cref="CGData.Dispose(bool)"/>
        /// <seealso cref="ArrayPool{U}"/>
        /// </param>
        /// <param name="requests">request parameters</param>
        /// <returns>the data</returns>
        private CGData[] GetData<T>(int slotIndex, out bool isDataDisposable, params CGDataRequestParameter[] requests
            ) where T : CGData
        {
            CGModuleOutputSlot source = SourceSlot(slotIndex);
            if (source == null || !source.Module.Active)
            {
                isDataDisposable = true;
                return new CGData[0];
            }

            // Handles IOnRequestProcessing modules (i.e. modules that provides data on the fly)
            if (source.Module is IOnRequestProcessing)
            {
                bool needNewData = (source.Data == null || source.Data.Length == 0);
                // Return last data?
                if (!needNewData && source.LastRequestParameters != null && source.LastRequestParameters.Length == requests.Length)
                {
                    for (int i = 0; i < requests.Length; i++)
                        if (!requests[i].Equals(source.LastRequestParameters[i]))
                        {
                            needNewData = true;
                            break;
                        }
                }
                else
                    needNewData = true;

                if (needNewData)
                {
                    source.LastRequestParameters = requests;
#if UNITY_EDITOR || CURVY_DEBUG
                    source.Module.DEBUG_LastUpdateTime = System.DateTime.Now;
                    Module.DEBUG_ExecutionTime.Pause();
                    source.Module.DEBUG_ExecutionTime.Start();
#endif
                    source.Module.UIMessages.Clear();//TODO Find a way to move this line of code inside OnSlotDataRequest
                    source.SetData(((IOnRequestProcessing)source.Module).OnSlotDataRequest(this, source, requests));
#if UNITY_EDITOR || CURVY_DEBUG
                    source.Module.DEBUG_ExecutionTime.Stop();
                    Module.DEBUG_ExecutionTime.Start();
#endif
                }

            }

            bool copyData = InputInfo.ModifiesData || source.Module is IOnRequestProcessing;

            CGData[] result = copyData
                ? cloneData<T>(source.Data)
                : source.Data;

            isDataDisposable = copyData && result != null;

            return result; ;
        }

        private static CGData[] cloneData<T>(CGData[] source) where T : CGData
        {
            T[] d = new T[source.Length];
            for (int i = 0; i < source.Length; i++)
                d[i] = source[i] == null ? null : source[i].Clone<T>();
            return d;
        }

    }

    /// <summary>
    /// Class defining a module's output slot
    /// </summary>
    [System.Serializable]
    public class CGModuleOutputSlot : CGModuleSlot
    {
        public OutputSlotInfo OutputInfo { get { return Info as OutputSlotInfo; } }
        public CGData[] Data = new CGData[0];
        public CGDataRequestParameter[] LastRequestParameters; // used for caching of Virtual Modules

        public CGModuleOutputSlot() : base() { }

        protected override void LoadLinkedSlots()
        {
            if (!Module.Generator.IsInitialized)
                return;
            base.LoadLinkedSlots();
            mLinkedSlots = new List<CGModuleSlot>();
            List<CGModuleLink> lnks = Module.GetOutputLinks(this);
            foreach (CGModuleLink l in lnks)
            {
                CGModule mod = Module.Generator.GetModule(l.TargetModuleID, true);
                if (mod)
                {
                    CGModuleInputSlot slot = mod.InputByName[l.TargetSlotName];

                    // Sanitize missing links
                    if (!slot.Module.GetInputLink(slot, this))
                    {
                        slot.Module.InputLinks.Add(new CGModuleLink(slot, this));
                        slot.ReInitializeLinkedSlots();
                    }

                    if (!mLinkedSlots.Contains(slot))
                        mLinkedSlots.Add(slot);
                }
                else
                {
                    Module.OutputLinks.Remove(l);
                }
            }
        }

        public override void LinkTo(CGModuleSlot inputSlot)
        {
            if (!HasLinkTo(inputSlot))
            {
                LinkInputAndOutput(inputSlot, this);
                base.LinkTo(inputSlot);
            }
        }

        public override void UnlinkFrom(CGModuleSlot inputSlot)
        {
            if (HasLinkTo(inputSlot))
            {
                CGModuleInputSlot cgModuleInputSlot = (CGModuleInputSlot)inputSlot;
                CGModuleLink l1 = Module.GetOutputLink(this, cgModuleInputSlot);
                Module.OutputLinks.Remove(l1);

                CGModuleLink l2 = inputSlot.Module.GetInputLink(cgModuleInputSlot, this);
                inputSlot.Module.InputLinks.Remove(l2);

                LinkedSlots.Remove(inputSlot);
                inputSlot.LinkedSlots.Remove(this);

                base.UnlinkFrom(inputSlot);
            }
        }



        public bool HasData
        {
            get { return Data != null && Data.Length > 0 && Data[0] != null; }
        }

        public void ClearData()
        {
            AssignNewData(new CGData[0]);
        }

        private void AssignNewData(CGData[] newData)
        {
            if (Data == newData)
                return;

            foreach (CGData cgData in Data)
                if (cgData && newData.Contains(cgData) == false)
                    cgData.Dispose();

            Data = newData;
        }

        public void SetData<T>(List<T> newData) where T : CGData
        {
            CGData[] newDataArray;
            if (newData == null)
                newDataArray = new CGData[0];
            else
            {
                if (!Info.Array && newData.Count > 1)
                    Debug.LogWarning("[Curvy] " + Module.GetType().Name + " (" + Info.DisplayName + ") only supports a single data item! Either avoid calculating unnecessary data or define the slot as an array!");
                newDataArray = newData.ToArray();
            }
            AssignNewData(newDataArray);
        }

        public void SetData(params CGData[] newData)
        {
            //TODO why does this not do the same test then the other SetData method, i.e. if (!Info.Array && data.Count > 1)
            AssignNewData((newData == null) ? new CGData[0] : newData);
        }

        public T GetData<T>() where T : CGData
        {
            return (Data.Length == 0) ? null : Data[0] as T;
        }

        public T[] GetAllData<T>() where T : CGData
        {
            return Data as T[];
        }
    }

    /// <summary>
    /// Attribute to define slot properties
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes")]
    [AttributeUsage(AttributeTargets.Field)]
    public class SlotInfo : Attribute, IComparable
    {
        /// <summary>
        /// Defines what type of Array is used
        /// </summary>
        public enum SlotArrayType
        {
            Unknown,
            /// <summary>
            /// An array that behaves like an array code wise and UI wise
            /// </summary>
            Normal,
            /// <summary>
            /// An array that behave like an array code wise, but is displayed as a single instance of CGData UI wise.
            /// This allows for CG modules to send/receive arrays, without giving the user the possibility to link multiple modules to the slot
            /// </summary>
            Hidden
        }

        public readonly Type[] DataTypes;

        /// <summary>
        /// If empty Field's name will be used, with slight modifications
        /// </summary>
        public string Name;

        private string displayName = null;
        /// <summary>
        /// If not null, this string will be used in the UI, while <see cref="Name"/> will be used in the data serialization and slots linking logic
        /// </summary>
        public string DisplayName
        {
            get { return displayName ?? Name; }
            set { displayName = value; }
        }

        public string Tooltip;

        /// <summary>
        /// Whether or not the slot accepts an array of CGData instances or a single instance of it
        /// </summary>
        public bool Array;//DESIGN should be renamed to IsArray

        /// <summary>
        /// When <see cref="Array"/> is true, this value defines what type of Array is used
        /// </summary>
        public SlotArrayType ArrayType = SlotArrayType.Normal;

        protected SlotInfo(string name, params Type[] type)
        {
            DataTypes = type;
            Name = name;
        }
        protected SlotInfo(params Type[] type) : this(null, type) { }

        public int CompareTo(object obj)
        {
            return String.Compare(((SlotInfo)obj).Name, Name, StringComparison.Ordinal);
        }

        //TODO code analysis (CA1036) says that Equal, !=, <, == and > should be defined since IComparable is implemented

    }
    /// <summary>
    /// Attribute to define input sot properties
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class InputSlotInfo : SlotInfo
    {
        public bool RequestDataOnly = false;
        public bool Optional = false;
        /// <summary>
        /// Whether this data is altered by the module.
        /// If true, the module providing data to this slot will return a copy of its data, and not the original copy, so you can safely modify it.
        /// </summary>
        // DESIGN should this be removed, and ask users to just clone the data when they need to modify it?
        public bool ModifiesData = false;

        public InputSlotInfo(string name, params Type[] type) : base(name, type) { }
        public InputSlotInfo(params Type[] type) : this(null, type) { }

        /// <summary>
        /// Gets whether outType is of same type or a subtype of one of our input types
        /// </summary>
        public bool IsValidFrom(Type outType)
        {
            for (int x = 0; x < DataTypes.Length; x++)
                if (outType == DataTypes[x] || outType.IsSubclassOf(DataTypes[x]))
                    return true;
            return false;
        }
    }

    /// <summary>
    /// Attribute to define output slot properties
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class OutputSlotInfo : SlotInfo
    {
        public Type DataType
        {
            get
            {
                return DataTypes[0];
            }
        }

        public OutputSlotInfo(Type type) : this(null, type) { }

        public OutputSlotInfo(string name, Type type) : base(name, type) { }
    }

    /// <summary>
    /// An <see cref="OutputSlotInfo"/> preset for modules that output CGShape data. Allows modules to output a <see cref="CGShape"/> that varies along a shape extrusion. See also <see cref="CGDataRequestShapeRasterization"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class ShapeOutputSlotInfo : OutputSlotInfo
    {
        /// <summary>
        /// Whether this module outputs a <see cref="CGShape"/> that varies along a shape extrusion
        /// </summary>
        public bool OutputsVariableShape = false;

        public ShapeOutputSlotInfo() : this(null) { }
        public ShapeOutputSlotInfo(string name) : base(name, typeof(CGShape)) { }
    }
}