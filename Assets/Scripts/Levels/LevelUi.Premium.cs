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
        private const string PremiumIconPath = "Icons/icon_diamond_neon";
        private const string RemoveAdsIconName = "B_RemoveAds";
        private const string GameRemoveAdsName = "B_RemoveAds_Game";
        private const string GamePauseName = "B_Pause";
        private const float GameIconStep = 0.075f; // screen share between the left column's icons
        private const float PremiumPanelHeight = 1060f;
        private const float PremiumFeatureHeight = 74f;
        private const float PremiumHeadingY = 370f;
        private const float PremiumFeaturesY = 225f;
        private const float PremiumLockHeadingY = 410f;
        private const float PremiumLockNoticeY = 305f;
        private const float PremiumLockFeaturesY = 185f;
        private const float PremiumActionY = -80f;
        private const float PremiumStatusY = -185f;
        private const float PremiumRestoreY = -255f;
        private const float PremiumBackY = -345f;

        private static readonly string[] PremiumFeatures = { "Remove all ads", "100 more levels", "All themes unlocked" };

        private TextMeshProUGUI _premiumStatus;
        private bool _premiumLocked;

        // The left column stacks under the no-ads icon, in the same size.
        private void CreateLeftMenuIcon(string name, string spritePath, int slot, UnityEngine.Events.UnityAction onClick)
        {
            RectTransform removeAds = null;
            Button template = null;
            foreach (Button button in Resources.FindObjectsOfTypeAll<Button>())
            {
                if (!button.gameObject.scene.IsValid()) continue;
                if (button.name == RemoveAdsIconName) removeAds = (RectTransform)button.transform;
                if (button.name == SettingsIconName) template = button;
            }
            if (removeAds == null || template == null) return;

            Button icon = Instantiate(template, removeAds.parent);
            icon.name = name;
            icon.onClick = new Button.ButtonClickedEvent();
            icon.onClick.AddListener(onClick);
            icon.GetComponent<Image>().sprite = Resources.Load<Sprite>(spritePath);

            RectTransform rect = (RectTransform)icon.transform;
            rect.anchorMin = removeAds.anchorMin;
            rect.anchorMax = removeAds.anchorMax;
            rect.pivot = removeAds.pivot;
            rect.sizeDelta = removeAds.sizeDelta;
            rect.anchoredPosition = removeAds.anchoredPosition + Vector2.down * ((removeAds.rect.height + LanguageIconGap) * slot);
        }

        // No-ads sits last, so hiding it once bought leaves no gap in the column.
        private void MoveRemoveAdsIcon(int slot)
        {
            foreach (Button button in Resources.FindObjectsOfTypeAll<Button>())
            {
                if (!button.gameObject.scene.IsValid() || button.name != RemoveAdsIconName) continue;
                RectTransform rect = (RectTransform)button.transform;
                rect.anchoredPosition += Vector2.down * ((rect.rect.height + LanguageIconGap) * slot);
                return;
            }
        }

        // Hidden for Premium owners, who have nothing left to buy here.
        private void CreateGamePremiumIcon()
        {
            if (PurchaseManager.Instance != null && PurchaseManager.Instance.PremiumOwned) return;
            if (PlayerPrefs.GetInt(PurchaseManager.PremiumPrefKey) == 1) return;

            RectTransform removeAds = null;
            foreach (Button button in Resources.FindObjectsOfTypeAll<Button>())
                if (button.gameObject.scene.IsValid() && button.name == GameRemoveAdsName)
                    removeAds = (RectTransform)button.transform;
            if (removeAds == null) return;

            GameObject icon = new GameObject("B_PremiumGame", typeof(RectTransform), typeof(Image), typeof(Button));
            icon.transform.SetParent(removeAds.parent, false);
            icon.transform.SetSiblingIndex(removeAds.GetSiblingIndex() + 1);
            Image image = icon.GetComponent<Image>();
            image.sprite = Resources.Load<Sprite>(PremiumIconPath);
            image.preserveAspect = true;
            icon.GetComponent<Button>().onClick.AddListener(ShowPremiumInGame);

            RectTransform rect = (RectTransform)icon.transform;
            rect.anchorMin = removeAds.anchorMin + Vector2.down * GameIconStep;
            rect.anchorMax = removeAds.anchorMax + Vector2.down * GameIconStep;
            rect.pivot = removeAds.pivot;
            rect.sizeDelta = removeAds.sizeDelta;
            rect.anchoredPosition = removeAds.anchoredPosition;
        }

        // The run waits behind the pause menu while the player looks at the offer.
        private void ShowPremiumInGame()
        {
            foreach (Button button in Resources.FindObjectsOfTypeAll<Button>())
            {
                if (!button.gameObject.scene.IsValid() || button.name != GamePauseName) continue;
                button.onClick.Invoke();
                break;
            }
            ShowPremium();
        }

        // Reached from the menu or from a won level, so leaving always goes back to the menu.
        private void ShowPremiumLock()
        {
            _premiumLocked = true;
            ShowPremium();
        }

        private void ShowPremium()
        {
            PurchaseManager purchases = PurchaseManager.Instance;
            Transform screen = OpenScreen();
            screen.GetComponent<Image>().color = IntroDim;

            RectTransform panel = CreatePanel(screen, new Vector2(WinPanelSize.x, PremiumPanelHeight));
            TextMeshProUGUI heading = CreateHeading(panel, Loc.Caps("Premium"), 72f);
            Place(heading.rectTransform, new Vector2(0f, _premiumLocked ? PremiumLockHeadingY : PremiumHeadingY), new Vector2(740f, 100f));

            if (_premiumLocked)
            {
                TextMeshProUGUI notice = CreatePlainText(panel, Loc.T("Get Premium to play the next level. Premium includes:"), 36f, Accent);
                Place(notice.rectTransform, new Vector2(0f, PremiumLockNoticeY), new Vector2(740f, 110f));
            }

            float featuresY = _premiumLocked ? PremiumLockFeaturesY : PremiumFeaturesY;
            for (int i = 0; i < PremiumFeatures.Length; i++)
            {
                TextMeshProUGUI feature = CreatePlainText(panel, Loc.T(PremiumFeatures[i]), 40f, Color.white);
                Place(feature.rectTransform, new Vector2(0f, featuresY - i * PremiumFeatureHeight), new Vector2(740f, 64f));
            }

            bool owned = purchases != null && purchases.PremiumOwned;
            if (!owned)
            {
                Button buy = CreateMenuStyleButton(panel, BuyPremiumLabel(purchases), BuyPremium);
                Place((RectTransform)buy.transform, new Vector2(0f, PremiumActionY), WinButtonSize);
            }

            _premiumStatus = CreatePlainText(panel, owned ? Loc.T("Premium is active") : string.Empty, 34f, Accent);
            Place(_premiumStatus.rectTransform, new Vector2(0f, PremiumStatusY), new Vector2(740f, 56f));

            if (!owned)
            {
                Button restore = CreateMenuStyleButton(panel, "Restore Purchases", RestorePremium);
                Place((RectTransform)restore.transform, new Vector2(0f, PremiumRestoreY), WinButtonSize);
            }

            Button back = CreateBackLink(panel, ClosePremium);
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
            if (owned) ThemeManager.Selected = OceanThemeName;
            StopPremiumEvents();
            _session.ReturnToMenu();
        }

        private void HandlePremiumFailed(string reason)
        {
            if (_premiumStatus != null) _premiumStatus.text = Loc.T("Purchase failed");
        }

        private void ClosePremium()
        {
            StopPremiumEvents();
            if (_premiumLocked) _session.ReturnToMenu();
            else CloseScreen();
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
