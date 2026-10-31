using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Localization
{
    /// <summary>
    /// Translates the text typed into the scene, so the shipped scene stays in English. Text that
    /// scripts write later is translated by those scripts.
    /// </summary>
    public static class SceneLocalizer
    {
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
        }

        private static bool IsPlainFont(TMP_Text label)
        {
            return label.font == Levels.PlainFont.Asset;
        }
    }
}
