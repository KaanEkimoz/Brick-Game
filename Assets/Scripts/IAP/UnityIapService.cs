using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

// The classic IStoreListener path is marked [Obsolete] in IAP 5.x ("upgrade to v5"),
// but it is fully functional, battle-tested, and far simpler than the new builder API.
// We intentionally use it; silence the upgrade nag so the project compiles clean.
#pragma warning disable CS0618

namespace Ekimoz.IAP
{
    /// <summary>
    /// <see cref="IPurchaseService"/> backed by Unity IAP (UnityEngine.Purchasing) 5.x.
    /// All vendor types stay inside this file — the rest of the game only sees
    /// <see cref="IPurchaseService"/>. On Google Play, owned non-consumables are restored
    /// automatically at init (ProcessPurchase fires for them), so a reinstall keeps Remove Ads.
    /// </summary>
    public class UnityIapService : IDetailedStoreListener, IPurchaseService
    {
        private IStoreController _controller;
        private IExtensionProvider _extensions;
        private HashSet<string> _productIds = new HashSet<string>();
        private bool _verbose;

        public bool IsInitialized => _controller != null;

        public event Action OnInitialized;
        public event Action<string> OnInitializeFailed;
        public event Action<string> OnPurchaseSucceeded;
        public event Action<string, string> OnPurchaseFailed;

        public UnityIapService(bool verboseLogging = true)
        {
            _verbose = verboseLogging;
        }

        public void Initialize(IEnumerable<string> nonConsumableProductIds)
        {
            if (_controller != null) return; // already initialized

            var module = StandardPurchasingModule.Instance();
            var builder = ConfigurationBuilder.Instance(module);

            _productIds.Clear();
            foreach (var id in nonConsumableProductIds)
            {
                if (string.IsNullOrWhiteSpace(id)) continue;
                _productIds.Add(id);
                builder.AddProduct(id, ProductType.NonConsumable);
            }

            if (_productIds.Count == 0)
            {
                Log("No product ids supplied — IAP not initialized.", true);
                OnInitializeFailed?.Invoke("no_products");
                return;
            }

            UnityPurchasing.Initialize(this, builder);
        }

        public void Purchase(string productId)
        {
            if (_controller == null)
            {
                Log($"Purchase('{productId}') before init — ignored.", true);
                OnPurchaseFailed?.Invoke(productId, "not_initialized");
                return;
            }

            var product = _controller.products.WithID(productId);
            if (product == null || !product.availableToPurchase)
            {
                Log($"Product '{productId}' not available to purchase.", true);
                OnPurchaseFailed?.Invoke(productId, "unavailable");
                return;
            }

            Log($"Initiating purchase: {productId}");
            _controller.InitiatePurchase(product);
        }

        public void Restore()
        {
            if (_controller == null)
            {
                Log("Restore before init — ignored.", true);
                return;
            }

            // Android (Google Play): owned non-consumables are auto-restored at init via
            // ProcessPurchase, so there is nothing extra to do here. Apple requires an
            // explicit restore call routed through the StoreKit extension.
            if (Application.platform == RuntimePlatform.IPhonePlayer ||
                Application.platform == RuntimePlatform.OSXPlayer)
            {
                var apple = _extensions?.GetExtension<IAppleExtensions>();
                if (apple != null)
                {
                    apple.RestoreTransactions((success, error) =>
                        Log($"Apple restore finished. success={success} error={error}", !success));
                    return;
                }
            }

            // On Google Play, re-emit any already-owned products so the UI re-syncs.
            foreach (var id in _productIds)
            {
                var p = _controller.products.WithID(id);
                if (p != null && p.hasReceipt)
                    OnPurchaseSucceeded?.Invoke(id);
            }
        }

        public bool IsOwned(string productId)
        {
            if (_controller == null) return false;
            var p = _controller.products.WithID(productId);
            return p != null && p.hasReceipt;
        }

        public string GetLocalizedPrice(string productId)
        {
            if (_controller == null) return null;
            var p = _controller.products.WithID(productId);
            return p?.metadata?.localizedPriceString;
        }

        // ---------------- IStoreListener / IDetailedStoreListener ----------------
        // Implemented explicitly so the Unity IAP callback names (OnInitialized,
        // OnInitializeFailed, OnPurchaseFailed) don't collide with the same-named
        // IPurchaseService events above. The SDK invokes them via the interface.

        void IStoreListener.OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            _controller = controller;
            _extensions = extensions;
            Log("Unity IAP initialized.");

            // Surface any non-consumable the store already reports as owned (Google Play
            // auto-restore on a fresh install with the same account lands here).
            foreach (var id in _productIds)
            {
                var p = _controller.products.WithID(id);
                if (p != null && p.hasReceipt)
                {
                    Log($"Already owned at init: {id}");
                    OnPurchaseSucceeded?.Invoke(id);
                }
            }

            OnInitialized?.Invoke();
        }

        void IStoreListener.OnInitializeFailed(InitializationFailureReason error)
        {
            HandleInitializeFailed(error, null);
        }

        void IStoreListener.OnInitializeFailed(InitializationFailureReason error, string message)
        {
            HandleInitializeFailed(error, message);
        }

        private void HandleInitializeFailed(InitializationFailureReason error, string message)
        {
            Log($"Unity IAP init failed: {error} {message}", true);
            OnInitializeFailed?.Invoke(error.ToString());
        }

        PurchaseProcessingResult IStoreListener.ProcessPurchase(PurchaseEventArgs args)
        {
            string id = args.purchasedProduct.definition.id;
            Log($"Purchase processed: {id}");
            OnPurchaseSucceeded?.Invoke(id);
            return PurchaseProcessingResult.Complete;
        }

        void IStoreListener.OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            string id = product != null ? product.definition.id : "unknown";
            Log($"Purchase failed: {id} ({failureReason})", true);
            OnPurchaseFailed?.Invoke(id, failureReason.ToString());
        }

        void IDetailedStoreListener.OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        {
            string id = product != null ? product.definition.id : "unknown";
            string reason = failureDescription != null
                ? $"{failureDescription.reason}: {failureDescription.message}"
                : "unknown";
            Log($"Purchase failed: {id} ({reason})", true);
            OnPurchaseFailed?.Invoke(id, reason);
        }

        private void Log(string msg, bool warn = false)
        {
            if (!_verbose && !warn) return;
            if (warn) Debug.LogWarning($"[IAP] {msg}");
            else Debug.Log($"[IAP] {msg}");
        }
    }
}
