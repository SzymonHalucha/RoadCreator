using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using SH.RoadCreator.Algorithm;

namespace SH.RoadCreator.Viusal
{
    [CustomEditor(typeof(Segment))]
    public class SegmentEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Open Segment Editor")) SegmentEditorWindow.OpenWindow((Segment)target);
            EditorGUILayout.Space();
            DrawDefaultInspector();
        }

        [OnOpenAsset()]
        public static bool OpenAsset(int id, int line)
        {
            Segment segment = EditorUtility.InstanceIDToObject(id) as Segment;
            if (segment == null) return false;

            SegmentEditorWindow.OpenWindow(segment);
            return true;
        }
    }
}