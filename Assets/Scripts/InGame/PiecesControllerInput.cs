using Tutorial;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace InGame
{
    public partial class PiecesController
    {
        // Convenience alias so the tutorial-gate calls below read naturally.
        private static bool Allow(TutorialController.TutorialGesture g) => TutorialController.IsGestureAllowed(g);


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
            if (Input.GetKeyDown(KeyCode.H)) DebugForceAbility(Gameplay.AbilityType.Laser);
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
            if (!Allow(TutorialController.TutorialGesture.Rotate)) return;
            _curPieceRotation.RotatePiece(true, true);
            OnRotate?.Invoke();
        }
        public void RotateCounterClockwise()
        {
            if (!Allow(TutorialController.TutorialGesture.Rotate)) return;
            _curPieceRotation.RotatePiece(false, true);
            OnRotate?.Invoke();
        }
        public void SendPieceToFloor()
        {
            if (!Allow(TutorialController.TutorialGesture.HardDrop)) return;
            _curPieceMovement.SendPieceToFloor();
            OnHardDrop?.Invoke();
        }
        public void MoveDown()
        {
            if (!Allow(TutorialController.TutorialGesture.SoftDrop)) return;
            MoveCurPiece(Vector2Int.down);
            OnSoftDrop?.Invoke();
        }
        public void MoveRight()
        {
            if (!Allow(TutorialController.TutorialGesture.MoveLR)) return;
            MoveCurPiece(Vector2Int.right);
            OnHorizontalMove?.Invoke();
        }
        public void MoveLeft()
        {
            if (!Allow(TutorialController.TutorialGesture.MoveLR)) return;
            MoveCurPiece(Vector2Int.left);
            OnHorizontalMove?.Invoke();
        }

        #endregion
    }
}
