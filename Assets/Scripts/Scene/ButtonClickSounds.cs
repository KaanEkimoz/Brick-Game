using System.Collections.Generic;
using Themes;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Scene
{
    /// <summary>Gives every button the menu's click sound. Scene buttons carry it in their
    /// wiring; buttons built in code do not, so they get it here as they appear. It plays
    /// through the scene's own click source, which the sound switch mutes.</summary>
    public class ButtonClickSounds : MonoBehaviour
    {
        private const string ClickPath = "Sounds/ui_click";
        private const string PlayOneShotMethod = "PlayOneShot";
        private const float ScanInterval = 0.25f;

        private static ButtonClickSounds _instance;
        private static readonly HashSet<int> Skipped = new HashSet<int>();

        private readonly HashSet<int> _hooked = new HashSet<int>();
        private AudioClip _defaultClip;
        private AudioSource _source;
        private float _nextScan;

        /// <summary>Leaves a button silent, for one that runs another button's sounding chain.</summary>
        public static void Skip(Button button)
        {
            Skipped.Add(button.GetInstanceID());
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Create()
        {
            GameObject go = new GameObject(nameof(ButtonClickSounds));
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<ButtonClickSounds>();
        }

        private void Awake()
        {
            _defaultClip = Resources.Load<AudioClip>(ClickPath);
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }

        private void HandleSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
        {
            _hooked.Clear();
            Skipped.Clear();
            _source = null;
            _nextScan = 0f;
        }

        private void Update()
        {
            if (Time.unscaledTime < _nextScan) return;
            _nextScan = Time.unscaledTime + ScanInterval;

            foreach (Button button in FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (!_hooked.Add(button.GetInstanceID())) continue;

                AudioSource wired = WiredSource(button.onClick);
                if (wired != null)
                {
                    if (_source == null) _source = wired;
                    continue;
                }
                button.onClick.AddListener(() => Play(button));
            }
        }

        private static AudioSource WiredSource(UnityEvent clickChain)
        {
            for (int i = 0; i < clickChain.GetPersistentEventCount(); i++)
                if (clickChain.GetPersistentMethodName(i) == PlayOneShotMethod && clickChain.GetPersistentTarget(i) is AudioSource source)
                    return source;
            return null;
        }

        private void Play(Button button)
        {
            if (Skipped.Contains(button.GetInstanceID())) return;
            if (!SFXToggle.IsSfxEnabled || _source == null) return;

            AudioClip clip = ThemeManager.ButtonClip != null ? ThemeManager.ButtonClip : _defaultClip;
            if (clip != null) _source.PlayOneShot(clip);
        }

        /// <summary>Plays a clip through the click source, for sounds that are not buttons.</summary>
        public static void PlayEffect(AudioClip clip)
        {
            if (_instance == null || _instance._source == null || clip == null) return;
            if (SFXToggle.IsSfxEnabled) _instance._source.PlayOneShot(clip);
        }
    }
}
