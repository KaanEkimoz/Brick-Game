using System.Collections;
using System.Collections.Generic;
using Board;
using Extras;
using InGame;
using Piece;
using Tiles;
using UnityEngine;

namespace Persistence
{
    /// <summary>
    /// Autosaves after every piece spawn and restores the game from disk when the player taps "Continue".
    /// </summary>
    /// <remarks>
    /// Snapshot timing: subscribed to <see cref="PieceSpawner.OnPieceSpawned"/>, which fires right after a new
    /// piece is instantiated but before it drops. At this moment the board only holds settled tiles, so the
    /// saved state perfectly mirrors the on-screen image when the save was written.
    /// </remarks>
    public class GameSaveManager : MonoBehaviour
    {
        [Header("Scene References")]
        [SerializeField] private PieceSpawner _pieceSpawner;
        [SerializeField] private BoardController _boardController;
        [SerializeField] private ScoreController _scoreController;
        [SerializeField] private LevelController _levelController;
        [SerializeField] private PiecesController _piecesController;

        [Header("Continue Prompt")]
        [Tooltip("Panel shown on scene start when a save exists. Contains Continue + New Game buttons hooked to OnContinueClicked / OnNewGameClicked.")]
        [SerializeField] private GameObject _continuePromptPanel;

#if UNITY_EDITOR
        [Header("Test (Editor only)")]
        [Tooltip("EDITOR ONLY — skip the Continue/New-Game prompt and always start a fresh game " +
                 "(handy while testing the tutorial). Compiled out of device builds, so players " +
                 "still get the normal Continue prompt.")]
        [SerializeField] private bool _editorAlwaysNewGame = true;
#endif

        [Tooltip("Start menu root hidden while the continue prompt is visible so the player cannot bypass the prompt by tapping Play.")]
        [SerializeField] private GameObject _startMenuPanel;

        [Header("Restored Tile (optional)")]
        [Tooltip("Optional prefab cloned when restoring board tiles. If null, a runtime GameObject with SpriteRenderer + TileController is created instead.")]
        [SerializeField] private GameObject _restoredTilePrefab;

        private bool _restoring;
        private bool _saveHandled;

        private void OnEnable()
        {
            PieceSpawner.OnPieceSpawned += HandlePieceSpawnedAutosave;
            PiecesController.OnGameOver += HandleGameOverClearSave;
        }

        private void OnDisable()
        {
            PieceSpawner.OnPieceSpawned -= HandlePieceSpawnedAutosave;
            PiecesController.OnGameOver -= HandleGameOverClearSave;
        }

        private IEnumerator Start()
        {
            // Wait one frame so BoardController.Start() has created the grid.
            yield return null;

            if (_continuePromptPanel != null)
                _continuePromptPanel.SetActive(false);
        }

        /// <summary>
        /// Hook to the Play button's OnClick. Shows the continue prompt only when the saved run has real
        /// progress (score &gt; 0); otherwise deletes the stale save and starts a new game.
        /// </summary>
        public void OnPlayRequested()
        {
#if UNITY_EDITOR
            if (_editorAlwaysNewGame)
            {
                // Testing convenience: never prompt, always start fresh in the Editor.
                if (GameSaveStorage.HasSave()) GameSaveStorage.Delete();
                _saveHandled = true;
                if (_piecesController != null) _piecesController.StartGame();
                return;
            }
#endif
            if (GameSaveStorage.HasSave())
            {
                GameSaveData data = GameSaveStorage.Load();
                if (data != null && data.score > 0 && _continuePromptPanel != null)
                {
                    _continuePromptPanel.SetActive(true);
                    return;
                }

                // Save exists but has nothing worth resuming — drop it so the prompt never lies.
                GameSaveStorage.Delete();
            }

            _saveHandled = true;
            if (_piecesController != null)
                _piecesController.StartGame();
        }

        // === UI Button Hooks (wire these up in the Inspector) ===

        /// <summary>Hook to the "Continue" button's OnClick.</summary>
        public void OnContinueClicked()
        {
            if (_saveHandled) return;
            _saveHandled = true;

            if (_continuePromptPanel != null)
                _continuePromptPanel.SetActive(false);

            GameSaveData data = GameSaveStorage.Load();
            if (data == null)
                return;

            RestoreFromData(data);
        }

        /// <summary>Hook to the "New Game" button's OnClick.</summary>
        public void OnNewGameClicked()
        {
            if (_saveHandled) return;
            _saveHandled = true;

            GameSaveStorage.Delete();
            if (_continuePromptPanel != null)
                _continuePromptPanel.SetActive(false);

            if (_piecesController != null)
                _piecesController.StartGame();
        }

        // === Autosave ===

