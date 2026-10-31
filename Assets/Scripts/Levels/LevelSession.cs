using System;
using Board;
using Extras;
using Gameplay;
using InGame;
using Piece;
using Tiles;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace Levels
{
    /// <summary>
    /// Runs one level: lays out the board, applies its rules, tracks the objective and ends the
    /// round. Created at runtime on every scene load, so the scene needs no wiring.
    /// </summary>
    public class LevelSession : MonoBehaviour
    {
        private const string HudCanvasName = "FrontCanvas";
        private const string NextPiecePath = "I_NextPiece";
        private const char FixedChar = '#';
        private const char GoldChar = '*';
        private const char NormalChar = 'X';
        private const float SolidTilePixelsPerUnit = 4f; // Unity's white texture is 4x4
        private static readonly Color FixedTint = new Color(0.5f, 0.53f, 0.6f, 1f);
        private static readonly Color GoldTint = new Color(1f, 0.82f, 0.15f, 1f);

        public static LevelSession Instance { get; private set; }

        /// <summary>True once a round has ended, until the next one begins.</summary>
        public static bool RoundOver { get; private set; }

        public static Action OnProgressChanged;
        public static Action<LevelDefinition> OnLevelWon;

        private BoardController _board;
        private PiecesController _pieces;
        private PieceSpawner _spawner;
        private ScoreController _score;
        private LevelController _level;
        private GameObject _startCanvas;
        private GameObject _gameOverMenu;
        private GameObject _nextPiece;

        private Sprite _solidTile;
        private bool _layoutPending;
        private int _linesCleared;
        private int _tetrises;
        private int _abilityCells;
        private int _comboStreak;
        private int _bestCombo;
        private float _timeLeft;

        public LevelDefinition Current { get; private set; }
        public int PiecesUsed { get; private set; }
        public int TimeLeftSeconds => Mathf.CeilToInt(_timeLeft);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            SceneManager.sceneLoaded += (scene, mode) => CreateRuntime();
            CreateRuntime();
        }

        private static void CreateRuntime()
        {
            if (Instance != null) return;
            GameObject go = new GameObject("LevelRuntime");
            go.AddComponent<AudioSource>();
            go.AddComponent<LevelSession>();
            go.AddComponent<LevelUi>();
        }

        private void Awake()
        {
            Instance = this;
            RoundOver = false;
            ResetRules();

            _board = BoardController.Instance;
            _pieces = PiecesController.Instance;
            _spawner = PieceSpawner.Instance;
            _score = FindAnyObjectByType<ScoreController>();
            _level = FindAnyObjectByType<LevelController>();

            foreach (Canvas canvas in Resources.FindObjectsOfTypeAll<Canvas>())
            {
                if (!canvas.gameObject.scene.IsValid()) continue;
                if (canvas.name == "StartCanvas") _startCanvas = canvas.gameObject;
                if (canvas.name == HudCanvasName) _nextPiece = canvas.transform.Find(NextPiecePath).gameObject;
            }
            GameOver gameOver = FindAnyObjectByType<GameOver>(FindObjectsInactive.Include);
            if (gameOver != null) _gameOverMenu = gameOver.GameOverScreen;
        }

        private void OnEnable()
        {
            PieceSpawner.OnPieceSpawned += HandlePieceSpawned;
            BoardController.OnBoardCleared += HandleBoardCleared;
            BoardController.OnLineCleared += HandleLineCleared;
            BoardController.OnLinesCleared += HandleLinesCleared;
            BoardController.OnTetrisCleared += HandleTetris;
            AbilityExecutor.OnAbilityCellsCleared += HandleAbilityCells;
            ScoreController.OnScoreAdded += HandleScoreAdded;
            PiecesController.OnGameOver += HandleGameOver;
        }

        private void OnDisable()
        {
            PieceSpawner.OnPieceSpawned -= HandlePieceSpawned;
            BoardController.OnBoardCleared -= HandleBoardCleared;
            BoardController.OnLineCleared -= HandleLineCleared;
            BoardController.OnLinesCleared -= HandleLinesCleared;
            BoardController.OnTetrisCleared -= HandleTetris;
            AbilityExecutor.OnAbilityCellsCleared -= HandleAbilityCells;
            ScoreController.OnScoreAdded -= HandleScoreAdded;
            PiecesController.OnGameOver -= HandleGameOver;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Update()
        {
            if (Current == null || RoundOver || Current.timeLimit <= 0) return;

            int shownBefore = TimeLeftSeconds;
            _timeLeft -= Time.deltaTime;
            if (TimeLeftSeconds != shownBefore) OnProgressChanged?.Invoke();
            if (_timeLeft <= 0f) _pieces.GameOver();
        }

        /// <summary>Starts a level from the menu, the win dialog or a retry.</summary>
        public void Begin(LevelDefinition level)
        {
            bool firstRun = PiecesController.CurPiece == null;
            Current = level;
            ApplyRules(level);
            RoundOver = false;
            ResetCounters();

            if (_gameOverMenu != null) _gameOverMenu.SetActive(false);

            if (firstRun)
            {
                if (_startCanvas != null) _startCanvas.SetActive(false);
                PieceBag.Reset();
                _spawner.ResetBufferedAbility();
                _level.ResetLevel();
                _pieces.StartGame();
            }
            else
            {
                _board.ClearBoard();
                _pieces.DestroyCurPiece();
                _spawner.SpawnPiece();
                _score.ResetScore();
                _level.ResetLevel();
            }

            FindAnyObjectByType<BGMusicToggle>()?.PlayMusic();
            OnProgressChanged?.Invoke();
        }

        /// <summary>Freezes the prepared board while the level intro plays.</summary>
        public void SetIntroHold(bool held)
        {
            RoundOver = held;
            Time.timeScale = held ? 0f : 1f;
        }

        /// <summary>Returns to the main menu.</summary>
        public void ReturnToMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        /// <summary>Called once a placed piece has fully resolved. Returns true when the round
        /// ended, which stops the next piece from spawning.</summary>
        public bool ResolvePlacedPiece()
        {
            if (Current == null) return false;

            OnProgressChanged?.Invoke();
            if (IsObjectiveMet())
            {
                Win();
                return true;
            }
            if (Current.pieceLimit > 0 && PiecesUsed >= Current.pieceLimit)
            {
                _pieces.GameOver();
                return true;
            }
            return false;
        }

        public int Progress
        {
            get
            {
                switch (Current.objective)
                {
                    case "Lines": return _linesCleared;
                    case "Score": return ScoreController.score;
                    case "AbilityCells": return _abilityCells;
                    case "Tetrises": return _tetrises;
                    case "Combo": return _bestCombo;
                    case "Gold": return _board.CountTiles(TileKind.Gold);
                    default: return _board.CountClearableTiles();
                }
            }
        }

        private bool IsObjectiveMet()
        {
            if (Current.objective == "Gold" || Current.objective == "ClearBoard")
                return Progress == 0;
            return Progress >= Current.target;
        }

        private void Win()
        {
            RoundOver = true;
            Time.timeScale = 0f; // frozen until the next level starts or the player leaves
            _pieces.EndRound();
            LevelProgress.MarkCompleted(Current.id);
            OnLevelWon?.Invoke(Current);
        }

        private void ApplyRules(LevelDefinition level)
        {
            GameMode.IsLevels = true;
            GameMode.IsExtended = level.abilityMode == "bar";
            BoardController.LineClearsDisabled = level.noLineClears;
            GhostPieceController.Hidden = level.hideGhost;
            LevelController.FixedSpeedLevel = level.dropLevel;
            LevelController.DisplayLevel = level.id;
            PieceBag.SetAllowedPieces(level.pieces);
            if (_nextPiece != null) _nextPiece.SetActive(!level.hideNext);
        }

        private static void ResetRules()
        {
            GameMode.IsLevels = false;
            BoardController.LineClearsDisabled = false;
            GhostPieceController.Hidden = false;
            LevelController.FixedSpeedLevel = 0;
            LevelController.DisplayLevel = 0;
            PieceBag.SetAllowedPieces(null);
        }

        private void ResetCounters()
        {
            _layoutPending = true;
            PiecesUsed = 0;
            _linesCleared = 0;
            _tetrises = 0;
            _abilityCells = 0;
            _comboStreak = 0;
            _bestCombo = 0;
            _timeLeft = Current.timeLimit;
        }

        private void HandleBoardCleared()
        {
            if (Current == null) return;
            RoundOver = false;
            ResetCounters();
        }

        private void HandlePieceSpawned()
        {
            if (Current == null || RoundOver) return;

            if (_layoutPending)
            {
                _layoutPending = false;
                PlaceLayout();
            }

            PiecesUsed++;
            ChargePieceIfDue();
            if (Current.garbageEvery > 0 && PiecesUsed > 1 && (PiecesUsed - 1) % Current.garbageEvery == 0)
                PushGarbage();
            OnProgressChanged?.Invoke();
        }

        private void PlaceLayout()
        {
            Sprite[] palette = _spawner.TileSprites;
            GameObject template = PieceController.Tiles[0].gameObject;
            if (_solidTile == null)
            {
                Texture2D white = Texture2D.whiteTexture;
                _solidTile = Sprite.Create(white, new Rect(0f, 0f, white.width, white.height),
                    new Vector2(0.5f, 0.5f), SolidTilePixelsPerUnit);
            }

            for (int y = 0; y < Current.rows.Count; y++)
            {
                string row = Current.rows[y];
                for (int x = 0; x < row.Length; x++)
                {
                    char cell = row[x];
                    Vector2Int coords = new Vector2Int(x, y);
                    Sprite normal = palette[(x + y) % palette.Length];
                    if (cell == NormalChar) _board.PlaceTile(coords, template, normal, Color.white, TileKind.Normal);
                    else if (cell == FixedChar) _board.PlaceTile(coords, template, _solidTile, FixedTint, TileKind.Fixed);
                    else if (cell == GoldChar) _board.PlaceTile(coords, template, _solidTile, GoldTint, TileKind.Gold);
                }
            }
        }

        private void ChargePieceIfDue()
        {
            if (Current.abilityEvery <= 0 || PiecesUsed % Current.abilityEvery != 0) return;

            AbilityType ability;
            switch (Current.abilityMode)
            {
                case "bomb": ability = AbilityType.Bomb; break;
                case "laser": ability = AbilityType.Laser; break;
                case "patch": ability = AbilityType.Patch; break;
                default: ability = (AbilityType)Random.Range(0, Enum.GetValues(typeof(AbilityType)).Length); break;
            }
            _spawner.ChargeCurrentPiece(ability, Random.Range(0, PieceController.Tiles.Length));
        }

        private void PushGarbage()
        {
            Sprite[] palette = _spawner.TileSprites;
            GameObject template = PieceController.Tiles[0].gameObject;
            bool room = _board.PushGarbageRow(Random.Range(0, _board.gridSizeX), template, palette[Random.Range(0, palette.Length)]);
            if (!room) _pieces.GameOver();
        }

        private void HandleGameOver()
        {
            if (Current != null) RoundOver = true;
        }

        private void HandleLineCleared(int row)
        {
            if (Current == null) return;
            _linesCleared++;
        }

        private void HandleLinesCleared(int count)
        {
            if (Current == null) return;
            _comboStreak = count > 0 ? _comboStreak + 1 : 0;
            _bestCombo = Mathf.Max(_bestCombo, _comboStreak);
        }

        private void HandleTetris()
        {
            if (Current != null) _tetrises++;
        }

        private void HandleAbilityCells(Vector2Int anchor, int cells)
        {
            if (Current != null) _abilityCells += cells;
        }

        private void HandleScoreAdded(int gained, int total)
        {
            if (Current != null) OnProgressChanged?.Invoke();
        }
    }
}
