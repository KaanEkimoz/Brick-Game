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
        
            CurrentLevel = 1 + totalClearedLines / 10;
            OnLevelIncreased.Invoke();
        
            if (CurrentLevel >= maxLevel)
                CurrentLevel = maxLevel;
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
        }

        /// <summary>Restores the level from a save file. Fires OnLevelIncreased so drop-time and UI recalc.</summary>
        public void LoadLevel(int value)
        {
            CurrentLevel = Mathf.Clamp(value, 1, maxLevel);
            UpdateLevelText();
            OnLevelIncreased?.Invoke();
        }
    }
}
