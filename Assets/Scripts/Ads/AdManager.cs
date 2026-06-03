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

        /// <summary>
        /// PlayerPrefs flag set by the IAP layer once the player owns the "Remove Ads"
        /// product. Owned here (not in the IAP layer) so the ad layer can decide, on its
        /// own, never to start any ad SDK — keeping the Ads module self-contained.
        /// </summary>
        public const string AdsRemovedPrefKey = "ads_removed";

        [SerializeField] private AdConfig _config;

        private IAdProvider _provider;

        /// <summary>True when the player bought Remove Ads. No ads of any kind are shown.</summary>
        public bool AdsRemoved { get; private set; }

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

            AdsRemoved = PlayerPrefs.GetInt(AdsRemovedPrefKey, 0) == 1;
            if (AdsRemoved)
            {
                // Remove Ads owned — never start any ad SDK. No banner, no interstitial,
                // no rewarded. All Show* calls below become null-safe no-ops.
                Debug.Log("[Ads] Remove Ads owned — ad provider not started.");
                return;
            }

            // The one place a concrete network is chosen.
            _provider = new LevelPlayProvider();
            _provider.OnInterstitialClosed += () => OnInterstitialClosed?.Invoke();
            _provider.OnRewardedClosed += earned => OnRewardedClosed?.Invoke(earned);
            _provider.Initialize(_config);
        }

        /// <summary>
        /// Called by the IAP layer when Remove Ads is purchased or restored. Persists the
        /// flag and immediately tears down any visible ad (banner). Future Show* calls are
        /// gated off. Safe to call mid-session.
        /// </summary>
        public void SetAdsRemoved(bool removed)
        {
            AdsRemoved = removed;
            PlayerPrefs.SetInt(AdsRemovedPrefKey, removed ? 1 : 0);
            PlayerPrefs.Save();

            if (removed)
            {
                _provider?.HideBanner();
                Debug.Log("[Ads] Remove Ads applied — all ads suppressed.");
            }
        }

        public void ShowInterstitial() { if (AdsRemoved) return; _provider?.ShowInterstitial(); }
        public void ShowRewarded() { if (AdsRemoved) return; _provider?.ShowRewarded(); }
        public void ShowBanner() { if (AdsRemoved) return; _provider?.ShowBanner(); }
        public void HideBanner() => _provider?.HideBanner();
    }
}
