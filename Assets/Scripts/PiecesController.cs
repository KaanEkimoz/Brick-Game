using System.Collections;
using Piece;
using UnityEngine;
public partial class PiecesController : MonoBehaviour 
{
    public static PiecesController Instance;
    public float dropTimeInSeconds;
    
    public static GameObject CurPiece;
    private PieceMovement _curPieceMovement;
    private PieceRotation _curPieceRotation;
    private Coroutine _dropCurPiece;
    private void OnEnable()
    {
        PieceSpawner.OnPieceSpawned += InitializeMovement;
        PieceSpawner.OnPieceSpawned += InitializeRotation;
        PieceSpawner.OnPieceSpawned += StartDropCurPiece;
        LevelController.OnLevelIncreased += UpdateDropTime;
    }
    private void OnDisable()
    {
        PieceSpawner.OnPieceSpawned -= InitializeMovement;
        PieceSpawner.OnPieceSpawned -= InitializeRotation;
        PieceSpawner.OnPieceSpawned -= StartDropCurPiece;
        LevelController.OnLevelIncreased -= UpdateDropTime;
    }
    /// <summary>
    /// Called as soon as the instance is enabled. Sets the singleton and offset data arrays.
    /// </summary>
    private void Awake()
    {
        Instance = this;
    }
    private void StartDropCurPiece()
    {
        _dropCurPiece = StartCoroutine(DropCurPiece());
    }
    /// <summary>
    /// Drops the piece the current piece the player is controlling by one unit.
    /// </summary>
    /// <returns>Function is called on a loop based on the 'dropTimeInSeconds' variable.</returns>
    private IEnumerator DropCurPiece()
    {
        //TO DO: Add Pause Conditions
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
        StopCoroutine(_dropCurPiece);
    }

    private void InitializeMovement()
    {
        _curPieceMovement = CurPiece.GetComponent<PieceMovement>();
    }

    private void InitializeRotation()
    {
        _curPieceRotation = CurPiece.GetComponent<PieceRotation>();
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
    private void MoveCurPiece(Vector2Int movement)
    {
        /*if(CurPiece == null)
        {
            return;
        }*/
        _curPieceMovement.MovePiece(movement);
    }
    private void StartGame()
    {
        if(_curPieceMovement != null)
        {
            return;
        }
        PieceSpawner pieceSpawner = FindObjectOfType<PieceSpawner>();
        pieceSpawner.SpawnGhostPiece();
        pieceSpawner.SpawnPiece();
    }
    private void UpdateDropTime()
    {
        dropTimeInSeconds -= (float)(LevelController.CurrentLevel -1) / 20;
    }
}
