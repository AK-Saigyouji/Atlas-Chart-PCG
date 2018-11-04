using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEditor;
using AKSaigyouji.AtlasGeneration;

namespace AKSaigyouji.AtlasGeneration
{
    [CustomEditor(typeof(MarkerMapper), editorForChildClasses: true)]
    public sealed class MarkerMapperEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var paletteVariable = serializedObject.FindProperty("_palette");
            MarkerPalette palette = (MarkerPalette)EditorGUILayout.ObjectField("Palette", 
                paletteVariable.objectReferenceValue, typeof(MarkerPalette), allowSceneObjects: false);
            paletteVariable.objectReferenceValue = palette;
            bool isPaletteSelected = palette != null;

            string[] presetNames = isPaletteSelected ? palette.GetPresets().Select(p => p.Name).ToArray() : null;

            EditorGUILayout.PropertyField(serializedObject.FindProperty("parent"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("clampPositionsToGrid"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("twoDimensional"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("chartsToSkip"), includeChildren: true);
            if (EditorGUILayout.PropertyField(serializedObject.FindProperty("keys"), new GUIContent("Spawners")))
            {
                EditorGUI.indentLevel++;
                var keys = serializedObject.FindProperty("keys");
                var spawners = serializedObject.FindProperty("values");
                if (GUILayout.Button("Add Spawner", GUILayout.Width(150)))
                {
                    keys.InsertArrayElementAtIndex(keys.arraySize);
                    spawners.InsertArrayElementAtIndex(spawners.arraySize);
                }
                spawners.arraySize = keys.arraySize;
                for (int i = 0; i < keys.arraySize; i++)
                {
                    SerializedProperty key = keys.GetArrayElementAtIndex(i);
                    SerializedProperty spawner = spawners.GetArrayElementAtIndex(i);
                    EditorGUILayout.BeginHorizontal();
                    spawner.isExpanded = EditorGUILayout.Foldout(spawner.isExpanded, key.stringValue, true);
                    if (GUILayout.Button("Remove " + key.stringValue, EditorStyles.miniButton, GUILayout.MaxWidth(170)))
                    {
                        keys.DeleteArrayElementAtIndex(i);
                        spawners.DeleteArrayElementAtIndex(i);
                    }
                    else
                    {
                        EditorGUILayout.EndHorizontal();
                        if (spawner.isExpanded)
                        {
                            EditorGUI.indentLevel++;
                            if (isPaletteSelected) // if palette is selected, provide dropdown from palette presets
                            {
                                int selected = EditorGUILayout.Popup("Preset", Array.IndexOf(presetNames, key.stringValue), presetNames);
                                key.stringValue = presetNames[selected];
                            }
                            else // if no palette is selected, ask for an arbitrary string
                            {
                                EditorGUILayout.PropertyField(key, new GUIContent("Preset"));
                            }
                            EditorGUILayout.PropertyField(spawner, new GUIContent("Prefabs"), includeChildren: true);
                            EditorGUI.indentLevel--;
                        }
                    }
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}