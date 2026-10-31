using Scene;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//Pauses every object that is time-based
public class PauseTheGame : MonoBehaviour
{
    private const string RestartButtonPath = "PauseMenu/PausePanel/B_Restart";
    private const string PauseMainMenuButtonPath = "PauseMenu/PausePanel/B_MainMenu";
    private const string GameOverMainMenuButtonPath = "GameOverMenu/GOM_Panel/B_MainMenu";
    private const string ResumeButtonPath = "PauseMenu/PausePanel/B_Resume";
    private const string PauseButtonName = "B_Pause";

    /// <summary>True from the pause press until play restarts, including the resume countdown.
    /// Touch input runs on unscaled time, so gameplay input checks this instead of timeScale.</summary>
    public static bool IsPaused { get; private set; }

    private ResumeCountdown _countdown;
    private Button _pauseButton;

    private void Awake()
    {
        IsPaused = false;

        Transform canvas = transform.parent;
        Button restartButton = canvas.Find(RestartButtonPath).GetComponent<Button>();
        Button resumeButton = canvas.Find(ResumeButtonPath).GetComponent<Button>();
        foreach (Button button in canvas.GetComponentsInChildren<Button>(true))
            if (button.name == PauseButtonName) _pauseButton = button;

        _countdown = gameObject.AddComponent<ResumeCountdown>();
        _countdown.Build(canvas, resumeButton.GetComponentInChildren<TMP_Text>(true).font);

        gameObject.AddComponent<ConfirmDialog>()
            .Build(restartButton, resumeButton, "Restart?", "Your current game will be lost", ConfirmDialog.ConfirmText);
        GuardMainMenu(canvas.Find(PauseMainMenuButtonPath), resumeButton);
        GuardMainMenu(canvas.Find(GameOverMainMenuButtonPath), resumeButton);
    }

    private void GuardMainMenu(Transform mainMenuButton, Button cancelTemplate)
    {
        gameObject.AddComponent<ConfirmDialog>().Build(mainMenuButton.GetComponent<Button>(), cancelTemplate,
            ConfirmDialog.MainMenuTitle, ConfirmDialog.MainMenuMessage, ConfirmDialog.ConfirmText);
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

    // Leaving the app mid-run opens the pause menu, so play waits for the resume countdown.
    private void OnApplicationPause(bool paused)
    {
        if (!paused) return;

        PlayerPrefs.Save();
        InGame.PiecesController pieces = InGame.PiecesController.Instance;
        if (IsPaused || pieces == null || !pieces.IsRunActive || Levels.LevelSession.RoundOver) return;
        if (Tutorial.TutorialController.IsActive || !_pauseButton.gameObject.activeInHierarchy) return;
        _pauseButton.onClick.Invoke();
    }

    // Returning from the background must not thaw a paused game.
    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus && IsPaused) Time.timeScale = 0.0f;
    }
}
