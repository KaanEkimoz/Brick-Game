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

        /// <summary>Fires exactly once per run, the moment the player's live score
        /// crosses their previously saved high score. Subsequent score gains in the
        /// same run do not re-fire — _highScoreBeatenFired guards the latch. Used by
        /// the "NEW BEST!" center banner to celebrate the moment of overtaking the
        /// record without spamming on every subsequent line clear.</summary>
        public static Action<int> OnHighScoreBeaten;

        public TextMeshProUGUI scoreText;
        public TextMeshProUGUI highScoreText;
        public static int score;
        private int highScore = 0;
        // Per-run latch: cleared on ResetScore and on a fresh Start. Without this,
        // every score gain after the beat would re-fire the banner.
        private bool _highScoreBeatenFired = false;
        // Snapshot of the high score AT THE START of the run. Beat detection compares
        // the live score against this constant, not the live `highScore` (which itself
        // grows whenever score > highScore and would otherwise make the threshold
        // race the player and never let them "beat" it cleanly).
        private int _runStartHighScore = 0;
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
            _runStartHighScore = highScore;
            _highScoreBeatenFired = _runStartHighScore == 0;
            // If the player has never set a high score, every run is technically a
            // "new best" from the first line clear — skip the banner in that case
            // (the latch is pre-fired). The banner exists to mark crossing an EXISTING
            // bar, not the first run of a fresh install.
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
            // Latch crossing the run-start record — once per run only. Negative score
            // deltas (ResetScore subtracts) cannot cross the threshold.
            if (point > 0
                && !_highScoreBeatenFired
                && _runStartHighScore > 0
                && score > _runStartHighScore)
            {
                _highScoreBeatenFired = true;
                OnHighScoreBeaten?.Invoke(score);
            }
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
            // Restart path: re-arm the per-run latch and re-snapshot the current
            // record so the banner can fire again on the next run.
            _runStartHighScore = PlayerPrefs.GetInt(HighScoreKey);
            _highScoreBeatenFired = _runStartHighScore == 0;
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
