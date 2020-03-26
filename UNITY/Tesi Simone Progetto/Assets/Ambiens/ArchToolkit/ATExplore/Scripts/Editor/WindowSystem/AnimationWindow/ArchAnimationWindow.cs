using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ArchToolkit.AnimationSystem;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ArchToolkit.Editor.Window
{
    public class ArchAnimationWindow : ArchWindowCategory
    {
        public ArchAnimationWindow(WindowStatus status) : base(status)
        {
            this.ButtonCount = 5;
        }

        public override void DrawGUI()
        {
            EditorGUILayout.BeginVertical();
            
            if (GUILayout.Button(new GUIContent(ArchToolkitText.ADD_ROTATION,ArchToolkitText.ROTATION_TOOLTIP), GUILayout.Height(ArchToolkitWindowData.BUTTON_HEIGHT)))
            {
                ArchDoorInspector.Instance.AddAnimation<RotateAround>();
            }

            if (GUILayout.Button(new GUIContent(ArchToolkitText.ADD_TRANSLATION,ArchToolkitText.TRANSLATION_TOOLTIP), GUILayout.Height(ArchToolkitWindowData.BUTTON_HEIGHT)))
            {
                ArchDrawerInspector.Instance.AddAnimation<Translate>();
            }

            if(GUILayout.Button(new GUIContent( ArchToolkitText.ADD_SWITCH_MATERIAL,ArchToolkitText.SWITCH_MATERIAL_TOOLTIP), GUILayout.Height(ArchToolkitWindowData.BUTTON_HEIGHT)))
            {
                ArchMaterialInspector.Instance.SetAnimation(Selection.activeGameObject);
            }
            if (Selection.activeGameObject != null)
            {
                var d = Selection.activeGameObject.GetComponent<DraggableObjectInteractable>();
                if (Selection.activeGameObject != null && d == null)
                {
                    if (GUILayout.Button(new GUIContent(ArchToolkitText.ADD_DRAGGABLE, ArchToolkitText.DRAGGABLE_TOOLTIP), GUILayout.Height(ArchToolkitWindowData.BUTTON_HEIGHT)))
                    {
                        DraggableInteractionInspector.AddDraggable(Selection.activeGameObject);
                    }
                }
                else if (d != null)
                {
                    if (GUILayout.Button(new GUIContent(ArchToolkitText.REMOVE_DRAGGABLE, ArchToolkitText.DRAGGABLE_TOOLTIP), GUILayout.Height(ArchToolkitWindowData.BUTTON_HEIGHT)))
                    {
                        GameObject.DestroyImmediate(d);
                    }
                }
            }
            /*
            EditorGUI.BeginDisabledGroup(!this.IsMobile());

            if (GUILayout.Button(new GUIContent(ArchToolkitText.ADD_TELEPORT_POINT,ArchToolkitText.VR_MOBILE_TELEPORT_POINT_TOOLTIP), GUILayout.Height(ArchToolkitWindowData.BUTTON_HEIGHT)))
            {
                ArchToolkitManager.Instance.managerContainer.pathManager.AddPoint();
            }

            EditorGUI.EndDisabledGroup();
            */
            //Sequence Holder
            var seqHolder=GameObject.FindObjectOfType<ambiens.archtoolkit.atexplore.actionSystem.SequenceHolder>();
            if (seqHolder == null)
            {
                if (GUILayout.Button(new GUIContent(ArchToolkitText.ADD_SEQUENCE_HOLDER, ArchToolkitText.ADD_SEQUENCE_HOLDER), GUILayout.Height(ArchToolkitWindowData.BUTTON_HEIGHT)))
                {
                    var go = new GameObject("[Sequence Holder]");
                    var sh=go.AddComponent<ambiens.archtoolkit.atexplore.actionSystem.SequenceHolder>();
                }
            }
            else{
                if (GUILayout.Button(new GUIContent(ArchToolkitText.SELECT_SEQUENCE_HOLDER, ArchToolkitText.SELECT_SEQUENCE_HOLDER), GUILayout.Height(ArchToolkitWindowData.BUTTON_HEIGHT)))
                {
                    Selection.activeGameObject=seqHolder.gameObject;
                }
            }
            EditorGUILayout.EndVertical();
        }

        public override void OnClose()
        {
            
        }

        public override void OnSelectionChange(GameObject gameObject)
        {
            
        }

        private bool IsMobile()
        {
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
                return true;

            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
                return true;

            return false;
        }
    }
}
