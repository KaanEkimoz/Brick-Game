using UnityEngine;
using UnityEngine.SceneManagement;
public partial class PiecesController
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            CurPieceController.SendPieceToFloor();
        }
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            MoveCurPiece(Vector2Int.down);
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.D))
        {
            MoveCurPiece(Vector2Int.left);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.A))
        {
            MoveCurPiece(Vector2Int.right);
        }
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            StartGame();
        }
        if (Input.GetKeyDown(KeyCode.R)){
            SceneManager.LoadScene(0);
        }
        if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Space))
        {
            CurPieceController.RotatePiece(true, true);
        }
        if (Input.GetKeyDown(KeyCode.Z))
        {
            CurPieceController.RotatePiece(false, true);
        }

        
    }
}
