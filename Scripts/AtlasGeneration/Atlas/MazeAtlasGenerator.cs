/* This class provides the plumbing for the various modules involved in the generation of a maze-style atlas. This is
 a style of map generation found in many ARPGs, such as Diablo 2 and Path of Exile. Charts represent a block/subsection
 of the final map. The cells of a maze determine the number and location of each block relative to other blocks, 
 and the connections (links) between cells indicate which blocks should connect to their neighbours. The chart provider
 maps cells (performing any required processing tasks on the charts, if needed) to charts. A content strategy is then
 responsible for converting markers to content.
 */

using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using AKSaigyouji.Maps;
using AKSaigyouji.Modules.MapGeneration;

using Random = System.Random;

namespace AKSaigyouji.AtlasGeneration
{ 
    public sealed class MazeAtlasGenerator : MonoBehaviour
    {
        public ChartProviderModule ChartProvider
        {
            get { return chartProvider; }
            set { if (value == null) throw new ArgumentNullException("value"); chartProvider = value; }
        }

        public MazeGenModule MazeGenerator
        {
            get { return mazeGenerator; }
            set { if (value == null) throw new ArgumentNullException("value"); mazeGenerator = value; }
        }

		public int Seed { get { return seed; } set { seed = value; } }

        [SerializeField] ChartProviderModule chartProvider;
        [SerializeField] MazeGenModule mazeGenerator;

        [SerializeField] int seed;

        Random random;

        public Atlas Generate(int seed)
        {
            this.seed = seed;
            return Generate();
        }

        public Atlas Generate()
        {
            random = new Random(seed);
            mazeGenerator.Seed = random.Next(int.MinValue, int.MaxValue);

            Maze maze = mazeGenerator.GetMaze();
            Coord[] cells = maze.GetCells();
            Coord[] offsets = GetOffsets(cells, chartProvider.ChartLength, chartProvider.ChartWidth);
            RawChart[] charts = ExtractCharts(maze, chartProvider, cells);
            Map map = BuildMap(charts, offsets, seed);
            Chart[] processedCharts = ProcessCharts(charts, offsets);
            Atlas atlas = new Atlas(processedCharts, map);
            return atlas;
        }

        static RawChart[] ExtractCharts(Maze maze, ChartProviderModule chartProvider, Coord[] cells)
        {
            Dictionary<Coord, RawChart> chartTable = chartProvider.GetCharts(maze);
            RawChart[] charts = cells.Select(cell => chartTable[cell]).ToArray();
            return charts;
        }

        static Map BuildMap(RawChart[] charts, Coord[] offsets, int seed)
        {
            var modules = charts.Select(chart => MapGenStaticMap.Construct(chart.Map)).ToArray();
            var compoundModule = MapGenCompound.Construct(modules, offsets.Select(coord => (Vector2)coord));
            Map map = compoundModule.Generate(seed);
            return map;
        }

        static Chart[] ProcessCharts(RawChart[] charts, Coord[] offsets)
        {
            return Enumerable.Range(0, charts.Length)
                             .Select(i => new Chart(charts[i], offsets[i]))
                             .ToArray();
        }

        static Coord[] GetOffsets(Coord[] cells, int lengthScale, int widthScale)
        {
            int xMin = cells.Min(coord => coord.x);
            int yMin = cells.Min(coord => coord.y);
            var offsets = cells.Select(coord => new Coord(coord.x - xMin, coord.y - yMin))
                               .Select(c => new Coord(c.x * lengthScale, c.y * widthScale));
            return offsets.ToArray();
        }
    } 
}