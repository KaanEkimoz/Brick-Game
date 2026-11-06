using System;
using System.Collections.Generic;
using Levels;
using UnityEngine;

namespace Daily
{
    /// <summary>
    /// Picks the day's challenge from the pool in Resources/Daily, the same for everyone that
    /// day. The pool is walked in a shuffled order, so no challenge repeats until all were used.
    /// </summary>
    public static class DailyChallenge
    {
        private const string PoolPath = "Daily/daily_challenges";

        // Day 1 of the challenge. Changing it renumbers every day and resets streaks.
        private static readonly DateTime Epoch = new DateTime(2026, 11, 1);

        private static List<LevelDefinition> _pool;

        /// <summary>Today's challenge number, counted on the device's local calendar.</summary>
        public static int Today => DayIndex(DateTime.Now);

        public static int DayIndex(DateTime time) => (int)(time.Date - Epoch).TotalDays + 1;

        public static DateTime DateOf(int day) => Epoch.AddDays(day - 1);

        /// <summary>Spreads neighbouring numbers apart so their orders look unrelated.</summary>
        public static int Seed(int value)
        {
            unchecked
            {
                uint hash = (uint)value * 2654435761u;
                hash ^= hash >> 16;
                hash *= 0x45d9f3bu;
                hash ^= hash >> 16;
                return (int)hash;
            }
        }

        /// <summary>The challenge for a day, as a level with id 0 so it never touches the
        /// Levels progress or the level achievements.</summary>
        public static LevelDefinition ForDay(int day)
        {
            List<LevelDefinition> pool = Pool;
            if (pool.Count == 0) return null;

            int index = Math.Max(0, day - 1);
            int cycle = index / pool.Count;
            int[] order = ShuffledOrder(pool.Count, Seed(cycle + 1));
            LevelDefinition source = pool[order[index % pool.Count]];

            LevelDefinition copy = JsonUtility.FromJson<LevelDefinition>(JsonUtility.ToJson(source));
            copy.id = 0;
            return copy;
        }

        private static List<LevelDefinition> Pool
        {
            get
            {
                if (_pool != null) return _pool;
                TextAsset file = Resources.Load<TextAsset>(PoolPath);
                _pool = file != null ? JsonUtility.FromJson<LevelFile>(file.text).levels : new List<LevelDefinition>();
                if (_pool.Count == 0) Debug.LogError("[Daily] The challenge pool is missing or empty.");
                return _pool;
            }
        }

        // Fisher-Yates with xorshift32, so every device builds the same order.
        private static int[] ShuffledOrder(int count, int seed)
        {
            int[] order = new int[count];
            for (int i = 0; i < count; i++) order[i] = i;
            uint state = seed == 0 ? 0x9E3779B9u : (uint)seed;
            for (int i = count - 1; i > 0; i--)
            {
                state ^= state << 13;
                state ^= state >> 17;
                state ^= state << 5;
                int j = (int)(state % (uint)(i + 1));
                (order[i], order[j]) = (order[j], order[i]);
            }
            return order;
        }
    }
}
