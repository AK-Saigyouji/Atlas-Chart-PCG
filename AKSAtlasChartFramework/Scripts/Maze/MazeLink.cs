using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace AKSaigyouji.AtlasGeneration
{
    /// <summary>
    /// Represents a link/connection/opening between two points in a maze. Note that
    /// the MazeLink between a and b is equal to the MazeLink between b and a.
    /// </summary>
    [Serializable]
    public struct MazeLink : IEquatable<MazeLink>
    {
        public Coord CellA { get { return a; } }
        public Coord CellB { get { return b; } }

        public bool IsHorizontal { get { return CellA.y == CellB.y; } }
        public bool IsVertical { get { return CellA.x == CellB.x; } }

        [SerializeField, HideInInspector] Coord a;
        [SerializeField, HideInInspector] Coord b;
        
        public MazeLink(Coord a, Coord b)
        {
            if (a.SquaredDistance(b) != 1)
                throw new ArgumentException("Coordinates must be adjacent (horizontally or vertically) in a link.");

            this.a = a;
            this.b = b;
        }

        public bool ConnectsTo(Coord cell)
        {
            return a == cell || b == cell;
        }

        public override string ToString()
        {
            return string.Format("Link between {0} and {1}.", a, b);
        }

        public override int GetHashCode()
        {
            // Need to provide a symmetric hash code to match with the symmetric equality.
            int leftHash = 391 + a.GetHashCode();
            leftHash = leftHash * 23 + b.GetHashCode();

            int rightHash = 391 + b.GetHashCode();
            rightHash = rightHash * 23 + a.GetHashCode();
            return leftHash + rightHash;
        }

        public override bool Equals(object obj)
        {
            return obj is MazeLink && this == (MazeLink)obj;
        }

        public bool Equals(MazeLink other)
        {
            return this == other;
        }

        public static bool operator ==(MazeLink x, MazeLink y)
        {
            return (x.a == y.a && x.b == y.b) || (x.a == y.b && x.b == y.a);
        }
        public static bool operator !=(MazeLink x, MazeLink y)
        {
            return !(x == y);
        }
    } 
}