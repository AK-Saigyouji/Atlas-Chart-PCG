using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEditor;

namespace AKSaigyouji.AtlasGeneration
{
    [CustomEditor(typeof(StaticMaze), true)]
    public sealed class StaticMazeEditor : MazeGenModuleEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Edit"))
            {
                StaticMaze maze = (StaticMaze)serializedObject.targetObject;
                MazeWindow.OpenWindow(serializedObject, maze.Cells, maze.Links, maze.CellTags, UpdateMaze);
            }
        }

        void UpdateMaze(List<Coord> cells, List<MazeLink> links, List<CellTag> tags)
        {
            var maze = (StaticMaze)serializedObject.targetObject;
            maze.ReplaceContents(cells, links, tags);
            var serializedMaze = new SerializedObject(maze);
            serializedObject.CopyFromSerializedProperty(serializedMaze.FindProperty("cells"));
            serializedObject.CopyFromSerializedProperty(serializedMaze.FindProperty("links"));
            serializedObject.CopyFromSerializedProperty(serializedMaze.FindProperty("cellTags"));
            serializedObject.ApplyModifiedProperties();
        }
    }
}