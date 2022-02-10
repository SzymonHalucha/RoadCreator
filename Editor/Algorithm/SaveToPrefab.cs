using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SH.RoadCreator.Algorithm
{
    /// <summary>
    /// This class allows you to save game objects and their dependencies to the prefab.
    /// </summary>
    public static class SaveToPrefab
    {
        private const string _prefabSuffix = ".prefab";
        private const string _meshSuffix = "_Mesh.asset";
        private const string _materialSuffix = "_Material.mat";
        private const string _textureSuffix = "_Texture.png";
        private const string _textureName = "_Patterns";

        /// <summary>
        /// Save the game object to the prefab with its dependencies, if any exists.
        /// </summary>
        /// <param name="gameObject">Selected game object.</param>
        /// <param name="name">Selected name (E.g. Road1).</param>
        /// <param name="path">Project relative path (E.g. Assets/Prefabs/Roads/).</param>
        public static void SaveGameObject(GameObject gameObject, string name, string path)
        {
            if (!Directory.Exists(Application.dataPath.Replace("/Assets", "/") + path))
                Directory.CreateDirectory(Application.dataPath.Replace("/Assets", "/") + path);

            if (path[path.Length - 1] != '/') path += '/';

            DeleteExistingFiles(name, path);

            SaveGameObjectMesh(gameObject, name, path);
            SaveGameObjectMaterial(gameObject, name, path);
            SaveGameObjectTexture(gameObject, name, path);
            SaveGameObjectPrefab(gameObject, name, path);

            AssetDatabase.Refresh();
            AddTextureToMaterial(name, path);
            ChangeTextureImportSettings(name, path);
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// Remove all prefabs and their dependencies with the specified name in the selected path, if any exists.
        /// </summary>
        /// <param name="name">Selected name (E.g. Road1).</param>
        /// <param name="path">Project relative path (E.g. Assets/Prefabs/Roads/).</param>
        private static void DeleteExistingFiles(string name, string path)
        {
            AssetDatabase.DeleteAsset(path + name + _meshSuffix);
            AssetDatabase.DeleteAsset(path + name + _materialSuffix);
            AssetDatabase.DeleteAsset(path + name + _textureSuffix);
            AssetDatabase.DeleteAsset(path + name + _prefabSuffix);
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Save the selected game object mesh to the asset file format (.asset).
        /// </summary>
        /// <param name="gameObject">Selected game object.</param>
        /// <param name="name">Selected name (E.g. Road1).</param>
        /// <param name="path">Project relative path (E.g. Assets/Prefabs/Roads/).</param>
        private static void SaveGameObjectMesh(GameObject gameObject, string name, string path)
        {
            MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
            if (meshFilter == null || meshFilter.sharedMesh == null) return;

            path += name + _meshSuffix;
            AssetDatabase.CreateAsset(meshFilter.sharedMesh, path);
        }

        /// <summary>
        /// Save the selected game object material to the material file format (.mat).
        /// </summary>
        /// <param name="gameObject">Selected game object.</param>
        /// <param name="name">Selected name (E.g. Road1).</param>
        /// <param name="path">Project relative path (E.g. Assets/Prefabs/Roads/).</param>
        private static void SaveGameObjectMaterial(GameObject gameObject, string name, string path)
        {
            MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
            if (meshRenderer == null || meshRenderer.sharedMaterial == null) return;

            path += name + _materialSuffix;
            AssetDatabase.CreateAsset(meshRenderer.sharedMaterial, path);
        }

        /// <summary>
        /// Save the selected game object texture to the file with PNG format.
        /// </summary>
        /// <param name="gameObject">Selected game object.</param>
        /// <param name="name">Selected name (E.g. Road1).</param>
        /// <param name="path">Project relative path (E.g. Assets/Prefabs/Roads/).</param>
        private static void SaveGameObjectTexture(GameObject gameObject, string name, string path)
        {
            MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
            if (meshRenderer == null || meshRenderer.sharedMaterial == null) return;

            Texture2D texture2D = meshRenderer.sharedMaterial.GetTexture(_textureName) as Texture2D;
            if (texture2D == null) return;

            path += name + _textureSuffix;
            File.WriteAllBytes(Application.dataPath.Replace("/Assets", "/") + path, texture2D.EncodeToPNG());
        }

        /// <summary>
        /// Change the PNG texture file import settings to best fit.
        /// </summary>
        /// <param name="name">Selected name (E.g. Road1).</param>
        /// <param name="path">Project relative path (E.g. Assets/Prefabs/Roads/).</param>
        private static void ChangeTextureImportSettings(string name, string path)
        {
            path += name + _textureSuffix;
            TextureImporter importer = TextureImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return;

            importer.alphaIsTransparency = true;
            importer.filterMode = FilterMode.Point;
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.isReadable = true;
            importer.SaveAndReimport();
        }

        /// <summary>
        /// Add the texture PNG file to the prefab material file.
        /// </summary>
        /// <param name="name">Selected name (E.g. Road1).</param>
        /// <param name="path">Project relative path (E.g. Assets/Prefabs/Roads/).</param>
        private static void AddTextureToMaterial(string name, string path)
        {
            string texturePath = path + name + _textureSuffix;
            string materialPath = path + name + _materialSuffix;

            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            if (texture == null) return;

            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null) return;

            material.SetTexture(_textureName, texture);
        }

        /// <summary>
        /// Save the game object to the prefab (.prefab).
        /// </summary>
        /// <param name="gameObject">Selected game object.</param>
        /// <param name="name">Selected name (E.g. Road1).</param>
        /// <param name="path">Project relative path (E.g. Assets/Prefabs/Roads/).</param>
        private static void SaveGameObjectPrefab(GameObject gameObject, string name, string path)
        {
            path += name + _prefabSuffix;
            PrefabUtility.SaveAsPrefabAsset(gameObject, path);
        }
    }
}