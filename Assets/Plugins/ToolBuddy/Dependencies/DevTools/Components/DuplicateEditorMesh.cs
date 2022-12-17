// =====================================================================
// Copyright 2013-2017 Fluffy Underware
// All rights reserved
// 
// http://www.fluffyunderware.com
// =====================================================================

using UnityEngine;
using System.Collections;


namespace FluffyUnderware.DevTools
{
    /// <summary>
    /// Add this script to a GameObject with a MeshFilter to ensure it will be properly duplicated in the editor!
    /// </summary>
    /// <remarks>On Duplicating, Awake() checks if the sharedMesh is already used in the scene. If yes, a new mesh will be created to ensure that each sharedMesh is unique</remarks>
    [ExecuteAlways]
    [RequireComponent(typeof(MeshFilter))]
    public abstract class DuplicateEditorMesh : DTVersionedMonoBehaviour
    {
        MeshFilter mFilter;

        public MeshFilter Filter
        {
            get {
                if (mFilter == null)
                    mFilter = GetComponent<MeshFilter>();
                return mFilter; 
            }
        }

        protected virtual void Awake()
        {
            if (!Application.isPlaying)
            {
                MeshFilter meshFilter = Filter;
                if (meshFilter && meshFilter.sharedMesh != null)
                {
                    DuplicateEditorMesh[] otherWatchdogs = GameObject.FindObjectsOfType<DuplicateEditorMesh>();
                    foreach (DuplicateEditorMesh dog in otherWatchdogs)
                    {
                        if (dog != this)
                        {
                            MeshFilter otherMF = dog.Filter;
                            if (otherMF && otherMF.sharedMesh == meshFilter.sharedMesh)
                            {
                                Mesh m = new Mesh();
                                m.name = otherMF.sharedMesh.name;
                                meshFilter.mesh = m;
                            }
                        }
                    }
                }
            }
        }
    }
}
