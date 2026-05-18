using System.IO;
using Gameplay;
using UnityEngine;

namespace Persistence
{
    /// <summary>
    /// JSON file I/O for <see cref="GameSaveData"/>.
    /// One slot PER GAME MODE — Classic and Extended each get their own file at
    /// <c>Application.persistentDataPath/brickgame_save_(classic|extended).json</c>.
    /// A run paused in Classic no longer shows the Continue prompt when the player
    /// taps Extended (and vice versa); each mode is genuinely fresh until its own
    /// file lands on disk.
    ///
    /// The legacy single-slot file <c>brickgame_save.json</c> is deleted on the
    /// first call after upgrade so users who shipped on earlier versions don't
    /// see a stale Continue prompt on a mode they never touched.
    /// </summary>
    public static class GameSaveStorage
    {
        private const string LegacyFileName = "brickgame_save.json";
        private const string ClassicFileName = "brickgame_save_classic.json";
        private const string ExtendedFileName = "brickgame_save_extended.json";

        private static bool _legacyChecked;

        private static string FileName => GameMode.IsExtended ? ExtendedFileName : ClassicFileName;

        private static string FilePath
        {
            get
            {
                DropLegacyFileOnce();
                return Path.Combine(Application.persistentDataPath, FileName);
            }
        }

        /// <summary>
        /// One-time cleanup of the pre-v1.2.8 monolithic save. The old single
        /// slot could not tell Classic and Extended apart, so we drop it outright
        /// the first time any storage call runs. The user only loses an unfinished
        /// run from before the upgrade — Game Over had already cleared it anyway,
        /// so in practice almost no one will lose anything.
        /// </summary>
        private static void DropLegacyFileOnce()
        {
            if (_legacyChecked) return;
            _legacyChecked = true;

            string legacy = Path.Combine(Application.persistentDataPath, LegacyFileName);
            if (!File.Exists(legacy)) return;

            try { File.Delete(legacy); }
            catch (IOException e) { Debug.LogWarning($"GameSaveStorage: legacy save delete failed. {e.Message}"); }
        }

        public static bool HasSave() => File.Exists(FilePath);

        public static void Save(GameSaveData data)
        {
            try
            {
                string json = JsonUtility.ToJson(data);
                File.WriteAllText(FilePath, json);
            }
            catch (IOException e)
            {
                Debug.LogError($"GameSaveStorage: failed to write save file. {e.Message}");
            }
        }

        public static GameSaveData Load()
        {
            if (!File.Exists(FilePath))
                return null;

            try
            {
                string json = File.ReadAllText(FilePath);
                return JsonUtility.FromJson<GameSaveData>(json);
            }
            catch (IOException e)
            {
                Debug.LogError($"GameSaveStorage: failed to read save file. {e.Message}");
                return null;
            }
        }

        public static void Delete()
        {
            if (!File.Exists(FilePath))
                return;

            try
            {
                File.Delete(FilePath);
            }
            catch (IOException e)
            {
                Debug.LogError($"GameSaveStorage: failed to delete save file. {e.Message}");
            }
        }
    }
}
