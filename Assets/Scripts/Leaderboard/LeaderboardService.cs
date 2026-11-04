using System;
using System.Collections.Generic;
using Gameplay;

namespace Leaderboard
{
    /// <summary>Single entry point for the online leaderboards.</summary>
    public static class LeaderboardService
    {
        private const int MonthlyLoadCount = 25; // room for scores from a device with a wrong date

        /// <summary>Fires each time a sign-in succeeds.</summary>
        public static Action OnSignedIn;

        private static ILeaderboardProvider _provider;
        private static bool _signInDeclined;

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

        public static bool IsSignedIn => Provider.IsSignedIn;

        public static LeaderboardBoard CurrentBoard => GameMode.IsExtended ? LeaderboardBoard.Extended : LeaderboardBoard.Classic;

        /// <summary>Signs in without any prompt, so a player who declined is not asked again.</summary>
        public static void SignInSilently()
        {
            Provider.SignIn(false, signedIn =>
            {
                if (signedIn) HandleSignedIn();
            });
        }

        /// <summary>Shows the sign-in prompt, unless the player is already signed in.</summary>
        public static void SignIn(Action<bool> done)
        {
            if (Provider.IsSignedIn)
            {
                done(true);
                return;
            }

            Provider.SignIn(true, signedIn =>
            {
                if (signedIn) HandleSignedIn();
                done(signedIn);
            });
        }

        private static void HandleSignedIn()
        {
            SendAllPending();
            OnSignedIn?.Invoke();
        }

        /// <summary>Lets the next load ask for sign-in again. Called when the player opens the
        /// board, so switching tabs does not repeat a prompt they declined.</summary>
        public static void AllowSignInPrompt()
        {
            _signInDeclined = false;
        }

        public static void Submit(LeaderboardBoard board, long score, Action done)
        {
            PendingScores.Add(board, score, DateTime.UtcNow);
            SendPending(board, done);
        }

        private static void SendAllPending()
        {
            foreach (LeaderboardBoard board in Enum.GetValues(typeof(LeaderboardBoard)))
                SendPending(board, () => { });
        }

        // Without the server's date the score stays queued for the next attempt.
        private static void SendPending(LeaderboardBoard board, Action done)
        {
            if (!Provider.IsSignedIn || !PendingScores.TryGet(board, out long score, out DateTime earned))
            {
                if (!Provider.IsSignedIn) UnityEngine.Debug.Log($"[Leaderboard] Not signed in, {board} score kept for later.");
                done();
                return;
            }

            LeaderboardClock.Fetch(utcNow =>
            {
                if (!utcNow.HasValue)
                {
                    UnityEngine.Debug.LogWarning("[Leaderboard] No server time, score kept for later.");
                    done();
                    return;
                }

                // A score earned in an earlier month no longer belongs on the monthly board.
                bool sameMonth = LeaderboardSchedule.SameMonth(earned, utcNow.Value);
                long monthlyScore = sameMonth ? LeaderboardSchedule.StampMonth(score, utcNow.Value) : -1;
                Provider.Submit(board, score, monthlyScore, accepted =>
                {
                    UnityEngine.Debug.Log($"[Leaderboard] {board} {score} accepted={accepted}");
                    if (accepted) PendingScores.Clear(board, score);
                    done();
                });
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

            if (_signInDeclined)
            {
                done(null);
                return;
            }

            Provider.SignIn(true, signedIn =>
            {
                _signInDeclined = !signedIn;
                if (signedIn)
                {
                    HandleSignedIn();
                    LoadSignedIn(board, period, count, done);
                }
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
