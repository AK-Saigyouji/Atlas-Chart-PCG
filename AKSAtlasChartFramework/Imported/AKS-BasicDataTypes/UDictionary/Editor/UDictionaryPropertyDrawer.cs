using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEditor;

namespace AKSaigyouji.Dictionary
{
    [CustomPropertyDrawer(typeof(UDictionary), true)]
    public sealed class UDictionaryPropertyDrawer : PropertyDrawer
    {
        const string KEYS = "keys";
        const string VALUES = "values";

        const int BUTTON_WIDTH = 25;
        const int BUTTON_SPACING = 5;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight;
            if (property.isExpanded)
            {
                int numElements = property.FindPropertyRelative(KEYS).arraySize;
                height += (numElements + 1) * EditorGUIUtility.singleLineHeight;
            }
            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            position.height = EditorGUIUtility.singleLineHeight;
            if (property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label))
            {
                int oldIndentLevel = EditorGUI.indentLevel;
                position.y += EditorGUIUtility.singleLineHeight;
                EditorGUI.indentLevel++;
                var keys = property.FindPropertyRelative(KEYS);
                var values = property.FindPropertyRelative(VALUES);

                Rect sizeRect = EditorGUI.PrefixLabel(position, new GUIContent("Size"));
                int newSize = EditorGUI.IntField(sizeRect, keys.arraySize);
                keys.arraySize = newSize;
                values.arraySize = newSize;
                position.y += EditorGUIUtility.singleLineHeight;

                int deleteIndex = -1;
                int duplicateIndex = -1;
                int moveIndex = -1;
                for (int i = 0; i < keys.arraySize; i++)
                {
                    Rect rect = EditorGUI.PrefixLabel(position, new GUIContent("Item " + i));
                    rect.width -= BUTTON_SPACING + 3 * BUTTON_WIDTH;

                    Rect left = rect;
                    left.width /= 2;
                    Rect right = left;
                    right.x += left.width;

                    Rect leftButton = right;
                    leftButton.x += right.width + BUTTON_SPACING;
                    leftButton.width = BUTTON_WIDTH;
                    Rect middleButton = leftButton;
                    middleButton.x += BUTTON_WIDTH;
                    Rect rightButton = middleButton;
                    rightButton.x += BUTTON_WIDTH;
                    
                    EditorGUI.PropertyField(left, keys.GetArrayElementAtIndex(i), GUIContent.none);
                    EditorGUI.PropertyField(right, values.GetArrayElementAtIndex(i), GUIContent.none);
                    if (GUI.Button(leftButton, new GUIContent("+"), EditorStyles.miniButtonLeft))
                    {
                        duplicateIndex = i;
                    }
                    if (GUI.Button(middleButton, new GUIContent("\u2193"), EditorStyles.miniButtonMid))
                    {
                        moveIndex = i;
                    }
                    if (GUI.Button(rightButton, new GUIContent("X"), EditorStyles.miniButtonRight))
                    {
                        deleteIndex = i;
                    }

                    position.y += EditorGUIUtility.singleLineHeight;
                }
                if (duplicateIndex != -1)
                {
                    keys.InsertArrayElementAtIndex(duplicateIndex);
                    values.InsertArrayElementAtIndex(duplicateIndex);
                }
                if (moveIndex != -1)
                {
                    keys.MoveArrayElement(moveIndex, moveIndex + 1);
                    values.MoveArrayElement(moveIndex, moveIndex + 1);
                }
                if (deleteIndex != -1)
                {
                    keys.DeleteArrayElementAtIndex(deleteIndex);
                    values.DeleteArrayElementAtIndex(deleteIndex);
                }
                EditorGUI.indentLevel = oldIndentLevel;
            }
            EditorGUI.EndProperty();
        }
    } 
}