/* Static charts are at the heart of the atlas system, as they can be adorned extensively with markers to represent
 content to be laid out. Although it's ultimately the Map that we care about, openly using textures both internally and 
 externally has several benefits. Simple image data can easily be compressed very effectively, so a very large number
 of charts can be stored as assets without blowing up the build sizes. Externally, using charts allows for the immediate 
 visualization of the map structure, and makes for simple plug and play of textures without having to create a new
 type of scriptable object for Maps.*/

using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using AKSaigyouji.Maps;

namespace AKSaigyouji.AtlasGeneration
{
    /// <summary>
    /// A chart holding a single, fixed map, in the form of a 2d texture.
    /// </summary>
    public sealed class ChartStatic : RawChart
    {
        public Texture2D Texture { get { return texture; } set { texture = value; } }

        public override Map Map
        {
            get
            {
                // This is an expensive operation, and there's no way to make this clear in the API without leaking
                // the chart's abstraction, so the map is cached in case it gets accessed repeatedly.
                if (cachedMap == null)
                {
                    cachedMap = Map.FromTexture(texture);
                }
                return cachedMap;
            }
        }
        public override MetaData MetaData { get { return metaData; } }
        public override IEnumerable<RawMarker> Markers { get { return markers; } }

        [SerializeField] Texture2D texture;
        [SerializeField] MetaData metaData;
        [SerializeField] List<RawMarker> markers;

        Map cachedMap;

        public static ChartStatic Construct(Texture2D map)
        {
            if (map == null)
                throw new ArgumentNullException();

            ChartStatic chart = CreateInstance<ChartStatic>();
            chart.texture = map;
            chart.metaData = new MetaData();
            chart.markers = new List<RawMarker>();
            return chart;
        }

        public static ChartStatic Construct(Map map)
        {
            if (map == null)
                throw new ArgumentNullException();

            Texture2D mapTexture = map.ToTexture();
            ChartStatic chart = Construct(mapTexture);
            chart.cachedMap = map;
            return chart;
        }

        /// <summary>
        /// Replaces the map in the chart, creating a new static chart with the same markers and metadata as the original.
        /// Note that the copy may be shallow.
        /// </summary>
        public static ChartStatic ReplaceMap(Map map, RawChart chart)
        {
            ChartStatic newChart = Construct(map);
            newChart.name = chart.name;
            newChart.markers = chart.Markers.ToList();
            newChart.metaData = chart.MetaData;
            return newChart;
        }

        public void AddMarker(RawMarker marker)
        {
            if (marker == null)
                throw new ArgumentNullException();

            markers.Add(marker);
        }

        public void RemoveMarker(RawMarker marker)
        {
            if (marker == null)
                throw new ArgumentNullException();

            if (!markers.Remove(marker))
                throw new ArgumentException("This chart does not contain the marker.");
        }

        public void RemoveAllMarkers()
        {
            markers.Clear();
        }
    } 
}