using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace AKSaigyouji.AtlasGeneration
{
    /// <summary>
    /// Can be used in conjunction with the MazeTexture to draw images onto a maze
    /// texture's cells. See docstrings for each method to see where the image is
    /// drawn within the cell to avoid overlapping.
    /// </summary>
    public sealed class MazeDrawPrimitives
    {
        /// <summary>
        /// Draws a zero in the top left corner of a cell.
        /// </summary>
        public static IEnumerable<Coord> Zero()
        {
            Coord offset = new Coord(2, 6);
            return new[]
            {
                new Coord(0, 0), new Coord(1, 0), // bot
                new Coord(0, 3), new Coord(1, 3), // top
                new Coord(-1, 1), new Coord(-1, 2), // left
                new Coord(2, 1), new Coord(2, 2), // right
            }.Select(c => c + offset);
        }

        /// <summary>
        /// Draws a one in the bottom left corner of a cell.
        /// </summary>
        public static IEnumerable<Coord> One()
        {
            Coord offset = new Coord(3, 1);
            return new[]
            {
                new Coord(-1, 0), new Coord(0, 0), new Coord(1, 0), // base
                new Coord(0, 1), new Coord(0, 2), new Coord(0, 3), // middle
                new Coord(-1, 2) // hook
            }.Select(c => c + offset);
        }

        /// <summary>
        /// Draws an exclamation point on the right side of a cell.
        /// </summary>
        public static IEnumerable<Coord> Exclamation()
        {
            Coord offset = new Coord(7, 8);
            return new[]
            {
                new Coord(0, 0), new Coord(0, -1), new Coord(0, -2), new Coord(0, -3), new Coord(0, -4), new Coord(0, -6)
            }.Select(c => c + offset);
        }
    } 
}