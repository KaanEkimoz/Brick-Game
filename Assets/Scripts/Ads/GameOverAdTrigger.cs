using InGame;
using UnityEngine;

namespace Ekimoz.Ads
{
    /// <summary>
    /// Brick Game-specific glue between gameplay and the generic <see cref="AdManager"/>.
    /// Shows an interstitial on game over, but only every Nth time so the retry loop
    /// stays fast and non-annoying. Keeping this game-specific logic out of AdManager
    /// is what lets AdManager/IAdProvider stay reusable across titles.
    /// </summary>
    public class GameOverAdTrigger : MonoBehaviour
    {
        [Tooltip("Show an interstitial once every N game-overs (1 = every time).")]
        [SerializeField] private int _showEveryNthGameOver = 2;

        private int _gameOverCount;

        private void OnEnable() => PiecesController.OnGameOver += HandleGameOver;
        private void OnDisable() => PiecesController.OnGameOver -= HandleGameOver;

        private void HandleGameOver()
        {
            _gameOverCount++;
            if (_showEveryNthGameOver <= 0) return;
            if (_gameOverCount % _showEveryNthGameOver != 0) return;

            if (AdManager.Instance != null)
                AdManager.Instance.ShowInterstitial();
        }
    }
}
