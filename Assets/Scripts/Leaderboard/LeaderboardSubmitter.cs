using System;
using Board;
using Extras;
using Gameplay;
using InGame;
using UnityEngine;

namespace Leaderboard
{
    /// <summary>Follows the run's score and sends it when the game ends.</summary>
    public class LeaderboardSubmitter : MonoBehaviour
    {
        /// <summary>Fires once the finished run's score has been sent or skipped.</summary>
        public static Action OnRunSettled;

        private BoardController _board;
        private bool _gameEnded;

        private void Awake()
        {
            _board = FindAnyObjectByType<BoardController>(FindObjectsInactive.Include);
        }

        private void Start()
        {
            LeaderboardService.SignInSilently();
        }

        private void OnEnable()
        {
            ScoreController.OnScoreAdded += HandleScoreAdded;
            PiecesController.OnGameOver += HandleGameOver;
        }

        private void OnDisable()
        {
            ScoreController.OnScoreAdded -= HandleScoreAdded;
            PiecesController.OnGameOver -= HandleGameOver;
        }

        private void Update()
        {
            if (!_gameEnded && !PauseTheGame.IsPaused && ScoreController.score > 0)
                RunLedger.Tick(Time.deltaTime);
        }

        private void HandleScoreAdded(int points, int newScore)
        {
            if (GameMode.IsLevels) return;
            _gameEnded = false;
            RunLedger.Record(points, newScore);
        }

        private void HandleGameOver()
        {
            if (GameMode.IsLevels || _board == null) return;
            _gameEnded = true;

            // The ledger only logs now: its checks dropped real Extended records.
            int score = ScoreController.score;
            RunLedger.CanSubmit(score, _board.TotalClearedLines, GameMode.IsExtended);
            if (score > 0)
                LeaderboardService.Submit(LeaderboardService.CurrentBoard, score, () => OnRunSettled?.Invoke());
            else
                OnRunSettled?.Invoke();
        }
    }
}
