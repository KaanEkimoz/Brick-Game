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
    /// Runs the daily challenge on top of <see cref="LevelSession"/>: seeds the piece bag with
    /// the day, counts the clears for the share card and records the day's first run. Created at
    /// runtime on every scene load, like the Levels runtime.
    /// </summary>
    public class DailySession : MonoBehaviour
    {
        private const int QuadLines = 4;

        public static DailySession Instance { get; private set; }

        /// <summary>Fires when a daily run ends, by top-out, piece limit or restart.</summary>
        public static Action<DailyResult> OnRunFinished;

        private readonly StringBuilder _clears = new StringBuilder();
        private bool _running;
        private bool _official;
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
            go.AddComponent<DailySession>();
            go.AddComponent<DailyUi>();
        }

        private void Awake()
        {
            Instance = this;
            GameMode.IsDaily = false;
            PieceBag.SetSeed(null);
            DailyStreak.FinishAbandoned();
        }

        private void OnEnable()
        {
            PiecesController.OnGameOver += HandleGameOver;
            BoardController.OnBoardCleared += HandleBoardCleared;
            BoardController.OnLinesCleared += HandleLinesCleared;
            ScoreController.OnScoreAdded += HandleScoreAdded;
        }

        private void OnDisable()
        {
            PiecesController.OnGameOver -= HandleGameOver;
            BoardController.OnBoardCleared -= HandleBoardCleared;
            BoardController.OnLinesCleared -= HandleLinesCleared;
            ScoreController.OnScoreAdded -= HandleScoreAdded;
        }

        // Leaving through the menu reloads the scene; the official run counts as it stands.
        private void OnDestroy()
        {
            if (_running && _official) DailyStreak.Record(BuildResult());
            if (Instance == this) Instance = null;
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused) PlayerPrefs.Save();
        }

        /// <summary>True when today's run would count.</summary>
        public static bool TodayIsOpen => !DailyStreak.HasPlayed(DailyChallenge.Today);

        /// <summary>Starts today's challenge: the first run of the day counts, later ones are practice.</summary>
        public void Begin()
        {
            if (LevelSession.Instance == null) return;

            _running = false;
            _day = DailyChallenge.Today;
            _official = !DailyStreak.HasPlayed(_day);
            GameMode.IsDaily = true;
            PieceBag.SetSeed(DailyChallenge.Seed(_day));
            if (_official) DailyStreak.MarkStarted(_day);

            // Begin clears the board on a retry; that is not a restart to record.
            _starting = true;
            LevelSession.Instance.Begin(DailyChallenge.Definition());
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

        private DailyResult BuildResult()
        {
            return new DailyResult
            {
                Day = _day,
                Score = ScoreController.score,
                Lines = _lines,
                Quads = _quads,
                Clears = _clears.ToString(),
                Official = _official,
            };
        }

        private void Finish(bool announce)
        {
            if (!_running) return;
            _running = false;

            DailyResult result = BuildResult();
            if (_official) result.FreezesUsed = DailyStreak.Record(result);
            _official = false;
            if (announce) OnRunFinished?.Invoke(result);
        }

        private void HandleGameOver()
        {
            Finish(true);
        }

        // The pause menu's Restart clears the board first, while the score is still there.
        // The official run ends with that score and the new one is practice.
        private void HandleBoardCleared()
        {
            if (_starting || !_running) return;
            Finish(false);
            ResetCounters();
            _running = true;
        }

        private void HandleLinesCleared(int count)
        {
            if (!_running || count <= 0) return;
            _lines += count;
            if (count >= QuadLines) _quads++;
            _clears.Append((char)('0' + Mathf.Min(count, QuadLines)));
            SaveLive();
        }

        private void HandleScoreAdded(int gained, int total)
        {
            if (_running) SaveLive();
        }

        private void SaveLive()
        {
            if (_official) DailyStreak.SaveLive(ScoreController.score, _lines, _quads, _clears.ToString());
        }
    }
}
