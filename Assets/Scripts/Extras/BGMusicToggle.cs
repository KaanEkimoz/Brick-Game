using UnityEngine;

public class BGMusicToggle : MonoBehaviour
{
    private const string PREFS_KEY = "BackgroundMusicEnabled";
    private bool bgMusicEnabled = true;
    private AudioSource bgMusicSource;
    void Start()
    {
        bgMusicSource = GetComponent<AudioSource>();
        bgMusicEnabled = true;
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

    }
    public void DisableBackgroundMusic()
    {
        // Toggle background music enabled state
        bgMusicEnabled = false;

        bgMusicSource.Stop();
    }
    public void PlayMusic()
    {
        // Play music if it is enabled
        if (bgMusicEnabled)
            bgMusicSource.Play();
    }
}

