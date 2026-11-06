using System;
using System.Collections.Generic;
using Levels;

namespace Daily
{
    /// <summary>
    /// The rules of the daily challenge: one piece sequence per calendar day, the same on every
    /// device, played as Classic for a fixed number of pieces.
    /// </summary>
    public static class DailyChallenge
    {
        public const int PieceLimit = 100;
        public const string Objective = "Daily";

        // Day 1 of the challenge. Changing it renumbers every day and resets streaks.
        private static readonly DateTime Epoch = new DateTime(2026, 11, 1);

        /// <summary>Today's challenge number, counted on the device's local calendar.</summary>
        public static int Today => DayIndex(DateTime.Now);

        public static int DayIndex(DateTime time) => (int)(time.Date - Epoch).TotalDays + 1;

        public static DateTime DateOf(int day) => Epoch.AddDays(day - 1);

        /// <summary>Spreads neighbouring days apart so their piece orders look unrelated.</summary>
        public static int Seed(int day)
        {
            unchecked
            {
                uint hash = (uint)day * 2654435761u;
                hash ^= hash >> 16;
                hash *= 0x45d9f3bu;
                hash ^= hash >> 16;
                return (int)hash;
            }
        }

        /// <summary>A Levels definition that never ends by its objective, only by the piece
        /// limit or a top-out, and speeds up like Classic.</summary>
        public static LevelDefinition Definition()
        {
            return new LevelDefinition
            {
                id = 0,
                name = "Daily",
                hint = "",
                objective = Objective,
                target = int.MaxValue,
                rows = new List<string>(),
                pieceLimit = PieceLimit,
                dropLevel = 0,
            };
        }
    }
}
