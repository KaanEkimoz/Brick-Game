using System;
using System.Collections.Generic;

namespace Ekimoz.IAP
{
    /// <summary>
    /// Store-agnostic in-app purchase surface. The game only ever talks to this interface;
    /// the concrete service (Unity IAP today) hides the vendor SDK, exactly like
    /// <c>IAdProvider</c> does for ads. Swapping billing backends means swapping the
    /// service, not touching game code. Designed to live alongside the Ads layer so every
    /// Ekimoz title reuses the same shape.
    /// </summary>
    public interface IPurchaseService
    {
        bool IsInitialized { get; }

        /// <summary>Fired once the underlying billing SDK finishes initialization.</summary>
        event Action OnInitialized;

        /// <summary>Initialization failed (no billing, offline, etc.). string = reason.</summary>
        event Action<string> OnInitializeFailed;

        /// <summary>A purchase (or auto-restore at launch) completed. string = product id.</summary>
        event Action<string> OnPurchaseSucceeded;

        /// <summary>A purchase failed or was cancelled. (product id, reason).</summary>
        event Action<string, string> OnPurchaseFailed;

        /// <summary>Register the non-consumable product ids and start the billing SDK.</summary>
        void Initialize(IEnumerable<string> nonConsumableProductIds);

        /// <summary>Begin the platform purchase flow for the given product.</summary>
        void Purchase(string productId);

        /// <summary>Re-query owned non-consumables (mainly for iOS; Android auto-restores at init).</summary>
        void Restore();

        /// <summary>True if the store reports this non-consumable as already owned.</summary>
        bool IsOwned(string productId);

        /// <summary>Localized price string for UI (e.g. "₺24,99"), or null if unknown.</summary>
        string GetLocalizedPrice(string productId);
    }
}
