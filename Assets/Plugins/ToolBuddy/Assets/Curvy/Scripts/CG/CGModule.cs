// =====================================================================
// Copyright 2013-2022 ToolBuddy
// All rights reserved
// 
// http://www.toolbuddy.net
// =====================================================================

using System;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using FluffyUnderware.DevTools.Extensions;

using System.Collections;
using System.Globalization;
using FluffyUnderware.DevTools;
using JetBrains.Annotations;
using Random = UnityEngine.Random;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AnimatedValues;
#endif




namespace FluffyUnderware.Curvy.Generator
{
    /// <summary>
    /// Curvy Generator module base class
    /// </summary>
    [ExecuteAlways]
    public abstract class CGModule : DTVersionedMonoBehaviour
    {
        #region ### Events ###

        /*! \cond PRIVATE */

        [Group("Events", Expanded = false, Sort = 1000)]
        [SerializeField]
        protected CurvyCGEvent m_OnBeforeRefresh = new CurvyCGEvent();
        [Group("Events")]
        [SerializeField]
        protected CurvyCGEvent m_OnRefresh = new CurvyCGEvent();

        /*! \endcond */

        public CurvyCGEvent OnBeforeRefresh
        {
            get { return m_OnBeforeRefresh; }
            set
            {
                if (m_OnBeforeRefresh != value)
                    m_OnBeforeRefresh = value;

            }

        }

        public CurvyCGEvent OnRefresh
        {
            get { return m_OnRefresh; }
            set
            {
                if (m_OnRefresh != value)
                    m_OnRefresh = value;

            }

        }

        protected CurvyCGEventArgs OnBeforeRefreshEvent(CurvyCGEventArgs e)
        {
            if (OnBeforeRefresh != null)
                OnBeforeRefresh.Invoke(e);
            return e;
        }

        protected CurvyCGEventArgs OnRefreshEvent(CurvyCGEventArgs e)
        {
            if (OnRefresh != null)
                OnRefresh.Invoke(e);
            return e;
        }

        #endregion

        #region ### Public Fields & Properties ###

        #region --- Fields ---

        [SerializeField, HideInInspector] private string m_ModuleName;
        [SerializeField, HideInInspector] private bool m_Active = true;

        [Group("Seed Options", Expanded = false, Sort = 1001)]
        [GroupCondition(nameof(usesRandom))]
        [FieldAction("CBSeedOptions", ShowBelowProperty = true)]
        [SerializeField]
        private bool m_RandomizeSeed = false;

        [SerializeField, HideInInspector] private int m_Seed = unchecked((int)System.DateTime.Now.Ticks);

        #endregion


        #region --- API Accessors ---

        public string ModuleName
        {
            get { return name; }
            set
            {
                if (name != value)
                {
                    name = value;
                    renameManagedResourcesINTERNAL();
                }
            }
        }

        public bool Active
        {
            get { return m_Active; }
            set
            {
                if (m_Active != value)
                {
                    m_Active = value;
                    Dirty = true;
                    Generator.sortModulesINTERNAL();
                }
            }
        }

        /// <summary>
        /// If <see cref="RandomizeSeed"/> is set to false, Seed is used to initialize Unity's random numbers generator before refreshing the
        /// If <see cref="RandomizeSeed"/> is set to true, a random seed will be used
        /// current module
        /// </summary>
        public int Seed
        {
            get { return m_Seed; }
            set
            {
                if (m_Seed != value)
                    m_Seed = value;
                Dirty = true;
            }
        }

        /// <summary>
        /// If is set to false, <see cref="Seed"/> is used to initialize Unity's random numbers generator before refreshing the current module.
        /// If set to true, a random seed will be used
        /// </summary>
        public bool RandomizeSeed
        {
            get { return m_RandomizeSeed; }
            set
            {
                if (m_RandomizeSeed != value)
                    m_RandomizeSeed = value;
            }
        }

        #endregion



        [System.NonSerialized]
        public List<string> UIMessages = new List<string>();

