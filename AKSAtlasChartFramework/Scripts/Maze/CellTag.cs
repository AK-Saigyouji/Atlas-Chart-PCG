/* The main reason this exists is because of the lack of generic serialization. This is a serializable substitute for 
 * KeyValuePair<Coord, string> meant to be used to carry metadata for maze cells. 
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AKSaigyouji.AtlasGeneration
{
    [System.Serializable]
    public struct CellTag
    {
        public Coord Cell { get { return cell; } }
        public MetaData MetaData { get { return tag; } }

        /// <summary>
        /// The number of elements in the tag. Equivalent to Tag.Count.
        /// </summary>
        public int Count { get { return tag.Count; } }

        [SerializeField] Coord cell;
        [SerializeField] MetaData tag;

        public CellTag(Coord cell, MetaData tag)
        {
            if (tag == null)
                throw new System.ArgumentNullException("tag");

            this.cell = cell;
            this.tag = tag;
        }

        public CellTag(Coord cell, IEnumerable<string> tags)
        {
            this.cell = cell;
            this.tag = new MetaData(tags);
        }

        public CellTag(Coord cell, string tag)
        {
            this.cell = cell;
            this.tag = new MetaData(tag);
        }

        public CellTag DeepCopy()
        {
            return new CellTag(cell, new MetaData(tag));
        }

        public IEnumerable<string> GetAllKeys()
        {
            return tag.Select(pair => pair.Key);
        }

        public static explicit operator KeyValuePair<Coord, MetaData>(CellTag cellTag)
        {
            return new KeyValuePair<Coord, MetaData>(cellTag.cell, cellTag.tag);
        }

        public static explicit operator CellTag(KeyValuePair<Coord, MetaData> pair)
        {
            return new CellTag(pair.Key, pair.Value);
        }
    }
}