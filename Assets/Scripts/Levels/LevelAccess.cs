using Ekimoz.IAP;
using UnityEngine;

namespace Levels
{
    /// <summary>The first levels are free and the rest need Premium. Players who were already
    /// past the free levels keep what they had unlocked when this rule arrived.</summary>
    public static class LevelAccess
    {
        public const int FreeLevels = 15;

        private const string FreeUntilKey = "levels_free_until";

        public static bool CanPlay(int id)
        {
            if (id <= FreeLevels) return true;
            if (PlayerPrefs.GetInt(PurchaseManager.PremiumPrefKey) == 1) return true;
            return id <= FreeUntil();
        }

        private static int FreeUntil()
        {
            if (!PlayerPrefs.HasKey(FreeUntilKey))
            {
                PlayerPrefs.SetInt(FreeUntilKey, Mathf.Max(FreeLevels, LevelProgress.Unlocked));
                PlayerPrefs.Save();
            }
            return PlayerPrefs.GetInt(FreeUntilKey);
        }
    }
}
