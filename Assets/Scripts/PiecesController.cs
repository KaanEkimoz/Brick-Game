using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class PiecesController : MonoBehaviour {

    public static PiecesController Instance;

    public GameObject piecePrefab;
    public Vector2Int spawnPos;
    public float dropTimeInSeconds;
    public Coroutine dropCurPiece;
    public Vector2Int[,] JLSTZ_OFFSET_DATA { get; private set; }
    public Vector2Int[,] I_OFFSET_DATA { get; private set; }
    public Vector2Int[,] O_OFFSET_DATA { get; private set; }
    //public List<GameObject> piecesInGame;
    public GameObject gameOverText;

    GameObject curPiece;
    PieceController curPieceController;

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
    /// Called at the first frame instance is enabled. Sets some variables.
    /// </summary>
    private void Start()
    {
        gameOverText.SetActive(false);
    }

    /// <summary>
    /// Called once every frame. Checks for player input.
    /// </summary>
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            curPieceController.SendPieceToFloor();
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            MoveCurPiece(Vector2Int.down);
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            MoveCurPiece(Vector2Int.left);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            MoveCurPiece(Vector2Int.right);
        }
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if(curPieceController != null)
            {
                return;
            }
            SpawnPiece();
        }
        if (Input.GetKeyDown(KeyCode.R)){
            SceneManager.LoadScene(0);
        }

        if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Space))
        {
            curPieceController.RotatePiece(true, true);
        }
        if (Input.GetKeyDown(KeyCode.Z))
        {
            curPieceController.RotatePiece(false, true);
        }

        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            SpawnDebug(0);
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SpawnDebug(1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SpawnDebug(2);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SpawnDebug(3);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            SpawnDebug(4);
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            SpawnDebug(5);
        }
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            SpawnDebug(6);
        }

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
        gameOverText.SetActive(true);
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

    public void SpawnDebug(int id)
    {
        GameObject localGameObject = GameObject.Instantiate(piecePrefab, transform);
        curPiece = localGameObject;
        PieceType randPiece = (PieceType)id;
        curPieceController = curPiece.GetComponent<PieceController>();
        curPieceController.SpawnPiece(randPiece);
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
}
