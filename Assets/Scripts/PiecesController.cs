using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;
public partial class PiecesController : MonoBehaviour {

    public static PiecesController Instance;

    public GameObject piecePrefab;
    public Vector2Int spawnPos;
    public float dropTimeInSeconds;
    public Vector2Int[,] JLSTZ_OFFSET_DATA { get; private set; }
    public Vector2Int[,] I_OFFSET_DATA { get; private set; }
    public Vector2Int[,] O_OFFSET_DATA { get; private set; }

    private GameObject curPiece;
    private PieceController curPieceController;
    private Coroutine dropCurPiece;

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
    public void PieceSet()
    {
        StopCoroutine(dropCurPiece);
    }
    /// <summary>
    /// Makes any necessary changes once the game has ended.
    /// </summary>
    public void GameOver()
    {
        PieceSet();
    }
    /// <summary>
    /// Spawns a new Tetris piece.
    /// </summary>
    public void SpawnPiece()
    {
        GameObject localGameObject = Instantiate(piecePrefab, transform);
        curPiece = localGameObject;
        PieceType randPiece = (PieceType)Random.Range(0, 7);        
        curPieceController = curPiece.GetComponent<PieceController>();
        curPieceController.SpawnPiece(randPiece);

        dropCurPiece = StartCoroutine(DropCurPiece());
    }
    /// <summary>
    /// Moves the current piece controlled by the player.
    /// </summary>
    /// <param name="movement">X,Y amount the piece should be moved by</param>
    public void MoveCurPiece(Vector2Int movement)
    {
        if(curPiece == null)
        {
            return;
        }
        curPieceController.MovePiece(movement);
    }
    private void StartGame()
    {
        if(curPieceController != null)
        {
            return;
        }
        SpawnPiece();
    }
}
