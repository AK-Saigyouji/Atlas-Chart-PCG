using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace AKSaigyouji
{
    [CustomPropertyDrawer(typeof(Coord))]
    public sealed class CoordPropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            Rect coordRect = EditorGUI.PrefixLabel(position, label);
            Rect left = coordRect;
            left.width /= 2;
            left.width -= 2;
            Rect right = left;
            right.x += left.width + 2;
            EditorGUI.indentLevel = 0;
            DrawLabelledInt(left, property.FindPropertyRelative("x"));
            DrawLabelledInt(right, property.FindPropertyRelative("y"));
            EditorGUI.EndProperty();
        }

        void DrawLabelledInt(Rect position, SerializedProperty property)
        {
            const int LABEL_WIDTH = 13;
            Rect label = position;
            label.width = LABEL_WIDTH;
            Rect value = position;
            value.width -= LABEL_WIDTH;
            value.x += LABEL_WIDTH;
            EditorGUI.LabelField(label, property.displayName);
            property.intValue = EditorGUI.IntField(value, property.intValue);
        }
    } 
}