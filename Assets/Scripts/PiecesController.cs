using System.Collections;
using UnityEngine;
public partial class PiecesController : MonoBehaviour 
{
    public static PiecesController Instance;
    public float dropTimeInSeconds;
    
    public static GameObject CurPiece;
    private PieceMovement CurPieceMovement;
    private PieceRotation CurPieceRotation;
    private Coroutine dropCurPiece;
    private void OnEnable()
    {
        PieceSpawner.PieceSpawned += InitializeMovement;
        PieceSpawner.PieceSpawned += InitializeRotation;
        PieceSpawner.PieceSpawned += StartDropCurPiece;
        LevelController.OnLevelIncreased += UpdateDropTime;
    }
    private void OnDisable()
    {
        PieceSpawner.PieceSpawned -= InitializeMovement;
        PieceSpawner.PieceSpawned -= InitializeRotation;
        PieceSpawner.PieceSpawned -= StartDropCurPiece;
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
        dropCurPiece = StartCoroutine(DropCurPiece());
    }
    /// <summary>
    /// Drops the piece the current piece the player is controlling by one unit.
    /// </summary>
    /// <returns>Function is called on a loop based on the 'dropTimeInSeconds' variable.</returns>
    IEnumerator DropCurPiece()
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
        StopCoroutine(dropCurPiece);
    }

    public void InitializeMovement()
    {
        CurPieceMovement = CurPiece.GetComponent<PieceMovement>();
    }

    public void InitializeRotation()
    {
        CurPieceRotation = CurPiece.GetComponent<PieceRotation>();
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
        if(CurPiece == null)
        {
            return;
        }
        CurPieceMovement.MovePiece(movement);
    }
    private void StartGame()
    {
        if(CurPieceMovement != null)
        {
            return;
        }
        
        PieceSpawner pieceSpawner = FindObjectOfType<PieceSpawner>();
        pieceSpawner.SpawnPiece();
    }
    private void UpdateDropTime()
    {
        dropTimeInSeconds -= (float)(LevelController.CurrentLevel -1) / 20;
    }
}
