using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace AKSaigyouji.AtlasGeneration
{
    [CustomEditor(typeof(TagDrawer), editorForChildClasses:true)]
    public sealed class TagDrawerEditor : Editor
    {
        const string COLOR_PROPERTY = "color";
        const string COORDS_PROPERTY = "coords";

        // cache the texture to avoid excessive allocations
        Texture2D texture;

        const int SIZE = 11;

        int xAdd = 0;
        int yAdd = 0;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty(COLOR_PROPERTY));
            EditorGUILayout.Space();
            var coords = serializedObject.FindProperty(COORDS_PROPERTY);
            int deleteIndex = -1;
            for (int i = 0; i < coords.arraySize; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(coords.GetArrayElementAtIndex(i));
                if (GUILayout.Button("X", GUILayout.MaxWidth(40)))
                {
                    deleteIndex = i;
                }
                EditorGUILayout.EndHorizontal();
            }
            if (deleteIndex != -1)
            {
                coords.DeleteArrayElementAtIndex(deleteIndex);
            }
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("New Element");
            xAdd = EditorGUILayout.IntField(xAdd);
            yAdd = EditorGUILayout.IntField(yAdd);
            if (GUILayout.Button("Add"))
            {
                int index = coords.arraySize;
                coords.InsertArrayElementAtIndex(index);
                var added = coords.GetArrayElementAtIndex(index);
                added.FindPropertyRelative("x").intValue = xAdd;
                added.FindPropertyRelative("y").intValue = yAdd;
            }
            EditorGUILayout.EndHorizontal();
            serializedObject.ApplyModifiedProperties();
        }

        public override bool HasPreviewGUI()
        {
            return true;
        }

        public override void OnPreviewGUI(Rect previewArea, GUIStyle gUIStyle)
        {
            if (Event.current.type != EventType.Repaint) // reduce the number of redundant preview draws.
            {
                return;
            }
            var coords = serializedObject.FindProperty("coords");
            if (coords.arraySize == 0)
            {
                return;
            }
            Color pixelColor = serializedObject.FindProperty(COLOR_PROPERTY).colorValue;
            if (texture == null)
            {
                texture = new Texture2D(SIZE, SIZE, TextureFormat.RGB24, mipChain: false);
                texture.filterMode = FilterMode.Point;
                texture.wrapMode = TextureWrapMode.Clamp;
            }
            var pixels = texture.GetPixels32();

            for (int y = 0; y < SIZE; y++)
            {
                for (int x = 0; x < SIZE; x++)
                {
                    pixels[y * SIZE + x] = Color.white;
                }
            }
            for (int i = 0; i < coords.arraySize; i++)
            {
                var serializedCoord = coords.GetArrayElementAtIndex(i);
                int x = serializedCoord.FindPropertyRelative("x").intValue;
                int y = serializedCoord.FindPropertyRelative("y").intValue;
                pixels[y * SIZE + x] = pixelColor;
            }
            if (0 < xAdd && xAdd < SIZE - 1 && 0 < yAdd && yAdd < SIZE - 1)
            {
                Color32 oldColor = pixels[yAdd * SIZE + xAdd];
                oldColor.r /= 2;
                oldColor.g /= 2;
                oldColor.b /= 2;
                pixels[yAdd * SIZE + xAdd] = oldColor;
            }
            texture.SetPixels32(pixels);
            texture.Apply();

            // By default, the image will be stretched to fit the preview area, which will distort
            // the squares. We instead drawn onto the biggest centered square that will fit
            // into the preview area, so that the cells are squares.
            float previewSize = Mathf.Min(previewArea.width, previewArea.height);
            // Center the previewArea
            previewArea.x += (previewArea.width - previewSize) / 2;
            previewArea.y += (previewArea.height - previewSize) / 2;
            // Shrink the bigger dimension to be the same as the smaller dimension.
            previewArea.width = previewSize;
            previewArea.height = previewSize;
            GUI.DrawTexture(previewArea, texture);
        }
    } 
}