using Board;
using InGame;
using Piece;
using Tiles;
using UnityEngine;

namespace Gameplay
{
    /// <summary>
    /// Shows the board area a charged piece will hit if it drops from its current position.
    /// Added at runtime by AbilityExecutor, so it needs no scene wiring. A single stretched
    /// sprite covers the target rectangle.
    /// </summary>
    public class AbilityTargetPreview : MonoBehaviour
    {
        private const int SortingOrder = 8; // one above the tiles
        private static readonly Color AreaColor = new Color(1f, 1f, 1f, 0.22f);

        private SpriteRenderer _area;
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
            if (_area != null) _area.enabled = false;
        }

        private void Refresh()
        {
            if (_marker == null)
            {
                if (_area != null) _area.enabled = false;
                return;
            }

            TileController chargedTile = PieceController.Tiles[_marker.TileIndex];
            Vector2Int landing = chargedTile.coordinates + Vector2Int.down * GetDropDistance();
            GetTargetBounds(landing, out Vector2Int min, out Vector2Int max);

            EnsureArea(chargedTile);
            Vector2 center = (Vector2)(min + max) * 0.5f;
            _area.transform.position = BoardController.CellToWorld(center.x, center.y);
            _area.transform.localScale = new Vector3(max.x - min.x + 1, max.y - min.y + 1, 1f);
            _area.enabled = true;
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
                case AbilityType.Laser:
                    min = new Vector2Int(0, landing.y);
                    max = new Vector2Int(boardMax.x, landing.y);
                    break;
                default:
                    min = new Vector2Int(0, landing.y - AbilityExecutor.PatchRowCount + 1);
                    max = new Vector2Int(boardMax.x, landing.y);
                    break;
            }

            min = Vector2Int.Max(min, Vector2Int.zero);
            max = Vector2Int.Min(max, boardMax);
        }

        private void EnsureArea(TileController tile)
        {
            if (_area != null) return;

            GameObject go = new GameObject("AbilityTargetArea");
            go.transform.SetParent(transform, false);
            _area = go.AddComponent<SpriteRenderer>();
            _area.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
            // Share the tile material so the overlay renders through the same URP path.
            _area.sharedMaterial = tile.GetComponent<SpriteRenderer>().sharedMaterial;
            _area.sortingLayerID = tile.GetComponent<SpriteRenderer>().sortingLayerID;
            _area.sortingOrder = SortingOrder;
            _area.color = AreaColor;
        }
    }
}
