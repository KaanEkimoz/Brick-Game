using UnityEngine;
public class PieceController : MonoBehaviour 
{
    public static TileController[] Tiles;
    private void OnEnable()
    {
        PieceSpawner.PieceSpawned += InitializeTiles;
    }

    private void OnDisable()
    {
        PieceSpawner.PieceSpawned -= InitializeTiles;
    }
    private void Awake()
    {
        InitializeTiles();
    }
    private void InitializeTiles()
    {
        Tiles = new TileController[4];
        for(int i = 1; i <= Tiles.Length; i++)
        {
            string tileName = "Tile" + i;
            TileController newTile = transform.Find("Tiles").Find(tileName).GetComponent<TileController>();
            Tiles[i - 1] = newTile;
        }
    }
}