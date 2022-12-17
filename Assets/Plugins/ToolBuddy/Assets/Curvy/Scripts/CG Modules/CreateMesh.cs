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
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using ToolBuddy.Pooling.Pools;
using FluffyUnderware.Curvy;
using FluffyUnderware.Curvy.Pools;
using FluffyUnderware.DevTools.Extensions;
using FluffyUnderware.Curvy.Utils;
using FluffyUnderware.DevTools;
using JetBrains.Annotations;
using ToolBuddy.Pooling.Collections;
using UnityEngine.Assertions;
using UnityEngine.Rendering;

namespace FluffyUnderware.Curvy.Generator.Modules
{
    [ModuleInfo("Create/Mesh", ModuleName = "Create Mesh")]
    [HelpURL(CurvySpline.DOCLINK + "cgcreatemesh")]
    public class CreateMesh : ResourceExportingModule, ISerializationCallbackReceiver
    {
        /// <summary>
        /// The default value of Tag of created objects
        /// </summary>
        private const string DefaultTag = "Untagged";


        [HideInInspector]
        [InputSlotInfo(typeof(CGVMesh), Array = true, Name = "VMesh")]
        public CGModuleInputSlot InVMeshArray = new CGModuleInputSlot();

        [HideInInspector]
        [InputSlotInfo(typeof(CGSpots), Array = true, Name = "Spots", Optional = true)]
        public CGModuleInputSlot InSpots = new CGModuleInputSlot();

        /// <summary>
        /// The created meshes at the last update (call to Refresh). This list is not maintained outside of module updates, so if a user manually deletes one of the created meshes, its entry in this list will still be there, but with a null value (since deleted objects are equal to null in Unity's world) 
        /// </summary>
        [SerializeField, CGResourceCollectionManager("Mesh", ShowCount = true)]
        private CGMeshResourceCollection m_MeshResources = new CGMeshResourceCollection();

        #region ### Serialized Fields ###

        [Tab("General")]

        [Tooltip("Merge meshes")]
        [FieldCondition(nameof(CanUpdate), true, Action = ConditionalAttribute.ActionEnum.Enable)]
        [SerializeField]
        private bool m_Combine;

        [SerializeField]
        [Tooltip("Warning: this operation is Editor only (not available in builds) and CPU intensive.\nWhen combining multiple meshes, the UV2s are by default kept as is. Use this option to recompute them by uwrapping the combined mesh.")]
        [FieldCondition(nameof(m_Combine), true, Action = ConditionalAttribute.ActionEnum.Show)]
        private bool unwrapUV2;

        [Tooltip("When Combine is true, combine only meshes sharing the same index\nIs used only if Spots are provided")]
#if UNITY_EDITOR
        [FieldCondition(nameof(m_Combine), true, false, Action = ConditionalAttribute.ActionEnum.Show)]
        [FieldCondition(nameof(CanUpdate), true, false, ConditionalAttribute.OperatorEnum.AND, nameof(CanGroupMeshes), true, false, Action = ConditionalAttribute.ActionEnum.Enable)]
#endif
        [SerializeField]
        private bool m_GroupMeshes = false;

        [SerializeField]
        [Tooltip("If true, the generated mesh will have normals")]
        private bool includeNormals = true;

        [SerializeField]
        [Tooltip("If true, the generated mesh will have tangents")]
        private bool includeTangents = false;

        [SerializeField, HideInInspector]
        [FieldCondition(nameof(CanUpdate), true, Action = ConditionalAttribute.ActionEnum.Enable)]
        private CGYesNoAuto m_AddNormals = CGYesNoAuto.Auto;

        [SerializeField, HideInInspector]
        [FieldCondition(nameof(CanUpdate), true, Action = ConditionalAttribute.ActionEnum.Enable)]
        private CGYesNoAuto m_AddTangents = CGYesNoAuto.No;

        [SerializeField, HideInInspector]
        [FieldCondition(nameof(CanUpdate), true, Action = ConditionalAttribute.ActionEnum.Enable)]
        private bool m_AddUV2 = true;

        [SerializeField]
        [Tooltip("If enabled, meshes will have the Static flag set, and will not be generated/updated in Play Mode")]
        [FieldCondition(nameof(CanModifyStaticFlag), true, Action = ConditionalAttribute.ActionEnum.Enable)]
        private bool m_MakeStatic;

        [SerializeField]
        [Tooltip("The Layer of the created game object")]
        [FieldCondition(nameof(CanUpdate), true, Action = ConditionalAttribute.ActionEnum.Enable)]
        [Layer]
        private int m_Layer;

