/* Markers represent a location where content can be placed. At a minimum, they specify a position, size, and
 * user-specified category. They can also have arbitrary meta-data in the form of key-value pairs of strings. */

using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace AKSaigyouji.AtlasGeneration
{
    [Serializable]
    public sealed class RawMarker
    {
        public Vector2 Position { get { return position; } set { position = value; } }
        public Vector2 Size { get { return size; } set { size = value; } }

        public string Category { get { return category; } }
        public string Preset { get { return preset; } }
        public MetaData MetaData { get { return metaData; } }

        [SerializeField] string category;
        [SerializeField] string preset;
        [SerializeField] Vector2 position;
        [SerializeField] Vector2 size;
        [SerializeField] MetaData metaData;

        public RawMarker(Vector2 position, Vector2 size, string category = "", string preset = "", MetaData metaData = null)
        {
            if (category == null)
                throw new ArgumentNullException("category");
            if (preset == null)
                throw new ArgumentNullException("preset");

            this.position = position;
            this.category = category;
            this.preset = preset;
            this.size = size;
            this.metaData = metaData;
        }
    }
}