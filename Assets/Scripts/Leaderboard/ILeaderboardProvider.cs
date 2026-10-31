using System;
using System.Collections.Generic;

namespace Leaderboard
{
    public enum LeaderboardBoard
    {
        Classic,
        Extended,
    }

    public enum LeaderboardPeriod
    {
        AllTime,
        Monthly,
        Weekly,
    }

    public struct LeaderboardRow
    {
        public long Rank;
        public string Name;
        public long Score;
        public bool IsPlayer;
    }

    /// <summary>The online service behind the leaderboards. <c>rows</c> is null when loading failed.</summary>
    public interface ILeaderboardProvider
    {
        bool IsSignedIn { get; }
        void SignIn(bool interactive, Action<bool> done);

        /// <param name="monthlyScore">The monthly board's stamped score, or negative to skip it.</param>
        /// <param name="done">Receives whether every board accepted the score.</param>
        void Submit(LeaderboardBoard board, long score, long monthlyScore, Action<bool> done);
        void LoadTop(LeaderboardBoard board, LeaderboardPeriod period, int count, Action<List<LeaderboardRow>> done);
    }
}
