using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SH.RoadCreator.Algorithm
{
    /// <summary>
    /// This class is used to store information about road paths used in the scene editor.
    /// </summary>
    [Serializable]
    public class RoadPoint
    {
        public Vector3[] Tangents = new Vector3[2];

        [SerializeField] private List<Vector3> _positions = new List<Vector3>();

        public Vector3 TangentStart { get => Tangents[0]; set => Tangents[0] = value; }
        public Vector3 TangentEnd { get => Tangents[1]; set => Tangents[1] = value; }
        public Vector3 FirstPosition { get => _positions[0]; set => _positions[0] = value; }
        public Vector3 LastPosition { get => _positions[_positions.Count - 1]; set => _positions[_positions.Count - 1] = value; }
        public int Count { get => _positions.Count; }
        public Vector3 this[int index] { get => _positions[index]; set => _positions[index] = value; }

        public RoadPoint(Vector3 position)
        {
            this._positions.Add(position);
            this.Tangents[0] = Vector3.zero;
            this.Tangents[1] = Vector3.zero;
        }

        public RoadPoint(Vector3 position, Vector3 tangentStart)
        {
            this._positions.Add(position);
            this.Tangents[0] = tangentStart;
            this.Tangents[1] = Vector3.zero;
        }

        public RoadPoint(Vector3 position, Vector3 tangentStart, Vector3 tangentEnd)
        {
            this._positions.Add(position);
            this.Tangents[0] = tangentStart;
            this.Tangents[1] = tangentEnd;
        }

        public void AddPosition(Vector3 position) => _positions.Add(position);
        public void RemovePositionAt(int index) => _positions.RemoveAt(index);
        public void RemovePosition(Vector3 position) => _positions.Remove(position);
        public void InsertPosition(int index, Vector3 position) => _positions.Insert(index, position);

        /// <summary>
        /// Remove all the position points without the root position point.
        /// </summary>
        public void Clear() => _positions.RemoveRange(1, _positions.Count - 1);

        /// <summary>
        /// Remove all the position points.
        /// </summary>
        public void ClearAll() => _positions.Clear();
    }
}