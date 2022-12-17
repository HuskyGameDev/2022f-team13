// =====================================================================
// Copyright 2013-2017 Fluffy Underware
// All rights reserved
// 
// http://www.fluffyunderware.com
// =====================================================================

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using FluffyUnderware.DevTools.Extensions;
using JetBrains.Annotations;

namespace FluffyUnderware.DevTools
{
    /// <summary>
    /// A pool of instances of a prefab GameObject
    /// </summary>
    [RequireComponent(typeof(PoolManager))]
    [HelpURL(DTUtility.HelpUrlBase + "dtprefabpool")]
    public class PrefabPool : DTVersionedMonoBehaviour, IPool
    {
        [FieldCondition(nameof(m_Identifier), "", false, ActionAttribute.ActionEnum.ShowWarning, "Please enter an identifier! (Select a prefab to set automatically)")]
        [SerializeField]
        string m_Identifier = String.Empty;

        [SerializeField]
        List<GameObject> m_Prefabs = new List<GameObject>();

        [Inline]
        [SerializeField]
        PoolSettings m_Settings;



        PoolManager mManager;

        public string Identifier
        {
            get { return m_Identifier; }
            set
            {
                if (m_Identifier != value)
                {
                    m_Identifier = value;
                }
            }

        }
        public List<GameObject> Prefabs
        {
            get { return m_Prefabs; }
            set
            {
                if (m_Prefabs != value)
                    m_Prefabs = value;
            }
        }


        public PoolSettings Settings
        {
            get { return m_Settings; }
            set
            {
                if (m_Settings != value)
                    m_Settings = value;
                if (m_Settings != null)
                    m_Settings.OnValidate();
            }
        }

        [Obsolete("This member is no more used by Curvy Splines, and will get removed in a future update. Use GetComponent<PoolManager>() instead")]
        public PoolManager Manager
        {
            get
            {
                if (mManager == null)
                    mManager = GetComponent<PoolManager>();
                return mManager;
            }
        }

        List<GameObject> mObjects = new List<GameObject>();

        double mLastTime;
        double mDeltaTime;

        void Start()
        {
            if (Settings.Prewarm)
                Reset();
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            Settings = m_Settings;
            if (m_Identifier == String.Empty && Prefabs.Any(p => p != null))
                Identifier = Prefabs.First().name;
        }
#endif

        public void Initialize([NotNull] string ident, PoolSettings settings, params GameObject[] prefabs)
        {
            Identifier = ident;
            m_Settings = settings;
            Prefabs = new List<GameObject>(prefabs);
            mLastTime = DTTime.TimeSinceStartup + UnityEngine.Random.Range(0, Settings.Speed);
            if (Settings.Prewarm)
                Reset();
        }

        public void Update()
        {
            if (Application.isPlaying)
            {
                mDeltaTime += DTTime.TimeSinceStartup - mLastTime;
                mLastTime = DTTime.TimeSinceStartup;

                if (Settings.Speed > 0)
                {
                    int c = (int)(mDeltaTime / Settings.Speed);
                    mDeltaTime -= c;

                    if (Count > Settings.Threshold)
                    {
                        c = Mathf.Min(c, Count - Settings.Threshold);
                        while (c-- > 0)
                        {
                            if (Settings.Debug)
                                log("Threshold exceeded: Deleting item");
                            destroy(mObjects[0]);
                            mObjects.RemoveAt(0);
                        }
                    }
                    else if (Count < Settings.MinItems)
                    {
                        c = Mathf.Min(c, Settings.MinItems - Count);
                        while (c-- > 0)
                        {
                            if (Settings.Debug)
                                log("Below MinItems: Adding item");
                            mObjects.Add(create());
                        }
                    }
                }
                else
                    mDeltaTime = 0;
            }
        }

        public void Reset()
        {
            if (Application.isPlaying)
            {
                while (Count < Settings.MinItems)
                {
                    mObjects.Add(create());
                }
                while (Count > Settings.Threshold)
                {
                    destroy(mObjects[0]);
                    mObjects.RemoveAt(0);
                }
                if (Settings.Debug)
                    log("Prewarm/Reset");
            }
        }

        public void Clear()
        {
            if (Settings.Debug)
                log("Clear");
            for (int i = 0; i < Count; i++)
                destroy(mObjects[i]);
            mObjects.Clear();
        }

        public int Count
        {
            get { return mObjects.Count; }
        }

        public GameObject Pop(Transform parent = null)
        {
            GameObject item = null;
            if (Count > 0)
            {
                item = mObjects[0];
                mObjects.RemoveAt(0);
            }
            else
            {
                if (Settings.AutoCreate || !Application.isPlaying)
                {
                    if (Settings.Debug)
                        log("Auto create item");
                    item = create();
                }
            }
            if (item)
            {
                item.hideFlags = HideFlags.None;
                item.transform.parent = parent;
                if (Settings.AutoEnableDisable)
                    item.SetActive(true);
                sendAfterPop(item);
                if (Settings.Debug)
                    log("Pop " + item);
            }

            return item;
        }

        public virtual void Push(GameObject item)
        {
            if (Settings.Debug)
                log("Push " + item);
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                item.Destroy(false, true);
            }
            else
#endif
            if (item != null)
            {
                sendBeforePush(item);
                mObjects.Add(item);
                item.transform.parent = transform;
                item.hideFlags = (Settings.Debug) ? HideFlags.DontSave : HideFlags.HideAndDontSave;
                if (Settings.AutoEnableDisable)
                    item.SetActive(false);
            }
        }

        GameObject create()
        {
            if (Prefabs.Count == 0)
                throw new InvalidOperationException(String.Format("[Curvy] The Prefab Pool '{0}' in game object '{1}' could not create a pool element because its Prefabs list is empty", this.Identifier, this.gameObject.name));

            //TODO should this Random.Range call be deterministic?
            GameObject prefab = Prefabs[UnityEngine.Random.Range(0, Prefabs.Count)];
            GameObject result;
            {
#if UNITY_EDITOR
                bool isPrefabAsset;
                //We have to check GetPrefabInstanceStatus first, because GetPrefabAssetType returns (as far as I understand) the same thing for both a prefab asset and a prefab instance
                if (PrefabUtility.GetPrefabInstanceStatus(prefab) == PrefabInstanceStatus.NotAPrefab)
                {
                    PrefabAssetType prefabAssetType = PrefabUtility.GetPrefabAssetType(prefab);
                    isPrefabAsset = prefabAssetType == PrefabAssetType.Regular || prefabAssetType == PrefabAssetType.Variant;
                }
                else
                    isPrefabAsset = false;

                result = isPrefabAsset
                    ? PrefabUtility.InstantiatePrefab(prefab) as GameObject
                    : Instantiate(prefab);
#else
                result = Instantiate(prefab);
#endif
            }

            result.name = prefab.name;
            result.transform.parent = transform;
            if (Settings.AutoEnableDisable)
                result.SetActive(false);
            return result;
        }

        void destroy(GameObject go)
        {
            go.Destroy(false, true);
        }

        void log(string msg)
        {
            Debug.Log(string.Format("[{0}] ({1} items) {2}", Identifier, Count, msg));
        }

        void setParent(Transform item, Transform parent)
        {
            if (item != null)
                item.parent = parent;
        }

        void sendAfterPop(GameObject item)
        {
            item.SendMessage(nameof(IPoolable.OnAfterPop), SendMessageOptions.DontRequireReceiver);
        }

        void sendBeforePush(GameObject item)
        {
            item.SendMessage(nameof(IPoolable.OnBeforePush), SendMessageOptions.DontRequireReceiver);
        }


    }


}
