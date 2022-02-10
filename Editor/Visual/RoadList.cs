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
    /// This class is responsible for displaying the window with a list of road projects.
    /// </summary>
    public class RoadList
    {
        private VisualElement _root;
        private RoadEditorWindow _window;

        private const string uss_card = "card";
        private const string uss_cardTitle = "card-title";
        private const string uss_cardInfo = "card-info";
        private const string uss_cardPath = "card-path";

        public RoadList(VisualElement root, RoadEditorWindow window)
        {
            this._root = root;
            this._window = window;
        }

        private void OnRefreshButtonClicked()
        {
            _window.UpdateGUI();
        }

        private void OnCardButtonClicked(Road road)
        {
            _window.UpdateGUI(road);
        }

        /// <summary>
        /// Search for all road projects files and display in the window.
        /// </summary>
        public void UpdateGUI()
        {
            _root.Q<Button>("refresh-button").clicked += () => { OnRefreshButtonClicked(); };

            string[] paths = SearchForRoadPaths();
            Road[] roads = GetRoadsFromPaths(paths);
            VisualElement list = _root.Q<VisualElement>("card-list");

            for (int i = 0; i < paths.Length; i++)
                CreateCard(list, roads[i], paths[i]);
        }

        /// <summary>
        /// Search for paths to road project files.
        /// </summary>
        /// <returns>Returns an array of all file paths found.</returns>
        private string[] SearchForRoadPaths()
        {
            string[] guids = AssetDatabase.FindAssets("t: RoadCreator.Algorithm.Road");
            string[] paths = new string[guids.Length];

            for (int i = 0; i < guids.Length; i++)
                paths[i] = AssetDatabase.GUIDToAssetPath(guids[i]);

            return paths;
        }

        /// <summary>
        /// Load road projects from an array of file paths.
        /// </summary>
        /// <param name="paths">Array of file paths.</param>
        /// <returns>Returns an array of road projects.</returns>
        private Road[] GetRoadsFromPaths(string[] paths)
        {
            Road[] roads = new Road[paths.Length];

            for (int i = 0; i < paths.Length; i++)
                roads[i] = AssetDatabase.LoadAssetAtPath<Road>(paths[i]);

            return roads;
        }

        /// <summary>
        /// Create a visual list object that represents the road project object.
        /// </summary>
        /// <param name="parent">Parent of the visual object.</param>
        /// <param name="road">Selected road project.</param>
        /// <param name="filePath">Path to the road project.</param>
        private void CreateCard(VisualElement parent, Road road, string filePath)
        {
            Button card = new Button();
            card.AddToClassList(uss_card);
            card.clicked += () => { OnCardButtonClicked(road); };
            parent.Add(card);

            Label title = new Label(road.name);
            title.AddToClassList(uss_cardTitle);
            card.Add(title);

            string segmentName = (road.RoadSegment == null) ? "Null" : road.RoadSegment.name;
            Label info = new Label($"Segment: {segmentName}, Points: {road.Count}");
            info.AddToClassList(uss_cardInfo);
            card.Add(info);

            Label path = new Label($"Path: {filePath}");
            path.AddToClassList(uss_cardPath);
            card.Add(path);
        }
    }
}