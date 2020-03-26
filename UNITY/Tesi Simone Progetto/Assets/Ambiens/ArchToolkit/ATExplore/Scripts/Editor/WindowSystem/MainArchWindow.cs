using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor.Build;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace ArchToolkit.Editor.Window
{
    [System.Serializable]
    public enum WindowStatus
    {
        Scene = 0,
        Interaction = 1,
        Physics = 2,
        Navigation = 3

    }

    public class PlatformChangedListener : IActiveBuildTargetChanged
    {
        public int callbackOrder { get { return 0; } }

        public static Action<BuildTarget> OnPlatformSwitched;

        public void OnActiveBuildTargetChanged(BuildTarget previousTarget, BuildTarget newTarget)
        {
            Debug.Log("Platform changed Succesfully to: " + newTarget);
            if (OnPlatformSwitched != null)
                OnPlatformSwitched(newTarget);
        }
    }

    [System.Serializable]
    [InitializeOnLoad]
    public class MainArchWindow : MainArchWindowBase
    {
        public static MainArchWindow Instance;

        public int selectedInspector = 0;

        public Action OnOpen;

        public IArchWindow CurrentWindow
        {
            get { return this.currentWindow; }
        }

        [SerializeField]
        private List<IArchWindow> archWindows = new List<IArchWindow>();

        [SerializeField]
        private List<IArchInspector> archInspectors = new List<IArchInspector>();

        private string[] tabs = new string[] { "Scene", "Interaction", "Physics" };

        private IArchWindow currentWindow;

        private int selectedWindow = 0;

        private Vector2 scroll;

        private GUIStyle mainAreaStyle;

        private GUIStyle inspectorStyle;

        private Rect inspectorRectArea;

        [SerializeField]
        private EditorUpdateManager updateManager;

        private GameObject _currentSelected;
        [SerializeField]
        private ArchToolkitManager atManager;

        [MenuItem("Tools/Ambiens/ArchToolkit/AT+Explore")]
        public static void Init()
        {
            var window = EditorWindow.GetWindow<MainArchWindow>("AT+Explore");

            Instance = window;

            Instance.GenerateWindows();

            Instance.GenerateInspector();

            if (Instance.atManager == null)
                Instance.atManager = ArchToolkitManager.Factory();

            Instance.mainAreaStyle = ArchToolkitWindowData.GetStyle(TextAnchor.UpperCenter);

            if (Instance.currentWindow == null)
                Instance.currentWindow = Instance.archWindows.Find(w => w.GetStatus == WindowStatus.Scene);

            EditorUtility.SetDirty(Instance);
        }

        public bool AddWindow(IArchWindow window)
        {
            if (this.archWindows.Exists(w => w.GetStatus == window.GetStatus))
                return false;

            this.archWindows.Add(window);
            return true;
        }

        public bool AddInspector(IArchInspector inspector)
        {
            if (this.archInspectors.Exists(i => i.Name == inspector.Name))
                return false;

            this.archInspectors.Add(inspector);
            return true;
        }

        private void GenerateInspector()
        {
            if (!this.archInspectors.Exists(i => i.Name == "Switch material inspector"))
                new ArchMaterialInspector("Switch material inspector");

            if (!this.archInspectors.Exists(i => i.Name == "Rotate Animation inspector"))
                new ArchDoorInspector("Rotate Animation inspector");

            if (!this.archInspectors.Exists(i => i.Name == "Translate Animation inspector"))
                new ArchDrawerInspector("Translate Animation inspector");

            if (!this.archInspectors.Exists(i => i.Name == "Scene Options"))
                new ArchCharacterInspector("Scene Options");

            //TODO-> Add Action Sequence inspector!
            
        }


        private void OnFocus()
        {
            if (this.archInspectors.Count > 0)
            {
                foreach (var inspector in this.archInspectors)
                {
                    inspector.OnFocus();
                }
            }
        }

        private void GenerateWindows()
        {
            new ArchBaseWindow(WindowStatus.Scene);

            new ArchAnimationWindow(WindowStatus.Interaction);

            new ArchPhysicsWindow(WindowStatus.Physics);
        }

        private void OnProjectChange()
        {
            //this.Repaint();

            foreach (var inspector in this.archInspectors)
            {
                if (inspector == null)
                    continue;

                inspector.OnProjectChange();
            }

            this.OnSelectionChange();
        }

        private void Update()
        {
            if (EditorApplication.isPlaying || EditorApplication.isPaused)
                return;


            if (EditorApplication.isCompiling)
            {
                this._currentSelected = null;
                return;
            }

            // Check that windows is null
            if (Instance == null) // this fix the bug when app compile, the window change to null
            {
                Init();

                return;
            }

            if (this.atManager == null)
            {
                this.atManager = ArchToolkitManager.Factory();
            }

            if (this.archInspectors.Count > 0)
            {
                foreach (var inspector in this.archInspectors)
                {
                    inspector.OnUpdate();
                }
            }

            if (this._currentSelected != Selection.activeGameObject || this._currentSelected == null) // if it changed while window was not on focus
            {
                this.OnSelectionChange();
            }
        }

        private void OnGUI()
        {
            if (Instance == null)
                return;


            if (this.inspectorStyle == null)
                this.inspectorStyle = new GUIStyle(GUI.skin.box);

            var window = this.currentWindow;

            if (this.archInspectors == null || this.archInspectors.Count <= 0)
                return;

            if (this.currentWindow == null)
            {
                this.selectedWindow = 0;
                this.currentWindow = this.archWindows[this.selectedWindow];
                this.currentWindow.OnOpen();
            }
            else
                this.selectedWindow = System.Convert.ToInt32(this.currentWindow.GetStatus);

            this.ApplyLogo();

            this.inspectorRectArea = new Rect(0, ArchToolkitWindowData.MainAreaAnchor, this.position.width, this.currentWindow.MaxWindowHeight);

            // Create the main area
            GUILayout.BeginArea( this.inspectorRectArea,this.mainAreaStyle);

            this.currentWindow = this.archWindows[GUILayout.Toolbar(this.selectedWindow, this.tabs, GUILayout.Height(ArchToolkitWindowData.TAB_HEIGHT))];

            if (window != this.currentWindow || window == null)
            {
                if (this.currentWindow != null)
                {
                    this.currentWindow.OnOpen();
                }

                if (window != null)
                {
                    window.OnClose();
                }
            }

            GUILayout.Space(ArchToolkitWindowData.PADDING);

            if (this.currentWindow != null)
                this.currentWindow.DrawGUI();
            else
            {
                Debug.LogWarning("No window found");

                this.currentWindow = this.archWindows[0];

                this.currentWindow.DrawGUI();
            }

            // end the main area
            GUILayout.EndArea();

            if (ArchToolkitManager.IsInstanced())
            {
                GUILayout.BeginArea(new Rect(0, ArchToolkitWindowData.MainAreaAnchor + this.currentWindow.MaxWindowHeight, this.position.width, this.position.height - 50));

                GUILayout.BeginVertical();

                if (this.archInspectors.Count > 0)
                {
                    var scrollbarStyle = new GUIStyle(GUI.skin.horizontalScrollbar);

                    scrollbarStyle.fixedHeight = scrollbarStyle.fixedWidth = 0;

                    scroll = EditorGUILayout.BeginScrollView(scroll, scrollbarStyle, GUI.skin.verticalScrollbar, GUILayout.Width(this.position.width), GUILayout.Height(this.position.height - (ArchToolkitWindowData.MainAreaAnchor + this.currentWindow.MaxWindowHeight)));

                    foreach (var inspector in this.archInspectors)
                    {
                        if (inspector.IsInspectorVisible())
                        {
                            GUILayout.BeginVertical(this.inspectorStyle);

                            GUILayout.Space(ArchToolkitWindowData.PADDING);

                            inspector.OnGui();

                            GUILayout.EndVertical();
                        }
                    }

                    EditorGUILayout.EndScrollView();
                }

                GUILayout.EndVertical();

                GUILayout.EndArea();
            }
        }

        private void OnEnable()
        {
            if (!ArchToolkitManager.IsInstanced())
            {
                ArchToolkitManager.Factory();
            }


#if UNITY_2019_1_OR_NEWER
            UnityEditor.PackageManager.UI.PackageManagerExtensions.RegisterExtension(new Setup.InputAndTagSetup());
#endif
            new Setup.InputAndTagSetup().Setup();

            if (this.currentWindow != null)
                this.currentWindow.OnOpen();

            if (this.archInspectors.Count > 0)
            {
                foreach (var inspector in this.archInspectors)
                {
                    inspector.OnEnable();
                }
            }

            if (this.updateManager == null)
            {
                if (!EditorUpdateManager.IsInstanced()) // if is not instanced , create a new EditorUpdatemanager
                    this.updateManager = new EditorUpdateManager();
                else
                    this.updateManager = EditorUpdateManager.Instance;
            }
            else
            {
                this.updateManager.RefreshAllActions();
            }

            if (this.OnOpen != null)
                this.OnOpen();

            this.Repaint();

            Selection.selectionChanged += this.Repaint;

            EditorUtility.SetDirty(this);
        }

        private void OnSelectionChange()
        {
            this._currentSelected = Selection.activeGameObject;

            if (this.currentWindow != null)
            {
                this.currentWindow.OnSelectionChange(Selection.activeGameObject);
            }

            if (this.archInspectors.Count > 0)
            {
                foreach (var inspector in this.archInspectors)
                {
                    inspector.OnSelectionChange(Selection.activeGameObject);
                }
            }

            this.Repaint();

        }

        private void OnDestroy()
        {

            if (this.archInspectors.Count > 0)
            {
                foreach (var inspector in this.archInspectors)
                {
                    inspector.OnClose();
                }
            }

            this.archInspectors.Clear();

            this.archWindows.Clear();

            this.OnOpen = null;

            Selection.selectionChanged -= this.Repaint;

            Instance = null;
        }
    }
}