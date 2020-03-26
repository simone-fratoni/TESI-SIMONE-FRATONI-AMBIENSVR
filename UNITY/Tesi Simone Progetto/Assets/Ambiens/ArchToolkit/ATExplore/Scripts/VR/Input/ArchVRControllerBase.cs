using System.Collections;
using System.Collections.Generic;
using UnityEngine; 
using System;
#if !UNITY_WEBGL && !UNITY_STANDALONE_OSX && IS_LEGACY_INPUT_INSTALLED
using UnityEngine.SpatialTracking;
#endif
using UnityEngine.XR;
using ArchToolkit.InputSystem;
using ArchToolkit.Character;

namespace ArchToolkit.VR
{
#if !UNITY_WEBGL
    //[RequireComponent(typeof(TrackedPoseDriver))]
#endif
    public class ArchVRControllerBase : MonoBehaviour
    {
        [SerializeField]
        public XRNode NodeType;
        private bool __trackingAcquired = false;

        public bool TrackingAcquired {
            set
            {
                this.__trackingAcquired = value;
                if (this.__trackingAcquired)
                {
                    this.InitializeController();
                }
            }
            get
            {
                return __trackingAcquired;
            }
        }

        [SerializeField]
        private bool triggerPressed;

        public GameObject uiAnchor;

        public GameObject rayOrigin;

        protected virtual void Awake()
        {
            ArchToolkitManager.Instance.managerContainer.archToolkitVRManager.OnConnectedDevice += this.DeviceConnected;
        }
        protected virtual void Update()
        {
            if (!ArchToolkitManager.IsInstanced())
                return;
            if (this.TrackingAcquired)
            {
                this.gameObject.transform.localPosition = InputTracking.GetLocalPosition(NodeType);
                this.gameObject.transform.localRotation = InputTracking.GetLocalRotation(NodeType);
            }
        }

        protected virtual void DeviceConnected(ConnectedDevice connectedDevice)
        {
            Debug.Log("Device Connected");
            this.InitializeController();
        }

        internal virtual void SetController(Action onError)
        {
            Debug.Log("Set Controller -> "+ArchToolkitManager.Instance.managerContainer.archToolkitVRManager.ConnectedDevice);
            switch (ArchToolkitManager.Instance.managerContainer.archToolkitVRManager.ConnectedDevice)
            {
                case ConnectedDevice.Not_Established:
                    //Load Oculus by default
                    if (this.NodeType == XRNode.LeftHand)
                        this.InstantiateController(ArchToolkitDataPaths.RESOURCES_OCULUS_CONTROLLER_LEFT, onError);
                    else if (this.NodeType == XRNode.RightHand)
                        this.InstantiateController(ArchToolkitDataPaths.RESOURCES_OCULUS_CONTROLLER_RIGHT, onError);
                    break;
                case ConnectedDevice.None:
                    break;
                case ConnectedDevice.Htc_Vive:
                    this.InstantiateController(ArchToolkitDataPaths.RESOURCES_VIVE_CONTROLLER, onError);
                    break;
                case ConnectedDevice.Oculus_GO:
                    if (this.NodeType == XRNode.RightHand)
                        this.InstantiateController(ArchToolkitDataPaths.RESOURCES_OCULUS_GO_CONTROLLER, onError);
                    break;
                case ConnectedDevice.Oculus_Quest:
                case ConnectedDevice.Oculus_RiftS:
                    if (this.NodeType == XRNode.RightHand)
                        this.InstantiateController(ArchToolkitDataPaths.RESOURCES_OCULUS_QUEST_S_CONTROLLER_RIGHT, onError);
                    if (this.NodeType == XRNode.LeftHand)
                        this.InstantiateController(ArchToolkitDataPaths.RESOURCES_OCULUS_QUEST_S_CONTROLLER_LEFT, onError);
                    break;
                case ConnectedDevice.Oculus_Rift:
                    if (this.NodeType == XRNode.LeftHand)
                        this.InstantiateController(ArchToolkitDataPaths.RESOURCES_OCULUS_CONTROLLER_LEFT, onError);
                    else if (this.NodeType == XRNode.RightHand)
                        this.InstantiateController(ArchToolkitDataPaths.RESOURCES_OCULUS_CONTROLLER_RIGHT, onError);
                    break;
                case ConnectedDevice.Dell_Visor:
                case ConnectedDevice.Lenovo_Explorer:
                case ConnectedDevice.HP_Windows_Mixed_Reality:
                    if (this.NodeType == XRNode.LeftHand)
                        this.InstantiateController(ArchToolkitDataPaths.RESOURCES_WMX_CONTROLLER_LEFT, onError);
                    else if (this.NodeType == XRNode.RightHand)
                        this.InstantiateController(ArchToolkitDataPaths.RESOURCES_WMX_CONTROLLER_RIGHT, onError);
                    break;
                case ConnectedDevice.Mobile_Cardboard:
                    this.gameObject.SetActive(false);
                    this.rayOrigin.SetActive(false);
                    return;
                default:
                    break;
            }
        }

