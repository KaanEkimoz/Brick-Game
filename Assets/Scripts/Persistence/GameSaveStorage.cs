using System.IO;
using UnityEngine;

namespace Persistence
{
    /// <summary>
    /// JSON file I/O for <see cref="GameSaveData"/>.
    /// One slot stored at <c>Application.persistentDataPath/brickgame_save.json</c>.
    /// </summary>
    public static class GameSaveStorage
    {
        private const string FileName = "brickgame_save.json";

        private static string FilePath => Path.Combine(Application.persistentDataPath, FileName);

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
