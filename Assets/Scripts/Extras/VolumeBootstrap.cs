using UnityEngine;

namespace Extras
{
    /// <summary>
    /// Applies the saved master volume to AudioListener BEFORE any scene loads,
    /// so the first audio frame already respects the user's preference.
    ///
    /// Bug it fixes: VolumeLevelSlider lived under PauseMenu, which is inactive at
    /// scene start, so its OnEnable never ran until the player opened Pause and
    /// touched the slider. Until then AudioListener.volume stayed at the engine
    /// default (1.0) — music and SFX were loud, then dropped to the saved value
    /// only after the slider was nudged. This bootstrap runs unconditionally on
    /// every scene load so the saved value is applied immediately.
    /// </summary>
    public static class VolumeBootstrap
    {
        /// <summary>PlayerPrefs key shared by VolumeBootstrap and VolumeLevelSlider so the
        /// "what key holds the master volume" answer lives in exactly one place.</summary>
        public const string PrefKey = "volumeLevel";

        /// <summary>Default master volume used when no saved value exists yet. Shared with
        /// VolumeLevelSlider so the two never drift if the default is retuned.</summary>
        public const float DefaultVolume = 0.5f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Apply()
        {
            if (!PlayerPrefs.HasKey(PrefKey))
            {
                PlayerPrefs.SetFloat(PrefKey, DefaultVolume);
                PlayerPrefs.Save();
            }
            AudioListener.volume = PlayerPrefs.GetFloat(PrefKey, DefaultVolume);
        }
    }
}
