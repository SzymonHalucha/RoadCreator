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
    /// This class is responsible for displaying the window with a list of segment projects.
    /// </summary>
    public class SegmentList
    {
        private VisualElement _root;
        private SegmentEditorWindow _window;

        private const string uss_card = "card";
        private const string uss_cardTitle = "card-title";
        private const string uss_cardInfo = "card-info";
        private const string uss_cardPath = "card-path";

        public SegmentList(VisualElement root, SegmentEditorWindow window)
        {
            this._root = root;
            this._window = window;
        }

        private void OnRefreshButtonClicked()
        {
            _window.UpdateGUI();
        }

        private void OnCardButtonClicked(Segment segment)
        {
            _window.UpdateGUI(segment);
        }

        /// <summary>
        /// Search for all segment projects files and display in the window.
        /// </summary>
        public void UpdateGUI()
        {
            _root.Q<Button>("refresh-button").clicked += () => { OnRefreshButtonClicked(); };

            string[] paths = SearchForSegmentsPaths();
            Segment[] segments = GetSegmentsFromPaths(paths);
            VisualElement list = _root.Q<VisualElement>("card-list");

            for (int i = 0; i < paths.Length; i++)
                CreateCard(list, segments[i], paths[i]);
        }

        /// <summary>
        /// Search for paths to segment project files.
        /// </summary>
        /// <returns>Returns an array of all file paths found.</returns>
        private string[] SearchForSegmentsPaths()
        {
            string[] guids = AssetDatabase.FindAssets("t: RoadCreator.Algorithm.Segment");
            string[] paths = new string[guids.Length];

            for (int i = 0; i < guids.Length; i++)
                paths[i] = AssetDatabase.GUIDToAssetPath(guids[i]);

            return paths;
        }

        /// <summary>
        /// Load segment projects from an array of file paths.
        /// </summary>
        /// <param name="paths">Array of file paths.</param>
        /// <returns>Returns an array of segment projects.</returns>
        private Segment[] GetSegmentsFromPaths(string[] paths)
        {
            Segment[] segments = new Segment[paths.Length];

            for (int i = 0; i < paths.Length; i++)
                segments[i] = AssetDatabase.LoadAssetAtPath<Segment>(paths[i]);

            return segments;
        }

        /// <summary>
        /// Create a visual list object that represents the segment project object.
        /// </summary>
        /// <param name="parent">Parent of the visual object.</param>
        /// <param name="segment">Selected segment project.</param>
        /// <param name="filePath">Path to the segment project.</param>
        private void CreateCard(VisualElement parent, Segment segment, string filePath)
        {
            Button card = new Button();
            card.AddToClassList(uss_card);
            card.clicked += () => { OnCardButtonClicked(segment); };
            parent.Add(card);

            Label title = new Label(segment.name);
            title.AddToClassList(uss_cardTitle);
            card.Add(title);

            Label info = new Label($"Modules: {segment.Modules.Count}, Patterns: {segment.Patterns.Count}");
            info.AddToClassList(uss_cardInfo);
            card.Add(info);

            Label path = new Label($"Path: {filePath}");
            path.AddToClassList(uss_cardPath);
            card.Add(path);
        }
    }
}