/* This custom editor provides a button to generate atlases from the inspector.*/

using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AKSaigyouji.AtlasGeneration
{
    [CustomEditor(typeof(MazeAtlasGenerator))]
    public sealed class MazeAtlasGeneratorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if (Application.isPlaying && GUILayout.Button("Generate"))
            {
                var generator = (MazeAtlasGenerator)serializedObject.targetObject;
                generator.Seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
                generator.Generate();
            }
        }
    } 
}