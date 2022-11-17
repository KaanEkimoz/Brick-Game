using UnityEngine;
public class GhostPieceController : MonoBehaviour
{
    [HideInInspector]public TileController[] ghostTiles;
    private void OnEnable()
    {
        PieceSpawner.PieceSpawned += UpdateGhostTiles;
        PieceMovement.OnPieceMovement += UpdateGhostTiles;
        PieceRotation.OnPieceRotation += UpdateGhostTiles;
    }
    private void OnDisable()
    {
        PieceSpawner.PieceSpawned -= UpdateGhostTiles;
        PieceMovement.OnPieceMovement -= UpdateGhostTiles;
        PieceRotation.OnPieceRotation += UpdateGhostTiles;
    }
    private void Awake()
    {
        InitializeGhostTiles();
    }
    private void InitializeGhostTiles()
    {
        ghostTiles = new TileController[4];
        {
            for(int i = 1; i <= ghostTiles.Length; i++)
            {
                string tileName = "GhostTile" + i;
                TileController newTile = transform.Find("GhostTiles").Find(tileName).GetComponent<TileController>();
                ghostTiles[i - 1] = newTile;
            }
            
        }
    }
    private void UpdateGhostTiles()
    {
        for (int i = 1; i <= ghostTiles.Length; i++)
        {
            Vector2Int newPos = new Vector2Int((int) PieceController.Tiles[i - 1].gameObject.transform.position.x,
                (int) PieceController.Tiles[i - 1].gameObject.transform.position.y);
            ghostTiles[i - 1].UpdatePosition(newPos);
        }
        SendGhostPieceToFloor();
    }
    private void SendGhostPieceToFloor()
    {
        while(MoveGhostPiece(Vector2Int.down)) {}
    }
    private bool MoveGhostPiece(Vector2Int movement)
    {
        foreach (var tile in ghostTiles)
        {
            if (!tile.CanTileMove(movement+ tile.coordinates))
            {
                return false;
            }
        }
        foreach (var tile in ghostTiles)
        {
            tile.MoveTile(movement);
        }
        return true;
    }
}
