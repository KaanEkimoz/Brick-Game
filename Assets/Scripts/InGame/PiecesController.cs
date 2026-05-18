using System;
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

        public static Action OnGameOver;
        /// <summary>Fired each time the player drives the piece down (keyboard MoveDown or held soft-drop). Not fired for the automatic gravity tick.</summary>
        public static Action OnSoftDrop;

        //Soft Drop Button Hold
        private float softDropButtonHoldTime = 0.38f;
        private float softDropHoldDropIntervalTime = 0.08f;
        private bool softDropIsHolding = false;

        // True once GameOver() has fired. Acts as a re-entry guard so a soft-drop
        // hold coroutine that still has the dead piece in hand cannot keep calling
        // MovePiece → SetPiece → GameOver in a tight loop, which on a full board
        // spammed thousands of log lines per second and crashed the Editor.
        private bool _gameOver = false;

        public void OnSoftDropButtonDown()
        {
            softDropIsHolding = true;
            StartCoroutine(SoftDropHoldCoroutine());
        }
        public void OnSoftDropButtonUp()
        {
            softDropIsHolding = false;
        }
        private IEnumerator SoftDropHoldCoroutine()
        {
            float timer = 0f;
            while (softDropIsHolding && timer < softDropButtonHoldTime)
            {
                yield return null;
                timer += Time.deltaTime;
            }

            if (softDropIsHolding && timer >= softDropButtonHoldTime)
            {
                while (softDropIsHolding)
                {
                    MoveCurPiece(Vector2Int.down);
                    OnSoftDrop?.Invoke();
                    yield return new WaitForSeconds(softDropHoldDropIntervalTime);
                }
            }
        }


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
            if (_dropCurPiece == null) return;
            StopCoroutine(_dropCurPiece);
            _dropCurPiece = null;
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
            // Re-entry guard — board-full scenarios used to fire GameOver dozens
            // of times in a single frame, each call re-broadcasting OnGameOver
            // (ad reloads, scoreboard refresh, etc.).
            if (_gameOver) return;
            _gameOver = true;

            // Stop both the gravity coroutine AND the soft-drop hold coroutine.
            // Do NOT Destroy(CurPiece) here — PieceController.Tiles is a static
            // reference into the live piece, and tearing it down mid-frame leaves
            // dangling Unity-null components that the next coroutine tick or
            // input handler would call .CanTileMove on, NRE-spamming the editor.
            softDropIsHolding = false;
            StopDropCurPiece();

            OnGameOver?.Invoke();
        }
        /// <summary>
        /// Moves the current piece controlled by the player.
        /// </summary>
        /// <param name="movement">X,Y amount the piece should be moved by</param>
        private void MoveCurPiece(Vector2Int movement)
        {
            // Hard stop on every input path once game-over has fired so the
            // soft-drop coroutine (or any stale tap from a touch buffer) can't
            // replay MovePiece → CanTileMove false → SetPiece → GameOver.
            if (_gameOver) return;
            if (CurPiece == null)
                return;
            _curPieceMovement.MovePiece(movement);
        }
        private void UpdateDropTimeAccordingTheLevel()
        {
            _dropTimeInSeconds = 0.75f - (float)(LevelController.CurrentLevel - 1) / 40;
        }
        public void DestroyCurPiece()
        {
            StopDropCurPiece();
            if (PiecesController.CurPiece)
                Destroy(PiecesController.CurPiece);
        }
    }
}
