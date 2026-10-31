using UnityEngine;

namespace Levels
{
    /// <summary>Remembers the highest level the player may enter.</summary>
    public static class LevelProgress
    {
        private const string UnlockedKey = "levels_unlocked";

        public static int Unlocked => Mathf.Max(1, PlayerPrefs.GetInt(UnlockedKey, 1));

        public static void MarkCompleted(int id)
        {
            if (id + 1 <= Unlocked) return;
            PlayerPrefs.SetInt(UnlockedKey, id + 1);
            PlayerPrefs.Save();
        }
    }
}
