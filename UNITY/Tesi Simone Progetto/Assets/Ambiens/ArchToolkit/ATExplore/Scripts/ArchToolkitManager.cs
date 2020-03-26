using UnityEngine;
using System;
using ArchToolkit.Navigation;
using ArchToolkit.Character;
using ArchToolkit.Utils;
using ArchToolkit.VR;
using UnityEngine.EventSystems;
using ArchToolkit.Settings;

namespace ArchToolkit
{
    public enum MovementTypePerPlatform
    {
        FullScreen360,
        VR,
        FollowCamera
    }

    [Serializable]
    public class ArchToolkitManager : Singleton<ArchToolkitManager>
    {
        [Serializable]
        public struct ManagersContainer
        {
            public PathManager pathManager;
            
            public ArchToolkitVRManager archToolkitVRManager;
        }

        public ManagersContainer managerContainer = new ManagersContainer();

        public ArchSettings settings
        {
            get
            {
                if(useGlobalSettings){
                    if(_settings == null)
                        _settings = Resources.Load<ArchSettings>(ArchToolkitDataPaths.ARCH_SETTINGS);
                }
                else{
                    if(ForcedSceneSettings!=null)
                        _settings=ForcedSceneSettings;
                }
                return _settings;
            }
        }
        public bool useGlobalSettings=true;
        public ArchSettings ForcedSceneSettings;
        private ArchSettings _settings;
        public void ForceSettingsRefresh(){_settings=null;}

        public MovementTypePerPlatform movementTypePerPlatform
        {
            get
            {
                return this.settings.movementTypePerPlatform;
            }
            set
            {
                this.settings.movementTypePerPlatform = value;

                if (this.OnChangeMovementType != null)
                    this.OnChangeMovementType(this.settings.movementTypePerPlatform);
            }
        }
        
        public Action<MovementTypePerPlatform> OnChangeMovementType;

        public Action OnVisitorCreated;

        public ArchCharacter visitor;

        public Transform Tom;

        public Bounds sceneBounds=new Bounds();

        [SerializeField]
        private EventSystem eventSystem;
        
        public static ArchToolkitManager Factory()
        {
            if (!ArchToolkitManager.IsInstanced())
            {
                var archManager = new GameObject("ArchToolkitManager DON'T DESTROY THIS");

                ArchToolkitManager._instance = archManager.AddComponent<ArchToolkitManager>();
            }

            _instance.Init();

            return _instance;

        }

        protected virtual void Init()
        {
            if (this.managerContainer.pathManager == null)
            {
                this.managerContainer.pathManager = new PathManager();
            }


            if (this.managerContainer.archToolkitVRManager != null)
            {
                if (this.settings.movementTypePerPlatform != MovementTypePerPlatform.VR )
                {
                    GameObject.DestroyImmediate(this.managerContainer.archToolkitVRManager.gameObject);
                }
            }

            if ((this.settings.movementTypePerPlatform == MovementTypePerPlatform.VR ) &&
                this.managerContainer.archToolkitVRManager == null)
            {

                var archVR = GameObject.FindObjectOfType<ArchToolkitVRManager>();

                if (archVR == null)
                {
                    var goVR = new GameObject("ArchToolkitVRManager");

                    archVR = goVR.AddComponent<ArchToolkitVRManager>();
                }

                archVR.gameObject.transform.SetParent(this.transform);

                this.managerContainer.archToolkitVRManager = archVR;

            }

            if (this.eventSystem == null)
            {
                var eventSystemGO = GameObject.Instantiate(Resources.Load<GameObject>(ArchToolkitDataPaths.RESOURCES_EVENTSYSTEM));

                eventSystemGO.transform.SetParent(this.transform);

                this.eventSystem = eventSystemGO.GetComponent<EventSystem>();
            }
        }

        protected virtual void Start()
        {
            if(settings.StartAutomatically)
                this.IntitializeAll();
        }

        public virtual void IntitializeAll()
        {
            this.Init();

            if (this.managerContainer.pathManager.PathPoints.Count == 0 || this.managerContainer.pathManager.PathPoints[0] == null)
            {
                Debug.LogError("Please insert Tom (starting point)");
                return;
            }

            var startPoint = this.managerContainer.pathManager.PathPoints[0];

            if (startPoint != null)
            {
                this.Tom = startPoint.transform;
            }

            if (this.Tom == null)
            {
                Debug.LogError("Please insert Tom (starting point)");
                return;
            }

            this.LoadVisitor();

            this.OnChangeMovementType += this.VisitorChanged;

            //DontDestroyOnLoad(this);
        }

