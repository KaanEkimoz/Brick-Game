using System;
using UnityEngine;
public class Tetromino : MonoBehaviour
{//Subject
    public static event Action<Tetromino> OnLanded;
    public static event Action OnLandEnded;
    
    private float _fallTimer;
    public float timeBetweenFallsInSeconds = 1;
    public bool canAutoFall = true;
    public bool allowRotation = true;
    public bool isTetrominoI;
    [HideInInspector] public bool isLanded;
    void Update()
    {
        if (CanLand())
            Land();
        CheckUserInput();
        MoveDownWithTime();
    }
    void CheckUserInput()
    {
        if (Input.GetKey(KeyCode.S))
            MoveToTheDirection(Vector3.down);

        if (Input.GetKeyDown(KeyCode.R) && allowRotation)
            RotateTheTetromino();

        if (Input.GetKeyDown(KeyCode.A))
            MoveToTheDirection(Vector3.left);
        if (Input.GetKeyDown(KeyCode.D))
            MoveToTheDirection(Vector3.right);
    }
    private void RotateTheTetromino()
    {
        transform.Rotate(0, 0, -90);
        foreach (Transform mino in transform)
        {
            if (isTetrominoI)
            {
                if (transform.rotation.eulerAngles.z == 270 || transform.rotation.eulerAngles.z == 90)
                {
                    //GameGrid.GameGridInstance.IsPositionFilled(Round)
                }
            }
            //After rotating it, if any mino is not inside the grid; shifts the all the minos (up, right or left)
            if (GameGrid.GameGridInstance.CheckIsInsideGrid(mino.position) == false)
            {
                if (mino.position.y < GameGrid.GridPosition.y)
                    transform.position += Vector3.up;

                if (mino.position.x < GameGrid.GridPosition.x)
                    transform.position += Vector3.right;
                else if (mino.position.x > GameGrid.GridPosition.x + GameGrid.GridWidth)
                    transform.position += Vector3.left;

                break;
            }
            if(GameGrid.GameGridInstance.IsPositionFilled(mino.position))
            {
                if(transform.position.x > mino.position.x)
                    transform.position += Vector3.right;
                else if(transform.position.x < mino.position.x)
                    transform.position += Vector3.left;
            }
        }
    }
    void MoveDownWithTime()
    {
        if (IsTimePeriodPassed())
        {
            MoveToTheDirection(Vector3.down);
            _fallTimer = Time.time;
        }
    }
    bool IsTimePeriodPassed()
    {
        return Time.time - _fallTimer >= timeBetweenFallsInSeconds && canAutoFall;
    }
    void MoveToTheDirection(Vector3 pos)
    {
        if (CanMoveToTheDirection(pos))
            transform.position += pos;
    }
    bool CanMoveToTheDirection(Vector3 direction)
    {
        foreach (Transform mino in transform)
        {
            Vector3 nextPos = GameGrid.GameGridInstance.Round(mino.position + direction);

            if (GameGrid.GameGridInstance.CheckIsInsideGrid(nextPos) == false || GameGrid.GameGridInstance.IsPositionFilled(nextPos) )
            {
                return false;
            }
        }
        return true;
    }
    private void Land()
    {
        OnLanded?.Invoke(this);
        enabled = false;
        canAutoFall = false;
        OnLandEnded?.Invoke();
    }
    private bool CanLand() 
    {
        return CanMoveToTheDirection(Vector3.down) == false && IsTimePeriodPassed(); 
    }
}
