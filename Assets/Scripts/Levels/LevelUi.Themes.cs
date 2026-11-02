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
        private const string ThemesIconPath = "Icons/icon_themes_neon";
        private const string ThemePreviewFolder = "ThemePreviews/";
        private const string OceanThemeName = "deep_ocean";
        private const string NeonPreviewName = "neon";
        private const float ThemesPanelHeight = 1300f;
        private const float ThemesHeadingY = 550f;
        private const float ThemeCardX = 200f;
        private const float ThemeCardY = 120f;
        private const float ThemeNameY = -310f;
        private const float ThemeBadgeY = -362f;
        private const float ThemesBackY = -450f;
        private const float ThemeOutlineSize = 6f;
        private static readonly Vector2 ThemeCardSize = new Vector2(340f, 580f);

        private void ShowThemes()
        {
            Transform screen = OpenScreen();
            screen.GetComponent<Image>().color = IntroDim;

            RectTransform panel = CreatePanel(screen, new Vector2(WinPanelSize.x, ThemesPanelHeight));
            TextMeshProUGUI heading = CreateHeading(panel, Loc.Caps("Themes"), 72f);
            Place(heading.rectTransform, new Vector2(0f, ThemesHeadingY), new Vector2(740f, 100f));

            CreateThemeCard(panel, NeonPreviewName, "Neon", string.Empty, -ThemeCardX);
            CreateThemeCard(panel, OceanThemeName, "Deep Ocean", OceanThemeName, ThemeCardX);

            Button back = CreateBackLink(panel, CloseScreen);
            Place((RectTransform)back.transform, new Vector2(0f, ThemesBackY), WinButtonSize);
        }

        private void CreateThemeCard(Transform panel, string preview, string label, string theme, float x)
        {
            bool free = string.IsNullOrEmpty(theme);
            bool premium = PurchaseManager.Instance != null && PurchaseManager.Instance.PremiumOwned;
            bool selected = ThemeManager.Selected == theme;

            GameObject card = new GameObject("ThemeCard_" + preview, typeof(RectTransform), typeof(Image), typeof(Button));
            card.transform.SetParent(panel, false);
            Image image = card.GetComponent<Image>();
            image.sprite = Resources.Load<Sprite>(ThemePreviewFolder + preview);
            image.preserveAspect = false;
            Place((RectTransform)card.transform, new Vector2(x, ThemeCardY), ThemeCardSize);

            Outline outline = card.AddComponent<Outline>();
            outline.effectColor = selected ? Accent : new Color(1f, 1f, 1f, 0.25f);
            outline.effectDistance = Vector2.one * ThemeOutlineSize;
            card.GetComponent<Button>().onClick.AddListener(() => SelectTheme(theme, free || premium));

            TextMeshProUGUI name = CreateHeading(panel, Loc.Caps(label), 40f);
            Place(name.rectTransform, new Vector2(x, ThemeNameY), new Vector2(360f, 56f));

            string state = selected ? Loc.T("Selected") : free || premium ? string.Empty : Loc.T("Premium");
            TextMeshProUGUI badge = CreatePlainText(panel, state, 30f, Accent);
            Place(badge.rectTransform, new Vector2(x, ThemeBadgeY), new Vector2(360f, 44f));
        }

        private void SelectTheme(string theme, bool available)
        {
            if (!available)
            {
                ShowPremium();
                return;
            }
            if (ThemeManager.Selected == theme)
            {
                CloseScreen();
                return;
            }

            ThemeManager.Selected = theme;
            _session.ReturnToMenu();
        }
    }
}
