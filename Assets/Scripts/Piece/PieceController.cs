using UnityEngine;

namespace Piece
{
    public class PieceController : MonoBehaviour 
    {
        public static TileController[] Tiles;
        public TileController[] tiles;
        private void Awake()
        {
            InitializeTiles();
        }
        private void InitializeTiles()
        {
            Tiles = tiles;
        }
    }
}