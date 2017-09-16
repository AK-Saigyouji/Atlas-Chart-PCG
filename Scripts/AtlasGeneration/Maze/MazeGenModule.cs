using AKSaigyouji.Modules;

namespace AKSaigyouji.AtlasGeneration
{
    /// <summary>
    /// Base class for maze generator modules. Derive from this to implement your own.
    /// </summary>
    public abstract class MazeGenModule : Module
    {
        protected const string DEFAULT_FILE_NAME = "Maze Generator";
        protected const string MAZE_GEN_ASSET_PATH = MODULE_ASSET_PATH + "Maze Generators/";

        public abstract Maze GetMaze();
    }
}