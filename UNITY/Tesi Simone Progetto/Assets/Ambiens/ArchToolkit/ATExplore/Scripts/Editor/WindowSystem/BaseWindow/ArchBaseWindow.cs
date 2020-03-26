using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ArchToolkit.Utils;
using UnityEditor;

namespace ArchToolkit.Editor.Window
{

    public class ArchBaseWindow : ArchWindowCategory
    {
        [SerializeField]
        private ArchToolkitManager toolkitManager;

        public ArchBaseWindow(WindowStatus status) : base(status)
        {
            this.ButtonCount = 1;
        }
        
        public override void DrawGUI()
        {
            if (this.toolkitManager == null)
                this.toolkitManager = GameObject.FindObjectOfType<ArchToolkitManager>();

            EditorGUILayout.BeginVertical();

            EditorGUI.BeginDisabledGroup(this.toolkitManager != null && this.toolkitManager.managerContainer.pathManager.PathPoints.Count > 0);

            if (GUILayout.Button(new GUIContent(ArchToolkitText.TOM_BUTTON,ArchToolkitText.TOM_BUTTON_TOOLTIP), GUILayout.Height(ArchToolkitWindowData.BUTTON_HEIGHT)))
            {
                if (!ArchToolkitManager.IsInstanced())
                {
                    ArchToolkitManager.Factory();
                }

                if (ArchToolkitManager.IsInstanced())
                {

                    if (ArchToolkitManager.Instance.managerContainer.pathManager.PathPoints.Count > 0)
                    {
                        EditorUtility.DisplayDialog("Warning", "Starting point already exist", "Ok, I understand");
                        return;
                    }

                    ArchToolkitManager.Instance.managerContainer.pathManager.SetStartingPoint();

                    this.toolkitManager = ArchToolkitManager.Instance;
                }
            }

            EditorGUI.EndDisabledGroup();
            
            EditorGUILayout.EndVertical();

        }

        public override void OnClose()
        {
            base.OnClose();
        }

        public override void OnOpen()
        {
            this.toolkitManager = GameObject.FindObjectOfType<ArchToolkitManager>();
        }
    }
}
