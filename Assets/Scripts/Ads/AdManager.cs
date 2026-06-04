using System;
using System.Collections;
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

        // Banner load-failure retry with backoff (pumped here because the provider is a plain
        // C# object with no coroutine host). Reset on a successful load.
        private Coroutine _bannerRetry;
        private int _bannerRetryCount;
        private const int MaxBannerRetries = 6;

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
            _provider.OnBannerLoaded += HandleBannerLoaded;
            _provider.OnBannerLoadFailed += HandleBannerLoadFailed;
            _provider.Initialize(_config);
        }

        private void HandleBannerLoaded()
        {
            // Fresh success — clear the backoff so a later refresh failure starts over.
            _bannerRetryCount = 0;
            if (_bannerRetry != null) { StopCoroutine(_bannerRetry); _bannerRetry = null; }
        }

        private void HandleBannerLoadFailed()
        {
            if (AdsRemoved) return;
            if (_bannerRetry != null) StopCoroutine(_bannerRetry);
            _bannerRetry = StartCoroutine(RetryBannerLoad());
        }

        private IEnumerator RetryBannerLoad()
        {
            if (_bannerRetryCount >= MaxBannerRetries) { _bannerRetry = null; yield break; }
            _bannerRetryCount++;
            // Exponential backoff capped at 60s: 4, 8, 16, 32, 60, 60...
            float delay = Mathf.Min(60f, 4f * Mathf.Pow(2f, _bannerRetryCount - 1));
            yield return new WaitForSecondsRealtime(delay);
            _bannerRetry = null;
            if (!AdsRemoved) _provider?.ReloadBanner();
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
                // Stop any pending banner retry and fully suppress the provider so a banner
                // load that completes AFTER this call can never re-show for an owner.
                if (_bannerRetry != null) { StopCoroutine(_bannerRetry); _bannerRetry = null; }
                _provider?.SuppressAds();
                Debug.Log("[Ads] Remove Ads applied — all ads suppressed.");
            }
        }

        public void ShowInterstitial() { if (AdsRemoved) return; _provider?.ShowInterstitial(); }
        public void ShowRewarded() { if (AdsRemoved) return; _provider?.ShowRewarded(); }
        public void ShowBanner() { if (AdsRemoved) return; _provider?.ShowBanner(); }
        public void HideBanner() => _provider?.HideBanner();
    }
}
