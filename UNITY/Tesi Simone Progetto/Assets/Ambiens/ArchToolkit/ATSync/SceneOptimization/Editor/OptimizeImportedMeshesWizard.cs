using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEditor;
using UnityEngine;
namespace Archtoolkit.ATImport
{
    [System.Serializable]
    public class OptimizeImportedMeshesWizard : EditorWindow
    {
        public static OptimizeImportedMeshesWizard mWindow;
        public static void Init()
        {
            var window = (OptimizeImportedMeshesWizard)EditorWindow.GetWindow(typeof(OptimizeImportedMeshesWizard), false, "Optimize Meshes");

            window.maxSize = new Vector2(600, 600);
            window.minSize = new Vector2(600, 600);

            mWindow = window;

            window.Show();
        }
        private List<MeshFilter> filters;
        private List<Mesh> meshes;
        private List<Mesh> NotSavedMeshes;

        private void UpdateData()
        {
            this.filters = new List<MeshFilter>(GameObject.FindObjectsOfType<MeshFilter>());
            this.meshes = new List<Mesh>();
            this.NotSavedMeshes = new List<Mesh>();
            foreach (var f in this.filters)
            {
                if (!meshes.Contains(f.sharedMesh))
                {
                    meshes.Add(f.sharedMesh);
                }
            }
            foreach (var m in this.meshes)
            {
                var p = AssetDatabase.GetAssetPath(m);
                if (String.IsNullOrEmpty(p))
                {
                    NotSavedMeshes.Add(m);
                }
            }
            MeshDataList = new Dictionary<int, meshData>();
        }
        private float angle = 0;
        private float MaxRatioForOptimization = 25;
        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            if (!this.isOptimizing)
            {
                EditorApplication.update -= this.OptimizationUpdate;
                if (GUILayout.Button("Refresh Data"))
                {
                    this.UpdateData();
                }

                if (this.filters != null && this.filters.Count > 0)
                {
                    GUILayout.Label("In the scene there are \n" +
                    "- " + this.filters.Count + " MeshFilters\n" +
                    "- " + this.meshes.Count + " Shared Meshes\n" +
                    "- " + this.NotSavedMeshes.Count + " Meshes not saved on disk" +
                    "", EditorStyles.boldLabel);


                    if (GUILayout.Button("Find duplicate meshes"))
                    {
                        StartOptimization();
                    }
                    GUILayout.Space(10);

                    angle = EditorGUILayout.Slider("Angle: ", angle, 0, 360);
                    var color = GUI.backgroundColor;
                    GUI.backgroundColor = Color.cyan;

                    if (GUILayout.Button("Refresh normals"))
                    {
                        this.StartRecalculateNormals();
                    }

                    GUI.backgroundColor = color;
                    GUILayout.Space(10);

                    if (GUILayout.Button("Save unsaved meshes as files"))
                    {
                        StartSavingMeshes();
                    }
                    if (GUILayout.Button("Remove BimData to reduce scene file size"))
                    {
                        var data=new List<BimData>(GameObject.FindObjectsOfType<BimData>());
                        while (data.Count > 0)
                        {
                            var d = data[0];
                            data.RemoveAt(0);
                            GameObject.DestroyImmediate(d);
                        }
                    }
                    GUILayout.Space(10);
                }
            }
            else
            {
                var rect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);

                var perc = (float)CurrentAnalizedMeshFilter / (float)this.filters.Count;

                EditorGUI.ProgressBar(rect, perc, "Working... " + CurrentAnalizedMeshFilter + "/" + this.filters.Count);

