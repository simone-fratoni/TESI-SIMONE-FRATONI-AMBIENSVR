using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ArchToolkit.InputSystem;
using System.Linq;

namespace ArchToolkit.Character
{
    public class HUD : MonoBehaviour
    {
        
        public string FlyCamText = "FLYCAM \n Press space to walk on the ground";
        public string WalkText = "WALKCAM \n Press space to fly";

        public Text switchText;
        public GameObject switchTextPanel;

        protected ArchCharacter visitor;

        public virtual void Exit()
        {
            if (ArchToolkitManager.IsInstanced())
            {
                ArchToolkitManager.Instance.DoExit();
            }
        }

        protected virtual void Awake()
        {
            this.InitializeHUD();

            ArchToolkitManager.Instance.OnVisitorCreated += this.InitializeHUD;
        }

        protected virtual void InitializeHUD()
        {
            if (ArchToolkitManager.IsInstanced() && ArchToolkitManager.Instance.visitor != null)
            {
                this.visitor = ArchToolkitManager.Instance.visitor;

                if(this.visitor.LockMovement != LockMovementTo.None)
                {
                    if (this.switchTextPanel != null)
                        this.switchTextPanel.SetActive(false);
                }

                this.SwitchText(this.visitor.MovementType);

                if(this.visitor != null)
                    this.visitor.OnMovementTypeChanged += this.SwitchText;
            }
        }

        private void SwitchText(MovementType movementType)
        {
            if (this.switchText == null)
                return;

            if (movementType == MovementType.FlyCam)
            {
                this.switchText.text = this.FlyCamText;
            }
            else
                this.switchText.text = this.WalkText;
        }

       
    }
}
