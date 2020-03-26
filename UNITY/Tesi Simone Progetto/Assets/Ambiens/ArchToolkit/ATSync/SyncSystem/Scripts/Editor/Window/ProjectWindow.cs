using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using Archtoolkit.ATImport.Utils;
using AmbiensServer.LocalClient;
using AmbiensServer.Server;
using System.Linq;
using ambiens.avrs.view;
using ambiens.avrs.controller;
using ambiens.avrs.model;
using ambiens.utils.loader;
using ambiens.avrs.loader;
using ambiens.avrs;
using ambiens.utils.common;
using Newtonsoft.Json;
using UnityEditor.SceneManagement;
using System.Diagnostics;
using UnityEngine.Rendering;

namespace Archtoolkit.ATImport
{
    public class ProjectWindow : ProjectWindowBase
    {
        public static ProjectWindow projectWindow;
        public List<MATProject> avrsProjects = new List<MATProject>();
  

        private Vector2 scrollviewProject = Vector2.zero;

        public List<Avrs> toImport = new List<Avrs>();
        


        [MenuItem("Tools/Ambiens/ArchToolkit/AT+Sync")]
        public static void Init()
        {
            var window = (ProjectWindow)EditorWindow.GetWindow(typeof(ProjectWindow), false, "AT+Sync");

            window.maxSize = new Vector2(600, 600);
            window.minSize = new Vector2(600, 600);

            projectWindow = window;

            window.Show();
        }

        

        protected override void AssignProjects()
        {
            foreach (var item in this.avrsProjects)
            {
                if (item == null)
                    continue;

                if (item.views.Exists(a => a.isImporting))
                    return;
            }

            this.avrsProjects = this.GetProjects(projectsPath);
        }

        private void OnDestroy()
        {
            EditorApplication.update -= this.CustomUpdate;
#if UNITY_2018_1_OR_NEWER
            EditorApplication.projectChanged -= this.AssignProjects;
#endif
        }
        
        protected override void CustomUpdate()
        {
            if (RuntimeMeshLoader.IsInstanced())
            {
                RuntimeMeshLoader.Instance.Update();
            }

            if (RuntimeMeshDataOrderedLoader.IsInstanced())
            {
                RuntimeMeshDataOrderedLoader.Instance.Update();
            }
            if (RuntimeTextureLoader.IsInstanced())
            {
                RuntimeTextureLoader.Instance.Update();
            }
            if (RuntimeJSonLoader.IsInstanced())
            {
                RuntimeJSonLoader.Instance.Update();
            }

            if (this.currentlyImporting != null)
            {
                if (CheckInstantiationPercentage())
                {
                    this.currentlyImporting.isImporting = false;
                    this.currentlyImporting = null;
                }
            }

            CheckToImportList();
        }

        protected override void ProjectChanged(Avrs avrsChanged)
        {
            UnityEngine.Debug.Log("Project Changed "+avrsChanged.projectPath);
            toImport.Add(avrsChanged);
        }

