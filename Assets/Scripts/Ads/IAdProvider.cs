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

        void Initialize(AdConfig config);

        void LoadInterstitial();
        void ShowInterstitial();

        void LoadRewarded();
        void ShowRewarded();

        void ShowBanner();
        void HideBanner();
    }
}
