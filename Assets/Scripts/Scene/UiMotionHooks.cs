using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Scene
{
    /// <summary>Adds the menu motion to the scene: every button presses, the scene's panels
    /// play an entrance, the logo floats and Play breathes. Buttons built later are found by a
    /// light periodic scan, the same way their click sound is.</summary>
    public class UiMotionHooks : MonoBehaviour
    {
        private const float ScanInterval = 0.25f;
        private const string LogoName = "I_Logo";
        private const string PlayName = "B_Play";
        private const string GameOverPanelName = "GOM_Panel";
        private static readonly string[] PopPanelNames = { "PausePanel", "SettingsPanel", "ContinuePromptPanel" };

        private readonly HashSet<int> _seen = new HashSet<int>();
        private float _nextScan;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Create()
        {
            GameObject go = new GameObject(nameof(UiMotionHooks));
            DontDestroyOnLoad(go);
            go.AddComponent<GameOverMotion>();
            go.AddComponent<PieceLandingSquash>();
            go.AddComponent<UiMotionHooks>();
        }

        private void Awake()
        {
            SceneManager.sceneLoaded += HandleSceneLoaded;
            HookScene();
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }

        private void HandleSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
        {
            _seen.Clear();
            _nextScan = 0f;
            HookScene();
        }

        private void HookScene()
        {
            foreach (RectTransform rect in Resources.FindObjectsOfTypeAll<RectTransform>())
            {
                if (!rect.gameObject.scene.IsValid()) continue;
                string name = rect.name;
                if (name == GameOverPanelName)
                {
                    PanelPop.Add(rect.gameObject, PanelPop.Style.SlideUp);
                    GetComponent<GameOverMotion>().Bind(rect);
                }
                else if (System.Array.IndexOf(PopPanelNames, name) >= 0) PanelPop.Add(rect.gameObject, PanelPop.Style.Pop);
                else if (name == LogoName && rect.GetComponent<LogoFloat>() == null) rect.gameObject.AddComponent<LogoFloat>();
            }
        }

        private void Update()
        {
            if (Time.unscaledTime < _nextScan) return;
            _nextScan = Time.unscaledTime + ScanInterval;

            foreach (Button button in FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (!_seen.Add(button.GetInstanceID())) continue;
                if (button.transition == Selectable.Transition.Animation || button.GetComponent<ButtonMotion>() != null) continue;

                ButtonMotion motion = button.gameObject.AddComponent<ButtonMotion>();
                if (button.name == PlayName) motion.EnablePulse();
            }
        }
    }
}
