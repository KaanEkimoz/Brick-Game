using System;
using System.Text;
using Board;
using Extras;
using Gameplay;
using InGame;
using Levels;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Daily
{
    /// <summary>
    /// Runs the day's challenge on top of <see cref="LevelSession"/>: a level from the daily
    /// pool, with the piece order seeded by the day. A win completes the day and moves the
    /// streak; a loss can be retried. Created at runtime on every scene load, like the Levels
    /// runtime.
    /// </summary>
    public class DailySession : MonoBehaviour
    {
        private const int QuadLines = 4;

        public static DailySession Instance { get; private set; }

        /// <summary>Fires when a daily run ends, won or lost.</summary>
        public static Action<DailyResult> OnRunFinished;

        private readonly StringBuilder _clears = new StringBuilder();
        private bool _running;
        private bool _starting;
        private int _day;
        private int _lines;
        private int _quads;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            SceneManager.sceneLoaded += (scene, mode) => CreateRuntime();
            CreateRuntime();
        }

        private static void CreateRuntime()
        {
            if (Instance != null) return;
            GameObject go = new GameObject("DailyRuntime");
            go.AddComponent<AudioSource>();
            go.AddComponent<DailySession>();
            go.AddComponent<DailyUi>();
        }

        private void Awake()
        {
            Instance = this;
            GameMode.IsDaily = false;
            PieceBag.SetSeed(null);
        }

        private void OnEnable()
        {
            PiecesController.OnGameOver += HandleGameOver;
            LevelSession.OnLevelWon += HandleLevelWon;
            BoardController.OnBoardCleared += HandleBoardCleared;
            BoardController.OnLinesCleared += HandleLinesCleared;
        }

        private void OnDisable()
        {
            PiecesController.OnGameOver -= HandleGameOver;
            LevelSession.OnLevelWon -= HandleLevelWon;
            BoardController.OnBoardCleared -= HandleBoardCleared;
            BoardController.OnLinesCleared -= HandleLinesCleared;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        /// <summary>True until today's challenge is won.</summary>
        public static bool TodayIsOpen => !DailyStreak.HasPlayed(DailyChallenge.Today);

        /// <summary>Today's challenge, or null when the pool is missing.</summary>
        public static LevelDefinition TodayChallenge => DailyChallenge.ForDay(DailyChallenge.Today);

        /// <summary>Starts or retries today's challenge, until it is won.</summary>
        public void Begin()
        {
            LevelDefinition challenge = TodayChallenge;
            if (LevelSession.Instance == null || challenge == null || !TodayIsOpen) return;

            _running = false;
            _day = DailyChallenge.Today;
            GameMode.IsDaily = true;
            PieceBag.SetSeed(DailyChallenge.Seed(_day));

            // Begin clears the board on a retry; that is not a restart to count.
            _starting = true;
            LevelSession.Instance.Begin(challenge);
            _starting = false;

            ResetCounters();
            _running = true;
        }

        private void ResetCounters()
        {
            _lines = 0;
            _quads = 0;
            _clears.Length = 0;
        }

        private DailyResult BuildResult(bool won)
        {
            return new DailyResult
            {
                Day = _day,
                Score = ScoreController.score,
                Lines = _lines,
                Quads = _quads,
                Pieces = LevelSession.Instance != null ? LevelSession.Instance.PiecesUsed : 0,
                Clears = _clears.ToString(),
                Won = won,
            };
        }

        private void Finish(bool won)
        {
            if (!_running) return;
            _running = false;

            DailyResult result = BuildResult(won);
            if (won) result.FreezesUsed = DailyStreak.Record(result);
            OnRunFinished?.Invoke(result);
        }

        private void HandleLevelWon(LevelDefinition level)
        {
            Finish(true);
        }

        private void HandleGameOver()
        {
            Finish(false);
        }

        // The pause menu's Restart starts the challenge over; the counters follow.
        private void HandleBoardCleared()
        {
            if (!_starting && _running) ResetCounters();
        }

        private void HandleLinesCleared(int count)
        {
            if (!_running || count <= 0) return;
            _lines += count;
            if (count >= QuadLines) _quads++;
            _clears.Append((char)('0' + Mathf.Min(count, QuadLines)));
        }
    }
}
