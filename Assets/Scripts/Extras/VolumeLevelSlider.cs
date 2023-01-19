using UnityEngine;
using UnityEngine.UI;

public class VolumeLevelSlider : MonoBehaviour
{
    [SerializeField] private float defaultVolume = 0.75f;
    [SerializeField] private Slider volumeLevelSlider;
    
    void Start()
    {
        if (!PlayerPrefs.HasKey("volumeLevel"))
            PlayerPrefs.SetFloat("volumeLevel", defaultVolume);

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
        AudioListener.volume = PlayerPrefs.GetFloat("volumeLevel");
    }
}
