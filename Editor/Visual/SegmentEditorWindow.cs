using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using SH.RoadCreator.Algorithm;

namespace SH.RoadCreator.Viusal
{
    /// <summary>
    /// This class is responsible for displaying and managing the segment creator window.
    /// </summary>
    public class SegmentEditorWindow : EditorWindow
    {
        private SegmentList _list;
        private SelectedSegment _selected;
        private SegmentDisplay _display;

        /// <summary>
        /// Open a list of available segment projects.
        /// </summary>
        [MenuItem("Tools/Road Creator/Segment Editor")]
        public static void OpenWindow()
        {
            SegmentEditorWindow window = GetWindow<SegmentEditorWindow>();
            window.titleContent = new GUIContent("Segment Editor");
            window.minSize = new Vector2(384f, 384f);
            window.UpdateGUI();
        }

        /// <summary>
        /// Open the window for the selected segment project.
        /// </summary>
        /// <param name="segment">Selected segment project.</param>
        public static void OpenWindow(Segment segment)
        {
            SegmentEditorWindow window = GetWindow<SegmentEditorWindow>();
            window.titleContent = new GUIContent("Segment Editor");
            window.minSize = new Vector2(384f, 384f);
            window.UpdateGUI(segment);
        }

        /// <summary>
        /// This is Unity method, it is called only once on startup.
        /// </summary>
        private void CreateGUI()
        {
            _list = new SegmentList(rootVisualElement, this);
            _selected = new SelectedSegment(rootVisualElement, this);
            _display = new SegmentDisplay();
        }

        /// <summary>
        /// This is Unity method, it is called before this object is deleted.
        /// </summary>
        private void OnDestroy()
        {
            _display.DestroySegmentObject();
        }

        /// <summary>
        /// Update the list of available segment projects.
        /// </summary>
        public void UpdateGUI()
        {
            _display.DestroySegmentObject();
            ResetWindow();
            AddUxml(rootVisualElement, "Packages/com.sh.roadcreator/Editor/Visual/SegmentList.uxml");
            _list.UpdateGUI();
        }

        /// <summary>
        /// Update the window view to the selected segment project.
        /// </summary>
        /// <param name="road">Selected segment project.</param>
        public void UpdateGUI(Segment segment)
        {
            ResetWindow();
            AddUxml(rootVisualElement, "Packages/com.sh.roadcreator/Editor/Visual/SelectedSegment.uxml");
            _selected.UpdateGUI(segment, _display);
        }

        /// <summary>
        /// Reset UXML structure.
        /// </summary>
        private void ResetWindow()
        {
            rootVisualElement.Clear();
        }

        /// <summary>
        /// Add a UXML structure from the specified path to the selected parent.
        /// </summary>
        /// <param name="parent">Selected parent object.</param>
        /// <param name="path">Project relative path (E.g. Assets/Example1/Example2).</param>
        private void AddUxml(VisualElement parent, string path)
        {
            VisualTreeAsset visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path);
            parent.Add(visualTreeAsset.Instantiate());
        }
    }
}