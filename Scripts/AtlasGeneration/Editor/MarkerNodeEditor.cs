using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AKSaigyouji.AtlasGeneration
{
    [CustomEditor(typeof(MarkerNode))]
    public sealed class MarkerNodeEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("marker"), GUIContent.none, true);
            serializedObject.ApplyModifiedProperties();
        }
    } 
}