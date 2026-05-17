using System;
using Board;
using Gameplay;
using TMPro;
using UnityEngine;

namespace Extras
{
    public class ScoreController : MonoBehaviour
    {
        // Classic high score keeps the original key for backwards compatibility with shipped builds.
        private const string ClassicHighScoreKey = "highScore";
        private const string ExtendedHighScoreKey = "highScore_Extended";

        private static string HighScoreKey => GameMode.IsExtended ? ExtendedHighScoreKey : ClassicHighScoreKey;

        /// <summary>Reads the high score for the currently selected GameMode (Classic vs Extended)
        /// directly from PlayerPrefs. Use this from screens that show the high score outside of
        /// the live ScoreController instance, e.g. the Game Over panel.</summary>
        public static int GetCurrentHighScore() => PlayerPrefs.GetInt(HighScoreKey);

        /// <summary>Fires after a positive score gain lands (delta, new total). Popup effect hooks here.</summary>
        public static Action<int, int> OnScoreAdded;

        public TextMeshProUGUI scoreText;
        public TextMeshProUGUI highScoreText;
        public static int score;
        private int highScore = 0;
        private void OnEnable()
        {
            BoardController.OnLinesCleared += ClearedLineScore;
            GameMode.OnChanged += LoadHighScore;
        }
        private void OnDisable()
        {
            BoardController.OnLinesCleared -= ClearedLineScore;
            GameMode.OnChanged -= LoadHighScore;
        }
        private void Start()
        {
            LoadHighScore();
            score = 0;
            UpdateScoreText();
        }
        private void ClearedLineScore(int consecutiveLineClears)
        {
            switch (consecutiveLineClears)
            {
                case 1:
                    AddScore(40 * (LevelController.CurrentLevel + 1));
                    break;
                case 2:
                    AddScore(100 * (LevelController.CurrentLevel + 1));
                    break;
                case 3:
                    AddScore(300 * (LevelController.CurrentLevel + 1));
                    break;
                case 4:
                    AddScore(1200 * (LevelController.CurrentLevel + 1));
                    break;
            }
        }
        private void AddScore(int point)
        {
            score += point;
            UpdateScoreText();
            if (score > highScore)
                SaveHighScore();
            if (point > 0)
                OnScoreAdded?.Invoke(point, score);
        }
        private void UpdateScoreText()
        {
            scoreText.text = "Score\n" + score;
        }
        private void UpdateHighScoreText()
        {
            highScoreText.text = "High Score\n" + highScore;
        }
        private void SaveHighScore()
        {
            highScore = score;
            PlayerPrefs.SetInt(HighScoreKey, highScore);
            UpdateHighScoreText();
        }
        private void LoadHighScore()
        {
            highScore = PlayerPrefs.GetInt(HighScoreKey);
            UpdateHighScoreText();
        }
        public void ResetScore()
        {
            AddScore(-score);
        }

        /// <summary>Awards score for extended-mode ability clears (scales with current level).</summary>
        public void AddBonusScore(int points)
        {
            AddScore(points);
        }

        /// <summary>Restores the score from a save file and refreshes the UI + high score.</summary>
        public void LoadScore(int value)
        {
            score = value;
            UpdateScoreText();
            if (score > highScore)
                SaveHighScore();
        }
    }
}
