using Ekimoz.Ads;
using Ekimoz.Splash;
using UnityEngine;

namespace Scene
{
    /// <summary>
    /// Brick Game's side of the Ekimoz splash: the jingle follows the SFX setting, and the
    /// banner, which shows itself as soon as the ad SDK is ready, stays hidden until the splash
    /// is gone. The splash package itself knows nothing about this game.
    /// </summary>
    public class SplashGlue : MonoBehaviour
    {
        private const float HideInterval = 0.25f;

        private float _nextHide;

        // Runs before the splash starts at BeforeSceneLoad.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Hook()
        {
            EkimozSplash.SoundAllowed = () => SFXToggle.IsSfxEnabled;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (!EkimozSplash.IsShowing) return;
            GameObject go = new GameObject("SplashGlue");
            DontDestroyOnLoad(go);
            go.AddComponent<SplashGlue>();
        }

        private void OnEnable()
        {
            EkimozSplash.OnFinished += HandleFinished;
        }

        private void OnDisable()
        {
            EkimozSplash.OnFinished -= HandleFinished;
        }

        // The SDK can finish loading the banner at any moment, so the hide is repeated.
        private void Update()
        {
            if (Time.unscaledTime < _nextHide) return;
            _nextHide = Time.unscaledTime + HideInterval;
            if (AdManager.Instance != null) AdManager.Instance.HideBanner();
        }

        private void HandleFinished()
        {
            if (AdManager.Instance != null) AdManager.Instance.ShowBanner();
            Destroy(gameObject);
        }
    }
}
