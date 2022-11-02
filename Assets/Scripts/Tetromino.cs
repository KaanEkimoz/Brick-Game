using System;
using UnityEngine;

public class Tetromino : MonoBehaviour
{
    float fallTimer;
    //fall speed in minutes
    public float fallSpeedInSeconds = 1;
    public bool canAutoFall = true;
    public bool allowRotation = true;
    void Update()
    {
        CheckUserInput();
        MoveDownWithTime();

    }
    void CheckUserInput()
    {
        if(Input.GetKeyDown(KeyCode.S))
            MoveDown();
        //if (Input.GetKeyDown(KeyCode.A))
            //MoveRight();
    }
    private void Rotate()
    {
        transform.Rotate(0, 0, -90);
        foreach (Transform mino in transform)
        {
            if (Grid.GameGrid.CheckIsInsideGrid(mino.position) == false)
            {
                if (mino.position.y < Grid.GameGrid.transform.position.y)
                    mino.position += Vector3.up;
            }
        }
    }
    void MoveDownWithTime()
    {
        if (IsTimePeriodPassed() && CanMoveDown())
        {
            transform.position += new Vector3(0, -1, 0);
            fallTimer = Time.time;
        }
    }

    void MoveDown()
    {
        if(CanMoveDown())
            transform.position += new Vector3(0, -1, 0);
    }
    bool CanMoveDown()
    {
        foreach(Transform mino in transform)
        {
            Vector3 nextPos = Grid.GameGrid.Round(mino.position + Vector3.down);
                
            if(Grid.GameGrid.CheckIsInsideGrid(nextPos) == false)
            {
                return false;
            }
        }
        return true;
    }
    bool IsTimePeriodPassed()
    {
        return Time.time - fallTimer >= fallSpeedInSeconds && canAutoFall;
    }
}
