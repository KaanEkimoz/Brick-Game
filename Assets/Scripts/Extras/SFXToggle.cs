using UnityEngine;

public class SFXToggle : MonoBehaviour
{
    private const string PREFS_KEY = "SFXEnabled";
    [SerializeField] private bool sfxEnabled = true;
    private AudioSource sfxSource;

    /// <summary>Global SFX gate read by every SFX script before it plays.
    /// Seeded BeforeSceneLoad from PlayerPrefs so SFX scripts that fire in Awake
    /// (e.g. PieceSpawner spawn-click) see the correct value regardless of script
    /// execution order. Flipped synchronously by EnableSFX / DisableSFX.</summary>
    public static bool IsSfxEnabled { get; private set; } = true;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        IsSfxEnabled = PlayerPrefs.GetInt(PREFS_KEY, 1) == 1;
    }

    private void Awake()
    {
        sfxSource = GetComponent<AudioSource>();
        sfxEnabled = PlayerPrefs.GetInt(PREFS_KEY, 1) == 1;
        IsSfxEnabled = sfxEnabled;
        if (sfxSource != null)
            sfxSource.mute = !sfxEnabled;
    }

    public void EnableSFX()
    {
        sfxEnabled = true;
        IsSfxEnabled = true;
        PlayerPrefs.SetInt(PREFS_KEY, 1);
        PlayerPrefs.Save();
        if (sfxSource != null)
            sfxSource.mute = false;
    }

    public void DisableSFX()
    {
        sfxEnabled = false;
        IsSfxEnabled = false;
        PlayerPrefs.SetInt(PREFS_KEY, 0);
        PlayerPrefs.Save();
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
