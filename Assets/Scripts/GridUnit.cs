using UnityEngine;
public class GridUnit
{
    public GameObject gridUnit { get; }
    public GameObject tileOnGridUnit;
    public Vector2Int location { get; }
    public bool isOccupied;
    public GridUnit(GameObject newGameObject, Transform boardParent, int x, int y)
    {
        gridUnit = GameObject.Instantiate(newGameObject, boardParent);
        location = new Vector2Int(x, y);
        isOccupied = false;

        gridUnit.transform.position = new Vector3(location.x, location.y);
    }
}
