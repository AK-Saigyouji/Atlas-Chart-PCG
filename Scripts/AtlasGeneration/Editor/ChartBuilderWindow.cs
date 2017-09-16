/* This represents the meat of the functionality for the chart builder window, which allows for maps, in the form of
 Texture2Ds, to be adorned with markers for the purpose of content placement. */

using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using AKSaigyouji.EditorScripting;
using AKSaigyouji.Modules.MapGeneration;

namespace AKSaigyouji.AtlasGeneration
{
    public enum SnapSetting : sbyte
    {
        None = -1,
        Half = 0,
        Tenth = 1,
        Hundredth = 2,
    }

    public sealed class ChartBuilderWindow : EditorWindow
    {
        #region FieldsAndProperties
        [SerializeField] ChartStatic chart;

        Texture2D Map
        {
            get { return chart == null ? null : chart.Texture; }
            set
            {
                if (chart == null)
                {
                    if (value != null)
                    {
                        CreateNewChart(value);
                    }
                }
                else
                {
                    chart.Texture = value;
                }
            }
        }

        int scale = 20;

        MarkerNode SelectedMarker
        {
            get { return selectedMarker; }
            set
            {
                if (value != null)
                {
                    markers.Remove(value);
                    markers.Add(value); // move marker to end of list so it's rendered on top of other markers.
                }
                selectedMarker = value;
            }
        }
        MarkerNode selectedMarker;

        List<MarkerNode> markers;
        bool showGrids;
        public static SnapSetting snapSetting = SnapSetting.Tenth;

        readonly DragHandler dragHandler = new DragHandler();
        Vector2 dragTotal = Vector2.one;

        string chartName; // Used to update the chart's name

        MarkerPaletteModule Palette // retrieves custom pallete if it was assigned, otherwise uses default palette.
        {
            get
            {
                if (customPalette != null) return customPalette;
                if (defaultPalette == null) defaultPalette = MarkerPalette.DefaultPalette;
                return defaultPalette;
            }
        }

        MarkerPaletteModule customPalette;
        MarkerPaletteModule defaultPalette;

        Editor paletteEditor;
        Editor selectedMarkerEditor;

        bool panelChartFoldout = false;
        bool panelEditorOptionsFoldout = false;
        bool panelMarkerOptionsFoldout = false;

        readonly Color GRID_COLOR_MINOR = new Color(0.5f, 0.5f, 0.5f, 0.2f);
        readonly Color GRID_COLOR_MAJOR = new Color(0.5f, 0.5f, 0.5f, 0.4f);

        const int PADDING = 2;

        const int PANEL_OFFSET = 250;

        #endregion

        [MenuItem("Window/AKS - Chart Builder")]
        static void OpenWindow()
        {
            var window = GetWindow<ChartBuilderWindow>();
            window.titleContent = new GUIContent("Chart Builder");
        }

        #region UnityFunctions
        void OnEnable()
        {
            Load();
            SelectedMarker = null;
            if (markers == null)
            {
                markers = chart == null ? new List<MarkerNode>() : chart.Markers.Select(m => MarkerNode.Construct(m)).ToList();
            }
        }

        void OnDisable()
        {
            MarkChartDirty();
            Save();
        }

        void OnGUI()
        {
            DrawPanel();
            HandleContextMenu();

            GUI.BeginGroup(GetTextureArea());
            DrawMap();
            DrawMarkers();
            GUI.EndGroup();

            DrawGrids();

            HandleDrag();
            if (GUI.changed)
            {
                Repaint();
            }
        }
        #endregion

        #region ContextMenu
        void HandleContextMenu()
        {
            // Note: this gets called outside of the texture area group, so it uses coordinates relative to the window.
            if (MouseClickedOnRect(GetTextureArea(), mouseButton: 1))
            {
                var menu = new GenericMenu();
                AddMenuItemAddMarker(menu);
                AddMenuItemDuplicateMarker(menu);
                AddMenuItemRemoveMarker(menu);
                AddMenuItemRemoveAllMarkers(menu);
                menu.ShowAsContext();
            }
        }

        void AddMenuItemAddMarker(GenericMenu menu)
        {
            var addMarker = "Add New Marker";
            var addPresetMarker = "Add Preset Marker";
            if (chart != null && Map != null)
            {
                Vector2 pos = Event.current.mousePosition;
                foreach (MarkerCategory category in Palette.GetCategories())
                {
                    var label = new GUIContent(string.Format("{0}/{1}", addMarker, category.name));
                    menu.AddItem(label, false, () => CreateMarker(pos, Vector2.one, category.name));
                }
                foreach (MarkerPreset preset in Palette.GetPresets())
                {
                    var presetLabel = new GUIContent(string.Format("{0}/{1}", addPresetMarker, preset.Name));
                    menu.AddItem(presetLabel, false, () => CreateMarker(pos, preset.Size, preset.Category, preset.Name, preset.MetaData));
                }
            }
            else
            {
                menu.AddDisabledItem(new GUIContent(addMarker));
            }
        }

