using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Localization
{
    /// <summary>
    /// Translates the text typed into the scene and adds the language button to Settings, so the
    /// shipped scene stays in English. Text that scripts write later is translated by those scripts.
    /// </summary>
    public static class SceneLocalizer
    {
        private const string BackButtonName = "B_Back";
        private const string SettingsPanelName = "SettingsPanel";
        private const float BackButtonDrop = 18f; // makes room for the language button
        private const float LanguageButtonGap = 10f;
        private const float LanguageFontScale = 0.5f;
        private static readonly Vector2 LanguageButtonSize = new Vector2(320f, 38f);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            SceneManager.sceneLoaded += (scene, mode) => Apply();
            Apply();
        }

        private static void Apply()
        {
            foreach (TMP_Text label in Resources.FindObjectsOfTypeAll<TMP_Text>())
            {
                if (!label.gameObject.scene.IsValid()) continue;
                if (string.IsNullOrEmpty(label.text)) continue;
                if (!Loc.TryGetSceneText(label.text, out string text)) continue;
                label.text = IsPlainFont(label) ? text : Loc.Upper(text);
            }

            foreach (Button button in Resources.FindObjectsOfTypeAll<Button>())
            {
                if (button.name == BackButtonName && button.gameObject.scene.IsValid()
                    && button.transform.parent.name == SettingsPanelName)
                    AddLanguageButton(button);
            }
        }

        private static bool IsPlainFont(TMP_Text label)
        {
            return label.font == Levels.PlainFont.Asset;
        }

        private static void AddLanguageButton(Button backButton)
        {
            Button button = UnityEngine.Object.Instantiate(backButton, backButton.transform.parent);
            button.name = "B_Language";
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(SelectNextLanguage);

            RectTransform rect = (RectTransform)button.transform;
            RectTransform back = (RectTransform)backButton.transform;
            back.anchoredPosition += Vector2.down * BackButtonDrop;
            rect.sizeDelta = LanguageButtonSize;
            rect.anchoredPosition = back.anchoredPosition
                + Vector2.up * (back.sizeDelta.y * 0.5f + LanguageButtonSize.y * 0.5f + LanguageButtonGap);

            TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
            label.text = Loc.Upper(Loc.F("Language: {0}", Loc.NativeName(Loc.Current)));
            label.fontSize *= LanguageFontScale;
        }

        // Reloading rebuilds every screen in the new language; this button is only in the menu.
        private static void SelectNextLanguage()
        {
            int count = Enum.GetValues(typeof(Language)).Length;
            Loc.Select((Language)(((int)Loc.Current + 1) % count));
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