        public CurvyGenerator Generator
        {
            get { return mGenerator; }
        }

        private CurvyGenerator mGenerator;



        #endregion

        #region ### Graph Related ###


        #region UniqueID

        [SerializeField, HideInInspector] private int m_UniqueID;

        public int UniqueID
        {
            get { return m_UniqueID; }
        }

        // In order to reduce per frame allocations, we cache the string version
        private string m_UniqueIDString = null;
        private string UniqueIDString
        {
            get
            {
                if (m_UniqueIDString == null)
                    m_UniqueIDString = m_UniqueID.ToString(System.Globalization.CultureInfo.InvariantCulture);
                return m_UniqueIDString;
            }
        }

        #endregion


        /// <summary>
        /// Whether this module has circular reference errors
        /// </summary>
        public bool CircularReferenceError { get; set; }

        /// <summary>
        /// Helper for topology sorting
        /// </summary>
        //DESIGN inline this in doSortModules()
        internal int SortAncestors;


        [HideInInspector]
        public CGModuleProperties Properties = new CGModuleProperties();
        [HideInInspector]
        public List<CGModuleLink> InputLinks = new List<CGModuleLink>();
        [HideInInspector]
        public List<CGModuleLink> OutputLinks = new List<CGModuleLink>();

        // These will be filled by reflection in OnEnable()
        public Dictionary<string, CGModuleInputSlot> InputByName { get; private set; }
        public Dictionary<string, CGModuleOutputSlot> OutputByName { get; private set; }
        public List<CGModuleInputSlot> Input { get; private set; }
        public List<CGModuleOutputSlot> Output { get; private set; }

        public ModuleInfoAttribute Info
        {
            get
            {
                if (mInfo == null)
                    mInfo = getInfo();
                return mInfo;
            }
        }

        private ModuleInfoAttribute mInfo;

        //-----

        public bool Dirty
        {
            get { return mDirty; }
            set
            {
                if (mDirty != value)
                    mDirty = value;


                if (mDirty)
                {
                    bool isConfigured = IsConfigured;
                    if (mLastIsConfiguredState != isConfigured)
                        mStateChangeDirty = true;
                    mLastIsConfiguredState = isConfigured;
                    if (Output != null)
                    {
                        for (int i = 0; i < Output.Count; i++)
                        {
                            if (Output[i].IsLinked)
                            {
                                List<CGModule> modules = Output[i].GetLinkedModules();
                                for (int m = 0; m < modules.Count; m++)
                                    //BUG? does the || modules[m].CircularReferenceError create infinite dirtying logique?
                                    if (modules[m] != this || modules[m].CircularReferenceError) // prevent circular reference
                                        modules[m].Dirty = true;
                            }
                        }
                    }
                }

                if (this is IOnRequestProcessing || this is INoProcessing)
                {
                    mDirty = false;
                    if (Output != null)
                        for (int i = 0; i < Output.Count; i++)
                            Output[i].LastRequestParameters = null;
                }

            }
        }

        private bool mDirty = true;
        private bool mInitialized = false;

        private bool mStateChangeDirty;
        private bool mLastIsConfiguredState;


        #endregion

        #region ### Debugging ###
#if UNITY_EDITOR || CURVY_DEBUG
        public System.DateTime DEBUG_LastUpdateTime;
        public TimeMeasure DEBUG_ExecutionTime = new TimeMeasure(5);
#endif

        #endregion

        #region ### Unity Callbacks (Virtual) ###

        protected virtual void Awake()
        {
            mGenerator = RetrieveGenerator();
        }

        protected virtual void OnEnable()
        {
            if (mGenerator)
            {
                Initialize();
                Generator.sortModulesINTERNAL();
            }
        }

