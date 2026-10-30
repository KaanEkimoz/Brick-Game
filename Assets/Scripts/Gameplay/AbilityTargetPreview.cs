using System.Collections.Generic;
using Board;
using InGame;
using Piece;
using Tiles;
using UnityEngine;

namespace Gameplay
{
    /// <summary>
    /// Shows the board area a charged piece will hit if it drops from its current position.
    /// Added at runtime by AbilityExecutor, so it needs no scene wiring. One stretched sprite
    /// covers a Bomb or Laser target; Patch marks each hole it will fill.
    /// </summary>
    public class AbilityTargetPreview : MonoBehaviour
    {
        private const int SortingOrder = 8; // one above the tiles
        private static readonly Color AreaColor = new Color(1f, 1f, 1f, 0.22f);
        private static readonly Color PatchCellColor = new Color(0.45f, 1f, 0.55f, 0.6f);

        private readonly List<SpriteRenderer> _shapes = new List<SpriteRenderer>();
        private readonly List<Vector2Int> _landingTiles = new List<Vector2Int>();
        private readonly List<Vector2Int> _patchCells = new List<Vector2Int>();
        private Sprite _shapeSprite;
        private AbilityMarker _marker;

        private void OnEnable()
        {
            PieceSpawner.OnPieceSpawned += HandlePieceChanged;
            PieceSpawner.OnPieceCharged += HandlePieceChanged;
            PieceMovement.OnPieceMovement += Refresh;
            PieceRotation.OnPieceRotation += Refresh;
            PiecesController.OnGameOver += Hide;
        }

        private void OnDisable()
        {
            PieceSpawner.OnPieceSpawned -= HandlePieceChanged;
            PieceSpawner.OnPieceCharged -= HandlePieceChanged;
            PieceMovement.OnPieceMovement -= Refresh;
            PieceRotation.OnPieceRotation -= Refresh;
            PiecesController.OnGameOver -= Hide;
        }

        private void HandlePieceChanged()
        {
            _marker = PiecesController.CurPiece != null
                ? PiecesController.CurPiece.GetComponent<AbilityMarker>()
                : null;
            Refresh();
        }

        private void Hide()
        {
            _marker = null;
            HideShapesFrom(0);
        }

        private void Refresh()
        {
            if (_marker == null)
            {
                HideShapesFrom(0);
                return;
            }

            TileController chargedTile = PieceController.Tiles[_marker.TileIndex];
            int dropDistance = GetDropDistance();

            if (_marker.Ability == AbilityType.Patch)
            {
                ShowPatchCells(chargedTile, dropDistance);
                return;
            }

            GetTargetBounds(chargedTile.coordinates + Vector2Int.down * dropDistance, out Vector2Int min, out Vector2Int max);
            ShowRect(0, chargedTile, min, max, AreaColor);
            HideShapesFrom(1);
        }

        private void ShowPatchCells(TileController chargedTile, int dropDistance)
        {
            _landingTiles.Clear();
            foreach (TileController tile in PieceController.Tiles)
                _landingTiles.Add(tile.coordinates + Vector2Int.down * dropDistance);

            BoardController.Instance.CollectPatchCells(AbilityExecutor.PatchCellCount, _landingTiles, _patchCells);
            for (int i = 0; i < _patchCells.Count; i++)
                ShowRect(i, chargedTile, _patchCells[i], _patchCells[i], PatchCellColor);
            HideShapesFrom(_patchCells.Count);
        }

        private void ShowRect(int index, TileController tile, Vector2Int min, Vector2Int max, Color color)
        {
            SpriteRenderer shape = GetShape(index, tile);
            Vector2 center = (Vector2)(min + max) * 0.5f;
            shape.transform.position = BoardController.CellToWorld(center.x, center.y);
            shape.transform.localScale = new Vector3(max.x - min.x + 1, max.y - min.y + 1, 1f);
            shape.color = color;
            shape.enabled = true;
        }

        private void HideShapesFrom(int index)
        {
            for (int i = index; i < _shapes.Count; i++)
                _shapes[i].enabled = false;
        }

        private static int GetDropDistance()
        {
            int distance = 0;
            while (CanDrop(distance + 1)) distance++;
            return distance;
        }

        private static bool CanDrop(int distance)
        {
            foreach (TileController tile in PieceController.Tiles)
            {
                if (!tile.CanTileMove(tile.coordinates + Vector2Int.down * distance))
                    return false;
            }
            return true;
        }

        private void GetTargetBounds(Vector2Int landing, out Vector2Int min, out Vector2Int max)
        {
            BoardController board = BoardController.Instance;
            Vector2Int boardMax = new Vector2Int(board.gridSizeX - 1, board.gridSizeY - 1);

            switch (_marker.Ability)
            {
                case AbilityType.Bomb:
                    min = landing - Vector2Int.one;
                    max = landing + Vector2Int.one;
                    break;
                case AbilityType.Laser when _marker.TryGetComponent(out LaserOrientation laser) && !laser.IsHorizontal:
                    min = new Vector2Int(landing.x, 0);
                    max = new Vector2Int(landing.x, boardMax.y);
                    break;
                default:
                    min = new Vector2Int(0, landing.y);
                    max = new Vector2Int(boardMax.x, landing.y);
                    break;
            }

            min = Vector2Int.Max(min, Vector2Int.zero);
            max = Vector2Int.Min(max, boardMax);
        }

        private SpriteRenderer GetShape(int index, TileController tile)
        {
            if (_shapeSprite == null)
                _shapeSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);

            while (_shapes.Count <= index)
            {
                GameObject go = new GameObject("AbilityTargetShape");
                go.transform.SetParent(transform, false);
                SpriteRenderer shape = go.AddComponent<SpriteRenderer>();
                shape.sprite = _shapeSprite;
                // Share the tile material so the overlay renders through the same URP path.
                shape.sharedMaterial = tile.GetComponent<SpriteRenderer>().sharedMaterial;
                shape.sortingLayerID = tile.GetComponent<SpriteRenderer>().sortingLayerID;
                shape.sortingOrder = SortingOrder;
                _shapes.Add(shape);
            }
            return _shapes[index];
        }
    }
}
