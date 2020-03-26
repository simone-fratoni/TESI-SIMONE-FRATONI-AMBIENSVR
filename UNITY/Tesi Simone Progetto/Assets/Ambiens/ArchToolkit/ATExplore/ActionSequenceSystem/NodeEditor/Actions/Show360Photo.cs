using System.Collections;
using System.Collections.Generic;
using ambiens.archtoolkit.atexplore.XNode;
using UnityEngine;

namespace ambiens.archtoolkit.atexplore.actionSystem
{
    [CreateNodeMenuAttribute("Actions/Show 360 Photo")]
    public class Show360Photo : AAction
    {
        [HideInInspector]
        public Vector3 tpTargetPosition;
        [Input]
        public Texture2D texture;

        public float yRotation;

        protected override void _RuntimeInit()
        {

        }

        protected override bool _StartAction()
        {

            Three60Photo.Instance.transform.position = this.tpTargetPosition;

            var txt = this.GetInputValue<Texture2D>("texture");
            if (txt == null) txt = texture;

            ArchToolkit.ArchToolkitManager.Instance.visitor.Teleport(this.tpTargetPosition);
            Three60Photo.Instance.SetTexture(txt);
            ArchToolkit.ArchToolkitManager.Instance.visitor.LockPosition();

            Three60Photo.Instance.transform.Rotate(Vector3.up, yRotation);

            return true;
        }

    }
}