        public void Initialize()
        {
            if (!mGenerator)
                mGenerator = RetrieveGenerator();
            if (!mGenerator)
                Invoke(nameof(Delete), 0);
            else
            {
                mInfo = getInfo();

                if (string.IsNullOrEmpty(ModuleName))
                    if (string.IsNullOrEmpty(Info.ModuleName))
                        ModuleName = Generator.getUniqueModuleNameINTERNAL(Info.MenuName.Substring(Info.MenuName.LastIndexOf("/", StringComparison.Ordinal) + 1));
                    else
                        ModuleName = Generator.getUniqueModuleNameINTERNAL(Info.ModuleName);

                loadSlots();
                mInitialized = true;
            }
        }

        protected virtual void OnDisable()
        {
        }

        protected virtual void OnDestroy()
        {
            bool realDestroy = true;
#if UNITY_EDITOR
            if (EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying)
                realDestroy = false;
#endif
            setTreeDirtyStateChange();
            List<Component> res;
            List<string> resNames;
            // Resources
            if (realDestroy)
            {
                if (GetManagedResources(out res, out resNames))
                {
                    for (int i = res.Count - 1; i >= 0; i--)
                        DeleteManagedResource(resNames[i], res[i], string.Empty, true);
                }
            }

            // Links

            List<CGModuleInputSlot> inSlots = GetInputSlots();
            foreach (CGModuleInputSlot slot in inSlots)
            {
                List<CGModule> linkedModules = slot.GetLinkedModules();
                foreach (CGModule module in linkedModules)
                    if (module != null)
                        module.ReInitializeLinkedSlots();
            }

            List<CGModuleOutputSlot> outSlots = GetOutputSlots();
            foreach (CGModuleOutputSlot slot in outSlots)
            {
                List<CGModule> linkedModules = slot.GetLinkedModules();
                foreach (CGModule module in linkedModules)
                    if (module != null)
                        module.ReInitializeLinkedSlots();
            }

            if (Generator)
            {
                // Delete module
                Generator.ModulesByID.Remove(UniqueID);
                Generator.Modules.Remove(this);
                Generator.sortModulesINTERNAL();
            }
            mInitialized = false;
        }

#if UNITY_EDITOR
        /*DESIGN:simplify the CGModules' OnValidate and properties by:
        - Make properties set dirty only if the value has changed
        - Set dirty to true in CGModule.OnValidate
        - avoid calling SomeProperty = m_SomeProperty in the OnValidate of all CGModules. You have of course to handle any work done by SomeProperty, that is not dirtying, in the OnValidate of these modules.*/
        protected virtual void OnValidate()
        {
        }

        private void Update()
        {
            if (!Application.isPlaying)
                renameManagedResourcesINTERNAL();
        }
#endif
        private void OnDidApplyAnimationProperties()
        {
            Dirty = true;
        }

        #endregion

        #region ### Virtual Methods & Properties ###

        /// <summary>
        /// Gets whether the module is properly configured i.e. has everything to work like intended
        /// </summary>
        public virtual bool IsConfigured
        {
            get
            {
                if (!IsInitialized || CircularReferenceError || !Active)
                    return false;

                int validTotalLinks = 0;
                for (int i = 0; i < Input.Count; i++)
                {
                    InputSlotInfo myInfo = Input[i].InputInfo;
                    if (Input[i].IsLinked)
                    {
                        for (int link = 0; link < Input[i].Count; link++)
                            if (Input[i].SourceSlot(link) != null)
                            {
                                if (Input[i].SourceSlot(link).Module.IsConfigured)
                                    validTotalLinks++;
                                else if (!myInfo.Optional)
                                    return false;
                            }
                    }
                    else
                        if (myInfo == null || !myInfo.Optional)
                        return false;
                }

                return validTotalLinks > 0 || Input.Count == 0;
            }
        }

        /// <summary>
        /// Gets whether the module and all its dependencies are fully initialized
        /// </summary>
        public virtual bool IsInitialized { get { return mInitialized; } }

        /// <summary>
        /// Add Module processing code in here
        /// </summary>
        public virtual void Refresh()
        {
            //            Debug.Log(name + ".Refresh()");
            UIMessages.Clear();
        }

