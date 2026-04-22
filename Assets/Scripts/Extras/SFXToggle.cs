using UnityEngine;

public class SFXToggle : MonoBehaviour
{
    private const string PREFS_KEY = "SFXEnabled";
    [SerializeField] private bool sfxEnabled = true;
    private AudioSource sfxSource;

    void Awake()
    {
        sfxSource = GetComponent<AudioSource>();
        sfxEnabled = PlayerPrefs.GetInt(PREFS_KEY, 1) == 1;
        if (sfxSource != null)
            sfxSource.mute = !sfxEnabled;
    }

    public void EnableSFX()
    {
        sfxEnabled = true;
        PlayerPrefs.SetInt(PREFS_KEY, 1);
        if (sfxSource != null)
            sfxSource.mute = false;
    }

    public void DisableSFX()
    {
        sfxEnabled = false;
        PlayerPrefs.SetInt(PREFS_KEY, 0);
        if (sfxSource != null)
            sfxSource.mute = true;
    }

    public void SetEnabled(bool enabled)
    {
        if (enabled) EnableSFX();
        else DisableSFX();
    }

    public bool IsEnabled() => sfxEnabled;
}
