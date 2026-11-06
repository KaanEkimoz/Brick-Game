using UnityEngine;

namespace Daily
{
    /// <summary>One finished daily run.</summary>
    public struct DailyResult
    {
        public int Day;
        public int Score;
        public int Lines;
        public int Quads;
        /// <summary>One digit per clear, 1 to 4 lines, in the order they happened.</summary>
        public string Clears;
        /// <summary>False for every run after the day's first.</summary>
        public bool Official;
        public int FreezesUsed;
    }

    /// <summary>How a past day shows in the streak row.</summary>
    public enum DayMark { None, Played, Frozen }

    /// <summary>
    /// The day's official result and the play streak, kept in PlayerPrefs. Only the first run of
    /// a day counts; a missed day is covered by a streak freeze when one is left.
    /// </summary>
    public static class DailyStreak
    {
        public const int MaxFreezes = 2;
        public const int FreezeEveryDays = 7;
        private const int StartingFreezes = 1;

        private const string LastDayKey = "daily_last_day";
        private const string StreakKey = "daily_streak";
        private const string BestStreakKey = "daily_best_streak";
        private const string FreezesKey = "daily_freezes";
        private const string ResultDayKey = "daily_result_day";
        private const string ResultScoreKey = "daily_result_score";
        private const string ResultLinesKey = "daily_result_lines";
        private const string ResultQuadsKey = "daily_result_quads";
        private const string ResultClearsKey = "daily_result_clears";
        private const string StartedDayKey = "daily_started_day";
        private const string LiveScoreKey = "daily_live_score";
        private const string LiveLinesKey = "daily_live_lines";
        private const string LiveQuadsKey = "daily_live_quads";
        private const string LiveClearsKey = "daily_live_clears";
        private const string HistoryKey = "daily_history";
        private const char FrozenSuffix = 'f';
        private const int HistoryLength = 14;

        public static int BestStreak => PlayerPrefs.GetInt(BestStreakKey, 0);
        public static int Freezes => PlayerPrefs.GetInt(FreezesKey, StartingFreezes);

        public static bool HasPlayed(int day) => PlayerPrefs.GetInt(ResultDayKey, 0) == day;

        /// <summary>The streak as it stands today: kept while the gap since the last played
        /// day can still be covered by freezes, otherwise zero.</summary>
        public static int CurrentStreak
        {
            get
            {
                int last = PlayerPrefs.GetInt(LastDayKey, 0);
                if (last <= 0) return 0;
                int missed = DailyChallenge.Today - last - 1;
                return missed <= Freezes ? PlayerPrefs.GetInt(StreakKey, 0) : 0;
            }
        }

        /// <summary>Whether the day was played, covered by a freeze, or neither.</summary>
        public static DayMark MarkOf(int day)
        {
            foreach (string entry in PlayerPrefs.GetString(HistoryKey, "").Split(','))
            {
                bool frozen = entry.Length > 0 && entry[entry.Length - 1] == FrozenSuffix;
                string number = frozen ? entry.Substring(0, entry.Length - 1) : entry;
                if (int.TryParse(number, out int value) && value == day) return frozen ? DayMark.Frozen : DayMark.Played;
            }
            return DayMark.None;
        }

        // Keeps the last two weeks, enough for the seven-day row.
        private static void AddHistory(string entry)
        {
            string history = PlayerPrefs.GetString(HistoryKey, "");
            history = history.Length == 0 ? entry : history + "," + entry;
            string[] entries = history.Split(',');
            if (entries.Length > HistoryLength)
                history = string.Join(",", entries, entries.Length - HistoryLength, HistoryLength);
            PlayerPrefs.SetString(HistoryKey, history);
        }