        public virtual void Reset()
        {
            ModuleName = string.IsNullOrEmpty(Info.ModuleName) ? GetType().Name : Info.ModuleName;

            //Remove all non-persisent (ie created from script) listeners from the events. Might help with garbage collection
            if (OnBeforeRefresh != null)
                OnBeforeRefresh.RemoveAllListeners();
            if (OnRefresh != null)
                OnRefresh.RemoveAllListeners();

            OnBeforeRefresh = new CurvyCGEvent();
            OnRefresh = new CurvyCGEvent();

            DeleteAllOutputManagedResources();
        }

        public void ReInitializeLinkedSlots()
        {
            List<CGModuleInputSlot> ins = GetInputSlots();
            List<CGModuleOutputSlot> ous = GetOutputSlots();
            for (int i = 0; i < ins.Count; i++)
                ins[i].ReInitializeLinkedSlots();
            for (int i = 0; i < ous.Count; i++)
                ous[i].ReInitializeLinkedSlots();
        }

        /// <summary>
        /// Delete all the managed resources acting as an output. One example of this are the generated meshes by the <see cref="FluffyUnderware.Curvy.Generator.Modules.CreateMesh"/> module
        /// </summary>
        /// <returns>True if there were deleted resources</returns>
        virtual public bool DeleteAllOutputManagedResources()
        {
            return false;
        }

        /// <summary>
        /// Called when a module's state changes (Link added/removed, Active toggles etc..)
        /// </summary>
        public virtual void OnStateChange()
        {
            //            Debug.Log(name + ".OSC, configured="+IsConfigured);
            Dirty = true;

            if (Output != null)
            {
                for (int i = 0; i < Output.Count; i++)
                {
                    Output[i].ClearData();
                    /*
                    if (Output[i].IsLinked)
                    {
                        var modules = Output[i].GetLinkedModules();
                        for (int m = 0; m < modules.Count; m++)
                            if (modules[m] != this || modules[m].CircularReferenceError) // prevent circular reference
                                modules[m].CheckAndRaiseOnStateChangedINTERNAL();
                    }
                     */
                }
            }
#if UNITY_EDITOR
            if (Input != null)
                for (int i = 0; i < Input.Count; i++)
                    Input[i].LastDataCountINTERNAL = 0;
#endif
            if (!IsConfigured)
                DeleteAllOutputManagedResources();
        }

        /// <summary>
        /// Called after a module was copied to a template
        /// </summary>
        /// <remarks>Use this handle references that can't be templated etc...</remarks>
        public virtual void OnTemplateCreated()
        {
            DeleteAllOutputManagedResources();
        }

        #endregion

        #region ### Helpers ###

        /// <summary>
        /// Gets a request parameter of a certain type
        /// </summary>
        /// <typeparam name="T">Type derived from PCGDataRequestParameter</typeparam>
        /// <param name="requests">reference to the list of request parameters</param>
        /// <returns>the wanted request parameter or null</returns>
        static protected T GetRequestParameter<T>(ref CGDataRequestParameter[] requests) where T : CGDataRequestParameter
        {
            for (int i = 0; i < requests.Length; i++)
                if (requests[i] is T)
                    return (T)requests[i];

            return null;
        }

        /// <summary>
        /// Removes a certain request parameter from the requests array
        /// </summary>
        /// <param name="requests">reference to the requests array</param>
        /// <param name="request">the request to remove</param>
        static protected void RemoveRequestParameter(ref CGDataRequestParameter[] requests, CGDataRequestParameter request)
        {
            for (int i = 0; i < requests.Length; i++)
                if (requests[i] == request)
                {
                    requests = requests.RemoveAt(i);
                    return;
                }

        }

        #endregion

        #region ### Public Methods ###


        public CGModuleLink GetOutputLink(CGModuleOutputSlot outSlot, CGModuleInputSlot inSlot)
        {
            return GetLink(OutputLinks, outSlot, inSlot);
        }

