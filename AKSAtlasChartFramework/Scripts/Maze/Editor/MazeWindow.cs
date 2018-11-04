using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEditor;

namespace AKSaigyouji.AtlasGeneration
{
    public enum HighlightRegion { None, Square, Vertical, Horizontal }

    sealed class MazeWindow : EditorWindow
    {
        SerializedObject serializedMaze;
        SerializedProperty serializedLength;
        SerializedProperty serializedWidth;

        int scale = 25;

        MazeDrawer mazeDrawer = new MazeDrawer();
        MazeTexture mazeTexture;

        Action<List<Coord>, List<MazeLink>, List<CellTag>> updateMaze;

        List<Coord> selectedCells;
        List<MazeLink> selectedLinks;
        Dictionary<Coord, List<KeyValuePair<string, string>>> cellTags;

        AtlasEditorState properties;

        readonly Color WALL_COLOR = MazeDrawer.BACKGROUND_COLOR;
        readonly Color CELL_COLOR = MazeDrawer.CELL_COLOR;
        readonly Color SELECTED_COLOR = new Color(0.4039f, 0.5608f, 0.4588f); // desaturated green
        readonly Color TAG_COLOR = new Color(0.4745f, 0.3529f, 0.3529f); // desaturated red

        const float PADDING = 5f;
        const float PANEL_WIDTH = 300f;

        const string EDITOR_PREFS_PREFIX = "AKSaigyouji.AtlasGeneration.MazeWindow.";
        const string EDITOR_PREFS_SCALE_KEY = EDITOR_PREFS_PREFIX + "Scale";
        const string EDITOR_PREFS_WINDOW_SIZE_X_KEY = EDITOR_PREFS_PREFIX + "sizeX";
        const string EDITOR_PREFS_WINDOW_SIZE_Y_KEY = EDITOR_PREFS_PREFIX + "sizeY";
        const string EDITOR_PREFS_WINDOW_POSITION_X_KEY = EDITOR_PREFS_PREFIX + "positionX";
        const string EDITOR_PREFS_WINDOW_POSITION_Y_KEY = EDITOR_PREFS_PREFIX + "positionY";

        IEnumerable<Coord> EnumerateGridCoordinates()
        {
            int width = serializedWidth.intValue;
            int length = serializedLength.intValue;
            for (int y = 0; y < width; y++)
            {
                for (int x = 0; x < length; x++)
                {
                    yield return new Coord(x, y);
                }
            }
        }

        void CreateMazeTexture()
        {
            Maze maze = new Maze(EnumerateGridCoordinates(), Enumerable.Empty<MazeLink>());
            mazeTexture = mazeDrawer.CreateTexture(maze, properties.TagTable);
            foreach (Coord cell in selectedCells)
            {
                mazeTexture.ColorCell(cell, SELECTED_COLOR);
            }
            foreach (MazeLink link in selectedLinks)
            {
                mazeTexture.ColorLink(link, SELECTED_COLOR);
            }
            if (properties.TagTable != null)
            {
                foreach (var tagListPair in cellTags)
                {
                    DrawTags(tagListPair.Key);
                }
            }
            mazeTexture.Apply();
        }

