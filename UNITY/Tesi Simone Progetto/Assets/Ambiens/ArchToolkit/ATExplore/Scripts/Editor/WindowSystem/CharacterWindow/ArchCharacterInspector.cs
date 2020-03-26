using System.Collections.Generic;
using UnityEngine;
using ArchToolkit.Navigation;
using UnityEditor;
using ArchToolkit.Character;
using System.Linq;
using UnityEditorInternal;
using UnityEngine.XR;
using ArchToolkit.Settings;
using UnityEditor.SceneManagement;
using ArchToolkit.Editor.Utils;
using System.Reflection;
using System;

namespace ArchToolkit.Editor.Window
{
    internal enum VisualizationType
    {
        VR,
        Mode360
    }

    internal enum TargetNativePlatformSupported
    {
        Android,
        Ios,
        PC,
        Mac,
        WebGL
    }



    public class ArchCharacterInspector : ArchInspectorBase
    {
        private GameObject _selectedGameObject;

        private bool _isVisibleOnlyInSceneTab = true;

        [SerializeField]
        private TargetNativePlatformSupported targetNativePlatformSupported;

        [SerializeField]
        private VisualizationType visualizationType = VisualizationType.Mode360;

        [SerializeField]
        private int visualizationIndexPopup = 0;

        private int tempLayerMask = 0;

        private Dictionary<string, bool> vrMobileApiSupported = new Dictionary<string, bool> { { "cardboard", false }, { "Oculus", false } };

        private bool tempIsOculus = false;

        public ArchCharacterInspector(string name) : base(name)
        {
            EditorApplication.playModeStateChanged += EditorStateChanged;

            if (EditorPrefs.HasKey("VisualizationType"))
                this.visualizationType = (VisualizationType)EditorPrefs.GetInt("VisualizationType");
            else
                EditorPrefs.SetInt("VisualizationType", (int)this.visualizationType);

            this.CheckPlatform(ref this.targetNativePlatformSupported);

            PlatformChangedListener.OnPlatformSwitched += this.PlatformSwitched;

            var target = EditorUserBuildSettings.activeBuildTarget;
            var group = BuildPipeline.GetBuildTargetGroup(target);
            this.tempIsOculus = this.isOculusFirst(group);

            for (int i = 0; i < this.vrMobileApiSupported.Count; i++)
            {
                var sdk = this.vrMobileApiSupported.Keys.ToList()[i];

                if (EditorPrefs.HasKey("Arch" + sdk))
                {
                    this.vrMobileApiSupported[sdk] = EditorPrefs.GetBool("Arch" + sdk);
                }
            }

        }

        private void EditorStateChanged(PlayModeStateChange obj)
        {
            switch (obj)
            {
                case PlayModeStateChange.EnteredEditMode:
                    break;
                case PlayModeStateChange.ExitingEditMode:
                    this.Apply(false);
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    break;
                default:
                    break;
            }
        }

        protected virtual void PlatformSwitched(BuildTarget buildTarget)
        {
            this.CheckPlatform(ref this.targetNativePlatformSupported, buildTarget);
        }

        public override bool IsInspectorVisible()
        {
            if (!this._isVisibleOnlyInSceneTab)
            {
                if (_selectedGameObject == null)
                {
                    this.isInspectorVisible = false;
                    return false;
                }
                var selectedPoint = _selectedGameObject.GetComponent<PathPoint>();

                if (selectedPoint != null)
                {
                    if (selectedPoint.ID == 0)
                    {
                        this.isInspectorVisible = true;
                        return true;
                    }
                }

                this.isInspectorVisible = false;
                return false;
            }
            else
            {
                if (MainArchWindow.Instance.CurrentWindow.GetStatus == WindowStatus.Scene)
                {
                    this.isInspectorVisible = true;
                    return true;
                }
            }

            this.isInspectorVisible = false;
            return false;

        }

        public override void OnClose()
        {
            base.OnClose();

            PlatformChangedListener.OnPlatformSwitched -= this.PlatformSwitched;

            EditorApplication.playModeStateChanged -= this.EditorStateChanged;
        }

        public override void OnEnable()
        {
            var target = EditorUserBuildSettings.activeBuildTarget;
            var group = BuildPipeline.GetBuildTargetGroup(target);
            this.tempIsOculus = this.isOculusFirst(group);
        }

        public override void OnFocus()
        {

            //this.CheckPlatform(ref this.targetNativePlatformSupported);
            var target = EditorUserBuildSettings.activeBuildTarget;
            var group = BuildPipeline.GetBuildTargetGroup(target);
            this.tempIsOculus = this.isOculusFirst(group);

            this.visualizationType = (VisualizationType)EditorPrefs.GetInt("VisualizationType");
        }

