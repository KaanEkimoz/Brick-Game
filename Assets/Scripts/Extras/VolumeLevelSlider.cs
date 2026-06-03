using Extras;
using UnityEngine;
using UnityEngine.UI;

public class VolumeLevelSlider : MonoBehaviour
{
    [SerializeField] private Slider volumeLevelSlider;

    private void Awake()
    {
        // VolumeBootstrap already seeds the pref BeforeSceneLoad, but be defensive: if the
        // bootstrap was edited / disabled / failed, ensure the key exists before any read.
        if (!PlayerPrefs.HasKey(VolumeBootstrap.PrefKey))
        {
            PlayerPrefs.SetFloat(VolumeBootstrap.PrefKey, VolumeBootstrap.DefaultVolume);
            PlayerPrefs.Save();
        }
    }

    private void OnEnable() => LoadVolume();

    /// <summary>Wired from the Slider's OnValueChanged in the Inspector. Applied + saved
    /// every drag tick so the value persists even if the app is force-killed.</summary>
    public void ChangeVolume()
    {
        if (volumeLevelSlider == null) return;
        AudioListener.volume = volumeLevelSlider.value;
        PlayerPrefs.SetFloat(VolumeBootstrap.PrefKey, volumeLevelSlider.value);
        PlayerPrefs.Save();
    }

    private void LoadVolume()
    {
        // GetFloat with the explicit default — never let a missing key produce 0 (silent).
        float v = PlayerPrefs.GetFloat(VolumeBootstrap.PrefKey, VolumeBootstrap.DefaultVolume);
        AudioListener.volume = v;
        if (volumeLevelSlider != null)
            volumeLevelSlider.SetValueWithoutNotify(v);
    }
}
