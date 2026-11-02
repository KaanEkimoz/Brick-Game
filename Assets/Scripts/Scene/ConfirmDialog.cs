using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Scene
{
    /// <summary>
    /// Asks for confirmation before a menu button runs. The button's existing click chain moves
    /// behind the dialog's Yes button, so the scene wiring stays the single definition of what
    /// the button does. The dialog reuses the menu's own buttons for its look.
    /// </summary>
    public class ConfirmDialog : MonoBehaviour
    {
        public const string MainMenuTitle = "Main Menu?";
        public const string MainMenuMessage = "Are you sure you want to leave?";
        public const string ConfirmText = "Confirm";
        public const string NoText = "Cancel";

        private const float TitleFontSize = 50f;
        private const float MessageFontSize = 30f;
        private const float ButtonFontSize = 52f;
        private static readonly Vector2 TextSize = new Vector2(490f, 110f);
        private static readonly Vector2 ButtonSize = new Vector2(560f, 142f);

        private GameObject _root;

        /// <summary>Builds the hidden dialog on the guarded button's panel and hooks the button.
        /// The menu roots above the panels are plain transforms with a large scale.</summary>
        public void Build(Button guardedButton, Button cancelTemplate, string title, string message, string yesText)
        {
            Transform panel = guardedButton.transform.parent;
            _root = CreateBackdrop(panel);

            TMP_Text neonLabel = cancelTemplate.GetComponentInChildren<TMP_Text>(true);
            TextMeshProUGUI heading = CreateText(Localization.Loc.Caps(title), neonLabel.font, TitleFontSize, neonLabel.color, new Vector2(0f, 185f));
            PanelStyle.FitLine(heading, TitleFontSize);
            CreateText(Localization.Loc.T(message), Levels.PlainFont.Asset, MessageFontSize, Color.white, new Vector2(0f, 105f));

            Button.ButtonClickedEvent guardedChain = guardedButton.onClick;
            Themes.ThemeManager.ThemeClickSound(guardedChain);
            guardedButton.onClick = new Button.ButtonClickedEvent();
            guardedButton.onClick.AddListener(() =>
            {
                // The panel can gain children after this dialog is built.
                _root.transform.SetAsLastSibling();
                _root.SetActive(true);
            });

            Button template = PanelStyle.FindButton();
            Button yes = CreateButton(template, yesText, new Vector2(0f, -30f));
            yes.onClick.AddListener(() =>
            {
                _root.SetActive(false);
                guardedChain.Invoke();
            });

            Button no = CreateButton(template, NoText, new Vector2(0f, -195f));
            no.onClick.AddListener(() => _root.SetActive(false));
        }

        public static GameObject CreateBackdrop(Transform panel)
        {
            GameObject go = new GameObject("ConfirmDialog", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(panel, false);
            RectTransform rect = (RectTransform)go.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            PanelStyle.ApplyFrame(go.GetComponent<Image>(), panel.GetComponent<Image>());
            go.SetActive(false);
            return go;
        }

        private TextMeshProUGUI CreateText(string text, TMP_FontAsset font, float fontSize, Color color, Vector2 position)
        {
            GameObject go = new GameObject("Text", typeof(RectTransform));
            go.transform.SetParent(_root.transform, false);
            TextMeshProUGUI label = go.AddComponent<TextMeshProUGUI>();
            label.font = font;
            label.fontSize = fontSize;
            label.color = color;
            label.alignment = TextAlignmentOptions.Center;
            label.raycastTarget = false;
            label.text = text;
            PlaceCentered(label.rectTransform, position, TextSize);
            return label;
        }

        private Button CreateButton(Button template, string text, Vector2 position)
        {
            Button button = Instantiate(template, _root.transform);
            button.gameObject.SetActive(true);
            button.onClick = new Button.ButtonClickedEvent();
            button.transform.localScale = Vector3.one;
            PlaceCentered((RectTransform)button.transform, position, ButtonSize);

            TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
            label.text = Localization.Loc.Caps(text);
            label.color = PanelStyle.ButtonTextColor;
            label.alignment = TextAlignmentOptions.Center;
            PanelStyle.FitButtonLabel(label, ButtonFontSize, ButtonSize.x);
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.offsetMin = Vector2.zero;
            label.rectTransform.offsetMax = Vector2.zero;
            return button;
        }

        private static void PlaceCentered(RectTransform rect, Vector2 position, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
        }
    }
}
