// =====================================================================
// Copyright 2013-2017 Fluffy Underware
// All rights reserved
// 
// http://www.fluffyunderware.com
// =====================================================================

using System.Collections.Generic;
using System.Linq;
using FluffyUnderware.DevTools.Extensions;
using UnityEngine;

namespace FluffyUnderware.DevTools
{

    public class DTSingleton<T> : MonoBehaviour, IDTSingleton where T : MonoBehaviour, IDTSingleton
    {
        static T _instance;
        static readonly object _lock = new object();
        static bool applicationIsQuitting = false;
        bool isDuplicateInstance = false;

        public static bool HasInstance
        {
            get { return _instance != null; }
        }

        public static T Instance
        {
            get
            {
                if (!Application.isPlaying)
                    applicationIsQuitting = false;
                if (applicationIsQuitting)
                    return null;

                if (_instance == null)
                    lock (_lock)
                        if (_instance == null)
                        {
                            IEnumerable<Object> objectsOfTypeT = FindObjectsOfType(typeof(T)).Where(o => o != null);
#if CURVY_SANITY_CHECKS
                            if (objectsOfTypeT.Count() != FindObjectsOfType(typeof(T)).Length)
                                DTLog.LogError("[Curvy] DTSingleton.Instance : FindObjectsOfType " + typeof(T).FullName + " returned at least one null reference");
#endif                              

                            _instance = objectsOfTypeT.Any()
                                ? (T)objectsOfTypeT.ElementAt(0)
                                : new GameObject().AddComponent<T>();
                        }

                return _instance;
            }
        }

        public virtual void Awake()
        {
            T instance = Instance;
            lock (_lock)
            {
                if (instance == null)
                    DTLog.LogError("[DevTools] DTSingleton instance was null. This happens if the instance is destroyed by the user, which is not a supported operation. If you did not destroy the singleton's instance, then please fill a bug report.", this);
                else
                    if (GetInstanceID() != instance.GetInstanceID())
                    {
                        instance.MergeDoubleLoaded((IDTSingleton)this);
                        this.isDuplicateInstance = true;
                        this.Invoke(nameof(DestroySelf), 0);
                    }
            }
        }

        protected virtual void OnDestroy()
        {
            lock (_lock)
                if (Application.isPlaying && !isDuplicateInstance)
                {
                    applicationIsQuitting = true;
                    _instance = null;
                }
        }

        public virtual void MergeDoubleLoaded(IDTSingleton newInstance)
        {
        }

        void DestroySelf()
        {
            gameObject.Destroy(false, true);
        }

    }

    public interface IDTSingleton
    {
        void MergeDoubleLoaded(IDTSingleton newInstance);
    }
}
