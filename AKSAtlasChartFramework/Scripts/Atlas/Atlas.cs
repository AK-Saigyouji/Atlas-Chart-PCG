using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using AKSaigyouji.Maps;

namespace AKSaigyouji.AtlasGeneration
{
    [SerializeField]
    public sealed class Atlas
    {
        /// <summary>
        /// The number of charts contained in this Atlas.
        /// </summary>
        public int Count { get { return charts.Length; } }

        /// <summary>
        /// Each chart represents a piece of the global map, including a local submap and content markers.
        /// </summary>
        public IEnumerable<Chart> Charts { get { return charts; } }

        /// <summary>
        /// The entire map this atlas corresponds to. 
        /// </summary>
        public Map GlobalMap { get { return map; } }

        readonly Chart[] charts;
        readonly Map map;

        public Atlas(IEnumerable<Chart> charts, Map map)
        {
            if (map == null)
                throw new ArgumentNullException("map");

            if (charts == null)
                throw new ArgumentNullException("charts");

            if (charts.Any(c => c == null))
                throw new ArgumentException("Null chart passed to atlas constructor.");

            this.charts = charts.ToArray();
            this.map = map;
        }

        /// <summary>
        /// Do these coordinates belong to the atlas? In particular, do these coordinates belong to any particular chart?
        /// Note that this is a stronger condition than being valid coordinates for the global map.
        /// </summary>
        public bool IsContainedInAtlas(Coord coordinate)
        {
            return IsContainedInAtlas(coordinate.x, coordinate.y);
        }

        /// <summary>
        /// Do these coordinates belong to the atlas? In particular, do these coordinates belong to any particular chart?
        /// Note that this is a stronger condition than being valid coordinates for the global map.
        /// </summary>
        public bool IsContainedInAtlas(int x, int y)
        {
            return charts.Any(chart =>
                chart.Offset.x <= x && x < chart.Offset.x + chart.Length &&
                chart.Offset.y <= y && y < chart.Offset.y + chart.Width);
        }

        // The following is a collection of enumeration and filtering methods simplifying a number a very common
        // patterns of use mainly for those who are unfamiliar/uncomfortable with LINQ. 
        #region LINQWrappers

        /// <summary>
        /// Return all the markers contained in these charts as a single, flat sequence. 
        /// </summary>
        public static IEnumerable<Marker> EnumerateMarkers(IEnumerable<Chart> charts)
        {
            if (charts == null)
                throw new ArgumentNullException("charts");

            return charts.SelectMany(chart => chart.Markers);
        }

        /// <summary>
        /// Return all unused markers contained in these charts as a single, flat sequence. 
        /// </summary>
        public static IEnumerable<Marker> EnumerateUnusedMarkers(IEnumerable<Chart> charts)
        {
            if (charts == null)
                throw new ArgumentNullException("charts");

            return charts.SelectMany(chart => chart.UnusedMarkers);
        }

        /// <summary>
        /// Get the charts adjacent (horizontally or vertically, not diagonally) to the given chart.
        /// </summary>
        public IEnumerable<Chart> GetAdjacentCharts(Chart chart)
        {
            if (chart == null)
                throw new ArgumentNullException("chart");

            Coord offset = chart.Offset;
            int length = chart.Length;
            int width = chart.Width;
            return charts.Where(otherChart => AreAdjacent(offset, otherChart.Offset, length, width)).ToArray();
        }

        /// <summary>
        /// Retrieves all charts with the given metadata;
        /// </summary>
        public static IEnumerable<Chart> FilterByMetaData(IEnumerable<Chart> charts, string key, string value)
        {
            if (charts == null)
                throw new ArgumentNullException("charts");

            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Key must be valid non-empty string.");

            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("Value must be valid non-empty string.");

            return charts.Where(chart => chart.Filter(key, value));
        }

        /// <summary>
        /// Retrieves all charts that don't have the given key with associated value. 
        /// Useful to skip charts specifically marked to be skipped.
        /// </summary>
        public static IEnumerable<Chart> ExcludeByMetaData(IEnumerable<Chart> charts, string key, string value)
        {
            if (charts == null)
                throw new ArgumentNullException("charts");

            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Key must be valid non-empty string.");

            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("Value must be valid non-empty string.");

            return charts.Where(chart => !chart.Filter(key, value));
        }

        /// <summary>
        /// Retrieves all charts with the given metadata key.
        /// </summary>
        public static IEnumerable<Chart> FilterByMetaData(IEnumerable<Chart> charts, string key)
        {
            if (charts == null)
                throw new ArgumentNullException("charts");

            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Key must be valid non-empty string.");

            return charts.Where(chart => chart.Filter(key));
        }

        /// <summary>
        /// Retrieves all charts that don't have the given key. Useful to skip charts specifically marked to be skipped.
        /// </summary>
        public static IEnumerable<Chart> ExcludeByMetaData(IEnumerable<Chart> charts, string key)
        {
            if (charts == null)
                throw new ArgumentNullException("charts");

            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Key must be valid non-empty string.");

            return charts.Where(chart => !chart.Filter(key));
        }

        public static IEnumerable<Marker> FilterByMetaData(IEnumerable<Marker> markers, string key)
        {
            if (markers == null)
                throw new ArgumentNullException("markers");

            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Key must be valid non-empty string.");

            return markers.Where(marker => marker.Filter(key));
        }

        public static IEnumerable<Marker> FilterByMetaData(IEnumerable<Marker> markers, string key, string value)
        {
            if (markers == null)
                throw new ArgumentNullException("markers");

            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Key must be valid non-empty string.");

            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("Value must be valid non-empty string.");

            return markers.Where(marker => marker.Filter(key, value));
        }

        public static IEnumerable<Marker> ExcludeByMetaData(IEnumerable<Marker> markers, string key)
        {
            if (markers == null)
                throw new ArgumentNullException("markers");

            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Key must be valid non-empty string.");

            return markers.Where(marker => !marker.Filter(key));
        }

        public static IEnumerable<Marker> ExcludeByMetaData(IEnumerable<Marker> markers, string key, string value)
        {
            if (markers == null)
                throw new ArgumentNullException("markers");

            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Key must be valid non-empty string.");

            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("Value must be valid non-empty string.");

            return markers.Where(marker => !marker.Filter(key, value));
        }

        #endregion

        static bool AreAdjacent(Coord chartOffsetA, Coord chartOffsetB, int length, int width)
        {
            Coord delta = (chartOffsetB - chartOffsetA).AbsoluteValues();
            return delta.x == length && delta.y == 0 
                || delta.y == width && delta.x == 0;
        }
    } 
}