        void AddMenuItemDuplicateMarker(GenericMenu menu)
        {
            var duplicateMarker = new GUIContent("Duplicate Marker");
            MarkerNode nodeUnderMouse;
            if (IsNodeRightClicked(out nodeUnderMouse))
            {
                menu.AddItem(duplicateMarker, false, () => CreateMarker(nodeUnderMouse));
            }
            else
            {
                menu.AddDisabledItem(duplicateMarker);
            }
        }

        void AddMenuItemRemoveMarker(GenericMenu menu)
        {
            var removeMarker = new GUIContent("Remove Marker");
            MarkerNode nodeUnderMouse;
            if (IsNodeRightClicked(out nodeUnderMouse))
            {
                menu.AddItem(removeMarker, false, () => RemoveMarker(nodeUnderMouse));
            }
            else
            {
                menu.AddDisabledItem(removeMarker);
            }
        }

        void AddMenuItemRemoveAllMarkers(GenericMenu menu)
        {
            var removeAllMarkers = new GUIContent("Remove All Markers");
            if (markers.Count == 0)
            {
                menu.AddDisabledItem(removeAllMarkers);
            }
            else
            {
                menu.AddItem(removeAllMarkers, false, RemoveAllMarkers);
            }
        }
        #endregion

        #region TextureArea
        void DrawGrids()
        {
            if (showGrids)
            {
                HandleHelpers.DrawGrid(GetTextureArea(), scale, GRID_COLOR_MINOR, dragTotal);
                HandleHelpers.DrawGrid(GetTextureArea(), scale * 10, GRID_COLOR_MAJOR, dragTotal);
            }
        }

        void DrawMap()
        {
            if (chart == null)
            {
                DrawMessageInTextureArea("Insert a Chart or Map to begin.");
            }
            else if (Map == null)
            {
                DrawMessageInTextureArea("Insert a Map (texture) to begin.");
            }
            else
            {
                Vector2 size = new Vector2(Map.width, Map.height);
                GUI.DrawTexture(new Rect(dragTotal, scale * size), Map);
            }
        }

        Rect GetTextureArea()
        {
            return new Rect(PANEL_OFFSET, 0f, position.width, position.height);
        }

        void DrawMessageInTextureArea(string message)
        {
            Rect rect = new Rect(position.size / 2, new Vector2(250, 20));
            rect.x -= (PANEL_OFFSET + 250) / 2;
            GUI.Box(rect, message);
        }
        #endregion

        #region MouseEvents

        bool IsNodeRightClicked(out MarkerNode rightClickedNode)
        {
            rightClickedNode = markers.LastOrDefault(node =>
            {
                Rect markerRect = GetMarkerRect(node);
                markerRect.x += PANEL_OFFSET;
                return MouseClickedOnRect(markerRect, mouseButton: 1);
            });
            return rightClickedNode != null;
        }

        void HandleDrag()
        {
            Event e = Event.current;
            Rect textureArea = GetTextureArea();
            Vector2 delta = new Vector2(e.delta.x, -e.delta.y) / scale;
            foreach (MarkerNode marker in ((IEnumerable<MarkerNode>)markers).Reverse())
            {
                Rect markerRect = GetMarkerRect(marker);
                markerRect.x += textureArea.x;
                marker.UpdateDrag(e, markerRect, delta);
            }
            if (dragHandler.IsDragging(e, textureArea))
            {
                dragTotal += e.delta;
                GUI.changed = true;
                e.Use();
            }
        }

        bool MouseClickedOnRect(Rect rect, int mouseButton)
        {
            Event e = Event.current;
            return rect.Contains(e.mousePosition) && e.type == EventType.MouseDown && e.button == mouseButton;
        }

        #endregion

        #region Serialization

        void Save()
        {
            var save = ChartEditorSave.CreateSave();
            save.chart = chart;
            save.palette = customPalette;
            save.scale = scale;
            save.showGrids = showGrids;
            save.panelChartFoldout = panelChartFoldout;
            save.panelEditorOptionsFoldout = panelEditorOptionsFoldout;
            save.panelMarkerOptionsFoldout = panelMarkerOptionsFoldout;
            save.snapSetting = snapSetting;
            ChartEditorSave.Save(save);
        }

