using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEditor;

namespace AKSaigyouji.AtlasGeneration
{
    [CustomPropertyDrawer(typeof(RandomArray), useForChildren: true)]
    public sealed class RandomArrayDrawer : PropertyDrawer
    {
        float Height { get { return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing; } }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return property.isExpanded ? (1 + property.FindPropertyRelative("values").arraySize) * Height : Height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            position.height = EditorGUIUtility.singleLineHeight;
            var values = property.FindPropertyRelative("values");
            var weights = property.FindPropertyRelative("weights");
            int count = EditorGUI.IntField(position, "Count", values.arraySize);
            position.y += Height;
            values.arraySize = count;
            weights.arraySize = count;
            int totalWeight = 0;
            for (int i = 0; i < count; i++)
            {
                var weightProp = weights.GetArrayElementAtIndex(i);
                if (weightProp.intValue < 1)
                {
                    weightProp.intValue = 1;
                }
                totalWeight += weightProp.intValue;
            }
            for (int i = 0; i < count; i++)
            {
                Rect rect = EditorGUI.PrefixLabel(position, new GUIContent("Element " + i));
                Rect valueRect = new Rect(rect.x, rect.y, rect.width - 70, rect.height);
                Rect weightRect = new Rect(valueRect.xMax + 5, rect.y, 30, rect.height);
                Rect percentRect = new Rect(weightRect.xMax + 5, rect.y, 30, rect.height);
                var indent = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;
                EditorGUI.PropertyField(valueRect, values.GetArrayElementAtIndex(i), GUIContent.none);
                EditorGUI.PropertyField(weightRect, weights.GetArrayElementAtIndex(i), GUIContent.none);
                EditorGUI.LabelField(percentRect, (100 * weights.GetArrayElementAtIndex(i).intValue / totalWeight).ToString() + "%");
                EditorGUI.indentLevel = indent;
                position.y += Height;
            }
            EditorGUI.EndProperty();
        }
    }
}