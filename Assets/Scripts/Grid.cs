using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Grid : MonoBehaviour
{//Observer
    public static Grid GameGrid { get; set; }
    public GameObject borderObject;
    public static int gridWidth = 10;
    public static int gridHeight = 20;
    private Vector2 spawnPosition;
    public GameObject[] tetrominos;
    public List<Vector3> filledPositions = new List<Vector3>();
    
    private Vector3 GridPosition { get; set; }

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
        Tetromino.OnLandEnded += SpawnRandomTetromino;
        Tetromino.OnLanded += UpdateGrid;
        
        GridPosition = Round(transform.position);
        CreateGridBorders();
        spawnPosition = CalculateSpawnPosition();
        SpawnRandomTetromino();
        GameGrid = GetComponent<Grid>();
    }
    private void CreateGridBorders()
    {
        for (float i = 0; i < gridWidth; i++)
        {
            Vector3 cellPosition = new Vector3(i, 0, 0);
            Instantiate(borderObject,Round(GridPosition + cellPosition),Quaternion.identity,transform.parent);
        }

        for (float i = 0; i < gridHeight; i++)
        {
            Vector3 cellPosition = new Vector3(0, i, 0);
            Instantiate(borderObject,Round(GridPosition + cellPosition),Quaternion.identity,transform.parent);
            cellPosition = new Vector3(gridWidth, i, 0);
            Instantiate(borderObject,Round(GridPosition + cellPosition),Quaternion.identity,transform.parent);
        }
    }

    public bool IsPositionFilled(Vector2 pos)
    {
        return filledPositions.Contains(pos);
    }
    public bool CheckIsInsideGrid(Vector2 pos)
    {
        bool checkXBorder = Round(pos).x < GridPosition.x + GridWidth && pos.x > GridPosition.x;
        bool checkYBorder = Round(pos).y > GridPosition.y;
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
        int spawnYOffset = gridHeight + 1;
        int spawnXOffset = gridWidth / 2;
        return new Vector3(transform.position.x + spawnXOffset, GridPosition.y + spawnYOffset, 0f);
    }
    public int GenerateRandomTetrominoNumber()
    {
        int randomTetromino = Random.Range(0, 7);
        return randomTetromino;
    }
    public void UpdateGrid(Tetromino tetromino)
    {
        /*for (int y = 0; y < GridHeight; y++)
        {
            for (int x = 0; x < GridWidth; x++)
            {
                if (grid[x, y]?.parent == tetromino.position)
                {
                    
                }
            }    
        }*/
        foreach (Transform mino in tetromino.transform)
        {
            filledPositions.Add(mino.position);
        }
    }
}
