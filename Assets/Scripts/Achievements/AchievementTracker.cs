using Board;
using Extras;
using Gameplay;
using InGame;
using Leaderboard;
using Levels;
using Persistence;
using UnityEngine;

namespace Achievements
{
    /// <summary>Turns gameplay events into achievements.</summary>
    public class AchievementTracker : MonoBehaviour
    {
        private const int FirstSpeedGoal = 5;
        private const int LastSpeedGoal = 15;
        private const int FirstScoreGoal = 10000;
        private const int LastScoreGoal = 50000;

        private void Start()
        {
            AwardEarlierProgress();
            AchievementService.SendPending();
        }

        private void OnEnable()
        {
            BoardController.OnLinesCleared += HandleLinesCleared;
            BoardController.OnTetrisCleared += HandleTetris;
            LevelController.OnLevelIncreased += HandleSpeedChanged;
            ScoreController.OnScoreAdded += HandleScoreAdded;
            AbilityExecutor.OnAbilityUsed += HandleAbilityUsed;
            BonusCollectibles.OnFruitCollected += HandleFruitCollected;
            LevelSession.OnLevelWon += HandleLevelWon;
            PiecesController.OnGameOver += HandleSyncMoment;
            LeaderboardService.OnSignedIn += HandleSyncMoment;
        }

        private void OnDisable()
        {
            BoardController.OnLinesCleared -= HandleLinesCleared;
            BoardController.OnTetrisCleared -= HandleTetris;
            LevelController.OnLevelIncreased -= HandleSpeedChanged;
            ScoreController.OnScoreAdded -= HandleScoreAdded;
            AbilityExecutor.OnAbilityUsed -= HandleAbilityUsed;
            BonusCollectibles.OnFruitCollected -= HandleFruitCollected;
            LevelSession.OnLevelWon -= HandleLevelWon;
            PiecesController.OnGameOver -= HandleSyncMoment;
            LeaderboardService.OnSignedIn -= HandleSyncMoment;
        }

        // Players who reached these goals before achievements existed still receive them.
        private static void AwardEarlierProgress()
        {
            AwardScore(Mathf.Max(ScoreController.ClassicHighScore, ScoreController.ExtendedHighScore));
            AwardLevel(LevelProgress.Unlocked - 1);
        }

        private static void HandleLinesCleared(int lines)
        {
            if (lines <= 0) return;
            AchievementService.Unlock(GPGSIds.achievement_first_line);
            AchievementService.AddLines(lines);
        }

        private static void HandleTetris()
        {
            AchievementService.Unlock(GPGSIds.achievement_four_at_once);
        }

        private static void HandleSpeedChanged()
        {
            if (GameMode.IsLevels) return;
            if (LevelController.CurrentLevel >= FirstSpeedGoal) AchievementService.Unlock(GPGSIds.achievement_picking_up_speed);
            if (LevelController.CurrentLevel >= LastSpeedGoal) AchievementService.Unlock(GPGSIds.achievement_top_speed);
        }

        private static void HandleScoreAdded(int points, int score)
        {
            if (!GameMode.IsLevels) AwardScore(score);
        }

        private static void AwardScore(int score)
        {
            if (score >= FirstScoreGoal) AchievementService.Unlock(GPGSIds.achievement_five_digits);
            if (score >= LastScoreGoal) AchievementService.Unlock(GPGSIds.achievement_high_roller);
        }

        private static void HandleAbilityUsed(AbilityType ability)
        {
            if (ability == AbilityType.Bomb) AchievementService.Unlock(GPGSIds.achievement_big_bang);
            else if (ability == AbilityType.Dynamite) AchievementService.Unlock(GPGSIds.achievement_demolition_crew);
        }

        private static void HandleFruitCollected()
        {
            AchievementService.Unlock(GPGSIds.achievement_fruit_picker);
        }

        private static void HandleLevelWon(LevelDefinition level)
        {
            AwardLevel(level.id);
            CloudProgress.Sync();
        }

        private static void HandleSyncMoment()
        {
            AchievementService.SendPending();
            CloudProgress.Sync();
        }

        private static void AwardLevel(int level)
        {
            if (level >= 10) AchievementService.Unlock(GPGSIds.achievement_warmed_up);
            if (level >= 25) AchievementService.Unlock(GPGSIds.achievement_getting_serious);
            if (level >= 50) AchievementService.Unlock(GPGSIds.achievement_halfway_there);
            if (level >= 100) AchievementService.Unlock(GPGSIds.achievement_level_master);
        }
    }
}
