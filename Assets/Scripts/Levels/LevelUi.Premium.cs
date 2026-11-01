using Ekimoz.IAP;
using Localization;
using Themes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Levels
{
    public partial class LevelUi
    {
        private const string PremiumIconPath = "Icons/icon_premium_neon";
        private const string PremiumTheme = "deep_ocean";
        private const float PremiumPanelHeight = 980f;
        private const float PremiumFeatureHeight = 74f;
        private const float PremiumHeadingY = 370f;
        private const float PremiumFeaturesY = 225f;
        private const float PremiumActionY = -80f;
        private const float PremiumStatusY = -185f;
        private const float PremiumRestoreY = -275f;
        private const float PremiumBackY = -375f;

        private static readonly string[] PremiumFeatures = { "Play without ads", "Deep Ocean theme" };

        private TextMeshProUGUI _premiumStatus;

        private void ShowPremium()
        {
            PurchaseManager purchases = PurchaseManager.Instance;
            Transform screen = OpenScreen();
            screen.GetComponent<Image>().color = IntroDim;

            RectTransform panel = CreatePanel(screen, new Vector2(WinPanelSize.x, PremiumPanelHeight));
            TextMeshProUGUI heading = CreateHeading(panel, Loc.Caps("Premium"), 72f);
            Place(heading.rectTransform, new Vector2(0f, PremiumHeadingY), new Vector2(740f, 100f));

            for (int i = 0; i < PremiumFeatures.Length; i++)
            {
                TextMeshProUGUI feature = CreatePlainText(panel, Loc.T(PremiumFeatures[i]), 40f, Color.white);
                Place(feature.rectTransform, new Vector2(0f, PremiumFeaturesY - i * PremiumFeatureHeight), new Vector2(740f, 64f));
            }

            bool owned = purchases != null && purchases.PremiumOwned;
            Button action = owned
                ? CreateMenuStyleButton(panel, ThemeButtonLabel(), TogglePremiumTheme)
                : CreateMenuStyleButton(panel, BuyPremiumLabel(purchases), BuyPremium);
            Place((RectTransform)action.transform, new Vector2(0f, PremiumActionY), WinButtonSize);

            _premiumStatus = CreatePlainText(panel, owned ? Loc.T("Premium is active") : string.Empty, 34f, Accent);
            Place(_premiumStatus.rectTransform, new Vector2(0f, PremiumStatusY), new Vector2(740f, 56f));

            if (!owned)
            {
                Button restore = CreateMenuStyleButton(panel, "Restore Purchases", RestorePremium);
                Place((RectTransform)restore.transform, new Vector2(0f, PremiumRestoreY), WinButtonSize);
            }

            Button back = CreateMenuStyleButton(panel, "Back", ClosePremium);
            Place((RectTransform)back.transform, new Vector2(0f, PremiumBackY), WinButtonSize);

            if (purchases == null) return;
            purchases.OnPremiumOwnedChanged += HandlePremiumChanged;
            purchases.OnPurchaseFailed += HandlePremiumFailed;
        }

        private static string BuyPremiumLabel(PurchaseManager purchases)
        {
            string price = purchases != null ? purchases.PremiumPrice : null;
            return string.IsNullOrEmpty(price) ? "Buy" : Loc.Caps("Buy") + " " + price;
        }

        private static string ThemeButtonLabel()
        {
            return ThemeManager.Selected == PremiumTheme ? "Use Neon theme" : "Use Deep Ocean theme";
        }

        private void BuyPremium()
        {
            PurchaseManager purchases = PurchaseManager.Instance;
            if (purchases == null || !purchases.IsInitialized)
            {
                _premiumStatus.text = Loc.T("Store is not ready");
                return;
            }
            purchases.BuyPremium();
        }

        private static void RestorePremium()
        {
            PurchaseManager.Instance?.RestorePurchases();
        }

        private void HandlePremiumChanged(bool owned)
        {
            if (owned) ThemeManager.Selected = PremiumTheme;
            StopPremiumEvents();
            _session.ReturnToMenu();
        }

        private void HandlePremiumFailed(string reason)
        {
            if (_premiumStatus != null) _premiumStatus.text = Loc.T("Purchase failed");
        }

        private void TogglePremiumTheme()
        {
            ThemeManager.Selected = ThemeManager.Selected == PremiumTheme ? string.Empty : PremiumTheme;
            StopPremiumEvents();
            _session.ReturnToMenu();
        }

        private void ClosePremium()
        {
            StopPremiumEvents();
            CloseScreen();
        }

        private void StopPremiumEvents()
        {
            PurchaseManager purchases = PurchaseManager.Instance;
            if (purchases == null) return;
            purchases.OnPremiumOwnedChanged -= HandlePremiumChanged;
            purchases.OnPurchaseFailed -= HandlePremiumFailed;
        }
    }
}
