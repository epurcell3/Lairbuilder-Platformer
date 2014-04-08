using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(Tilemap))]
public class TilemapEditor : Editor {

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Tilemap map = (Tilemap)target;
        if (GUILayout.Button("Generate Map"))
        {
            map.Generate();
        }
    }
}
