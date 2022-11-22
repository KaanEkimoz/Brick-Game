using System;
using InGame;
using UnityEngine;
using Utils;

namespace Piece
{
    public class PieceRotation : MonoBehaviour
    {
        //Given prefab's Type
        public PieceType curType;
        public Vector2Int[,] JLSTZ_OFFSET_DATA { get; private set; }
        public Vector2Int[,] I_OFFSET_DATA { get; private set; }
        public Vector2Int[,] O_OFFSET_DATA { get; private set; }

        private PieceMovement _pieceMovement;

        public static Action OnPieceRotation;
        private int RotationIndex { get; set; }

        private void Awake()
        {
            RotationIndex = 0;
        
            JLSTZ_OFFSET_DATA = new Vector2Int[5, 4];
            JLSTZ_OFFSET_DATA[0, 0] = Vector2Int.zero;
            JLSTZ_OFFSET_DATA[0, 1] = Vector2Int.zero;
            JLSTZ_OFFSET_DATA[0, 2] = Vector2Int.zero;
            JLSTZ_OFFSET_DATA[0, 3] = Vector2Int.zero;

            JLSTZ_OFFSET_DATA[1, 0] = Vector2Int.zero;
            JLSTZ_OFFSET_DATA[1, 1] = new Vector2Int(1,0);
            JLSTZ_OFFSET_DATA[1, 2] = Vector2Int.zero;
            JLSTZ_OFFSET_DATA[1, 3] = new Vector2Int(-1, 0);

            JLSTZ_OFFSET_DATA[2, 0] = Vector2Int.zero;
            JLSTZ_OFFSET_DATA[2, 1] = new Vector2Int(1, -1);
            JLSTZ_OFFSET_DATA[2, 2] = Vector2Int.zero;
            JLSTZ_OFFSET_DATA[2, 3] = new Vector2Int(-1, -1);

            JLSTZ_OFFSET_DATA[3, 0] = Vector2Int.zero;
            JLSTZ_OFFSET_DATA[3, 1] = new Vector2Int(0, 2);
            JLSTZ_OFFSET_DATA[3, 2] = Vector2Int.zero;
            JLSTZ_OFFSET_DATA[3, 3] = new Vector2Int(0, 2);

            JLSTZ_OFFSET_DATA[4, 0] = Vector2Int.zero;
            JLSTZ_OFFSET_DATA[4, 1] = new Vector2Int(1, 2);
            JLSTZ_OFFSET_DATA[4, 2] = Vector2Int.zero;
            JLSTZ_OFFSET_DATA[4, 3] = new Vector2Int(-1, 2);

            I_OFFSET_DATA = new Vector2Int[5, 4];
            I_OFFSET_DATA[0, 0] = Vector2Int.zero;
            I_OFFSET_DATA[0, 1] = new Vector2Int(-1, 0);
            I_OFFSET_DATA[0, 2] = new Vector2Int(-1, 1);
            I_OFFSET_DATA[0, 3] = new Vector2Int(0, 1);

            I_OFFSET_DATA[1, 0] = new Vector2Int(-1, 0);
            I_OFFSET_DATA[1, 1] = Vector2Int.zero;
            I_OFFSET_DATA[1, 2] = new Vector2Int(1, 1);
            I_OFFSET_DATA[1, 3] = new Vector2Int(0, 1);

            I_OFFSET_DATA[2, 0] = new Vector2Int(2, 0);
            I_OFFSET_DATA[2, 1] = Vector2Int.zero;
            I_OFFSET_DATA[2, 2] = new Vector2Int(-2, 1);
            I_OFFSET_DATA[2, 3] = new Vector2Int(0, 1);

            I_OFFSET_DATA[3, 0] = new Vector2Int(-1, 0);
            I_OFFSET_DATA[3, 1] = new Vector2Int(0, 1);
            I_OFFSET_DATA[3, 2] = new Vector2Int(1, 0);
            I_OFFSET_DATA[3, 3] = new Vector2Int(0, -1);

            I_OFFSET_DATA[4, 0] = new Vector2Int(2, 0);
            I_OFFSET_DATA[4, 1] = new Vector2Int(0, -2);
            I_OFFSET_DATA[4, 2] = new Vector2Int(-2, 0);
            I_OFFSET_DATA[4, 3] = new Vector2Int(0, 2);

            O_OFFSET_DATA = new Vector2Int[1, 4];
            O_OFFSET_DATA[0, 0] = Vector2Int.zero;
            O_OFFSET_DATA[0, 1] = Vector2Int.down;
            O_OFFSET_DATA[0, 2] = new Vector2Int(-1, -1);
            O_OFFSET_DATA[0, 3] = Vector2Int.left;

            _pieceMovement = GetComponent<PieceMovement>();
        }
        /// <summary>
        /// Rotates the piece by 90 degrees in specified direction. Offset operations should almost always be attempted,
        /// unless you are rotating the piece back to its original position.
        /// </summary>
        /// <param name="clockwise">Set to true if rotating clockwise. Set to False if rotating CCW</param>
        /// <param name="shouldOffset">Set to true if offset operations should be attempted.</param>
        public void RotatePiece(bool clockwise, bool shouldOffset)
        {
            int oldRotationIndex = RotationIndex;
            RotationIndex += clockwise ? 1 : -1;
            RotationIndex = EkimozUtils.Mod(RotationIndex, 4);

            foreach (var t in PieceController.Tiles)
            {
                t.RotateTile(PieceController.Tiles[0].coordinates, clockwise);
            }

            if (!shouldOffset)
            {
                return;
            }

            bool canOffset = Offset(oldRotationIndex, RotationIndex);

            if (!canOffset)
            {
                RotatePiece(!clockwise, false);
            }
        
            OnPieceRotation?.Invoke();
        }
        /// <summary>
        /// Performs 5 tests on the piece to find a valid final location for the piece.
        /// </summary>
        /// <param name="oldRotIndex">Original rotation index of the piece</param>
        /// <param name="newRotIndex">Rotation index the piece will be rotating to</param>
        /// <returns>True if one of the tests passed and a final location was found. False if all test failed.</returns>
        bool Offset(int oldRotIndex, int newRotIndex)
        {
            Vector2Int[,] curOffsetData;
        
            if(curType == PieceType.O)
            {
                curOffsetData = O_OFFSET_DATA;
            }
            else if(curType == PieceType.I)
            {
                curOffsetData = I_OFFSET_DATA;
            }
            else
            {
                curOffsetData = JLSTZ_OFFSET_DATA;
            }

            Vector2Int endOffset = Vector2Int.zero;

            bool movePossible = false;

            for (int testIndex = 0; testIndex < 5; testIndex++)
            {
                var offsetVal1 = curOffsetData[testIndex, oldRotIndex];
                var offsetVal2 = curOffsetData[testIndex, newRotIndex];
                endOffset = offsetVal1 - offsetVal2;
                if (_pieceMovement.CanMovePiece(endOffset))
                {
                    movePossible = true;
                    break;
                }
            }
            if (movePossible)
            {
                _pieceMovement.MovePiece(endOffset);
            }
            else
            {
                Debug.Log("Move impossible");
            }
            return movePossible;
        }
    
    }
}
