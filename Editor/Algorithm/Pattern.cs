using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SH.RoadCreator.Algorithm
{
    /// <summary>
    /// This class is used to store information about the pattern schema.
    /// </summary>
    [Serializable]
    public class Pattern
    {
        public string Name = "New Pattern";
        public Vector2 Size = new Vector2(0.5f, 0.5f);
        public Vector2 Tiling = new Vector2(1f, 1f);
        public Vector2 Offset = new Vector2(0, 0);
        public bool SymmetryX = false;
        public bool SymmetryY = false;
        public Color BaseColor = Color.gray;
        public Type PatternType = Type.Fill;

        public enum Type { Fill }

        public Pattern()
        {

        }

        public Pattern(Pattern orginal)
        {
            this.Name = orginal.Name;
            this.Size = orginal.Size;
            this.Tiling = orginal.Tiling;
            this.Offset = orginal.Offset;
            this.SymmetryX = orginal.SymmetryX;
            this.SymmetryY = orginal.SymmetryY;
            this.BaseColor = orginal.BaseColor;
            this.PatternType = orginal.PatternType;
        }
    }
}