        public static void OpenWindow(SerializedObject maze, 
            IEnumerable<Coord> cells, IEnumerable<MazeLink> links, IEnumerable<CellTag> tags,
            Action<List<Coord>, List<MazeLink>, List<CellTag>> updateMaze)
        {
            Assert.IsNotNull(maze);
            var window = GetWindow<MazeWindow>("Maze Editor", true);
            if (EditorPrefs.HasKey(EDITOR_PREFS_SCALE_KEY))
            {
                window.scale = EditorPrefs.GetInt(EDITOR_PREFS_SCALE_KEY);
            }
            window.position = window.ComputeWindowPosition();
            window.updateMaze = updateMaze;
            window.serializedMaze = maze;
            window.serializedLength = maze.FindProperty("length");
            window.serializedWidth = maze.FindProperty("width");

            window.selectedCells = cells.ToList();
            window.selectedLinks = links.ToList();

            window.cellTags = new Dictionary<Coord, List<KeyValuePair<string, string>>>();
            foreach (var tag in tags)
            {
                if (!window.cellTags.ContainsKey(tag.Cell))
                {
                    window.cellTags.Add(tag.Cell, tag.MetaData.ToList());
                }
                else
                {
                    Debug.LogWarningFormat("Removing duplicate cell {0} with metadata: {1}", tag.Cell, tag.MetaData);
                }
            }

            window.properties = AtlasEditorState.Load();

            window.CreateMazeTexture();
        }

        void OnSelectionChange()
        {
            Close();
        }

        Rect ComputeWindowPosition()
        {
            Resolution res = Screen.currentResolution;
            float x = EditorPrefs.GetFloat(EDITOR_PREFS_WINDOW_POSITION_X_KEY, res.width / 3);
            float y = EditorPrefs.GetFloat(EDITOR_PREFS_WINDOW_POSITION_Y_KEY, res.height / 3);
            float width = EditorPrefs.GetFloat(EDITOR_PREFS_WINDOW_SIZE_X_KEY, res.width / 3);
            float height = EditorPrefs.GetFloat(EDITOR_PREFS_WINDOW_SIZE_Y_KEY, 2 * res.height / 3 - 50f);
            return new Rect(x, y, width, height);
        }

        void OnDisable()
        {
            EditorPrefs.SetInt(EDITOR_PREFS_SCALE_KEY, scale);

            EditorPrefs.SetFloat(EDITOR_PREFS_WINDOW_POSITION_X_KEY, position.x);
            EditorPrefs.SetFloat(EDITOR_PREFS_WINDOW_POSITION_Y_KEY, position.y);
            EditorPrefs.SetFloat(EDITOR_PREFS_WINDOW_SIZE_X_KEY, position.width);
            EditorPrefs.SetFloat(EDITOR_PREFS_WINDOW_SIZE_Y_KEY, position.height);
        }

        void SaveMaze()
        {
            // This removes metadata with empty keys, logging a warning if any are found.
            // If duplicate keys are found, then this will cause an error and the maze won't be saved.
            var cleanedCellTags = new List<CellTag>();
            int removedTags = 0;
            foreach (var cellTag in cellTags)
            {
                MetaData cleanMetaData = new MetaData();
                var rawMetaData = cellTag.Value;
                removedTags += rawMetaData.Count(pair => pair.Key == "");
                foreach (var pair in rawMetaData)
                {
                    if (!string.IsNullOrEmpty(pair.Key)) // only keep metadata with nonempty keys. Empty val is okay.
                    {
                        cleanMetaData.Add(pair);
                    }
                }
                if (cleanMetaData.Count > 0) // if all the tags had empty keys, we skip it altogether
                {
                    cleanedCellTags.Add(new CellTag(cellTag.Key, cleanMetaData));
                }
            }
            if (removedTags > 0)
            {
                Debug.LogWarningFormat("{0} metadata pairs removed due to empty keys.", removedTags);
            }
            updateMaze(selectedCells, selectedLinks, cleanedCellTags);
        }

        void Update()
        {
            // Normally editors get updated infrequently since they're mostly static. Since we want to highlight the 
            // part of the texture that the mouse is currently hovering over, we repaint far more frequently to ensure 
            // the highlighted part gets updated smoothly. Without this, there's a noticeably visual delay as the cursor 
            // moves around. 
            Repaint();
        }

        void OnGUI()
        {
            if (serializedMaze == null) // Happens e.g. if re-compiling scripts with a maze editor window open.
            {
                Close();
                return;
            }

            serializedMaze.Update();

            GUILayout.BeginHorizontal();
            DrawPanel();
            DrawMazeArea();
            GUILayout.EndHorizontal();

            serializedMaze.ApplyModifiedProperties();
        }