        [SerializeField]
        [Tooltip("The Tag of the created game object")]
        [FieldCondition(nameof(CanUpdate), true, Action = ConditionalAttribute.ActionEnum.Enable)]
        [Tag]
        private string m_Tag = DefaultTag;

        [Tab("Renderer")]
        [SerializeField]
        [FieldCondition(nameof(CanUpdate), true, Action = ConditionalAttribute.ActionEnum.Enable)]
        private bool m_RendererEnabled = true;

        [SerializeField]
        [FieldCondition(nameof(CanUpdate), true, Action = ConditionalAttribute.ActionEnum.Enable)]
        private ShadowCastingMode m_CastShadows = ShadowCastingMode.On;

        [SerializeField]
        [FieldCondition(nameof(CanUpdate), true, Action = ConditionalAttribute.ActionEnum.Enable)]
        private bool m_ReceiveShadows = true;

        [SerializeField]
        [FieldCondition(nameof(CanUpdate), true, Action = ConditionalAttribute.ActionEnum.Enable)]
        private LightProbeUsage m_LightProbeUsage = LightProbeUsage.BlendProbes;

        [HideInInspector]
        [SerializeField]
        [FieldCondition(nameof(CanUpdate), true, Action = ConditionalAttribute.ActionEnum.Enable)]
        private bool m_UseLightProbes = true;


        [SerializeField]
        [FieldCondition(nameof(CanUpdate), true, Action = ConditionalAttribute.ActionEnum.Enable)]
        private ReflectionProbeUsage m_ReflectionProbes = ReflectionProbeUsage.BlendProbes;
        [SerializeField]
        [FieldCondition(nameof(CanUpdate), true, Action = ConditionalAttribute.ActionEnum.Enable)]
        private Transform m_AnchorOverride;

        [Tab("Collider")]
        [SerializeField]
        [FieldCondition(nameof(CanUpdate), true, Action = ConditionalAttribute.ActionEnum.Enable)]
        private CGColliderEnum m_Collider = CGColliderEnum.Mesh;

        [FieldCondition(nameof(m_Collider), CGColliderEnum.Mesh)]
        [SerializeField]
        [FieldCondition(nameof(CanUpdate), true, Action = ConditionalAttribute.ActionEnum.Enable)]
        private bool m_Convex;

        [SerializeField]
        [FieldCondition(nameof(EnableIsTrigger), true, Action = ConditionalAttribute.ActionEnum.Enable)]
        private bool m_IsTrigger;

        [Tooltip("Options used to enable or disable certain features in Collider mesh cooking. See Unity's MeshCollider.cookingOptions for more details")]
        [FieldCondition(nameof(m_Collider), CGColliderEnum.Mesh)]
        [SerializeField]
        [EnumFlag]
        [FieldCondition(nameof(CanUpdate), true, Action = ConditionalAttribute.ActionEnum.Enable)]
        private MeshColliderCookingOptions m_CookingOptions = CGMeshResource.EverMeshColliderCookingOptions;

#if UNITY_EDITOR
        [FieldCondition(nameof(CanUpdate), true, false, ConditionalAttribute.OperatorEnum.AND, "m_Collider", CGColliderEnum.None, true, Action = ConditionalAttribute.ActionEnum.Enable)]
#endif
        [Label("Auto Update")]
        [SerializeField]
        private bool m_AutoUpdateColliders = true;

#if UNITY_EDITOR
        [FieldCondition(nameof(CanUpdate), true, false, ConditionalAttribute.OperatorEnum.AND, "m_Collider", CGColliderEnum.None, true, Action = ConditionalAttribute.ActionEnum.Enable)]
#endif
        [SerializeField]
        private PhysicMaterial m_Material;

        #endregion

        #region ### Public Properties ###

        #region --- General ---
        public bool Combine
        {
            get { return m_Combine; }
            set
            {
                if (m_Combine != value)
                    m_Combine = value;
                Dirty = true;
            }
        }

        /// <summary>
        /// Warning: this operation is Editor only (not available in builds) and CPU intensive
        /// When combining multiple meshes, the UV2s are by default kept as is. Use this option to recompute them by uwrapping the combined mesh.
        /// </summary>
        public bool UnwrapUV2
        {
            get
            {
#if UNITY_EDITOR == false
            DTLog.Log("[Curvy] UV2 Unwrapping is not available outside of the editor", this);    
#endif
                return unwrapUV2;
            }
            set
            {
#if UNITY_EDITOR == false
            DTLog.Log("[Curvy] UV2 Unwrapping is not available outside of the editor", this);    
#endif
                if (unwrapUV2 != value)
                {
                    unwrapUV2 = value;
                    Dirty = true;
                }
            }
        }

