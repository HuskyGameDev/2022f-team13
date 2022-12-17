// =====================================================================
// Copyright 2013-2017 Fluffy Underware
// All rights reserved
// 
// http://www.fluffyunderware.com
// =====================================================================

using UnityEngine;
using UnityEditor;

namespace FluffyUnderware.DevToolsEditor
{

    public class DTToolbarToggleButton : DTToolbarItem
    {

        public virtual bool On
        {
            get { return mOn; }
            set
            {
                if (mOn != value)
                {
                    mOn = value;
                    if (!mOn && FocusedItem == this)
                        FocusedItem = null;
                }
            }
        }

        internal bool mOn;

        public override bool ShowClientArea
        {
            get
            {
                return On;
            }
            
        }

        public override void OnOtherItemClicked(DTToolbarItem other)
        {
            base.OnOtherItemClicked(other);
            On = false;
        }

        public override void OnClick()
        {
            base.OnClick();
            On = !On;
        }

        public override void Render(Rect r)
        {
            bool b = GUI.Toggle(r, On, Content, DTStyles.TBButton);
            if (b!=On)
                OnClick();
        }

        
    }

}
