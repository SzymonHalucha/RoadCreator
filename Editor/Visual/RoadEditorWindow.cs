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
    /// This class is responsible for displaying and managing the road creator window.
    /// </summary>
    public class RoadEditorWindow : EditorWindow
    {
        private RoadList _list;
        private SelectedRoad _selected;
        private SelectedRoadScene _selectedScene;
        private RoadDisplay _display;

        /// <summary>
        /// Open a list of available road projects.
        /// </summary>
        [MenuItem("Tools/Road Creator/Road Editor")]
        public static void OpenWindow()
        {
            RoadEditorWindow window = GetWindow<RoadEditorWindow>();
            window.titleContent = new GUIContent("Road Editor");
            window.minSize = new Vector2(384f, 384f);
            window.UpdateGUI();
        }

        /// <summary>
        /// Open the window for the selected road project.
        /// </summary>
        /// <param name="road">Selected road project.</param>
        public static void OpenWindow(Road road)
        {
            RoadEditorWindow window = GetWindow<RoadEditorWindow>();
            window.titleContent = new GUIContent("Road Editor");
            window.minSize = new Vector2(384f, 384f);
            window.UpdateGUI(road);
        }

        /// <summary>
        /// This is Unity method, it is called only once on startup.
        /// </summary>
        private void CreateGUI()
        {
            _display = new RoadDisplay();
            _selectedScene = new SelectedRoadScene();
            _list = new RoadList(rootVisualElement, this);
            _selected = new SelectedRoad(rootVisualElement, this);
        }

        /// <summary>
        /// This is Unity method, it is called before this object is deleted.
        /// </summary>
        private void OnDestroy()
        {
            _selectedScene.DisableSceneGUI();
            _display.DestroyRoadObject();
        }

        /// <summary>
        /// Update the list of available road projects.
        /// </summary>
        public void UpdateGUI()
        {
            _selectedScene.DisableSceneGUI();
            _display.DestroyRoadObject();
            ResetWindow();
            AddUxml(rootVisualElement, "Packages/com.sh.roadcreator/Editor/Visual/RoadList.uxml");
            _list.UpdateGUI();
        }

        /// <summary>
        /// Update the window view to the selected road project.
        /// </summary>
        /// <param name="road">Selected road project.</param>
        public void UpdateGUI(Road road)
        {
            ResetWindow();
            AddUxml(rootVisualElement, "Packages/com.sh.roadcreator/Editor/Visual/SelectedRoad.uxml");
            _selected.UpdateGUI(road, _display);
            _selectedScene.UpdateSceneGUI(_display);
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