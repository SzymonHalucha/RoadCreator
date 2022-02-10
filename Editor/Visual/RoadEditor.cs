using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using SH.RoadCreator.Algorithm;

namespace SH.RoadCreator.Viusal
{
    [CustomEditor(typeof(Road))]
    public class RoadEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Open Road Editor")) RoadEditorWindow.OpenWindow((Road)target);
            EditorGUILayout.Space();
            DrawDefaultInspector();
        }

        [OnOpenAsset()]
        public static bool OpenAsset(int id, int line)
        {
            Road road = EditorUtility.InstanceIDToObject(id) as Road;
            if (road == null) return false;

            RoadEditorWindow.OpenWindow(road);
            return true;
        }
    }
}