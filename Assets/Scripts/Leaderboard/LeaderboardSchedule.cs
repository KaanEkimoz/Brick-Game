using System;
using System.Collections.Generic;
using Localization;

namespace Leaderboard
{
    /// <summary>
    /// Reset times. Play Games resets weekly boards at the start of Sunday in UTC-7 all year.
    /// It has no monthly boards, so a monthly score carries its month in the high digits: a new
    /// month outranks every older score, and only the current month's rows are shown.
    /// </summary>
    public static class LeaderboardSchedule
    {
        private const long MonthStep = 10000000000L;
        private const string WeeklyFormat = "dddd HH:mm";
        private const string MonthlyFormat = "d MMMM HH:mm";

        private static readonly TimeSpan ResetOffset = TimeSpan.FromHours(-7);

        public static long StampMonth(long score, DateTime utcNow)
        {
            return MonthKey(utcNow) * MonthStep + score;
        }

        /// <summary>True when both times fall in the same board month, which starts in UTC-7.</summary>
        public static bool SameMonth(DateTime utcA, DateTime utcB)
        {
            return MonthKey(utcA) == MonthKey(utcB);
        }

        /// <summary>Keeps the current month's rows, restored to plain scores.</summary>
        public static List<LeaderboardRow> ReadMonth(List<LeaderboardRow> rows, DateTime utcNow, int count)
        {
            if (rows == null) return null;

            long month = MonthKey(utcNow);
            List<LeaderboardRow> current = new List<LeaderboardRow>();
            foreach (LeaderboardRow row in rows)
            {
                if (row.Score / MonthStep != month) continue;
                if (current.Count >= count && !row.IsPlayer) continue;

                LeaderboardRow plain = row;
                plain.Score = row.Score % MonthStep;
                current.Add(plain);
            }
            return current;
        }

        /// <summary>The next reset in the player's own time, or null for a board that never resets.</summary>
        public static string NextResetText(LeaderboardPeriod period, DateTime utcNow)
        {
            switch (period)
            {
                case LeaderboardPeriod.Weekly:
                    return LocalText(NextWeekly(utcNow), WeeklyFormat);
                case LeaderboardPeriod.Monthly:
                    return LocalText(NextMonthly(utcNow), MonthlyFormat);
                default:
                    return null;
            }
        }

        private static long MonthKey(DateTime utcNow)
        {
            DateTime resetTime = utcNow + ResetOffset;
            return resetTime.Year * 12L + resetTime.Month - 1;
        }

        private static DateTime NextWeekly(DateTime utcNow)
        {
            DateTime resetTime = utcNow + ResetOffset;
            int daysLeft = 7 - (int)resetTime.DayOfWeek;
            return resetTime.Date.AddDays(daysLeft) - ResetOffset;
        }

        private static DateTime NextMonthly(DateTime utcNow)
        {
            DateTime resetTime = utcNow + ResetOffset;
            return new DateTime(resetTime.Year, resetTime.Month, 1).AddMonths(1) - ResetOffset;
        }

        private static string LocalText(DateTime utc, string format)
        {
            return DateTime.SpecifyKind(utc, DateTimeKind.Utc).ToLocalTime().ToString(format, Loc.Culture);
        }
    }
}
