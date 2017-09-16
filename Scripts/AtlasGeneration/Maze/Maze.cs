using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using AKSaigyouji.Maps;

namespace AKSaigyouji.AtlasGeneration
{
    public sealed class Maze
    {
        readonly Dictionary<Coord, Cell> cells;

        public int CellCount { get { return cells.Count; } }

        /// <summary>
        /// Define a new maze, with the given locations, links, and cell info.
        /// </summary>
        /// <param name="cellLocations">Coordinates of all the charts in the maze. Must be nonempty and non-null.</param>
        /// <param name="links">Connections to carve between charts. Can be empty but not null.</param>
        /// <param name="cellInfo">Optional tag associated with each cell. Empty and null are valid.</param>
        public Maze(IEnumerable<Coord> cellLocations, IEnumerable<MazeLink> links, IEnumerable<KeyValuePair<Coord, string>> cellInfo = null)
        {
            if (cellLocations == null)
                throw new ArgumentNullException("chartLocations");

            if (cellLocations.Count() == 0)
                throw new ArgumentException("Must have at least one cell to build a maze.");

            if (links == null)
                throw new ArgumentNullException("links");

            var coordToOpenings = new Dictionary<Coord, Cell>(cellLocations.Count());
            foreach (Coord coord in cellLocations)
            {
                coordToOpenings[coord] = new Cell();
            }
            foreach (MazeLink link in links)
            {
                Vector2 diff = link.a - link.b;
                Openings openingA;
                Openings openingB;
                
                if (diff.x == 1)
                {
                    openingA = Openings.Left;
                    openingB = Openings.Right;
                }
                else if (diff.x == -1)
                {
                    openingA = Openings.Right;
                    openingB = Openings.Left;
                }
                else if (diff.y == 1)
                {
                    openingA = Openings.Down;
                    openingB = Openings.Up;
                }
                else
                {
                    openingA = Openings.Up;
                    openingB = Openings.Down;
                }
                coordToOpenings[link.a].CreateOpening(openingA);
                coordToOpenings[link.b].CreateOpening(openingB);
            }
            if (cellInfo != null)
            {
                foreach (var info in cellInfo)
                {
                    coordToOpenings[info.Key].info = info.Value;
                }
            }
            cells = coordToOpenings;
        }

        /// <summary>
        /// Returns coordinates for all the cells in the maze.
        /// </summary>
        public Coord[] GetCells()
        {
            return cells.Keys.ToArray();
        }

        /// <summary>
        /// Was additional information provided for this cell?
        /// </summary>
        public bool HasInfo(Coord cell)
        {
            ThrowIfMissing(cell);
            return cells[cell].info != null;
        }

        /// <summary>
        /// Gets the additional information provided for this cell. It is recommended that HasInfo is first called to check
        /// if this cell has such information.
        /// </summary>
        public string GetInfo(Coord cell)
        {
            ThrowIfMissing(cell);
            return cells[cell].info;
        }

        /// <summary>
        /// Does the cell have an opening to the right?
        /// </summary>
        public bool IsRightOpen(Coord cell)
        {
            return IsOpenAt(cell, Openings.Right);
        }

        /// <summary>
        /// Does the cell have an opening to the left?
        /// </summary>
        public bool IsLeftOpen(Coord cell)
        {
            return IsOpenAt(cell, Openings.Left);
        }

        /// <summary>
        /// Does the cell have an opening to the top?
        /// </summary>
        public bool IsTopOpen(Coord cell)
        {
            return IsOpenAt(cell, Openings.Up);
        }

        /// <summary>
        /// Does the cell have an opening to the bottom?
        /// </summary>
        public bool IsBottomOpen(Coord cell)
        {
            return IsOpenAt(cell, Openings.Down);
        }

        bool IsOpenAt(Coord cell, Openings opening)
        {
            ThrowIfMissing(cell);
            return cells[cell].IsOpenAt(opening);
        }

        void ThrowIfMissing(Coord cell)
        {
            if (!cells.ContainsKey(cell))
                throw new ArgumentException("Maze does not contain this cell.");
        }

        sealed class Cell
        {
            public string info;
            public Openings opening;

            public bool IsOpenAt(Openings opening)
            {
                return (this.opening & opening) != 0;
            }

            public void CreateOpening(Openings opening)
            {
                this.opening |= opening;
            }
        }

        // This is used internally by the maze to encode all 16 configurations of openings in a single byte.
        [Flags]
        enum Openings : Byte
        {
            None = 0,
            Left = 1,
            Up = 2, 
            Right = 4,
            Down = 8
        }
    } 
}