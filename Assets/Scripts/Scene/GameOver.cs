using UnityEngine;
using InGame;
using TMPro;
using Extras;

public class GameOver : MonoBehaviour
{
    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private TextMeshProUGUI endScore, highScore;
    [SerializeField] private AudioSource bgMusic;

    private void OnEnable()
    {
        PiecesController.OnGameOver += OpenGameOverScreen;
        PiecesController.OnGameOver += UpdateScores;
        PiecesController.OnGameOver += StopMusic;
    }
    private void OnDisable()
    {
        PiecesController.OnGameOver -= OpenGameOverScreen;
        PiecesController.OnGameOver -= UpdateScores;
        PiecesController.OnGameOver -= StopMusic;
    }
    private void OpenGameOverScreen()
    {
        gameOverScreen.SetActive(true);
    }
    private void UpdateScores()
    {
        endScore.text = "Score\n" + ScoreController.score;
        // Read the mode-aware high score (Classic / Extended) instead of the legacy key,
        // otherwise the Extended Game Over panel shows the Classic record.
        highScore.text = "High Score\n" + ScoreController.GetCurrentHighScore();
    }
    private void StopMusic()
    {
        bgMusic.Pause();
    }
}
