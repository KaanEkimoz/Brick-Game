using System;
using Unity.Services.LevelPlay;
using UnityEngine;

namespace Ekimoz.Ads
{
    /// <summary>
    /// <see cref="IAdProvider"/> backed by Unity LevelPlay (Ads Mediation) SDK 9.x.
    /// All vendor-specific types stay inside this file; swapping to AppLovin MAX later
    /// means writing a sibling provider and changing one line in <see cref="AdManager"/>.
    /// </summary>
    public class LevelPlayProvider : IAdProvider
    {
        private AdConfig _config;
        private LevelPlayInterstitialAd _interstitial;
        private LevelPlayRewardedAd _rewarded;
        private LevelPlayBannerAd _banner;
        private bool _bannerLoaded;
        private bool _bannerVisible;
        private bool _rewardEarnedThisShow;

        // Set once Remove Ads is owned. Blocks every auto-show path inside the provider,
        // even if SDK init / banner load completes AFTER suppression was requested.
        private bool _suppressed;

        public bool IsInitialized { get; private set; }
        public bool IsInterstitialReady => _interstitial != null && _interstitial.IsAdReady();
        public bool IsRewardedReady => _rewarded != null && _rewarded.IsAdReady();

        public event Action OnInitialized;
        public event Action OnInterstitialClosed;
        public event Action<bool> OnRewardedClosed;
        public event Action OnBannerLoaded;
        public event Action OnBannerLoadFailed;

        public void Initialize(AdConfig config)
        {
            _config = config;
            if (config == null || !config.HasValidKey)
            {
                Debug.LogWarning("[Ads] AdConfig missing or app key empty — LevelPlay not initialized.");
                return;
            }

            if (config.VerboseLogging)
                LevelPlay.SetAdaptersDebug(true);

            LevelPlay.OnInitSuccess += HandleInitSuccess;
            LevelPlay.OnInitFailed += HandleInitFailed;
            LevelPlay.Init(config.ActiveAppKey);
        }

        private void HandleInitSuccess(LevelPlayConfiguration configuration)
        {
            IsInitialized = true;
            if (_config.VerboseLogging) Debug.Log("[Ads] LevelPlay init success.");

            // Remove Ads may have been applied (restore) while the SDK was still initializing.
            // If so, never create or load any ad unit — this is the fix for the banner showing
            // for owners on a fresh install where the local pref hadn't been set yet at Awake.
            if (_suppressed)
            {
                OnInitialized?.Invoke();
                return;
            }

            SetupInterstitial();
            SetupRewarded();
            SetupBanner();
            OnInitialized?.Invoke();
        }

        private void HandleInitFailed(LevelPlayInitError error)
        {
            Debug.LogWarning($"[Ads] LevelPlay init failed: {error}");
        }

        // ---------------- Interstitial ----------------

        private void SetupInterstitial()
        {
            if (string.IsNullOrWhiteSpace(_config.InterstitialAdUnitId)) return;

            _interstitial = new LevelPlayInterstitialAd(_config.InterstitialAdUnitId);
            _interstitial.OnAdClosed += _ => { OnInterstitialClosed?.Invoke(); _interstitial.LoadAd(); };
            _interstitial.OnAdDisplayFailed += (_, __) => { OnInterstitialClosed?.Invoke(); _interstitial.LoadAd(); };
            _interstitial.LoadAd();
        }

        public void LoadInterstitial()
        {
            if (_interstitial != null && !_interstitial.IsAdReady())
                _interstitial.LoadAd();
        }

        public void ShowInterstitial()
        {
            if (IsInterstitialReady)
                _interstitial.ShowAd();
            else
                OnInterstitialClosed?.Invoke(); // never block the game flow if no ad is ready
        }

        // ---------------- Rewarded ----------------

        private void SetupRewarded()
        {
            if (string.IsNullOrWhiteSpace(_config.RewardedAdUnitId)) return;

            _rewarded = new LevelPlayRewardedAd(_config.RewardedAdUnitId);
            _rewarded.OnAdRewarded += (_, __) => _rewardEarnedThisShow = true;
            _rewarded.OnAdClosed += _ =>
            {
                OnRewardedClosed?.Invoke(_rewardEarnedThisShow);
                _rewardEarnedThisShow = false;
                _rewarded.LoadAd();
            };
            _rewarded.OnAdDisplayFailed += (_, __) =>
            {
                OnRewardedClosed?.Invoke(false);
                _rewardEarnedThisShow = false;
                _rewarded.LoadAd();
            };
            _rewarded.LoadAd();
        }

        public void LoadRewarded()
        {
            if (_rewarded != null && !_rewarded.IsAdReady())
                _rewarded.LoadAd();
        }

        public void ShowRewarded()
        {
            if (IsRewardedReady)
            {
                _rewardEarnedThisShow = false;
                _rewarded.ShowAd();
            }
            else
            {
                OnRewardedClosed?.Invoke(false);
            }
        }

        // ---------------- Banner ----------------

        private void SetupBanner()
        {
            if (_suppressed) return;
            if (string.IsNullOrWhiteSpace(_config.BannerAdUnitId)) return;
            _banner = new LevelPlayBannerAd(_config.BannerAdUnitId);
            _banner.OnAdLoaded += info =>
            {
                _bannerLoaded = true;
                if (_config.VerboseLogging) Debug.Log("[Ads] Banner loaded.");
                OnBannerLoaded?.Invoke();
                // Show right away if the game asked for it before the load finished —
                // unless Remove Ads was applied in the meantime.
                if (_bannerVisible && !_suppressed) _banner.ShowAd();
            };
            _banner.OnAdLoadFailed += err =>
            {
                _bannerLoaded = false;
                if (_config.VerboseLogging) Debug.LogWarning($"[Ads] Banner load failed: {err}");
                // LevelPlay does NOT auto-retry a failed initial load (only auto-refreshes
                // AFTER a successful one). Hand the retry to the host so a single startup
                // miss doesn't leave the banner absent for the whole session.
                if (!_suppressed) OnBannerLoadFailed?.Invoke();
            };
            _banner.OnAdClicked += _ => { if (_config.VerboseLogging) Debug.Log("[Ads] Banner clicked."); };
            _banner.LoadAd();
            // Auto-show as soon as it loads — banners stay visible during gameplay by default.
            _bannerVisible = true;
        }

        public void ShowBanner()
        {
            if (_suppressed) return;
            _bannerVisible = true;
            if (_banner == null)
            {
                // SDK may not have initialized yet; SetupBanner on init will read _bannerVisible.
                return;
            }
            if (_bannerLoaded) _banner.ShowAd();
        }

        public void HideBanner()
        {
            _bannerVisible = false;
            if (_banner != null) _banner.HideAd();
        }

        public void ReloadBanner()
        {
            if (_suppressed) return;
            if (_banner == null)
            {
                // Init may have failed to reach SetupBanner; build it now.
                SetupBanner();
                return;
            }
            _bannerVisible = true;
            _banner.LoadAd();
        }

        public void SuppressAds()
        {
            _suppressed = true;
            _bannerVisible = false;
            _bannerLoaded = false;
            if (_banner != null)
            {
                _banner.HideAd();
                _banner = null;
            }
        }
    }
}
