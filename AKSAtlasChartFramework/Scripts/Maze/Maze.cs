using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace AKSaigyouji.AtlasGeneration
{
    public sealed class Maze
    {
        public int CellCount { get { return cellLocations.Length; } }
        public int LinkCount { get { return links.Length; } }
        public int TagCount { get { return tags.Length; } }

        readonly Dictionary<Coord, Cell> cells;

        // The data below is encoded in the cells dictionary, but is also preserved
        // separately below for convenience.
        readonly Coord[] cellLocations;
        readonly MazeLink[] links;
        readonly CellTag[] tags;

        /// <summary>
        /// Define a new maze, with the given locations, links.
        /// </summary>
        /// <param name="cellLocations">Coordinates of all the charts in the maze. Must be nonempty and non-null.</param>
        /// <param name="links">Connections to carve between charts. Can be empty but not null.</param>
        public Maze(IEnumerable<Coord> cellLocations, IEnumerable<MazeLink> links) :
            this(cellLocations, links, Enumerable.Empty<CellTag>())
        {

        }

        /// <summary>
        /// Define a new maze, with the given locations, links, and cell info.
        /// </summary>
        /// <param name="cellLocations">Coordinates of all the charts in the maze. Must be nonempty and non-null.</param>
        /// <param name="links">Connections to carve between charts. Can be empty but not null.</param>
        /// <param name="cellInfo">Optional tags associated with selected cells. Can be empty but not null.</param>
        public Maze(IEnumerable<Coord> cellLocations, IEnumerable<MazeLink> links, IEnumerable<KeyValuePair<Coord, MetaData>> cellInfo)
            : this(cellLocations, links, cellInfo.Select(pair => (CellTag)pair)) { }

        /// <summary>
        /// Define a new maze, with the given locations, links, and cell info.
        /// </summary>
        /// <param name="cellLocations">Coordinates of all the charts in the maze. Must be nonempty and non-null.</param>
        /// <param name="links">Connections to carve between charts. Can be empty but not null.</param>
        /// <param name="cellInfo">Optional tags associated with selected cells. Can be empty but not null.</param>
        public Maze(IEnumerable<Coord> cellLocations, IEnumerable<MazeLink> links, IEnumerable<CellTag> cellInfo)
        {
            if (cellLocations == null)
                throw new ArgumentNullException("chartLocations");
            if (links == null)
                throw new ArgumentNullException("links");
            if (cellInfo == null)
                throw new ArgumentNullException("cellInfo");

            this.cellLocations = cellLocations.ToArray();
            this.links = links.ToArray();
            this.tags = cellInfo.ToArray();

            var coordToOpenings = new Dictionary<Coord, Cell>(cellLocations.Count());
            foreach (Coord coord in cellLocations)
            {
                coordToOpenings[coord] = new Cell();
            }
            foreach (MazeLink link in links)
            {
                Coord diff = link.CellA - link.CellB;
                Openings openingA;
                Openings openingB;

                if (diff.x == 1) // Cell A is to the right of Cell B
                {
                    openingA = Openings.Left;
                    openingB = Openings.Right;
                }
                else if (diff.x == -1) // Cell A is to the left of Cell B
                {
                    openingA = Openings.Right;
                    openingB = Openings.Left;
                }
                else if (diff.y == 1) // Cell A is above Cell B
                {
                    openingA = Openings.Down;
                    openingB = Openings.Up;
                }
                else // Cell A is below Cell B
                {
                    openingA = Openings.Up;
                    openingB = Openings.Down;
                }
                coordToOpenings[link.CellA].CreateOpening(openingA);
                coordToOpenings[link.CellB].CreateOpening(openingB);
            }
            foreach (CellTag info in cellInfo)
            {
                if (!coordToOpenings.ContainsKey(info.Cell))
                {
                    throw new ArgumentException(string.Format("CellTag given for cell not in maze: {0}.", info.Cell));
                }
                coordToOpenings[info.Cell].info = info.MetaData;
            }
            cells = coordToOpenings;
        }

        /// <summary>
        /// Create a copy of the cells.
        /// </summary>
        public Coord[] GetCells()
        {
            return cellLocations.ToArray();
        }

        /// <summary>
        /// Create a copy of the links.
        /// </summary>
        public MazeLink[] GetLinks()
        {
            return links.ToArray();
        }

        /// <summary>
        /// Create a copy of the tags.
        /// </summary>
        public CellTag[] GetTags()
        {
            var copy = new CellTag[tags.Length];
            for (int i = 0; i < tags.Length; i++)
            {
                // Metadata is a mutable class, so if we return tags.ToArray(),
                // we'll get copies of the celltags, but they'll refer to the same
                // mutable metadata. 
                copy[i] = tags[i].DeepCopy();
            }
            return copy;
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
        public MetaData GetInfo(Coord cell)
        {
            ThrowIfMissing(cell);
            MetaData info = cells[cell].info;
            if (info == null)
            {
                throw new InvalidOperationException("Cell does not have info. Consider using HasInfo before getting info.");
            }
            return info;
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
            {
                throw new ArgumentException("Maze does not contain this cell.");
            }
        }

        sealed class Cell
        {
            public MetaData info;
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

        // This is used internally by the maze to compactly encode all 16 configurations
        // of a openings for a cell.
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