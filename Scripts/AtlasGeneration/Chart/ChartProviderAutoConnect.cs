using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using AKSaigyouji.Maps;
using AKSaigyouji.Modules.MapGeneration;

namespace AKSaigyouji.AtlasGeneration
{
    /// <summary>
    /// A chart provider that automatically carves openings into the sides of randomly chosen charts
    /// so that they line up with each other.
    /// </summary>
    [CreateAssetMenu(fileName = FILE_NAME, menuName = ASSET_PATH + "Auto Connector")]
    public sealed class ChartProviderAutoConnect : ChartProviderModule
    {
        public override int Seed { get { return seed; } set { seed = value; } }

        public ChartStatic[] FillerCharts
        {
            get { return fillerCharts; }
            set
            {
                if (value == null) throw new ArgumentNullException();
                if (value.Any(ch => ch == null)) throw new ArgumentException("Null chart in chart array.");
                fillerCharts = value.ToArray();
            }
        }

        public ChartStatic[] EntranceCharts
        {
            get { return entranceCharts; }
            set
            {
                if (value == null) throw new ArgumentNullException();
                if (value.Any(ch => ch == null)) throw new ArgumentException("Null chart in chart array.");
                entranceCharts = value.ToArray();
            }
        }

        public ChartStatic[] ExitCharts
        {
            get { return exitCharts; }
            set
            {
                if (value == null) throw new ArgumentNullException();
                if (value.Any(ch => ch == null)) throw new ArgumentException("Null chart in chart array.");
                exitCharts = value.ToArray();
            }
        }

        public override int ChartLength { get { return textureLength; } }
        public override int ChartWidth { get { return textureLength; } }

        /// <summary>
        /// Openings carved into the chart will have length given by this number. Must be at least 1.
        /// </summary>
        public int OpeningLength
        {
            get { return openingLength; }
            set
            {
                if (value < 1) throw new ArgumentOutOfRangeException("value", "Must be at least 1.");
                openingLength = value;
            }
        }

        [Tooltip("One chart will be chosen from this collection for the beginning of the maze.")]
        [SerializeField] ChartStatic[] entranceCharts;

        [Tooltip("One chart will be chosen from this collection for the end of the maze.")]
        [SerializeField] ChartStatic[] exitCharts;

        [Tooltip("The remaining charts will be drawn randomly from this collection.")]
        [SerializeField] ChartStatic[] fillerCharts;

        [SerializeField] int textureLength;
        [SerializeField] int textureWidth;

        [Tooltip("Length of opening carved into chunks.")]
        [SerializeField] int openingLength = 3;
        [SerializeField] int seed;

        System.Random random;

        public static ChartProviderAutoConnect Construct(int length, int width)
        {
            var provider = CreateInstance<ChartProviderAutoConnect>();
            provider.textureLength = length;
            provider.textureWidth = width;
            return provider;
        }

        public override Dictionary<Coord, RawChart> GetCharts(Maze maze)
        {
            ValidateCharts();
            random = new System.Random(seed);
            Coord[] cells = maze.GetCells();
            var chartsByCoord = new Dictionary<Coord, RawChart>();
            
            for (int i = 0; i < cells.Length; i++)
            {
                Coord cell = cells[i];
                ChartStatic[] candidateCharts = DetermineChartCandidates(maze, cell);
                RawChart chart = candidateCharts[random.Next(0, candidateCharts.Length)];
                ChartStatic carvedChart = CarveEntrancesIntoChart(chart, maze, cell);
                chartsByCoord[cell] = carvedChart;
            }
            return chartsByCoord;
        }

        ChartStatic[] DetermineChartCandidates(Maze maze, Coord cell)
        {
            if (maze.HasInfo(cell))
            {
                string cellInfo = maze.GetInfo(cell);
                if (cellInfo == "start")
                {
                    return entranceCharts;
                }
                else if (cellInfo == "end")
                {
                    return exitCharts;
                }
            }
            return fillerCharts;
        }

        ChartStatic CarveEntrancesIntoChart(RawChart chart, Maze maze, Coord coord)
        {
            IEnumerable<MapEntrance> entrances = GetEntrances(maze, coord);
            chart.Seed = seed;
            var staticModule = MapGenStaticMap.Construct(chart.Map);
            var carvedModule = MapGenEntranceCarver.Construct(staticModule, entrances);
            return ChartStatic.ReplaceMap(carvedModule.Generate(seed), chart);
        }

        IEnumerable<MapEntrance> GetEntrances(Maze maze, Coord coord)
        {
            if (maze.IsBottomOpen(coord)) yield return BuildMapEntrance(BoundaryPoint.Side.Bottom);
            if (maze.IsTopOpen(coord))    yield return BuildMapEntrance(BoundaryPoint.Side.Top);
            if (maze.IsLeftOpen(coord))   yield return BuildMapEntrance(BoundaryPoint.Side.Left);
            if (maze.IsRightOpen(coord))  yield return BuildMapEntrance(BoundaryPoint.Side.Right);
        }

        void ValidateCharts()
        {
            if (fillerCharts == null || fillerCharts.Length == 0)
                throw new InvalidOperationException("No charts");

            if (fillerCharts.Any(chart => chart == null))
                throw new InvalidOperationException("Null chart");

            if (fillerCharts.Any(chart => chart.Texture == null))
                throw new InvalidOperationException("Chart with null map");

            if (fillerCharts.Select(chart => chart.Texture).Any(map => map.width != textureLength || map.height != textureWidth))
                throw new InvalidOperationException("Charts with maps of inconsistent size.");
        }

        MapEntrance BuildMapEntrance(BoundaryPoint.Side side)
        {
            return new MapEntrance(side, (textureLength - openingLength) / 2, openingLength);
        }

        void OnValidate()
        {
            textureLength = Mathf.Max(0, textureLength);
            textureWidth = Mathf.Max(0, textureWidth);
            openingLength = Mathf.Max(1, openingLength);
        }
    }
}