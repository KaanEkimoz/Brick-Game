using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class Grid : MonoBehaviour
{
    public static Grid GameGrid { get; set; }
    public GameObject borderObject;
    public static int gridWidth = 10;
    public static int gridHeight = 20;
    private Vector2 spawnPosition;

    public GameObject[] tetrominos;
    
    public int GridWidth
    {
        get => gridWidth;
        set => gridWidth = value;
    }
    public  int GridHeight
    {
        get => gridHeight;
        set => gridHeight = value;
    }
    void Start()
    {
        transform.position = Round(transform.position);
        
        
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
            Instantiate(borderObject,Round(transform.position + cellPosition),Quaternion.identity,transform.parent);
        }

        for (float i = 0; i < gridHeight; i++)
        {
            Vector3 cellPosition = new Vector3(0, i, 0);
            Instantiate(borderObject,Round(transform.position + cellPosition),Quaternion.identity,transform.parent);
            cellPosition = new Vector3(gridWidth, i, 0);
            Instantiate(borderObject,Round(transform.position + cellPosition),Quaternion.identity,transform.parent);
        }
    }
    
    
    public bool CheckIsInsideGrid(Vector2 pos)
    {
        bool checkXBorder = Round(pos).x < transform.position.x + GridWidth && pos.x > transform.position.x;
        bool checkYBorder = Round(pos).y > transform.position.y;
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
        Instantiate(tetrominos[RandomTetrominoNumber()], spawnPosition, transform.rotation);
    }

    private Vector3 CalculateSpawnPosition()
    {
        int spawnYOffset = gridHeight + 1;
        int spawnXOffset = gridWidth / 2;
        return new Vector3(transform.position.x + spawnXOffset, transform.position.y + spawnYOffset, 0f);
    }

    public int RandomTetrominoNumber()
    {
        int randomTetromino = Random.Range(0, 7);
        return randomTetromino;
    }
}
