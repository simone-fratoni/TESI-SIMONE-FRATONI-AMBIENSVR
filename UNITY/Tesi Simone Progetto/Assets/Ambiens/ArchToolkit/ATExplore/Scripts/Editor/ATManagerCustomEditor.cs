using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace ArchToolkit
{
    [CustomEditor(typeof(ArchToolkitManager))]
    public class ATManagerCustomEditor : UnityEditor.Editor
    {
        BoxBoundsHandle handle=new BoxBoundsHandle();
        public override void OnInspectorGUI() 
        {
            base.DrawDefaultInspector();

            if (GUILayout.Button(new GUIContent("Refresh scene Bounds", "Refresh bounds size")))
            {
                CalculateSceneBounds();
            }
        }

        private void OnSceneGUI()
        {
            ArchToolkitManager man = (ArchToolkitManager)target;

            handle.center = man.sceneBounds.center;
            handle.size = man.sceneBounds.size;

            // draw the handle
            EditorGUI.BeginChangeCheck();
            handle.DrawHandle();
            if (EditorGUI.EndChangeCheck())
            {
                // record the target object before setting new values so changes can be undone/redone
                Undo.RecordObject(man, "Change Bounds");

                // copy the handle's updated data back to the target object
                Bounds newBounds = new Bounds();
                newBounds.center = handle.center;
                newBounds.size = handle.size;
                man.sceneBounds = newBounds;
            }

        }

        void CalculateSceneBounds()
        {
            var rootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();

            Bounds bounds = new Bounds();

            foreach(var go in rootObjects)
            {
                AddGOBounds(go, ref bounds);
            }

            ArchToolkitManager man = (ArchToolkitManager)target;
            man.sceneBounds = bounds;
        }
        void AddGOBounds(GameObject go, ref Bounds bounds)
        {
            var rend = go.GetComponent<MeshRenderer>();
            if (rend != null)
            {
                if (bounds.size == Vector3.zero)
                {
                    bounds = rend.bounds;
                }
                else
                    bounds.Encapsulate(rend.bounds);
            }
            int childCount = go.transform.childCount;
            for(int i=0; i<childCount; i++)
            {

                AddGOBounds(go.transform.GetChild(i).gameObject, ref bounds);
            }
        }
    }
}

