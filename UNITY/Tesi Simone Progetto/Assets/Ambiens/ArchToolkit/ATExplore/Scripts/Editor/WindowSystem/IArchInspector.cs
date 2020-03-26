using UnityEngine;

namespace ArchToolkit.Editor.Window
{

    public interface IArchInspector
    {
        string Name
        {
            get;
            set;
        }

        bool InspectorVisible
        {
            get;
        }
    
        void OnSelectionChange(GameObject gameObject);

        void OnProjectChange();

        bool IsInspectorVisible();

        void OnFocus();

        void OnEnable();

        void OnGui();

        void OnClose();

        void OnUpdate();
    }
}