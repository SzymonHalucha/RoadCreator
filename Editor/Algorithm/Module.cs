using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SH.RoadCreator.Algorithm
{
    /// <summary>
    /// This class is used to store information about the segment module used to create segment profiles.
    /// </summary>
    [Serializable]
    public class Module
    {
        public string Name = "New Module";
        public float Width = 1f;
        public float Height = 1f;
        public Color BaseColor = Color.white;

        public Module()
        {

        }

        public Module(Module orginal)
        {
            this.Name = orginal.Name;
            this.Width = orginal.Width;
            this.Height = orginal.Height;
            this.BaseColor = orginal.BaseColor;
        }
    }
}