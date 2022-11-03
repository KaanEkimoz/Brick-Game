using System;
using UnityEngine;
public class Tetromino : MonoBehaviour
{//Subject
    public static event Action<Tetromino> OnLanded;
    public static event Action OnLandEnded;
    float fallTimer;
    public float timeBetweenFallsInSeconds = 1;
    public bool canAutoFall = true;
    public bool allowRotation = true;
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
        if (Input.GetKeyDown(KeyCode.S))
            MoveToTheDirection(Vector3.down);

        if (Input.GetKeyDown(KeyCode.R) && allowRotation)
            Rotate();

        if (Input.GetKeyDown(KeyCode.A))
            MoveToTheDirection(Vector3.left);
        if (Input.GetKeyDown(KeyCode.D))
            MoveToTheDirection(Vector3.right);
    }
    private void Rotate()
    {
        transform.Rotate(0, 0, -90);
        foreach (Transform mino in transform)
        {
            if (Grid.GameGrid.IsPositionFilled(mino.position))
            {
                if(transform.position.x > mino.position.x)
                    transform.position += Vector3.right;
                else if(transform.position.x < mino.position.x)
                    transform.position += Vector3.left;
            }
            //After rotating it, if any mino is not inside the grid; shifts the all the minos (up, right or left)
            if (Grid.GameGrid.CheckIsInsideGrid(mino.position) == false)
            {
                if (mino.position.y <= Grid.GameGrid.transform.position.y)
                    transform.position += Vector3.up;

                if (mino.position.x <= Grid.GameGrid.transform.position.x)
                    transform.position += Vector3.right;
                else if (mino.position.x >= Grid.GameGrid.transform.position.x + Grid.GridWidth)
                    transform.position += Vector3.left;
            }
        }
    }
    void MoveDownWithTime()
    {
        if (IsTimePeriodPassed())
        {
            MoveToTheDirection(Vector3.down);
            fallTimer = Time.time;
        }
    }
    bool IsTimePeriodPassed()
    {
        return Time.time - fallTimer >= timeBetweenFallsInSeconds && canAutoFall;
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
            Vector3 nextPos = Grid.GameGrid.Round(mino.position + direction);

            if (Grid.GameGrid.CheckIsInsideGrid(nextPos) == false || Grid.GameGrid.IsPositionFilled(nextPos) )
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
