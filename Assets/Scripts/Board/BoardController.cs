using System;
using System.Collections.Generic;
using Piece;
using Tiles;
using UnityEngine;

namespace Board
{
    public class BoardController : MonoBehaviour
    {
        [Space]
        public GameObject gridUnitPrefab;
        [Space]
        public int gridSizeX, gridSizeY;
        [Space]
        public GameObject tetrisText;
    
        public static Action<int> OnLinesCleared;
        public static Action<int> OnTotalClearedLinesChanged;
        public static BoardController Instance;
   
        private int _totalClearedLines;
        private GridUnit[,] _fullGrid;
        private void Awake()
        {
            #region Singleton

            if (Instance != null && Instance != this) 
            { 
                Destroy(this); 
            } 
            else 
            { 
                Instance = this; 
            }

            #endregion
        }
        void Start()
        {
            CreateGrid();
            tetrisText.SetActive(false);
        }
        private void CreateGrid()
        {
            _fullGrid = new GridUnit[gridSizeX, gridSizeY];
            for(int y = 0; y < gridSizeY; y++)
            {
                for(int x = 0; x < gridSizeX; x++)
                {
                    GridUnit newGridUnit = new GridUnit(gridUnitPrefab, transform, x, y);
                    _fullGrid[x, y] = newGridUnit;
                }
            }
        }
        /// <summary>
        /// Checks to see if the coordinate is a valid coordinate on the current tetris board.
        /// </summary>
        /// <param name="coordToTest">The x,y coordinate to test</param>
        /// <returns>Returns true if the coordinate to test is a vaild coordinate on the tetris board</returns>
        public bool IsInGrid(Vector2Int coordToTest)
        {
            return coordToTest.x >= 0 && coordToTest.x < gridSizeX && coordToTest.y >= 0;
        }
        /// <summary>
        /// Checks to see if a given coordinate is occupied by a tetris piece
        /// </summary>
        /// <param name="coordToTest">The x,y coordinate to test</param>
        /// <returns>Returns true if the coordinate is not occupied by a tetris piece</returns>
        public bool IsPosEmpty(Vector2Int coordToTest)
        {
            //if it is not inside of the Grid, it's empty
            if(coordToTest.y >= gridSizeY)
            {
                return true;
            }
            return !_fullGrid[coordToTest.x, coordToTest.y].isOccupied;
        }
        /// <summary>
        /// Called when a piece is set in place. Sets the grid location to an occupied state.
        /// </summary>
        /// <param name="coords">The x,y coordinates to be occupied.</param>
        /// <param name="tileGameObject">GameObject of the specific tile on this grid location.</param>
        public void OccupyPos(Vector2Int coords, GameObject tileGameObject)
        {
            _fullGrid[coords.x, coords.y].isOccupied = true;
            _fullGrid[coords.x, coords.y].tileOnGridUnit = tileGameObject;
        }
        /// <summary>
        /// Checks line by line from bottom to top to see if that line is full and should be cleared.
        /// </summary>
        public void CheckLineClears()
        {
            //List of indexes for the lines that need to be cleared.
            List<int> linesToClear = new List<int>();

            //Counts how many lines next to each other will be cleared.
            //If this count get to four lines, that is a Tetris line clear.
            int consecutiveLineClears = 0;

            for(int y = 0; y < gridSizeY; y++)
            {
                bool lineClear = true;
                for(int x = 0; x < gridSizeX; x++)
                {
                    if (!_fullGrid[x, y].isOccupied)
                        lineClear = false;
                }
                if (lineClear)
                {
                    linesToClear.Add(y);
                    consecutiveLineClears++;
                    if (consecutiveLineClears == 4)
                    {
                        ShowTetrisText();
                    }
                    ClearLine(y);
                }
            }
            OnLinesCleared?.Invoke(consecutiveLineClears);
        

            if (linesToClear.Count <= 0) 
                return;
            
            for(int i = 0; i < linesToClear.Count; i++)
                DropTheLine(linesToClear[i] + 1 - i);
        }
        private void DropTheLine(int dropLineIndex)
        {
            /* The initial index of dropLineIndex is calculated by taking the index of the first line
                 that was cleared then adding 1 to indicate the index of the line above the cleared line,
                 then the value i is subtracted to compensate for any lines already cleared.
                 */
            for (int lineToDrop = dropLineIndex; lineToDrop < gridSizeY; lineToDrop++)
            {
                for (int x = 0; x < gridSizeX; x++)
                {
                    GridUnit curGridUnit = _fullGrid[x, lineToDrop];
                    if (curGridUnit.isOccupied)
                    {
                        MoveTileDown(curGridUnit);
                    }
                }
            }
        }
        /// <summary>
        /// Displays the Tetris text when a Tetris line clear is achieved.
        /// </summary>
        void ShowTetrisText()
        {
            tetrisText.SetActive(true);
            Invoke("HideTetrisText", 3f);
        }
        /// <summary>
        /// Hides the Tetris line clear text.
        /// </summary>
        void HideTetrisText()
        {
            tetrisText.SetActive(false);
        }
        /// <summary>
        /// Moves an individual tile down one unit.
        /// </summary>
        /// <param name="curGridUnit">The grid unit that contains the tile to be moved down</param>
        void MoveTileDown(GridUnit curGridUnit)
        {
            TileController curTile = curGridUnit.tileOnGridUnit.GetComponent<TileController>();
            curTile.MoveTile(Vector2Int.down);
            curTile.SetTile();
            curGridUnit.tileOnGridUnit = null;
            curGridUnit.isOccupied = false;
        }
        /// <summary>
        /// Clears all tiles from a specified line
        /// </summary>
        /// <param name="lineToClear">Index of the line to be cleared</param>
        void ClearLine(int lineToClear)
        {
            if(lineToClear < 0 || lineToClear > gridSizeY)
            {
                Debug.LogError("Error: Cannot Clear Line: " + lineToClear);
                return;
            }
            for(int x = 0; x < gridSizeX; x++)
            {
                PieceController.Tiles[_fullGrid[x, lineToClear].tileOnGridUnit.GetComponent<TileController>().tileIndex] = null;
                Destroy(_fullGrid[x, lineToClear].tileOnGridUnit);
                _fullGrid[x, lineToClear].tileOnGridUnit = null;
                _fullGrid[x, lineToClear].isOccupied = false;
            }
            _totalClearedLines++;
            OnTotalClearedLinesChanged(_totalClearedLines);
        }
    }
}
