// =====================================================================
// Copyright 2013-2022 ToolBuddy
// All rights reserved
// 
// http://www.toolbuddy.net
// =====================================================================

using System;
using UnityEngine;
using System.Collections;
using FluffyUnderware.DevTools;
using FluffyUnderware.DevTools.Extensions;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace FluffyUnderware.Curvy.Generator
{
    /// <summary>
    /// Mesh Resource Component used by Curvy Generator
    /// </summary>
    [RequireComponent(typeof(MeshRenderer))]
    [HelpURL(CurvySpline.DOCLINK + "cgmeshresource")]
    public class CGMeshResource : DuplicateEditorMesh, IPoolable
    {
        /// <summary>
        /// The value of the "Everything" entry in a <see cref="MeshCollider.cookingOptions"/>'s inspector
        /// </summary>
        public const MeshColliderCookingOptions EverMeshColliderCookingOptions = 
            MeshColliderCookingOptions.EnableMeshCleaning|
            MeshColliderCookingOptions.CookForFasterSimulation|
            MeshColliderCookingOptions.UseFastMidphase|
            MeshColliderCookingOptions.WeldColocatedVertices;

        private MeshRenderer mRenderer;
        private Collider mCollider;

        public MeshRenderer Renderer
        {
            get
            {
                if (mRenderer == null)
                    mRenderer = GetComponent<MeshRenderer>();
                return mRenderer;
            }
        }

        public Collider Collider
        {
            get
            {
                if (mCollider == null)
                    mCollider = GetComponent<Collider>();
                return mCollider;
            }

        }

        public Mesh Prepare()
        {
            return Filter.PrepareNewShared();
        }

        public bool ColliderMatches(CGColliderEnum type)
        {
            if (Collider == null && type == CGColliderEnum.None)
                return true;
            if (Collider is MeshCollider && type == CGColliderEnum.Mesh)
                return true;
            if (Collider is BoxCollider && type == CGColliderEnum.Box)
                return true;
            if (Collider is SphereCollider && type == CGColliderEnum.Sphere)
                return true;
            if (Collider is CapsuleCollider && type == CGColliderEnum.Capsule)
                return true;

            return false;
        }

        public void RemoveCollider()
        {
            if (Collider)
            {
                mCollider.Destroy(false, false);
                mCollider = null;
            }
        }

        /// <summary>
        /// Updates the collider if existing, and create a new one if not.
        /// </summary>
        /// <param name="mode">The collider's type</param>
        /// <param name="convex">Used only when mode is CGColliderEnum.Mesh</param>
        /// <param name="isTrigger">Is the collider a Trigger</param>
        /// <param name="material">The collider's material</param>
        /// <param name="meshCookingOptions">Used only when mode is CGColliderEnum.Mesh</param>
        /// <returns></returns>
        public bool UpdateCollider(CGColliderEnum mode, bool convex, bool isTrigger, PhysicMaterial material
            , MeshColliderCookingOptions meshCookingOptions = EverMeshColliderCookingOptions
            )
        {
            if (Collider == null)
                switch (mode)
                {
                    case CGColliderEnum.Mesh:
                        mCollider = gameObject.AddComponent<MeshCollider>();
                        break;
                    case CGColliderEnum.Box:
                        mCollider = gameObject.AddComponent<BoxCollider>();
                        break;
                    case CGColliderEnum.Sphere:
                        mCollider = gameObject.AddComponent<SphereCollider>();
                        break;
                    case CGColliderEnum.Capsule:
                        mCollider = gameObject.AddComponent<CapsuleCollider>();
                        break;
                    case CGColliderEnum.None:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

            if (mode != CGColliderEnum.None)
            {
                switch (mode)
                {
                    case CGColliderEnum.Mesh:
                        MeshCollider meshCollider = Collider as MeshCollider;
                        if (meshCollider != null)
                        {
                            meshCollider.sharedMesh = null;
                            meshCollider.convex = convex;
                            meshCollider.isTrigger = isTrigger;
                            meshCollider.cookingOptions = meshCookingOptions;
                            try
                            {
                                meshCollider.sharedMesh = Filter.sharedMesh;
                            }
#if CURVY_SANITY_CHECKS
                            catch (Exception e)
                            {
                                DTLog.LogException(e, this);
#else
                            catch
                            {
#endif
                                return false;
                            }
                        }
                        else
                            DTLog.LogError("[Curvy] Collider of wrong type", this);
                        break;
                    case CGColliderEnum.Box:
                        BoxCollider boxCollider = Collider as BoxCollider;
                        if (boxCollider != null)
                        {
                            boxCollider.isTrigger = isTrigger;
                            boxCollider.center = Filter.sharedMesh.bounds.center;
                            boxCollider.size = Filter.sharedMesh.bounds.size;
                        }
                        else
                            DTLog.LogError("[Curvy] Collider of wrong type", this);
                        break;
                    case CGColliderEnum.Sphere:
                        SphereCollider sphereCollider = Collider as SphereCollider;
                        if (sphereCollider != null)
                        {
                            sphereCollider.isTrigger = isTrigger;
                            sphereCollider.center = Filter.sharedMesh.bounds.center;
                            sphereCollider.radius = Filter.sharedMesh.bounds.extents.magnitude;
                        }
                        else
                            DTLog.LogError("[Curvy] Collider of wrong type", this);
                        break;
                    case CGColliderEnum.Capsule:
                        CapsuleCollider capsuleCollider = Collider as CapsuleCollider;
                        if (capsuleCollider != null)
                        {
                            Bounds sharedMeshBounds = Filter.sharedMesh.bounds;
                            capsuleCollider.isTrigger = isTrigger;
                            capsuleCollider.center = sharedMeshBounds.center;
                            capsuleCollider.radius = new Vector2(sharedMeshBounds.extents.x, sharedMeshBounds.extents.y).magnitude;
                            capsuleCollider.height = sharedMeshBounds.size.z;
                            capsuleCollider.direction = 2;//Z
                        }
                        else
                            DTLog.LogError("[Curvy] Collider of wrong type", this);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                Collider.material = material;
            }

            return true;
        }

        public void OnBeforePush()
        {
        }

        public void OnAfterPop()
        {
        }
    }

    /// <summary>
    /// Collection of Mesh Resources
    /// </summary>
    [System.Serializable]
    public class CGMeshResourceCollection : ICGResourceCollection
    {
        public List<CGMeshResource> Items = new List<CGMeshResource>();

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