        /// <summary>
        /// When Combine is true, combine only meshes sharing the same index.
        /// </summary>
        /// <remarks>Is used only if <see cref="InSpots"/> is not empty</remarks>
        /// <remarks>Please keep in mind that meshes provided by the <see cref="DeformMesh"/> module do not share the same index.</remarks>
        public bool GroupMeshes
        {
            get { return m_GroupMeshes; }
            set
            {
                if (m_GroupMeshes != value)
                    m_GroupMeshes = value;
                Dirty = true;
            }
        }

        /// <summary>
        /// If true, the generated mesh will have normals
        /// </summary>
        public bool IncludeNormals
        {
            get { return includeNormals; }
            set
            {
                if (includeNormals != value)
                    includeNormals = value;
                Dirty = true;
            }
        }

        /// <summary>
        /// If true, the generated mesh will have tangents
        /// </summary>
        public bool IncludeTangents
        {
            get { return includeTangents; }
            set
            {
                if (includeTangents != value)
                    includeTangents = value;
                Dirty = true;
            }
        }

        [Obsolete("Use IncludeNormals instead")]
        public CGYesNoAuto AddNormals
        {
            get { return m_AddNormals; }
            set
            {
                if (m_AddNormals != value)
                    m_AddNormals = value;
                Dirty = true;
            }
        }

        [Obsolete("Use IncludeTangents instead")]
        public CGYesNoAuto AddTangents
        {
            get { return m_AddTangents; }
            set
            {
                if (m_AddTangents != value)
                    m_AddTangents = value;
                Dirty = true;
            }
        }

        [Obsolete("UV2 is now always added")]
        public bool AddUV2
        {
            get { return m_AddUV2; }
            set
            {
                if (m_AddUV2 != value)
                    m_AddUV2 = value;
                Dirty = true;
            }
        }


        public int Layer
        {
            get { return m_Layer; }
            set
            {
                int v = Mathf.Clamp(value, 0, 32);
                if (m_Layer != v)
                    m_Layer = v;
                Dirty = true;
            }
        }

        public string Tag
        {
            get { return m_Tag; }
            set
            {
                if (m_Tag != value)//TODO get rid of value comparison in all properties, or at least add the Dirty = true line inside the if
                    m_Tag = value;
                Dirty = true;
            }
        }

        /// <summary>
        /// If enabled, meshes will have the Static flag set, and will not be generated/updated in Play Mode. This is to maintain Unity optimizations done in Edit Mode to static GameObjects.
        /// </summary>
        /// <remarks>Do not set to true if you rely on Play Mode generated meshes</remarks>
        public bool MakeStatic
        {
            get { return m_MakeStatic; }
            set
            {
                if (m_MakeStatic != value)
                    m_MakeStatic = value;
                Dirty = true;
            }
        }
        #endregion

        #region --- Renderer ---
        public bool RendererEnabled
        {
            get { return m_RendererEnabled; }
            set
            {
                if (m_RendererEnabled != value)
                    m_RendererEnabled = value;
                Dirty = true;
            }
        }

        public ShadowCastingMode CastShadows
        {
            get { return m_CastShadows; }
            set
            {
                if (m_CastShadows != value)
                    m_CastShadows = value;
                Dirty = true;
            }
        }

        public bool ReceiveShadows
        {
            get { return m_ReceiveShadows; }
            set
            {
                if (m_ReceiveShadows != value)
                    m_ReceiveShadows = value;
                Dirty = true;
            }
        }

        public bool UseLightProbes
        {
            get { return m_UseLightProbes; }
            set
            {
                if (m_UseLightProbes != value)
                    m_UseLightProbes = value;
                Dirty = true;
            }
        }

        public LightProbeUsage LightProbeUsage
        {
            get { return m_LightProbeUsage; }
            set
            {
                if (m_LightProbeUsage != value)
                    m_LightProbeUsage = value;
                Dirty = true;
            }
        }


        public ReflectionProbeUsage ReflectionProbes
        {
            get { return m_ReflectionProbes; }
            set
            {
                if (m_ReflectionProbes != value)
                    m_ReflectionProbes = value;
                Dirty = true;
            }
        }

        public Transform AnchorOverride
        {
            get { return m_AnchorOverride; }
            set
            {
                if (m_AnchorOverride != value)
                    m_AnchorOverride = value;
                Dirty = true;
            }
        }

        #endregion

        #region --- Collider ---

        public CGColliderEnum Collider
        {
            get { return m_Collider; }
            set
            {
                if (m_Collider != value)
                    m_Collider = value;
                Dirty = true;
            }
        }