        public List<CGModuleLink> GetOutputLinks(CGModuleOutputSlot outSlot)
        {
            return GetLinks(OutputLinks, outSlot);
        }

        public CGModuleLink GetInputLink(CGModuleInputSlot inSlot, CGModuleOutputSlot outSlot)
        {
            return GetLink(InputLinks, inSlot, outSlot);
        }

        public List<CGModuleLink> GetInputLinks(CGModuleInputSlot inSlot)
        {
            return GetLinks(InputLinks, inSlot);
        }


        public CGModule CopyTo(CurvyGenerator targetGenerator)
        {
            if (this == null)
                throw new InvalidOperationException($"[Curvy] Trying to copy the already deleted module {this.name}");

            CGModule newModule = this.DuplicateGameObject<CGModule>(targetGenerator.transform, false);
            newModule.mGenerator = targetGenerator;
            newModule.Initialize();
            newModule.ModuleName = ModuleName;
            newModule.ModuleName = targetGenerator.getUniqueModuleNameINTERNAL(newModule.ModuleName);
            newModule.SetUniqueIdINTERNAL();
            newModule.renameManagedResourcesINTERNAL();
            return newModule;
        }

        public Component AddManagedResource(string resourceName, string context = "", int index = -1)
        {
            Component res = CGResourceHandler.CreateResource(this, resourceName, context);
            if (res == null)
                throw new InvalidOperationException(String.Format("[Curvy] Could not create managed resource of type '{0}'. In some cases this is due to not enough elements in a resources Pool", resourceName));
            res.name = GetResourceName(
                context == "" ? resourceName : (resourceName + context),
                index);
            res.transform.SetParent(transform);
            return res;
        }


        public void DeleteManagedResource(string resourceName, Component res, [NotNull] string context = "", bool dontUsePool = false)
        {
            if (res)
                CGResourceHandler.DestroyResource(this, resourceName, res, context, dontUsePool);
        }

        public bool IsManagedResource(Component res)
        {
            return (res && res.transform.parent == transform);//res.gameObject.GetComponentInParent<CurvyGenerator>() == Generator);RetrieveGenerator
        }


        public List<IPool> GetAllPrefabPools()
        {
            return Generator.PoolManager.FindPools(UniqueIDString + "_");
        }

        public void DeleteAllPrefabPools()
        {
            Generator.PoolManager.DeletePools(UniqueIDString + "_");
        }

        public void Delete()
        {
            OnStateChange();
            gameObject.Destroy(true, true);
        }

        public CGModuleInputSlot GetInputSlot(string name)
        {
            return (InputByName != null && InputByName.ContainsKey(name)) ? InputByName[name] : null;
        }

        public List<CGModuleInputSlot> GetInputSlots(System.Type filterType = null)
        {
            if (filterType == null)
                return new List<CGModuleInputSlot>(Input);
            else
            {
                List<CGModuleInputSlot> res = new List<CGModuleInputSlot>();
                for (int i = 0; i < Output.Count; i++)
                    if (Output[i].Info.DataTypes[0] == filterType || Output[i].Info.DataTypes[0].IsSubclassOf(filterType))
                        res.Add(Input[i]);

                return res;
            }
        }

        public CGModuleOutputSlot GetOutputSlot(string name)
        {
            return (OutputByName != null && OutputByName.ContainsKey(name)) ? OutputByName[name] : null;
        }

        public List<CGModuleOutputSlot> GetOutputSlots(System.Type filterType = null)
        {
            if (filterType == null)
                return new List<CGModuleOutputSlot>(Output);
            else
            {
                List<CGModuleOutputSlot> res = new List<CGModuleOutputSlot>();
                for (int i = 0; i < Output.Count; i++)
                    if (Output[i].Info.DataTypes[0] == filterType || Output[i].Info.DataTypes[0].IsSubclassOf(filterType))
                        res.Add(Output[i]);

                return res;
            }
        }

