using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SH.RoadCreator.Algorithm
{
    /// <summary>
    /// This class is used to store segment information such as pattern texture resolution, list of segment modules or list of segment patterns.
    /// </summary>
    [CreateAssetMenu(menuName = "Road Creator/Segment", fileName = "New Segment", order = 0)]
    public class Segment : ScriptableObject
    {
        public float Length = 8f;
        public Vector2Int Resolution = new Vector2Int(1024, 1024);
        public List<Module> Modules = new List<Module>();
        public List<Pattern> Patterns = new List<Pattern>();
    }
}