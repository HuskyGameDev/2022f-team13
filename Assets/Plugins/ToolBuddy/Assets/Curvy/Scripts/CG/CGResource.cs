// =====================================================================
// Copyright 2013-2022 ToolBuddy
// All rights reserved
// 
// http://www.toolbuddy.net
// =====================================================================

using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using System.Reflection;
using FluffyUnderware.Curvy.Shapes;
using FluffyUnderware.DevTools;
using FluffyUnderware.DevTools.Extensions;
using JetBrains.Annotations;
using UnityEngine.Assertions;

namespace FluffyUnderware.Curvy.Generator
{
    /// <summary>
    /// Resource attribute
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public sealed class ResourceLoaderAttribute : System.Attribute
    {
        public readonly string ResourceName;

        public ResourceLoaderAttribute([NotNull] string resName)
        {
            ResourceName = resName;
        }
    }

    /// <summary>
    /// Resource Helper class used by Curvy Generator
    /// </summary>
    public static class CGResourceHandler
    {
        private static readonly Dictionary<string, ICGResourceLoader> Loader = new Dictionary<string, ICGResourceLoader>();

        public static Component CreateResource(CGModule module, [NotNull] string resName, [NotNull] string context)
        {
            if (Loader.Count == 0)
                getLoaders();
            if (Loader.ContainsKey(resName))
            {
                ICGResourceLoader loader = Loader[resName];
                return loader.Create(module, context);
            }
            else
            {
                Debug.LogError("CGResourceHandler: Missing Loader for resource '" + resName + "'");
                return null;
            }

        }

        public static void DestroyResource(CGModule module, [NotNull] string resName, Component obj, [NotNull] string context, bool kill)
        {
            if (Loader.Count == 0)
                getLoaders();
            if (Loader.ContainsKey(resName))
            {
                ICGResourceLoader loader = Loader[resName];
                loader.Destroy(module, obj, context, kill);
            }
            else
                Debug.LogError("CGResourceHandler: Missing Loader for resource '" + resName + "'");
        }

        private static void getLoaders()
        {
#if UNITY_EDITOR
            TypeCache.TypeCollection types = TypeCache.GetTypesDerivedFrom<ICGResourceLoader>();
#else
            Type[] types = TypeExt.GetLoadedTypes();
            Type ICGResourceLoaderType = typeof(ICGResourceLoader);
#endif

            foreach (Type T in types)
            {
#if UNITY_EDITOR == false
                if (ICGResourceLoaderType.IsAssignableFrom(T) && ICGResourceLoaderType != T)
#endif
                {
                    object[] attributes = (object[])T.GetCustomAttributes(typeof(ResourceLoaderAttribute), true);
                    if (attributes.Length > 0)
                    {
                        ICGResourceLoader o = (ICGResourceLoader)System.Activator.CreateInstance(T);
                        if (o != null)
                            Loader.Add(((ResourceLoaderAttribute)attributes[0]).ResourceName, o);
                    }
                    else
                    {
                        DTLog.LogError(String.Format("[Curvy] Could not register resource loader of type {0} because it does not have a ResourceLoader attribute", T.FullName));
                    }
                }
            }
        }
    }

    /// <summary>
    /// Spline resource loader class
    /// </summary>
    [ResourceLoader("Spline")]
    public class CGSplineResourceLoader : ICGResourceLoader
    {
        public Component Create(CGModule cgModule, string context)
        {
            CurvySpline spl = CurvySpline.Create();
            spl.transform.position = Vector3.zero;
            spl.Closed = true;
            spl.Add(new Vector3(0, 0, 0), new Vector3(5, 0, 10), new Vector3(-5, 0, 10));
            return spl;
        }

        public void Destroy(CGModule cgModule, Component obj, string context, bool kill)
        {
            if (obj != null)
                obj.gameObject.Destroy(false, false);//isUndoable is set to false because that's how it was working before. Try make this operation undoable if needed
        }
    }

    /// <summary>
    /// Shape (2D spline) resource loader class
    /// </summary>
    [ResourceLoader("Shape")]
    public class CGShapeResourceLoader : ICGResourceLoader
    {

        public Component Create(CGModule cgModule, string context)
        {
            CurvySpline spl = CurvySpline.Create();
            spl.transform.position = Vector3.zero;
            spl.RestrictTo2D = true;
            spl.Closed = true;
            spl.Orientation = CurvyOrientation.None;
            spl.gameObject.AddComponent<CSCircle>().Refresh();
            return spl;
        }

        public void Destroy(CGModule cgModule, Component obj, string context, bool kill)
        {
            if (obj != null)
                obj.gameObject.Destroy(false, false);//isUndoable is set to false because that's how it was working before. Try make this operation undoable if needed
        }
    }

    /// <summary>
    /// Mesh resource loader class
    /// </summary>
    [ResourceLoader("Mesh")]
    public class CGMeshResourceLoader : ICGResourceLoader
    {
        public Component Create(CGModule cgModule, string context)
        {
            Component cmp = cgModule.Generator.PoolManager.GetComponentPool<CGMeshResource>().Pop();
            return cmp;
        }

        public void Destroy(CGModule cgModule, Component obj, string context, bool kill)
        {
#if CURVY_SANITY_CHECKS
            Assert.IsTrue(obj.GetComponent<CGMeshResource>() != null);
#endif
            if (obj != null)
            {
                if (kill)
                {
                    obj.gameObject.Destroy(false, false);
                }
                else
                {
                    obj.StripComponents(typeof(CGMeshResource), typeof(MeshFilter), typeof(MeshRenderer));
                    //OPTIM should we assign null to sharedMesh, so it can be garbage collected? It seems (need deeper investiguation) safe since every time we pop a CGMeshResource from the pool, the following code clears the shared mesh if it exists. And if you put this optim in prod, make sure the retrieval of a CGMeshResource from the pool and its initialization are done in the same atomic operation, and not like now in two separate methods
                    //obj.GetComponent<MeshFilter>().sharedMesh = null;
                    cgModule.Generator.PoolManager.GetComponentPool<CGMeshResource>().Push(obj);
                }
            }
        }
    }

    /// <summary>
    /// GameObject resource loader class
    /// </summary>
    [ResourceLoader("GameObject")]
    public class CGGameObjectResourceLoader : ICGResourceLoader
    {
        public Component Create(CGModule cgModule, string context)
        {
            GameObject go = cgModule.Generator.PoolManager.GetPrefabPool(context).Pop();
            return go ? go.transform : null;
        }

        public void Destroy(CGModule cgModule, Component obj, string context, bool kill)
        {
            if (obj != null)
            {
                if (kill)
                {
                    obj.gameObject.Destroy(false, false);
                }
                else
                {
                    cgModule.Generator.PoolManager.GetPrefabPool(context).Push(obj.gameObject);
                }
            }
        }
    }

    /// <summary>
    /// Collection of GameObject resources
    /// </summary>
    [System.Serializable]
    public class CGGameObjectResourceCollection : ICGResourceCollection
    {
        public List<Transform> Items = new List<Transform>();
        public List<string> PoolNames = new List<string>();

        public int Count
        {
            get
            {
                return Items.Count;
            }
        }

        public Component[] ItemsArray
        {
            get { return Items.ToArray(); }
        }
    }

}