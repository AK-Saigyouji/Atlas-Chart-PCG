using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using AKSaigyouji.Maps;

namespace AKSaigyouji.AtlasGeneration
{
    public sealed class Marker
    {
        /// <summary>
        /// Which category this marker belongs to, as defined by a marker palette.
        /// </summary>
        public string Category { get { return category; } }

        /// <summary>
        /// The name of the preset used to build this marker, as defined by a marker palette. If no preset was used, 
        /// will be the empty string.
        /// </summary>
        public string Preset { get { return preset; } }

        /// <summary>
        /// Position of the marker relative to its containing chart.
        /// </summary>
        public Vector2 LocalPosition { get { return localPosition; } }

        /// <summary>
        /// Global position of the marker, taking into account the chart's offset in the atlas.
        /// </summary>
        public Vector2 GlobalPositon { get { return globalPosition; } }

        /// <summary>
        /// The size of the marker's box.
        /// </summary>
        public Vector2 Size { get { return size; } }

        public bool Used { get { return used; } }

        /// <summary>
        /// Rect corresponding to this marker (matches the visual box seen in the chart editor). Position is given in
        /// global coordinates.
        /// </summary>
        public Rect Rect { get { return new Rect(globalPosition, size); } }

        /// <summary>
        /// Extra data associated with this marker.
        /// </summary>
        public IEnumerable<KeyValuePair<string, string>> MetaData { get { return metaData; } }

        readonly string category;
        readonly string preset;
        readonly Vector2 localPosition;
        readonly Vector2 globalPosition;
        readonly Vector2 size;
        readonly IEnumerable<KeyValuePair<string, string>> metaData;

        bool used = false;

        public Marker(RawMarker marker, Coord offset)
        {
            if (marker == null)
                throw new ArgumentNullException("marker");

            size = marker.Size;
            preset = marker.Preset;
            category = marker.Category;
            metaData = marker.MetaData.Unpack();
            localPosition = marker.Position;
            globalPosition = localPosition + offset;
        }

        /// <summary>
        /// Does this marker have metadata with the following key?
        /// </summary>
        public bool Filter(string key, StringComparison comparisonType = StringComparison.Ordinal)
        {
            return metaData.Any(pair => pair.Key.Equals(key, comparisonType));
        }

        /// <summary>
        /// Does this marker have metadata with the following key and corresponding value?
        /// </summary>
        public bool Filter(string key, string value, StringComparison comparisonType = StringComparison.Ordinal)
        {
            return metaData.Any(pair => pair.Key.Equals(key, comparisonType) && pair.Value.Equals(value, comparisonType));
        }

        /// <summary>
        /// Declare this marker used. Throws exception if using a used marker.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public void Use()
        {
            if (used)
                throw new InvalidOperationException("Marker already used.");

            used = true;
        }
    } 
}