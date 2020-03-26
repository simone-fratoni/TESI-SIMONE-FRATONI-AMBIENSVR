
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if !UNITY_WEBGL && !UNITY_STANDALONE_OSX && IS_LEGACY_INPUT_INSTALLED
using UnityEngine.SpatialTracking;
#endif
using UnityEngine.XR;

namespace ArchToolkit.VR
{
    public enum ConnectedDevice
    {
        Not_Established,
        None,
        Htc_Vive,
        Oculus_Rift,
        Oculus_GO,
        Dell_Visor,
        Lenovo_Explorer,
        HP_Windows_Mixed_Reality,
        Mobile_Cardboard,
        Oculus_Quest,
        Oculus_RiftS
    }

    public enum JoystickConnected
    {
        Not_Established,
        Htc_Vive_Controller,
        Windows_Mixed_Controller,
        Oculus_Rift_Controller,
        Oculus_GO_Controller,
        Open_VR_Controller,
        Oculus_Quest_Controller,
    }

    public class ArchToolkitVRManager : MonoBehaviour
    {
        public ConnectedDevice ConnectedDevice
        {
            get
            {
                return this._connectedDevice;
            }
            protected set
            {
                this._connectedDevice = value;

                if(this._connectedDevice != ConnectedDevice.None)
                {
                    if (this.OnConnectedDevice != null)
                        this.OnConnectedDevice(this._connectedDevice);
                }
            }
        }

        public Action<ConnectedDevice> OnConnectedDevice;

        public bool isDeviceConnected;

        public bool isVrRunning;

        //[SerializeField]
        //public List<JoystickConnected> joysticksConnected = new List<JoystickConnected>();
        [SerializeField]
        private ConnectedDevice _connectedDevice = ConnectedDevice.Not_Established;
        
        public ArchVRControllerBase leftController;

        public ArchVRControllerBase rightController;

        public bool isTrackingAcquired = false;

        public List<string> supportedDevices = new List<string>();
        
        public bool HasRightHand()
        {
            return this.rightController != null && this.rightController.TrackingAcquired;
        }

        protected List<string> sdkSupported = new List<string> {
#if UNITY_ANDROID || UNITY_IOS
            "Cardboard",
#else
            "OpenVR","Oculus" 
#endif

            };

        private int sdkLoadedIndex = 0;

        protected virtual void Awake()
        {
            InputTracking.trackingLost += InputTracking_trackingLost;
            InputTracking.trackingAcquired += InputTracking_trackingAcquired;

            ArchToolkitManager.Instance.OnVisitorCreated += this.InitializeVR;
        }

        public virtual void InitializeVR()
        {

            // MOBILE DEVICES
            this.AddSupportedDevice("Oculus Go");

            this.AddSupportedDevice("Cardboard");
            //HTC VIVE
            this.AddSupportedDevice("Vive MV.");

            this.AddSupportedDevice("Vive MV");

            this.AddSupportedDevice("Vive. MV");
            //OCULUS RIFT
            this.AddSupportedDevice("Oculus Rift");

            this.AddSupportedDevice("Oculus Rift CV1");

            this.AddSupportedDevice("Oculus Rift CV2");
            // WINDOWS MIXED REALITY
            this.AddSupportedDevice("Lenovo Explorer");

            this.AddSupportedDevice("DELL VISOR");

            this.AddSupportedDevice("HP Windows Mixed Reality Headset");

            this.AddSupportedDevice("Oculus Quest");

            // FINISH

            StartCoroutine(this.SetDevice());

        }

        protected virtual void AddSupportedDevice(string deviceName)
        {
            if (!this.supportedDevices.Contains(deviceName))
                this.supportedDevices.Add(deviceName);
        }

        protected virtual void RemoveSupportedDevice(string deviceName)
        {
            if (this.supportedDevices.Contains(deviceName))
                this.supportedDevices.Remove(deviceName);
        }

        protected virtual void OnDestroy()
        {
            InputTracking.trackingLost -= InputTracking_trackingLost;

            InputTracking.trackingAcquired -= InputTracking_trackingAcquired;

            //this.joysticksConnected.Clear();

            if(ArchToolkitManager.IsInstanced())
                ArchToolkitManager.Instance.OnVisitorCreated -= this.InitializeVR;

            this.StopCoroutine(this.SetDevice());
        }

        protected virtual void OnApplicationQuit()
        {
            InputTracking.trackingLost -= InputTracking_trackingLost;

            InputTracking.trackingAcquired -= InputTracking_trackingAcquired;
        }

        protected virtual void InputTracking_trackingAcquired(XRNodeState obj)
        {

            Debug.Log("Tracking Acquired " + obj.nodeType);

            if (obj.nodeType == XRNode.Head)
                this.isTrackingAcquired = true;
            if(obj.nodeType == XRNode.LeftHand)
            {
                if (this.leftController != null)
                    this.leftController.TrackingAcquired = true;
            }
            if (obj.nodeType == XRNode.RightHand)
            {
                if (this.rightController != null)
                    this.rightController.TrackingAcquired = true;
            }
        }

        protected virtual void InputTracking_trackingLost(XRNodeState obj)
        {
            if (obj.nodeType == XRNode.Head)
                this.isTrackingAcquired = false;

            if (obj.nodeType == XRNode.LeftHand)
            {
                if (this.leftController != null)
                    this.leftController.TrackingAcquired = false;
            }
            if (obj.nodeType == XRNode.RightHand)
            {
                if (this.rightController != null)
                    this.rightController.TrackingAcquired = false;
            }
        }

        protected virtual void Update()
        {
            this.isDeviceConnected = XRDevice.isPresent;

            this.isVrRunning = XRSettings.isDeviceActive;
        }