        public bool AutoUpdateColliders
        {
            get { return m_AutoUpdateColliders; }
            set
            {
                if (m_AutoUpdateColliders != value)
                    m_AutoUpdateColliders = value;
                Dirty = true;
            }
        }

        public bool Convex
        {
            get { return m_Convex; }
            set
            {
                if (m_Convex != value)
                    m_Convex = value;
                Dirty = true;
            }
        }

        /// <summary>
        /// Is the created collider a trigger
        /// </summary>
        public bool IsTrigger
        {
            get { return m_IsTrigger; }
            set
            {
                if (m_IsTrigger != value)
                    m_IsTrigger = value;
                Dirty = true;
            }
        }

        /// <summary>
        /// Options used to enable or disable certain features in Collider mesh cooking. See Unity's MeshCollider.cookingOptions for more details
        /// </summary>
        public MeshColliderCookingOptions CookingOptions
        {
            get { return m_CookingOptions; }
            set
            {
                if (m_CookingOptions != value)
                    m_CookingOptions = value;
                Dirty = true;
            }
        }

        public PhysicMaterial Material
        {
            get { return m_Material; }
            set
            {
                if (m_Material != value)
                    m_Material = value;
                Dirty = true;
            }
        }

        #endregion

        /// <summary>
        /// The created meshes at the last update (call to Refresh). This list is not maintained outside of module updates, so if a user manually deletes one of the created meshes, its entry in this list will still be there, but with a null value (since deleted objects are equal to null in Unity's world) 
        /// </summary>
        public CGMeshResourceCollection Meshes
        {
            get { return m_MeshResources; }
        }

        /// <summary>
        /// Count of <see cref="Meshes"/>
        /// </summary>
        public int MeshCount
        {
            get { return Meshes.Count; }
        }

        public int VertexCount { get; private set; }

        #endregion

        #region ### Private Fields & Properties ###

        private readonly CGSpotComparer cgSpotComparer = new CGSpotComparer();


        private bool CanGroupMeshes
        {
            get
            {
                return (InSpots.IsLinked);
            }
        }

        private bool CanModifyStaticFlag
        {
            get
            {
#if UNITY_EDITOR
                return Application.isPlaying == false;
#else
                return false;
#endif
            }
        }

        private bool CanUpdate
        {
            get
            {
                return !Application.isPlaying || !MakeStatic;
            }
        }

        //Do not remove, used in FieldCondition in this file
        private bool EnableIsTrigger
        {
            get
            {
                return CanUpdate && (m_Collider != CGColliderEnum.Mesh || m_Convex);
            }
        }

        #endregion

        #region ### Unity Callbacks ###
        /*! \cond UNITY */

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            IncludeNormals = includeNormals;
            IncludeTangents = includeTangents;
#pragma warning disable 618
            AddNormals = m_AddNormals;
            AddTangents = m_AddTangents;
#pragma warning restore 618
            Collider = m_Collider;
            Dirty = true;
        }
#endif

        public override void Reset()
        {
            base.Reset();
            Combine = false;
            UnwrapUV2 = false;
            GroupMeshes = false;
            IncludeNormals = true;
            IncludeTangents = false;
#pragma warning disable 618
            AddNormals = CGYesNoAuto.Auto;
            AddTangents = CGYesNoAuto.No;
#pragma warning restore 618
            MakeStatic = false;
            Material = null;
            Layer = 0;
            Tag = DefaultTag;
            CastShadows = ShadowCastingMode.On;
            RendererEnabled = true;
            ReceiveShadows = true;
            UseLightProbes = true;
            LightProbeUsage = LightProbeUsage.BlendProbes;
            ReflectionProbes = ReflectionProbeUsage.BlendProbes;
            AnchorOverride = null;
            Collider = CGColliderEnum.Mesh;
            AutoUpdateColliders = true;
            Convex = false;
            IsTrigger = false;
#pragma warning disable 618
            AddUV2 = true;
#pragma warning restore 618
            CookingOptions = CGMeshResource.EverMeshColliderCookingOptions;
        }

        /*! \endcond */
        #endregion

        #region ### Public Methods ###

        public CreateMesh()
        {
            Version = "1";
        }