        private void HandlePieceSpawnedAutosave()
        {
            if (_restoring) return; // the spawn that restores from save must not overwrite itself
            if (_pieceSpawner == null || _boardController == null) return;

            // Skip the autosave while the run still has zero meaningful progress.
            // Without this guard a first-time player who taps Play, sees a piece spawn,
            // then closes the app would leave a score=0 save behind; the next launch
            // would still pop the Continue prompt (HasSave() true, even if the score
            // gate later filters it). Wait for the first cleared line before writing.
            if (ScoreController.score <= 0 && _boardController.TotalClearedLines <= 0)
                return;

            GameSaveData data = new GameSaveData
            {
                score = ScoreController.score,
                totalClearedLines = _boardController.TotalClearedLines,
                currentLevel = LevelController.CurrentLevel,
                currentPieceType = GetCurrentPieceType(),
                nextPieceType = PieceSpawner.NextPieceType,
                // Capture the 7-bag's remaining draws so a resumed run picks up exactly where it left off.
                bagState = PieceBag.Snapshot(),
            };

            foreach (KeyValuePair<Vector2Int, GameObject> kv in _boardController.GetOccupiedTiles())
            {
                int spriteIndex = ResolveSpriteIndex(kv.Value);
                data.tiles.Add(new TileSaveData(kv.Key.x, kv.Key.y, spriteIndex));
            }

            GameSaveStorage.Save(data);
        }

        private PieceType GetCurrentPieceType()
        {
            // PieceSpawner keeps _curPieceType private; read it off the active piece's PieceRotation component instead.
            if (PiecesController.CurPiece == null)
                return PieceSpawner.NextPieceType;

            PieceRotation rot = PiecesController.CurPiece.GetComponent<PieceRotation>();
            return rot != null ? rot.curType : PieceSpawner.NextPieceType;
        }

        private int ResolveSpriteIndex(GameObject tileGo)
        {
            if (_pieceSpawner == null || tileGo == null)
                return 0;

            SpriteRenderer sr = tileGo.GetComponent<SpriteRenderer>();
            if (sr == null || sr.sprite == null)
                return 0;

            Sprite[] palette = _pieceSpawner.TileSprites;
            for (int i = 0; i < palette.Length; i++)
            {
                if (palette[i] == sr.sprite)
                    return i;
            }
            return 0;
        }

        // === Restore ===

        private void HandleGameOverClearSave()
        {
            GameSaveStorage.Delete();
        }

        private void RestoreFromData(GameSaveData data)
        {
            _restoring = true;

            try
            {
                // 1) Put settled tiles back on the board.
                foreach (TileSaveData tileData in data.tiles)
                {
                    GameObject tileGo = CreateRestoredTile(tileData);
                    _boardController.OccupyPos(new Vector2Int(tileData.x, tileData.y), tileGo);
                }

                // 2) Restore counters (UI + drop-speed).
                if (_scoreController != null)
                    _scoreController.LoadScore(data.score);

                if (_levelController != null)
                    _levelController.LoadLevel(data.currentLevel);

                _boardController.SetTotalClearedLines(data.totalClearedLines);

                // 3) Restore the 7-bag exactly as it was when the run was paused. Empty list
                // (v1 saves or fresh bags) lets PieceBag refill + reshuffle on the next draw.
                PieceBag.RestoreFromSnapshot(data.bagState);

                // 4) Spawn ghost + active piece with the saved types (bypasses the random roll).
                _pieceSpawner.SpawnGhostPiece();
                _pieceSpawner.SpawnPieceFromSave(data.currentPieceType, data.nextPieceType);
            }
            finally
            {
                _restoring = false;
            }
        }

        private GameObject CreateRestoredTile(TileSaveData tileData)
        {
            GameObject go;

            if (_restoredTilePrefab != null)
            {
                go = Instantiate(_restoredTilePrefab, _boardController.transform);
            }
            else
            {
                go = new GameObject($"RestoredTile_{tileData.x}_{tileData.y}");
                go.transform.SetParent(_boardController.transform);
                go.AddComponent<SpriteRenderer>();
                go.AddComponent<TileController>();
            }

            // Apply the board's WorldOrigin so restored tiles land where the live grid renders.
            go.transform.position = Board.BoardController.CellToWorld(tileData.x, tileData.y);

            SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
            if (sr != null && _pieceSpawner != null)
            {
                Sprite[] palette = _pieceSpawner.TileSprites;
                if (tileData.spriteIndex >= 0 && tileData.spriteIndex < palette.Length)
                    sr.sprite = palette[tileData.spriteIndex];
            }

            TileController tc = go.GetComponent<TileController>();
            if (tc != null)
            {
                tc.coordinates = new Vector2Int(tileData.x, tileData.y);
                tc.tileIndex = -1; // sentinel: not part of the active piece's Tiles array
            }

            return go;
        }
    }
}