        private void CheckToImportList()
        {
            if (this.currentlyImporting == null)
            {
                if (toImport.Count > 0)
                {
                    var sceneToImport = toImport[0];
                    toImport.RemoveAt(0);

                    this.Import(sceneToImport);
                }
            }
        }
        protected override void OnPreImport(AvrsDeserializer deserializer)
        {
            new BimDataComponent(deserializer.sceneController);

            if (GraphicsSettings.renderPipelineAsset != null)
            {
                UnityEngine.Debug.Log(GraphicsSettings.renderPipelineAsset.name);

                if (GraphicsSettings.renderPipelineAsset.name.ToLower().Contains("hd"))
                {
                    CMaterial.CurrentRenderPipeline = CMaterial.RenderPipelineType.HDRP;
                    //Very brutal check for HDRP Preview material
                    var s = Shader.Find(CMaterial.materialMaps[CMaterial.CurrentRenderPipeline][MaterialType.Standard]);
                    if (s == null)
                    {
                        CMaterial.materialMaps[CMaterial.CurrentRenderPipeline][MaterialType.Standard] = "HDRenderPipeline/Lit";
                        CMaterial.materialMaps[CMaterial.CurrentRenderPipeline][MaterialType.Transparent] = "HDRenderPipeline/Lit";

                    }
                }
                else if (GraphicsSettings.renderPipelineAsset.name.ToLower().Contains("universal"))
                {
                    CMaterial.CurrentRenderPipeline = CMaterial.RenderPipelineType.URP;
                }
                else if (GraphicsSettings.renderPipelineAsset.name.ToLower().Contains("lwrp"))
                {
                    CMaterial.CurrentRenderPipeline = CMaterial.RenderPipelineType.URP;
                    //Very brutal check for LWRP Preview material
                    var s = Shader.Find(CMaterial.materialMaps[CMaterial.CurrentRenderPipeline][MaterialType.Standard]);
                    if (s == null)
                    {
                        CMaterial.materialMaps[CMaterial.CurrentRenderPipeline][MaterialType.Standard] = "Lightweight Render Pipeline/Lit";
                        CMaterial.materialMaps[CMaterial.CurrentRenderPipeline][MaterialType.Transparent] = "Lightweight Render Pipeline/Lit";

                    }
                }
            }
            
        }
        protected override void OnPreSyncGO(GameObject go)
        {
            // Refresh Components
            var bimDatas = go.GetComponents<BimData>();

            foreach (var bim in bimDatas)
            {
                if (bim == null)
                    continue;

                GameObject.DestroyImmediate(bim);
            }
        }
        private bool CheckInstantiationPercentage()
        {
            float tPerc = 1;
            float mPerc = 1;
            if (RuntimeMeshLoader.IsInstanced())
            {
                if (RuntimeMeshLoader.Instance.toRequest != 0)
                    mPerc = (float)RuntimeMeshLoader.Instance.requested / (float)RuntimeMeshLoader.Instance.toRequest;
            }
            if (RuntimeTextureLoader.IsInstanced())
            {
                if (RuntimeTextureLoader.Instance.toRequest != 0)
                    tPerc = (float)RuntimeTextureLoader.Instance.requested / (float)RuntimeTextureLoader.Instance.toRequest;
            }

            if (tPerc == 1f && mPerc == 1f)
            {
                return true;
            }
            else return false;
        }

        

