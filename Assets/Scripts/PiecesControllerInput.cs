using UnityEngine;
using UnityEngine.SceneManagement;
public partial class PiecesController
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            SendPieceToFloor();
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            MoveDown();
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
    }
    public void RotateClockwise()
    {
        CurPieceRotation.RotatePiece(true, true);
    }
    public void RotateCounterClockwise()
    {
        CurPieceRotation.RotatePiece(false, true);
    }
    public void SendPieceToFloor()
    {
        CurPieceMovement.SendPieceToFloor();
    }
    public void MoveDown()
    {
        MoveCurPiece(Vector2Int.down);
    }
    public void MoveRight()
    {
        MoveCurPiece(Vector2Int.right);
    }
    public void MoveLeft()
    {
        MoveCurPiece(Vector2Int.left);
    }
}