        public override bool DeleteAllOutputManagedResources()
        {
            bool result = base.DeleteAllOutputManagedResources();

            //delete all children
            int childCount = transform.childCount;
            //the following line is not prefect, since a module can have children that are not mesh resources, but I believe it is ok to assume so, worst case scenario in rare occasions there will be extra work done from the code that uses the "result" value. Best case scenario you are keeping the behaviour consistent with CreateGameObject module
            result |= childCount > 0;

            List<CGMeshResource> meshResources = new List<CGMeshResource>(childCount);
            List<Transform> nonMeshResourceChildren = new List<Transform>();

            for (int i = 0; i < childCount; i++)
            {
                Transform child = transform.GetChild(i);
                if (child.TryGetComponent(out CGMeshResource resource))
                    meshResources.Add(resource);
                else
                    nonMeshResourceChildren.Add(child);
            }

            //it might seem a good idea to not use meshResources, and just iterate through all children and delete them, but the deletion code can, depending on different on edit/play mode and prefab status, either delete instantly the object, delete it at the end of the frame, or not delete it at all, leading to the iteration logic having to handle all of those cases in deciding what should be the iteration index. I prefer to play it safe, and use the destructionTargets list
            foreach (CGMeshResource resource in meshResources)
                DeleteManagedResource("Mesh", resource);

            //we delete children that are not mesh resources to stay consistent with CreteGameObject module, that deletes all chidlren, and consistent with TryDeleteChildrenFromAssociatedPrefab. Such inconsistency with TryDeleteChildrenFromAssociatedPrefab might lead to that method loading the prefab asset at every update, since it will always detect the non mesh resource child, which will never be deleted by the this method.
            foreach (Transform child in nonMeshResourceChildren)
                child.gameObject.Destroy(false, true);

            VertexCount = 0;
            Meshes.Items.Clear();

            return result;
        }

        [Obsolete("Use DeleteAllOutputManagedResources instead")]
        public void Clear()
        {
            DeleteAllOutputManagedResources();
        }

        public override void Refresh()
        {
            base.Refresh();
            if (CanUpdate)
            {
                TryDeleteChildrenFromAssociatedPrefab();
                DeleteAllOutputManagedResources();

                List<CGVMesh> VMeshes = InVMeshArray.GetAllData<CGVMesh>(out bool isVMeshesDisposable);
                List<CGSpots> Spots = InSpots.GetAllData<CGSpots>(out bool isSpotsDisposable);

                SubArray<CGSpot>? flattenedSpotsArray = ToOneDimensionalArray(Spots, out bool isCopy);
                int vMeshesCount = VMeshes.Count;

                VertexCount = 0;
                Meshes.Items.Clear();

                if (vMeshesCount > 0 && (!InSpots.IsLinked || (flattenedSpotsArray != null && flattenedSpotsArray.Value.Count > 0)))
                {
                    if (flattenedSpotsArray != null && flattenedSpotsArray.Value.Count > 0)
                    {
                        SubArray<CGSpot> subArray = flattenedSpotsArray.Value;
                        for (int i = 0; i < subArray.Count; i++)
                        {
                            CGSpot spot = subArray.Array[i];
                            if (spot.Index >= vMeshesCount)
                            {
                                int correctedIndex = vMeshesCount - 1;
                                UIMessages.Add($"Spot index {spot.Index} references an non existing VMesh. There is/are only {vMeshesCount} valid input VMesh(es). An index of {correctedIndex} was used instead");
                                subArray.Array[i] = new CGSpot(correctedIndex, spot.Position, spot.Rotation, spot.Scale);
                            }
                        }
                        CreateSpotMeshes(VMeshes, flattenedSpotsArray.Value, Combine, isCopy, Meshes.Items);
                    }
                    else
                        CreateMeshes(VMeshes, Combine, Meshes.Items);
                }
                // Cleanup
                if (isCopy)
                    ArrayPools.CGSpot.Free(flattenedSpotsArray.Value);

                if (isVMeshesDisposable)
                {
                    VMeshes.ForEach(d => d.Dispose());
                }

                if (isSpotsDisposable)
                {
                    Spots.ForEach(d => d.Dispose());
                }

                // Update Colliders?
                if (AutoUpdateColliders)
                    UpdateColliders();
            }
            else
                UIMessages.Add("Make Static is enabled. This stops mesh generation in Play Mode, to maintain Unity optimizations done in Edit Mode to static GameObjects.");

            if (MakeStatic && CurvyGlobalManager.SaveGeneratorOutputs == false)
                UIMessages.Add("Make Static is incompatible with Preferences -> Curvy -> Save Generator Outputs being false.");
        }

