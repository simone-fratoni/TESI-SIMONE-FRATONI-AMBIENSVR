using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BimData))]
[System.Serializable]
public class BimDataCustomInspector : Editor
{
    private BimData componentTarget;
    
    private void OnEnable()
    {
        if (this.componentTarget == null)
        {
            this.componentTarget = (BimData)target;
            EditorUtility.SetDirty(this.componentTarget);
        }
    }

    public override void OnInspectorGUI()
    {
        if (this.componentTarget != null)
        {

            if(this.componentTarget.bimDatas.Count == 0)
            {

                for (int i = 0; i < this.componentTarget.keys.Count; i++)
                {
                    var key = this.componentTarget.keys[i];
                    var value = this.componentTarget.values[i];

                    this.componentTarget.bimDatas.Add(key,value);
                }
            }

            GUILayout.BeginVertical();
            
            GUILayout.Space(20);

            GUILayout.Label("Bim type: " + componentTarget.dataType.ToString());

            GUILayout.Space(20);

            GUILayout.BeginHorizontal();

            GUILayout.Label("Name ");

            GUILayout.Space(40);

            GUILayout.Label("Value ");

            GUILayout.EndHorizontal();

            foreach (var item in this.componentTarget.bimDatas)
            {
                GUILayout.BeginHorizontal();

                GUILayout.TextField(item.Key, GUILayout.Width(EditorGUIUtility.currentViewWidth / 2));

                GUILayout.TextField(item.Value, GUILayout.Width(EditorGUIUtility.currentViewWidth / 2 - 40));

                GUILayout.EndHorizontal();

            }

            // GUILayout.EndScrollView();

            GUILayout.EndVertical();

            if (GUI.changed)
                EditorUtility.SetDirty(this.componentTarget);
        }
    }
}
