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
        public const string MainMenuYes = "Yes";
        public const string NoText = "Cancel";

        private const float ButtonFontSize = 44f;
        private static readonly Color BackdropColor = new Color(0f, 0f, 0.08f, 0.92f);

        private GameObject _root;

        /// <summary>Builds the hidden dialog on the guarded button's panel and hooks the button.
        /// The menu roots above the panels are plain transforms with a large scale.</summary>
        public void Build(Button guardedButton, Button cancelTemplate, string title, string message, string yesText)
        {
            _root = CreateBackdrop(guardedButton.transform.parent);

            TMP_Text buttonLabel = cancelTemplate.GetComponentInChildren<TMP_Text>(true);
            CreateText(Localization.Loc.T(title), buttonLabel, 64f, new Vector2(0f, 230f));
            CreateText(Localization.Loc.T(message), buttonLabel, 34f, new Vector2(0f, 120f));

            Button.ButtonClickedEvent guardedChain = guardedButton.onClick;
            guardedButton.onClick = new Button.ButtonClickedEvent();
            guardedButton.onClick.AddListener(() => _root.SetActive(true));

            Button yes = CreateButton(guardedButton, yesText, new Vector2(0f, -30f));
            yes.onClick.AddListener(() =>
            {
                _root.SetActive(false);
                guardedChain.Invoke();
            });

            Button no = CreateButton(cancelTemplate, NoText, new Vector2(0f, -160f));
            no.onClick.AddListener(() => _root.SetActive(false));
        }

        private static GameObject CreateBackdrop(Transform parent)
        {
            GameObject go = new GameObject("ConfirmDialog", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            RectTransform rect = (RectTransform)go.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            go.GetComponent<Image>().color = BackdropColor;
            go.SetActive(false);
            return go;
        }

        private void CreateText(string text, TMP_Text style, float fontSize, Vector2 position)
        {
            GameObject go = new GameObject("Text", typeof(RectTransform));
            go.transform.SetParent(_root.transform, false);
            TextMeshProUGUI label = go.AddComponent<TextMeshProUGUI>();
            label.font = Levels.PlainFont.Asset;
            label.fontSize = fontSize;
            label.color = style.color;
            label.alignment = TextAlignmentOptions.Center;
            label.raycastTarget = false;
            label.text = text;
            PlaceCentered(label.rectTransform, position, new Vector2(560f, 110f));
        }

        private Button CreateButton(Button template, string text, Vector2 position)
        {
            Button button = Instantiate(template, _root.transform);
            button.onClick = new Button.ButtonClickedEvent();
            button.transform.localScale = Vector3.one;
            PlaceCentered((RectTransform)button.transform, position, new Vector2(440f, 110f));

            TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
            label.font = Levels.PlainFont.Asset;
            label.text = Localization.Loc.T(text);
            label.fontSize = ButtonFontSize;
            label.rectTransform.anchoredPosition = Vector2.zero;
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
