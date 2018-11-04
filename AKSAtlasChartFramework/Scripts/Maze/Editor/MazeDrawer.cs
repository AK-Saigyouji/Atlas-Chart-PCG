using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace AKSaigyouji.AtlasGeneration
{
    /// <summary>
    /// Responsible for drawing maze textures in the editor.
    /// </summary>
    public sealed class MazeDrawer
    {
        // White and black are harsh on the eyes, so shades of grey are used instead, with a 
        // desaturated red for tags
        public static readonly Color32 CELL_COLOR = new Color(0.85f, 0.85f, 0.85f); // light grey
        public static readonly Color32 BACKGROUND_COLOR = new Color(0.3f, 0.3f, 0.3f); // dark grey

        const int SCALE = 10;

        public MazeTexture CreateTexture(Maze maze, TagDrawTable tagTable)
        {
            if (maze == null)
                throw new ArgumentNullException("maze");

            Coord numCells = CalculateCells(maze.GetCells());
            var mazeTexture = new MazeTexture(numCells.x, numCells.y);
            mazeTexture.SetBackgroundColor(BACKGROUND_COLOR);
            Coord[] cells = maze.GetCells();
            Coord bottomLeft = GetBottomLeft(cells);
            foreach (Coord actualCell in cells)
            {
                mazeTexture.ColorCell(actualCell - bottomLeft, CELL_COLOR);
            }
            foreach (MazeLink link in maze.GetLinks())
            {
                var shiftedLink = new MazeLink(link.CellA - bottomLeft, link.CellB - bottomLeft);
                mazeTexture.ColorLink(shiftedLink, CELL_COLOR);
            }
            if (tagTable != null)
            {
                foreach (CellTag tag in maze.GetTags())
                {
                    Coord shiftedCell = tag.Cell - bottomLeft;
                    MetaData metaData = tag.MetaData;
                    foreach (var pair in metaData)
                    {
                        TagDrawer drawer = tagTable[pair.Key];
                        mazeTexture.ColorTag(shiftedCell, drawer.Coords, drawer.Color);
                    }
                }
            }
            mazeTexture.Apply();
            return mazeTexture;
        }

        static Coord GetBottomLeft(Coord[] coords)
        {
            int minX = coords.DefaultIfEmpty().Min(c => c.x);
            int minY = coords.DefaultIfEmpty().Min(c => c.y);
            return new Coord(minX, minY);
        }

        static Coord GetTopRight(Coord[] coords)
        {
            int maxX = coords.DefaultIfEmpty().Max(c => c.x);
            int maxY = coords.DefaultIfEmpty().Max(c => c.y);
            return new Coord(maxX, maxY);
        }

        static Coord CalculateCells(Coord[] coords)
        {
            // If bottom left is (1,3) and top right is (4, 5), then we have 4 cells by 3 cells in
            // the maze.
            return Coord.one + GetTopRight(coords) - GetBottomLeft(coords);
        }
    }
}