                var working = LaunchedThreads.FindAll(t => t.state== MeshComparerWorker.threadState.started);
                EditorGUILayout.LabelField("Total Jobs " + LaunchedThreads.Count);
                foreach (var t in working)
                {
                    EditorGUILayout.LabelField("Thread working " + t.auxMeshFilter.name + " " + t.currentMeshFilter.name);
                }
            }
            EditorGUILayout.EndVertical();
        }
        private int CurrentAnalizedMeshFilter;
        private bool isOptimizing = false;
        private string meshFolder;
        private void StartOptimization()
        {
            CurrentAnalizedMeshFilter = 0;
            isOptimizing = true;
            
            EditorApplication.update += this.OptimizationUpdate;
        }
        private void StartSavingMeshes()
        {
            CurrentAnalizedMeshFilter = 0;
            isOptimizing = true;
            meshFolder = Application.dataPath + "/ATSyncMeshes/";

            if (!Directory.Exists(meshFolder))
                Directory.CreateDirectory(meshFolder);
            meshFolder = "Assets/ATSyncMeshes/";

            EditorApplication.update += this.SavingUpdate;
        }
        private void StartRecalculateNormals()
        {
            CurrentAnalizedMeshFilter = 0;
            isOptimizing = true;
            EditorApplication.update += this.NormalUpdate;
        }
        private List<MeshComparerWorker> LaunchedThreads=new List<MeshComparerWorker>();
        private int CurrentComparedMeshFilter=0;
        public List<MeshFilter> sharedMeshFilterData=new List<MeshFilter>();

        public struct meshData
        {
            
            //public List<Vector3> orderedVectors;
            public int[] triangles;
            public List<Vector3> vectors;
            public Vector3 center;
        }
        public static Dictionary<int,meshData> MeshDataList = new Dictionary<int, meshData>();

        public static meshData GetData(Mesh m)
        {

            /*if (MeshDataList.ContainsKey(m.GetInstanceID()))
            {
                return MeshDataList[m.GetInstanceID()];
            }
            else
            {*/
                var d = new meshData();
                

                //d.orderedVectors = new List<Vector3>();
                var v = m.vertices;
                var t = m.triangles;
                d.vectors = new List<Vector3>(v);
                d.center = m.bounds.center;
                
                
                d.triangles = t;
                //MeshDataList.Add(m.GetInstanceID(), d);

                return d;
           // }
        }
        int currentFilterManaging = 0;
        void OptimizationUpdate()
        {
            
            if (this.filters.Count>0)
            {
                var currentMeshFilter = this.filters[0];
                MeshFilter auxMeshFilter;

                float rotationStepSize = 90;
                int rotationStep = (int)360f / (int)rotationStepSize;
                if (LaunchedThreads.Count > 0)
                {

                    int workingNumber = LaunchedThreads.FindAll(t => t.state == MeshComparerWorker.threadState.started).Count;
                    int threadIndex = 0;
                    while (threadIndex < LaunchedThreads.Count)
                    {
                        // -1 =>the thread has not started yet
                        // -2 => the thread is working
                        // 0, 1 => thread result
                        if (LaunchedThreads[threadIndex].state == MeshComparerWorker.threadState.ready && workingNumber<5)
                        {
                            //LaunchedThreads[threadIndex].threadHandle.Join();

                            var t = new Thread(LaunchedThreads[threadIndex].DoWork);
                            t.Start();
                            LaunchedThreads[threadIndex].threadHandle = t;
                            workingNumber++;
                            this.Repaint();

                        }
                        else if (LaunchedThreads[threadIndex].state == MeshComparerWorker.threadState.finished)//Thread finished work
                        {
                            var todestroy = LaunchedThreads[threadIndex];
                            if (todestroy.GetResult() == 1)
                            {

                                if (this.filters.Contains(todestroy.auxMeshFilter))
                                {
                                    var sameMeshes = this.filters.FindAll(f => f.sharedMesh == todestroy.auxMeshFilter.sharedMesh );
                                    foreach (var f in sameMeshes)
                                    {
                                        f.sharedMesh = todestroy.currentMeshFilter.sharedMesh;
                                        f.gameObject.transform.localRotation = Quaternion.Euler(new Vector3(0, todestroy.RotationToApply, 0));

                                        var coll = f.gameObject.GetComponent<MeshCollider>();
                                        if (coll != null) coll.sharedMesh = f.sharedMesh;
                                        this.filters.Remove(f);
                                    }
                                }
                            }
                            
                            LaunchedThreads.Remove(todestroy);
                            if(todestroy.threadHandle!=null)
                                todestroy.threadHandle.Join();
                            
                            workingNumber--;

                        }
                        
                        threadIndex++;  
                    }

                    /*if (LaunchedThreads.Count == 0)
                    {
                    }*/
                }
                else
                {
                    //currentFilterManaging = Math.Min(this.filters.Count, currentFilterManaging + 200);

                    var sameMeshes = this.filters.FindAll(f => f.sharedMesh == currentMeshFilter.sharedMesh && f != currentMeshFilter);
                    foreach (var f in sameMeshes) this.filters.Remove(f);
                   // Debug.Log(currentMeshFilter.gameObject.name + " Removed " + sameMeshes.Count+" Similar", currentMeshFilter.gameObject);

                    sameMeshes.Clear();

                    var filtered = this.filters.FindAll(f => f.sharedMesh != currentMeshFilter.sharedMesh 
                        && f.sharedMesh.triangles.Length == currentMeshFilter.sharedMesh.triangles.Length
                        && Approximately(f.sharedMesh.bounds.size, currentMeshFilter.sharedMesh.bounds.size) );

                    Debug.Log(currentMeshFilter.gameObject.name + " " + filtered.Count,currentMeshFilter.gameObject);

                    for (int i = 0; i < filtered.Count; i++)
                    {
                        auxMeshFilter = filtered[i];
                        Debug.Log("--> "+ auxMeshFilter.gameObject.name, auxMeshFilter.gameObject);

                        if (String.IsNullOrEmpty(auxMeshFilter.sharedMesh.name)) 
                            auxMeshFilter.sharedMesh.name = auxMeshFilter.gameObject.name;

                        var comparer = new MeshComparerWorker(
                            auxMeshFilter,
                            currentMeshFilter,
                            rotationStepSize,
                            rotationStep,
                            MaxRatioForOptimization);
                        //comparer.DoWork();

                        LaunchedThreads.Add(comparer);  
                    }
                    //CurrentAnalizedMeshFilter++;
                    this.filters.Remove(currentMeshFilter);
                }

            }
            else
            {
                isOptimizing = false;
                EditorApplication.update -= this.OptimizationUpdate;
                this.UpdateData();
            }
        }

        public class MeshComparerWorker
        {
            public MeshComparerWorker(MeshFilter _auxMeshFilter, MeshFilter _currentMeshFilter,  float _rotationStepSize, int _rotationStep, float _MaxRatioForOptimization)
            {
                
                this.auxMeshFilter = _auxMeshFilter;
                this.currentMeshFilter = _currentMeshFilter;
                
                this.rotationStepSize = _rotationStepSize;
                this.rotationStep = _rotationStep;
                this.MaxRatioForOptimization = _MaxRatioForOptimization;
                this.state = threadState.notStarted;

                this.auxData = OptimizeImportedMeshesWizard.GetData(auxMeshFilter.sharedMesh);
                this.currentData = OptimizeImportedMeshesWizard.GetData(currentMeshFilter.sharedMesh);
                
                this.state = threadState.ready;
            }

            public float percentage;

            public Thread threadHandle;

            public meshData auxData, currentData;

            public MeshFilter auxMeshFilter, currentMeshFilter;
            //Mesh auxMesh, currentMesh;
            string auxMeshID, currentMeshID;
            float rotationStepSize, MaxRatioForOptimization;
            int rotationStep;
            Vector3 auxV1, auxV2;
            //Vector3[] auxVertices, currentVertices;
            public enum threadState
            {
                notStarted,
                ready,
                started,
                finished
            }
            public threadState state = threadState.notStarted;
            private int result = -1;
            public float RotationToApply;

           
            public void DoWork( )
            {
                this.state = threadState.started;
               // Debug.Log("JOB Started");


                bool found = false;
                this.RotationToApply = 0;
                int angleStep = 0;
                int NotEqual = 0;
                float ratio = 0;
                var quat = Quaternion.Euler(0, rotationStepSize, 0);

                while (!found && angleStep < rotationStep)
                {
                    NotEqual = 0;
                    this.RotationToApply = rotationStepSize * angleStep;
                    if (angleStep > 0)
                    {
                        for (int i = 0; i < this.auxData.vectors.Count; i++)
                        {
                            this.auxData.vectors[i] = quat * this.auxData.vectors[i];
                        }
                    }
                    
                    //Debug.Log("Angle step "+angleStep+" "+ this.RotationToApply);
                    for (int j = 0; j < this.currentData.vectors.Count; j++)
                    {
                        auxV1 = this.currentData.vectors[j]; //- this.currentData.center;
                        var founded = this.auxData.vectors.FindAll(a => 
                            OptimizeImportedMeshesWizard.Approximately(a, auxV1));

                        if (founded.Count > 0)
                        {
                            this.auxData.vectors.Remove(founded[0]);
                        }
                        else
                        {
                            NotEqual++;
                        }
                        /*if (j < this.auxData.GetOrderedVertex().Count)
                        {
                            
                            auxV2 = Quaternion.Euler(0, rotationStepSize * angleStep, 0) * this.auxData.GetOrderedVertex()[j]-auxCenter;
                            if (!Approximately(auxV1, auxV2))
                            {
                                NotEqual++;
                                Debug.Log("not equal " + NotEqual);
                            }
                                
                        }
                        else NotEqual++;
                        */
                        ratio = ((float)NotEqual / (float)this.currentData.vectors.Count) * 100f;
                        if (ratio > MaxRatioForOptimization) break;
                    }

                    if (ratio < this.MaxRatioForOptimization)
                    {
                        //Debug.Log("OK-> " + ratio+" "+ this.RotationToApply);

                        found = true;
                        break;
                    }
                    else
                    {
                        angleStep++;
                    }
                    //else Debug.Log("ratio "+ratio+ " "+NotEqual+"/"+this.currentData.vectors.Count);
                }
                if (found)
                {
                    //Debug.Log("FOUND -> " + ratio);
                    this.state = threadState.finished;
                    result = 1;
                }
                else
                {
                    //Debug.Log("NOT FOUND -> " + ratio);

                    this.state = threadState.finished;
                    result = 0;
                }
                
            }

            public int GetResult()
            {
                return this.result;
            }
        }

        void NormalUpdate()
        {
            if (CurrentAnalizedMeshFilter < this.filters.Count)
            {
                var currentMeshFilter = this.filters[CurrentAnalizedMeshFilter];

                var normalComponent = currentMeshFilter.gameObject.GetComponent<RecalculateNormalsComponent>();
                if (normalComponent == null) normalComponent = currentMeshFilter.gameObject.AddComponent<RecalculateNormalsComponent>();
                normalComponent.RecalculateNormals(angle);
                var filtered = this.filters.FindAll((MeshFilter f) => f.sharedMesh == currentMeshFilter.sharedMesh && f != currentMeshFilter);

                while (filtered.Count > 0)
                {
                    this.filters.Remove(filtered[0]);
                    filtered.RemoveAt(0);
                }

                CurrentAnalizedMeshFilter++;
            }
            else
            {
                isOptimizing = false;
                EditorApplication.update -= this.NormalUpdate;
                this.UpdateData();
            }
        }

        void SavingUpdate()
        {
            if (CurrentAnalizedMeshFilter < this.filters.Count)
            {
                var currentMeshFilter = this.filters[CurrentAnalizedMeshFilter];
                if (String.IsNullOrEmpty(AssetDatabase.GetAssetPath(currentMeshFilter.sharedMesh)))
                {
                   
                    AssetDatabase.CreateAsset(currentMeshFilter.sharedMesh, meshFolder + "mesh_"+ambiens.utils.common.Utils.md5(currentMeshFilter.sharedMesh.name) + ".asset");

                    var filtered = this.filters.FindAll((MeshFilter f) => f.sharedMesh == currentMeshFilter.sharedMesh && f != currentMeshFilter);

                    while (filtered.Count > 0)
                    {
                        filtered[0].sharedMesh = currentMeshFilter.sharedMesh;
                        this.filters.Remove(filtered[0]);
                        filtered.RemoveAt(0);
                    }

                }

                CurrentAnalizedMeshFilter++;
            }
            else
            {
                isOptimizing = false;
                EditorApplication.update -= this.SavingUpdate;
                AssetDatabase.Refresh();
                this.UpdateData();
            }
        }
        public static bool Approximately(Vector3 me, Vector3 other, float allowedDifference = 0.005f)
        {
            //Debug.Log("Compare " + me.x+" "+other.x + " " + me.y + " " + other.y + " " + me.z + " " + other.z);

            var dx = me.x - other.x;
            //Debug.Log("dx " + dx);

            if (Mathf.Abs(dx) > allowedDifference)
                return false;

            var dy = me.y - other.y;
            //Debug.Log("dy " + dy);

            if (Mathf.Abs(dy) > allowedDifference)
                return false;

            var dz = me.z - other.z;
            //Debug.Log("dz " + dz);

            return Mathf.Abs(dz) < allowedDifference;
        }
    }
    
}