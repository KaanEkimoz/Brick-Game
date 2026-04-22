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
            PieceRotation.OnPieceRotation += UpdateGhostTiles;
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
                var tilePos = tile.transform.position;
                Vector2Int newPos = new Vector2Int((int)tilePos.x, (int) tilePos.y);
                ghostTiles[i - 1].UpdatePosition(newPos);
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