        public void UpdateColliders()
        {
            List<CGMeshResource> meshResources = Meshes.Items;
            bool success = true;

            //Parallel mesh baking if needed
            if (Collider == CGColliderEnum.Mesh && meshResources.Count > 1)//do not bake if no mesh collider asked
            {
                SubArray<int> meshIds = ArrayPools.Int32.Allocate(meshResources.Count, false);
                for (var i = 0; i < meshResources.Count; i++)
                {
                    if (meshResources[i] == null)
                    {
#if CURVY_SANITY_CHECKS_PRIVATE
                        DTLog.LogError("[Curvy] A resource was null.", this);
#endif
                        meshIds.Array[i] = 0; //meshIds is allocated without being cleared, so set to 0 to avoid using the meshId from a previous call
                    }
                    else
                        meshIds.Array[i] = meshResources[i].Filter.sharedMesh.GetInstanceID();
                }

                Parallel.For(0, meshResources.Count, (i) =>
                {
                    Physics.BakeMesh(meshIds.Array[i], Convex);
                }
                );

                ArrayPools.Int32.Free(meshIds);
            }

            for (int r = 0; r < meshResources.Count; r++)
            {
                if (meshResources[r] == null)
                    continue;
                if (!meshResources[r].UpdateCollider(Collider, Convex, IsTrigger, Material, CookingOptions))
                    success = false;
            }
            if (!success)
                UIMessages.Add("Error setting collider!");
        }

        #region ISerializationCallbackReceiver implementation

        public void OnBeforeSerialize()
        {

        }

        public void OnAfterDeserialize()
        {
            if (String.IsNullOrEmpty(Version))
            {
                Version = "1";
#pragma warning disable 618
                IncludeNormals = AddNormals != CGYesNoAuto.No;
                IncludeTangents = AddTangents != CGYesNoAuto.No;
#pragma warning restore 618
            }
        }

        #endregion

        #endregion

        #region ### Privates ###
        /*! \cond PRIVATE */

        private void CreateMeshes(List<CGVMesh> vMeshes, bool combine, [NotNull] List<CGMeshResource> createdMeshes)
        {
            if (combine && vMeshes.Count > 1)
            {
                CGVMesh curVMesh = new CGVMesh();
                curVMesh.MergeVMeshes(vMeshes, 0, vMeshes.Count - 1);
                WriteVMeshToMesh(curVMesh, createdMeshes);
            }
            else
                for (int index = 0; index < vMeshes.Count; index++)
                    WriteVMeshToMesh(vMeshes[index], createdMeshes);
        }

        private void CreateSpotMeshes(List<CGVMesh> vMeshes, SubArray<CGSpot> spots, bool combine, bool spotsIsACopy, [NotNull] List<CGMeshResource> createdMeshes)
        {
            int vmCount = vMeshes.Count;
            CGSpot spot;

            bool allocateNewSpotsArray = combine && GroupMeshes && spotsIsACopy == false;

            if (allocateNewSpotsArray)
                spots = ArrayPools.CGSpot.Clone(spots);

            if (combine)
            {
                if (GroupMeshes)
                    System.Array.Sort(spots.Array, 0, spots.Count, cgSpotComparer);

                spot = spots.Array[0];
                CGVMesh curVMesh = new CGVMesh(vMeshes[spot.Index]);
                if (spot.Position != Vector3.zero || spot.Rotation != Quaternion.identity || spot.Scale != Vector3.one)
                    curVMesh.TRS(spot.Matrix);
                for (int s = 1; s < spots.Count; s++)
                {
                    spot = spots.Array[s];
                    // Filter spot.index not in vMeshes[]
                    if (spot.Index > -1 && spot.Index < vmCount)
                    {
                        if (GroupMeshes && spot.Index != spots.Array[s - 1].Index)
                        { // write curVMesh 
                            WriteVMeshToMesh(curVMesh, createdMeshes);
                            curVMesh.Dispose();
                            curVMesh = new CGVMesh(vMeshes[spot.Index]);
                            if (!spot.Matrix.isIdentity)
                                curVMesh.TRS(spot.Matrix);
                        }
                        else
                        {
                            // Add new vMesh to curVMesh
                            //OPTIM use MergeVMeshes to merge everything at once
                            curVMesh.MergeVMesh(vMeshes[spot.Index], spot.Matrix);
                        }
                    }
                }
                WriteVMeshToMesh(curVMesh, createdMeshes);
                curVMesh.Dispose();
            }
            else
            {
                for (int s = 0; s < spots.Count; s++)
                {
                    spot = spots.Array[s];
                    // Filter spot.index not in vMeshes[]
                    if (spot.Index > -1 && spot.Index < vmCount)
                    {
                        CGMeshResource res = WriteVMeshToMesh(vMeshes[spot.Index], createdMeshes);
                        // Don't touch vertices, TRS Resource instead
                        if (spot.Position != Vector3.zero || spot.Rotation != Quaternion.identity || spot.Scale != Vector3.one)
                            spot.ToTransform(res.Filter.transform);
                    }
                }
            }

            if (allocateNewSpotsArray)
                ArrayPools.CGSpot.Free(spots);
        }

