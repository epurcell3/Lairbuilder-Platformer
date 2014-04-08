using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(Tilemap))]
public class TilemapEditor : Editor {

    public SerializedProperty input_method_Prop,
                              size_x_Prop,
                              size_y_Prop,
                              tile_resolution_Prop,
                              tile_size_Prop,
                              solid_threshold_Prop,
                              tileset_Prop,
                              tile_ids_Prop,
                              tiled_filepath_Prop;

    void OnEnable()
    {
        input_method_Prop = serializedObject.FindProperty("input_method");
        size_x_Prop = serializedObject.FindProperty("size_x");
        size_y_Prop = serializedObject.FindProperty("size_y");
        tile_resolution_Prop = serializedObject.FindProperty("tile_resolution");
        tile_size_Prop = serializedObject.FindProperty("tile_size");
        solid_threshold_Prop = serializedObject.FindProperty("solid_threshold");
        tileset_Prop = serializedObject.FindProperty("tileset");
        tile_ids_Prop = serializedObject.FindProperty("tile_ids");
        tiled_filepath_Prop = serializedObject.FindProperty("tiled_filepath");
    }


    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(input_method_Prop);

        Tilemap.Input inp = (Tilemap.Input)input_method_Prop.enumValueIndex;

        switch (inp)
        {
            case Tilemap.Input.MANUAL:
                EditorGUILayout.PropertyField(size_x_Prop);
                EditorGUILayout.PropertyField(size_y_Prop);
                EditorGUILayout.PropertyField(tile_resolution_Prop);
                EditorGUILayout.PropertyField(tile_size_Prop);
                EditorGUILayout.PropertyField(solid_threshold_Prop);
                EditorGUILayout.PropertyField(tileset_Prop);
                EditorGUILayout.PropertyField(tile_ids_Prop);
                break;
            case Tilemap.Input.FILE:
                EditorGUILayout.PropertyField(tiled_filepath_Prop);
                EditorGUILayout.PropertyField(tile_size_Prop);
                EditorGUILayout.PropertyField(tileset_Prop);
                break;
        }

        Tilemap map = (Tilemap)target;
        if (GUILayout.Button("Generate Map"))
        {
            map.Generate();
        }

        serializedObject.ApplyModifiedProperties();
    }
}
