using System;
using System.Collections.Generic;

namespace Leaderboard
{
    /// <summary>Stands in where Play Games does not exist, such as the Editor.</summary>
    public class PreviewLeaderboardProvider : ILeaderboardProvider
    {
        private const int PlayerIndex = 3;

        public bool IsSignedIn => true;

        public void SignIn(bool interactive, Action<bool> done)
        {
            done(true);
        }

        public void Submit(LeaderboardBoard board, long score, long monthlyScore, Action<bool> done)
        {
            UnityEngine.Debug.Log($"[Leaderboard] Preview submit: {board} {score} {monthlyScore}");
            done(true);
        }

        public void LoadTop(LeaderboardBoard board, LeaderboardPeriod period, int count, Action<List<LeaderboardRow>> done)
        {
            List<LeaderboardRow> rows = new List<LeaderboardRow>();
            for (int i = 0; i < Math.Max(count, PlayerIndex + 1); i++)
            {
                if (i >= count && i != PlayerIndex) continue;

                long score = 120000 - i * 4500;
                if (period == LeaderboardPeriod.Monthly) score = LeaderboardSchedule.StampMonth(score, DateTime.UtcNow);
                rows.Add(new LeaderboardRow { Rank = i + 1, Name = $"Player {i + 1}", Score = score, IsPlayer = i == PlayerIndex });
            }
            done(rows);
        }
    }
}
