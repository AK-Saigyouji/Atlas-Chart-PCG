using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using AKSaigyouji.EditorScripting;

namespace AKSaigyouji.AtlasGeneration
{
    [CustomPropertyDrawer(typeof(RawMarker))]
    public sealed class MarkerPropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float heightForNonMetaDataProperties = 4 * EditorHelpers.PROPERTY_HEIGHT_TOTAL;
            return heightForNonMetaDataProperties + EditorGUI.GetPropertyHeight(property.FindPropertyRelative("metaData"));
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.indentLevel++;
            position.height = EditorGUIUtility.singleLineHeight;
            var categoryLabel = new GUIContent("Category", "What type of content this marker represents.");
            Rect categoryRect = EditorGUI.PrefixLabel(position, categoryLabel);
            EditorGUI.PropertyField(categoryRect, property.FindPropertyRelative("category"), GUIContent.none);

            bool guiTurnedOff = GUI.enabled;
            if (guiTurnedOff)
            {
                GUI.enabled = false; // quick and dirty way of making this field readonly.
            }
            position.y += EditorHelpers.PROPERTY_HEIGHT_TOTAL;
            var presetLabel = new GUIContent("Preset", "Preset used to create marker, if applicable.");
            Rect presetRect = EditorGUI.PrefixLabel(position, presetLabel);
            EditorGUI.PropertyField(presetRect, property.FindPropertyRelative("preset"), GUIContent.none);
            if (guiTurnedOff)
            {
                GUI.enabled = true;
            }

            position.y += EditorHelpers.PROPERTY_HEIGHT_TOTAL;
            var positionLabel = new GUIContent("Position", "Position relative to this chart.");
            var positionRect = EditorGUI.PrefixLabel(position, positionLabel);
            EditorGUI.PropertyField(positionRect, property.FindPropertyRelative("position"), GUIContent.none);

            position.y += EditorHelpers.PROPERTY_HEIGHT_TOTAL;
            var sizeLabel = new GUIContent("Size", "Represents the area in which content for this marker can be placed.");
            var sizeRect = EditorGUI.PrefixLabel(position, sizeLabel);
            EditorGUI.PropertyField(sizeRect, property.FindPropertyRelative("size"), GUIContent.none);

            position.y += EditorHelpers.PROPERTY_HEIGHT_TOTAL;
            var metaDataLabel = new GUIContent("MetaData", "(Optional) Additional data about this marker, which will be "+
                                               "made available to the code responsible for consuming the marker.");
            EditorGUI.PropertyField(position, property.FindPropertyRelative("metaData"), metaDataLabel);

            EditorGUI.indentLevel--;
            EditorGUI.EndProperty();
        }
    } 
}