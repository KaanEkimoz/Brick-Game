using UnityEngine;
using UnityEngine.SceneManagement;

namespace InGame
{
    public partial class PiecesController
    {
        private void Update()
        {
            CheckKeyboardInputs();
        }
        private void CheckKeyboardInputs()
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
                SendPieceToFloor();
            if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                MoveDown();
                if (PiecesController.Instance != null)
                    PiecesController.Instance.OnSoftDropButtonDown();
            }
            if (Input.GetKeyUp(KeyCode.DownArrow) || Input.GetKeyUp(KeyCode.S))
            {
                if (PiecesController.Instance != null)
                    PiecesController.Instance.OnSoftDropButtonUp();
            }
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
                MoveRight();
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
                MoveLeft();
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                StartGame();
            if (Input.GetKeyDown(KeyCode.R))
                SceneManager.LoadScene(0);
            if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Space))
                RotateClockwise();
            if (Input.GetKeyDown(KeyCode.Z))
                RotateCounterClockwise();
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.B)) DebugForceAbility(Gameplay.AbilityType.Bomb);
            if (Input.GetKeyDown(KeyCode.H)) DebugForceAbility(Gameplay.AbilityType.HorizontalRow);
            if (Input.GetKeyDown(KeyCode.V)) DebugForceAbility(Gameplay.AbilityType.VerticalColumn);
            if (Input.GetKeyDown(KeyCode.F) && Gameplay.AbilityCharger.Instance != null)
                Gameplay.AbilityCharger.Instance.DebugForceFill(Gameplay.AbilityType.Bomb);
            if (Input.GetKeyDown(KeyCode.T)) Board.BoardController.OnTetrisCleared?.Invoke();
            if (Input.GetKeyDown(KeyCode.L))
            {
                Extras.LevelController.CurrentLevel++;
                Extras.LevelController.OnLevelIncreased?.Invoke();
            }
#endif
        }

#if UNITY_EDITOR
        private void DebugForceAbility(Gameplay.AbilityType type)
        {
            PieceSpawner spawner = FindObjectOfType<PieceSpawner>();
            if (spawner != null) spawner.DebugForceAbility(type);
        }
#endif
    
        #region Input Functions
        public void StartGame()
        {
            if(_curPieceMovement != null)
                return;
            PieceSpawner pieceSpawner = FindObjectOfType<PieceSpawner>();
            pieceSpawner.SpawnGhostPiece();
            pieceSpawner.SpawnPiece();
        }
        public void RotateClockwise()
        {
            _curPieceRotation.RotatePiece(true, true);
        }
        public void RotateCounterClockwise()
        {
            _curPieceRotation.RotatePiece(false, true);
        }
        public void SendPieceToFloor()
        {
            _curPieceMovement.SendPieceToFloor();
        }
        public void MoveDown()
        {
            MoveCurPiece(Vector2Int.down);
            OnSoftDrop?.Invoke();
        }
        public void MoveRight()
        {
            MoveCurPiece(Vector2Int.right);
        }
        public void MoveLeft()
        {
            MoveCurPiece(Vector2Int.left);
        }

        #endregion
    }
}
