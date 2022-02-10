using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SH.RoadCreator.Algorithm
{
    /// <summary>
    /// This class is responsible for generating all meshes and textures for segments and roads.
    /// </summary>
    public class Mesher
    {
        public List<Vector3> Vertices { get; private set; } = new List<Vector3>();
        public List<int> Triangles { get; private set; } = new List<int>();
        public List<Color> Colors { get; private set; } = new List<Color>();
        public List<Vector2> UV { get; private set; } = new List<Vector2>();
        public Color[,] Pixels { get; private set; }

        /// <summary>
        /// Construct a Mesher with the selected resolution for the patterns texture.
        /// </summary>
        /// <param name="resolution">Selected resolution (E.g. 1024x1024).</param>
        public Mesher(Vector2Int resolution)
        {
            this.Pixels = new Color[resolution.x, resolution.y];
        }

        /// <summary>
        /// Clear a Mesher lists.
        /// </summary>
        public void Clear()
        {
            Vertices.Clear();
            Triangles.Clear();
            Colors.Clear();
            UV.Clear();
        }

        /// <summary>
        /// Create a unity mesh form the Mesher lists.
        /// </summary>
        /// <returns>Returns a unity mesh.</returns>
        public Mesh ToUnityMesh()
        {
            Mesh mesh = new Mesh();
            mesh.name = "RoadMesh";

            mesh.vertices = Vertices.ToArray();
            mesh.triangles = Triangles.ToArray();
            mesh.colors = Colors.ToArray();
            mesh.uv = UV.ToArray();

            mesh.RecalculateBounds();
            mesh.RecalculateTangents();
            mesh.RecalculateNormals();
            mesh.OptimizeIndexBuffers();
            mesh.OptimizeReorderVertexBuffer();
            mesh.Optimize();

            return mesh;
        }

        /// <summary>
        /// Create a unity texture from the Mesher array of colors.
        /// </summary>
        /// <returns>Returns a unity texture.</returns>
        public Texture2D ToUnityTexture()
        {
            Texture2D texture = new Texture2D(Pixels.GetLength(0), Pixels.GetLength(1), TextureFormat.RGBA32, -1, true);
            texture.name = "RoadTexture";

            for (int x = 0; x < texture.width; x++)
                for (int y = 0; y < texture.height; y++)
                    texture.SetPixel(x, y, Pixels[x, y]);

            texture.filterMode = FilterMode.Point;
            texture.anisoLevel = 1;
            texture.wrapMode = TextureWrapMode.Repeat;
            texture.Apply();
            return texture;
        }

        /// <summary>
        /// Generate a segment mesh from the selected segment.
        /// </summary>
        /// <param name="segment">Selected segment.</param>
        public void GenerateMeshFormSegment(Segment segment)
        {
            Vector3[] template = GenerateTemplateProfile(segment);
            List<Vector3[]> profiles = new List<Vector3[]>();
            Vector3 offset = new Vector3(0, 0, segment.Length);
            profiles.Add(GetTransformedProfile(template, Vector3.zero, offset, false));
            profiles.Add(new Vector3[profiles[0].Length]);

            for (int i = 0; i < profiles[0].Length; i++)
                profiles[1][i] = profiles[0][i] + offset;

            CalculateVertices(profiles);
            CalculateTriangles(profiles);
            CalculateColors(profiles.Count, segment.Modules);
            CalculateUV(profiles, template, segment.Length);
            AddPatternsToPixels(segment.Patterns);
        }

        /// <summary>
        /// Generate a road mesh from the selected road.
        /// </summary>
        /// <param name="road">Selected road.</param>
        public void GenerateMeshFormRoad(Road road)
        {
            Vector3[] template = GenerateTemplateProfile(road.RoadSegment);
            List<Vector3[]> profiles = GenerateProfileListForRoad(road, template);

            CalculateVertices(profiles);
            CalculateTriangles(profiles);
            CalculateColors(profiles.Count, road.RoadSegment.Modules);
            CalculateUV(profiles, template, road.RoadSegment.Length);
            AddPatternsToPixels(road.RoadSegment.Patterns);
        }

        /// <summary>
        /// Generate an array of segment profile positions without any transformations.
        /// </summary>
        /// <param name="segment">Selected segment.</param>
        /// <returns>Returns an array of profile position points.</returns>
        private Vector3[] GenerateTemplateProfile(Segment segment)
        {
            List<Vector3> profile = new List<Vector3>();

            float width = 0;
            profile.Add(new Vector3(0, 0, 0));
            foreach (Module module in segment.Modules)
            {
                profile.Add(new Vector3(width, module.Height, 0));
                profile.Add(new Vector3(width + module.Width, module.Height, 0));
                width += module.Width;
            }
            profile.Add(new Vector3(width, 0, 0));

            for (int i = 0; i < profile.Count; i++)
                profile[i] = new Vector3(profile[i].x - width / 2f, profile[i].y, profile[i].z);

            return profile.ToArray();
        }

        /// <summary>
        /// Generate a segment profile from the array of positions at each road point.
        /// </summary>
        /// <param name="road">A road object that contains a list of road points.</param>
        /// <param name="template">An array of positions that making up the template segment profile.</param>
        /// <returns>Returns a complete list of profiles at each road points.</returns>
        private List<Vector3[]> GenerateProfileListForRoad(Road road, Vector3[] template)
        {
            List<Vector3[]> profiles = new List<Vector3[]>();

            for (int i = 0; i < road.Count; i++)
            {
                for (int j = 0; j < road[i].Count; j++)
                {
                    Vector3[] profile;

                    if (i >= road.Count - 1 && j >= road[i].Count - 1)
                        profile = GetTransformedProfile(template, road[i].FirstPosition, road[i - 1].LastPosition, true);
                    else if (j >= road[i].Count - 1)
                        profile = GetTransformedProfile(template, road[i][j], road[i + 1].FirstPosition, false);
                    else
                        profile = GetTransformedProfile(template, road[i][j], road[i][j + 1], false);

                    profiles.Add(profile);
                }
            }

            return profiles;
        }

        /// <summary>
        /// Create a copy of the segment profile from the template, add an offset and rotation to the copied segment profile. 
        /// </summary>
        /// <param name="template">An array of positions that making up the template segment profile.</param>
        /// <param name="current">The offset to be added to the copied profile.</param>
        /// <param name="next">The next position to calculate the direction of the copied profile.</param>
        /// <param name="reverse">If is true calculates the direction from next to current (Useful for the last position point).</param>
        /// <returns>Returns the copied profile from the template with an offset and rotation added.</returns>
        private Vector3[] GetTransformedProfile(Vector3[] template, Vector3 current, Vector3 next, bool reverse)
        {
            Vector3[] profile = (Vector3[])template.Clone();
            Vector3 direction = reverse ? current - next : next - current;

            AddRotationToProfile(profile, direction);
            AddOffsetToProfile(profile, current);

            return profile;
        }

        /// <summary>
        /// Add rotation to the selected segment profile based on the direction.
        /// </summary>
        /// <param name="profile">Selected segment profile.</param>
        /// <param name="direction">Selected direction.</param>
        private void AddRotationToProfile(Vector3[] profile, Vector3 direction)
        {
            Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, direction);

            for (int i = 0; i < profile.Length; i++)
                profile[i] = rotation * profile[i];
        }

        /// <summary>
        /// Add an offset to the selected segment profile.
        /// </summary>
        /// <param name="profile">Selected segment profile.</param>
        /// <param name="offset">Selected offset.</param>
        private void AddOffsetToProfile(Vector3[] profile, Vector3 offset)
        {
            for (int i = 0; i < profile.Length; i++)
                profile[i] = profile[i] + offset;
        }

        /// <summary>
        /// Determine the vertices for the list of segment profiles and add them to the Mesher list of vertices.
        /// </summary>
        /// <param name="profiles">Selected list of segment profiles.</param>
        private void CalculateVertices(List<Vector3[]> profiles)
        {
            for (int i = 0; i < profiles.Count; i++)
            {
                for (int j = 0; j < profiles[i].Length; j++)
                {
                    Vertices.Add(profiles[i][j]);
                    if (j > 0 && j < profiles[i].Length - 1)
                        Vertices.Add(profiles[i][j]);
                }
            }
        }

        /// <summary>
        /// Specify the order of vertices based on the list of segment profile to create the mesh triangles. 
        /// </summary>
        /// <param name="profiles">Selected list of segment profiles.</param>
        private void CalculateTriangles(List<Vector3[]> profiles)
        {
            for (int i = 0; i < profiles.Count - 1; i++)
            {
                int length = profiles[i].Length * 2 - 2;
                for (int j = 0; j < length - 1; j++)
                {
                    Triangles.Add(((i) * length) + j);
                    Triangles.Add(((i + 1) * length) + j);
                    Triangles.Add(((i + 1) * length) + j + 1);

                    Triangles.Add(((i + 1) * length) + j + 1);
                    Triangles.Add(((i) * length) + j + 1);
                    Triangles.Add(((i) * length) + j);
                }
            }
        }

        /// <summary>
        /// Determine the color of the vertices based on the list of segment modules and add colors for each segment profile.
        /// </summary>
        /// <param name="profilesAmount">Number of segment profiles.</param>
        /// <param name="modules">List of segment modules.</param>
        private void CalculateColors(int profilesAmount, List<Module> modules)
        {
            for (int i = 0; i < profilesAmount; i++)
            {
                float lastHeight = float.PositiveInfinity;

                AddDoubleColor(modules[0].BaseColor);
                for (int j = 0; j < modules.Count; j++)
                {
                    if (lastHeight > modules[j].Height && j != 0) AddDoubleColor(modules[j - 1].BaseColor);
                    AddDoubleColor(modules[j].BaseColor);
                    if (lastHeight <= modules[j].Height) AddDoubleColor(modules[j].BaseColor);
                    lastHeight = modules[j].Height;
                }
                AddDoubleColor(modules[modules.Count - 1].BaseColor);
            }
        }

        private void AddDoubleColor(Color color)
        {
            Colors.Add(color);
            Colors.Add(color);
        }

        /// <summary>
        /// Calculate the uv coordinates of the vertices based on the template profile of the segment (uv width includes width and height).
        /// </summary>
        /// <param name="profiles">Selected list of segment profiles.</param>
        /// <param name="template">An array of positions that making up the template segment profile.</param>
        /// <param name="length">Base length of the segment.</param>
        private void CalculateUV(List<Vector3[]> profiles, Vector3[] template, float length)
        {
            float widthUVSum = GetVertexUVWidth(template, template.Length - 1);
            int halfCount = Mathf.RoundToInt(template.Length / 2f);
            float lengthUV = 0;

            for (int i = 0; i < profiles.Count; i++)
            {
                lengthUV += (i <= 0) ? 0 : (profiles[i - 1][halfCount] - profiles[i][halfCount]).magnitude;
                float currentWidthUV = 0;

                UV.Add(new Vector2(0, lengthUV));
                for (int j = 1; j < profiles[i].Length - 1; j++)
                {
                    currentWidthUV = GetVertexUVWidth(template, j);
                    UV.Add(new Vector2(currentWidthUV / widthUVSum, lengthUV / length));
                    UV.Add(new Vector2(currentWidthUV / widthUVSum, lengthUV / length));
                }
                UV.Add(new Vector2(1f, lengthUV));
            }
        }

        /// <summary>
        /// Get the uv width of the selected vertex index.
        /// </summary>
        /// <param name="profile">Selected segment profile list.</param>
        /// <param name="vertexIndex">The index of the selected vertex.</param>
        /// <returns>Returns the uv width for the selected vertex.</returns>
        private float GetVertexUVWidth(Vector3[] profile, int vertexIndex)
        {
            float widthSum = Mathf.Abs(profile[0].x) + Mathf.Abs(profile[profile.Length - 1].x);
            float widthUV = 0;

            for (int i = 0; i < vertexIndex; i++)
            {
                Vector2 current = new Vector2(profile[i].x + widthSum / 2f, profile[i].y);
                Vector2 next = new Vector2(profile[i + 1].x + widthSum / 2f, profile[i + 1].y);

                if (next.x != current.x) widthUV += Mathf.Abs(next.x - current.x);
                if (next.y != current.y) widthUV += Mathf.Abs(next.y - current.y);
            }

            return widthUV;
        }

        /// <summary>
        /// Add all the patterns form the list to the Mesher array of colors.
        /// </summary>
        /// <param name="patterns">List of segment patterns.</param>
        private void AddPatternsToPixels(List<Pattern> patterns)
        {
            SetPixelsTransparent();

            foreach (Pattern pattern in patterns)
            {
                switch (pattern.PatternType)
                {
                    case Pattern.Type.Fill:
                        AddFillPattern(pattern);
                        break;
                }
            }
        }

        /// <summary>
        /// Fill the Mesher array of colors with transparent white.
        /// </summary>
        private void SetPixelsTransparent()
        {
            for (int x = 0; x < Pixels.GetLength(0); x++)
                for (int y = 0; y < Pixels.GetLength(1); y++)
                    Pixels[x, y] = new Color(1f, 1f, 1f, 0);
        }

        /// <summary>
        /// Add the fill pattern to the Mesher array of color.
        /// </summary>
        /// <param name="pattern">Pattern scheme.</param>
        private void AddFillPattern(Pattern pattern)
        {
            int width = Pixels.GetLength(0);
            int height = Pixels.GetLength(1);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float offsetX = width * (pattern.Offset.x - pattern.Size.x / 2f);
                    float offsetY = height * (pattern.Offset.y - pattern.Size.y / 2f);

                    float tilingX = (width - offsetX + x) % (width / pattern.Tiling.x);
                    float tilingY = (height - offsetY + y) % (height / pattern.Tiling.y);

                    bool checkX = tilingX <= pattern.Size.x * (width / pattern.Tiling.x) ? true : false;
                    bool checkY = tilingY <= pattern.Size.y * (height / pattern.Tiling.y) ? true : false;

                    if (checkX && checkY)
                    {
                        Pixels[x, y] = pattern.BaseColor;
                        if (pattern.SymmetryX) Pixels[width - x, y] = pattern.BaseColor;
                        if (pattern.SymmetryY) Pixels[x, height - y] = pattern.BaseColor;
                    }
                }
            }
        }
    }
}