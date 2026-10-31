using System;

namespace Ekimoz.Ads
{
    /// <summary>
    /// SDK-agnostic ad surface. The game only ever talks to this interface; the
    /// concrete provider (LevelPlay today, AppLovin MAX / AdMob tomorrow) hides the
    /// vendor SDK. Swapping networks means swapping the provider, not touching game code.
    /// Designed to live in a shared package so every Ekimoz title reuses it.
    /// </summary>
    public interface IAdProvider
    {
        bool IsInitialized { get; }
        bool IsInterstitialReady { get; }
        bool IsRewardedReady { get; }

        /// <summary>Fired once the underlying SDK finishes initialization.</summary>
        event Action OnInitialized;

        /// <summary>Fired when an interstitial is dismissed (closed or failed to show).</summary>
        event Action OnInterstitialClosed;

        /// <summary>Fired when a rewarded ad finishes. bool = reward actually earned.</summary>
        event Action<bool> OnRewardedClosed;

        /// <summary>Fired when the banner finishes loading successfully.</summary>
        event Action OnBannerLoaded;

        /// <summary>Fired when a banner load attempt fails, so the host can schedule a retry.</summary>
        event Action OnBannerLoadFailed;

        /// <summary>Fired when the SDK fails to start, so the host can schedule a retry.</summary>
        event Action OnInitializeFailed;

        /// <summary>Fired when an interstitial load fails, so the host can schedule a retry.</summary>
        event Action OnInterstitialLoadFailed;

        void Initialize(AdConfig config);

        /// <summary>Re-attempt SDK startup after a failure.</summary>
        void RetryInitialize();

        /// <summary>Passes the player's personalized-ads answer to the SDK. Call before Initialize.</summary>
        void SetConsent(bool consent);

        void LoadInterstitial();
        void ShowInterstitial();

        void LoadRewarded();
        void ShowRewarded();

        void ShowBanner();
        void HideBanner();

        /// <summary>Re-attempt loading the banner after a previous failure.</summary>
        void ReloadBanner();

        /// <summary>
        /// Permanently suppress every ad for the rest of this session (Remove Ads owned).
        /// Tears down the banner and blocks any future auto-show even if SDK init/load
        /// completes later — closing the purchase-vs-init race.
        /// </summary>
        void SuppressAds();
    }
}
