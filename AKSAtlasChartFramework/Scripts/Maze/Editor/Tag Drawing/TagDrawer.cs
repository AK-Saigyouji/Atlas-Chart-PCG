using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace AKSaigyouji.AtlasGeneration
{
    /// <summary>
    /// Object responsible for drawing a tag (from Maze metadata) onto a cell in a texture.
    /// </summary>
    [CreateAssetMenu(fileName = "Tag Drawer", menuName = "AKSaigyouji/Maze Generators/Tag Drawer")]
    public sealed class TagDrawer : ScriptableObject
    {
        public IEnumerable<Coord> Coords { get { return coords; } }
        public Color Color { get { return color; } }

        [SerializeField] Coord[] coords;
        [SerializeField] Color color = new Color(177f / 255, 129f / 255, 129f / 255); // Desaturated red

        public static TagDrawer Construct(IEnumerable<Coord> coords, Color color)
        {
            var tagDrawer = CreateInstance<TagDrawer>();
            tagDrawer.coords = coords.ToArray();
            tagDrawer.color = color;
            return tagDrawer;
        }

        void OnValidate()
        {
            if (coords == null)
                return;

            for (int i = 0; i < coords.Length; i++)
            {
                coords[i] = new Coord(Mathf.Clamp(coords[i].x, 1, 9), Mathf.Clamp(coords[i].y, 1, 9));
            }
        }
    } 
}