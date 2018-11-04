using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace AKSaigyouji.AtlasGeneration
{
    public sealed class MazeTexture
    {
        public int NumCellsX { get { return numCellsX; } }
        public int NumCellsY { get { return numCellsY; } }
        public Texture2D CellLayer { get { return cellLayer; } }
        public Texture2D TagLayer { get { return tagLayer; } }
        public int Scale { get { return SCALE; } }

        // Used by update to determine if we need to update the corresponding texture.
        bool cellsChanged;
        bool tagsChanged;

        // The highlight rect is where the highlightBlock will be drawn. The block itself is always the size of a cell,
        // but the rect is shaped/sized to fix the region being highlighted (either a cell or passage).
        Rect? highlightRect;

        readonly int numCellsX;
        readonly int numCellsY;
        readonly Texture2D cellLayer;
        readonly Texture2D tagLayer;
        readonly Texture2D highlightBlock;

        // The size of the texture will be numCells * SCALE + 1. The interior of each cell is a SCALE - 1 by SCALE - 1 box,
        // with the first column and row being reserved for the boundary between adjacent cells (hence the -1). 
        // Thus the pixel corresponding to the point (a, b) within cell (x, y) is (x, y) * SCALE + (a, b).
        // values of 0 or 10 will correspond to the boundary of the cell. 
        const int SCALE = 10;

        const int CELL_SIZE = SCALE - 1;
        const int CELL_AREA = CELL_SIZE * CELL_SIZE;

        // This is the length of the bar corresponding to an opening in the cell. We leave a pixel on both sides (hence -2)
        // to make it visually clear that there is an edge between two connected cells, as otherwise they look like a
        // contiguous rectangle. 
        const int BAR_LENGTH = CELL_SIZE - 2;

        public MazeTexture(int numCellsX, int numCellsY)
        {
            this.numCellsX = numCellsX;
            this.numCellsY = numCellsY;
            cellLayer = CreateBaseTexture(numCellsX, numCellsY);
            tagLayer = CreateTransparentTexture(numCellsX, numCellsY);
            highlightBlock = CreateHighlightBlock();
            cellsChanged = true;
            tagsChanged = true;
        }

        public void SetBackgroundColor(Color color)
        {
            var pixels = cellLayer.GetPixels32();
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }
            cellLayer.SetPixels32(pixels);
            cellsChanged = true;
        }

        /// <summary>
        /// Draws the maze onto the screen in the given rect.
        /// </summary>
        public void Draw(Rect rect)
        {
            GUI.DrawTexture(rect, CellLayer);
            GUI.DrawTexture(rect, TagLayer);
            if (highlightRect.HasValue)
            {
                GUI.DrawTexture(TransformRect(rect, highlightRect.Value), highlightBlock);
            }
        }

        public void ColorCell(Coord cell, Color color)
        {
            int xOffset = cell.x * SCALE;
            int yOffset = cell.y * SCALE;
            Color[] colors = Enumerable.Range(0, CELL_AREA).Select(i => color).ToArray();
            cellLayer.SetPixels(xOffset + 1, yOffset + 1, CELL_SIZE, CELL_SIZE, colors);
            cellsChanged = true;
        }

        public void ColorLink(MazeLink link, Color color)
        {
            if (link.IsHorizontal)
            {
                Coord cell = new Coord(Math.Max(link.CellA.x, link.CellB.x), link.CellB.y);
                ColorVerticalBar(cell, color);
            }
            else
            {
                Coord cell = new Coord(link.CellA.x, Math.Max(link.CellA.y, link.CellB.y));
                ColorHorizontalBar(cell, color);
            }
            cellsChanged = true;
        }

        /// <summary>
        /// Draw a custom image onto the given cell in the tag layer. The coords are relative to the cell, and should
        /// respect the boundary of the cell. i.e. they should be within 1 and 9 inclusive. These correspond to the 
        /// interior pixels of the cell. Values of 0 or 10 are valid, but correspond to the (potentially shared) 
        /// boundary pixels, which may obscure openings or overwrite the custom boundary pixels of a shared cell.
        /// </summary>
        public void ColorTag(Coord cell, IEnumerable<Coord> image, Color color)
        {
            Coord start = SCALE * cell;
            foreach (Coord target in image.Select(offset => start + offset))
            {
                tagLayer.SetPixel(target.x, target.y, color);
            }
            tagsChanged = true;
        }

        public void ClearTags(Coord cell)
        {
            Coord start = SCALE * cell;
            for (int y = 1; y < 10; y++)
            {
                for (int x = 1; x < 10; x++)
                {
                    tagLayer.SetPixel(start.x + x, start.y + y, Color.clear);
                }
            }
            tagsChanged = true;
        }

        /// <summary>
        /// Activate a highlight over this region, given this cell location.
        /// </summary>
        /// <param name="location">Coordinates of the highlighted region.</param>
        /// <param name="region">The type of region being highlighted.</param>
        public void SetHighlight(Coord location, HighlightRegion region)
        {
            location.y = (short)(numCellsY - location.y - (region == HighlightRegion.Horizontal ? 0 : 1));
            Coord offset = SCALE * location;
            if (region == HighlightRegion.Horizontal)
            {
                offset.x += 2;
                highlightRect = new Rect(offset, new Vector2(BAR_LENGTH, 1));
            }
            else if (region == HighlightRegion.Vertical)
            {
                offset.y += 2;
                highlightRect = new Rect(offset, new Vector2(1, BAR_LENGTH));
            }
            else if (region == HighlightRegion.Square) 
            {
                offset += Coord.one;
                highlightRect = new Rect(offset, CELL_SIZE * Vector2.one);
            }
            else
            {
                Debug.LogErrorFormat("Attempted to set highlight at location {0} with region set to None", location);
            }
        }

        /// <summary>
        /// Remove the highlight, if there is one. Will do nothing if nothing is highlighted.
        /// </summary>
        public void RemoveHighlight()
        {
            highlightRect = null;
        }

        /// <summary>
        /// Apply any changes to the texture. Analogous to the Apply method on Unity texture objects.
        /// </summary>
        public void Apply()
        {
            if (tagsChanged)
            {
                tagLayer.Apply(false);
                tagsChanged = false;
            }
            if (cellsChanged)
            {
                cellLayer.Apply(false);
                cellsChanged = false;
            }
        }

        /// <summary>
        /// Transform the original rect to fit the containing Rect.
        /// </summary>
        Rect TransformRect(Rect containing, Rect original)
        {
            var scale = new Vector2(containing.width / cellLayer.width, containing.height / cellLayer.height);
            original.center += containing.min;
            original.x *= scale.x;
            original.y *= scale.y;
            original.width *= scale.x;
            original.height *= scale.y;
            return original;
        }

        void ColorHorizontalBar(Coord cell, Color color)
        {
            int xOffset = cell.x * SCALE;
            int yOffset = cell.y * SCALE;
            Color[] colors = Enumerable.Range(0, BAR_LENGTH).Select(i => color).ToArray();
            cellLayer.SetPixels(xOffset + 2, yOffset, BAR_LENGTH, 1, colors);
            cellsChanged = true;
        }

        void ColorVerticalBar(Coord cell, Color color)
        {
            int xOffset = cell.x * SCALE;
            int yOffset = cell.y * SCALE;
            Color[] colors = Enumerable.Range(0, BAR_LENGTH).Select(i => color).ToArray();
            cellLayer.SetPixels(xOffset, yOffset + 2, 1, BAR_LENGTH, colors);
            cellsChanged = true;
        }

        static Texture2D CreateBaseTexture(int numCellsX, int numCellsY)
        {
            // The extra 1 to both texture dimensions is to get a black line around the border.
            var texture = new Texture2D(SCALE * numCellsX + 1, SCALE * numCellsY + 1, TextureFormat.ARGB32, mipChain: false);
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            return texture;
        }

        static Texture2D CreateTransparentTexture(int numCellsX, int numCellsY)
        {
            var texture = CreateBaseTexture(numCellsX, numCellsY);
            var pixels = texture.GetPixels32();
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i].a = 1;
            }
            texture.SetPixels32(pixels);
            return texture;
        }

        static Texture2D CreateHighlightBlock()
        {
            var texture = new Texture2D(SCALE, SCALE, TextureFormat.ARGB32, mipChain: false);
            var pixels = texture.GetPixels32();
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = new Color32(255, 255, 255, 200);
            }
            texture.SetPixels32(pixels);
            return texture;
        }
    } 
}