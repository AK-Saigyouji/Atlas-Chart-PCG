using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using AKSaigyouji.Maps;

namespace AKSaigyouji.AtlasGeneration
{
    /// <summary>
    /// Represents a link/connection/opening between two points in a maze. 
    /// </summary>
    public struct MazeLink
    {
        public readonly Coord a, b;
        
        public MazeLink(Coord a, Coord b)
        {
            if (a.SquaredDistance(b) != 1)
                throw new ArgumentException("Coordinates must be adjacent (horizontally or vertically) in a link.");

            this.a = a;
            this.b = b;
        }
        
    } 
}