        public override void OnGui()
        {
            base.OnGui();

            var target = EditorUserBuildSettings.activeBuildTarget;
            var group = BuildPipeline.GetBuildTargetGroup(target);

            if (!this.inspectorFoldoutOpen)
                return;

            if (EditorApplication.isPlaying || EditorApplication.isPaused)
                return;

            if (!ArchToolkitManager.IsInstanced())
            {
                GUILayout.Label("Arch toolkit manager is not instanced");

                ArchToolkitManager.Factory();
            }
            else
            {
                if (ArchToolkitManager.Instance.settings == null)
                {
                    EditorGUILayout.LabelField("Please, you need to create a new Settings");
                    return;
                }

                EditorGUILayout.LabelField("Movement Options", EditorStyles.boldLabel);

                GUILayout.Space(2);

                bool prevGlobalSettings = ArchToolkitManager.Instance.useGlobalSettings;
                ArchToolkitManager.Instance.useGlobalSettings = EditorGUILayout.Toggle("Use Global Settings:", ArchToolkitManager.Instance.useGlobalSettings);

                if (prevGlobalSettings != ArchToolkitManager.Instance.useGlobalSettings)
                {
                    if (ArchToolkitManager.Instance.useGlobalSettings)
                    {
                        ArchToolkitManager.Instance.ForcedSceneSettings = null;
                        ArchToolkitManager.Instance.ForceSettingsRefresh();
                    }
                    else
                    {
                        var currentScene = EditorSceneManager.GetActiveScene();

                        var p = currentScene.path.Replace(currentScene.name + ".unity", "");
                        var n = currentScene.name + "_settings";
                        var found = AssetDatabase.FindAssets(n, new[] { p.Remove(p.Length - 1) });
                        if (found.Count() > 0)
                        {
                            ArchToolkitManager.Instance.ForcedSceneSettings = AssetDatabase.LoadAssetAtPath<ArchSettings>(AssetDatabase.GUIDToAssetPath(found[0]));
                        }
                        else
                        {
                            var s = EditorUtils.CreateAsset<ArchSettings>(n, p);
                            ArchToolkitManager.Instance.ForcedSceneSettings = s;
                        }
                    }
                    EditorUtility.SetDirty(ArchToolkitManager.Instance.gameObject);
                }


                ArchToolkitManager.Instance.settings.movementSpeed = EditorGUILayout.FloatField("Movement Speed:", ArchToolkitManager.Instance.settings.movementSpeed);
                ArchToolkitManager.Instance.settings.clumbSpeed = EditorGUILayout.FloatField("Climb Speed:", ArchToolkitManager.Instance.settings.clumbSpeed);
                ArchToolkitManager.Instance.settings.MouseRotationSpeed = EditorGUILayout.FloatField("Mouse Rotation Speed:", ArchToolkitManager.Instance.settings.MouseRotationSpeed);
                ArchToolkitManager.Instance.settings.TouchRotationSpeed = EditorGUILayout.FloatField("Touch Rotation Speed:", ArchToolkitManager.Instance.settings.TouchRotationSpeed);

                ArchToolkitManager.Instance.settings.RunFaster = EditorGUILayout.FloatField("Run Faster:", ArchToolkitManager.Instance.settings.RunFaster);
                ArchToolkitManager.Instance.settings.RunSpeed = EditorGUILayout.FloatField("Run Speed:", ArchToolkitManager.Instance.settings.RunSpeed);

                this.tempLayerMask = EditorGUILayout.MaskField(new GUIContent("Walkable layers: ", ArchToolkitText.WALKABLE_LAYERS_TOOLTIP), InternalEditorUtility.LayerMaskToConcatenatedLayersMask(ArchToolkitManager.Instance.settings.walkableLayers.value),
                                                                                                                      InternalEditorUtility.layers);

                ArchToolkitManager.Instance.settings.walkableLayers = InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(this.tempLayerMask);
                this.tempLayerMask = InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(this.tempLayerMask);
                ArchToolkitManager.Instance.settings.walkableLayers = this.tempLayerMask;

                GUILayout.Space(2);

                EditorGUILayout.LabelField("Visualization Options", EditorStyles.boldLabel);
                bool prevVR = PlayerSettings.virtualRealitySupported;
                PlayerSettings.virtualRealitySupported = EditorGUILayout.Toggle("VR Supported", PlayerSettings.virtualRealitySupported);

                if (prevVR != PlayerSettings.virtualRealitySupported)
                {
                    //Force VR supported for all platform to avoid any error when you switch platform!
                    PlayerSettings.SetVirtualRealitySupported(BuildTargetGroup.iOS, PlayerSettings.virtualRealitySupported);
                    PlayerSettings.SetVirtualRealitySupported(BuildTargetGroup.Android, PlayerSettings.virtualRealitySupported);
                }
                if (EditorPrefs.HasKey("VisualizationType"))
                {
                    this.visualizationIndexPopup = EditorPrefs.GetInt("VisualizationType");
                }

                GUILayout.Space(2);
                this.visualizationType = VisualizationType.Mode360;


#if IS_LEGACY_INPUT_INSTALLED
                if (PlayerSettings.virtualRealitySupported)
                {

                    bool singlePass = PlayerSettings.stereoRenderingPath == StereoRenderingPath.SinglePass;
                    bool newSinglePassVal = EditorGUILayout.Toggle("Use SinglePass Stereo", singlePass);
                    if (singlePass != newSinglePassVal)
                    {
                        if (newSinglePassVal) PlayerSettings.stereoRenderingPath = StereoRenderingPath.SinglePass;
                        else PlayerSettings.stereoRenderingPath = StereoRenderingPath.MultiPass;
                    }

                    if (this.targetNativePlatformSupported != TargetNativePlatformSupported.WebGL)
                    {
                        this.visualizationIndexPopup = EditorGUILayout.Popup("Start Visualization Type: ", this.visualizationIndexPopup, new string[] { "VR", "360Mode" });
                        this.visualizationType = (VisualizationType)this.visualizationIndexPopup;

                        EditorPrefs.SetInt("VisualizationType", this.visualizationIndexPopup);
                    }
                    if (this.visualizationType == VisualizationType.VR)
                    {

                        this.tempIsOculus = EditorGUILayout.Toggle("Oculus (GO, Quest, Rift...)", this.tempIsOculus);

                        if (this.tempIsOculus)
                        {
#if UNITY_ANDROID
                            bool isQuest = EditorPrefs.GetBool("IsOculusQuest", false);

                            isQuest = EditorGUILayout.Toggle("Oculus Quest", isQuest);
                            EditorPrefs.SetBool("IsOculusQuest", isQuest);

                            /* EXP - HAND TRACKING
                            if (isQuest)
                            {

                                bool IsHandTrackingEnabled = EditorPrefs.GetBool("IsHandTrackingEnabled", false);
                                if (!this.HasType("OVRHand"))
                                {
                                    IsHandTrackingEnabled = false;
                                    EditorPrefs.SetBool("IsHandTrackingEnabled", false);
                                }

                                IsHandTrackingEnabled = EditorGUILayout.Toggle("[BETA] Hand Tracking", IsHandTrackingEnabled);

                                if (IsHandTrackingEnabled)
                                {
                                    if (this.HasType("OVRHand"))
                                    {
#if !AVR_OCULUS_QUEST_HAND_TRACKING
                                        Setup.InputAndTagSetup.AddScriptingSymbol("AVR_OCULUS_QUEST_HAND_TRACKING");
#endif
                                    }
                                    else
                                    {
                                        EditorPrefs.SetBool("IsHandTrackingEnabled", false);
#if AVR_OCULUS_QUEST_HAND_TRACKING
                                        Setup.InputAndTagSetup.RemoveScriptingSymbol("AVR_OCULUS_QUEST_HAND_TRACKING");
#endif
                                        EditorUtility.DisplayDialog("Oculus Quest Hand Tracking support", "In order to add Hand Tracking support you need to install the latest version of Oculus Integration from the Unity Asset Store!", "Got it!");
                                    }
                                }
                                else
                                {
#if AVR_OCULUS_QUEST_HAND_TRACKING
                                    Setup.InputAndTagSetup.RemoveScriptingSymbol("AVR_OCULUS_QUEST_HAND_TRACKING");
#endif
                                }
                                EditorPrefs.SetBool("IsHandTrackingEnabled", IsHandTrackingEnabled);
                            }*/
#else
                            var labelStyle = new GUIStyle(GUI.skin.label);

                            //labelStyle.fontSize = 12;
                            labelStyle.wordWrap = true;
                            EditorGUILayout.LabelField("If you want to target Oculus Go or Quest you need to Switch Platform to Android.", labelStyle);
#endif
                        }
                        else 
                            EditorPrefs.SetBool("IsOculusQuest", false);
                    }
                    else 
                        EditorPrefs.SetBool("IsOculusQuest", false);
                }
#else
                if (NamespaceExists("UnityEngine.SpatialTracking"))
                {
                    Setup.InputAndTagSetup.AddScriptingSymbol();
                }
                if(PlayerSettings.virtualRealitySupported)
                {
                    var xrStyle = new GUIStyle(GUI.skin.label);

                    xrStyle.normal.textColor = Color.red;
                    xrStyle.fontSize = 12;
                    xrStyle.wordWrap = true;
                    GUILayout.Label("Warning! You cannot use VR in Unity 2019 without XR Legacy Input Helpers.", xrStyle);
                    this.visualizationType = VisualizationType.Mode360;         
                }
#endif

                if (this.visualizationType == VisualizationType.Mode360)
                {
                    ArchToolkitManager.Instance.movementTypePerPlatform = MovementTypePerPlatform.FullScreen360;

                    ArchToolkitManager.Instance.settings.lockMovementTo = (LockMovementTo)EditorGUILayout.EnumPopup("Lock Movement:", ArchToolkitManager.Instance.settings.lockMovementTo);

                    if (ArchToolkitManager.Instance.settings.lockMovementTo == LockMovementTo.None)
                    {
                        ArchToolkitManager.Instance.settings.movementType = (MovementType)EditorGUILayout.EnumPopup("Movement Type:", ArchToolkitManager.Instance.settings.movementType);
                    }
                }

                GUILayout.Space(2);

                EditorGUILayout.LabelField("Platform Options", EditorStyles.boldLabel);

                GUILayout.Space(2);

                this.targetNativePlatformSupported = (TargetNativePlatformSupported)EditorGUILayout.EnumPopup("Platform: ", this.targetNativePlatformSupported);

                EditorGUILayout.BeginHorizontal();
                GUI.backgroundColor = ArchToolkitWindowData.ApplyColorButton;

                if (GUILayout.Button(ArchToolkitText.APPLY))
                {
                    this.Apply(this.tempIsOculus);
                    EditorUtility.SetDirty(ArchToolkitManager.Instance.settings);
                    AssetDatabase.SaveAssets();
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        private void SetVRDevices(bool StartAsVR, bool isOculusBuild = false)
        {

            this.SetVRDevicesForPlatform(StartAsVR, isOculusBuild, BuildTargetGroup.Standalone);
            this.SetVRDevicesForPlatform(StartAsVR, isOculusBuild, BuildTargetGroup.Android);
            this.SetVRDevicesForPlatform(StartAsVR, isOculusBuild, BuildTargetGroup.iOS);

            if (!StartAsVR)
            {
                if (ArchToolkitManager.Instance.managerContainer.archToolkitVRManager != null)
                    GameObject.DestroyImmediate(ArchToolkitManager.Instance.managerContainer.archToolkitVRManager.gameObject);

            }
            else
            {
#if AVR_OCULUS_QUEST_HAND_TRACKING
                CheckOrInitOculusQuestHandTracking();
#endif
            }

        }

        bool isOculusFirst(BuildTargetGroup group)
        {

            var currentTargetDevices = PlayerSettings.GetVirtualRealitySDKs(group);
            if (currentTargetDevices.Count() == 0)
            {
                return false;
            }
            if (currentTargetDevices[0] == "Oculus")
            {
                return true;
            }
            return false;
        }
        void SetVRDevicesForPlatform(bool StartAsVR, bool isOculusBuild, BuildTargetGroup group)
        {
            var currentTargetDevices = PlayerSettings.GetVirtualRealitySDKs(group);

            if (!StartAsVR)
            {
                if (currentTargetDevices.Count() == 0)
                {
                    currentTargetDevices = new string[] { "None" };
                }
                else
                {
                    if (currentTargetDevices[0] != "None")
                    {
                        var aux = new string[currentTargetDevices.Count() + 1];
                        aux[0] = "None";
                        for (int i = 0; i < currentTargetDevices.Count(); i++) aux[i + 1] = currentTargetDevices[i];
                        currentTargetDevices = aux;
                    }
                }
            }
            else
            {

                if (group == BuildTargetGroup.Standalone)
                {
                    if (isOculusBuild)
                        currentTargetDevices = new string[] { "Oculus", "OpenVR" };
                    else
                        currentTargetDevices = new string[] { "OpenVR", "Oculus" };
                }
                else if (group == BuildTargetGroup.Android)
                {
                    if (isOculusBuild)
                        currentTargetDevices = new string[] { "Oculus" };
                    else
                        currentTargetDevices = new string[] { "cardboard" };
                }
                else if (group == BuildTargetGroup.iOS)
                    currentTargetDevices = new string[] { "cardboard" };

                /*else{
                    if(currentTargetDevices[0]=="None")
                    {
                        if(group== BuildTargetGroup.Standalone)
                            currentTargetDevices=new string[]{"OpenVR","Oculus"};
                        else if(group == BuildTargetGroup.Android)
                            currentTargetDevices=new string[]{"Cardboard","Oculus"};
                        else if(group == BuildTargetGroup.iOS)
                            currentTargetDevices=new string[]{"Cardboard"};
                    }
                }*/
            }

            PlayerSettings.SetVirtualRealitySDKs(group, currentTargetDevices);
        }
        internal virtual void CheckPlatform(ref TargetNativePlatformSupported targetNativePlatformSupported, BuildTarget? buildTarget = null)
        {
            if (buildTarget == null)
                buildTarget = EditorUserBuildSettings.activeBuildTarget;

            switch (buildTarget)
            {
#if UNITY_2017
                case BuildTarget.StandaloneOSXUniversal:
                    targetNativePlatformSupported = TargetNativePlatformSupported.Mac;
                    break;
#else
                case BuildTarget.StandaloneOSX:
                    targetNativePlatformSupported = TargetNativePlatformSupported.Mac;
                    break;
#endif
                case BuildTarget.StandaloneWindows:
                    targetNativePlatformSupported = TargetNativePlatformSupported.PC;
                    break;
                case BuildTarget.iOS:
                    targetNativePlatformSupported = TargetNativePlatformSupported.Ios;
                    break;
                case BuildTarget.Android:
                    targetNativePlatformSupported = TargetNativePlatformSupported.Android;
                    break;
                case BuildTarget.StandaloneWindows64:
                    targetNativePlatformSupported = TargetNativePlatformSupported.PC;
                    break;
                case BuildTarget.WebGL:
                    targetNativePlatformSupported = TargetNativePlatformSupported.WebGL;
                    this.visualizationType = VisualizationType.Mode360;
                    break;
                default:
                    break;
            }
        }

        public void Apply(bool isOculusBuild = false)
        {
            EditorPrefs.SetInt("VisualizationType", (int)this.visualizationType);

            if (this.visualizationType == VisualizationType.VR)
            {
                ArchToolkitManager.Instance.movementTypePerPlatform = MovementTypePerPlatform.VR;

                this.SetVRDevices(true, isOculusBuild);
            }

            if (this.visualizationType == VisualizationType.Mode360)
            {
                if (ArchToolkitManager.Instance.settings.lockMovementTo == LockMovementTo.Classic) // Force to classic
                    ArchToolkitManager.Instance.settings.movementType = MovementType.Classic;
                else if (ArchToolkitManager.Instance.settings.lockMovementTo == LockMovementTo.FlyCam) // Force to flycam
                    ArchToolkitManager.Instance.settings.movementType = MovementType.FlyCam;

                this.SetVRDevices(false);
            }

            this.SwitchPlatform();
        }

        private void SwitchPlatform()
        {
            switch (this.targetNativePlatformSupported)
            {
                case TargetNativePlatformSupported.WebGL:
                    EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);
                    break;
                case TargetNativePlatformSupported.Android:
                    EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
                    break;
                case TargetNativePlatformSupported.Ios:
                    EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS);
                    break;
                case TargetNativePlatformSupported.PC:
                    EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
                    break;
#if UNITY_2017
                case TargetNativePlatformSupported.Mac:
                    EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneOSXUniversal);
                    break;
#else
                case TargetNativePlatformSupported.Mac:
                    EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneOSX);
                    break;
#endif
                default:
                    break;
            }
        }

        public override void OnSelectionChange(GameObject gameObject)
        {
            this._selectedGameObject = gameObject;

            //this.CheckPlatform(ref this.targetNativePlatformSupported);
        }

        public override void OnUpdate()
        {

        }

        private bool IsOnMobile()
        {
            return (this.targetNativePlatformSupported == TargetNativePlatformSupported.Android ||
                    this.targetNativePlatformSupported == TargetNativePlatformSupported.Ios);
        }
        public bool HasType(string typeName)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.Name == typeName)
                        return true;
                }
            }

            return false;
        }
        public bool NamespaceExists(string desiredNamespace)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.Namespace == desiredNamespace)
                        return true;
                }
            }
            return false;
        }

#if AVR_OCULUS_QUEST_HAND_TRACKING
        public static void CheckOrInitOculusQuestHandTracking()
        {
            var hands = GameObject.FindObjectsOfType<OVRHand>();
            if (hands.Length == 0)
            {
                var handPrefab = AssetDatabase.FindAssets("AVR_OVRHandPrefab");
                if (handPrefab.Length > 0)
                {
                    var handP = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(handPrefab[0]));
                    var go=GameObject.Instantiate(handP);
                }
            }
        }
#endif
    }
}