        protected virtual void InstantiateController(string path, Action onError)
        {
            try
            {
                var obj = GameObject.Instantiate(Resources.Load<GameObject>(path), Vector3.zero, Quaternion.identity, this.transform);
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localRotation = Quaternion.Euler(Vector3.zero);
                this.__trackingAcquired = true;

            }
            catch (Exception e)
            {
                if (onError != null)
                    onError();

                Debug.LogError("Attention: " + e.Message);
            }

        }

        protected virtual void CreateUIAnchor()
        {
            //Ui
            uiAnchor = new GameObject("UiAnchor");

            uiAnchor.transform.SetParent(this.transform);

            uiAnchor.transform.localPosition = Vector3.zero;
            uiAnchor.transform.localPosition = new Vector3(
                uiAnchor.transform.localPosition.x,
                 0.05f,
                0.08f
                );

            uiAnchor.transform.localRotation = Quaternion.Euler(Vector3.zero);
        }

        
        

        public virtual void OnControllerMoving()
        {

        }

        public virtual void OnTriggerPressed(Action OnClick, float triggerSensibility = 0.85f)
        {
            if (this.NodeType == XRNode.LeftHand)
            {
                if (!this.triggerPressed && InputListener.TriggerLeft >= triggerSensibility)
                {
                    this.triggerPressed = true;

                    if (OnClick != null)
                        OnClick();
                }
                else
                {
                    if (this.triggerPressed && InputListener.TriggerLeft < triggerSensibility)
                    {
                        this.triggerPressed = false;
                    }
                }
            }

            else if (this.NodeType == XRNode.RightHand)
            {
                if (!this.triggerPressed && InputListener.TriggerRight >= triggerSensibility)
                {
                    this.triggerPressed = true;

                    if (OnClick != null)
                        OnClick();
                }
                else
                {
                    if (this.triggerPressed && InputListener.TriggerRight < triggerSensibility)
                    {
                        this.triggerPressed = false;
                    }
                }
            }
        }

        public virtual void OnMenuPressed()
        {

        }

        public virtual void VibrateController()
        {

        }

        internal virtual void InitializeController()
        {
            Debug.Log("Initialize controller... "+TrackingAcquired);

            if (ArchToolkitManager.Instance.managerContainer.archToolkitVRManager == null)
            {
                Debug.LogWarning("VR manager is not instanced");
                return;
            }
            Debug.Log("Controller initialing...");
            #if !UNITY_WEBGL && !UNITY_STANDALONE_OSX && IS_LEGACY_INPUT_INSTALLED
            if (this.NodeType == XRNode.LeftHand)
            {
                var poseDriver = this.GetComponent<TrackedPoseDriver>();
                poseDriver.SetPoseSource(TrackedPoseDriver.DeviceType.GenericXRController, TrackedPoseDriver.TrackedPose.LeftPose);
                ArchToolkitManager.Instance.managerContainer.archToolkitVRManager.leftController = this;
                this.CreateUIAnchor();
                Debug.Log("Set left controller");
            }
            else if (this.NodeType == XRNode.RightHand)
            {
                this.GetComponent<TrackedPoseDriver>().SetPoseSource(TrackedPoseDriver.DeviceType.GenericXRController, TrackedPoseDriver.TrackedPose.RightPose);
                ArchToolkitManager.Instance.managerContainer.archToolkitVRManager.rightController = this;
                Debug.Log("Set right controller");
            }

            Debug.Log("This node tipe is " + this.NodeType);

            this.SetController(null);

            InputTracking.Recenter();
#endif

        }
    }
}