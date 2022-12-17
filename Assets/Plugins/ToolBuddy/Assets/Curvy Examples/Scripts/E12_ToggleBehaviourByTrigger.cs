// =====================================================================
// Copyright 2013-2022 ToolBuddy
// All rights reserved
// 
// http://www.toolbuddy.net
// =====================================================================

using UnityEngine;

namespace FluffyUnderware.Curvy.Examples
{
    public class E12_ToggleBehaviourByTrigger : MonoBehaviour
    {
        public Behaviour UIElement;

        void OnTriggerEnter()
        {
            if (UIElement)
                UIElement.enabled = !UIElement.enabled;
        }
    }
}