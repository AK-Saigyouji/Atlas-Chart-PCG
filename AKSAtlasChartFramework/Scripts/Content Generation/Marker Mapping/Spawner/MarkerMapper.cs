using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using AKSaigyouji.AtlasGeneration;

namespace AKSaigyouji.AtlasGeneration
{
    public abstract class MarkerMapper : MonoBehaviour { }

    /// <summary>
    /// A simple content strategy that instantiates a specific prefab for specific types of marker presets.
    /// </summary>
    public abstract class MarkerMapper<T> : MarkerMapper, IContentStrategy
    {
        [SerializeField] string[] keys;
        [SerializeField] T[] values;

        [Tooltip("Instantiated objects will be children of this object.")]
        [SerializeField] Transform parent;

        [Tooltip("Any chart with metadata in this list will be skipped. Key is required. Blank value acts as a wildcard. e.g. (Density, High) " +
            "will only match charts with the exact pair (Density, High), but the pair (Density, ), with an empty value, will" +
            "match all of (Density, ), (Density, Low), (Density, High), etc.")]
        [SerializeField] MetaData chartsToSkip;

        [Tooltip("Positions will be truncated so that they land on the integer grid, e.g. (3.4, 5.9) -> (3,5)")]
        [SerializeField] bool clampPositionsToGrid = false;

        [Tooltip("If 2D, the marker's (x,y) position will be interpreted as (x, y, 0). Otherwise, as (x, 0, y).")]
        [SerializeField] bool twoDimensional = false;

        #if UNITY_EDITOR
        [SerializeField] UnityEngine.Object _palette; // used purely by editor script, do not use in this script
        #endif

        public void GenerateContent(Atlas atlas)
        {
            var lookupTable = Enumerable.Range(0, keys.Length).ToDictionary(i => keys[i], i => values[i]);

            var markers = atlas.Charts
                               .Where(SkipFilter)
                               .SelectMany(chart => chart.UnusedMarkers)
                               .Where(marker => lookupTable.ContainsKey(marker.Preset))
                               .ToArray();

            foreach (Marker marker in markers)
            {
                T spawner = lookupTable[marker.Preset];
                InstantiateContent(spawner, marker, parent);
            }
        }

        protected abstract void InstantiateContent(T spawner, Marker marker, Transform parent);

        /// <summary>
        /// Convert (x, y) marker coordinate to actual 3d position based on the settings in the mapper.
        /// </summary>
        protected Vector3 ComputePosition(Vector2 markerPosition)
        {
            if (clampPositionsToGrid)
            {
                markerPosition = (Coord)markerPosition;
            }

            return twoDimensional ? (Vector3)markerPosition : new Vector3(markerPosition.x, 0f, markerPosition.y);
        }

        bool SkipFilter(Chart chart)
        {
            return !(chart.MetaData.Any(datum => chartsToSkip.ContainsKey(datum.Key)
            && (chartsToSkip[datum.Key] == string.Empty || chartsToSkip[datum.Key] == datum.Value)));
        }
    }
}