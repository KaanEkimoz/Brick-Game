using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Gameplay;

namespace Persistence
{
    /// <summary>Marks a save as written by the game, so a run edited on disk cannot be ranked.
    /// The key ships with the game, so this stops file edits rather than a determined attacker.</summary>
    public static class SaveSignature
    {
        private static readonly byte[] Key = Encoding.UTF8.GetBytes("brick-" + "run-" + "ledger-" + "2c91f7");

        public static string Sign(GameSaveData data)
        {
            StringBuilder run = new StringBuilder();
            run.Append(string.Join("|",
                GameMode.IsExtended,
                data.score,
                data.totalClearedLines,
                data.currentLevel,
                data.runSeconds.ToString("R", CultureInfo.InvariantCulture),
                data.ranked,
                data.currentPieceType,
                data.nextPieceType,
                data.abilityCharge,
                data.currentAbility,
                data.currentAbilityTile,
                string.Join(",", data.bagState)));
            foreach (TileSaveData tile in data.tiles)
                run.Append('|').Append(tile.x).Append(',').Append(tile.y).Append(',').Append(tile.spriteIndex);
            return Hash(run.ToString());
        }

        public static bool IsValid(GameSaveData data)
        {
            return !string.IsNullOrEmpty(data.signature) && data.signature == Sign(data);
        }

        public static string Hash(string text)
        {
            using (HMACSHA256 hmac = new HMACSHA256(Key))
                return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(text)));
        }
    }
}
