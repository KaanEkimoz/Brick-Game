using UnityEngine;

namespace Board
{
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

            // Anchor each grid cell to BoardController's own world position so the playfield
            // can be moved as a single unit in the scene without rewriting every cell. Falls back
            // to boardParent.position if used directly during board construction (Instance is
            // already assigned in Awake, well before CreateGrid runs in Start).
            Vector3 origin = boardParent != null ? boardParent.position : Vector3.zero;
            gridUnit.transform.position = origin + new Vector3(location.x, location.y);
        }
    }
}
