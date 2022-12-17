// =====================================================================
// Copyright 2013-2022 ToolBuddy
// All rights reserved
// 
// http://www.toolbuddy.net
// =====================================================================

using UnityEngine;
using System.Collections;

namespace FluffyUnderware.Curvy.Generator
{
    /// <summary>
    /// Class defining a linkage between two modules' slots
    /// </summary>
    [System.Serializable]
    public class CGModuleLink
    {
        // Self
        [SerializeField] private int m_ModuleID;
        [SerializeField] private string m_SlotName;
        // Other
        [SerializeField] private int m_TargetModuleID;
        [SerializeField] private string m_TargetSlotName;

        public int ModuleID { get { return m_ModuleID; } }
        public string SlotName { get { return m_SlotName;} }
        public int TargetModuleID { get { return m_TargetModuleID; } }
        public string TargetSlotName { get { return m_TargetSlotName; } }
        
        

        public CGModuleLink(int sourceID, string sourceSlotName, int targetID, string targetSlotName)
        {
            m_ModuleID = sourceID;
            m_SlotName = sourceSlotName;
            m_TargetModuleID = targetID;
            m_TargetSlotName = targetSlotName;
        }

        public CGModuleLink(CGModuleSlot source, CGModuleSlot target) : this(source.Module.UniqueID,source.Name,target.Module.UniqueID,target.Name) {}

        public bool IsSame(CGModuleLink o)
        {
            return (ModuleID == o.ModuleID &&
                    SlotName == o.SlotName &&
                    TargetModuleID == o.TargetModuleID &&
                    TargetSlotName == o.m_TargetSlotName);
        }

        public bool IsSame(CGModuleSlot source, CGModuleSlot target)
        {
            return (ModuleID == source.Module.UniqueID &&
                    SlotName == source.Name &&
                    TargetModuleID == target.Module.UniqueID &&
                    TargetSlotName == target.Name);
        }

        public bool IsTo(CGModuleSlot s)
        {
            return (s.Module.UniqueID == TargetModuleID && s.Name == TargetSlotName);
        }

        public bool IsFrom(CGModuleSlot s)
        {
            return (s.Module.UniqueID == ModuleID && s.Name == SlotName);
        }

        public bool IsUsing(CGModule module)
        {
            return (ModuleID == module.UniqueID || TargetModuleID == module.UniqueID);
        }

        public bool IsBetween(CGModuleSlot one, CGModuleSlot another)
        {
            return ((IsTo(one) && IsFrom(another)) ||
                   (IsTo(another) && IsFrom(one)));
        }

        public void SetModuleIDIINTERNAL(int moduleID, int targetModuleID)
        {
            m_ModuleID = moduleID;
            m_TargetModuleID = targetModuleID;
        }
        

        public static implicit operator bool(CGModuleLink a)
        {
            return !object.ReferenceEquals(a, null);
        }

        public override string ToString()
        {
            return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}({1})->{2}({3})", SlotName, ModuleID, TargetSlotName, TargetModuleID);
        }
        
    }

   
}