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
    /// This class is responsible for the logic for displaying the segment mesh and texture in the editor scene.
    /// </summary>
    public class SegmentDisplay
    {
        private GameObject _gameObject;
        private Shader _roadShader;
        private Segment _current;
        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;

        private const string _shaderSRPName = "RoadShaderSRP";
        private const string _shaderURPName = "RoadShaderURP";

        /// <summary>
        /// Current active segment project.
        /// </summary>
        public Segment Current { get => _current; }

        /// <summary>
        /// Create a preview game object for the segment and add the required components.
        /// </summary>
        /// <param name="name">Name for the preview game object.</param>
        public void CreateSegmentObject(string name)
        {
            if (GraphicsSettings.currentRenderPipeline != null)
                _roadShader = Resources.Load<Shader>(_shaderURPName);
            else
                _roadShader = Resources.Load<Shader>(_shaderSRPName);

            _gameObject = new GameObject("[PreviewDoNotUse]" + name);
            _meshFilter = _gameObject.AddComponent<MeshFilter>();
            _meshRenderer = _gameObject.AddComponent<MeshRenderer>();
            _meshRenderer.sharedMaterial = new Material(_roadShader);
        }

        /// <summary>
        /// Destroy the preview game object for the segment.
        /// </summary>
        public void DestroySegmentObject()
        {
            if (_gameObject != null) MonoBehaviour.DestroyImmediate(_gameObject);
        }

        /// <summary>
        /// Generate a segment mesh from the selected segment and assign to the preview game object.
        /// </summary>
        /// <param name="segment">Selected segment.</param>
        public void GenerateSegmentMesh(Segment segment)
        {
            if (segment == null || segment.Modules == null || segment.Modules.Count <= 0) return;
            if (_gameObject == null) CreateSegmentObject(segment.name);
            _current = segment;

            Mesher mesher = new Mesher(segment.Resolution);
            mesher.GenerateMeshFormSegment(segment);

            _meshFilter.sharedMesh = mesher.ToUnityMesh();
            _meshRenderer.sharedMaterial.SetTexture("_Patterns", mesher.ToUnityTexture());
        }

        /// <summary>
        /// Remove the preview game object mesh and texture.
        /// </summary>
        public void ClearSegmentMesh()
        {
            if (_meshFilter != null) _meshFilter.sharedMesh = null;
            if (_meshRenderer != null) _meshRenderer.sharedMaterial.SetTexture("_patterns", null);
        }
    }
}