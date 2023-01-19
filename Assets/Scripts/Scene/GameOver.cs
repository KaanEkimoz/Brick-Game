using UnityEngine;
using InGame;
using TMPro;
using Extras;

public class GameOver : MonoBehaviour
{
    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private TextMeshProUGUI endScore, highScore;
    [SerializeField] private AudioSource bgMusic;

    //Ads
    InterstitialAds interstitialAd;
    private void Awake()
    {
        interstitialAd = GetComponent<InterstitialAds>();
    }
    private void OnEnable()
    {
        PiecesController.OnGameOver += OpenGameOverScreen;
        PiecesController.OnGameOver += UpdateScores;
        PiecesController.OnGameOver += StopMusic;
        PiecesController.OnGameOver += interstitialAd.ShowAd;
    }
    private void OnDisable()
    {
        PiecesController.OnGameOver -= OpenGameOverScreen;
        PiecesController.OnGameOver -= UpdateScores;
        PiecesController.OnGameOver -= StopMusic;
        PiecesController.OnGameOver -= interstitialAd.ShowAd;
    }
    private void OpenGameOverScreen()
    {
        gameOverScreen.SetActive(true);
    }
    private void UpdateScores()
    {
        endScore.text = "Score\n" + ScoreController.score;
        highScore.text = "High Score\n" + PlayerPrefs.GetInt("highScore");
    }
    private void StopMusic()
    {
        bgMusic.Pause();
    }
}
