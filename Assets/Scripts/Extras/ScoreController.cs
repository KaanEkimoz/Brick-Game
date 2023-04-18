using Board;
using TMPro;
using UnityEngine;

namespace Extras
{
    public class ScoreController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI highScoreText;
        public static int score;
        public static int highScore = 0;
        private void OnEnable()
        {
            BoardController.OnLinesCleared += ClearedLineScore;
        }
        private void OnDisable()
        {
            BoardController.OnLinesCleared -= ClearedLineScore;
        }
        private void Start()
        {
            highScore = 0;
            UpdateHighScoreText();
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
            UpdateHighScoreText();
        }
        public void ResetScore()
        {
            AddScore(-score);
        }
    }
}
