using System;
using Board;
using TMPro;
using UnityEngine;
namespace Extras
{
    public class LevelController : MonoBehaviour
    {
        public TextMeshProUGUI levelText;
        public int maxLevel = 15;
        public static int CurrentLevel = 1;

        public static Action OnLevelIncreased;

        /// <summary>True only while LoadLevel is broadcasting OnLevelIncreased. Lets listeners
        /// distinguish a real level-up (banner-worthy) from a save restore (no banner).</summary>
        public static bool IsRestoring { get; private set; }
        private void OnEnable()
        {
            BoardController.OnTotalClearedLinesChanged += CalculateLevel;
        }
        private void OnDisable()
        {
            BoardController.OnTotalClearedLinesChanged -= CalculateLevel;
        }
        private void Start()
        {
            ResetLevel();
        }
        private void CalculateLevel(int totalClearedLines)
        {
            if(CurrentLevel == maxLevel)
                return;

            // Clamp BEFORE broadcasting. Previously the event fired with the raw (un-clamped)
            // level, so a multi-line clear that pushed the computed level past maxLevel made
            // drop-time listeners speed up beyond the intended cap (and, at extreme line
            // counts, toward a zero/negative wait = unplayable instant fall).
            CurrentLevel = Mathf.Clamp(1 + totalClearedLines / 10, 1, maxLevel);
            OnLevelIncreased?.Invoke();
            UpdateLevelText();
        }
        private void UpdateLevelText()
        {
            levelText.text = "Level\n" + CurrentLevel;
        }
        public void ResetLevel()
        {
            CurrentLevel = 1;
            UpdateLevelText();
            // Without this invoke, PiecesController.UpdateDropTimeAccordingTheLevel
            // never re-runs and the gravity coroutine stays at the previous run's
            // (faster) drop time. Result: HUD reads "Level 1" but pieces still fall
            // at Level 12 speed after a restart.
            OnLevelIncreased?.Invoke();
        }

        /// <summary>Restores the level from a save file. Fires OnLevelIncreased so drop-time and UI recalc.</summary>
        public void LoadLevel(int value)
        {
            CurrentLevel = Mathf.Clamp(value, 1, maxLevel);
            UpdateLevelText();
            IsRestoring = true;
            try { OnLevelIncreased?.Invoke(); }
            finally { IsRestoring = false; }
        }
    }
}
