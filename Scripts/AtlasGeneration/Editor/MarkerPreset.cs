using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace AKSaigyouji.AtlasGeneration
{
    [Serializable]
    public sealed class MarkerPreset
    {
        public string Name { get { return name; } }
        public string Category { get { return category; } }
        public Vector2 Size { get { return size; } }
        public MetaData MetaData { get { return metaData; } }

        [SerializeField] string name;
        [SerializeField] string category;
        [SerializeField] Vector2 size;
        [SerializeField] MetaData metaData;

        const string DEFAULT_NAME = "Unnamed";
        const string DEFAULT_CATEGORY = "None";
        const float DEFAULT_SIZE_X = 1f;
        const float DEFAULT_SIZE_Y = 1f;

        public MarkerPreset() : 
            this(DEFAULT_NAME, DEFAULT_CATEGORY, new Vector2(DEFAULT_SIZE_X, DEFAULT_SIZE_Y), new MetaData()) { }

        public MarkerPreset(MarkerPreset preset) : this(preset.name, preset.category, preset.size, preset.metaData) { }

        public MarkerPreset(string name, string category, Vector2 size, MetaData metaData)
        {
            this.name = name;
            this.category = category;
            this.size = size;
            this.metaData = new MetaData(metaData);
        }
    } 
}