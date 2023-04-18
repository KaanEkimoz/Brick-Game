using UnityEngine;
using UnityEngine.UI;

public class VolumeLevelSlider : MonoBehaviour
{
    [SerializeField] private float defaultVolume = 0.75f;
    [SerializeField] private Slider volumeLevelSlider;
    
    void Start()
    {
        LoadVolume();
    }
    public void ChangeVolume()
    {
        AudioListener.volume = volumeLevelSlider.value;
    }
    private void LoadVolume()
    {
        AudioListener.volume = defaultVolume;
    }
}
