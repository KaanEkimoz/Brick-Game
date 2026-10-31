using System;
using System.Collections.Generic;
using Gameplay;

namespace Leaderboard
{
    /// <summary>Single entry point for the online leaderboards.</summary>
    public static class LeaderboardService
    {
        private static ILeaderboardProvider _provider;

        private static ILeaderboardProvider Provider
        {
            get
            {
                if (_provider != null) return _provider;
#if UNITY_ANDROID && !UNITY_EDITOR
                _provider = new PlayGamesLeaderboardProvider();
#else
                _provider = new PreviewLeaderboardProvider();
#endif
                return _provider;
            }
        }

        public static LeaderboardBoard CurrentBoard => GameMode.IsExtended ? LeaderboardBoard.Extended : LeaderboardBoard.Classic;

        /// <summary>Signs in without any prompt, so a player who declined is not asked again.</summary>
        public static void SignInSilently()
        {
            Provider.SignIn(false, signedIn => { });
        }

        public static void Submit(LeaderboardBoard board, long score)
        {
            if (Provider.IsSignedIn) Provider.Submit(board, score);
        }

        /// <summary>Loads the top scores, asking the player to sign in first when needed.</summary>
        public static void LoadTop(LeaderboardBoard board, int count, Action<List<LeaderboardRow>> done)
        {
            if (Provider.IsSignedIn)
            {
                Provider.LoadTop(board, count, done);
                return;
            }

            Provider.SignIn(true, signedIn =>
            {
                if (signedIn) Provider.LoadTop(board, count, done);
                else done(null);
            });
        }
    }
}
