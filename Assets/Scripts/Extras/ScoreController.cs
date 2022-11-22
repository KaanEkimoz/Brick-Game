using Board;
using TMPro;
using UnityEngine;

namespace Extras
{
    public class ScoreController : MonoBehaviour
    {
        public TextMeshProUGUI scoreText;
        private int score;
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
        }
        private void UpdateScoreText()
        {
            scoreText.text = "Score " + score;
        }
    }
}
