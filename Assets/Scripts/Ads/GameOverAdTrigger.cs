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

        [Tooltip("Minimum real seconds between two interstitial shows. Prevents back-to-back ads when the player keeps dying quickly.")]
        [SerializeField] private float _minSecondsBetweenAds = 90f;

        // Static so the counter + cooldown survive scene reloads and span both Classic and
        // Extended modes — the player can't reset their place in the rotation by switching modes.
        private static int s_gameOverCount;
        private static float s_lastAdRealtime = -9999f;

        private void OnEnable() => PiecesController.OnGameOver += HandleGameOver;
        private void OnDisable() => PiecesController.OnGameOver -= HandleGameOver;

        private void HandleGameOver()
        {
            s_gameOverCount++;
            if (_showEveryNthGameOver <= 0) return;
            if (s_gameOverCount % _showEveryNthGameOver != 0) return;

            // Frequency cap: never show two interstitials within _minSecondsBetweenAds.
            if (Time.realtimeSinceStartup - s_lastAdRealtime < _minSecondsBetweenAds) return;

            if (AdManager.Instance == null) return;
            if (!AdManager.Instance.IsInterstitialReady) return;

            s_lastAdRealtime = Time.realtimeSinceStartup;
            AdManager.Instance.ShowInterstitial();
        }
    }
}