        protected override void ShowProject()
        {
            this.ApplyLogo();

            // Create the main area
            GUILayout.BeginArea(new Rect(0, 60, this.position.width, this.position.height - 2), this.backgroundStyle);

            GUILayout.Space(5);

            GUILayout.BeginVertical();

            

            if (this.isCurrentlyImporting)
            {
                var rectBar = EditorGUILayout.BeginHorizontal();

                float perc = (float)RuntimeMeshLoader.Instance.requested / (float)RuntimeMeshLoader.Instance.toRequest;

                var progressField = new Rect(12, rectBar.yMax, rectBar.width-12, 15);

                EditorGUI.ProgressBar(progressField, perc, "Importing... " + RuntimeMeshLoader.Instance.requested + "/" + RuntimeMeshLoader.Instance.toRequest);
                this.Repaint();
                GUILayout.Space(5);

                EditorGUILayout.EndHorizontal();
            }
            else
            {
                //this.DrawSyncOptions();

                GUILayout.Space(5);

               // this.DrawOptimizationTools();

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Click on the project you need to import into the scene", EditorStyles.boldLabel);

                if (GUILayout.Button(this.syncTexture, GUILayout.Width(32), GUILayout.Height(32)))
                    this.AssignProjects();

                GUILayout.EndHorizontal();

                GUILayout.EndVertical();

                GUILayout.BeginVertical();

                GUILayout.Space(5);

                this.scrollviewProject = GUILayout.BeginScrollView(this.scrollviewProject, false, false, GUI.skin.horizontalScrollbar, GUI.skin.verticalScrollbar, GUILayout.Width(this.position.width), GUILayout.Height(this.position.height - 150));

                this.DrawProjectField();

                GUILayout.EndScrollView();

            }

            GUILayout.EndVertical();
            GUILayout.EndArea();

        }
        private bool isOptionsOpened = false;
        private void DrawSyncOptions()
        {
            isOptionsOpened = EditorGUILayout.Foldout(isOptionsOpened, "Sync Options", false);
            if (!isOptionsOpened) return;
            GUILayout.BeginVertical(GUI.skin.box);
            ATSettings.LoadSettings();
            ATSettings.maxVerticesForUV2 = EditorGUILayout.IntField(new GUIContent("Max vertices count to calculate UV2", "UV2 Channel calculation is slow, lower this value to import faster"), ATSettings.maxVerticesForUV2);
            ATSettings.saveMesh = EditorGUILayout.Toggle(new GUIContent("Save meshes", "Save mesh into the disk during the import"), ATSettings.saveMesh);
            ATSettings.setNormalToZero = EditorGUILayout.Toggle(new GUIContent("Set normal to 0 degree", "Set normal to zero degree during the import, this can avoid some artifacts"), ATSettings.setNormalToZero);

            ATSettings.SaveSettings();
            GUILayout.EndVertical();

        }
        private bool isToolsOpened = false;
        private void DrawOptimizationTools()
        {
            isToolsOpened = EditorGUILayout.Foldout(isToolsOpened, "Optimization Tools", false);
            if (!isToolsOpened) return;
            GUILayout.BeginVertical(GUI.skin.box);

            if (GUILayout.Button("Optimize Meshes"))
            {
                OptimizeImportedMeshesWizard.Init();
            }
            if (GUILayout.Button("Optimize Scene File size"))
            {
                UnityEngine.Debug.Log("Optimize File Size");
            }
            GUILayout.EndVertical();
        }
        private void DrawProjectField()
        {
            var labelSkin = new GUIStyle(GUI.skin.label);

            labelSkin.alignment = TextAnchor.UpperLeft;
            labelSkin.fontStyle = FontStyle.Normal;
            labelSkin.clipping = TextClipping.Overflow;
            labelSkin.fontSize = 11;

            var toggleSkin = new GUIStyle(GUI.skin.toggle);

            toggleSkin.alignment = TextAnchor.UpperLeft;

            foreach (var p in this.avrsProjects)
            {
                if (p == null)
                    continue;

                GUILayout.BeginVertical(GUI.skin.box);

                p.isOpenedInUI = EditorGUILayout.Foldout(p.isOpenedInUI, p.projectName, true);

                if (p.isOpenedInUI)
                {
                    if (p.views != null)
                    {
                        foreach (var avrs in p.views)
                        {
                            if (avrs == null)
                                continue;

                            var horizontal = EditorGUILayout.BeginHorizontal(GUI.skin.box);

                            GUILayout.Label(avrs.thumbnail, GUILayout.Width(32), GUILayout.Height(32));

                            var rectBar = EditorGUILayout.BeginVertical();

                            var syncChanged = avrs.autoSync;

                            avrs.autoSync = GUILayout.Toggle(avrs.autoSync, "Auto sync");

                            if (syncChanged != avrs.autoSync)
                            {
                                avrs.SetSyncTo(avrs.autoSync, this.ProjectChanged);
                            }

                            GUILayout.Label(avrs.name, labelSkin);

                            string projectPathToSee = avrs.projectPath;

                            if (avrs.projectPath.Length > 100) // in this way we avoid to have a label too long
                                projectPathToSee = "..." + avrs.projectPath.Substring(100);
                            else if (avrs.projectPath.Length > 50)
                                projectPathToSee = "..." + avrs.projectPath.Substring(50);

                            GUILayout.Label(projectPathToSee, labelSkin);
                            GUILayout.Label(avrs.lastEditTime.ToString(), labelSkin);

                            GUILayout.Space(20);

                            if (avrs.isImporting)
                            {
                                
                                float perc = (float)RuntimeMeshLoader.Instance.requested / (float)RuntimeMeshLoader.Instance.toRequest;

                                var progressField = new Rect(12, rectBar.yMax - 15, rectBar.width + 36, 15);

                                EditorGUI.ProgressBar(progressField, perc, "Importing... " + RuntimeMeshLoader.Instance.requested + "/" + RuntimeMeshLoader.Instance.toRequest);

                                this.Repaint();
                                
                            }
                            
                            EditorGUILayout.EndVertical();
                            if (!this.isCurrentlyImporting)
                            {
                                if (!avrs.isImporting)
                                {
                                    var import = GUILayout.Button(this.syncTexture, GUILayout.Height(32), GUILayout.Width(32));

                                    if (import)
                                    {
                                        avrs.isImporting = true;
                                        Import(avrs);
                                    }
                                }
                            }
                            //Latest Check
                            if(avrs.isImporting && !this.isCurrentlyImporting)
                            {
                                avrs.isImporting = false;
                                this.CheckNormals(avrs);
                            }

                            EditorGUILayout.EndHorizontal();
                        }
                    }
                }

                GUILayout.EndVertical();
            }
        }

       
        public void CheckNormals(Avrs project)
        {
            foreach (var item in project.root.GetComponentsInChildren<VMesh>())
            {
                if (item != null)
                {
                    if (item.GetComponent<RecalculateNormalsComponent>() == null)
                    {
                        var normalCompoenent = item.gameObject.AddComponent<RecalculateNormalsComponent>();

                        if (ATSettings.setNormalToZero)
                            normalCompoenent.RecalculateNormals(0);
                    }
                }
            }
        }

    }

}
