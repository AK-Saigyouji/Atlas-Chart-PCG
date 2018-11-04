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

        Texture2D Map
        {
            get { return properties.chart == null ? null : properties.chart.Texture; }
            set
            {
                if (properties.chart == null)
                {
                    if (value != null)
                    {
                        CreateNewChart(value);
                    }
                }
                else
                {
                    properties.chart.Texture = value;
                }
            }
        }

        MarkerPaletteModule Palette
        {
            get
            {
                if (properties.palette == null)
                    properties.palette = MarkerPalette.CreateDefaultPalette();

                return properties.palette;
            }
        }

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

        ChartEditorSave properties;
        List<MarkerNode> markers;

        readonly DragHandler dragHandler = new DragHandler();
        Vector2 dragTotal = Vector2.one;

        string chartName; // Used to update the chart's name

        Editor paletteEditor;
        Editor selectedMarkerEditor;

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
                markers = properties.chart == null 
                    ? new List<MarkerNode>() 
                    : properties.chart.Markers.Select(m => MarkerNode.Construct(m)).ToList();
            }
        }

        void OnDisable()
        {
            MarkChartDirty();
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
            if (properties.chart != null && Map != null)
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
            if (properties.showGrids)
            {
                HandleHelpers.DrawGrid(GetTextureArea(), properties.scale, GRID_COLOR_MINOR, dragTotal);
                HandleHelpers.DrawGrid(GetTextureArea(), properties.scale * 10, GRID_COLOR_MAJOR, dragTotal);
            }
        }

        void DrawMap()
        {
            if (properties.chart == null)
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
                GUI.DrawTexture(new Rect(dragTotal, properties.scale * size), Map);
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
            Vector2 delta = new Vector2(e.delta.x, -e.delta.y) / properties.scale;
            foreach (MarkerNode marker in ((IEnumerable<MarkerNode>)markers).Reverse())
            {
                Rect markerRect = GetMarkerRect(marker);
                markerRect.x += textureArea.x;
                marker.UpdateDrag(e, markerRect, delta, properties.snapSetting);
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

        void Load()
        {
            properties = ChartEditorSave.Load();
        }

        void CreateNewChart(Texture2D map)
        {
            properties.chart = ChartStatic.Construct(map);
            properties.chart.name = "New Chart";
            LoadChart(properties.chart);
            SaveChart();
        }

        void MarkChartDirty()
        {
            if (properties.chart != null)
            {
                EditorUtility.SetDirty(properties.chart);
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
            string path = EditorUtility.SaveFilePanelInProject("Create new chart...", "NewChart", "asset", "");
            //string chartPath = IOHelpers.GetAvailableAssetPath(path, System.IO.Path.GetFileName(path));
            AssetDatabase.CreateAsset(properties.chart, path);
            AssetDatabase.Refresh();
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
            Vector2 size = properties.scale * marker.Size;
            Vector2 rawLocation = marker.Location;
            Vector2 flippedLocation = new Vector2(rawLocation.x, Map.height - rawLocation.y);
            Vector2 scaledLocation = properties.scale * flippedLocation;
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
            properties.chart.AddMarker(copied.Marker);

            SelectedMarker = copied;
        }

        void CreateMarker(Vector2 position, Vector2 size, string category = "", string preset = "", MetaData metaData = null)
        {
            position = (position - dragTotal - PANEL_OFFSET * Vector2.right) / properties.scale;
            position.y = Map.height - position.y;
            var marker = new RawMarker(position, size, category, preset, metaData);
            properties.chart.AddMarker(marker);
            var markerNode = MarkerNode.Construct(marker);
            markers.Add(markerNode);

            SelectedMarker = markerNode;
        }

        void RemoveMarker(MarkerNode node)
        {
            properties.chart.RemoveMarker(node.Marker);
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
                properties.chart.RemoveAllMarkers();
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
            properties.panelChartFoldout = PanelHeaderFoldout(properties.panelChartFoldout, "Chart");
            EditorGUI.indentLevel++;
            if (properties.panelChartFoldout)
            {
                DrawChartField();
                if (properties.chart != null)
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
            properties.panelEditorOptionsFoldout = PanelHeaderFoldout(properties.panelEditorOptionsFoldout, "Editor Options");
            EditorGUI.indentLevel++;
            if (properties.panelEditorOptionsFoldout)
            {
                DrawGridToggle();
                DrawZoomField();
                DrawSnapSetting();   
            }
            EditorGUI.indentLevel--;
        }

        void DrawMarkerOptions()
        {
            properties.panelMarkerOptionsFoldout = PanelHeaderFoldout(properties.panelMarkerOptionsFoldout, "Marker Settings");
            EditorGUI.indentLevel++;
            if (properties.panelMarkerOptionsFoldout)
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
            properties.scale = EditorGUILayout.IntField(scaleLabel, properties.scale);
            properties.scale = Mathf.Max(1, properties.scale);
        }

        void DrawSnapSetting()
        {
            var snapLabel = new GUIContent("Snap Setting", "How should the markers be snapped to the grid?");
            properties.snapSetting = (SnapSetting)EditorGUILayout.EnumPopup(snapLabel, properties.snapSetting);
        }

        void DrawMarkerPalette()
        {
            EditorGUIUtility.labelWidth = 60f;
            EditorGUILayout.BeginHorizontal();
            var guiContent = new GUIContent("Palette", "Apply a custom Marker Palette to customize categories and colours.");
            EditorGUILayout.PrefixLabel(guiContent);
            properties.palette = (MarkerPalette)EditorGUILayout.ObjectField(
                properties.palette, typeof(MarkerPalette), allowSceneObjects: false);
            EditorGUILayout.EndHorizontal();
            Editor.CreateCachedEditor(Palette, null, ref paletteEditor);
            ((MarkerPaletteEditor)paletteEditor).canEdit = false;
            EditorGUIUtility.labelWidth = 0; 
            paletteEditor.OnInspectorGUI();
        }

        void DrawChartField()
        {
            EditorGUI.BeginChangeCheck();
            var newChart = (ChartStatic)EditorGUILayout.ObjectField(properties.chart, typeof(ChartStatic), false);
            if (EditorGUI.EndChangeCheck())
            {
                MarkChartDirty();
                properties.chart = newChart;
                LoadChart(properties.chart);
            }
        }

        void DrawUpdateChartName()
        {
            EditorGUI.BeginChangeCheck();
            chartName = EditorGUILayout.DelayedTextField("Name", chartName);
            if (EditorGUI.EndChangeCheck() && properties.chart != null)
            {
                AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(properties.chart), chartName);
            }
        }

        void DrawChartMetaData()
        {
            var serializedChart = new SerializedObject(properties.chart);
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
            properties.showGrids = EditorGUILayout.Toggle("Show Grid", properties.showGrids);
        }
        #endregion
        #endregion
    } 
}