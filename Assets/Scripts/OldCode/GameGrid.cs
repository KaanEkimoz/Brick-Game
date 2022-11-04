using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
public class GameGrid : MonoBehaviour
{//Observer
    public static GameGrid GameGridInstance { get; set; }
    public GameObject borderObject;
    public static int gridWidth = 10;
    public static int gridHeight = 20;
    private Vector2 spawnPosition;
    public GameObject[] tetrominos;

    private static Transform[,] grid = new Transform[GridWidth, GridHeight];
    
    public static Vector3 GridPosition { get; set; }

    public static int GridWidth
    {
        get => gridWidth;
        set => gridWidth = value;
    }
    public static int GridHeight
    {
        get => gridHeight;
        set => gridHeight = value;
    }
    void Start()
    {
        Tetromino.OnLanded += UpdateGrid;
        Tetromino.OnLandEnded += SpawnRandomTetromino;
        Tetromino.OnLandEnded += CheckForLines;
        

        GridPosition = Round(transform.position);
        spawnPosition = Round(CalculateSpawnPosition());
        SpawnRandomTetromino();
        GameGridInstance = GetComponent<GameGrid>();
    }
    public bool IsPositionFilled(Vector2 pos)
    {
        if(grid[Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y)] != null) 
            return true;
        return false;
    }
    public bool CheckIsInsideGrid(Vector2 pos)
    {
        bool checkXBorder = Round(pos).x <= 0 + GridWidth && Round(pos).x >= 0;
        bool checkYBorder = Round(pos).y >= 0;
        return checkXBorder && checkYBorder;
    }
    public Vector2 Round(Vector2 pos)
    {
        return new Vector2(Mathf.Round(pos.x), Mathf.Round(pos.y));
    }
    public Vector3 Round(Vector3 pos)
    {
        return new Vector3(Mathf.Round(pos.x), Mathf.Round(pos.y), Mathf.Round((pos.z)));
    }
    public void SpawnRandomTetromino()
    {
        Instantiate(tetrominos[GenerateRandomTetrominoNumber()], spawnPosition, transform.rotation);
    }
    private Vector3 CalculateSpawnPosition()
    {
        int spawnYOffset = gridHeight - 1;
        int spawnXOffset = gridWidth /  2;
        return new Vector3(GridPosition.x + spawnXOffset, GridPosition.y + spawnYOffset, 0f);
    }
    private int GenerateRandomTetrominoNumber()
    {
        int randomTetromino = Random.Range(0, 7);
        return randomTetromino;
    }
    private void UpdateGrid(Tetromino tetromino)
    {
        foreach (Transform mino in tetromino.transform)
        {
            int roundedX = Mathf.RoundToInt(mino.position.x);
            int roundedY = Mathf.RoundToInt(mino.position.y);
            grid[roundedX,roundedY] = mino;
        }
    }
    private void CheckForLines()
    {
        for (int lineCounter = GridHeight -1; lineCounter >= 0; lineCounter--)
        {
            if (HasLine(lineCounter))
            {
                DeleteLine(lineCounter);
                RowDown(lineCounter);
            }
        }
    }
    private bool HasLine(int lineNumber)
    {
        for (int x = 0; x < gridWidth; x++)
        {
            if (grid[x, lineNumber] == null)
                return false;
        }

        return true;
    }
    private void DeleteLine(int lineNumber)
    {
        for (int k = 0; k < gridWidth; k++)
        {
            Destroy(grid[k,lineNumber].gameObject);
            grid[k, lineNumber] = null;
        }
    }

    private void RowDown(int lineNumber)
    {
        for(int y = lineNumber; y < GridHeight; y++)
            for(int x = 0; x < gridWidth; x++)
                if (grid[x, y] != null)
                {
                    grid[x, y - 1] = grid[x, y];
                    grid[x, y] = null;
                    grid[x, y - 1].transform.position -= new Vector3(0, 1, 0);
                }
    }
}
