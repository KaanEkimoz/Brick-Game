using Extras;

namespace Leaderboard
{
    /// <summary>
    /// Keeps its own count of the current run so an edited score cannot reach the leaderboard.
    /// A run that fails a check stays playable and simply is not submitted.
    /// </summary>
    public static class RunLedger
    {
        private const long MaxGainPerLevel = 1200; // a four-line clear; a full-board ability pays less
        private const long MaxPointsPerLineAndLevel = 300; // a four-line clear shared by its lines
        private const float MinSecondsPerLine = 0.1f;

        private static long _total;

        public static float Seconds { get; private set; }
        public static bool Ranked { get; private set; } = true;

        public static void Record(int points, int newScore)
        {
            if (newScore == points)
            {
                _total = 0;
                Seconds = 0f;
                Ranked = true;
            }

            _total += points;
            if (_total != newScore || points > MaxGainPerLevel * (LevelController.CurrentLevel + 1))
                Ranked = false;
        }

        public static void Restore(int score, float seconds, bool ranked)
        {
            _total = score;
            Seconds = seconds;
            Ranked = ranked;
        }

        public static void Tick(float deltaTime)
        {
            Seconds += deltaTime;
        }

        /// <summary>True when the finished run's numbers could come from real play.</summary>
        public static bool CanSubmit(int score, int clearedLines, bool hasAbilities)
        {
            if (!Ranked || score <= 0 || score != _total) return false;
            if (clearedLines * MinSecondsPerLine > Seconds) return false;
            if (hasAbilities) return true;
            return score <= clearedLines * MaxPointsPerLineAndLevel * (LevelController.CurrentLevel + 1);
        }
    }
}
