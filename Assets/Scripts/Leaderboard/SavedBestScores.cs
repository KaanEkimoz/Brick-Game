using Extras;
using UnityEngine;

namespace Leaderboard
{
    /// <summary>Sends each mode's saved high score to its all-time board, so records set while
    /// the run checks were dropping scores still show up. Its date is unknown, so it skips the
    /// monthly board.</summary>
    public static class SavedBestScores
    {
        private const string SentKeyPrefix = "leaderboard_best_sent_";

        public static void Upload()
        {
            Send(LeaderboardBoard.Classic, ScoreController.ClassicHighScore);
            Send(LeaderboardBoard.Extended, ScoreController.ExtendedHighScore);
        }

        private static void Send(LeaderboardBoard board, int best)
        {
            string key = SentKeyPrefix + board;
            if (best <= 0 || best <= PlayerPrefs.GetInt(key)) return;

            LeaderboardService.SubmitAllTime(board, best, accepted =>
            {
                Debug.Log($"[Leaderboard] Saved best {board} {best} accepted={accepted}");
                if (!accepted) return;
                PlayerPrefs.SetInt(key, best);
                PlayerPrefs.Save();
            });
        }
    }
}
