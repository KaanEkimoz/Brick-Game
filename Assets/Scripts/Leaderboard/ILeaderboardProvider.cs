using System;
using System.Collections.Generic;

namespace Leaderboard
{
    public enum LeaderboardBoard
    {
        Classic,
        Extended,
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
        void Submit(LeaderboardBoard board, long score);
        void LoadTop(LeaderboardBoard board, int count, Action<List<LeaderboardRow>> done);
    }
}
