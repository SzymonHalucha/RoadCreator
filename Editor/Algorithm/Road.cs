using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SH.RoadCreator.Algorithm
{
    /// <summary>
    /// This class is used to store information about road project, road paths and segment assigned to them.
    /// </summary>
    [CreateAssetMenu(menuName = "Road Creator/Road", fileName = "New Road", order = 0)]
    public class Road : ScriptableObject
    {
        public Segment RoadSegment = null;
        public float Quality = 1f;
        public bool AutoRelocateTangents = true;
        public float GizmosSize = 0.5f;
        public Color GizmosColor = Color.yellow;
        public string SaveIn = "Prefabs/Roads/";

        [SerializeField] private List<RoadPoint> _points = new List<RoadPoint>();

        public List<RoadPoint> RoadPoints { get => _points; }
        public RoadPoint LastRoadPoint { get => _points[_points.Count - 1]; set => _points[_points.Count - 1] = value; }
        public int Count { get => _points.Count; }
        public RoadPoint this[int index] { get => _points[index]; }

        public void AddRoadPoint(RoadPoint point) => _points.Add(point);
        public void RemoveRoadPointAt(int index) => _points.RemoveAt(index);
        public void RemoveRoadPoint(RoadPoint point) => _points.Remove(point);
        public void InsertRoadPoint(int index, RoadPoint point) => _points.Insert(index, point);
        public void Clear() => _points.Clear();
    }
}