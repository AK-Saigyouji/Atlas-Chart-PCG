/* This class creates a layer of indirection on top of markers, taking care of editor-specific tasks like dragging 
 and grid-snapping. Markers are part of the actual project, while marker nodes are editor-only.*/

using System;
using UnityEngine;
using AKSaigyouji.EditorScripting;

namespace AKSaigyouji.AtlasGeneration
{
    public sealed class MarkerNode : ScriptableObject
    {
        public Vector2 Location
        {
            get { return marker.Position; }
            set { marker.Position = value; }
        }

        public Vector2 Size
        {
            get { return marker.Size; }
            set { marker.Size = value; }

        }
        public RawMarker Marker { get { return marker; } }

        [SerializeField] RawMarker marker;

        // This is used to snap the marker position. The "actual" position is kept track here, independently
        // of the marker value, and the marker value is updated by rounding this value. If we add deltas to 
        // the marker position directly and snap that, then we run into the problem that small deltas don't contribute 
        // anything to the position, leading to an inconsistent feel when dragging the markers around. We keep snapping
        // logic here and not in the marker, since this logic and data applies only to the behaviour in the editor, while
        // the markers are consumed outside of the editor. 
        Vector2 rawPosition;

        [SerializeField] DragHandler dragHandler = new DragHandler();

        public static MarkerNode Construct(RawMarker marker)
        {
            var node = CreateInstance<MarkerNode>();
            node.marker = marker;
            node.rawPosition = marker.Position;
            return node;
        }

        public MarkerNode DeepCopy()
        {
            RawMarker copiedMarker = new RawMarker(marker.Position, marker.Size, marker.Category);
            MarkerNode copiedNode = Construct(copiedMarker);
            copiedNode.rawPosition = rawPosition;
            return copiedNode;
        }

        /// <summary>
        /// Will consume the event and declare the GUI changed. Uses given delta instead of the event's.
        /// </summary>
        public void UpdateDrag(Event e, Rect rect, Vector2 delta)
        {
            if (dragHandler.IsDragging(e, rect))
            {
                e.Use();
                rawPosition += delta;
                marker.Position = Round(rawPosition);
                GUI.changed = true;
            }
        }

        static Vector2 Round(Vector2 raw)
        {
            SnapSetting snapSetting = ChartBuilderWindow.snapSetting;
            double multiple;

            if (snapSetting == SnapSetting.None)
            {
                return raw;
            }
            else if (snapSetting == SnapSetting.Half)
            {
                multiple = 0.5;
            }
            else if (snapSetting == SnapSetting.Tenth)
            {
                multiple = 0.1;
            }
            else if (snapSetting == SnapSetting.Hundredth)
            {
                multiple = 0.01;
            }
            else
            {
                throw new InvalidOperationException("Internal error: Invalid snap settings.");
            }
            return new Vector2(RoundDownToMultiple(raw.x, multiple), RoundDownToMultiple(raw.y, multiple));
        }

        static float RoundDownToMultiple(float x, double multiple)
        {
            return (float)((int)(x / multiple) * multiple);
        }
    } 
}