        public bool GetManagedResources(out List<Component> components, out List<string> resourceNames)
        {
            components = new List<Component>();
            resourceNames = new List<string>();
            FieldInfo[] fields = GetType().GetAllFields(false, true);
            foreach (FieldInfo f in fields)
            {
                CGResourceManagerAttribute at = f.GetCustomAttribute<CGResourceManagerAttribute>();
                if (at != null)
                {
                    if (typeof(ICGResourceCollection).IsAssignableFrom(f.FieldType))
                    {
                        ICGResourceCollection col = f.GetValue(this) as ICGResourceCollection;
                        if (col != null)
                        {
                            Component[] items = col.ItemsArray;
                            foreach (Component component in items)
                            {
                                //component can be null if for example the user delete from the hierarchy a CGMeshResource game object
                                if (component && component.transform.parent == transform)
                                {
                                    components.Add(component);
                                    resourceNames.Add(at.ResourceName);
                                }
                            }
                        }
                    }
                    else
                    {
                        Component component = f.GetValue(this) as Component;
                        if (component && component.transform.parent == transform)
                        {
                            components.Add(component);
                            resourceNames.Add(at.ResourceName);
                        }
                    }
                }
            }

            return (components.Count > 0);
        }



        #endregion

        #region ### Privates, Protected and Internals ###
        /*! \cond PRIVATE */
        /*! @name Internal Public
         *  Don't use them unless you know what you're doing!
         */
        //@{

        /*
        public void CheckAndRaiseOnStateChangedINTERNAL()
        {
            
            bool b = mLastIsConfiguredState;
            Debug.Log(name + " check ="+b+":"+IsConfigured);
            if (IsConfigured != b)
                OnStateChange();
            
        }
         */

        private readonly Dictionary<string, Dictionary<int, string>> resourcesNameCache = new Dictionary<string, Dictionary<int, string>>();

        private string GetResourceName(string resourceName, int index)
        {
            string newName = null;
            bool found;
            if (found = resourcesNameCache.TryGetValue(resourceName, out var dictionary))
                found = dictionary.TryGetValue(index, out newName);
            else
                resourcesNameCache[resourceName] = new Dictionary<int, string>();

            if (found == false)
            {
                newName = index > -1
                    ? string.Format(CultureInfo.InvariantCulture, "{0}_{1}_{2}{3:000}", ModuleName, UniqueIDString, resourceName, index)
                    : string.Format(CultureInfo.InvariantCulture, "{0}_{1}_{2}", ModuleName, UniqueIDString, resourceName);
                resourcesNameCache[resourceName][index] = newName;
            }

            return newName;
        }

        protected void RenameResource(string resourceName, Component resource, int index = -1)
        {
            string newName = GetResourceName(resourceName, index);
            //This check is necessary because when CurvyGenerator.ForceFrequentUpdates is true, this bug happens
            //[FIXED] When a scene has input spline path or input spline shape module, renaming objects from the hierarchy or though the F2 shortcut does not work
            if (resource.name != newName)
                resource.name = newName;
        }

        private static CGModuleLink GetLink(List<CGModuleLink> lst, CGModuleSlot source, CGModuleSlot target)
        {
            for (int i = 0; i < lst.Count; i++)
                if (lst[i].IsSame(source, target))
                    return lst[i];
            return null;
        }

        private static List<CGModuleLink> GetLinks(List<CGModuleLink> lst, CGModuleSlot source)
        {
            List<CGModuleLink> res = new List<CGModuleLink>();
            for (int i = 0; i < lst.Count; i++)
                if (lst[i].IsFrom(source))
                    res.Add(lst[i]);
            return res;
        }

        protected PrefabPool GetPrefabPool(GameObject prefab)
        {
            return Generator.PoolManager.GetPrefabPool(UniqueIDString + "_" + prefab.name, prefab);
        }

