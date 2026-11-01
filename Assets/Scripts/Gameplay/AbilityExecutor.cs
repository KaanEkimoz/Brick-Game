using System;
using System.Collections.Generic;
using Board;
using Extras;
using InGame;
using Piece;
using Tiles;
using UnityEngine;

namespace Gameplay
{
    /// <summary>
    /// Listens for piece landings; if the landed piece carries an AbilityMarker, fires the
    /// corresponding board effect from the charged tile and awards bonus score for the tiles
    /// it changed. Runs before CheckLineClears, so rows it completes score as normal line clears.
    /// </summary>
    public class AbilityExecutor : MonoBehaviour
    {
        [SerializeField] private BoardController _boardController;
        [SerializeField] private ScoreController _scoreController;

        // Per-tile base points; multiplied by (level + 1) like the line-clear formula.
        private const int PointsPerTile = 5;

        /// <summary>Holes one Patch fills, lowest first.</summary>
        public const int PatchCellCount = 6;

        private readonly List<Vector2Int> _patchCells = new List<Vector2Int>();

        public static Action<Vector2Int> OnBombExploded;
        public static Action<Vector2Int> OnRowExploded;       // anchor cell of the cleared row
        public static Action<Vector2Int> OnColumnExploded;    // anchor cell of the cleared column
        public static Action<AbilityType> OnAbilityUsed;

        /// <summary>Fires after every ability hit with (anchor cell, cells destroyed).
        /// Separate from the per-shape Bomb/Row/Column events because those drive VFX
        /// (already wired to particle prefabs) and adding a count parameter would
        /// break their existing signature. The "+N cells!" satisfaction popup hooks
        /// here. For Patch the count is cells filled. Not fired when the count is 0.</summary>
        public static Action<Vector2Int, int> OnAbilityCellsCleared;

        private void Awake()
        {
            gameObject.AddComponent<AbilityTargetPreview>();
        }

        private void OnEnable()
        {
            PieceMovement.OnPieceSettled += HandleSettle;
        }

        private void OnDisable()
        {
            PieceMovement.OnPieceSettled -= HandleSettle;
        }

        private void HandleSettle()
        {
            GameObject piece = PiecesController.CurPiece;
            if (piece == null) return;

            AbilityMarker marker = piece.GetComponent<AbilityMarker>();
            if (marker == null) return;

            if (_boardController == null || PieceController.Tiles == null || PieceController.Tiles.Length == 0)
                return;

            TileController chargedTile = PieceController.Tiles[marker.TileIndex];
            Vector2Int anchor = chargedTile.coordinates;
            int cleared = 0;
            switch (marker.Ability)
            {
                case AbilityType.Bomb:
                    cleared = _boardController.ClearBoxCells(anchor.x, anchor.y, 1);
                    OnBombExploded?.Invoke(anchor);
                    break;
                case AbilityType.Dynamite:
                {
                    Vector2Int other = PieceController.Tiles[1].coordinates;
                    Vector2Int min = Vector2Int.Min(anchor, other) - Vector2Int.one;
                    Vector2Int max = Vector2Int.Max(anchor, other) + Vector2Int.one;
                    cleared = _boardController.ClearRectCells(min, max);
                    OnBombExploded?.Invoke(anchor);
                    OnBombExploded?.Invoke(other);
                    break;
                }
                case AbilityType.Laser:
                {
                    LaserOrientation orientation = piece.GetComponent<LaserOrientation>();
                    bool horizontal = orientation == null || orientation.IsHorizontal;
                    if (horizontal)
                    {
                        cleared = _boardController.ClearRowCells(anchor.y);
                        OnRowExploded?.Invoke(anchor);
                    }
                    else
                    {
                        cleared = _boardController.ClearColumnCells(anchor.x);
                        OnColumnExploded?.Invoke(anchor);
                    }
                    break;
                }
                case AbilityType.Patch:
                {
                    // The charged tile stays on the board, so drop its tint and clone a sibling.
                    chargedTile.GetComponent<SpriteRenderer>().color = Color.white;
                    int templateIndex = (marker.TileIndex + 1) % PieceController.Tiles.Length;
                    _boardController.CollectPatchCells(PatchCellCount, null, _patchCells);
                    _boardController.FillCells(_patchCells, PieceController.Tiles[templateIndex].gameObject);
                    cleared = _patchCells.Count;

                    // Nothing to fill: leave the bar one clear short so the ability is not wasted.
                    if (cleared == 0 && AbilityCharger.Instance != null)
                        AbilityCharger.Instance.RestoreCharge(AbilityCharger.MaxCharge - 1);
                    break;
                }
            }

            OnAbilityUsed?.Invoke(marker.Ability);

            if (cleared > 0 && _scoreController != null)
                _scoreController.AddBonusScore(cleared * PointsPerTile * (LevelController.CurrentLevel + 1));

            if (cleared > 0)
                OnAbilityCellsCleared?.Invoke(anchor, cleared);
        }
    }
}
