using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;

namespace SH.RoadCreator.Algorithm
{
    /// <summary>
    /// This class is responsible for the logic for displaying the road mesh and texture in the editor scene.
    /// </summary>
    public class RoadDisplay
    {
        private GameObject _gameObject;
        private Shader _roadShader;
        private Road _current;
        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;

        private const string _shaderSRPName = "RoadShaderSRP";
        private const string _shaderURPName = "RoadShaderURP";

        /// <summary>
        /// Current active road project.
        /// </summary>
        public Road Current { get => _current; }

        /// <summary>
        /// Create the preview game object for the road and add the required components.
        /// </summary>
        /// <param name="name">Name for the preview game object.</param>
        public void CreateRoadObject(Road road)
        {
            if (GraphicsSettings.currentRenderPipeline != null)
                _roadShader = Resources.Load<Shader>(_shaderURPName);
            else
                _roadShader = Resources.Load<Shader>(_shaderSRPName);

            _current = road;
            _gameObject = new GameObject("[PreviewDoNotUse]" + road.name);
            _meshFilter = _gameObject.AddComponent<MeshFilter>();
            _meshRenderer = _gameObject.AddComponent<MeshRenderer>();
            _meshRenderer.sharedMaterial = new Material(_roadShader);
        }

        /// <summary>
        /// Destroy the preview game object for the road.
        /// </summary>
        public void DestroyRoadObject()
        {
            if (_gameObject != null) MonoBehaviour.DestroyImmediate(_gameObject);
        }

        /// <summary>
        /// Generate a road mesh from the current segment and assign it to the preview game object.
        /// </summary>
        public void GenerateRoadMesh()
        {
            if (_current == null || _current.RoadPoints == null || _current.Count <= 0) return;

            Mesher mesher = new Mesher(_current.RoadSegment.Resolution);
            mesher.GenerateMeshFormRoad(_current);

            _meshFilter.sharedMesh = mesher.ToUnityMesh();
            _meshRenderer.sharedMaterial.SetTexture("_Patterns", mesher.ToUnityTexture());
        }

        /// <summary>
        /// Remove mesh and texture of the preview game object.
        /// </summary>
        public void ClearRoadMesh()
        {
            if (_meshFilter != null) _meshFilter.sharedMesh = null;
            if (_meshRenderer != null) _meshRenderer.sharedMaterial.SetTexture("_patterns", null);
        }

        /// <summary>
        /// Add a road point with the desired position to the current road asset.
        /// </summary>
        /// <param name="position">Desired position.</param>
        public void AddPoint(Vector3 position)
        {
            RoadPoint point = new RoadPoint(position);
            _current.AddRoadPoint(point);

            int count = _current.Count;
            CalculateTangentsForAffectedRoadPoints(count - 1);
            CalculatePositionsForAffectedRoadPoints(count - 1);
        }

        /// <summary>
        /// Remove a road point with the selected index.
        /// </summary>
        /// <param name="index">Selected index.</param>
        public void RemovePoint(int index)
        {
            if (!IsIndexValid(index, _current.Count)) return;
            Undo.RecordObject(_current, "Remove Road Point");

            _current.RemoveRoadPointAt(index);
            if (_current.AutoRelocateTangents) CalculateTangentsForAffectedRoadPoints(index);
            CalculatePositionsForAffectedRoadPoints(index);
        }

        /// <summary>
        /// Move a road point with the selected index to the desired position.
        /// </summary>
        /// <param name="position">Desired position.</param>
        /// <param name="index">Selected index.</param>
        public void MovePoint(Vector3 position, int index)
        {
            Vector3 deltaPosition = _current[index].FirstPosition - position;
            _current[index].FirstPosition -= deltaPosition;

            if (IsTangentValid(_current[index].TangentStart)) _current[index].TangentStart -= deltaPosition;
            if (IsTangentValid(_current[index].TangentEnd)) _current[index].TangentEnd -= deltaPosition;

            if (_current.AutoRelocateTangents) CalculateTangentsForAffectedRoadPoints(index);
            CalculatePositionsForAffectedRoadPoints(index);
        }

        /// <summary>
        /// Move the tangent point of the road point to the desired position.
        /// </summary>
        /// <param name="position">Desired position.</param>
        /// <param name="roadPointIndex">Index of the selected road point.</param>
        /// <param name="tangentIndex">Index of the selected tangent (0 - TangentStart, 1 - TangentEnd).</param>
        public void MoveTangent(Vector3 position, int roadPointIndex, int tangentIndex)
        {
            RoadPoint point = _current[roadPointIndex];
            Vector3 deltaPosition = point.Tangents[tangentIndex] - position;
            point.Tangents[tangentIndex] -= deltaPosition;

            int oppositeIndex = (tangentIndex + 1) % 2;
            if (IsTangentValid(point.Tangents[oppositeIndex]))
            {
                float distance = (point.FirstPosition - point.Tangents[oppositeIndex]).magnitude;
                Vector3 direction = (point.FirstPosition - position).normalized;
                point.Tangents[oppositeIndex] = point.FirstPosition + direction * distance;
            }

            CalculatePositionsForAffectedRoadPoints(roadPointIndex);
        }

