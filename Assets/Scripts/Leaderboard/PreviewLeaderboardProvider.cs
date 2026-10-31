using System;
using System.Collections.Generic;

namespace Leaderboard
{
    /// <summary>Stands in where Play Games does not exist, such as the Editor.</summary>
    public class PreviewLeaderboardProvider : ILeaderboardProvider
    {
        public bool IsSignedIn => true;

        public void SignIn(bool interactive, Action<bool> done)
        {
            done(true);
        }

        public void Submit(LeaderboardBoard board, long score, long monthlyScore, Action done)
        {
            UnityEngine.Debug.Log($"[Leaderboard] Preview submit: {board} {score} {monthlyScore}");
            done();
        }

        public void LoadTop(LeaderboardBoard board, LeaderboardPeriod period, int count, Action<List<LeaderboardRow>> done)
        {
            List<LeaderboardRow> rows = new List<LeaderboardRow>();
            for (int i = 0; i < count; i++)
            {
                long score = 120000 - i * 4500;
                if (period == LeaderboardPeriod.Monthly) score = LeaderboardSchedule.StampMonth(score, DateTime.UtcNow);
                rows.Add(new LeaderboardRow { Rank = i + 1, Name = $"Player {i + 1}", Score = score, IsPlayer = i == 3 });
            }
            done(rows);
        }
    }
}
