using System;
using Board;
using Extras;
using InGame;
using Piece;
using UnityEngine;

namespace Gameplay
{
    /// <summary>
    /// Listens for piece landings; if the landed piece carries an AbilityMarker, fires the
    /// corresponding board-clear effect and awards bonus score for the tiles it destroyed.
    /// </summary>
    public class AbilityExecutor : MonoBehaviour
    {
        [SerializeField] private BoardController _boardController;
        [SerializeField] private ScoreController _scoreController;

        // Per-tile base points; multiplied by (level + 1) like the line-clear formula.
        private const int PointsPerTile = 5;

        public static Action<Vector2Int> OnBombExploded;
        public static Action<Vector2Int> OnRowExploded;       // anchor cell of the cleared row
        public static Action<Vector2Int> OnColumnExploded;    // anchor cell of the cleared column

        /// <summary>Fires after every ability hit with (anchor cell, cells destroyed).
        /// Separate from the per-shape Bomb/Row/Column events because those drive VFX
        /// (already wired to particle prefabs) and adding a count parameter would
        /// break their existing signature. The "+N cells!" satisfaction popup hooks
        /// here. Not fired when cleared == 0 (ability landed on empty board area).</summary>
        public static Action<Vector2Int, int> OnAbilityCellsCleared;

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

            Vector2Int anchor = PieceController.Tiles[0].coordinates;
            int cleared = 0;
            switch (marker.Ability)
            {
                case AbilityType.Bomb:
                    cleared = _boardController.ClearBoxCells(anchor.x, anchor.y, 1);
                    OnBombExploded?.Invoke(anchor);
                    break;
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
            }

            if (cleared > 0 && _scoreController != null)
                _scoreController.AddBonusScore(cleared * PointsPerTile * (LevelController.CurrentLevel + 1));

            if (cleared > 0)
                OnAbilityCellsCleared?.Invoke(anchor, cleared);
        }
    }
}
