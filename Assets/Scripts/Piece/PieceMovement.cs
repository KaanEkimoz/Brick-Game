using System;
using Board;
using InGame;
using UnityEngine;
namespace Piece
{
    public class PieceMovement : MonoBehaviour
    {
        public static Action OnPieceMovement;
        public static Action OnPieceSettled;
        /// <summary>Fired once with every tile coordinate when a piece lands via hard drop.</summary>
        public static Action<Vector2Int[]> OnHardDrop;

        private bool _hardDropping;

        /// <summary>
        /// Drops piece down as far as it can go.
        /// </summary>
        public void SendPieceToFloor()
        {
            _hardDropping = true;
            while (MovePiece(Vector2Int.down)) {}
            _hardDropping = false;
        }
        /// <summary>
        /// Moves the piece by the specified amount.
        /// </summary>
        /// <param name="movement">X,Y amount to move the piece</param>
        /// <returns>True if the piece was able to be moved. False if the move couldn't be completed.</returns>
        public bool MovePiece(Vector2Int movement)
        {
            foreach (var tile in PieceController.Tiles)
            {
                if (!tile.CanTileMove(movement + tile.coordinates))
                {
                    Debug.Log("Cant Go there!");
                    if(movement == Vector2Int.down)
                    {
                        SetPiece();
                    }
                    return false;
                }
            }
            foreach (var tile in PieceController.Tiles)
            {
                tile.MoveTile(movement);
            }
        
            OnPieceMovement?.Invoke();
        
            return true;
        }
        /// <summary>
        /// Sets the piece in its permanent location.
        /// </summary>
        private void SetPiece()
        {
            foreach (var tile in PieceController.Tiles)
            {
                if (!tile.SetTile())
                {
                    Debug.Log("GAME OVER!");
                    PiecesController.Instance.GameOver();
                    return;
                }
            }
            OnPieceSettled?.Invoke();
            if (_hardDropping)
            {
                var coords = new Vector2Int[PieceController.Tiles.Length];
                for (int i = 0; i < coords.Length; i++)
                    coords[i] = PieceController.Tiles[i] != null ? PieceController.Tiles[i].coordinates : Vector2Int.zero;
                OnHardDrop?.Invoke(coords);
            }
            BoardController.Instance.CheckLineClears();
            PiecesController.Instance.StopDropCurPiece();
            PieceSpawner pieceSpawner = FindObjectOfType<PieceSpawner>();
            pieceSpawner.SpawnPiece();
        }
        /// <summary>
        /// Checks if the piece is able to be moved by the specified amount. A piece cannot be moved if there
        /// is already another piece there or the piece would end up out of bounds.
        /// </summary>
        /// <param name="movement">X,Y amount to move the piece</param>
        /// <returns></returns>
        public bool CanMovePiece(Vector2Int movement)
        {
            foreach (var tile in PieceController.Tiles)
            {
                if (!tile.CanTileMove(movement + tile.coordinates))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
