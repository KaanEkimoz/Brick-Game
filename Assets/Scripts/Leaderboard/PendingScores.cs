using System;
using System.Globalization;
using Persistence;
using UnityEngine;

namespace Leaderboard
{
    /// <summary>Keeps each board's best score until the service has accepted it, so a run
    /// finished offline or signed out still reaches the leaderboard. Signed, because the
    /// stored value can be edited on the device.</summary>
    public static class PendingScores
    {
        private const string KeyPrefix = "leaderboard_pending_";
        private const char Separator = '|';

        public static void Add(LeaderboardBoard board, long score, DateTime utcNow)
        {
            if (TryGet(board, out long pending, out DateTime _) && pending >= score) return;

            string run = string.Join(Separator.ToString(), score, utcNow.Ticks);
            PlayerPrefs.SetString(Key(board), run + Separator + SaveSignature.Hash(Tagged(board, run)));
            PlayerPrefs.Save();
        }

        public static bool TryGet(LeaderboardBoard board, out long score, out DateTime earnedUtc)
        {
            score = 0;
            earnedUtc = default;

            string[] parts = PlayerPrefs.GetString(Key(board)).Split(Separator);
            if (parts.Length != 3) return false;
            if (parts[2] != SaveSignature.Hash(Tagged(board, parts[0] + Separator + parts[1]))) return false;
            if (!long.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out score)) return false;
            if (!long.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out long ticks)) return false;

            earnedUtc = new DateTime(ticks, DateTimeKind.Utc);
            return true;
        }

        /// <summary>Clears the score once sent, unless a better one arrived meanwhile.</summary>
        public static void Clear(LeaderboardBoard board, long sentScore)
        {
            if (TryGet(board, out long pending, out DateTime _) && pending > sentScore) return;

            PlayerPrefs.DeleteKey(Key(board));
            PlayerPrefs.Save();
        }

        private static string Key(LeaderboardBoard board) => KeyPrefix + board;

        private static string Tagged(LeaderboardBoard board, string run) => board + run;
    }
}
