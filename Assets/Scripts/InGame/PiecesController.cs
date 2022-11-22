using System.Collections;
using Extras;
using Piece;
using UnityEngine;

namespace InGame
{
    public partial class PiecesController : MonoBehaviour 
    {
        public static PiecesController Instance;
        public static GameObject CurPiece;
    
        private float _dropTimeInSeconds = 0.8f;
        private PieceMovement _curPieceMovement;
        private PieceRotation _curPieceRotation;
        private Coroutine _dropCurPiece;
    
        /// <summary>
        /// Event subscription
        /// </summary>
        private void OnEnable()
        {
            PieceSpawner.OnPieceSpawned += InitializeMovement;
            PieceSpawner.OnPieceSpawned += InitializeRotation;
            PieceSpawner.OnPieceSpawned += StartDropCurPiece;
            LevelController.OnLevelIncreased += UpdateDropTimeAccordingTheLevel;
        }
        /// <summary>
        /// Event unsubscription
        /// </summary>
        private void OnDisable()
        {
            PieceSpawner.OnPieceSpawned -= InitializeMovement;
            PieceSpawner.OnPieceSpawned -= InitializeRotation;
            PieceSpawner.OnPieceSpawned -= StartDropCurPiece;
            LevelController.OnLevelIncreased -= UpdateDropTimeAccordingTheLevel;
        }
        /// <summary>
        /// Called as soon as the instance is enabled. Sets the singleton.
        /// </summary>
        private void Awake()
        {
            #region Singleton

            if (Instance != null && Instance != this) 
            { 
                Destroy(this); 
            } 
            else 
            { 
                Instance = this; 
            }

            #endregion
        }
        /// <summary>
        /// Starts coroutine "DropCurPiece" and holds it as a variable to stop the coroutine later
        /// </summary>
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
                yield return new WaitForSeconds(_dropTimeInSeconds);
            } 
        }
        /// <summary>
        /// Once the piece is set in it's final location, the coroutine called to repeatedly drop the piece is stopped.
        /// </summary>
        public void StopDropCurPiece()
        {
            StopCoroutine(_dropCurPiece);
        }
        /// <summary>
        /// Initializes the movement component of the current piece
        /// </summary>
        private void InitializeMovement()
        {
            _curPieceMovement = CurPiece.GetComponent<PieceMovement>();
        }
        /// <summary>
        /// Initializes the rotation component of the current piece
        /// </summary>
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
            if (CurPiece == null)
                return;
            _curPieceMovement.MovePiece(movement);
        }
        private void UpdateDropTimeAccordingTheLevel()
        {
            _dropTimeInSeconds -= (float)(LevelController.CurrentLevel - 1) / 20;
        }
    }
}
