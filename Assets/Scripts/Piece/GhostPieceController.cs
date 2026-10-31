using InGame;
using Tiles;
using UnityEngine;
namespace Piece
{
    public class GhostPieceController : MonoBehaviour
    {
        public static bool Hidden;

        [Space]
        public GhostTileController[] ghostTiles;
        private void OnEnable()
        {
            PieceSpawner.OnPieceSpawned += UpdateGhostTiles;
            PieceSpawner.OnPieceCharged += UpdateGhostTiles;
            PieceMovement.OnPieceMovement += UpdateGhostTiles;
            PieceRotation.OnPieceRotation += UpdateGhostTiles;
        }
        private void OnDisable()
        {
            PieceSpawner.OnPieceSpawned -= UpdateGhostTiles;
            PieceSpawner.OnPieceCharged -= UpdateGhostTiles;
            PieceMovement.OnPieceMovement -= UpdateGhostTiles;
            PieceRotation.OnPieceRotation -= UpdateGhostTiles;
        }
        private void UpdateGhostTiles()
        {
            // No active piece tiles, or a level hides the ghost: park the ghost tiles off-screen.
            if (Hidden || PieceController.Tiles == null || PieceController.Tiles.Length == 0)
            {
                foreach (var gt in ghostTiles)
                    gt.UpdatePosition(new Vector2Int(-100, -100));
                return;
            }

            for (int i = 1; i <= ghostTiles.Length; i++)
            {
                // A single-tile bomb stacks all the ghost tiles on its one cell.
                TileController tile = PieceController.Tiles[Mathf.Min(i - 1, PieceController.Tiles.Length - 1)];
                // Read the tile's cell coordinate directly instead of casting its world
                // transform.position to int. Now that BoardController.WorldOrigin is a
                // non-zero (and non-integer) offset, the old int-cast turned the world
                // position into the wrong grid cell — making the ghost lag the live piece
                // by WorldOrigin.x / WorldOrigin.y on each axis (e.g. couldn't reach the
                // left columns, overshot the right).
                ghostTiles[i - 1].UpdatePosition(tile.coordinates);
            }
            SendGhostPieceToFloor();
        }
        private void SendGhostPieceToFloor()
        {
            while(MoveGhostPiece(Vector2Int.down)) {}
        }
        private bool MoveGhostPiece(Vector2Int movement)
        {
            foreach (var tile in ghostTiles)
            {
                if (!tile.CanTileMove(movement+ tile.coordinates))
                {
                    return false;
                }
            }
            foreach (var tile in ghostTiles)
            {
                tile.MoveTile(movement);
            }
            return true;
        }
    }
}
