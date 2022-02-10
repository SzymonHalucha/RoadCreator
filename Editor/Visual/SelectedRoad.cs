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
    /// This class is responsible for displaying the window of a specific road project.
    /// </summary>
    public class SelectedRoad
    {
        private VisualElement _root;
        private RoadEditorWindow _window;
        private Road _current;
        private RoadDisplay _display;

        private const string uss_windowTitle = "title";
        private const string uss_windowItem = "window-item";

        public SelectedRoad(VisualElement root, RoadEditorWindow window)
        {
            this._root = root;
            this._window = window;
        }

        /// <summary>
        /// Update the window view to a specific road project.
        /// </summary>
        /// <param name="road">Selected road project.</param>
        /// <param name="display">An object responsible for the logic of displaying the road mesh and texture in the editor scene.</param>
        public void UpdateGUI(Road road, RoadDisplay display)
        {
            _current = road;
            _display = display;
            _display.CreateRoadObject(road);
            VisualElement container = _root.Q<VisualElement>(null, "container");
            CreateWindowHeader(container, road.name);
            CreatePropertyFields(container);
            CreateWindowButtons(container);
        }

        /// <summary>
        /// Create a title header for the selected road project window.
        /// </summary>
        /// <param name="parent">Parent of the title header.</param>
        /// <param name="name">Text of the title.</param>
        private void CreateWindowHeader(VisualElement parent, string name)
        {
            Label title = new Label(name);
            title.AddToClassList(uss_windowTitle);
            parent.Add(title);
        }

        /// <summary>
        /// Create property fields for each road projects variables.
        /// </summary>
        /// <param name="parent">Parent of the structure.</param>
        private void CreatePropertyFields(VisualElement parent)
        {
            SerializedObject serializedRoad = new SerializedObject(_current);

            SerializedProperty segmentProperty = serializedRoad.FindProperty(nameof(_current.RoadSegment));
            PropertyField segmentField = CreatePropertyField(parent, segmentProperty);

            SerializedProperty qualityProperty = serializedRoad.FindProperty(nameof(_current.Quality));
            PropertyField qualityField = CreatePropertyField(parent, qualityProperty);

            SerializedProperty relocateProperty = serializedRoad.FindProperty(nameof(_current.AutoRelocateTangents));
            PropertyField relocateField = CreatePropertyField(parent, relocateProperty);

            SerializedProperty gizmosSizeProperty = serializedRoad.FindProperty(nameof(_current.GizmosSize));
            PropertyField gizmosSizeField = CreatePropertyField(parent, gizmosSizeProperty);

            SerializedProperty gizmosColorProperty = serializedRoad.FindProperty(nameof(_current.GizmosColor));
            PropertyField gizmosColorField = CreatePropertyField(parent, gizmosColorProperty);
        }

        /// <summary>
        /// Create a single property field.
        /// </summary>
        /// <param name="parent">Parent of the structure.</param>
        /// <param name="property">Binding property.</param>
        /// <returns>Returns the created property field.</returns>
        private PropertyField CreatePropertyField(VisualElement parent, SerializedProperty property)
        {
            PropertyField field = new PropertyField();
            field.AddToClassList(uss_windowItem);
            field.BindProperty(property);
            parent.Add(field);

            return field;
        }

        /// <summary>
        /// Create all required buttons for the project.
        /// </summary>
        /// <param name="parent">Parent of the structure.</param>
        private void CreateWindowButtons(VisualElement parent)
        {
            SerializedObject serializedRoad = new SerializedObject(_current);

            Button list = CreateButton(parent, uss_windowItem, "View Road List");
            list.clicked += () => { _window.UpdateGUI(); };

            Button clearPoints = CreateButton(parent, uss_windowItem, "Clear Points");
            clearPoints.clicked += () => { _display.Current.Clear(); };

            Button generate = CreateButton(parent, uss_windowItem, "Generate Mesh");
            generate.clicked += () => { _display.GenerateRoadMesh(); };

            Button clearMesh = CreateButton(parent, uss_windowItem, "Clear Mesh");
            clearMesh.clicked += () => { _display.ClearRoadMesh(); };

            SerializedProperty saveInProperty = serializedRoad.FindProperty(nameof(_current.SaveIn));
            PropertyField saveInField = CreatePropertyField(parent, saveInProperty);
            Button save = CreateButton(parent, uss_windowItem, "Save to Prefab");
            save.clicked += () => { _display.SaveNewGameObjectToPrefab(); };
        }

        /// <summary>
        /// Create a single button.
        /// </summary>
        /// <param name="parent">Parent of the structure.</param>
        /// <param name="ussName">USS class name to assign.</param>
        /// <param name="text">Text displayed on the button.</param>
        /// <returns>Returns the created button.</returns>
        private Button CreateButton(VisualElement parent, string ussName, string text)
        {
            Button button = new Button();
            button.AddToClassList(ussName);
            button.text = text;
            parent.Add(button);

            return button;
        }
    }
}