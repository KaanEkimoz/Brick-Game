using UnityEngine;
using UnityEngine.UI;

public class VolumeLevelSlider : MonoBehaviour
{
    [SerializeField] private float defaultVolume = 0.5f;
    [SerializeField] private Slider volumeLevelSlider;

    void Awake()
    {
        if (!PlayerPrefs.HasKey("volumeLevel"))
            PlayerPrefs.SetFloat("volumeLevel", defaultVolume);
    }

    void OnEnable()
    {
        LoadVolume();
    }

    public void ChangeVolume()
    {
        AudioListener.volume = volumeLevelSlider.value;
        SaveVolume();
    }
    private void SaveVolume()
    {
        PlayerPrefs.SetFloat("volumeLevel", volumeLevelSlider.value);
    }
    private void LoadVolume()
    {
        float v = PlayerPrefs.GetFloat("volumeLevel");
        AudioListener.volume = v;
        if (volumeLevelSlider != null)
            volumeLevelSlider.SetValueWithoutNotify(v);
    }
}
