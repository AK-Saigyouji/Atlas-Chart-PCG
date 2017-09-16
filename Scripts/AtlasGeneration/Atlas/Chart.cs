using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using AKSaigyouji.Maps;

namespace AKSaigyouji.AtlasGeneration
{
    /// <summary>
    /// A chart that has been placed into a completed atlas and is ready to be consumed. 
    /// </summary>
    public sealed class Chart
    {
        public Map Map { get { return map; } }
        public int Length { get { return map.Length; } }
        public int Width { get { return map.Width; } }
        public string Name { get { return name; } }
        public Coord Offset { get { return offset; } }
        public IEnumerable<Marker> Markers { get { return markers; } }
        public IEnumerable<Marker> UsedMarkers { get { return markers.Where(m => m.Used); } }
        public IEnumerable<Marker> UnusedMarkers { get { return markers.Where(m => !m.Used); } }
        public IEnumerable<KeyValuePair<string, string>> MetaData { get { return metaData; } }

        readonly Map map;
        readonly string name;
        readonly Marker[] markers;
        readonly Coord offset;
        readonly IEnumerable<KeyValuePair<string, string>> metaData;

        public Chart(RawChart chart, Coord offset)
        {
            if (chart == null)
                throw new ArgumentNullException("chart");

            if (chart.Map == null)
                throw new ArgumentException("Chart has invalid (null) map");

            map = chart.Map;
            name = chart.name;
            markers = chart.Markers.Select(m => new Marker(m, offset)).ToArray();
            this.offset = offset;
            metaData = chart.MetaData.Unpack();
        }

        /// <summary>
        /// Does this chart have metadata with this key?
        /// </summary>
        public bool Filter(string key, StringComparison comparisonType = StringComparison.Ordinal)
        {
            return metaData.Any(pair => pair.Key.Equals(key, comparisonType));
        }

        /// <summary>
        /// Does this chart have metadata with this key and corresponding value?
        /// </summary>
        public bool Filter(string key, string value, StringComparison comparisonType = StringComparison.Ordinal)
        {
            return metaData.Any(pair => pair.Key.Equals(key, comparisonType) && pair.Value.Equals(value, comparisonType));
        }
    } 
}