using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace AKSaigyouji
{
    [CustomPropertyDrawer(typeof(UList), true)]
    public sealed class UListPropertyDrawer : PropertyDrawer
    {
        const string ELEMENTS = "elements";

        const int BUTTON_WIDTH = 25;
        const int BUTTON_SPACING = 5;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight;
            if (property.isExpanded)
            {
                var elements = property.FindPropertyRelative(ELEMENTS);
                for (int i = 0; i < elements.arraySize; i++)
                {
                    height += EditorGUI.GetPropertyHeight(elements.GetArrayElementAtIndex(i), includeChildren: true);
                }
                height += EditorGUIUtility.singleLineHeight;
            }
            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            position.height = EditorGUIUtility.singleLineHeight;
            property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label);
            if (property.isExpanded)
            {
                int oldIndentLevel = EditorGUI.indentLevel;
                position.y += EditorGUIUtility.singleLineHeight;
                EditorGUI.indentLevel++;
                var elements = property.FindPropertyRelative(ELEMENTS);
                Rect sizeRect = EditorGUI.PrefixLabel(position, new GUIContent("Size"));
                elements.arraySize = EditorGUI.IntField(sizeRect, elements.arraySize);
                position.y += EditorGUIUtility.singleLineHeight;

                int deleteIndex = -1;
                int duplicateIndex = -1;
                int moveIndex = -1;
                for (int i = 0; i < elements.arraySize; i++)
                {
                    Rect itemRect = EditorGUI.PrefixLabel(position, new GUIContent("Item " + i));
                    itemRect.width -= BUTTON_SPACING + 3 * BUTTON_WIDTH;
                    Rect leftButton = itemRect;
                    leftButton.x += itemRect.width + BUTTON_SPACING;
                    leftButton.width = BUTTON_WIDTH;
                    Rect middleButton = leftButton;
                    middleButton.x += BUTTON_WIDTH;
                    Rect rightButton = middleButton;
                    rightButton.x += BUTTON_WIDTH;
                    EditorGUI.PropertyField(itemRect, elements.GetArrayElementAtIndex(i));
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
                    elements.InsertArrayElementAtIndex(duplicateIndex);
                }
                if (moveIndex != -1)
                {
                    elements.MoveArrayElement(moveIndex, moveIndex + 1);
                }
                if (deleteIndex != -1)
                {
                    elements.DeleteArrayElementAtIndex(deleteIndex);
                }
                EditorGUI.indentLevel = oldIndentLevel;
            }
            EditorGUI.EndProperty();
        }
    } 
}