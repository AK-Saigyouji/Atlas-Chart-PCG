using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace AKSaigyouji.AtlasGeneration
{
    [CreateAssetMenu(fileName = "Maze", menuName = MAZE_GEN_ASSET_PATH + "Static")]
    public sealed class StaticMaze : MazeGenModule
    {
        public IEnumerable<Coord> Cells { get { return cells; } }
        public IEnumerable<MazeLink> Links { get { return links; } }
        public IEnumerable<CellTag> CellTags { get { return cellTags; } }

        [SerializeField, HideInInspector] int length = 6;
        [SerializeField, HideInInspector] int width = 6;

        [SerializeField, HideInInspector] List<Coord> cells = new List<Coord>() { Coord.zero };
        [SerializeField, HideInInspector] List<MazeLink> links = new List<MazeLink>();
        [SerializeField, HideInInspector] List<CellTag> cellTags = new List<CellTag>();

        public override Maze GetMaze()
        {
            return new Maze(cells, links, cellTags);
        }

        public void ReplaceContents(IEnumerable<Coord> newCells, IEnumerable<MazeLink> newLinks, IEnumerable<CellTag> newTags)
        {
            if (newCells == null)
                throw new ArgumentNullException("newCells");
            if (newLinks == null)
                throw new ArgumentNullException("newLinks");

            cells = newCells.ToList();
            links = newLinks.ToList();
            cellTags = newTags != null ? newTags.ToList() : cellTags = new List<CellTag>(0);
        }

        void OnValidate()
        {
            length = Mathf.Max(1, length);
            width = Mathf.Max(1, width);
        }
    }
}