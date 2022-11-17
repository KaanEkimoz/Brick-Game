using System;
using TMPro;
using UnityEngine;

public class LevelController : MonoBehaviour
{
    public TextMeshProUGUI levelText;
    public int maxLevel = 15;
    public static int CurrentLevel = 1;

    public static Action OnLevelIncreased;
    private void OnEnable()
    {
        BoardController.TotalLineClearedLinesChanged += CalculateLevel;
    }
    private void OnDisable()
    {
        BoardController.TotalLineClearedLinesChanged -= CalculateLevel;
    }
    private void Start()
    {
        CurrentLevel = 1;
        UpdateLevelText();
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
        levelText.text = "Level " + CurrentLevel;
    }
}