        void DrawPanel()
        {
            var panelSize = new Vector2(PANEL_WIDTH, position.height);
            var padding = PADDING * Vector2.one;
            var groupRect = new Rect(padding, panelSize - 2 * padding);
            GUI.Box(new Rect(Vector2.zero, panelSize), GUIContent.none);
            GUILayout.BeginArea(groupRect);
            GUILayout.BeginVertical();
            EditorGUI.BeginChangeCheck();
            properties.TagTable = (TagDrawTable)EditorGUILayout.ObjectField("Tag Table", properties.TagTable, typeof(TagDrawTable), allowSceneObjects: false);
            if (EditorGUI.EndChangeCheck())
            {
                CreateMazeTexture();
            }
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(serializedLength);
            EditorGUILayout.PropertyField(serializedWidth);
            if (EditorGUI.EndChangeCheck())
            {
                // The min is a functional requirement, the max is to avoid texture sizes for use cases
                // this tool is not suited to support.
                serializedLength.intValue = Mathf.Clamp(serializedLength.intValue, 1, 100);
                serializedWidth.intValue = Mathf.Clamp(serializedWidth.intValue, 1, 100);
                CreateMazeTexture();
            }
            scale = EditorGUILayout.IntField("Scale", scale);

            // Note that we defer adding or removing items to after the loop terminates, to
            // avoid changing the list while we iterate over it. 
            EditorGUILayout.LabelField("Tags", EditorStyles.boldLabel);
            Coord? targetCell = null;
            KeyValuePair<string, string>? deleteKvp = null;
            KeyValuePair<string, string>? newKvp = null;
            foreach (var tag in cellTags)
            {
                Coord cell = tag.Key;
                var metaData = tag.Value;
                foreach (var pair in metaData)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUIUtility.labelWidth = 50;  // reduce label size
                    EditorGUILayout.PrefixLabel(cell.ToString());
                    EditorGUIUtility.labelWidth = 0;  // restore label size
                    string key = EditorGUILayout.DelayedTextField(pair.Key);
                    string value = EditorGUILayout.DelayedTextField(pair.Value);
                    if (pair.Key != key || pair.Value != value)  // This pair changed
                    {
                        targetCell = cell;
                        deleteKvp = pair;
                        newKvp = new KeyValuePair<string, string>(key, value);
                    }
                    if (GUILayout.Button("X"))
                    {
                        targetCell = cell;
                        deleteKvp = pair;
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.Separator();  // Create space before drawing the next tag's data.
            }
            if (targetCell.HasValue)
            {
                cellTags[targetCell.Value].Remove(deleteKvp.Value);
                if (newKvp.HasValue)
                {
                    cellTags[targetCell.Value].Add(newKvp.Value);
                }
                DrawTags(targetCell.Value);
            }
            if (GUILayout.Button("Save"))
            {
                SaveMaze();
            }
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        void DrawMazeArea()
        {
            Vector2 panelSize = new Vector2(position.width - PANEL_WIDTH, position.height);
            Vector2 padding = PADDING * Vector2.one;
            Rect groupRect = new Rect(new Vector2(PANEL_WIDTH, 0) + padding, panelSize - 2 * padding);

            GUILayout.BeginArea(groupRect);
            HandleMouseClicks();
            DrawMaze();
            GUILayout.EndArea();
            mazeTexture.Apply();
        }

        void HandleMouseClicks()
        {
            if (Event.current == null)
                return;

            int descaledRight = mazeTexture.NumCellsX;
            int descaledBottom = mazeTexture.NumCellsY;
            int right = descaledRight * scale;
            int bottom = descaledBottom * scale;

            Vector2 rawPosition = Event.current.mousePosition;
            float x = rawPosition.x;
            float y = rawPosition.y;
            Highlight highlight;
            if ((0 < x && x < right) && (0 < y && y < bottom)) // Mouse is over maze
            {
                highlight = Highlight.CreateActive(rawPosition, descaledRight, descaledBottom, scale);
                if (Event.current.type == EventType.MouseDown && Event.current.button == 0) // Left click
                {
                    Toggle(highlight);
                }
                else if (Event.current.type == EventType.MouseDown && Event.current.button == 1) // Right click
                {
                    // Adding tag for unselected cell is unallowed.
                    if (highlight.Region == HighlightRegion.Square && selectedCells.Contains(highlight.Location))
                    {
                        AddTag(highlight.Location, "", "");
                    }
                }
            }
            else
            {
                highlight = Highlight.CreateInactive();
            }
            UpdateHighlight(highlight);
        }

        void AddTag(Coord cell, string key, string value)
        {
            Assert.IsNotNull(key);
            Assert.IsNotNull(value);
            var newPair = new KeyValuePair<string, string>(key, value);
            if (cellTags.ContainsKey(cell)) // Add pair to existing metadata.
            {
                cellTags[cell].Add(newPair);
            }
            else // New metadata, so we need to add a visual indicator to the cell.
            {
                cellTags[cell] = new List<KeyValuePair<string, string>>() { newPair };
                mazeTexture.ColorTag(cell, MazeDrawPrimitives.Exclamation(), TAG_COLOR);
            }
        }

        void DrawMaze()
        {
            mazeTexture.Draw(new Rect(0f, 0f, mazeTexture.NumCellsX * scale, mazeTexture.NumCellsY * scale));
        }

        void Toggle(Highlight highlight)
        {
            if (highlight.Region == HighlightRegion.Square)
            {
                ToggleSelectedCell(highlight.Location);
            }
            else if (highlight.Region == HighlightRegion.Horizontal || highlight.Region == HighlightRegion.Vertical)
            {
                ToggleSelectedLink(highlight.ImplicitMazeLink);
            }
            else
            {
                Debug.LogError("Internal error: attempted to toggle an inactive highlight.");
            }
        }

        void ToggleSelectedCell(Coord cell)
        {
            if (selectedCells.Contains(cell))
            {
                Deselect(cell);
            }
            else
            {
                Select(cell);
            }
        }

        void ToggleSelectedLink(MazeLink link)
        {
            if (selectedLinks.Contains(link))
            {
                Deselect(link);
            }
            else
            {
                Select(link);
            }
        }

        void Select(MazeLink link)
        {
            if (!selectedLinks.Contains(link))
            {
                selectedLinks.Add(link);
                mazeTexture.ColorLink(link, SELECTED_COLOR);
                // Ensure the link is between linked cells.
                Select(link.CellA);
                Select(link.CellB);
            }
        }

        void Deselect(MazeLink link)
        {
            if (selectedLinks.Contains(link))
            {
                selectedLinks.Remove(link);
                mazeTexture.ColorLink(link, WALL_COLOR);
            }
        }

        void Select(Coord cell)
        {
            if (!selectedCells.Contains(cell))
            {
                selectedCells.Add(cell);
                mazeTexture.ColorCell(cell, SELECTED_COLOR);
            }
        }

        void Deselect(Coord cell)
        {
            if (selectedCells.Contains(cell))
            {
                selectedCells.Remove(cell);
                mazeTexture.ColorCell(cell, CELL_COLOR);
                // Deselect any links linking to this cell.
                foreach (MazeLink adjacentLink in selectedLinks.Where(link => link.ConnectsTo(cell)).ToArray())
                {
                    Deselect(adjacentLink);
                }
                RemoveTag(cell);
            }
        }

        void DeselectCell(Coord cell)
        {
            selectedCells.Remove(cell);
            mazeTexture.ColorCell(cell, CELL_COLOR);
        }

        void RemoveTag(Coord cell)
        {
            if (cellTags.ContainsKey(cell))
            {
                cellTags.Remove(cell);
                mazeTexture.ClearTags(cell);
            }
        }

        void DrawTags(Coord cell)
        {
            if (cellTags[cell].Count == 0)
            {
                RemoveTag(cell);
            }
            else
            {
                mazeTexture.ClearTags(cell);
                foreach (var tagList in cellTags[cell])
                {
                    mazeTexture.ColorTag(cell, properties.TagTable[tagList.Key].Coords, TAG_COLOR);
                }
            }
        }

        void UpdateHighlight(Highlight highlight)
        {
            if (highlight.Active)
            {
                mazeTexture.SetHighlight(highlight.Location, highlight.Region);
            }
            else
            {
                mazeTexture.RemoveHighlight();
            }
        }

        struct Highlight
        {
            /// <summary>
            /// Is something currently highlighted? 
            /// </summary>
            public bool Active { get { return active; } }

            /// <summary>
            /// Grid location of currently highlighted component. Only accessible if the highlight is active.
            /// Attempting to access the location of an inactive highlight will result in an invalid
            /// operation exception.
            /// </summary>
            public Coord Location
            {
                get
                {
                    if (!active)
                        throw new InvalidOperationException("Inactive highlights do not have a location.");
                    return location;
                }
            }

            /// <summary>
            /// Highlighted region.
            /// </summary>
            public HighlightRegion Region { get { return region; } }

            /// <summary>
            /// The maze link between the two cells around the region. Only valid when region is vertical
            /// or horizontal. Otherwise throws an exception.
            /// </summary>
            public MazeLink ImplicitMazeLink
            {
                get
                {
                    if (Region != HighlightRegion.Horizontal && Region != HighlightRegion.Vertical)
                        throw new InvalidOperationException("No link defined by this region.");

                    Coord otherLocation = Region == HighlightRegion.Horizontal ? Location.DownShift : Location.LeftShift;
                    return new MazeLink(Location, otherLocation);
                }
            }

            bool active;
            Coord location;
            HighlightRegion region;

            public static Highlight CreateInactive()
            {
                var status = new Highlight();
                status.active = false;
                status.location = new Coord(-1, -1);
                status.region = HighlightRegion.None;
                return status;
            }

            /// <summary>
            /// Create an active highlight based on the given position, size of the maze, and 
            /// scale. Will compute which region is being highlighted.
            /// </summary>
            public static Highlight CreateActive(Vector2 position, int numCellsX, int numCellsY, int scale)
            {
                position.y = scale * numCellsY - position.y;
                Vector2 descaledPosition = position / scale;
                int xRound = Mathf.RoundToInt(descaledPosition.x);
                int yRound = Mathf.RoundToInt(descaledPosition.y);
                float xDelta = Mathf.Abs(descaledPosition.x - xRound);
                float yDelta = Mathf.Abs(descaledPosition.y - yRound);

                HighlightRegion highlightedRegion;
                Coord location;
                if ((0 < xRound) && (xRound < numCellsX) && (xDelta < 0.15f) && (xDelta <= yDelta))
                {
                    highlightedRegion = HighlightRegion.Vertical;
                    location = new Coord(xRound, (int)descaledPosition.y);
                }
                else if ((0 < yRound) && (yRound < numCellsY) && (yDelta < 0.15f) && (yDelta < xDelta))
                {
                    highlightedRegion = HighlightRegion.Horizontal;
                    location = new Coord((int)descaledPosition.x, yRound);
                }
                else
                {
                    highlightedRegion = HighlightRegion.Square;
                    location = (Coord)descaledPosition;
                }

                return CreateActive(location, highlightedRegion);
            }

            static Highlight CreateActive(Coord location, HighlightRegion region)
            {
                var status = new Highlight();
                status.active = true;
                status.location = location;
                status.region = region;
                return status;
            }
        }
    }
}