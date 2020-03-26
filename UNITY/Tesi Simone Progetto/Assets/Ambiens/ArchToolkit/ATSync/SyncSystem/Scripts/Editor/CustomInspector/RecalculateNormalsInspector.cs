using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(RecalculateNormalsComponent))]
[CanEditMultipleObjects]
public class RecalculateNormalsInspector : Editor
{
    private RecalculateNormalsComponent component;

    private void OnEnable()
    {
        if (component == null)
        {
            component = target as RecalculateNormalsComponent;
        }
        
    }

    public override void OnInspectorGUI()
    {

        GUILayout.BeginVertical();
        
        this.component.angle = EditorGUILayout.Slider("Angle: ",this.component.angle, 0, 360);
        
        GUILayout.Space(10);

        var color = GUI.backgroundColor;

        GUI.backgroundColor = Color.cyan;

        if (GUILayout.Button("Refresh normals"))
        {
            this.component.RecalculateNormals();
        }

        GUI.backgroundColor = color;

        GUILayout.EndVertical();
    }
}
