using System;
using System.Collections.Generic;
using Gameplay;

namespace Leaderboard
{
    /// <summary>Single entry point for the online leaderboards.</summary>
    public static class LeaderboardService
    {
        private const int MonthlyLoadCount = 25; // room for scores from a device with a wrong date

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

        public static void Submit(LeaderboardBoard board, long score, Action done)
        {
            if (!Provider.IsSignedIn)
            {
                done();
                return;
            }

            LeaderboardClock.Fetch(utcNow =>
            {
                long monthlyScore = utcNow.HasValue ? LeaderboardSchedule.StampMonth(score, utcNow.Value) : -1;
                Provider.Submit(board, score, monthlyScore, done);
            });
        }

        /// <summary>Loads the top scores, asking the player to sign in first when needed.</summary>
        public static void LoadTop(LeaderboardBoard board, LeaderboardPeriod period, int count, Action<List<LeaderboardRow>> done)
        {
            if (Provider.IsSignedIn)
            {
                LoadSignedIn(board, period, count, done);
                return;
            }

            Provider.SignIn(true, signedIn =>
            {
                if (signedIn) LoadSignedIn(board, period, count, done);
                else done(null);
            });
        }

        /// <summary>The player's all-time rank on a board, or nothing when it is unknown.</summary>
        public static void LoadPlayerRank(LeaderboardBoard board, Action<long?> done)
        {
            if (!Provider.IsSignedIn)
            {
                done(null);
                return;
            }

            Provider.LoadTop(board, LeaderboardPeriod.AllTime, 1, rows =>
            {
                LeaderboardRow player = rows == null ? default : rows.Find(row => row.IsPlayer);
                done(player.IsPlayer ? player.Rank : (long?)null);
            });
        }

        private static void LoadSignedIn(LeaderboardBoard board, LeaderboardPeriod period, int count, Action<List<LeaderboardRow>> done)
        {
            if (period != LeaderboardPeriod.Monthly)
            {
                Provider.LoadTop(board, period, count, done);
                return;
            }

            LeaderboardClock.Fetch(utcNow =>
            {
                if (!utcNow.HasValue)
                {
                    done(null);
                    return;
                }
                Provider.LoadTop(board, period, MonthlyLoadCount, rows =>
                    done(LeaderboardSchedule.ReadMonth(rows, utcNow.Value, count)));
            });
        }
    }
}
