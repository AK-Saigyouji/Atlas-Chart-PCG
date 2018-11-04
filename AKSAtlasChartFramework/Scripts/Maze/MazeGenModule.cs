using System;
using UnityEngine;
using AKSaigyouji.Modules;

namespace AKSaigyouji.AtlasGeneration
{
    /// <summary>
    /// Base class for maze generator modules. Derive from this to implement your own. Benefits from
    /// an inspector that will automatically attempt to draw the maze in the preview window.
    /// </summary>
    public abstract class MazeGenModule : Module
    {
        public abstract Maze GetMaze();

        /// <summary>
        /// The string associated with the entrance cell(s).
        /// </summary>
        public virtual string EntranceTag { get { return "Entrance"; } }

        /// <summary>
        /// The string associated with the exit cell(s).
        /// </summary>
        public virtual string ExitTag { get { return "Exit"; } }

        protected const string DEFAULT_FILE_NAME = "Maze Generator";
        protected const string MAZE_GEN_ASSET_PATH = MODULE_ASSET_PATH + "Maze Generators/";
    }
}