        /// <summary>
        /// create a mesh resource and copy vmesh data to the mesh!
        /// </summary>
        /// <param name="vmesh"></param>
        /// <param name="cgMeshResources"></param>
        private CGMeshResource WriteVMeshToMesh(CGVMesh vmesh, List<CGMeshResource> cgMeshResources)
        {
            CGMeshResource res = GetNewMesh(cgMeshResources.Count);
            cgMeshResources.Add(res);

            if (CanModifyStaticFlag)
                res.Filter.gameObject.isStatic = false;
            Mesh mesh = res.Prepare();
            res.gameObject.layer = Layer;
            res.gameObject.tag = Tag;
            vmesh.ToMesh(ref mesh, IncludeNormals, IncludeTangents);
            VertexCount += vmesh.Count;

            if (IncludeNormals && (vmesh.HasNormals == false || vmesh.HasPartialNormals))
                mesh.RecalculateNormals();
            if (IncludeTangents && (vmesh.HasTangents == false || vmesh.HasPartialTangents))
                mesh.RecalculateTangents();

            if (Combine && UnwrapUV2 && (vmesh.HasUV2))
#if UNITY_EDITOR
                UnityEditor.Unwrapping.GenerateSecondaryUVSet(mesh);
#else
            DTLog.Log("[Curvy] UV2 Unwrapping is not available outside of the editor", this);
#endif

#if CURVY_SANITY_CHECKS_PRIVATE
            if (IncludeNormals)
            {
                Vector3[] meshNormals = mesh.normals;
                for (var i = 0; i < meshNormals.Length; i++)
                {
                    if (meshNormals[i] == Vector3.zero)
                    {
                        Assert.IsTrue(false);
                    }
                }
            }

            if (includeTangents)
            {
                Vector4[] meshTangents = mesh.tangents;
                for (var i = 0; i < meshTangents.Length; i++)
                {
                    if (meshTangents[i] == Vector4.zero)
                    {
                        Assert.IsTrue(false);
                    }
                }
            }
#endif

            // Reset Transform
            res.Filter.transform.localPosition = Vector3.zero;
            res.Filter.transform.localRotation = Quaternion.identity;
            res.Filter.transform.localScale = Vector3.one;
            if (CanModifyStaticFlag)
                res.Filter.gameObject.isStatic = MakeStatic;
            res.Renderer.sharedMaterials = vmesh.GetMaterials();


            return res;
        }

        /// <summary>
        /// gets a new mesh resource and increase mCurrentMeshCount
        /// </summary>
        private CGMeshResource GetNewMesh(int currentMeshCount)
        {
            // Reuse existing resources
            CGMeshResource r = ((CGMeshResource)AddManagedResource("Mesh", "", currentMeshCount));

            // Renderer settings
            r.Renderer.shadowCastingMode = CastShadows;
            r.Renderer.enabled = RendererEnabled;
            r.Renderer.receiveShadows = ReceiveShadows;
            r.Renderer.lightProbeUsage = LightProbeUsage;
            r.Renderer.reflectionProbeUsage = ReflectionProbes;

            r.Renderer.probeAnchor = AnchorOverride;

            if (!r.ColliderMatches(Collider))
                r.RemoveCollider();

            return r;
        }


        private static SubArray<CGSpot>? ToOneDimensionalArray(List<CGSpots> spotsList, out bool arrayIsCopy)
        {
            SubArray<CGSpot>? output;
            switch (spotsList.Count)
            {
                case 1:
                    if (spotsList[0] != null)
                    {
                        output = new SubArray<CGSpot>(spotsList[0].Spots.Array, spotsList[0].Spots.Count);
                        arrayIsCopy = false;
                    }
                    else
                    {
                        output = null;
                        arrayIsCopy = false;
                    }
                    break;
                case 0:
                    output = null;
                    arrayIsCopy = false;
                    break;
                default:
                    {
                        output = ArrayPools.CGSpot.Allocate(spotsList.Where(s => s != null).Sum(s => s.Count));
                        arrayIsCopy = true;

                        CGSpot[] array = output.Value.Array;
                        int destinationIndex = 0;
                        foreach (CGSpots cgSpots in spotsList)
                        {
                            if (cgSpots == null)
                                continue;
                            Array.Copy(cgSpots.Spots.Array, 0, array, destinationIndex, cgSpots.Spots.Count);
                            destinationIndex += cgSpots.Spots.Count;
                        }
                    }

                    break;
            }

            return output;
        }

