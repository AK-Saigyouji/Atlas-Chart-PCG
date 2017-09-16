/* This algorithm for generating mazes is based loosely on how Path of Exile appears to do its dungeons. First, a grid
 of random weights is generated, then a random path is taken between two points on this grid. This yields a list of 
 coordinates. Links are generated between each consecutive pair of steps. Next, additional coordinates are generated
 adjacent to randomly chosen points in this path until an appropriate number of points is obtained.
 
  Start and end cells are marked based on the first and last points in the random path initially generated. These are 
 marked with the strings "start" and "end" in the maze.*/

using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using AKSaigyouji.ArrayExtensions;
using AKSaigyouji.Maps;

using Random = System.Random;

namespace AKSaigyouji.AtlasGeneration
{
    [CreateAssetMenu(fileName = DEFAULT_FILE_NAME, menuName = MAZE_GEN_ASSET_PATH + "Branching Weighted Path (BWP)")]
    public sealed class MazeGenBranchingWeightedPath : MazeGenModule
    {
        /// <summary>
        /// Must be at least 1.
        /// </summary>
        public int GridLength
        {
            get { return gridLength; }
            set { if (value < 1) throw new ArgumentOutOfRangeException(); gridLength = value; }
        }
        [Tooltip("Increase to get a more horizontal path.")]
        [SerializeField] int gridLength;

        /// <summary>
        /// Must be at least 1.
        /// </summary>
        public int GridWidth
        {
            get { return gridWidth; }
            set { if (value < 1) throw new ArgumentOutOfRangeException(); gridWidth = value; }
        }
        [Tooltip("Increase to get a more vertical path.")]
        [SerializeField] int gridWidth;

        /// <summary>
        /// Must not be null or empty. No restrictions on values.
        /// </summary>
        public int[] GridWeights
        {
            get { return gridWeights; }
            set { if (value == null) throw new ArgumentNullException("value"); gridWeights = value; }
        }
        [Tooltip("Randomly assigned grid weights when determining initial (pre-branching) path.")]
        [SerializeField] int[] gridWeights = new int[] { 1, 20, 10000 };

        /// <summary>
        /// Must be at least 1.
        /// </summary>
        public int CellCount
        {
            get { return cellCount; }
            set { if (value < 1) throw new ArgumentOutOfRangeException(); cellCount = value; }
        }
        [Tooltip("Will continue branching until this number of cells is obtained.")]
        [SerializeField] int cellCount;

        /// <summary>
        /// Must be between 0 (inclusive) and 1 (inclusive).
        /// </summary>
        public float ExtraLinkProportion
        {
            get { return extraLinkProportion; }
            set
            {
                if (extraLinkProportion < 0 || 1 < extraLinkProportion) throw new ArgumentOutOfRangeException();
                extraLinkProportion = value;
            }
        }
        [Range(0f, 1f)]
        [Tooltip("Beyond the minimum needed for connectivity, what approximate proportion of adjacent cells should be connected?")]
        [SerializeField] float extraLinkProportion = 0.33f;

        public override int Seed { get { return seed; } set { seed = value; } }
        [SerializeField] int seed;

        public override Maze GetMaze()
        {
            Random random = new Random(seed);
            int[,] weights = BuildRandomWeights(random, gridLength, gridWidth, gridWeights);
            List<Coord> path = BuildInitialPath(random, weights, cellCount);
            Coord start = path[0];
            Coord end = path[path.Count - 1];
            List<MazeLink> links = Enumerable.Range(0, path.Count - 1)
                                             .Select(i => new MazeLink(path[i], path[i + 1]))
                                             .ToList();

            ExpandPath(path, links, random, cellCount, extraLinkProportion);
            var cellInfo = new KeyValuePair<Coord, string>[]
            {
                new KeyValuePair<Coord, string>(start, "start"),
                new KeyValuePair<Coord, string>(end, "end")
            };
            Maze maze = new Maze(path, links, cellInfo); 
            return maze;
        }

