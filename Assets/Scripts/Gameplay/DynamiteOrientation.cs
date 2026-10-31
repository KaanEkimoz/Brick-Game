using Piece;
using UnityEngine;

namespace Gameplay
{
    /// <summary>Keeps the two halves of the dynamite image lined up with the piece as it rotates.</summary>
    public class DynamiteOrientation : MonoBehaviour
    {
        public void Apply()
        {
            Vector2Int direction = PieceController.Tiles[1].coordinates - PieceController.Tiles[0].coordinates;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            foreach (var tile in PieceController.Tiles)
                tile.transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }
}
