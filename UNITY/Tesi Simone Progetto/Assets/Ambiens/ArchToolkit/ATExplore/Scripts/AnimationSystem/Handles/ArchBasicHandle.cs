using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ArchToolkit.ArchGizmos;


namespace ArchToolkit.AnimationSystem
{
    [ExecuteInEditMode]
    public class ArchBasicHandle : MonoBehaviour
    {
        public AInteractable animationToOpen;

        private void Awake()
        {
            ArchToolkitManager.Instance.OnVisitorCreated += this.InitHandle;
        }

        public virtual void InitHandle()
        {
            if (ArchToolkitManager.IsInstanced())
            {
                if (ArchToolkitManager.Instance.visitor != null)
                {
                    foreach (var inputRaycaster in GameObject.FindObjectsOfType<ArchToolkit.Character.InputRaycaster>())
                    {
                        inputRaycaster.OnClick += this.OnClick;
                        inputRaycaster.OnHover += this.OnHover;

                    }
                }

            }
        }

      /*
#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlaying && !UnityEditor.EditorApplication.isCompiling)
            {
                if (this.animationToOpen != null)
                    this.animationToOpen.onDestroy += this.AnimationDestroyed;
            }
#endif
        }*/

        public void OnClick(Transform transform)
        {
            if (transform.gameObject != this.gameObject)
            {
                return;
            }

            if (this.animationToOpen != null)
            {
                if (this.animationToOpen.StartWith != AInteractable.StartWithType.waitcall)
                    this.animationToOpen.StartAnimation();
            }
        }
        public bool isHovering = false;
        public void OnHover(Ray ray, RaycastHit hit)
        {
            if (hit.transform.gameObject != this.gameObject)
            {
                return;
            }
            isHovering = true;
            this.onHover();

        }
        public virtual void onHover()
        {

        }
        protected virtual void Update()
        {
#if UNITY_EDITOR
            if (this.animationToOpen == null)
                GameObject.DestroyImmediate(this);
#endif

        }
        private void LateUpdate()
        {
            isHovering = false;
        }
        private void AnimationDestroyed()
        {
           // GameObject.DestroyImmediate(this);
        }
    }
}