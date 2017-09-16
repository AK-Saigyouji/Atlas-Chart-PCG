/* This object holds the data necessary to store and load the node editor between sessions. The SavedNode type
 handles serializing the nodes themselves, as that requires diving into a tree of references. */

using AKSaigyouji.EditorScripting;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEditor;

namespace AKSaigyouji.AtlasGeneration
{
    public sealed class ChartEditorSave : ScriptableObject
    {
        public ChartStatic chart;
        public MarkerPaletteModule palette;
        public int scale;
        public bool showGrids;
        public SnapSetting snapSetting;

        public bool panelChartFoldout;
        public bool panelEditorOptionsFoldout;
        public bool panelMarkerOptionsFoldout;

        const string assets = "Assets";
        const string resources = "Editor Default Resources";
        const string assetName = "AKSChartEditorState";
        const string assetNameWithExtension = assetName + ".asset";

        /// <summary>
        /// Load the editor from the last session. Returns null if none found.
        /// </summary>
        public static ChartEditorSave Load()
        {
            string path = IOHelpers.CombinePath(assets, resources, assetNameWithExtension);
            var save = EditorGUIUtility.Load(path) as ChartEditorSave;
            return save;
        }

        public static void Save(ChartEditorSave save)
        {
            Assert.IsNotNull(save);
            string folderPath = Path.Combine(assets, resources);
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.CreateFolder(assets, resources);
            }
            string assetPath = Path.Combine(folderPath, assetNameWithExtension);
            if (IOHelpers.AssetExists(assetName, folderPath))
            {
                AssetDatabase.DeleteAsset(assetPath);
            }
            AssetDatabase.CreateAsset(save, assetPath);
            AssetDatabase.SaveAssets();
        }

        public static ChartEditorSave CreateSave()
        {
            var savedEditor = CreateInstance<ChartEditorSave>();
            savedEditor.hideFlags = HideFlags.NotEditable;
            return savedEditor;
        }
    }
}