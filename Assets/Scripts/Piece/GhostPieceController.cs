using UnityEngine;

namespace Piece
{
    public class GhostPieceController : MonoBehaviour
    {
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
            for (int i = 1; i <= ghostTiles.Length; i++)
            {
                if(PieceController.Tiles[i-1] == null)
                    Destroy(ghostTiles[i-1]);
                Vector2Int newPos = new Vector2Int((int) PieceController.Tiles[i - 1].gameObject.transform.position.x,
                    (int) PieceController.Tiles[i - 1].gameObject.transform.position.y);
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
