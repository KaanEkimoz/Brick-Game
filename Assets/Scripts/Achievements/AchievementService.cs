using Leaderboard;
using UnityEngine;
#if UNITY_ANDROID && !UNITY_EDITOR
using GooglePlayGames;
#endif

namespace Achievements
{
    /// <summary>Unlocks Play Games achievements. Progress is kept on the device first, so a
    /// player who is offline or not signed in receives everything after the next sign-in.</summary>
    public static class AchievementService
    {
        private const string StatePrefix = "achievement_state_";
        private const string LinesKey = "achievement_lines";
        private const string SentLinesKey = "achievement_lines_sent";
        private const int Earned = 1;
        private const int Sent = 2;

        private const int LineWorkerSteps = 100;
        private const int LineLegendSteps = 1000;

        private static readonly string[] Unlockable =
        {
            GPGSIds.achievement_first_line,
            GPGSIds.achievement_four_at_once,
            GPGSIds.achievement_picking_up_speed,
            GPGSIds.achievement_five_digits,
            GPGSIds.achievement_big_bang,
            GPGSIds.achievement_demolition_crew,
            GPGSIds.achievement_fruit_picker,
            GPGSIds.achievement_warmed_up,
            GPGSIds.achievement_getting_serious,
            GPGSIds.achievement_high_roller,
            GPGSIds.achievement_top_speed,
            GPGSIds.achievement_halfway_there,
            GPGSIds.achievement_level_master,
        };

        public static void Unlock(string id)
        {
            string key = StatePrefix + id;
            if (PlayerPrefs.GetInt(key) != 0) return;
            PlayerPrefs.SetInt(key, Earned);
            PlayerPrefs.Save();
            SendAchievement(id);
        }

        public static void AddLines(int lines)
        {
            PlayerPrefs.SetInt(LinesKey, PlayerPrefs.GetInt(LinesKey) + lines);
        }

        /// <summary>Sends everything earned so far that Play Games has not confirmed.</summary>
        public static void SendPending()
        {
            PlayerPrefs.Save();
            foreach (string id in Unlockable)
            {
                if (PlayerPrefs.GetInt(StatePrefix + id) == Earned) SendAchievement(id);
            }
            SendLines();
        }

        public static void Show()
        {
            LeaderboardService.SignIn(signedIn =>
            {
                if (!signedIn) return;
                SendPending();
#if UNITY_ANDROID && !UNITY_EDITOR
                PlayGamesPlatform.Instance.ShowAchievementsUI();
#endif
            });
        }

        private static void SendAchievement(string id)
        {
            if (!LeaderboardService.IsSignedIn) return;
#if UNITY_ANDROID && !UNITY_EDITOR
            PlayGamesPlatform.Instance.UnlockAchievement(id, accepted =>
            {
                if (!accepted) return;
                PlayerPrefs.SetInt(StatePrefix + id, Sent);
                PlayerPrefs.Save();
            });
#endif
        }

        // Progress is sent as a total, so a repeated report never counts lines twice.
        private static void SendLines()
        {
            int lines = PlayerPrefs.GetInt(LinesKey);
            if (!LeaderboardService.IsSignedIn || lines == PlayerPrefs.GetInt(SentLinesKey)) return;
#if UNITY_ANDROID && !UNITY_EDITOR
            ReportLines(GPGSIds.achievement_line_worker, lines, LineWorkerSteps);
            ReportLines(GPGSIds.achievement_line_legend, lines, LineLegendSteps);
            PlayerPrefs.SetInt(SentLinesKey, lines);
            PlayerPrefs.Save();
#endif
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private static void ReportLines(string id, int lines, int steps)
        {
            double percent = Mathf.Min(100f, lines * 100f / steps);
            PlayGamesPlatform.Instance.ReportProgress(id, percent, accepted =>
            {
                // The next report repeats the total, so only the sent marker has to move back.
                if (!accepted) PlayerPrefs.DeleteKey(SentLinesKey);
            });
        }
#endif
    }
}
