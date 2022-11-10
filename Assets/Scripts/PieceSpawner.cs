using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class PieceSpawner : MonoBehaviour
{
    public GameObject piecePrefab;
    public Vector2Int spawnPosition;
    public Sprite[] tileSprites;
    public static PieceType NextPieceType;
    public PieceType curPieceType;
    
    public static Action PieceSpawned;
    public static Action NextPieceChanged;
    
    private TileController[] tiles;

    private void Start()
    {
        NextPieceType = (PieceType)Random.Range(0, 7);
    }

    /// <summary>
    /// Moves the attached tiles to form the Tetris piece specified. Also sets the correct color of tile sprite.
    /// </summary>
    /// <param name="newType">Type of tetris piece to be spawned.</param>
    private void UpdateTiles(PieceType newType, PieceController pieceController)
    {
        PieceType curType = newType;
        tiles = pieceController.tiles;
        tiles[0].UpdatePosition(spawnPosition);

        switch (curType)
        {
            case PieceType.I:
                tiles[1].UpdatePosition(spawnPosition + Vector2Int.left);
                tiles[2].UpdatePosition(spawnPosition + (Vector2Int.right * 2));
                tiles[3].UpdatePosition(spawnPosition + Vector2Int.right);
                SetTileSprites(tileSprites[0]);
                break;

            case PieceType.J:
                tiles[1].UpdatePosition(spawnPosition + Vector2Int.left);
                tiles[2].UpdatePosition(spawnPosition + new Vector2Int(-1, 1));
                tiles[3].UpdatePosition(spawnPosition + Vector2Int.right);
                SetTileSprites(tileSprites[1]);
                break;

            case PieceType.L:
                tiles[1].UpdatePosition(spawnPosition + Vector2Int.left);
                tiles[2].UpdatePosition(spawnPosition + new Vector2Int(1, 1));
                tiles[3].UpdatePosition(spawnPosition + Vector2Int.right);
                SetTileSprites(tileSprites[2]);
                break;

            case PieceType.O:
                tiles[1].UpdatePosition(spawnPosition + Vector2Int.right);
                tiles[2].UpdatePosition(spawnPosition + new Vector2Int(1, 1));
                tiles[3].UpdatePosition(spawnPosition + Vector2Int.up);
                SetTileSprites(tileSprites[3]);
                break;

            case PieceType.S:
                tiles[1].UpdatePosition(spawnPosition + Vector2Int.left);
                tiles[2].UpdatePosition(spawnPosition + new Vector2Int(1, 1));
                tiles[3].UpdatePosition(spawnPosition + Vector2Int.up);
                SetTileSprites(tileSprites[4]);
                break;

            case PieceType.T:
                tiles[1].UpdatePosition(spawnPosition + Vector2Int.left);
                tiles[2].UpdatePosition(spawnPosition + Vector2Int.up);
                tiles[3].UpdatePosition(spawnPosition + Vector2Int.right);
                SetTileSprites(tileSprites[5]);
                break;

            case PieceType.Z:
                tiles[1].UpdatePosition(spawnPosition + Vector2Int.up);
                tiles[2].UpdatePosition(spawnPosition + new Vector2Int(-1, 1));
                tiles[3].UpdatePosition(spawnPosition + Vector2Int.right);
                SetTileSprites(tileSprites[6]);
                break;
        }

        int index = 0;
        foreach(TileController tile in tiles)
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
        for(int i = 0; i < tiles.Length; i++)
        {
            if(tiles[i] == null)
            {
                continue;
            }
            tiles[i].gameObject.GetComponent<SpriteRenderer>().sprite = newSpr;
        }
    }
    public void SpawnPiece()
    {
        GameObject curPiece = Instantiate(piecePrefab, transform);
        InitializeCurPiece(curPiece);
        curPieceType = NextPieceType;
        NextPieceType = (PieceType) Random.Range(0, 7);
        NextPieceChanged?.Invoke();
        UpdateTiles(curPieceType,PiecesController.CurPieceController);
        PieceSpawned?.Invoke();
    }
    private void InitializeCurPiece(GameObject curPiece)
    {
        PiecesController.CurPiece = curPiece;
        PiecesController.CurPieceController = curPiece.GetComponent<PieceController>();
    }
}