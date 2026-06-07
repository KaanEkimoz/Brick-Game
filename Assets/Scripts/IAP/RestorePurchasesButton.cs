using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ekimoz.IAP
{
    /// <summary>
    /// UI glue for the "Restore Purchases" button. Required by App Store Review Guideline
    /// 3.1.1 — every iOS app that sells non-consumable IAP must expose a way to re-grant
    /// previously-bought entitlements (e.g. after reinstall, switching device, signing into
    /// a different Apple ID). Drop on the button GameObject, wire the Button + optional
    /// label + optional status text. Calls PurchaseManager.RestorePurchases() on click.
    ///
    /// Visibility: by default the button hides itself when Remove Ads is already owned
    /// (nothing to restore). Set _alwaysVisible = true if you want it shown unconditionally
    /// (some apps prefer this so the player can always find it).
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class RestorePurchasesButton : MonoBehaviour
    {
        [Tooltip("Root GameObject to hide once Remove Ads is owned (nothing to restore). " +
                 "Defaults to this GameObject. Ignored when _alwaysVisible is true.")]
        [SerializeField] private GameObject _rootToHideWhenOwned;

        [Tooltip("Keep the button visible even after Remove Ads is owned. Some stores prefer " +
                 "this so the player can always find Restore. Default off — hides when owned.")]
        [SerializeField] private bool _alwaysVisible;

        [Tooltip("Optional label updated with localized button text (e.g. 'Restore Purchases'). " +
                 "Leave empty if your button uses a hard-coded label.")]
        [SerializeField] private TMP_Text _label;

        [Tooltip("Optional status line that briefly shows feedback after a restore tap " +
                 "(\"Restoring...\" → \"Restored\" / \"Nothing to restore\"). Leave empty to skip.")]
        [SerializeField] private TMP_Text _statusLabel;

        [Tooltip("Hide the whole button on platforms where the store auto-restores at init " +
                 "(Google Play). Default on. Apple App Store always shows it.")]
        [SerializeField] private bool _hideOnAndroid = true;

        private Button _button;
        private float _statusClearTime;

        private void Awake()
        {
            _button = GetComponent<Button>();
            if (_rootToHideWhenOwned == null) _rootToHideWhenOwned = gameObject;
        }

        private void OnEnable()
        {
            // Android auto-restores at PurchaseManager.Init via Unity IAP; the button is mostly
            // dead weight there. iOS REQUIRES it (App Store Review 3.1.1).
#if UNITY_ANDROID && !UNITY_EDITOR
            if (_hideOnAndroid)
            {
                if (_rootToHideWhenOwned != null) _rootToHideWhenOwned.SetActive(false);
                return;
            }
#endif

            _button.onClick.AddListener(OnClick);

            var pm = PurchaseManager.Instance;
            if (pm != null)
            {
                pm.OnRemoveAdsOwnedChanged += HandleOwnedChanged;
                pm.OnReady += Refresh;
            }
            Refresh();
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(OnClick);
            var pm = PurchaseManager.Instance;
            if (pm != null)
            {
                pm.OnRemoveAdsOwnedChanged -= HandleOwnedChanged;
                pm.OnReady -= Refresh;
            }
        }

        private void Update()
        {
            if (_statusLabel != null && _statusClearTime > 0f && Time.unscaledTime >= _statusClearTime)
            {
                _statusLabel.text = string.Empty;
                _statusClearTime = 0f;
            }
        }

        private void OnClick()
        {
            var pm = PurchaseManager.Instance;
            if (pm == null) return;

            // The actual restore is a fire-and-forget — Unity IAP re-broadcasts ProcessPurchase
            // for owned entitlements, which PurchaseManager already routes into ApplyRemoveAds.
            // We just show a brief status line so the player knows something is happening.
            pm.RestorePurchases();
            ShowStatus(pm.RemoveAdsOwned ? "Already owned." : "Restoring…", 2.5f);
        }

        private void HandleOwnedChanged(bool owned)
        {
            Refresh();
            if (owned) ShowStatus("Purchases restored.", 2.5f);
        }

        private void Refresh()
        {
            var pm = PurchaseManager.Instance;
            bool owned = pm != null && pm.RemoveAdsOwned;
            if (_rootToHideWhenOwned != null && !_alwaysVisible)
                _rootToHideWhenOwned.SetActive(!owned);
            _button.interactable = pm != null && pm.IsInitialized && !owned;
        }

        private void ShowStatus(string msg, float seconds)
        {
            if (_statusLabel == null) return;
            _statusLabel.text = msg;
            _statusClearTime = Time.unscaledTime + seconds;
        }
    }
}