        protected bool TryDeleteChildrenFromAssociatedPrefab()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                int childCount = transform.childCount;
                for (int i = 0; i < childCount; i++)
                {
                    Transform item = transform.GetChild(i);
                    if (DTUtility.DoesPrefabStatusAllowDeletion(item.gameObject, out _) == false)
                    {
                        Generator.DeleteAllOutputManagedResourcesFromAssociatedPrefab();
                        return true;
                    }
                }
            }
#endif
            return false;
        }


        public int SetUniqueIdINTERNAL()
        {
            m_UniqueID = ++Generator.m_LastModuleID;
            m_UniqueIDString = null; //invalidate cache
            return m_UniqueID;
        }

        /// <summary>
        /// Initializes SortAncestor with number of connected Input links
        /// </summary>
        //DESIGN inline this in doSortModules()
        internal void initializeSort()
        {
            SortAncestors = 0;
            CircularReferenceError = false;
            //if (Active)
            //{
            for (int i = 0; i < Input.Count; i++)
                if (Input[i].IsLinked)
                    SortAncestors += Input[i].LinkedSlots.Count;
            //}
        }
        /// <summary>
        /// Decrement SortAncestor of linked modules and return a list of childs where SortAncestor==0
        /// </summary>
        /// <returns></returns>
        //DESIGN inline this in doSortModules()
        internal List<CGModule> decrementChilds()
        {
            List<CGModule> noAncestors = new List<CGModule>();
            for (int s = 0; s < Output.Count; s++)
                for (int l = 0; l < Output[s].LinkedSlots.Count; l++)
                {
                    if (--Output[s].LinkedSlots[l].Module.SortAncestors == 0)
                        noAncestors.Add(Output[s].LinkedSlots[l].Module);
                }

            return noAncestors;
        }

        internal void doRefresh()
        {
#if UNITY_EDITOR || CURVY_DEBUG
            DEBUG_LastUpdateTime = System.DateTime.Now;
            DEBUG_ExecutionTime.Start();
#endif

            if (RandomizeSeed)
                Random.InitState(unchecked((int)System.DateTime.Now.Ticks));
            else
                Random.InitState(Seed);
            OnBeforeRefreshEvent(new CurvyCGEventArgs(this));
            Refresh();
            Random.InitState(unchecked((int)System.DateTime.Now.Ticks));

#if UNITY_EDITOR || CURVY_DEBUG
            DEBUG_ExecutionTime.Stop();
#endif
            OnRefreshEvent(new CurvyCGEventArgs(this));

            mDirty = false;
        }

        internal ModuleInfoAttribute getInfo()
        {
            object[] inf = GetType().GetCustomAttributes(typeof(ModuleInfoAttribute), true);
            return (inf.Length > 0) ? (ModuleInfoAttribute)inf[0] : null;
        }

        private bool usesRandom()
        {
            return (Info != null && Info.UsesRandom);
        }

        private void loadSlots()
        {
            // Get list of Slots
            InputByName = new Dictionary<string, CGModuleInputSlot>();
            OutputByName = new Dictionary<string, CGModuleOutputSlot>();
            Input = new List<CGModuleInputSlot>();
            Output = new List<CGModuleOutputSlot>();
            FieldInfo[] fields = GetType().GetAllFields();
            //Debug.Log(name + ".loadSlots()");
            foreach (FieldInfo f in fields)
            {
                if (f.FieldType == typeof(CGModuleInputSlot))
                {
                    CGModuleInputSlot s = (CGModuleInputSlot)f.GetValue(this);
                    s.Module = this;
                    s.Info = getSlotInfo(f);
                    s.ReInitializeLinkedSlots();
                    InputByName.Add(s.Info.Name, s);
                    Input.Add(s);
                }
                else if (f.FieldType == typeof(CGModuleOutputSlot))
                {
                    CGModuleOutputSlot s = (CGModuleOutputSlot)f.GetValue(this);
                    s.Module = this;
                    s.Info = getSlotInfo(f);
                    s.ReInitializeLinkedSlots();
                    OutputByName.Add(s.Info.Name, s);
                    Output.Add(s);
                }
            }
        }

        private SlotInfo getSlotInfo(FieldInfo f)
        {
            SlotInfo si = f.GetCustomAttribute<SlotInfo>();
            if (si != null)
            {
                if (string.IsNullOrEmpty(si.Name))
                    si.Name = f.Name.TrimStart("In").TrimStart("Out");
                for (int x = 0; x < si.DataTypes.Length; x++)
                    if (!si.DataTypes[x].IsSubclassOf(typeof(CGData)))
                        Debug.LogError(string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}, Slot '{1}': Data type needs to be subclass of CGData!", GetType().Name, si.DisplayName));
                return si;
            }
            Debug.LogError("The Slot '" + f.Name + "' of type '" + f.DeclaringType.Name + "' needs a SlotInfo attribute!");
            return null;
        }

        private void setTreeDirtyStateChange()
        {
            mStateChangeDirty = true;
            if (Output != null)
            {
                for (int i = 0; i < Output.Count; i++)
                {
                    if (Output[i].IsLinked)
                    {
                        List<CGModule> modules = Output[i].GetLinkedModules();
                        for (int m = 0; m < modules.Count; m++)
                            //BUG? does the || modules[m].CircularReferenceError create infinite dirtying logique?
                            if (modules[m] != this || modules[m].CircularReferenceError) // prevent circular reference
                                modules[m].setTreeDirtyStateChange();
                    }
                }
            }
        }

        private CurvyGenerator RetrieveGenerator()
        {
            //return GetComponentInParent<CurvyGenerator>();
            return transform.parent != null
                ? transform.parent.GetComponent<CurvyGenerator>()
                : null;
        }

        public void checkOnStateChangedINTERNAL()
        {
            //            Debug.Log(ModuleName+".Check: " + mStateChangeDirty);
            if (mStateChangeDirty)
                OnStateChange();
            mStateChangeDirty = false;
        }


        public void renameManagedResourcesINTERNAL()
        {
            FieldInfo[] fields = GetType().GetAllFields(false, true);
            foreach (FieldInfo f in fields)
            {
                CGResourceManagerAttribute at = f.GetCustomAttribute<CGResourceManagerAttribute>();
                if (at != null)
                {
                    Component cmp = f.GetValue(this) as Component;
                    if (cmp && cmp.transform.parent == this.transform)
                        RenameResource(at.ResourceName, cmp);
                }
            }
        }


        //@}
        /*! \endcond */
        #endregion


    }

    /// <summary>
    /// Attribute defining basic module properties
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public sealed class ModuleInfoAttribute : System.Attribute, System.IComparable
    {
        /// <summary>
        /// Menu-Name of the module (without '/')
        /// </summary>
        public readonly string MenuName;
        /// <summary>
        /// Default Module name
        /// </summary>
        public string ModuleName;
        /// <summary>
        /// Tooltip Info
        /// </summary>
        public string Description;

        /// <summary>
        /// Whether the module uses Random, i.e. should show Seed options
        /// </summary>
        public bool UsesRandom;

        public ModuleInfoAttribute(string name)
        {
            MenuName = name;
        }

        public int CompareTo(object obj)
        {
            return String.Compare(MenuName, ((ModuleInfoAttribute)obj).MenuName, StringComparison.Ordinal);
        }



        //TODO code analysis (CA1036) says that Equal, !=, <, == and > should be defined since IComparable is implemented
    }


    /// <summary>
    /// CGModule helper class
    /// </summary>
    [System.Serializable]
    public class CGModuleProperties
    {
        public Rect Dimensions;
#if UNITY_EDITOR
        public AnimBool Expanded;
#endif
        public float MinWidth = 250;
        public float LabelWidth;
        public Color BackgroundColor = Color.black;

        public CGModuleProperties()
        {
#if UNITY_EDITOR
            Expanded = new AnimBool(true);
            Expanded.speed = 3;
#endif
        }
    }


}