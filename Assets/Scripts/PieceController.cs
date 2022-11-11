using UnityEngine;
public enum PieceType { O, I, S, Z, L, J, T }
public class PieceController : MonoBehaviour {

    public PieceType curType;
    [HideInInspector]public TileController[] tiles;
    [HideInInspector]public TileController[] ghostTiles;
    private int RotationIndex { get; set; }
    private void OnEnable()
    {
        PieceSpawner.PieceSpawned += UpdateGhostTiles;
    }
    private void OnDisable()
    {
        PieceSpawner.PieceSpawned -= UpdateGhostTiles;
    }
    /// <summary>
    /// Called as soon as the piece is initialized. Initializes some necessary values.
    /// </summary>
    private void Awake()
    {
        InitializeTiles();
        InitializeGhostTiles();
        
        RotationIndex = 0;
    }
    private void InitializeTiles()
    {
        tiles = new TileController[4];
        for(int i = 1; i <= tiles.Length; i++)
        {
            string tileName = "Tile" + i;
            TileController newTile = transform.Find("Tiles").Find(tileName).GetComponent<TileController>();
            tiles[i - 1] = newTile;
        }
    }
    private void InitializeGhostTiles()
    {
        ghostTiles = new TileController[4];
        {
            for(int i = 1; i <= ghostTiles.Length; i++)
            {
                string tileName = "GhostTile" + i;
                TileController newTile = transform.Find("GhostTiles").Find(tileName).GetComponent<TileController>();
                ghostTiles[i - 1] = newTile;
            }
            
        }
    }
    private void UpdateGhostTiles()
    {
        for (int i = 1; i <= ghostTiles.Length; i++)
        {
            Vector2Int newPos = new Vector2Int((int) tiles[i - 1].gameObject.transform.position.x,
                (int) tiles[i - 1].gameObject.transform.position.y);
            ghostTiles[i - 1].UpdatePosition(newPos);
        }
        SendGhostPieceToFloor();
    }
    /// <summary>
    /// Checks if the piece is able to be moved by the specified amount. A piece cannot be moved if there
    /// is already another piece there or the piece would end up out of bounds.
    /// </summary>
    /// <param name="movement">X,Y amount to move the piece</param>
    /// <returns></returns>
    private bool CanMovePiece(Vector2Int movement)
    {
        foreach (var tile in tiles)
        {
            if (!tile.CanTileMove(movement + tile.coordinates))
            {
                return false;
            }
        }
        return true;
    }
    /// <summary>
    /// Drops piece down as far as it can go.
    /// </summary>
    public void SendPieceToFloor()
    {
        while (MovePiece(Vector2Int.down)) {}
    }
    /// <summary>
    /// Moves the piece by the specified amount.
    /// </summary>
    /// <param name="movement">X,Y amount to move the piece</param>
    /// <returns>True if the piece was able to be moved. False if the move couldn't be completed.</returns>
    public bool MovePiece(Vector2Int movement)
    {
        foreach (var tile in tiles)
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

        foreach (var tile in tiles)
        {
            tile.MoveTile(movement);
        }
        UpdateGhostTiles();
        return true;
        
    }
    /// <summary>
    /// Rotates the piece by 90 degrees in specified direction. Offset operations should almost always be attempted,
    /// unless you are rotating the piece back to its original position.
    /// </summary>
    /// <param name="clockwise">Set to true if rotating clockwise. Set to False if rotating CCW</param>
    /// <param name="shouldOffset">Set to true if offset operations should be attempted.</param>
    public void RotatePiece(bool clockwise, bool shouldOffset)
    {
        int oldRotationIndex = RotationIndex;
        RotationIndex += clockwise ? 1 : -1;
        RotationIndex = Mod(RotationIndex, 4);

        foreach (var t in tiles)
        {
            t.RotateTile(tiles[0].coordinates, clockwise);
        }

        if (!shouldOffset)
        {
            return;
        }

        bool canOffset = Offset(oldRotationIndex, RotationIndex);

        if (!canOffset)
        {
            RotatePiece(!clockwise, false);
        }
        
        UpdateGhostTiles();
    }
    /// <summary>
    /// True modulo operation that works for positive and negative values.
    /// </summary>
    /// <param name="x">The dividend</param>
    /// <param name="m">The divisor</param>
    /// <returns>Returns the remainder of x divided by m</returns>
    int Mod(int x, int m)
    {
        return (x % m + m) % m;
    }

    /// <summary>
    /// Performs 5 tests on the piece to find a valid final location for the piece.
    /// </summary>
    /// <param name="oldRotIndex">Original rotation index of the piece</param>
    /// <param name="newRotIndex">Rotation index the piece will be rotating to</param>
    /// <returns>True if one of the tests passed and a final location was found. False if all test failed.</returns>
    bool Offset(int oldRotIndex, int newRotIndex)
    {
        Vector2Int[,] curOffsetData;
        
        if(curType == PieceType.O)
        {
            curOffsetData = PiecesController.Instance.O_OFFSET_DATA;
        }
        else if(curType == PieceType.I)
        {
            curOffsetData = PiecesController.Instance.I_OFFSET_DATA;
        }
        else
        {
            curOffsetData = PiecesController.Instance.JLSTZ_OFFSET_DATA;
        }

        Vector2Int endOffset = Vector2Int.zero;

        bool movePossible = false;

        for (int testIndex = 0; testIndex < 5; testIndex++)
        {
            var offsetVal1 = curOffsetData[testIndex, oldRotIndex];
            var offsetVal2 = curOffsetData[testIndex, newRotIndex];
            endOffset = offsetVal1 - offsetVal2;
            if (CanMovePiece(endOffset))
            {
                movePossible = true;
                break;
            }
        }

        if (movePossible)
        {
            MovePiece(endOffset);
        }
        else
        {
            Debug.Log("Move impossible");
        }
        return movePossible;
    }

    /// <summary>
    /// Sets the piece in its permanent location.
    /// </summary>
    private void SetPiece()
    {
        foreach (var tile in tiles)
        {
            if (!tile.SetTile())
            {
                Debug.Log("GAME OVER!");
                PiecesController.Instance.GameOver();
                return;
            }
        }
        BoardController.Instance.CheckLineClears();
        PiecesController.Instance.StopDropCurPiece();
        
        DestroyGhostTiles();
        PieceSpawner pieceSpawner = FindObjectOfType<PieceSpawner>();
        pieceSpawner.SpawnPiece();
    }
    private void DestroyGhostTiles()
    {
        Destroy(transform.Find("GhostTiles").gameObject);
    }
    private void SendGhostPieceToFloor()
    {
        while(MoveGhostPiece(Vector2Int.down)) {}
    }
    private bool MoveGhostPiece(Vector2Int movement)
    {
        foreach (var t in ghostTiles)
        {
            if (!t.CanTileMove(movement+ t.coordinates))
            {
                return false;
            }
        }
        for(int i = 0; i< ghostTiles.Length; i++)
        {
            ghostTiles[i].MoveTile(movement);
        }
        return true;
    }
}