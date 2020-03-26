using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using ambiens.avrs.model;
using UnityEditor;
using ambiens.avrs.controller;

namespace Archtoolkit.ATImport
{
    public class TempScene : ScriptableObject
    {
        public MScene mScene;
        public string path;
        public CScene currentSceneController;

        public TempScene Initialize(MScene mScene, string path, CScene controller)
        {
            var sO = Create(path);

            sO.mScene = mScene;
            sO.path = path;
            sO.currentSceneController = controller;

            return sO;
        }

        public static TempScene Create(string avrsPath)
        {
            var tempScene = Get(avrsPath);

            var assetName = System.IO.Path.GetFileNameWithoutExtension(avrsPath);
            
            if (tempScene == null)
            {
                if (!AssetDatabase.IsValidFolder("Assets/AvrsScenes"))
                    AssetDatabase.CreateFolder("Assets","AvrsScenes");

                string relativePath = "Assets/AvrsScenes/" + assetName + ".asset";

                var sO = ScriptableObject.CreateInstance<TempScene>();

                AssetDatabase.CreateAsset(sO, relativePath);

                return sO;
            }
            else
            {
                return tempScene;
            }
        }

        public static TempScene Get(string avrsPath)
        {
            var tempSceneAsset = AssetDatabase.FindAssets("t:TempScene");

            string[] paths = new string[tempSceneAsset.Length];

            for (int i = 0; i < tempSceneAsset.Length; i++)
            {
                paths[i] = AssetDatabase.GUIDToAssetPath(tempSceneAsset[i]);
            }

            var assetName = System.IO.Path.GetFileNameWithoutExtension(avrsPath);

            string correctAsset = paths.ToList().Find(p => p.Contains(assetName));

            if (!string.IsNullOrEmpty(correctAsset))
            {
                return AssetDatabase.LoadAssetAtPath<TempScene>(correctAsset);
            }

            return null;

        }
    }
}