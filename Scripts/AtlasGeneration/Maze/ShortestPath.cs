using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using AKSaigyouji.Maps;
using AKSaigyouji.DataStructures;

namespace AKSaigyouji.AtlasGeneration
{
    public sealed class ShortestPath
    {
        PriorityQueue<PathNode> q = new PriorityQueue<PathNode>();
        Dictionary<Coord, Coord> parents = new Dictionary<Coord, Coord>();
        HashSet<Coord> visited = new HashSet<Coord>();

        /// <summary>
        /// Finds the shortest path from start to end (inclusive), where the distance function is given 
        /// by the weights: stepping into a coordinate costs a weight given by the value in that cell.
        /// </summary>
        public IEnumerable<Coord> FindPath(int[,] weights, Coord start, Coord end)
        {
            if (weights == null)
                throw new ArgumentNullException("weights");

            var boundary = new Boundary(weights.GetLength(0), weights.GetLength(1));
            if (!boundary.IsInBounds(start) || !boundary.IsInBounds(end))
            {
                throw new ArgumentOutOfRangeException(string.Format
                    ("Coordinates out of range:\nStart: {0}\nEnd: {1}\nWeight Boundary: {2}", start, end, boundary));
            }

            int length = weights.GetLength(0);
            int width = weights.GetLength(1);

            q.Clear();
            parents.Clear();
            visited.Clear();

            q.Enqueue(new PathNode(start, 0));
            while (q.Count > 0)
            {
                PathNode currentNode = q.Dequeue();
                Coord currentCoord = currentNode.coord;
                long currentWeight = currentNode.weight;

                if (currentCoord == end)
                {
                    return RecoverPath(parents, start, end);
                }

                foreach (Coord neighbour in GetNeighbours(currentCoord))
                {
                    int x = neighbour.x, y = neighbour.y;
                    if (0 <= x && x < length && 0 <= y && y < width && !visited.Contains(neighbour))
                    {
                        q.Enqueue(new PathNode(neighbour, currentWeight + weights[x, y]));
                        visited.Add(neighbour);
                        parents[neighbour] = currentCoord;
                    }
                }
            }
            // this should never happen
            throw new InvalidOperationException("Internal error. Path-finding algorithm failed.");
        }

        static IEnumerable<Coord> RecoverPath(Dictionary<Coord, Coord> parents, Coord start, Coord end)
        {
            var path = new List<Coord>();
            path.Add(end);
            Coord next = parents[end];
            while (next != start)
            {
                path.Add(next);
                next = parents[next];
            }
            path.Add(start);
            path.Reverse();
            return path;
        }

        static IEnumerable<Coord> GetNeighbours(Coord coord)
        {
            yield return coord.LeftShift;
            yield return coord.RightShift;
            yield return coord.UpShift;
            yield return coord.DownShift;
        }

        struct PathNode : IComparable<PathNode>
        {
            public readonly Coord coord;
            public readonly long weight; 

            public PathNode(Coord coord, long weight)
            {
                this.coord = coord;
                this.weight = weight;
            }

            public int CompareTo(PathNode other)
            {
                return weight.CompareTo(other.weight);
            }
        }
    } 
}