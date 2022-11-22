using Board;
using UnityEngine;

namespace Tiles
{
    public class GhostTileController : MonoBehaviour
    {
        [HideInInspector]public Vector2Int coordinates;
        /// <summary>
        /// Checks to see if the tile can be moved to the specified position.
        /// </summary>
        /// <param name="endPos">Coordinates of the position you are trying to move the tile to</param>
        /// <returns>True if the tile can be moved there. False if the tile cannot be moved there</returns>
        public bool CanTileMove(Vector2Int endPos)
        {
            if (!BoardController.Instance.IsInGrid(endPos))
            {
                return false;
            }
            if (!BoardController.Instance.IsPosEmpty(endPos))
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// Moves the tile by the specified amount
        /// </summary>
        /// <param name="movement">X,Y amount the tile will be moved by</param>
        public void MoveTile(Vector2Int movement)
        {
            Vector2Int endPos = coordinates + movement;
            UpdatePosition(endPos);
        }
        /// <summary>
        /// Sets some new variables at the new position
        /// </summary>
        /// <param name="newPos">New position the tile will reside at</param>
        public void UpdatePosition(Vector2Int newPos)
        {
            coordinates = newPos;
            Vector3 newV3Pos = new Vector3(newPos.x, newPos.y);
            gameObject.transform.position = newV3Pos;
        }
    }
}