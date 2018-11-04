using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace AKSaigyouji.AtlasGeneration
{
    [CustomEditor(typeof(TagDrawTable), editorForChildClasses: true)]
    public sealed class TagDrawTableEditor : Editor
    {
        const string COLOR_PROPERTY = "color";
        const string COORDS_PROPERTY = "coords";
        const string DEFAULT_DRAWER_PROPERTY = "defaultDrawer";
        const string OTHER_DRAWERS_PROPERTY = "drawers";
        const string DICT_VALUES_PROPERTY = "values";

        // cache the texture to avoid excessive allocations
        Texture2D texture;

        const int SIZE = 11;

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
            var defaultDrawer = serializedObject.FindProperty(DEFAULT_DRAWER_PROPERTY);
            var otherDrawers = serializedObject.FindProperty(OTHER_DRAWERS_PROPERTY).FindPropertyRelative(DICT_VALUES_PROPERTY);
            if (defaultDrawer == null && otherDrawers == null)
            {
                return;
            }
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
            DrawTagDrawer(defaultDrawer, pixels);
            if (otherDrawers != null)
            {
                for (int i = 0; i < otherDrawers.arraySize; i++)
                {
                    DrawTagDrawer(otherDrawers.GetArrayElementAtIndex(i), pixels);
                }
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

        static void DrawTagDrawer(SerializedProperty drawer, Color32[] pixels)
        {
            if (drawer == null || drawer.objectReferenceValue == null)
                return;
            // FindPropertyRelative does not behave nicely when the property is a UnityEngine.Object, hence
            // the workaround to use FindProperty instead.
            var drawerObject = new SerializedObject(drawer.objectReferenceValue);
            var coords = drawerObject.FindProperty(COORDS_PROPERTY);
            var color = drawerObject.FindProperty(COLOR_PROPERTY).colorValue;
            for (int i = 0; i < coords.arraySize; i++)
            {
                var serializedCoord = coords.GetArrayElementAtIndex(i);
                int x = serializedCoord.FindPropertyRelative("x").intValue;
                int y = serializedCoord.FindPropertyRelative("y").intValue;
                pixels[y * SIZE + x] = color;
            }
        }
    }
}