        /// <summary>
        /// Calculate a tangents for the selected road point and its neighbors.
        /// </summary>
        /// <param name="index">Index of the selected road point.</param>
        private void CalculateTangentsForAffectedRoadPoints(int index)
        {
            int count = _current.Count;
            if (!IsIndexValid(index, count)) return;

            for (int i = index - 1; i < index + 1; i++)
            {
                RoadPoint prevoius = IsIndexValid(i - 1, count) ? _current[i - 1] : null;
                RoadPoint current = IsIndexValid(i, count) ? _current[i] : null;
                RoadPoint next = IsIndexValid(i + 1, count) ? _current[i + 1] : null;
                CalculateTangentsForRoadPoint(prevoius, current, next);
            }
        }

        /// <summary>
        /// Calculate a tangent for the selected road point.
        /// </summary>
        /// <param name="previous">Point needed to calculate the tangent start.</param>
        /// <param name="current">Selected road point.</param>
        /// <param name="next">Point needed to calculate the tangent end.</param>
        private void CalculateTangentsForRoadPoint(RoadPoint previous, RoadPoint current, RoadPoint next)
        {
            if (current == null) return;

            Vector3 direction = Vector3.zero;
            float[] distances = new float[2];

            if (previous != null)
            {
                direction += (previous.FirstPosition - current.FirstPosition).normalized;
                distances[0] = (previous.FirstPosition - current.FirstPosition).magnitude;
            }

            if (next != null)
            {
                direction -= (next.FirstPosition - current.FirstPosition).normalized;
                distances[1] = -(next.FirstPosition - current.FirstPosition).magnitude;
            }

            direction.Normalize();
            if (previous != null) current.TangentStart = current.FirstPosition + direction * distances[0] * 0.5f;
            if (next != null) current.TangentEnd = current.FirstPosition + direction * distances[1] * 0.5f;
        }

        /// <summary>
        /// Calculate the positions and number of position points for the road point and its neighbors.
        /// </summary>
        /// <param name="index">Index of the selected road point.</param>
        private void CalculatePositionsForAffectedRoadPoints(int index)
        {
            int count = _current.Count;
            if (!IsIndexValid(index, count)) return;

            for (int i = index - 2; i < index + 2; i++)
            {
                RoadPoint current = IsIndexValid(i, count) ? _current[i] : null;
                RoadPoint next = IsIndexValid(i + 1, count) ? _current[i + 1] : null;
                CalculatePositionsForRoadPoint(current, next);
            }
        }

        /// <summary>
        /// Calculate the positions and number of position points for the road point.
        /// </summary>
        /// <param name="currrent">Selected road point.</param>
        /// <param name="next">Point needed to establish direction.</param>
        private void CalculatePositionsForRoadPoint(RoadPoint currrent, RoadPoint next)
        {
            if (currrent == null || next == null) return;
            currrent.Clear();

            float length = Bezier.ApproximateCubicLength(currrent.FirstPosition, currrent.TangentEnd, next.TangentStart, next.FirstPosition);
            int fragments = Mathf.RoundToInt(length * _current.Quality);

            for (int i = 1; i < fragments; i++)
                currrent.AddPosition(Bezier.Cubic(currrent.FirstPosition, currrent.TangentEnd, next.TangentStart, next.FirstPosition, (float)i / fragments));
        }

        /// <summary>
        /// Check that the index is equal to or greater than 0 and less than the length of the list.
        /// </summary>
        /// <param name="index">Index to check.</param>
        /// <param name="listCount">Length of the list.</param>
        /// <returns>Returns true if the index is in range, false if is not.</returns>
        public bool IsIndexValid(int index, int listCount)
        {
            if (index >= 0 && index < listCount) return true;
            else return false;
        }

        /// <summary>
        /// Check that the tangent has a position different from zero (disabled position).
        /// </summary>
        /// <param name="tangent">Tangent to check.</param>
        /// <returns>Returns true if the tangnet position is different from zero, false if is not.</returns>
        public bool IsTangentValid(Vector3 tangent)
        {
            if (tangent != Vector3.zero) return true;
            else return false;
        }

        /// <summary>
        /// Create a new game object, generate mesh and texture for it, and then save it to the prefab.
        /// </summary>
        public void SaveNewGameObjectToPrefab()
        {
            GameObject objectToSave = new GameObject(_current.name);
            MeshFilter meshFilterToSave = objectToSave.AddComponent<MeshFilter>();
            MeshRenderer meshRendererToSave = objectToSave.AddComponent<MeshRenderer>();
            Material materialToSave = new Material(_roadShader);

            Mesher mesher = new Mesher(_current.RoadSegment.Resolution);
            mesher.GenerateMeshFormRoad(_current);

            meshFilterToSave.sharedMesh = mesher.ToUnityMesh();
            meshRendererToSave.sharedMaterial = materialToSave;
            meshRendererToSave.sharedMaterial.SetTexture("_Patterns", mesher.ToUnityTexture());

            SaveToPrefab.SaveGameObject(objectToSave, _current.name, "Assets/" + _current.SaveIn);
            MonoBehaviour.DestroyImmediate(objectToSave);
        }
    }
}