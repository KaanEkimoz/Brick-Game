using UnityEngine;

namespace Ekimoz.Ads
{
    /// <summary>
    /// Per-game ad configuration. One asset per project; keys come from the LevelPlay
    /// dashboard. Keeping these in a ScriptableObject (not hard-coded) is what makes the
    /// ad layer drop-in reusable across Ekimoz titles — each game ships its own AdConfig
    /// asset, the code stays identical.
    /// </summary>
    [CreateAssetMenu(fileName = "AdConfig", menuName = "Ekimoz/Ads/Ad Config")]
    public class AdConfig : ScriptableObject
    {
        [Header("App Key — LevelPlay dashboard > App settings")]
        [SerializeField] private string _androidAppKey;
        [SerializeField] private string _iosAppKey;

        [Header("Ad Unit IDs — LevelPlay dashboard > Ad units")]
        [SerializeField] private string _interstitialAdUnitId;
        [SerializeField] private string _rewardedAdUnitId;
        [SerializeField] private string _bannerAdUnitId;

        [Header("Behaviour")]
        [Tooltip("Test mode serves LevelPlay test ads — keep ON until live keys are verified.")]
        [SerializeField] private bool _testMode = true;
        [SerializeField] private bool _verboseLogging = true;

        public string InterstitialAdUnitId => _interstitialAdUnitId;
        public string RewardedAdUnitId => _rewardedAdUnitId;
        public string BannerAdUnitId => _bannerAdUnitId;
        public bool TestMode => _testMode;
        public bool VerboseLogging => _verboseLogging;

        /// <summary>App key for the current runtime platform.</summary>
        public string ActiveAppKey =>
            Application.platform == RuntimePlatform.IPhonePlayer ? _iosAppKey : _androidAppKey;

        public bool HasValidKey => !string.IsNullOrWhiteSpace(ActiveAppKey);
    }
}
