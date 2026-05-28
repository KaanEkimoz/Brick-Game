using System;
using UnityEngine;

namespace Ekimoz.Ads
{
    /// <summary>
    /// Single entry point the game talks to for ads. Owns the active <see cref="IAdProvider"/>
    /// and survives scene loads. Swapping ad networks is a one-line change here
    /// (<c>new LevelPlayProvider()</c> → <c>new MaxProvider()</c>); no game code changes.
    /// Reusable across Ekimoz titles — drop in this folder + an AdConfig asset per game.
    /// </summary>
    public class AdManager : MonoBehaviour
    {
        public static AdManager Instance { get; private set; }

        [SerializeField] private AdConfig _config;

        private IAdProvider _provider;

        public bool IsInitialized => _provider != null && _provider.IsInitialized;
        public bool IsInterstitialReady => _provider != null && _provider.IsInterstitialReady;
        public bool IsRewardedReady => _provider != null && _provider.IsRewardedReady;

        /// <summary>Interstitial dismissed (or skipped because none was ready).</summary>
        public event Action OnInterstitialClosed;

        /// <summary>Rewarded finished. bool = reward earned.</summary>
        public event Action<bool> OnRewardedClosed;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // The one place a concrete network is chosen.
            _provider = new LevelPlayProvider();
            _provider.OnInterstitialClosed += () => OnInterstitialClosed?.Invoke();
            _provider.OnRewardedClosed += earned => OnRewardedClosed?.Invoke(earned);
            _provider.Initialize(_config);
        }

        public void ShowInterstitial() => _provider?.ShowInterstitial();
        public void ShowRewarded() => _provider?.ShowRewarded();
        public void ShowBanner() => _provider?.ShowBanner();
        public void HideBanner() => _provider?.HideBanner();
    }
}
