using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ArchToolkit.Character
{

    public class HUDDesktop : HUD
    {
        public GameObject hud;
        public GameObject topHUD;

        protected override void Awake()
        {
            base.Awake();
            this.toggleHUD();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.H) && this.hud != null)
                this.toggleHUD();
        }

        private void toggleHUD()
        {
            this.hud.SetActive(!this.hud.activeInHierarchy);
            this.topHUD.SetActive(!this.topHUD.activeInHierarchy);
        }
    }
}