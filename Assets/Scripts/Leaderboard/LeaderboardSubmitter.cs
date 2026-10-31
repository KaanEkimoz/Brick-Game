using System;
using Board;
using Extras;
using Gameplay;
using InGame;
using UnityEngine;

namespace Leaderboard
{
    /// <summary>Follows the run's score and sends it when the game ends. Only the score earned
    /// live goes out; the saved high score can be edited on the device and is never sent.</summary>
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

            int score = ScoreController.score;
            if (RunLedger.CanSubmit(score, _board.TotalClearedLines, GameMode.IsExtended))
                LeaderboardService.Submit(LeaderboardService.CurrentBoard, score, () => OnRunSettled?.Invoke());
            else
                OnRunSettled?.Invoke();
        }
    }
}
