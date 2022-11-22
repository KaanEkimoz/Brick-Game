using System;
using Piece;
using Tiles;
using UnityEngine;
using Random = UnityEngine.Random;

namespace InGame
{
    public enum PieceType { O, I, S, Z, L, J, T }

    public class PieceSpawner : MonoBehaviour
    {
        [Space]
        [Header("Prefabs")]
        public GameObject ghostPiecePrefab;
        public GameObject piecePrefab;
    
        [Space]
        public Vector2Int spawnPosition;
    
        [Space]
        public Sprite[] tileSprites;

        public static Action OnPieceSpawned;
        public static Action<PieceType> OnNextPieceChanged;
    
        private PieceType _curPieceType;
        private static PieceType _nextPieceType;
        private TileController[] _tiles;

        private void Start()
        {
            _nextPieceType = (PieceType)Random.Range(0, 7);
        }

        /// <summary>
        /// Moves the attached tiles to form the Tetris piece specified. Also sets the correct color of tile sprite.
        /// </summary>
        /// <param name="newType">Type of tetris piece to be spawned.</param>
        private void UpdateTiles(PieceType newType, PieceController pieceController)
        {
            PieceType curType = newType;
            _tiles = PieceController.Tiles;
            _tiles[0].UpdatePosition(spawnPosition);

            switch (curType)
            {
                case PieceType.I:
                    _tiles[1].UpdatePosition(spawnPosition + Vector2Int.left);
                    _tiles[2].UpdatePosition(spawnPosition + (Vector2Int.right * 2));
                    _tiles[3].UpdatePosition(spawnPosition + Vector2Int.right);
                    SetTileSprites(tileSprites[0]);
                    break;

                case PieceType.J:
                    _tiles[1].UpdatePosition(spawnPosition + Vector2Int.left);
                    _tiles[2].UpdatePosition(spawnPosition + new Vector2Int(-1, 1));
                    _tiles[3].UpdatePosition(spawnPosition + Vector2Int.right);
                    SetTileSprites(tileSprites[1]);
                    break;

                case PieceType.L:
                    _tiles[1].UpdatePosition(spawnPosition + Vector2Int.left);
                    _tiles[2].UpdatePosition(spawnPosition + new Vector2Int(1, 1));
                    _tiles[3].UpdatePosition(spawnPosition + Vector2Int.right);
                    SetTileSprites(tileSprites[2]);
                    break;

                case PieceType.O:
                    _tiles[1].UpdatePosition(spawnPosition + Vector2Int.right);
                    _tiles[2].UpdatePosition(spawnPosition + new Vector2Int(1, 1));
                    _tiles[3].UpdatePosition(spawnPosition + Vector2Int.up);
                    SetTileSprites(tileSprites[3]);
                    break;

                case PieceType.S:
                    _tiles[1].UpdatePosition(spawnPosition + Vector2Int.left);
                    _tiles[2].UpdatePosition(spawnPosition + new Vector2Int(1, 1));
                    _tiles[3].UpdatePosition(spawnPosition + Vector2Int.up);
                    SetTileSprites(tileSprites[4]);
                    break;

                case PieceType.T:
                    _tiles[1].UpdatePosition(spawnPosition + Vector2Int.left);
                    _tiles[2].UpdatePosition(spawnPosition + Vector2Int.up);
                    _tiles[3].UpdatePosition(spawnPosition + Vector2Int.right);
                    SetTileSprites(tileSprites[5]);
                    break;

                case PieceType.Z:
                    _tiles[1].UpdatePosition(spawnPosition + Vector2Int.up);
                    _tiles[2].UpdatePosition(spawnPosition + new Vector2Int(-1, 1));
                    _tiles[3].UpdatePosition(spawnPosition + Vector2Int.right);
                    SetTileSprites(tileSprites[6]);
                    break;
            }
            int index = 0;
            foreach(TileController tile in _tiles)
            {
                tile.InitializeTile(pieceController, index);
                index++;
            }
        }
        /// <summary>
        /// Sets the sprites of all tiles on this piece
        /// </summary>
        /// <param name="newSpr">New sprite to set for this tile</param>
        private void SetTileSprites(Sprite newSpr)
        {
            for(int i = 0; i < _tiles.Length; i++)
            {
                if(_tiles[i] == null)
                {
                    continue;
                }
                _tiles[i].gameObject.GetComponent<SpriteRenderer>().sprite = newSpr;
            }
        }
        public void SpawnPiece()
        {
            GameObject curPiece = Instantiate(piecePrefab, transform);
            InitializeCurPiece(curPiece);
        
            _curPieceType = _nextPieceType;
            _nextPieceType = (PieceType) Random.Range(0, 7);
        
            OnNextPieceChanged?.Invoke(_nextPieceType);
            UpdateTiles(_curPieceType, PiecesController.CurPiece.GetComponent<PieceController>());
            OnPieceSpawned?.Invoke();
        }
        private void InitializeCurPiece(GameObject curPiece)
        {
            PiecesController.CurPiece = curPiece;
        }

        public void SpawnGhostPiece()
        {
            Instantiate(ghostPiecePrefab, transform);
        }
    }
}