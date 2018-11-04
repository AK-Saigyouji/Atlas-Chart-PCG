using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using AKSaigyouji.Maps;
using AKSaigyouji.Modules;

namespace AKSaigyouji.AtlasGeneration
{
    public abstract class ChartProviderModule : Module
    {
        protected const string FILE_NAME = "Chart Provider";
        protected const string ASSET_PATH = MODULE_ASSET_PATH + "Chart Providers/";

        /// <summary>
        /// Assigns a chart to each cell in the maze.
        /// </summary>
        public abstract Dictionary<Coord, RawChart> GetCharts(Maze maze);

        public abstract int ChartLength { get; }
        public abstract int ChartWidth { get; }
    } 
}