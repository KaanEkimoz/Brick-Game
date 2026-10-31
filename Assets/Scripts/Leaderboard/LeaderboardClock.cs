using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.Networking;

namespace Leaderboard
{
    /// <summary>Server time for the monthly board, so a changed device clock cannot post a score
    /// into a future month.</summary>
    public static class LeaderboardClock
    {
        private const string TimeUrl = "https://www.google.com/generate_204";
        private const string DateHeader = "Date";

        private static bool _hasTime;
        private static DateTime _serverUtc;
        private static float _receivedAt;

        public static void Fetch(Action<DateTime?> done)
        {
            if (_hasTime)
            {
                done(Now());
                return;
            }

            UnityWebRequest request = UnityWebRequest.Head(TimeUrl);
            request.SendWebRequest().completed += operation =>
            {
                string date = request.GetResponseHeader(DateHeader);
                request.Dispose();
                if (DateTime.TryParse(date, CultureInfo.InvariantCulture,
                        DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out DateTime utc))
                {
                    _serverUtc = utc;
                    _receivedAt = Time.realtimeSinceStartup;
                    _hasTime = true;
                    done(Now());
                    return;
                }
                done(null);
            };
        }

        private static DateTime Now()
        {
            return _serverUtc.AddSeconds(Time.realtimeSinceStartup - _receivedAt);
        }
    }
}
