using Scene;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//Pauses every object that is time-based
public class PauseTheGame : MonoBehaviour
{
    private const string PauseMenuPath = "PauseMenu";
    private const string RestartButtonPath = "PauseMenu/PausePanel/B_Restart";
    private const string ResumeButtonPath = "PauseMenu/PausePanel/B_Resume";

    /// <summary>True from the pause press until play restarts, including the resume countdown.
    /// Touch input runs on unscaled time, so gameplay input checks this instead of timeScale.</summary>
    public static bool IsPaused { get; private set; }

    private ResumeCountdown _countdown;

    private void Awake()
    {
        IsPaused = false;

        Transform canvas = transform.parent;
        Button restartButton = canvas.Find(RestartButtonPath).GetComponent<Button>();
        Button resumeButton = canvas.Find(ResumeButtonPath).GetComponent<Button>();

        _countdown = gameObject.AddComponent<ResumeCountdown>();
        _countdown.Build(canvas, resumeButton.GetComponentInChildren<TMP_Text>(true).font);

        gameObject.AddComponent<RestartConfirmDialog>()
            .Build(canvas.Find(PauseMenuPath), restartButton, resumeButton);
    }

    public void PauseGame()
    {
        _countdown.Cancel();
        IsPaused = true;
        Time.timeScale = 0.0f;
    }
    public void ResumeGame()
    {
        _countdown.Cancel();
        IsPaused = false;
        Time.timeScale = 1.0f;
    }

    /// <summary>Hook for the Resume button: keeps the game frozen through a 3-2-1 count.</summary>
    public void ResumeWithCountdown()
    {
        _countdown.Play(ResumeGame);
    }

    // Returning from the background must not thaw a paused game.
    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus && IsPaused) Time.timeScale = 0.0f;
    }
}
