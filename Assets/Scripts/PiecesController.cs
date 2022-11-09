using System;
using System.Collections;
using UnityEngine;
public partial class PiecesController : MonoBehaviour 
{//Observer

    public static PiecesController Instance;
    
    public float dropTimeInSeconds;
    public Vector2Int[,] JLSTZ_OFFSET_DATA { get; private set; }
    public Vector2Int[,] I_OFFSET_DATA { get; private set; }
    public Vector2Int[,] O_OFFSET_DATA { get; private set; }

    public static GameObject CurPiece;
    public static PieceController CurPieceController;
    private Coroutine dropCurPiece;
    private void OnEnable()
    {
        PieceSpawner.PieceSpawned += StartDropCurPiece;
    }
    /// <summary>
    /// Called as soon as the instance is enabled. Sets the singleton and offset data arrays.
    /// </summary>
    private void Awake()
    {
        Instance = this;
        
        JLSTZ_OFFSET_DATA = new Vector2Int[5, 4];
        JLSTZ_OFFSET_DATA[0, 0] = Vector2Int.zero;
        JLSTZ_OFFSET_DATA[0, 1] = Vector2Int.zero;
        JLSTZ_OFFSET_DATA[0, 2] = Vector2Int.zero;
        JLSTZ_OFFSET_DATA[0, 3] = Vector2Int.zero;

        JLSTZ_OFFSET_DATA[1, 0] = Vector2Int.zero;
        JLSTZ_OFFSET_DATA[1, 1] = new Vector2Int(1,0);
        JLSTZ_OFFSET_DATA[1, 2] = Vector2Int.zero;
        JLSTZ_OFFSET_DATA[1, 3] = new Vector2Int(-1, 0);

        JLSTZ_OFFSET_DATA[2, 0] = Vector2Int.zero;
        JLSTZ_OFFSET_DATA[2, 1] = new Vector2Int(1, -1);
        JLSTZ_OFFSET_DATA[2, 2] = Vector2Int.zero;
        JLSTZ_OFFSET_DATA[2, 3] = new Vector2Int(-1, -1);

        JLSTZ_OFFSET_DATA[3, 0] = Vector2Int.zero;
        JLSTZ_OFFSET_DATA[3, 1] = new Vector2Int(0, 2);
        JLSTZ_OFFSET_DATA[3, 2] = Vector2Int.zero;
        JLSTZ_OFFSET_DATA[3, 3] = new Vector2Int(0, 2);

        JLSTZ_OFFSET_DATA[4, 0] = Vector2Int.zero;
        JLSTZ_OFFSET_DATA[4, 1] = new Vector2Int(1, 2);
        JLSTZ_OFFSET_DATA[4, 2] = Vector2Int.zero;
        JLSTZ_OFFSET_DATA[4, 3] = new Vector2Int(-1, 2);

        I_OFFSET_DATA = new Vector2Int[5, 4];
        I_OFFSET_DATA[0, 0] = Vector2Int.zero;
        I_OFFSET_DATA[0, 1] = new Vector2Int(-1, 0);
        I_OFFSET_DATA[0, 2] = new Vector2Int(-1, 1);
        I_OFFSET_DATA[0, 3] = new Vector2Int(0, 1);

        I_OFFSET_DATA[1, 0] = new Vector2Int(-1, 0);
        I_OFFSET_DATA[1, 1] = Vector2Int.zero;
        I_OFFSET_DATA[1, 2] = new Vector2Int(1, 1);
        I_OFFSET_DATA[1, 3] = new Vector2Int(0, 1);

        I_OFFSET_DATA[2, 0] = new Vector2Int(2, 0);
        I_OFFSET_DATA[2, 1] = Vector2Int.zero;
        I_OFFSET_DATA[2, 2] = new Vector2Int(-2, 1);
        I_OFFSET_DATA[2, 3] = new Vector2Int(0, 1);

        I_OFFSET_DATA[3, 0] = new Vector2Int(-1, 0);
        I_OFFSET_DATA[3, 1] = new Vector2Int(0, 1);
        I_OFFSET_DATA[3, 2] = new Vector2Int(1, 0);
        I_OFFSET_DATA[3, 3] = new Vector2Int(0, -1);

        I_OFFSET_DATA[4, 0] = new Vector2Int(2, 0);
        I_OFFSET_DATA[4, 1] = new Vector2Int(0, -2);
        I_OFFSET_DATA[4, 2] = new Vector2Int(-2, 0);
        I_OFFSET_DATA[4, 3] = new Vector2Int(0, 2);

        O_OFFSET_DATA = new Vector2Int[1, 4];
        O_OFFSET_DATA[0, 0] = Vector2Int.zero;
        O_OFFSET_DATA[0, 1] = Vector2Int.down;
        O_OFFSET_DATA[0, 2] = new Vector2Int(-1, -1);
        O_OFFSET_DATA[0, 3] = Vector2Int.left;
    }
    private void StartDropCurPiece()
    {
        dropCurPiece = StartCoroutine(DropCurPiece());
    }
    /// <summary>
    /// Drops the piece the current piece the player is controlling by one unit.
    /// </summary>
    /// <returns>Function is called on a loop based on the 'dropTimeInSeconds' variable.</returns>
    IEnumerator DropCurPiece()
    {
        while (true)
        {
            MoveCurPiece(Vector2Int.down);
            yield return new WaitForSeconds(dropTimeInSeconds);
        } 
    }
    /// <summary>
    /// Once the piece is set in it's final location, the coroutine called to repeatedly drop the piece is stopped.
    /// </summary>
    public void StopDropCurPiece()
    {
        StopCoroutine(dropCurPiece);
    }
    /// <summary>
    /// Makes any necessary changes once the game has ended.
    /// </summary>
    public void GameOver()
    {
        StopDropCurPiece();
    }
    /// <summary>
    /// Moves the current piece controlled by the player.
    /// </summary>
    /// <param name="movement">X,Y amount the piece should be moved by</param>
    public void MoveCurPiece(Vector2Int movement)
    {
        if(CurPiece == null)
        {
            return;
        }
        CurPieceController.MovePiece(movement);
    }
    private void StartGame()
    {
        if(CurPieceController != null)
        {
            return;
        }
        
        PieceSpawner pieceSpawner = FindObjectOfType<PieceSpawner>();
        pieceSpawner.SpawnPiece();
    }
}
