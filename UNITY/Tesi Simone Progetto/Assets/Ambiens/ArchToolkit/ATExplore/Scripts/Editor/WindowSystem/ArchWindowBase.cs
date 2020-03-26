using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

#endif

namespace ArchToolkit.Editor.Window
{
    public class ArchWindowBase : IArchWindow
    {
        protected WindowStatus windowStatus;

        public float MaxWindowHeight
        {
            get
            {
                // 100 is default 
                // 50 is the height between tabs and button
                return this._windowHeight = ArchToolkitWindowData.TAB_HEIGHT + ArchToolkitWindowData.PADDING + 
                                            ((ArchToolkitWindowData.BUTTON_HEIGHT + GUI.skin.button.padding.bottom) * _buttonCount) + ArchToolkitWindowData.PADDING;
            }
            set
            {
                this._windowHeight = value;
            }
        }

        public int ButtonCount
        {
            get
            {
                return this._buttonCount;
            }
            set
            {
                this._buttonCount = value;
            }
        }

        protected float _windowHeight;

        protected int _buttonCount;

        public WindowStatus GetStatus
        {
            get
            {
                return windowStatus;
            }
        }

        public ArchWindowBase(WindowStatus windowStatus)
        {
            

            this.windowStatus = windowStatus;

            if(MainArchWindow.Instance != null)
                MainArchWindow.Instance.AddWindow(this);
        }

        public virtual void DrawGUI() { }

        public virtual void OnClose() { }

        public virtual void OnOpen() { }

        public virtual void OnSelectionChange(GameObject gameObject) { }
        
    }
}
