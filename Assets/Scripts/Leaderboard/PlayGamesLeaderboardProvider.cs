#if UNITY_ANDROID && !UNITY_EDITOR
using System;
using System.Collections.Generic;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace Leaderboard
{
    /// <summary>Leaderboards on Google Play Games Services.</summary>
    public class PlayGamesLeaderboardProvider : ILeaderboardProvider
    {
        public PlayGamesLeaderboardProvider()
        {
            PlayGamesPlatform.Activate();
        }

        public bool IsSignedIn => PlayGamesPlatform.Instance.IsAuthenticated();

        public void SignIn(bool interactive, Action<bool> done)
        {
            Action<SignInStatus> finished = status => done(status == SignInStatus.Success);
            if (interactive) PlayGamesPlatform.Instance.ManuallyAuthenticate(finished);
            else PlayGamesPlatform.Instance.Authenticate(finished);
        }

        public void Submit(LeaderboardBoard board, long score)
        {
            PlayGamesPlatform.Instance.ReportScore(score, BoardId(board), success =>
            {
                if (!success) Debug.LogWarning("[Leaderboard] Score was not accepted.");
            });
        }

        public void LoadTop(LeaderboardBoard board, int count, Action<List<LeaderboardRow>> done)
        {
            PlayGamesPlatform.Instance.LoadScores(BoardId(board), LeaderboardStart.TopScores, count,
                LeaderboardCollection.Public, LeaderboardTimeSpan.AllTime, data =>
                {
                    if (!data.Valid)
                    {
                        done(null);
                        return;
                    }
                    LoadNames(data, done);
                });
        }

        private static void LoadNames(LeaderboardScoreData data, Action<List<LeaderboardRow>> done)
        {
            List<IScore> scores = new List<IScore>(data.Scores);
            string playerId = PlayGamesPlatform.Instance.GetUserId();
            if (data.PlayerScore != null && !scores.Exists(score => score.userID == playerId))
                scores.Add(data.PlayerScore);

            string[] ids = new string[scores.Count];
            for (int i = 0; i < ids.Length; i++) ids[i] = scores[i].userID;

            PlayGamesPlatform.Instance.LoadUsers(ids, users =>
            {
                Dictionary<string, string> names = new Dictionary<string, string>();
                foreach (IUserProfile user in users) names[user.id] = user.userName;

                List<LeaderboardRow> rows = new List<LeaderboardRow>();
                foreach (IScore score in scores)
                {
                    names.TryGetValue(score.userID, out string name);
                    rows.Add(new LeaderboardRow
                    {
                        Rank = score.rank,
                        Name = name ?? string.Empty,
                        Score = score.value,
                        IsPlayer = score.userID == playerId,
                    });
                }
                done(rows);
            });
        }

        private static string BoardId(LeaderboardBoard board)
        {
            return board == LeaderboardBoard.Extended ? GPGSIds.leaderboard_extended : GPGSIds.leaderboard_classic;
        }
    }
}
#endif