        /// <summary>The official result for the given day, if it was played.</summary>
        public static bool TryGetResult(int day, out DailyResult result)
        {
            result = default;
            if (!HasPlayed(day)) return false;
            result = new DailyResult
            {
                Day = day,
                Score = PlayerPrefs.GetInt(ResultScoreKey, 0),
                Lines = PlayerPrefs.GetInt(ResultLinesKey, 0),
                Quads = PlayerPrefs.GetInt(ResultQuadsKey, 0),
                Clears = PlayerPrefs.GetString(ResultClearsKey, ""),
                Official = true,
            };
            return true;
        }

        /// <summary>Marks the official run as begun, so quitting midway still uses up the day.</summary>
        public static void MarkStarted(int day)
        {
            PlayerPrefs.SetInt(StartedDayKey, day);
            SaveLive(0, 0, 0, "");
            PlayerPrefs.Save();
        }

        /// <summary>Progress of the running official attempt, read back if the app is killed.</summary>
        public static void SaveLive(int score, int lines, int quads, string clears)
        {
            PlayerPrefs.SetInt(LiveScoreKey, score);
            PlayerPrefs.SetInt(LiveLinesKey, lines);
            PlayerPrefs.SetInt(LiveQuadsKey, quads);
            PlayerPrefs.SetString(LiveClearsKey, clears);
        }

        /// <summary>An official run that never reached its end screen (the app was closed)
        /// counts with the progress it had.</summary>
        public static void FinishAbandoned()
        {
            int started = PlayerPrefs.GetInt(StartedDayKey, 0);
            if (started <= 0) return;
            if (!HasPlayed(started))
            {
                Record(new DailyResult
                {
                    Day = started,
                    Score = PlayerPrefs.GetInt(LiveScoreKey, 0),
                    Lines = PlayerPrefs.GetInt(LiveLinesKey, 0),
                    Quads = PlayerPrefs.GetInt(LiveQuadsKey, 0),
                    Clears = PlayerPrefs.GetString(LiveClearsKey, ""),
                    Official = true,
                });
            }
            PlayerPrefs.DeleteKey(StartedDayKey);
            PlayerPrefs.Save();
        }

        /// <summary>Stores the official result and moves the streak on. Returns the number of
        /// freezes spent on missed days.</summary>
        public static int Record(DailyResult result)
        {
            if (HasPlayed(result.Day)) return 0;

            int last = PlayerPrefs.GetInt(LastDayKey, 0);
            int streak = PlayerPrefs.GetInt(StreakKey, 0);
            int freezes = Freezes;
            int used = 0;

            if (last <= 0) streak = 1;
            else
            {
                int gap = result.Day - last;
                if (gap <= 0) streak = Mathf.Max(1, streak); // the clock went back; keep what was earned
                else if (gap == 1) streak++;
                else
                {
                    int missed = gap - 1;
                    if (missed <= freezes)
                    {
                        freezes -= missed;
                        used = missed;
                        streak++;
                        for (int frozen = last + 1; frozen < result.Day; frozen++) AddHistory(frozen.ToString() + FrozenSuffix);
                    }
                    else streak = 1;
                }
            }

            if (streak % FreezeEveryDays == 0) freezes = Mathf.Min(MaxFreezes, freezes + 1);

            AddHistory(result.Day.ToString());
            PlayerPrefs.SetInt(LastDayKey, Mathf.Max(last, result.Day));
            PlayerPrefs.SetInt(StreakKey, streak);
            PlayerPrefs.SetInt(BestStreakKey, Mathf.Max(BestStreak, streak));
            PlayerPrefs.SetInt(FreezesKey, freezes);
            PlayerPrefs.SetInt(ResultDayKey, result.Day);
            PlayerPrefs.SetInt(ResultScoreKey, result.Score);
            PlayerPrefs.SetInt(ResultLinesKey, result.Lines);
            PlayerPrefs.SetInt(ResultQuadsKey, result.Quads);
            PlayerPrefs.SetString(ResultClearsKey, result.Clears ?? "");
            PlayerPrefs.DeleteKey(StartedDayKey);
            PlayerPrefs.Save();
            return used;
        }
    }
}
