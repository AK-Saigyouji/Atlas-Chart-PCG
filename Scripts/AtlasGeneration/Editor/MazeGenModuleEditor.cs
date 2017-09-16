using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using AKSaigyouji.Maps;

namespace AKSaigyouji.AtlasGeneration
{
    [CustomEditor(typeof(MazeGenModule), editorForChildClasses: true)]
    public sealed class MazeGenModuleEditor : Editor
    {
        bool visualize = true;

        // White and black are harsh on the eyes, so shades of grey are used instead, with a 
        // desaturated red for the entrance and exit in the maze (if they're flagged in the maze).
        readonly Color32 cellColor = new Color(0.85f, 0.85f, 0.85f); // dark grey
        readonly Color32 backgroundColor = new Color(0.3f, 0.3f, 0.3f); // light grey
        readonly Color32 specialColor = new Color(177 / 255f, 129 / 255f, 129 / 255f); // brandy rose

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }

        public override bool HasPreviewGUI()
        {
            return true;
        }

        public override void OnPreviewSettings()
        {
            if (GUILayout.Button("Reroll"))
            {
                ((MazeGenModule)target).Seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            }
            EditorGUIUtility.labelWidth = 100;
            visualize = EditorGUILayout.Toggle("Show Preview", visualize);
            EditorGUIUtility.labelWidth = 0;
        }

        public override void DrawPreview(Rect previewArea)
        {
            if (visualize && Event.current.type == EventType.Repaint)
            {
                Maze maze = ((MazeGenModule)target).GetMaze();
                Coord[] cells = maze.GetCells();
                int minX = cells.Min(c => c.x);
                int minY = cells.Min(c => c.y);
                int maxX = cells.Max(c => c.x);
                int maxY = cells.Max(c => c.y);
                Coord min = new Coord(minX, minY);

                // Add 1 to the number of cells, because the max is included (e.g. 0 to 5 is a range of 6 cells).
                int numCellsY = 1 + maxY - minY;
                int numCellsX = 1 + maxX - minX;
                int scale = 10;
                // The extra 1 to both texture dimensions is to get a black line around the border.
                Texture2D texture = new Texture2D(scale * numCellsX + 1, scale * numCellsY + 1, TextureFormat.ARGB32, mipmap: false);
                texture.filterMode = FilterMode.Point;
                texture.wrapMode = TextureWrapMode.Clamp;
                Color32[] pixels = new Color32[texture.width * texture.height];
                for (int i = 0; i < pixels.Length; i++)
                {
                    pixels[i] = backgroundColor;
                }
                foreach (Coord actualCell in cells)
                {
                    Coord cell = actualCell - min;
                    Coord pixelStart = scale * cell + Coord.one;
                    Coord pixelEnd = scale * (cell + Coord.one);
                    for (int y = pixelStart.y; y < pixelEnd.y; y++)
                    {
                        for (int x = pixelStart.x; x < pixelEnd.x; x++)
                        {
                            pixels[y * texture.width + x] = cellColor;
                        }
                    }
                    if (maze.IsBottomOpen(actualCell))
                    {
                        for (int x = pixelStart.x + 1; x < pixelEnd.x - 1; x++)
                        {
                            pixels[pixelStart.y * texture.width + x] = cellColor;
                        }
                    }
                    if (maze.IsTopOpen(actualCell))
                    {
                        for (int x = pixelStart.x + 1; x < pixelEnd.x - 1; x++)
                        {
                            pixels[pixelEnd.y * texture.width + x] = cellColor;
                        }
                    }
                    if (maze.IsLeftOpen(actualCell))
                    {
                        for (int y = pixelStart.y + 1; y < pixelEnd.y - 1; y++)
                        {
                            pixels[y * texture.width + pixelStart.x] = cellColor;
                        }
                    }
                    if (maze.IsRightOpen(actualCell))
                    {
                        for (int y = pixelStart.y + 1; y < pixelEnd.y - 1; y++)
                        {
                            pixels[y * texture.width + pixelEnd.x] = cellColor;
                        }
                    }
                    if (maze.HasInfo(actualCell))
                    {
                        if (maze.GetInfo(actualCell) == "start")
                        {
                            foreach (Coord coord in GetCoordsToDrawZero(pixelStart))
                            {
                                pixels[coord.y * texture.width + coord.x] = specialColor;
                            }
                        }
                        else if (maze.GetInfo(actualCell) == "end")
                        {
                            foreach (Coord coord in GetCoordsToDrawOne(pixelStart))
                            {
                                pixels[coord.y * texture.width + coord.x] = specialColor;
                            }
                        }
                    }
                }
                
                texture.SetPixels32(pixels);
                texture.Apply();

                float originalCellWidth = previewArea.width / numCellsX;
                float originalCellHeight = previewArea.height / numCellsY;

                // we want to make cells have equal height and width, and also need to ensure they stay in the preview box.
                float cellSize = Math.Min(originalCellWidth, originalCellHeight);

                float newWidth = numCellsX * cellSize;
                float newHeight = numCellsY * cellSize;

                float xOffset = (previewArea.width - newWidth) / 2;
                float yOffset = (previewArea.height - newHeight) / 2;

                previewArea.width = newWidth;
                previewArea.height = newHeight;
                previewArea.x += xOffset;
                previewArea.y += yOffset;

                GUI.DrawTexture(previewArea, texture);
            }
        }

        static IEnumerable<Coord> GetCoordsToDrawZero(Coord coord)
        {
            Coord offset = coord + new Coord(4, 1);
            return new[]
            {
                new Coord(-1, 0), new Coord(0, 0), new Coord(1, 0), // bot
                new Coord(-1, 5), new Coord(0, 5), new Coord(1, 5), // top
                new Coord(-2, 1), new Coord(-2, 2), new Coord(-2, 3), new Coord(-2, 4), // left
                new Coord(2, 1), new Coord(2, 2), new Coord(2, 3), new Coord(2, 4), // right
            }.Select(c => c + offset);
        }

        static IEnumerable<Coord> GetCoordsToDrawOne(Coord coord)
        {
            Coord offset = coord + new Coord(4, 1);
            return new[]
            {
                new Coord(-1, 0), new Coord(0, 0), new Coord(1, 0), // base
                new Coord(0, 1), new Coord(0, 2), new Coord(0, 3), new Coord(0, 4), new Coord(0, 5), // middle
                new Coord(-1, 4) // hook
            }.Select(c => c + offset);
        }
    } 
}