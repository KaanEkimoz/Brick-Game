using UnityEngine;

public class BGMusicToggle : MonoBehaviour
{
    private const string PREFS_KEY = "BackgroundMusicEnabled";
    [SerializeField] private bool bgMusicEnabled = true;
    private AudioSource bgMusicSource;
    void Start()
    {
        bgMusicSource = GetComponent<AudioSource>();

        // Load background music enabled state from player prefs
        bgMusicEnabled = PlayerPrefs.GetInt(PREFS_KEY, 1) == 1;
    }

    private void BGMusicOn()
    {
        bgMusicSource.Play();
    }
    private void BGMusicOff()
    {
        bgMusicSource.Pause();
    }
    public void EnableBackgroundMusicAndPlay()
    {
        // Toggle background music enabled state
        bgMusicEnabled = true;

        // Save the background music enabled state to player prefs
        PlayerPrefs.SetInt(PREFS_KEY, bgMusicEnabled ? 1 : 0);

        // Play or stop music based on the new background music enabled state
        if (bgMusicEnabled)
        {
            bgMusicSource.Play();
        }
        else
        {
            bgMusicSource.Stop();
        }
    }
    public void EnableBackgroundMusic()
    {
        // Toggle background music enabled state
        bgMusicEnabled = true;

        // Save the background music enabled state to player prefs
        PlayerPrefs.SetInt(PREFS_KEY, bgMusicEnabled ? 1 : 0);
    }
    public void DisableBackgroundMusic()
    {
        // Toggle background music enabled state
        bgMusicEnabled = false;

        // Save the background music enabled state to player prefs
        PlayerPrefs.SetInt(PREFS_KEY, bgMusicEnabled ? 1 : 0);


        // Play or stop music based on the new background music enabled state
        if (bgMusicEnabled)
        {
            bgMusicSource.Play();
        }
        else
        {
            bgMusicSource.Stop();
        }

    }
    public void PlayMusic()
    {
        // Play music if it is enabled
        if (bgMusicEnabled)
            bgMusicSource.Play();
    }
}