        public virtual string GetCurrentDevice()
        {
            string device = "None";

            Debug.Log("Device => XRDevice.Model " + XRDevice.model +
                " , XRSettings.loadedDeviceName " + XRSettings.loadedDeviceName + 
                " , SystemInfo.deviceName " + SystemInfo.deviceName);

            if (XRDevice.isPresent)
                device = XRDevice.model;

            if (XRSettings.loadedDeviceName == "cardboard") // cardboard is identified with loadedDeviceName and not with XRDevice.model
                device = "Cardboard";

            if (SystemInfo.deviceName == "Oculus Quest")
                device = "Oculus Quest";

            

            Debug.Log("Device XR is " + device);

#if UNITY_EDITOR
            Debug.Log("Current Device is " + device);
#endif

            return device;
        }
        /*
        public virtual void SetJoysticks()
        {
            if (this.ConnectedDevice == ConnectedDevice.Not_Established ||
                this.ConnectedDevice == ConnectedDevice.None)
                return;

            var joysticks = Input.GetJoystickNames();

            if (joysticks == null)
                this.joysticksConnected.Clear();

            if (joysticks != null && joysticks.Length == 0)
                this.joysticksConnected.Clear();


            foreach (var pad in joysticks)
            {
                if (string.IsNullOrEmpty(pad))
                {
                    Debug.Log("no pads connected");
                    continue;
                }

                switch (this._connectedDevice)
                {
                    case ConnectedDevice.Not_Established:
                        this.joysticksConnected.Add(JoystickConnected.Open_VR_Controller);
                        break;
                    case ConnectedDevice.None:
                        break;
                    case ConnectedDevice.Htc_Vive:
                        this.joysticksConnected.Add(JoystickConnected.Htc_Vive_Controller);
                        break;
                    case ConnectedDevice.Oculus_Rift:
                        this.joysticksConnected.Add(JoystickConnected.Oculus_Rift_Controller);
                        break;
                    case ConnectedDevice.Dell_Visor:
                    case ConnectedDevice.Lenovo_Explorer:
                    case ConnectedDevice.HP_Windows_Mixed_Reality:
                        this.joysticksConnected.Add(JoystickConnected.Windows_Mixed_Controller);
                        break;
                    case ConnectedDevice.Mobile_Cardboard:
                        break;
                    case ConnectedDevice.Oculus_GO:
                        this.joysticksConnected.Add(JoystickConnected.Oculus_GO_Controller);
                        break;
                    case ConnectedDevice.Oculus_Quest:
                        //ADD HAND TRACKING PREFAB HERE
#if AVR_OCULUS_QUEST_HAND_TRACKING

#else
                        this.joysticksConnected.Add(JoystickConnected.Oculus_Quest_Controller);
#endif

                        break;
                    default:
                        break;
                }
            }
        }
        */
        protected virtual IEnumerator SetDevice()
        {
            while (this.ConnectedDevice == ConnectedDevice.None || this.ConnectedDevice == ConnectedDevice.Not_Established)
            {
                var device = this.GetCurrentDevice();
                Debug.Log("Set device "+device);
                if (this.supportedDevices.Contains(device))
                {
                    if (device == "DELL VISOR")
                        this.ConnectedDevice = ConnectedDevice.Dell_Visor;
                    else if (device == "Cardboard")
                        this.ConnectedDevice = ConnectedDevice.Mobile_Cardboard;
                    else if (device == "Vive MV.")
                        this.ConnectedDevice = ConnectedDevice.Htc_Vive;
                    else if (device == "Vive. MV")
                        this.ConnectedDevice = ConnectedDevice.Htc_Vive;
                    else if (device == "Vive MV")
                        this.ConnectedDevice = ConnectedDevice.Htc_Vive;
                    else if (device == "Oculus Rift")
                        this.ConnectedDevice = ConnectedDevice.Oculus_Rift;
                    else if (device == "Oculus Rift CV1")
                        this.ConnectedDevice = ConnectedDevice.Oculus_Rift;
                    else if (device == "Oculus Rift CV2")
                        this.ConnectedDevice = ConnectedDevice.Oculus_Rift;
                    else if (device == "HP Windows Mixed Reality Headset")
                        this.ConnectedDevice = ConnectedDevice.HP_Windows_Mixed_Reality;
                    else if (device == "Oculus Go")
                        this.ConnectedDevice = ConnectedDevice.Oculus_GO;
                    else if (device == "Lenovo Explorer")
                        this.ConnectedDevice = ConnectedDevice.Lenovo_Explorer;
                    else if (device == "Oculus Quest")
                    {
                        this.ConnectedDevice = ConnectedDevice.Oculus_Quest;
                        XRDevice.SetTrackingSpaceType(TrackingSpaceType.RoomScale);//WORKAROUND?

                    }
                    else
                        this.ConnectedDevice = ConnectedDevice.Not_Established;
                }
                else
                {
                    this.ConnectedDevice = ConnectedDevice.None;

                    if (this.sdkLoadedIndex < this.sdkSupported.Count)
                    {
                        var sdk = this.sdkSupported[this.sdkLoadedIndex];

                        yield return LoadDevice(sdk, true);

                        sdkLoadedIndex++;
                    }
                    else this.sdkLoadedIndex = 0;
                }

                yield return new WaitForEndOfFrame();
            }

            //if (this.ConnectedDevice != ConnectedDevice.None)
            //    this.SetJoysticks();
        }

        public virtual IEnumerator LoadDevice(string newDevice, bool enable)
        {
            Debug.Log("Loading Device " + newDevice);
            XRSettings.LoadDeviceByName(newDevice);
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            XRSettings.enabled = enable;
            Debug.Log("Loaded Device " + XRDevice.model);

        }


    }


}
