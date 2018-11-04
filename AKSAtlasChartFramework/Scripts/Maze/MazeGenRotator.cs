using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AKSaigyouji.AtlasGeneration
{
    [CreateAssetMenu(fileName = "Maze Rotator", menuName = MAZE_GEN_ASSET_PATH + "Rotator (Wrapper)")]
    public sealed class MazeGenRotator : MazeGenModule
    {
        public MazeGenModule mazeGenerator;
        [Header("Enabled Symmetries:")]
        public bool Identity = true;
        public bool ClockwiseNinety = true;
        public bool ClockwiseOneEighty = true;
        public bool ClockwiseTwoSeventy = true;
        public bool FlipAcrossHorizontal = true;
        public bool FlipAcrossVertical = true;
        public bool FlipAcrossBottomLeftToTopRight = true;
        public bool FlipAcrossTopLeftToBottomRight = true;

        [SerializeField, HideInInspector] int seed;

        /// <summary>
        /// Set the seed. If more than one symmetry is enabled, one will be chosen at random upon
        /// calling GetMaze. 
        /// </summary>
        public override int Seed
        {
            get { return seed; }
            set { seed = value; }
        }

        /// <summary>
        /// Create a maze gen rotator with the given symmetries enabled.
        /// </summary>
        public static MazeGenRotator GetRotator(Symmetry symmetries)
        {
            if (symmetries == Symmetry.None)
                throw new System.ArgumentException("Rotator must have at least one symmetry enabled.");

            var rotator = CreateInstance<MazeGenRotator>();
            rotator.SetSymmetries(symmetries, true);
            return rotator;
        }

        public override Maze GetMaze()
        {
            if (mazeGenerator == null)
                throw new System.InvalidOperationException("No maze generator assigned.");

            var random = new System.Random(seed);

            var enabledMotions = new List<RigidMotion>(8);
            if (Identity)
                enabledMotions.Add(new RigidMotion());
            if (ClockwiseNinety)
                enabledMotions.Add(new RigidMotion(1, false));
            if (ClockwiseOneEighty)
                enabledMotions.Add(new RigidMotion(2, false));
            if (ClockwiseTwoSeventy)
                enabledMotions.Add(new RigidMotion(3, false));
            if (FlipAcrossBottomLeftToTopRight)
                enabledMotions.Add(new RigidMotion(0, true));
            if (FlipAcrossTopLeftToBottomRight)
                enabledMotions.Add(new RigidMotion(2, true));
            if (FlipAcrossHorizontal)
                enabledMotions.Add(new RigidMotion(3, true));
            if (FlipAcrossVertical)
                enabledMotions.Add(new RigidMotion(1, true));

            if (enabledMotions.Count == 0)
                throw new System.InvalidOperationException("Must enable at least one symmetry.");

            RigidMotion chosenMotion = enabledMotions[random.Next(0, enabledMotions.Count)];
            return chosenMotion.Apply(mazeGenerator.GetMaze());
        }

        public void SetSymmetries(Symmetry symmetries, bool value)
        {
            if ((symmetries & Symmetry.ClockwiseNinety) != 0)
                ClockwiseNinety = value;
            if ((symmetries & Symmetry.ClockwiseOneEighty) != 0)
                ClockwiseOneEighty = value;
            if ((symmetries & Symmetry.ClockwiseTwoSeventy) != 0)
                ClockwiseTwoSeventy = value;
            if ((symmetries & Symmetry.FlipAcrossBottomLeftToTopRight) != 0)
                FlipAcrossBottomLeftToTopRight = value;
            if ((symmetries & Symmetry.FlipAcrossHorizontal) != 0)
                FlipAcrossHorizontal = value;
            if ((symmetries & Symmetry.FlipAcrossTopRightToBottomRight) != 0)
                FlipAcrossTopLeftToBottomRight = value;
            if ((symmetries & Symmetry.FlipAcrossVertical) != 0)
                FlipAcrossVertical = value;
            if ((symmetries & Symmetry.Identity) != 0)
                Identity = value;
        }

        /// <summary>
        /// Another way to characterize a rigid motion: number of clockwise rotations (0-3) and 
        /// whether there's a flip along the main diagonal (along the bottomleft to topright diagonal).
        /// </summary>
        struct RigidMotion
        {
            public byte rotations;
            public bool flip;

            public RigidMotion(int rotations, bool flip)
            {
                this.rotations = (byte)rotations;
                this.flip = flip;
            }

            public Maze Apply(Maze maze)
            {
                var cells = maze.GetCells();
                var links = maze.GetLinks();
                var tags = maze.GetTags();

                int xMax = cells.Max(c => c.x);
                int yMax = cells.Max(c => c.y);
                for (int rotation = 0; rotation < rotations; rotation++)
                {
                    for (int cellIndex = 0; cellIndex < cells.Length; cellIndex++)
                    {
                        cells[cellIndex] = RotateCW(cells[cellIndex], xMax);
                    }
                    for (int linkIndex = 0; linkIndex < links.Length; linkIndex++)
                    {
                        links[linkIndex] = RotateCW(links[linkIndex], xMax);
                    }
                    for (int tagIndex = 0; tagIndex < tags.Length; tagIndex++)
                    {
                        tags[tagIndex] = RotateCW(tags[tagIndex], xMax);
                    }
                    int temp = xMax;
                    xMax = yMax;
                    yMax = temp;
                }
                if (flip)
                {
                    for (int cellIndex = 0; cellIndex < cells.Length; cellIndex++)
                    {
                        cells[cellIndex] = Flip(cells[cellIndex]);
                    }
                    for (int linkIndex = 0; linkIndex < links.Length; linkIndex++)
                    {
                        links[linkIndex] = Flip(links[linkIndex]);
                    }
                    for (int tagIndex = 0; tagIndex < tags.Length; tagIndex++)
                    {
                        tags[tagIndex] = Flip(tags[tagIndex]);
                    }
                }
                return new Maze(cells, links, tags);
            }

            Coord RotateCW(Coord coord, int xMax)
            {
                return new Coord(coord.y, xMax - coord.x);
            }

            MazeLink RotateCW(MazeLink link, int xMax)
            {
                return new MazeLink(RotateCW(link.CellA, xMax), RotateCW(link.CellB, xMax));
            }

            CellTag RotateCW(CellTag tag, int xMax)
            {
                return new CellTag(RotateCW(tag.Cell, xMax), tag.MetaData);
            }

            Coord Flip(Coord coord)
            {
                return new Coord(coord.y, coord.x);
            }

            MazeLink Flip(MazeLink link)
            {
                return new MazeLink(Flip(link.CellA), Flip(link.CellB));
            }

            CellTag Flip(CellTag tag)
            {
                return new CellTag(Flip(tag.Cell), tag.MetaData);
            }
        }
    }

    [System.Flags]
    public enum Symmetry
    {
        None = 0,
        ClockwiseNinety = 1,
        ClockwiseOneEighty = 2,
        ClockwiseTwoSeventy = 4,
        FlipAcrossHorizontal = 8,
        FlipAcrossVertical = 16,
        FlipAcrossBottomLeftToTopRight = 32,
        FlipAcrossTopRightToBottomRight = 64,
        // A separate non-zero flag for identity is necessary since we want to be able to include/exclude it.
        Identity = 128
    }
}