using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using ArchToolkit.InputSystem;
using ArchToolkit.VR;

namespace ArchToolkit.Character
{

    public class ArchVRCharacter : ArchCharacter
    {
        
        public event Action<bool> OnCheckVRInteraction;

        private List<string> NonPositionalTrackingDeviceNames = new List<string>()
        {
            "cardboard","gearvr","oculus go"
        };
        //public bool HeadPositionalTrackingEnabled = true;
        protected override void Awake()
        {
            this.SetValues();

            this.onHeightDifferenceChange += this.onVRHeightDifferenceChange;
            raycaster.OnHoverFloor += this.onHoverFloor;
            string deviceName = UnityEngine.XR.XRSettings.loadedDeviceName.ToLower();
            string deviceModel = UnityEngine.XR.XRDevice.model.ToLower();
            Debug.Log(deviceName+" "+deviceModel);
            if (this.NonPositionalTrackingDeviceNames.Contains(deviceName) || this.NonPositionalTrackingDeviceNames.Contains(deviceModel))
            {
                Debug.Log("Device "+deviceName+" 3DoF -> setting default height");
                this.HeightDifference = 1.6f;
            }
            else
            {
                this.___heightDifference = 0;
            }
        }

        private void onVRHeightDifferenceChange(float height)
        {
            this.CheckGround((RaycastHit hit)=>{
                this.transform.position = new Vector3(hit.point.x, hit.point.y+this.HeightDifference, hit.point.z);
            },this.transform);
        }

        protected override void Start()
        {
            this.DefaultRaycastTool= new DefaultRaycastTool();

        }

        protected override void SetValues()
        {
            base.SetValues();

            /*if (ArchToolkitManager.Instance.movementTypePerPlatform == MovementTypePerPlatform.VR_Mobile)
            {
                this.raycaster.UseTimer = ArchToolkitManager.Instance.settings.useGazeTimer;
                this.raycaster.maxTimer = ArchToolkitManager.Instance.settings.maxGazeTimer;
            }*/
            /*if (ArchToolkitManager.Instance.movementTypePerPlatform == MovementTypePerPlatform.VR_Mobile)
                this.SetHeight(this.thresholdHeight);*/

        }

        protected override void Update()
        {
            if (this.Head == null)
            {
                Debug.LogError("Null Head");
                return;
            }

            this.CurrentRaycastTool.Update(Time.deltaTime);

            //this.VRMovement();
        }
        /*protected void VRMovement()
        {
            if (ArchToolkitManager.Instance.movementTypePerPlatform == MovementTypePerPlatform.VR )
            {
                if (ArchToolkitManager.Instance.managerContainer.archToolkitVRManager != null &&
                    ArchToolkitManager.Instance.managerContainer.archToolkitVRManager.isVrRunning)
                {
                    
                    if (this.raycaster != null && this.raycaster.hit.transform != null) 
                    {
                      
                        if (this.CanTeleport(this.raycaster.hit, 45f))
                        {
                            if (!this.raycaster.UseTimer)
                            {
                               
                                if (ArchToolkitManager.Instance.managerContainer.archToolkitVRManager.rightController != null)
                                {
                                    ArchToolkitManager.Instance.managerContainer.archToolkitVRManager.rightController.OnTriggerPressed(() =>
                                    {
                                        this.TeleportWithFade();

                                    }, 0.95f);
                                }
                            }
                            else
                            {
                                if (!this.raycaster.UseTimer)
                                {
                                    if (InputListener.MouseButtonLeftDown)
                                    {
                                        this.TeleportWithFade();
                                    }
                                }
                            }
                        }

                    }
                    else
                    {
                        if (this.OnCheckVRInteraction != null)
                            this.OnCheckVRInteraction(false);
                    }
                }
            }
        }*/
    }
}