        /*! \endcond */

        #endregion

        #region Resources saving

        /// <summary>
        /// Saves the created meshes to asset files
        /// </summary>
        /// <remarks>Will open a file selector to select where to save the files</remarks>
        public void SaveToAsset()
        {
#if UNITY_EDITOR
            string assetPathBase = InquireAssetPath();
            if (string.IsNullOrEmpty(assetPathBase))
                return;

            List<Component> managedResources;
            GetManagedResources(out managedResources, out _);
            for (int i = 0; i < managedResources.Count; i++)
            {
                Mesh instantiatedMesh = Instantiate(managedResources[i].GetComponent<MeshFilter>().sharedMesh);
                string assetPath = ResourceFilesSavingState.GetAssetFullName(assetPathBase, i, managedResources.Count);
                SaveMeshToAsset(instantiatedMesh, assetPath);
            }

            UnityEditor.AssetDatabase.Refresh();
#else
            throw new InvalidOperationException("Operation available only in editor");
#endif
        }

        /// <summary>
        /// Saves a copy of the generated mesh(es) as Asset(s), then creates a GameObject, outside of the generator, referencing those mesh assets. This way the created GameObject can be made part of a prefab without issues
        /// </summary>
        /// <remarks>Will open a file selector to select where to save the files</remarks>
        public void SaveToSceneAndAsset()
        {
#if UNITY_EDITOR
            string assetPathBase = InquireAssetPath();
            if (string.IsNullOrEmpty(assetPathBase))
                return;

            try
            {
                GetManagedResources(out List<Component> managedResources, out _);
                resourceFilesSavingState = new ResourceFilesSavingState(assetPathBase, 0, managedResources.Count);

                SaveToScene();
                UnityEditor.AssetDatabase.Refresh();
            }
            finally
            {
                resourceFilesSavingState = null;
            }
#else
            throw new InvalidOperationException("Operation available only in editor");
#endif
        }

        protected override GameObject SaveResourceToScene(Component managedResource, Transform newParent)
        {
            MeshFilter meshFilter = managedResource.GetComponent<MeshFilter>();
            Mesh instantiatedMesh = Instantiate(meshFilter.sharedMesh);

            GameObject duplicateGameObject = managedResource.gameObject.DuplicateGameObject(newParent);
            duplicateGameObject.name = managedResource.name;
            duplicateGameObject.GetComponent<CGMeshResource>().Destroy(false, true);
            duplicateGameObject.GetComponent<MeshFilter>().sharedMesh = instantiatedMesh;
#if UNITY_EDITOR
            if (resourceFilesSavingState != null)
            {
                string assetName = resourceFilesSavingState.GetAssetFullName();
                SaveMeshToAsset(instantiatedMesh, assetName);
                resourceFilesSavingState.IncrementResourceIndex();
            }
#endif
            return duplicateGameObject;
        }

#if UNITY_EDITOR

        /// <summary>
        /// This state is used when saving both to the scene and the assets. The way this operation was implemented makes it a scene saving that saves to te assets folder too based on the state of this class's instance
        /// </summary>
        private class ResourceFilesSavingState
        {
            private string AssetsBasePath { get; }
            private int ResourceIndex { get; set; }
            private int ResourcesCount { get; }

            public ResourceFilesSavingState(string assetsBasePath, int resourceIndex, int resourcesCount)
            {
                this.AssetsBasePath = assetsBasePath;
                this.ResourceIndex = resourceIndex;
                this.ResourcesCount = resourcesCount;
            }
            public ResourceFilesSavingState()
            {
            }

            public void IncrementResourceIndex()
            {
                ResourceIndex++;
            }

            public string GetAssetFullName()
            {
                return GetAssetFullName(AssetsBasePath, ResourceIndex, ResourcesCount);
            }

            public static string GetAssetFullName(string assetsBasePath, int resourceIndex, int resourcesCount)
            {
                return resourcesCount > 1
                    ? $"{assetsBasePath}-{resourceIndex:D3}.asset"
                    : $"{assetsBasePath}.asset";
            }

        }

        private ResourceFilesSavingState resourceFilesSavingState;

        private static void SaveMeshToAsset(Mesh meshInstance, string assetPath)
        {
            UnityEditor.AssetDatabase.DeleteAsset(assetPath);
            UnityEditor.AssetDatabase.CreateAsset(meshInstance, assetPath);
        }

        private string InquireAssetPath()
        {
            return UnityEditor.EditorUtility.SaveFilePanelInProject("Save Assets", ModuleName, "mesh", "Save Mesh(es) as").Replace(".mesh", "");
        }
#endif
    }

    #endregion

}