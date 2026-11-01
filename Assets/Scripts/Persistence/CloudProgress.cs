using System;
using UnityEngine;
#if UNITY_ANDROID && !UNITY_EDITOR
using System.Text;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;
using Leaderboard;
#endif

namespace Persistence
{
    /// <summary>Keeps long-term progress in Play Games Saved Games. These values only ever grow,
    /// so a conflict between devices is settled by keeping the higher value of each one.
    /// A run in progress stays on its own device.</summary>
    public static class CloudProgress
    {
        /// <summary>Fires after a sync changed this device's saved progress.</summary>
        public static Action OnLocalChanged;

        // New keys go at the end, so older cloud files keep their meaning.
        private static readonly string[] Keys =
        {
            "levels_unlocked",
            "highScore",
            "highScore_Extended",
            "highScore_Levels",
            "achievement_lines",
            "tutorial_classic_seen_v1",
            "tutorial_extended_seen_v1",
            "tutorial_levels_seen_v1",
        };

        [Serializable]
        private class CloudFile
        {
            public int[] values = Array.Empty<int>();
        }

        private static int[] Merge(int[] cloud, out bool localChanged, out bool cloudChanged)
        {
            localChanged = false;
            cloudChanged = false;
            int[] merged = new int[Keys.Length];
            for (int i = 0; i < Keys.Length; i++)
            {
                int local = PlayerPrefs.GetInt(Keys[i]);
                int remote = i < cloud.Length ? cloud[i] : 0;
                merged[i] = Mathf.Max(local, remote);
                if (merged[i] != local)
                {
                    PlayerPrefs.SetInt(Keys[i], merged[i]);
                    localChanged = true;
                }
                if (merged[i] != remote) cloudChanged = true;
            }
            return merged;
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private const string FileName = "progress";

        private static bool _syncing;

        /// <summary>Reads the cloud file, merges it with this device and writes back any change.</summary>
        public static void Sync()
        {
            if (_syncing || !LeaderboardService.IsSignedIn) return;
            _syncing = true;

            PlayGamesPlatform.Instance.SavedGame.OpenWithAutomaticConflictResolution(FileName,
                DataSource.ReadCacheOrNetwork, ConflictResolutionStrategy.UseLongestPlaytime, (status, file) =>
                {
                    if (status != SavedGameRequestStatus.Success)
                    {
                        Finish(status);
                        return;
                    }
                    PlayGamesPlatform.Instance.SavedGame.ReadBinaryData(file, (readStatus, data) =>
                    {
                        if (readStatus == SavedGameRequestStatus.Success) Store(file, data);
                        else Finish(readStatus);
                    });
                });
        }

        private static void Store(ISavedGameMetadata file, byte[] data)
        {
            int[] merged = Merge(Parse(data), out bool localChanged, out bool cloudChanged);
            if (localChanged)
            {
                PlayerPrefs.Save();
                OnLocalChanged?.Invoke();
            }
            if (!cloudChanged)
            {
                _syncing = false;
                return;
            }

            byte[] updated = Encoding.UTF8.GetBytes(JsonUtility.ToJson(new CloudFile { values = merged }));
            SavedGameMetadataUpdate update = new SavedGameMetadataUpdate.Builder()
                .WithUpdatedDescription("Brick Game")
                .Build();
            PlayGamesPlatform.Instance.SavedGame.CommitUpdate(file, update, updated, (status, _) => Finish(status));
        }

        private static int[] Parse(byte[] data)
        {
            if (data == null || data.Length == 0) return Array.Empty<int>();
            try
            {
                CloudFile file = JsonUtility.FromJson<CloudFile>(Encoding.UTF8.GetString(data));
                return file?.values ?? Array.Empty<int>();
            }
            catch (ArgumentException)
            {
                return Array.Empty<int>();
            }
        }

        private static void Finish(SavedGameRequestStatus status)
        {
            _syncing = false;
            if (status != SavedGameRequestStatus.Success) Debug.LogWarning("[CloudProgress] " + status);
        }
#else
        public static void Sync() { }

        /// <summary>Editor-only way to exercise the merge rule.</summary>
        public static int[] DebugMerge(int[] cloud) => Merge(cloud, out _, out _);
#endif
    }
}
