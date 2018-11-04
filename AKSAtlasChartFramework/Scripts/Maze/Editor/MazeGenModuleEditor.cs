using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using AKSaigyouji.Maps;

namespace AKSaigyouji.AtlasGeneration
{
    [CustomEditor(typeof(MazeGenModule), editorForChildClasses: true)]
    public class MazeGenModuleEditor : Editor
    {
        bool visualize = true;

        readonly MazeDrawer mazeDrawer = new MazeDrawer();
        AtlasEditorState editorState;

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
                var mazeGen = (MazeGenModule)target;
                Maze maze;
                try
                {
                    maze = mazeGen.GetMaze();
                }
                catch (InvalidOperationException)
                {
                    // Maze generator is not properly configured yet: suppress error to avoid spamming the console,
                    // and exit the draw method.
                    return;
                }
                if (editorState == null)
                {
                    editorState = AtlasEditorState.Load();
                }
                MazeTexture mazeTexture = mazeDrawer.CreateTexture(maze, editorState.TagTable);

                int numCellsX = mazeTexture.NumCellsX;
                int numCellsY = mazeTexture.NumCellsY;

                float originalCellWidth = previewArea.width / numCellsX;
                float originalCellHeight = previewArea.height / numCellsY;

                // we want to make cells have equal height and width
                float cellSize = Math.Min(originalCellWidth, originalCellHeight);

                float newWidth = numCellsX * cellSize;
                float newHeight = numCellsY * cellSize;

                float xOffset = (previewArea.width - newWidth) / 2;
                float yOffset = (previewArea.height - newHeight) / 2;

                previewArea.width = newWidth;
                previewArea.height = newHeight;
                previewArea.x += xOffset;
                previewArea.y += yOffset;

                mazeTexture.Draw(previewArea);
            }
        }
    } 
}