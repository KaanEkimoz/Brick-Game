using System;
using System.Collections;
using Ekimoz.Ads;
using UnityEngine;

namespace Ekimoz.IAP
{
    /// <summary>
    /// Single entry point the game talks to for in-app purchases. Owns the active
    /// <see cref="IPurchaseService"/> and survives scene loads — mirrors <c>AdManager</c>.
    /// Currently handles one product: Remove Ads (non-consumable). When owned (purchased
    /// or auto-restored at launch), it tells <see cref="AdManager"/> to suppress every ad.
    /// </summary>
    public class PurchaseManager : MonoBehaviour
    {
        public static PurchaseManager Instance { get; private set; }

        /// <summary>Google Play / App Store product id. Must match the Play Console product.</summary>
        public const string RemoveAdsProductId = "remove_ads";

        [Tooltip("Verbose [IAP] logging. Turn off for release if noisy.")]
        [SerializeField] private bool _verboseLogging = true;

        private const float MaxInitRetryDelay = 120f;

        private IPurchaseService _service;
        private int _initRetryCount;

        /// <summary>True once Remove Ads is owned (purchased or restored).</summary>
        public bool RemoveAdsOwned { get; private set; }

        /// <summary>True once the billing SDK has finished initializing.</summary>
        public bool IsInitialized => _service != null && _service.IsInitialized;

        /// <summary>Fired whenever ownership state changes (so the Remove Ads button can refresh).</summary>
        public event Action<bool> OnRemoveAdsOwnedChanged;

        /// <summary>Fired when a purchase attempt fails or is cancelled. string = reason.</summary>
        public event Action<string> OnPurchaseFailed;

        /// <summary>Fired when the store finishes init (prices are available now).</summary>
        public event Action OnReady;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Optimistic initial state from the persisted flag, so the UI shows "owned"
            // instantly on relaunch without waiting for the store round-trip.
            RemoveAdsOwned = PlayerPrefs.GetInt(AdManager.AdsRemovedPrefKey, 0) == 1;

            _service = new UnityIapService(_verboseLogging);
            _service.OnInitialized += HandleInitialized;
            _service.OnInitializeFailed += HandleInitializeFailed;
            _service.OnPurchaseSucceeded += HandlePurchaseSucceeded;
            _service.OnPurchaseFailed += HandlePurchaseFailed;

            _service.Initialize(new[] { RemoveAdsProductId });
        }

        // ---------------- Public API (UI talks to these) ----------------

        /// <summary>Start the Remove Ads purchase flow.</summary>
        public void BuyRemoveAds()
        {
            if (RemoveAdsOwned)
            {
                // Already owned — re-apply suppression just in case and refresh UI.
                ApplyRemoveAds();
                return;
            }
            _service?.Purchase(RemoveAdsProductId);
        }

        /// <summary>Restore previously bought purchases (Google Play auto-restores at init).</summary>
        public void RestorePurchases() => _service?.Restore();

        /// <summary>Localized price for the Remove Ads button (e.g. "₺24,99"), or null if not ready.</summary>
        public string RemoveAdsPrice => _service?.GetLocalizedPrice(RemoveAdsProductId);

        // ---------------- Service callbacks ----------------

        private void HandleInitialized()
        {
            // Store may report the product already owned (restore). Sync just in case.
            bool owned = _service.IsOwned(RemoveAdsProductId);
            if (owned && !RemoveAdsOwned)
                ApplyRemoveAds();
            // Only Google Play reports a refund here. iOS keeps its receipt until a restore.
            else if (!owned && RemoveAdsOwned && Application.platform == RuntimePlatform.Android)
                RevokeRemoveAds();

            OnReady?.Invoke();
        }

        private void HandleInitializeFailed(string reason)
        {
            Debug.LogWarning($"[IAP] Billing init failed: {reason}. Retrying.");
            StartCoroutine(RetryInitialize());
        }

        private void HandlePurchaseSucceeded(string productId)
        {
            if (productId == RemoveAdsProductId)
                ApplyRemoveAds();
        }

        private void HandlePurchaseFailed(string productId, string reason)
        {
            if (productId == RemoveAdsProductId)
                OnPurchaseFailed?.Invoke(reason);
        }

        // 5, 10, 20, 40, 80, 120, 120...
        private IEnumerator RetryInitialize()
        {
            float delay = Mathf.Min(MaxInitRetryDelay, 5f * Mathf.Pow(2f, _initRetryCount));
            _initRetryCount++;
            yield return new WaitForSecondsRealtime(delay);
            _service.Initialize(new[] { RemoveAdsProductId });
        }

        // ---------------- Apply ----------------

        private void RevokeRemoveAds()
        {
            RemoveAdsOwned = false;
            PlayerPrefs.SetInt(AdManager.AdsRemovedPrefKey, 0);
            PlayerPrefs.Save();
            AdManager.Instance?.SetAdsRemoved(false);
            OnRemoveAdsOwnedChanged?.Invoke(false);
        }

        private void ApplyRemoveAds()
        {
            bool wasOwned = RemoveAdsOwned;
            RemoveAdsOwned = true;

            // Source of truth for "ads off" lives in AdManager's pref; this persists it too.
            AdManager.Instance?.SetAdsRemoved(true);

            // If AdManager isn't in this scene yet (shouldn't happen, but defensive),
            // still persist so the next AdManager.Awake reads it.
            PlayerPrefs.SetInt(AdManager.AdsRemovedPrefKey, 1);
            PlayerPrefs.Save();

            if (!wasOwned)
                OnRemoveAdsOwnedChanged?.Invoke(true);
        }
    }
}
