using System;
using System.Collections.Generic;

namespace IUHCampus.Guardian.Match3
{
    public class Match3BoardModel
    {
        public const int Width = 6;
        public const int Height = 5;
        private static readonly TileType[] PlayableTypes =
        {
            TileType.Sword,
            TileType.Shield,
            TileType.Energy,
            TileType.Heal,
            TileType.Purify
        };

        public static IReadOnlyList<TileType> Playable => PlayableTypes;

        private readonly TileType[,] m_Grid = new TileType[Width, Height];
        private readonly Random m_Random;

        public Match3BoardModel(int? seed = null)
        {
            m_Random = seed.HasValue ? new Random(seed.Value) : new Random();
        }

        public TileType GetTile(int x, int y)
        {
            if (!IsValidCoord(x, y)) return TileType.None;
            return m_Grid[x, y];
        }

        public void SetTile(int x, int y, TileType type)
        {
            if (IsValidCoord(x, y)) m_Grid[x, y] = type;
        }

        public bool IsValidCoord(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;

        public bool AreAdjacent(BoardCoord a, BoardCoord b)
        {
            int dx = Math.Abs(a.X - b.X);
            int dy = Math.Abs(a.Y - b.Y);
            return (dx == 1 && dy == 0) || (dx == 0 && dy == 1);
        }

        public void Initialize()
        {
            int attempts = 0;
            do
            {
                FillWithoutMatches();
                attempts++;
            } while (!HasValidMoves() && attempts < 100);
        }

        private void FillWithoutMatches()
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    List<TileType> available = new List<TileType>(PlayableTypes);

                    if (x >= 2 && m_Grid[x - 1, y] == m_Grid[x - 2, y])
                    {
                        available.Remove(m_Grid[x - 1, y]);
                    }

                    if (y >= 2 && m_Grid[x, y - 1] == m_Grid[x, y - 2])
                    {
                        available.Remove(m_Grid[x, y - 1]);
                    }

                    m_Grid[x, y] = available[m_Random.Next(available.Count)];
                }
            }
        }

        public bool CanSwap(BoardCoord a, BoardCoord b)
        {
            if (!IsValidCoord(a.X, a.Y) || !IsValidCoord(b.X, b.Y)) return false;
            if (!AreAdjacent(a, b)) return false;

            SwapDirect(a, b);
            bool hasMatch = FindMatches().Count > 0;
            SwapDirect(a, b);

            return hasMatch;
        }

        public void SwapDirect(BoardCoord a, BoardCoord b)
        {
            TileType temp = m_Grid[a.X, a.Y];
            m_Grid[a.X, a.Y] = m_Grid[b.X, b.Y];
            m_Grid[b.X, b.Y] = temp;
        }

        public List<MatchGroup> FindMatches()
        {
            HashSet<BoardCoord> matchedCoords = new HashSet<BoardCoord>();
            List<MatchGroup> groups = new List<MatchGroup>();

            for (int y = 0; y < Height; y++)
            {
                int matchLength = 1;
                for (int x = 1; x <= Width; x++)
                {
                    if (x < Width && m_Grid[x, y] == m_Grid[x - 1, y] && m_Grid[x, y] != TileType.None)
                    {
                        matchLength++;
                    }
                    else
                    {
                        if (matchLength >= 3)
                        {
                            List<BoardCoord> coords = new List<BoardCoord>();
                            TileType type = m_Grid[x - 1, y];
                            for (int k = x - matchLength; k < x; k++)
                            {
                                BoardCoord c = new BoardCoord(k, y);
                                coords.Add(c);
                                matchedCoords.Add(c);
                            }
                            groups.Add(new MatchGroup(type, coords));
                        }
                        matchLength = 1;
                    }
                }
            }

            for (int x = 0; x < Width; x++)
            {
                int matchLength = 1;
                for (int y = 1; y <= Height; y++)
                {
                    if (y < Height && m_Grid[x, y] == m_Grid[x, y - 1] && m_Grid[x, y] != TileType.None)
                    {
                        matchLength++;
                    }
                    else
                    {
                        if (matchLength >= 3)
                        {
                            List<BoardCoord> coords = new List<BoardCoord>();
                            TileType type = m_Grid[x, y - 1];
                            for (int k = y - matchLength; k < y; k++)
                            {
                                BoardCoord c = new BoardCoord(x, k);
                                coords.Add(c);
                                matchedCoords.Add(c);
                            }
                            groups.Add(new MatchGroup(type, coords));
                        }
                        matchLength = 1;
                    }
                }
            }

            return groups;
        }

        public CascadeStep ProcessStep()
        {
            List<MatchGroup> matches = FindMatches();
            if (matches.Count == 0) return null;

            bool hasMegaBomb = false;
            foreach (var group in matches)
            {
                if (group.Coords.Count >= 5)
                {
                    group.IsMegaBomb = true;
                    hasMegaBomb = true;

                    // 1. Radial splash explosion (3x3 area around every matched tile in the 5-match)
                    foreach (var c in group.Coords)
                    {
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            for (int dy = -1; dy <= 1; dy++)
                            {
                                int nx = c.X + dx;
                                int ny = c.Y + dy;
                                if (IsValidCoord(nx, ny) && m_Grid[nx, ny] != TileType.None)
                                {
                                    BoardCoord bc = new BoardCoord(nx, ny);
                                    if (!group.Coords.Contains(bc) && !group.BlastCoords.Contains(bc))
                                    {
                                        group.BlastCoords.Add(bc);
                                    }
                                }
                            }
                        }
                    }

                    // 2. Candy Crush Color Bomb: Clear all remaining tiles of this same type across the entire board!
                    for (int bx = 0; bx < Width; bx++)
                    {
                        for (int by = 0; by < Height; by++)
                        {
                            if (m_Grid[bx, by] == group.Type)
                            {
                                BoardCoord bc = new BoardCoord(bx, by);
                                if (!group.Coords.Contains(bc) && !group.BlastCoords.Contains(bc))
                                {
                                    group.BlastCoords.Add(bc);
                                }
                            }
                        }
                    }
                }
            }

            HashSet<BoardCoord> uniqueMatched = new HashSet<BoardCoord>();
            List<BoardCoord> allBlasted = new List<BoardCoord>();
            int swords = 0;
            int shields = 0;
            int energies = 0;
            int heals = 0;
            int purifies = 0;

            foreach (var group in matches)
            {
                // Process main match coords
                foreach (var coord in group.Coords)
                {
                    if (uniqueMatched.Add(coord))
                    {
                        TileType t = m_Grid[coord.X, coord.Y];
                        switch (t)
                        {
                            case TileType.Sword: swords++; break;
                            case TileType.Shield: shields++; break;
                            case TileType.Energy: energies++; break;
                            case TileType.Heal: heals++; break;
                        }
                    }
                }

                // Process mega-bomb blast wave coords
                foreach (var blastCoord in group.BlastCoords)
                {
                    if (uniqueMatched.Add(blastCoord))
                    {
                        allBlasted.Add(blastCoord);
                        TileType t = m_Grid[blastCoord.X, blastCoord.Y];
                        switch (t)
                        {
                            case TileType.Sword: swords++; break;
                            case TileType.Shield: shields++; break;
                            case TileType.Energy: energies++; break;
                            case TileType.Heal: heals++; break;
                            case TileType.Purify: purifies++; break;
                        }
                    }
                }
            }

            foreach (var coord in uniqueMatched)
            {
                m_Grid[coord.X, coord.Y] = TileType.None;
            }

            List<TileDrop> drops = new List<TileDrop>();
            List<TileSpawn> spawns = new List<TileSpawn>();

            for (int x = 0; x < Width; x++)
            {
                int emptyCount = 0;
                for (int y = 0; y < Height; y++)
                {
                    if (m_Grid[x, y] == TileType.None)
                    {
                        emptyCount++;
                    }
                    else if (emptyCount > 0)
                    {
                        TileType moving = m_Grid[x, y];
                        int newY = y - emptyCount;
                        m_Grid[x, newY] = moving;
                        m_Grid[x, y] = TileType.None;
                        drops.Add(new TileDrop(new BoardCoord(x, y), new BoardCoord(x, newY), moving));
                    }
                }

                for (int i = 0; i < emptyCount; i++)
                {
                    int targetY = Height - emptyCount + i;
                    TileType newType = PlayableTypes[m_Random.Next(PlayableTypes.Length)];
                    m_Grid[x, targetY] = newType;
                    spawns.Add(new TileSpawn(new BoardCoord(x, targetY), newType));
                }
            }

            CascadeStep step = new CascadeStep(matches, drops, spawns)
            {
                SwordCount = swords,
                ShieldCount = shields,
                EnergyCount = energies,
                HealCount = heals,
                PurifyCount = purifies,
                HasMegaBomb = hasMegaBomb
            };
            step.AllBlastedCoords.AddRange(allBlasted);

            return step;
        }

        public bool HasValidMoves()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (x < Width - 1 && CanSwap(new BoardCoord(x, y), new BoardCoord(x + 1, y)))
                    {
                        return true;
                    }
                    if (y < Height - 1 && CanSwap(new BoardCoord(x, y), new BoardCoord(x, y + 1)))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void Reshuffle()
        {
            List<TileType> existing = new List<TileType>();
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    existing.Add(m_Grid[x, y]);
                }
            }

            int attempts = 0;
            do
            {
                for (int i = existing.Count - 1; i > 0; i--)
                {
                    int swapIdx = m_Random.Next(i + 1);
                    TileType tmp = existing[i];
                    existing[i] = existing[swapIdx];
                    existing[swapIdx] = tmp;
                }

                int idx = 0;
                for (int x = 0; x < Width; x++)
                {
                    for (int y = 0; y < Height; y++)
                    {
                        m_Grid[x, y] = existing[idx++];
                    }
                }
                attempts++;
            } while ((FindMatches().Count > 0 || !HasValidMoves()) && attempts < 100);
        }
    }
}

