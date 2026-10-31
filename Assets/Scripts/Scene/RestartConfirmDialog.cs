using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Scene
{
    /// <summary>
    /// Asks for confirmation before the pause menu's Restart button runs. The button's existing
    /// click chain moves behind the dialog's Yes button, so the scene wiring stays the single
    /// definition of what Restart does. The dialog reuses the pause menu's buttons for its look.
    /// </summary>
    public class RestartConfirmDialog : MonoBehaviour
    {
        private const string TitleText = "Restart?";
        private const string MessageText = "Your current game will be lost";
        private const string YesText = "Yes, restart";
        private const string NoText = "Cancel";
        private const float ButtonFontSize = 26f;
        private static readonly Color BackdropColor = new Color(0f, 0f, 0.08f, 0.92f);

        private GameObject _root;

        /// <summary>Builds the hidden dialog inside the pause menu and hooks the Restart button.</summary>
        public void Build(Transform pauseMenu, Button restartButton, Button cancelTemplate)
        {
            _root = CreateBackdrop(pauseMenu);

            TMP_Text buttonLabel = cancelTemplate.GetComponentInChildren<TMP_Text>(true);
            CreateText(TitleText, buttonLabel, 44f, new Vector2(0f, 150f));
            CreateText(MessageText, buttonLabel, 20f, new Vector2(0f, 90f));

            Button.ButtonClickedEvent restartChain = restartButton.onClick;
            restartButton.onClick = new Button.ButtonClickedEvent();
            restartButton.onClick.AddListener(() => _root.SetActive(true));

            Button yes = CreateButton(restartButton, YesText, new Vector2(0f, -10f));
            yes.onClick.AddListener(() =>
            {
                _root.SetActive(false);
                restartChain.Invoke();
            });

            Button no = CreateButton(cancelTemplate, NoText, new Vector2(0f, -90f));
            no.onClick.AddListener(() => _root.SetActive(false));
        }

        private static GameObject CreateBackdrop(Transform parent)
        {
            GameObject go = new GameObject("RestartConfirm", typeof(RectTransform), typeof(Image));
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
            label.font = style.font;
            label.fontSize = fontSize;
            label.color = style.color;
            label.alignment = TextAlignmentOptions.Center;
            label.raycastTarget = false;
            label.text = text;
            PlaceCentered(label.rectTransform, position, new Vector2(520f, 80f));
        }

        private Button CreateButton(Button template, string text, Vector2 position)
        {
            Button button = Instantiate(template, _root.transform);
            button.onClick = new Button.ButtonClickedEvent();
            PlaceCentered((RectTransform)button.transform, position, new Vector2(360f, 64f));

            TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
            label.text = text;
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