        void Load()
        {
            var save = ChartEditorSave.Load();
            if (save != null)
            {
                chart = save.chart;
                customPalette = save.palette;
                scale = save.scale;
                showGrids = save.showGrids;
                snapSetting = save.snapSetting;
                panelChartFoldout = save.panelChartFoldout;
                panelEditorOptionsFoldout = save.panelEditorOptionsFoldout;
                panelMarkerOptionsFoldout = save.panelMarkerOptionsFoldout;
            }
        }

        void CreateNewChart(Texture2D map)
        {
            chart = ChartStatic.Construct(map);
            chart.name = "New Chart";
            LoadChart(chart);
            SaveChart();
        }

        void MarkChartDirty()
        {
            if (chart != null)
            {
                EditorUtility.SetDirty(chart);
            }
        }

        void LoadChart(ChartStatic chart)
        {
            if (chart == null)
            {
                markers.Clear();
                chartName = string.Empty;
            }
            else
            {
                markers = chart.Markers.Select(m => MarkerNode.Construct(m)).ToList();
                chartName = chart.name;
            }
        }
        
        void SaveChart()
        {
            string folderPath = NodeEditorSettings.PathToChartFolder;
            string chartPath = IOHelpers.GetAvailableAssetPath(folderPath, string.Format("{0}.asset", chart.name));
            AssetDatabase.CreateAsset(chart, chartPath);
        }

        #endregion 

        #region MarkerFunctions

        void DrawMarkers()
        {
            MarkerNode clickedNode = null;
            foreach (MarkerNode markerNode in markers)
            {
                Rect markerRect = GetMarkerRect(markerNode);
                if (MouseClickedOnRect(markerRect, mouseButton: 0))
                {
                    clickedNode = markerNode;
                }
                var markerColor = GetMarkerColor(markerNode.Marker);
                if (markerNode == selectedMarker)
                {
                    // To make the selected marker stand out, its transparency is increased/decreased if low/high. 
                    markerColor.a = markerColor.a > 0.5f ? 0.2f : 0.8f;
                }
                GUI.color = markerColor;
                GUI.Box(markerRect, GUIContent.none);
            }
            Event e = Event.current;
            if (clickedNode != null)
            {
                SelectedMarker = clickedNode;
                GUI.changed = true;
            }
            else if (e.type == EventType.MouseDown && e.button == 0)
            {
                SelectedMarker = null;
                GUI.changed = true;
            }
        }

        Color GetMarkerColor(RawMarker marker)
        {
            var category = Palette.GetCategories().FirstOrDefault(cat => cat.name == marker.Category);
            return category == null ? Palette.DefaultColor : category.color;
        }

        Rect GetMarkerRect(MarkerNode marker)
        {
            Vector2 size = scale * marker.Size;
            Vector2 rawLocation = marker.Location;
            Vector2 flippedLocation = new Vector2(rawLocation.x, Map.height - rawLocation.y);
            Vector2 scaledLocation = scale * flippedLocation;
            Vector2 offsetLocation = scaledLocation + dragTotal;
            Rect rect = new Rect();
            rect.size = size;
            rect.center = offsetLocation;
            return rect;
        }

        void CreateMarker(MarkerNode node)
        {
            var copied = node.DeepCopy();
            markers.Add(copied);
            chart.AddMarker(copied.Marker);

            SelectedMarker = copied;
        }

        void CreateMarker(Vector2 position, Vector2 size, string category = "", string preset = "", MetaData metaData = null)
        {
            position = (position - dragTotal - PANEL_OFFSET * Vector2.right) / scale;
            position.y = Map.height - position.y;
            var marker = new RawMarker(position, size, category, preset, metaData);
            chart.AddMarker(marker);
            var markerNode = MarkerNode.Construct(marker);
            markers.Add(markerNode);

            SelectedMarker = markerNode;
        }

        void RemoveMarker(MarkerNode node)
        {
            chart.RemoveMarker(node.Marker);
            markers.Remove(node);
            if (SelectedMarker == node)
            {
                SelectedMarker = null;
            }
        }

        void RemoveAllMarkers()
        {
            if (EditorUtility.DisplayDialog("Confirm Remove All", "Remove all markers from chart?", "Yes", "No"))
            {
                chart.RemoveAllMarkers();
                markers.Clear();
                SelectedMarker = null;
            }
        }

        #endregion

        #region Panel