        protected virtual void VisitorChanged(MovementTypePerPlatform movementTypePerPlatform)
        {
            if(this.visitor != null)
            {
                GameObject.Destroy(visitor.gameObject);
            }

            this.LoadVisitor();
        }

        protected virtual void LoadVisitor()
        {
            GameObject character = null;

            var customArchChar = FindObjectOfType<CustomArchCharacter>();
            if (customArchChar != null)
            {
                customArchChar.Init(this.movementTypePerPlatform);
                character = customArchChar.gameObject;
            }
            else
            {
                if (FindObjectOfType<ArchCharacter>() != null) Destroy(FindObjectOfType<ArchCharacter>().gameObject);

                switch (this.movementTypePerPlatform)
                {
                    case MovementTypePerPlatform.VR:
                        character = GameObject.Instantiate(Resources.Load<GameObject>(ArchToolkitDataPaths.RESOURCES_CHARACTER_VR_PREFAB), this.Tom.transform.position, this.Tom.transform.rotation);
                        /*break;character = GameObject.Instantiate(Resources.Load<GameObject>(ArchToolkitDataPaths.RESOURCES_CHARACTER_VR_PREFAB), this.Tom.transform.position, this.Tom.transform.rotation); DA OCULUS QUEST*/
                        break;
                    case MovementTypePerPlatform.FollowCamera:
                        character = GameObject.Instantiate(Resources.Load<GameObject>(ArchToolkitDataPaths.RESOURCES_CHARACTER_FOLLOWCAM_PREFAB), this.Tom.transform.position, this.Tom.transform.rotation);
                        break;
                    default:
                        character = GameObject.Instantiate(Resources.Load<GameObject>(ArchToolkitDataPaths.RESOURCES_CHARACTER_PREFAB), this.Tom.transform.position, this.Tom.transform.rotation);
                        break;
                }
            }
            

            /* ENNIO
            if (this.managerContainer.archToolkitVRManager == null)
            {
                Debug.Log("VR PREFAB NULL");
                character = GameObject.Instantiate(Resources.Load<GameObject>(ArchToolkitDataPaths.RESOURCES_CHARACTER_PREFAB), this.Tom.transform.position, this.Tom.transform.rotation);
            }
            else
            {
                if (this.movementTypePerPlatform == MovementTypePerPlatform.VR)
                {
                    character = GameObject.Instantiate(Resources.Load<GameObject>(ArchToolkitDataPaths.RESOURCES_CHARACTER_VR_PREFAB), this.Tom.transform.position, this.Tom.transform.rotation);
                }
                else if (this.movementTypePerPlatform == MovementTypePerPlatform.VR_Mobile)
                {
                    character = GameObject.Instantiate(Resources.Load<GameObject>(ArchToolkitDataPaths.RESOURCES_CHARACTER_MOBILE_VR_PREFAB), this.Tom.transform.position, this.Tom.transform.rotation);
                }
            }
*/
            if (character == null)
            {
                Debug.LogError("Cannot instantiate visitor, something is gone wrong");
                return;
            }

            this.visitor = character.GetComponent<ArchCharacter>();

            /*if (this.movementTypePerPlatform == MovementTypePerPlatform.VR_Mobile)
                //this.managerContainer.pathManager.EnableAllPoint(true);//EDIT DA OCULUS QUEST SUPPORT
                this.managerContainer.pathManager.EnableAllPoint(this.settings.useGazeTimer);
            else
                this.managerContainer.pathManager.EnableAllPoint(false);
*/
            if (this.OnVisitorCreated != null)
                this.OnVisitorCreated();

        }

        protected virtual void Update()
        {
            if (InputSystem.InputListener.EscapePressed)
                this.DoExit(); 
        }

        public void DoExit()
        {
            if (settings.ExitCallbackOverride == null)
            {
#if !UNITY_EDITOR
                Application.Quit();
#else
                UnityEditor.EditorApplication.isPlaying = false;
#endif
                if(settings.OnExit!=null)
                    settings.OnExit();
            }
            else
            {
                if (settings.OnExit != null)
                    settings.OnExit();
                settings.ExitCallbackOverride();
            }
                
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            this.OnChangeMovementType = null;
            this.OnVisitorCreated = null;
        }

    }
}
