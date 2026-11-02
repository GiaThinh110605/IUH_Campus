using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace IUHCampus.Guardian.Match3
{
    public class Match3BoardView : MonoBehaviour
    {
        [Header("Visual Sprites")]
        [SerializeField] private Sprite m_SpriteSword;
        [SerializeField] private Sprite m_SpriteShield;
        [SerializeField] private Sprite m_SpriteEnergy;
        [SerializeField] private Sprite m_SpriteHeal;
        [SerializeField] private Sprite m_SpriteTileBg;
        [SerializeField] private Sprite m_SpriteTileSelected;

        [Header("Layout Settings")]
        [SerializeField] private RectTransform m_GridRoot;
        [SerializeField] private float m_TileSize = 74f;
        [SerializeField] private float m_TileSpacing = 6f;

        [Header("Animation Durations")]
        [SerializeField] private float m_SwapDuration = 0.18f;
        [SerializeField] private float m_PopDuration = 0.15f;
        [SerializeField] private float m_DropDuration = 0.22f;

        private Match3BoardModel m_Model;
        private readonly Match3TileView[,] m_Views = new Match3TileView[Match3BoardModel.Width, Match3BoardModel.Height];
        private readonly Queue<Match3TileView> m_TilePool = new Queue<Match3TileView>();

        private BoardCoord? m_SelectedCoord;
        private GameObject m_SelectionHighlight;
        private bool m_IsBusy;

        public event Action<CascadeStep> OnStepResolved;
        public event Action OnTurnCompleted;

        public bool IsBusy => m_IsBusy;
        public Match3BoardModel Model => m_Model;

        public void Initialize(Match3BoardModel model)
        {
            m_Model = model;
            CreateSelectionHighlight();
            RebuildGridVisuals();
        }

        private void CreateSelectionHighlight()
        {
            if (m_SelectionHighlight != null) return;
            if (m_GridRoot == null) m_GridRoot = GetComponent<RectTransform>();

            m_SelectionHighlight = new GameObject("SelectionHighlight", typeof(RectTransform), typeof(Image));
            m_SelectionHighlight.transform.SetParent(m_GridRoot, false);
            var rect = m_SelectionHighlight.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(m_TileSize + 8f, m_TileSize + 8f);

            var img = m_SelectionHighlight.GetComponent<Image>();
            img.sprite = m_SpriteTileSelected;
            img.raycastTarget = false;
            m_SelectionHighlight.SetActive(false);
        }

        public void RebuildGridVisuals()
        {
            ClearExistingViews();

            for (int x = 0; x < Match3BoardModel.Width; x++)
            {
                for (int y = 0; y < Match3BoardModel.Height; y++)
                {
                    TileType type = m_Model.GetTile(x, y);
                    BoardCoord coord = new BoardCoord(x, y);
                    Match3TileView view = SpawnTileView(coord, type);
                    if (view != null)
                    {
                        var rt = view.RectTransform ?? view.GetComponent<RectTransform>();
                        if (rt != null) rt.anchoredPosition = CoordToLocalPos(coord);
                    }
                    m_Views[x, y] = view;
                }
            }
        }

        private void ClearExistingViews()
        {
            for (int x = 0; x < Match3BoardModel.Width; x++)
            {
                for (int y = 0; y < Match3BoardModel.Height; y++)
                {
                    if (m_Views[x, y] != null)
                    {
                        m_Views[x, y].gameObject.SetActive(false);
                        m_TilePool.Enqueue(m_Views[x, y]);
                        m_Views[x, y] = null;
                    }
                }
            }
        }

        private Match3TileView SpawnTileView(BoardCoord coord, TileType type)
        {
            Match3TileView view;
            if (m_TilePool.Count > 0)
            {
                view = m_TilePool.Dequeue();
                view.gameObject.SetActive(true);
            }
            else
            {
                GameObject go = new GameObject($"Tile_{coord.X}_{coord.Y}", typeof(RectTransform), typeof(Image), typeof(Match3TileView));
                go.transform.SetParent(m_GridRoot, false);
                view = go.GetComponent<Match3TileView>();
            }

            var rt = view.RectTransform ?? view.GetComponent<RectTransform>();
            if (rt != null) rt.sizeDelta = new Vector2(m_TileSize, m_TileSize);
            view.Setup(coord, type, GetSpriteForType(type), m_SpriteTileBg, OnTileSwiped, OnTileClicked);
            return view;
        }

        public Vector2 CoordToLocalPos(BoardCoord coord)
        {
            float totalWidth = (Match3BoardModel.Width * m_TileSize) + ((Match3BoardModel.Width - 1) * m_TileSpacing);
            float totalHeight = (Match3BoardModel.Height * m_TileSize) + ((Match3BoardModel.Height - 1) * m_TileSpacing);

            float startX = -totalWidth * 0.5f + (m_TileSize * 0.5f);
            float startY = -totalHeight * 0.5f + (m_TileSize * 0.5f);

            float posX = startX + coord.X * (m_TileSize + m_TileSpacing);
            float posY = startY + coord.Y * (m_TileSize + m_TileSpacing);

            return new Vector2(posX, posY);
        }

        public Sprite GetSpriteForType(TileType type)
        {
            switch (type)
            {
                case TileType.Sword: return m_SpriteSword;
                case TileType.Shield: return m_SpriteShield;
                case TileType.Energy: return m_SpriteEnergy;
                case TileType.Heal: return m_SpriteHeal;
                default: return null;
            }
        }

        private void OnTileClicked(BoardCoord coord)
        {
            if (m_IsBusy) return;

            if (!m_SelectedCoord.HasValue)
            {
                SelectTile(coord);
                return;
            }

            BoardCoord first = m_SelectedCoord.Value;
            if (first == coord)
            {
                DeselectTile();
                return;
            }

            if (m_Model.AreAdjacent(first, coord))
            {
                DeselectTile();
                TryPerformSwap(first, coord);
            }
            else
            {
                SelectTile(coord);
            }
        }

        private void OnTileSwiped(BoardCoord coord, Vector2 direction)
        {
            if (m_IsBusy) return;

            DeselectTile();
            int targetX = coord.X + Mathf.RoundToInt(direction.x);
            int targetY = coord.Y + Mathf.RoundToInt(direction.y);

            if (!m_Model.IsValidCoord(targetX, targetY)) return;

            BoardCoord targetCoord = new BoardCoord(targetX, targetY);
            TryPerformSwap(coord, targetCoord);
        }

        private void SelectTile(BoardCoord coord)
        {
            m_SelectedCoord = coord;
            if (m_SelectionHighlight != null)
            {
                m_SelectionHighlight.SetActive(true);
                m_SelectionHighlight.GetComponent<RectTransform>().anchoredPosition = CoordToLocalPos(coord);
                m_SelectionHighlight.transform.SetAsLastSibling();
            }
        }

        private void DeselectTile()
        {
            m_SelectedCoord = null;
            if (m_SelectionHighlight != null)
            {
                m_SelectionHighlight.SetActive(false);
            }
        }

        public void TryPerformSwap(BoardCoord a, BoardCoord b)
        {
            if (m_IsBusy) return;
            StartCoroutine(ExecuteSwapSequence(a, b));
        }

        private IEnumerator ExecuteSwapSequence(BoardCoord a, BoardCoord b)
        {
            m_IsBusy = true;

            Match3TileView viewA = m_Views[a.X, a.Y];
            Match3TileView viewB = m_Views[b.X, b.Y];

            Vector2 posA = CoordToLocalPos(a);
            Vector2 posB = CoordToLocalPos(b);

            Coroutine moveA = StartCoroutine(viewA.AnimateMoveTo(posB, m_SwapDuration));
            Coroutine moveB = StartCoroutine(viewB.AnimateMoveTo(posA, m_SwapDuration));

            yield return moveA;
            yield return moveB;

            bool isValid = m_Model.CanSwap(a, b);

            if (!isValid)
            {
                Coroutine backA = StartCoroutine(viewA.AnimateMoveTo(posA, m_SwapDuration));
                Coroutine backB = StartCoroutine(viewB.AnimateMoveTo(posB, m_SwapDuration));
                yield return backA;
                yield return backB;
                m_IsBusy = false;
                yield break;
            }

            m_Model.SwapDirect(a, b);
            m_Views[a.X, a.Y] = viewB;
            m_Views[b.X, b.Y] = viewA;
            viewA.UpdateCoord(b);
            viewB.UpdateCoord(a);

            yield return StartCoroutine(ResolveCascadesRoutine());

            m_IsBusy = false;
            OnTurnCompleted?.Invoke();
        }

        private IEnumerator ResolveCascadesRoutine()
        {
            while (true)
            {
                CascadeStep step = m_Model.ProcessStep();
                if (step == null) break;

                List<Coroutine> popCoroutines = new List<Coroutine>();
                foreach (var group in step.Matches)
                {
                    foreach (var coord in group.Coords)
                    {
                        Match3TileView v = m_Views[coord.X, coord.Y];
                        if (v != null && v.gameObject.activeSelf)
                        {
                            popCoroutines.Add(StartCoroutine(v.AnimatePopAndFade(m_PopDuration)));
                            m_Views[coord.X, coord.Y] = null;
                            m_TilePool.Enqueue(v);
                        }
                    }
                }

                foreach (var c in popCoroutines) yield return c;

                OnStepResolved?.Invoke(step);

                List<Coroutine> dropCoroutines = new List<Coroutine>();
                foreach (var drop in step.Drops)
                {
                    Match3TileView v = m_Views[drop.From.X, drop.From.Y];
                    if (v != null)
                    {
                        m_Views[drop.To.X, drop.To.Y] = v;
                        m_Views[drop.From.X, drop.From.Y] = null;
                        v.UpdateCoord(drop.To);
                        dropCoroutines.Add(StartCoroutine(v.AnimateMoveTo(CoordToLocalPos(drop.To), m_DropDuration)));
                    }
                }

                foreach (var spawn in step.Spawns)
                {
                    Match3TileView newView = SpawnTileView(spawn.Destination, spawn.Type);
                    m_Views[spawn.Destination.X, spawn.Destination.Y] = newView;
                    dropCoroutines.Add(StartCoroutine(newView.AnimateSpawn(CoordToLocalPos(spawn.Destination), m_DropDuration)));
                }

                foreach (var c in dropCoroutines) yield return c;

                yield return new WaitForSeconds(0.06f);
            }

            if (!m_Model.HasValidMoves())
            {
                m_Model.Reshuffle();
                RebuildGridVisuals();
            }
        }
    }
}

