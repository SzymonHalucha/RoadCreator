using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SH.RoadCreator.Algorithm;

namespace SH.RoadCreator.Viusal
{
    /// <summary>
    /// This class is responsible for displaying the road mesh and texture in the editor window.
    /// </summary>
    public class SelectedRoadScene
    {
        private RoadDisplay _display;
        private bool _needRepaint;

        /// <summary>
        /// Add the road displaying method to the current editor scene.
        /// </summary>
        /// <param name="display">An object responsible for the logic of displaying the road mesh and texture in the editor scene.</param>
        public void UpdateSceneGUI(RoadDisplay display)
        {
            _display = display;
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;
        }

        /// <summary>
        /// Remove the road displaying method from the current editor scene.
        /// </summary>
        public void DisableSceneGUI()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        /// <summary>
        /// Method responsible for displaying the road mesh and texture in the editor scene.
        /// </summary>
        public void OnSceneGUI(SceneView view)
        {
            DrawPoints();
            DrawTangents();

            switch (Event.current.type)
            {
                case EventType.KeyDown:
                    if (Event.current.isKey && Event.current.keyCode == KeyCode.D) RemovePointOnMousePosition();
                    break;

                case EventType.MouseDown:
                    if (Event.current.shift && Event.current.button == 0) AddPointOnMousePosition();
                    break;

                case EventType.Layout:
                    SetFocusOnEditor();
                    break;

                case EventType.Repaint:
                    DrawPointsLines();
                    DrawTangentsLines();
                    IfNeedRepaint();
                    break;
            }
        }

        /// <summary>
        /// Add a road point at the cursor position.
        /// </summary>
        private void AddPointOnMousePosition()
        {
            Vector3 position = GetMouseWorldPosition(0);
            _display.AddPoint(position);
            _needRepaint = true;
        }

        /// <summary>
        /// Remove a road point at the cursor position.
        /// </summary>
        private void RemovePointOnMousePosition()
        {
            float minDistance = float.PositiveInfinity;
            int minIndex = -1;

            for (int i = 0; i < _display.Current.Count; i++)
            {
                Vector3 mousePosition = GetMouseWorldPosition(_display.Current[i].FirstPosition.y);
                float currentDistance = (mousePosition - _display.Current[i].FirstPosition).sqrMagnitude;
                float size = HandleUtility.GetHandleSize(_display.Current[i].FirstPosition) * _display.Current.GizmosSize;

                if (minDistance < currentDistance || currentDistance > size * 4f) continue;

                minDistance = currentDistance;
                minIndex = i;
            }

            if (minIndex >= 0) _display.RemovePoint(minIndex);
        }

        /// <summary>
        /// Convert cursor position on screen to 3D position in editor scene.
        /// </summary>
        /// <param name="targetHeight">Target height in the editor scene for which the position is to be calculated.</param>
        /// <returns>Returns the 3D position of the cursor in the editor scene.</returns>
        private Vector3 GetMouseWorldPosition(float targetHeight)
        {
            Vector3 mouse = Event.current.mousePosition;
            Ray ray = HandleUtility.GUIPointToWorldRay(mouse);
            float distance = (targetHeight - ray.origin.y) / ray.direction.y;
            return ray.GetPoint(Mathf.Abs(distance));
        }

        /// <summary>
        /// Draw road points for the active project.
        /// </summary>
        private void DrawPoints()
        {
            if (_display.Current == null || _display.Current.Count <= 0) return;

            Handles.color = _display.Current.GizmosColor;
            for (int i = 0; i < _display.Current.Count; i++)
            {
                float size = HandleUtility.GetHandleSize(_display.Current[i].FirstPosition) * _display.Current.GizmosSize / 10f;
                Vector3 newPosition = Handles.FreeMoveHandle(_display.Current[i].FirstPosition, Quaternion.identity, size, Vector3.zero, Handles.DotHandleCap);

                if (newPosition == _display.Current[i].FirstPosition) continue;
                _display.MovePoint(newPosition, i);
            }
        }

        /// <summary>
        /// Draw the tangents of the road points for the active project.
        /// </summary>
        private void DrawTangents()
        {
            if (_display.Current == null || _display.Current.Count <= 0 || _display.Current.AutoRelocateTangents) return;

            Handles.color = Color.black;
            for (int i = 0; i < _display.Current.Count; i++)
            {
                for (int j = 0; j < _display.Current[i].Tangents.Length; j++)
                {
                    if (!_display.IsTangentValid(_display.Current[i].Tangents[j])) continue;

                    float size = HandleUtility.GetHandleSize(_display.Current[i].Tangents[j]) * _display.Current.GizmosSize / 15f;
                    Vector3 newPosition = Handles.FreeMoveHandle(_display.Current[i].Tangents[j], Quaternion.identity, size, Vector3.zero, Handles.DotHandleCap);

                    if (newPosition == _display.Current[i].Tangents[j]) continue;
                    _display.MoveTangent(newPosition, i, j);
                }
            }
        }

        /// <summary>
        /// Draw lines between road points for the active project.
        /// </summary>
        private void DrawPointsLines()
        {
            if (_display.Current == null || _display.Current.Count <= 0) return;

            Handles.color = _display.Current.GizmosColor;
            for (int i = 0; i < _display.Current.Count - 1; i++)
            {
                RoadPoint currentPoint = _display.Current[i];
                RoadPoint nextPoint = _display.Current[i + 1];

                for (int j = 0; j < currentPoint.Count - 1; j++)
                {
                    SwapHandlesColor(_display.Current.GizmosColor, Color.black);
                    Handles.DrawLine(currentPoint[j], currentPoint[j + 1]);
                }
                SwapHandlesColor(_display.Current.GizmosColor, Color.black);
                Handles.DrawLine(currentPoint.LastPosition, nextPoint.FirstPosition);
            }
        }

        /// <summary>
        /// Draw lines between tangents of the road points for the active project.
        /// </summary>
        private void DrawTangentsLines()
        {
            if (_display.Current == null || _display.Current.Count <= 0 || _display.Current.AutoRelocateTangents) return;

            Handles.color = Color.black;
            foreach (RoadPoint point in _display.Current.RoadPoints)
            {
                foreach (Vector3 tangent in point.Tangents)
                {
                    if (!_display.IsTangentValid(tangent)) continue;
                    Handles.DrawDottedLine(point[0], tangent, 2f);
                }
            }
        }

        private void SwapHandlesColor(Color a, Color b)
        {
            if (Handles.color != a) Handles.color = a;
            else Handles.color = b;
        }

        private void IfNeedRepaint()
        {
            if (_needRepaint)
            {
                HandleUtility.Repaint();
                _needRepaint = false;
            }
        }

        /// <summary>
        /// Set the focus for the game object of the active road project.
        /// </summary>
        private void SetFocusOnEditor()
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        }
    }
}