        static List<Coord> BuildInitialPath(Random random, int[,] weights, int maxLength)
        {
            // in order to give the path room to walk around, the start and end points are taken
            // between (length / 3, width / 3) and (2 * length / 3, 2 * width / 3)
            int left = weights.GetLength(0) / 3;
            int right = 2 * left;
            int bottom = weights.GetLength(1) / 3;
            int top = 2 * bottom;
            Coord start = new Coord(left, random.Next(bottom, top));
            Coord end = new Coord(right, random.Next(bottom, top));
            if (random.NextDouble() > 0.5f) // swap start/end half the time
            {
                Coord temp = start;
                start = end;
                end = temp;
            }
            if (start == end)
            {
                return new List<Coord>() { start };
            }
            var pathFinder = new ShortestPath();
            return pathFinder.FindPath(weights, start, end).Take(maxLength).ToList();
        }

        static void ExpandPath(List<Coord> path, List<MazeLink> links, Random random, int desiredRoomCount, float linkThreshold)
        {
            int coordsLeft = desiredRoomCount - path.Count;
            var directions = new Coord[] { new Coord(0, 1), new Coord(1, 0), new Coord(0, -1), new Coord(-1, 0) };
            while (coordsLeft > 0)
            {
                Shuffle(directions, random);
                // note: this random selection is very fast for path sizes of standard use cases, but scales poorly
                // for very large sizes. If trying to optimize for a non-standard use case with very large paths, 
                // improve the way the branch coord is chosen.
                Coord branchCoord = path[random.Next(1, path.Count - 1)];
                Coord openNeighbour;
                var neighbours = directions.Select(dir => dir + branchCoord);
                if (TryGetOpenNeighbour(neighbours, path, out openNeighbour))
                {
                    coordsLeft--;
                    var link = new MazeLink(branchCoord, openNeighbour);
                    // Insert into the second last spot, so the last coord doesn't change.
                    path.Insert(path.Count - 1, openNeighbour);
                    links.Add(link);
                    // The new neighbour may be adjacent to existing cells in the maze. We randomly select
                    // those cells and open links to them, with probability based on linkThreshold
                    var coordsAdjacentToOpenNeighbour = directions.Select(dir => dir + openNeighbour)
                                                                  .Where(coord => coord != branchCoord)
                                                                  .Where(coord => coord != path[0] && coord != path[path.Count - 1])
                                                                  .Where(coord => random.NextDouble() < linkThreshold)
                                                                  .Where(path.Contains);
                    foreach (Coord coordAdjacentToOpenNeighbour in coordsAdjacentToOpenNeighbour)
                    {
                        links.Add(new MazeLink(openNeighbour, coordAdjacentToOpenNeighbour));
                    }
                }
            }
        }

        static bool TryGetOpenNeighbour(IEnumerable<Coord> neighbours, List<Coord> path, out Coord openNeighbour)
        {
            foreach (Coord neighbour in neighbours)
            {
                if (!path.Contains(neighbour))
                {
                    openNeighbour = neighbour;
                    return true;
                }
            }
            openNeighbour = Coord.zero;
            return false;
        }

        static int[,] BuildRandomWeights(Random random, int length, int width, int[] gridWeights)
        {
            int[,] weights = new int[3 * length, 3 * width];

            weights.Transform((x, y) => weights[x, y] + PickRandom(gridWeights, random));

            return weights;
        }

        static T PickRandom<T>(IList<T> list, Random random)
        {
            return list[random.Next(0, list.Count)];
        }

        static void Shuffle<T>(IList<T> list, Random random)
        {
            for (int i = 0; i < list.Count - 1; i++)
            {
                Swap(list, i, random.Next(i, list.Count));
            }
        }

        static void Swap<T>(IList<T> list, int a, int b)
        {
            T temp = list[a];
            list[a] = list[b];
            list[b] = temp;
        }

        void OnValidate()
        {
            gridLength = Mathf.Max(1, gridLength);
            gridWidth = Mathf.Max(1, gridWidth);
            cellCount = Mathf.Max(1, cellCount);
        }
    }
}