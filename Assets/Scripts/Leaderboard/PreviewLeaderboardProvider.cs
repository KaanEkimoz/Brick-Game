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

        public void Submit(LeaderboardBoard board, long score)
        {
            UnityEngine.Debug.Log($"[Leaderboard] Preview submit: {board} {score}");
        }

        public void LoadTop(LeaderboardBoard board, int count, Action<List<LeaderboardRow>> done)
        {
            List<LeaderboardRow> rows = new List<LeaderboardRow>();
            for (int i = 0; i < count; i++)
                rows.Add(new LeaderboardRow { Rank = i + 1, Name = $"Player {i + 1}", Score = 120000 - i * 9500, IsPlayer = i == 3 });
            done(rows);
        }
    }
}
