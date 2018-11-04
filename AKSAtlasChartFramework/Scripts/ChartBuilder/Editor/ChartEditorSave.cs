using AKSaigyouji.EditorScripting;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEditor;

namespace AKSaigyouji.AtlasGeneration
{
    /// <summary>
    /// A data bag storing the chart editor's properties.
    /// </summary>
    public sealed class ChartEditorSave : EditorState
    {
        public ChartStatic chart;
        public MarkerPaletteModule palette;
        public int scale = 20;
        public bool showGrids;
        public SnapSetting snapSetting = SnapSetting.Tenth;

        public bool panelChartFoldout;
        public bool panelEditorOptionsFoldout;
        public bool panelMarkerOptionsFoldout;

        const string ASSET_NAME = "AKSChartEditorState";

        public static ChartEditorSave Load()
        {
            return Load<ChartEditorSave>(ASSET_NAME);
        }
    }
}