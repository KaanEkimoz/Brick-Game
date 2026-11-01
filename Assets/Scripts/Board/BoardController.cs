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
        /// <summary>Fires once per individual cleared row (y coordinate). Used by VFX.</summary>
        public static Action<int> OnLineCleared;
        /// <summary>Fires when a 4-line Tetris clear happens. Used by celebration VFX.</summary>
        public static Action OnTetrisCleared;
        /// <summary>Fires when ClearBoard wipes the run, including the in-place Restart.</summary>
        public static Action OnBoardCleared;
        /// <summary>When true, full rows stay on the board. Set by levels where only abilities clear tiles.</summary>
        public static bool LineClearsDisabled;
        public static BoardController Instance;

        /// <summary>World-space origin used when placing grid units, tiles and ghost tiles.
        /// Each cell's world position is WorldOrigin + (cellX, cellY). Moving the BoardController
        /// GameObject in the scene shifts the whole playfield as one — used to align the visible
        /// board with the UI grid frame in the redesigned mobile layout. Returns Vector3.zero if
        /// the singleton isn't ready yet (e.g. very early scene boot).</summary>
        public static Vector3 WorldOrigin => Instance != null ? Instance.transform.position : Vector3.zero;

        /// <summary>Convert a cell coordinate to its world-space position, taking WorldOrigin
        /// into account. Use this everywhere you'd otherwise write <c>new Vector3(cell.x, cell.y)</c>
        /// — VFX particle emissions, save restore, ability anchors, etc. — so the visual stays
        /// glued to the board when the playfield is moved as a unit.</summary>
        public static Vector3 CellToWorld(Vector2Int cell) => WorldOrigin + new Vector3(cell.x, cell.y);
        public static Vector3 CellToWorld(float x, float y) => WorldOrigin + new Vector3(x, y);

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
            Themes.ThemeManager.TintGrid(transform);
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

        // === Extended-mode ability clears ===
        // These partial clears do NOT cascade rows above (unlike full line clears), so they leave
        // gaps the player can stack on. CheckLineClears is still called afterwards by the settle
        // pipeline, so any full rows exposed by the ability will be counted normally.

        /// <summary>Clears every occupied cell inside a box centred on (cx, cy) with the given radius, then lets each affected column settle under gravity. Returns the number of cells destroyed.</summary>
        public int ClearBoxCells(int cx, int cy, int radius)
        {
            Vector2Int center = new Vector2Int(cx, cy);
            return ClearRectCells(center - Vector2Int.one * radius, center + Vector2Int.one * radius);
        }

        /// <summary>Clears every tile in the inclusive area, then settles its columns. Returns the number destroyed.</summary>
        public int ClearRectCells(Vector2Int min, Vector2Int max)
        {
            int count = 0;
            for (int y = min.y; y <= max.y; y++)
            {
                for (int x = min.x; x <= max.x; x++)
                {
                    if (x < 0 || x >= gridSizeX || y < 0 || y >= gridSizeY) continue;
                    if (DestroyTileAt(x, y)) count++;
                }
            }
            if (count > 0)
            {
                for (int x = min.x; x <= max.x; x++)
                {
                    if (x >= 0 && x < gridSizeX) CompactColumn(x);
                }
            }
            return count;
        }

        /// <summary>Clears every occupied cell in the given row, then cascades the rows above downward. Returns the number of cells destroyed.</summary>
        public int ClearRowCells(int y)
        {
            if (y < 0 || y >= gridSizeY) return 0;
            int count = 0;
            for (int x = 0; x < gridSizeX; x++)
                if (DestroyTileAt(x, y)) count++;
            if (count > 0)
                DropTheLine(y + 1);
            return count;
        }

        /// <summary>Clears every occupied cell in the given column. Returns the number of cells destroyed.</summary>
        public int ClearColumnCells(int x)
        {
            if (x < 0 || x >= gridSizeX) return 0;
            int count = 0;
            for (int y = 0; y < gridSizeY; y++)
                if (DestroyTileAt(x, y)) count++;
            return count;
        }

        /// <summary>Collects the lowest covered holes, bottom row first, up to maxCells. A hole is an empty cell with a tile above it in the same column. landingTiles lets the target preview count a piece that has not settled yet; pass null once it has.</summary>
        public void CollectPatchCells(int maxCells, IList<Vector2Int> landingTiles, List<Vector2Int> results)
        {
            results.Clear();
            for (int y = 0; y < gridSizeY; y++)
            {
                for (int x = 0; x < gridSizeX; x++)
                {
                    Vector2Int cell = new Vector2Int(x, y);
                    if (_fullGrid[x, y].isOccupied) continue;
                    if (landingTiles != null && landingTiles.Contains(cell)) continue;
                    if (!HasTileAbove(cell, landingTiles)) continue;
                    results.Add(cell);
                    if (results.Count == maxCells) return;
                }
            }
        }

        private bool HasTileAbove(Vector2Int cell, IList<Vector2Int> landingTiles)
        {
            for (int y = cell.y + 1; y < gridSizeY; y++)
            {
                if (_fullGrid[cell.x, y].isOccupied) return true;
                if (landingTiles != null && landingTiles.Contains(new Vector2Int(cell.x, y))) return true;
            }
            return false;
        }

        /// <summary>Places a clone of the given tile on each cell.</summary>
        public void FillCells(List<Vector2Int> cells, GameObject tileTemplate)
        {
            foreach (Vector2Int cell in cells)
            {
                GameObject tileGo = Instantiate(tileTemplate, transform);
                tileGo.transform.localScale = tileTemplate.transform.lossyScale;
                TileController tile = tileGo.GetComponent<TileController>();
                tile.tileIndex = -1; // sentinel: not part of the active piece's Tiles array
                tile.UpdatePosition(cell);
                OccupyPos(cell, tileGo);
            }
        }

        private bool DestroyTileAt(int x, int y)
        {
            GridUnit unit = _fullGrid[x, y];
            if (!unit.isOccupied) return false;
            if (GetKind(unit) == TileKind.Fixed) return false;
            if (unit.tileOnGridUnit != null) Destroy(unit.tileOnGridUnit);
            unit.tileOnGridUnit = null;
            unit.isOccupied = false;
            return true;
        }

        /// <summary>
        /// Pulls every occupied cell in column x down to fill any gaps, without touching other columns.
        /// Used by the bomb ability so the tiles above a 3x3 hole fall into it. Fixed tiles hold
        /// their cell, so tiles above one settle on top of it.
        /// </summary>
        private void CompactColumn(int x)
        {
            int writeY = 0;
            for (int y = 0; y < gridSizeY; y++)
            {
                if (!_fullGrid[x, y].isOccupied) continue;
                if (GetKind(_fullGrid[x, y]) == TileKind.Fixed)
                {
                    writeY = y + 1;
                    continue;
                }
                if (y != writeY)
                {
                    GameObject tileGo = _fullGrid[x, y].tileOnGridUnit;
                    TileController tc = tileGo != null ? tileGo.GetComponent<TileController>() : null;
                    if (tc != null) tc.UpdatePosition(new Vector2Int(x, writeY));
                    _fullGrid[x, writeY].isOccupied = true;
                    _fullGrid[x, writeY].tileOnGridUnit = tileGo;
                    _fullGrid[x, y].isOccupied = false;
                    _fullGrid[x, y].tileOnGridUnit = null;
                }
                writeY++;
            }
        }

        // === Levels hooks ===

        private static TileKind GetKind(GridUnit unit)
        {
            TileController tile = unit.tileOnGridUnit != null ? unit.tileOnGridUnit.GetComponent<TileController>() : null;
            return tile != null ? tile.kind : TileKind.Normal;
        }

        /// <summary>Clones a tile onto a cell with the given look.</summary>
        public void PlaceTile(Vector2Int cell, GameObject template, Sprite sprite, Color color, TileKind kind)
        {
            GameObject tileGo = Instantiate(template, transform);
            tileGo.transform.localScale = template.transform.lossyScale;
            tileGo.transform.localRotation = Quaternion.identity;
            SpriteRenderer renderer = tileGo.GetComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;
            TileController tile = tileGo.GetComponent<TileController>();
            tile.tileIndex = -1; // sentinel: not part of the active piece's Tiles array
            tile.kind = kind;
            tile.UpdatePosition(cell);
            OccupyPos(cell, tileGo);
        }

        /// <summary>Lifts the board one row and fills the bottom row except one gap. Returns false, leaving the board as it was, when the top row is occupied.</summary>
        public bool PushGarbageRow(int gapX, GameObject template, Sprite sprite)
        {
            for (int x = 0; x < gridSizeX; x++)
                if (_fullGrid[x, gridSizeY - 1].isOccupied) return false;

            for (int y = gridSizeY - 2; y >= 0; y--)
            {
                for (int x = 0; x < gridSizeX; x++)
                {
                    GridUnit from = _fullGrid[x, y];
                    if (!from.isOccupied) continue;
                    GridUnit to = _fullGrid[x, y + 1];
                    from.tileOnGridUnit.GetComponent<TileController>().UpdatePosition(new Vector2Int(x, y + 1));
                    to.isOccupied = true;
                    to.tileOnGridUnit = from.tileOnGridUnit;
                    from.isOccupied = false;
                    from.tileOnGridUnit = null;
                }
            }

            for (int x = 0; x < gridSizeX; x++)
                if (x != gapX) PlaceTile(new Vector2Int(x, 0), template, sprite, Color.white, TileKind.Normal);
            return true;
        }

        public int CountTiles(TileKind kind)
        {
            int count = 0;
            foreach (GridUnit unit in _fullGrid)
                if (unit.isOccupied && GetKind(unit) == kind) count++;
            return count;
        }

        /// <summary>Tiles that still have to go for the board to count as cleared.</summary>
        public int CountClearableTiles() => CountTiles(TileKind.Normal) + CountTiles(TileKind.Gold);

        // === Persistence hooks ===

        /// <summary>Total cleared-line counter; used by the save system to persist board progress.</summary>
        public int TotalClearedLines => _totalClearedLines;

        /// <summary>Sets the cleared-line counter when restoring from a save file.
        /// Also fires OnTotalClearedLinesChanged so listeners (Lines counter HUD,
        /// LevelController's drop-speed table) re-sync to the restored value
        /// instead of staying at zero until the next live line clear.</summary>
        public void SetTotalClearedLines(int value)
        {
            _totalClearedLines = value;
            OnTotalClearedLinesChanged?.Invoke(_totalClearedLines);
        }

        /// <summary>Enumerates every settled tile on the board — used by the save system to serialize grid state.</summary>
        public IEnumerable<KeyValuePair<Vector2Int, GameObject>> GetOccupiedTiles()
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                for (int x = 0; x < gridSizeX; x++)
                {
                    if (_fullGrid[x, y].isOccupied)
                    {
                        yield return new KeyValuePair<Vector2Int, GameObject>(
                            new Vector2Int(x, y), _fullGrid[x, y].tileOnGridUnit);
                    }
                }
            }
        }
        /// <summary>
        /// Checks line by line from bottom to top to see if that line is full and should be cleared.
        /// </summary>
        public void CheckLineClears()
        {
            if (LineClearsDisabled)
            {
                OnLinesCleared?.Invoke(0);
                return;
            }

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
                        OnTetrisCleared?.Invoke();
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
                    // Without fixed tiles the cell below is always free here; with them, tiles
                    // stop on top of whatever did not move.
                    if (curGridUnit.isOccupied && GetKind(curGridUnit) != TileKind.Fixed
                        && !_fullGrid[x, lineToDrop - 1].isOccupied)
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
                if (GetKind(_fullGrid[x, lineToClear]) == TileKind.Fixed)
                    continue;
                // Restored board tiles carry tileIndex = -1 and do not belong to PieceController.Tiles.
                int ti = _fullGrid[x, lineToClear].tileOnGridUnit.GetComponent<TileController>().tileIndex;
                if (ti >= 0 && ti < PieceController.Tiles.Length)
                    PieceController.Tiles[ti] = null;
                Destroy(_fullGrid[x, lineToClear].tileOnGridUnit);
                _fullGrid[x, lineToClear].tileOnGridUnit = null;
                _fullGrid[x, lineToClear].isOccupied = false;
            }
            OnLineCleared?.Invoke(lineToClear);
            _totalClearedLines++;
            OnTotalClearedLinesChanged(_totalClearedLines);
        }
        public void ClearBoard()
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                for (int x = 0; x < gridSizeX; x++)
                {
                    Destroy(_fullGrid[x, y].tileOnGridUnit);
                    _fullGrid[x, y].tileOnGridUnit = null;
                    _fullGrid[x, y].isOccupied = false;
                }
            }
            _totalClearedLines = 0;
            // The GameOver/Pause Restart buttons call ClearBoard() to reset the run
            // in-place (without reloading the scene). The Lines HUD listens to
            // OnTotalClearedLinesChanged — without this invoke, the counter stays
            // frozen at the previous run's value until the next live line clear.
            OnTotalClearedLinesChanged?.Invoke(_totalClearedLines);
            OnBoardCleared?.Invoke();
        }
    }
}
