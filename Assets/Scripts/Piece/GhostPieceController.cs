using InGame;
using Tiles;
using UnityEngine;
namespace Piece
{
    public class GhostPieceController : MonoBehaviour
    {
        [Space]
        public GhostTileController[] ghostTiles;
        private void OnEnable()
        {
            PieceSpawner.OnPieceSpawned += UpdateGhostTiles;
            PieceMovement.OnPieceMovement += UpdateGhostTiles;
            PieceRotation.OnPieceRotation += UpdateGhostTiles;
        }
        private void OnDisable()
        {
            PieceSpawner.OnPieceSpawned -= UpdateGhostTiles;
            PieceMovement.OnPieceMovement -= UpdateGhostTiles;
            PieceRotation.OnPieceRotation -= UpdateGhostTiles;
        }
        private void UpdateGhostTiles()
        {
            // Extended-mode ability pieces carry fewer tiles than the ghost's four; skip the
            // preview and park the unused ghost tiles off-screen so they don't linger.
            if (PieceController.Tiles == null || PieceController.Tiles.Length < ghostTiles.Length)
            {
                foreach (var gt in ghostTiles)
                    gt.UpdatePosition(new Vector2Int(-100, -100));
                return;
            }

            for (int i = 1; i <= ghostTiles.Length; i++)
            {
                TileController tile = PieceController.Tiles[i - 1];
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
