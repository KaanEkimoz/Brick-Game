using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ekimoz.IAP
{
    /// <summary>
    /// UI glue for the Remove Ads button. Drop on the button GameObject, wire the Button +
    /// optional label. Buys on click, shows the localized price when available, and hides
    /// the whole button once Remove Ads is owned. Game-specific glue — keeps PurchaseManager
    /// reusable.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class RemoveAdsButton : MonoBehaviour
    {
        private const string RemoveAdsKey = "Remove Ads";

        [Tooltip("Root object to hide once Remove Ads is owned. Defaults to this GameObject.")]
        [SerializeField] private GameObject _rootToHideWhenOwned;

        [Tooltip("Optional label. Shows e.g. 'Remove Ads — ₺24,99' when the price is known.")]
        [SerializeField] private TMP_Text _label;

        [Tooltip("Base label text shown before/around the price.")]
        [SerializeField] private string _labelText = RemoveAdsKey;

        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
            if (_rootToHideWhenOwned == null) _rootToHideWhenOwned = gameObject;
        }

        private void OnEnable()
        {
            _button.onClick.AddListener(OnClick);

            var pm = PurchaseManager.Instance;
            if (pm != null)
            {
                pm.OnRemoveAdsOwnedChanged += HandleOwnedChanged;
                pm.OnReady += Refresh;
                pm.OnPurchaseFailed += HandlePurchaseFailed;
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
                pm.OnPurchaseFailed -= HandlePurchaseFailed;
            }
        }

        private void OnClick()
        {
            PurchaseManager.Instance?.BuyRemoveAds();
        }

        private void HandleOwnedChanged(bool owned) => Refresh();

        private void HandlePurchaseFailed(string reason)
        {
            // Re-enable the button so the player can retry after a cancel/failure.
            if (_button != null) _button.interactable = true;
        }

        private void Refresh()
        {
            var pm = PurchaseManager.Instance;
            bool owned = pm != null && pm.RemoveAdsOwned;

            // Once owned, the button has no purpose — hide it entirely.
            if (_rootToHideWhenOwned != null)
                _rootToHideWhenOwned.SetActive(!owned);

            if (owned) return;

            // Stays greyed out until the store can actually take the purchase.
            _button.interactable = pm != null && pm.IsInitialized;

            if (_label != null)
            {
                string price = pm?.RemoveAdsPrice;
                string text = Localization.Loc.T(RemoveAdsKey);
                _label.text = string.IsNullOrEmpty(price) ? text : $"{text} — {price}";
            }
        }
    }
}
