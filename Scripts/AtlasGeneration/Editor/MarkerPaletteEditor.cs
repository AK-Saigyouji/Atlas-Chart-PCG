/* The custom inspector for the marker palette features a suite of options for creating color-coded categories and
 * marker presets. The marker presets automatically take on the color of their associated category, the options
 * for the category take on the facade of an enum despite no such enum existing, and better support for creating, 
 * deleting and duplicating presets is needed, as arrays are clumsy to work with in the inspector. */

using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using AKSaigyouji.EditorScripting;

namespace AKSaigyouji.AtlasGeneration
{
    [CustomEditor(typeof(MarkerPalette))]
    public sealed class MarkerPaletteEditor : Editor
    {
        public bool canEdit = true;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            if (!canEdit)
            {
                GUI.enabled = false;
            }

            var presetsProp = serializedObject.FindProperty("presets");
            var defaultProp = serializedObject.FindProperty("defaultColor");
            var categoriesProp = serializedObject.FindProperty("categories");
            int numCategories = categoriesProp.arraySize;
            EditorGUILayout.PropertyField(defaultProp);
            categoriesProp.isExpanded = EditorGUILayout.Foldout(categoriesProp.isExpanded, "Categories");
            if (categoriesProp.isExpanded)
            {
                EditorGUI.indentLevel++;
                for (int i = 0; i < numCategories; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    var categoryProp = categoriesProp.GetArrayElementAtIndex(i);
                    var nameProp = categoryProp.FindPropertyRelative("name");
                    var colorProp = categoryProp.FindPropertyRelative("color");

                    EditorGUILayout.PropertyField(nameProp, GUIContent.none);
                    int indentLevel = EditorGUI.indentLevel;
                    EditorGUI.indentLevel = 0;
                    EditorGUILayout.PropertyField(colorProp, GUIContent.none, GUILayout.MaxWidth(42));
                    EditorGUI.indentLevel = indentLevel;
                    if (canEdit)
                    {
                        if (GUILayout.Button("X", GUILayout.MaxWidth(25)))
                        {
                            ((MarkerPalette)serializedObject.targetObject).DeleteCategoryAtIndex(i);
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
                Rect buttonRect = EditorGUILayout.GetControlRect();
                buttonRect.x += EditorHelpers.HORIZONTAL_INDENT * EditorGUI.indentLevel;
                buttonRect.width = 150;
                if (GUI.Button(buttonRect, "Add Category"))
                {
                    ((MarkerPalette)serializedObject.targetObject).AddNewCategory();
                }
                EditorGUI.indentLevel--;
            }
            // don't draw anything preset-related if the palette is currently readonly and there are no presets to see
            if (canEdit || presetsProp.arraySize > 0)
            {
                presetsProp.isExpanded = EditorGUILayout.Foldout(presetsProp.isExpanded, "Marker Presets");
                if (presetsProp.isExpanded)
                {
                    EditorGUI.indentLevel++;
                    for (int i = 0; i < presetsProp.arraySize; i++)
                    {
                        var preset = presetsProp.GetArrayElementAtIndex(i);
                        PresetField(preset, categoriesProp, numCategories, i);
                    }
                    if (canEdit)
                    {
                        Rect buttonRect = EditorGUILayout.GetControlRect();
                        buttonRect.x += EditorHelpers.HORIZONTAL_INDENT * EditorGUI.indentLevel;
                        buttonRect.width = 150;
                        if (GUI.Button(buttonRect, "Add Preset"))
                        {
                            ((MarkerPalette)serializedObject.targetObject).AddNewPreset();
                        }
                    }
                    EditorGUI.indentLevel--;
                }
            }
            if (!canEdit)
            {
                GUI.enabled = true;
            }
            serializedObject.ApplyModifiedProperties();
        }

        void PresetField(SerializedProperty presetProp, SerializedProperty categoriesProp, int numCategories, int index)
        {
            var nameProp = presetProp.FindPropertyRelative("name");
            var sizeProp = presetProp.FindPropertyRelative("size");
            var categoryProp = presetProp.FindPropertyRelative("category");
            var metaDataProp = presetProp.FindPropertyRelative("metaData");
            var palette = ((MarkerPalette)serializedObject.targetObject);
            var fieldColor = palette.GetColor(categoryProp.stringValue);

            string name = string.Format("({0})", nameProp.stringValue);
            string label = string.Format("Preset {0}", index);
            EditorGUILayout.BeginHorizontal();
            presetProp.isExpanded = EditorGUILayout.Foldout(presetProp.isExpanded, label);
            GUIStyle labelStyle = new GUIStyle(EditorStyles.boldLabel);
            labelStyle.normal.textColor = fieldColor;
            if (!canEdit)
            {
                // This is an ad-hoc solution to an overlapping issue occuring in the chart builder window.
                GUILayout.Space(15);
            }
            EditorGUILayout.LabelField(name, labelStyle, GUILayout.ExpandWidth(false), GUILayout.MinWidth(50));
            if (canEdit && GUILayout.Button("Copy", GUILayout.MaxWidth(100)))
            {
                palette.DuplicatePresetAtIndex(index);
            }
            if (canEdit && GUILayout.Button("X", GUILayout.MaxWidth(25)))
            {
                palette.DeletePresetAtIndex(index);
            }
            EditorGUILayout.EndHorizontal();
            if (presetProp.isExpanded)
            {
                EditorGUI.indentLevel++;
                var nameLabel = new GUIContent("Name", "The name of this preset as it will appear in the editor window when creating a new marker.");
                EditorGUILayout.PropertyField(nameProp, nameLabel);

                CategoryField(categoryProp, categoriesProp, numCategories);

                var sizeLabel = new GUIContent("Size", "The default size for this marker. Can be individually adjusted afterwards.");
                EditorGUILayout.PropertyField(sizeProp, sizeLabel);

                var metaDataLabel = new GUIContent("MetaData", "The default metadata for this marker. Useful for assigning the keys for each entity in this category.");
                EditorGUILayout.PropertyField(metaDataProp, metaDataLabel);
                EditorGUI.indentLevel--;
            }
        }

        void CategoryField(SerializedProperty category, SerializedProperty categoriesProp, int numCategories)
        {
            string[] categories = new string[numCategories + 1];
            string currentCategory = category.stringValue;
            categories[numCategories] = "None";
            int indexOfCategory = numCategories;
            for (int i = 0; i < numCategories; i++)
            {
                categories[i] = categoriesProp.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue;
                if (categories[i] == currentCategory)
                {
                    indexOfCategory = i;
                }
            }
            var label = new GUIContent("Category", "Which category this preset belongs to, as defined in the marker palette.");
            int newIndex = EditorGUILayout.Popup(label, indexOfCategory, categories.Select(cat => new GUIContent(cat)).ToArray());
            category.stringValue = categories[newIndex];
        }
    } 
}