using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Localization
{
    public enum Language
    {
        English,
        Turkish,
        Spanish,
    }

    /// <summary>
    /// Text lookup for every player-facing string. The English text is the key, so a missing
    /// translation falls back to English. Tables live in Resources/Localization.
    /// </summary>
    public static class Loc
    {
        private const string LanguagePrefKey = "language";
        private const string UiTablePath = "Localization/ui_";
        private const string LevelTablePath = "Localization/levels_";

        private static readonly Regex LineBreakSpacing = new Regex(@"\s*\n\s*");

        private static readonly Dictionary<string, string> UiText = new Dictionary<string, string>();
        private static readonly Dictionary<string, string> SceneText = new Dictionary<string, string>();
        private static readonly Dictionary<int, LevelTextEntry> LevelText = new Dictionary<int, LevelTextEntry>();
        private static bool _loaded;
        private static Language _current;

        public static Language Current
        {
            get
            {
                EnsureLoaded();
                return _current;
            }
        }

        /// <summary>The language's own name, for the language button.</summary>
        public static string NativeName(Language language)
        {
            switch (language)
            {
                case Language.Turkish: return "Türkçe";
                case Language.Spanish: return "Español";
                default: return "English";
            }
        }

        public static void Select(Language language)
        {
            PlayerPrefs.SetInt(LanguagePrefKey, (int)language);
            PlayerPrefs.Save();
            Load(language);
        }

        public static string T(string english)
        {
            EnsureLoaded();
            return UiText.TryGetValue(english, out string text) ? text : english;
        }

        public static string F(string english, params object[] args)
        {
            return string.Format(T(english), args);
        }

        /// <summary>Translated and upper-cased. The game's neon font only has capitals, which
        /// hides the Turkish dot on a lowercase i, so text in that font goes through here.</summary>
        public static string Caps(string english)
        {
            return Upper(T(english));
        }

        public static string Upper(string text)
        {
            if (Current == Language.Turkish)
                text = text.Replace('i', 'İ').Replace('ı', 'I');
            return text.ToUpperInvariant();
        }

        public static string LevelName(int id, string english)
        {
            EnsureLoaded();
            return LevelText.TryGetValue(id, out LevelTextEntry entry) ? entry.name : english;
        }

        public static string LevelHint(int id, string english)
        {
            EnsureLoaded();
            return LevelText.TryGetValue(id, out LevelTextEntry entry) ? entry.hint : english;
        }

        /// <summary>Finds the translation of a text typed into the scene. Scene text differs from
        /// the keys in case and in the spaces around line breaks, so those are ignored.</summary>
        public static bool TryGetSceneText(string sceneText, out string text)
        {
            EnsureLoaded();
            return SceneText.TryGetValue(Normalize(sceneText), out text);
        }

        private static string Normalize(string text)
        {
            return LineBreakSpacing.Replace(text.Trim(), "\n").ToLowerInvariant();
        }

        private static void EnsureLoaded()
        {
            if (_loaded) return;
            Load(ReadSavedLanguage());
        }

        private static Language ReadSavedLanguage()
        {
            if (PlayerPrefs.HasKey(LanguagePrefKey))
                return (Language)PlayerPrefs.GetInt(LanguagePrefKey);

            switch (Application.systemLanguage)
            {
                case SystemLanguage.Turkish: return Language.Turkish;
                case SystemLanguage.Spanish: return Language.Spanish;
                default: return Language.English;
            }
        }

        private static void Load(Language language)
        {
            _loaded = true;
            _current = language;
            UiText.Clear();
            SceneText.Clear();
            LevelText.Clear();
            if (language == Language.English) return;

            string code = language == Language.Turkish ? "tr" : "es";
            TextAsset ui = Resources.Load<TextAsset>(UiTablePath + code);
            if (ui != null)
            {
                foreach (UiTextEntry entry in JsonUtility.FromJson<UiTextFile>(ui.text).entries)
                {
                    UiText[entry.en] = entry.text;
                    SceneText[Normalize(entry.en)] = entry.text;
                }
            }

            TextAsset levels = Resources.Load<TextAsset>(LevelTablePath + code);
            if (levels != null)
            {
                foreach (LevelTextEntry entry in JsonUtility.FromJson<LevelTextFile>(levels.text).entries)
                    LevelText[entry.id] = entry;
            }
        }

        [Serializable]
        private class UiTextFile
        {
            public UiTextEntry[] entries;
        }

        [Serializable]
        private class UiTextEntry
        {
            public string en;
            public string text;
        }

        [Serializable]
        private class LevelTextFile
        {
            public LevelTextEntry[] entries;
        }

        [Serializable]
        private class LevelTextEntry
        {
            public int id;
            public string name;
            public string hint;
        }
    }
}
