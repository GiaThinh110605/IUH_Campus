using System;
using System.Collections.Generic;

namespace IUHCampus.Guardian.Match3
{
    public enum TileType
    {
        None = 0,
        Sword = 1,
        Shield = 2,
        Energy = 3,
        Heal = 4,
        Purify = 5
    }

    public readonly struct BoardCoord : IEquatable<BoardCoord>
    {
        public readonly int X;
        public readonly int Y;

        public BoardCoord(int x, int y)
        {
            X = x;
            Y = y;
        }

        public bool Equals(BoardCoord other) => X == other.X && Y == other.Y;

        public override bool Equals(object obj) => obj is BoardCoord other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(X, Y);

        public override string ToString() => $"({X},{Y})";

        public static bool operator ==(BoardCoord left, BoardCoord right) => left.Equals(right);

        public static bool operator !=(BoardCoord left, BoardCoord right) => !left.Equals(right);
    }

    public class MatchGroup
    {
        public TileType Type { get; }
        public List<BoardCoord> Coords { get; }

        public MatchGroup(TileType type, List<BoardCoord> coords)
        {
            Type = type;
            Coords = coords;
        }
    }

    public struct TileDrop
    {
        public BoardCoord From;
        public BoardCoord To;
        public TileType Type;

        public TileDrop(BoardCoord from, BoardCoord to, TileType type)
        {
            From = from;
            To = to;
            Type = type;
        }
    }

    public struct TileSpawn
    {
        public BoardCoord Destination;
        public TileType Type;

        public TileSpawn(BoardCoord destination, TileType type)
        {
            Destination = destination;
            Type = type;
        }
    }

    public class CascadeStep
    {
        public List<MatchGroup> Matches { get; }
        public List<TileDrop> Drops { get; }
        public List<TileSpawn> Spawns { get; }
        public int SwordCount { get; set; }
        public int ShieldCount { get; set; }
        public int EnergyCount { get; set; }
        public int HealCount { get; set; }
        public int PurifyCount { get; set; }

        /// <summary>0 for the match made by the swap, 1+ for each cascade after it.</summary>
        public int ComboIndex { get; set; }

        /// <summary>Length of the longest single line in this step (3, 4, 5+).</summary>
        public int LongestMatch
        {
            get
            {
                int longest = 0;
                foreach (var group in Matches)
                {
                    if (group.Coords.Count > longest) longest = group.Coords.Count;
                }
                return longest;
            }
        }

        public CascadeStep(List<MatchGroup> matches, List<TileDrop> drops, List<TileSpawn> spawns)
        {
            Matches = matches;
            Drops = drops;
            Spawns = spawns;
        }
    }
}

