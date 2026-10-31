using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Persistence
{
    /// <summary>Marks a save as written by the game, so a run edited on disk cannot be ranked.
    /// The key ships with the game, so this stops file edits rather than a determined attacker.</summary>
    public static class SaveSignature
    {
        private static readonly byte[] Key = Encoding.UTF8.GetBytes("brick-" + "run-" + "ledger-" + "2c91f7");

        public static string Sign(GameSaveData data)
        {
            string run = string.Join("|",
                data.score,
                data.totalClearedLines,
                data.currentLevel,
                data.runSeconds.ToString("R", CultureInfo.InvariantCulture),
                data.ranked);
            using (HMACSHA256 hmac = new HMACSHA256(Key))
                return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(run)));
        }

        public static bool IsValid(GameSaveData data)
        {
            return !string.IsNullOrEmpty(data.signature) && data.signature == Sign(data);
        }
    }
}