        void DrawPanel()
        {
            Vector2 panelSize = new Vector2(PANEL_OFFSET, position.height);
            Vector2 padding = new Vector2(2 * PADDING, 2 * PADDING);
            Rect groupRect = new Rect(padding, panelSize - 2 * padding);
            GUI.Box(new Rect(Vector2.zero, panelSize), GUIContent.none);

            GUILayout.BeginArea(groupRect);
            GUILayout.BeginVertical();
            DrawChart();
            DrawEditorOptions();
            DrawMarkerOptions();
            DrawSelectedMarker();
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        void DrawChart()
        {
            panelChartFoldout = PanelHeaderFoldout(panelChartFoldout, "Chart");
            EditorGUI.indentLevel++;
            if (panelChartFoldout)
            {
                DrawChartField();
                if (chart != null)
                {
                    DrawUpdateChartName();
                    DrawChartMetaData();
                }
                DrawMapField();
            }
            EditorGUI.indentLevel--;
        }

        void DrawEditorOptions()
        {
            panelEditorOptionsFoldout = PanelHeaderFoldout(panelEditorOptionsFoldout, "Editor Options");
            EditorGUI.indentLevel++;
            if (panelEditorOptionsFoldout)
            {
                DrawGridToggle();
                DrawZoomField();
                DrawSnapSetting();   
            }
            EditorGUI.indentLevel--;
        }

        void DrawMarkerOptions()
        {
            panelMarkerOptionsFoldout = PanelHeaderFoldout(panelMarkerOptionsFoldout, "Marker Settings");
            EditorGUI.indentLevel++;
            if (panelMarkerOptionsFoldout)
            {
                DrawMarkerPalette();
            }
            EditorGUI.indentLevel--;
        }

        void DrawSelectedMarker()
        {
            if (SelectedMarker != null)
            {
                EditorGUIUtility.labelWidth = 80;
                EditorGUILayout.LabelField("Selected Marker", EditorStyles.boldLabel);
                DrawMarkerEditor(SelectedMarker);
                EditorGUIUtility.labelWidth = 0;
            }
        }

        bool PanelHeaderFoldout(bool foldout, string label)
        {
            return EditorGUILayout.Foldout(foldout, label, true);
        }

        #region PropertyDrawers
        void DrawMarkerEditor(MarkerNode marker)
        {
            Editor.CreateCachedEditor(marker, null, ref selectedMarkerEditor);
            if (selectedMarkerEditor != null)
            {
                selectedMarkerEditor.OnInspectorGUI();
            }
        }

        void DrawZoomField()
        {
            var scaleLabel = new GUIContent("Zoom");
            scale = EditorGUILayout.IntField(scaleLabel, scale);
            scale = Mathf.Max(1, scale);
        }

        void DrawSnapSetting()
        {
            var snapLabel = new GUIContent("Snap Setting", "How should the markers be snapped to the grid?");
            snapSetting = (SnapSetting)EditorGUILayout.EnumPopup(snapLabel, snapSetting);
        }

        void DrawMarkerPalette()
        {
            EditorGUIUtility.labelWidth = 60f;
            EditorGUILayout.BeginHorizontal();
            var guiContent = new GUIContent("Palette", "Apply a custom Marker Palette to customize categories and colours.");
            EditorGUILayout.PrefixLabel(guiContent);
            customPalette = (MarkerPalette)EditorGUILayout.ObjectField(customPalette, typeof(MarkerPalette), allowSceneObjects: false);
            EditorGUILayout.EndHorizontal();
            Editor.CreateCachedEditor(Palette, null, ref paletteEditor);
            ((MarkerPaletteEditor)paletteEditor).canEdit = false;
            EditorGUIUtility.labelWidth = 0; 
            paletteEditor.OnInspectorGUI();
        }

        void DrawChartField()
        {
            EditorGUI.BeginChangeCheck();
            var newChart = (ChartStatic)EditorGUILayout.ObjectField(chart, typeof(ChartStatic), false);
            if (EditorGUI.EndChangeCheck())
            {
                MarkChartDirty();
                chart = newChart;
                LoadChart(chart);
            }
        }

        void DrawUpdateChartName()
        {
            EditorGUILayout.BeginHorizontal();
            chartName = EditorGUILayout.TextField(chartName);
            if (GUILayout.Button("Update Name") && chart != null)
            {
                AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(chart), chartName);
            }
            EditorGUILayout.EndHorizontal();
        }

        void DrawChartMetaData()
        {
            var serializedChart = new SerializedObject(chart);
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(serializedChart.FindProperty("metaData"));
            if (EditorGUI.EndChangeCheck())
            {
                GUI.changed = true;
            }
        }

        void DrawMapField()
        {
            Map = (Texture2D)EditorGUILayout.ObjectField("Map (Texture2D)", Map, typeof(Texture2D), false);
        }

        void DrawGridToggle()
        {
            showGrids = EditorGUILayout.Toggle("Show Grid", showGrids);
        }
        #endregion
        #endregion
    } 
}