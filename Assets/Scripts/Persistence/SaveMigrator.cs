using UnityEngine;

namespace Persistence
{
    /// <summary>
    /// Forward-only save migration pipeline. When the disk version is older than
    /// <see cref="GameSaveData.CurrentVersion"/>, Migrate walks the data through each
    /// step in order until it reaches the current schema. Each step is intentionally
    /// tiny and additive — fields default to safe values, never throw on missing data.
    ///
    /// Adding a new schema bump: increment GameSaveData.CurrentVersion, add an
    /// "if (data.saveVersion == N)" block here that mutates fields from N to N+1,
    /// then write `data.saveVersion = N+1;`. The chain handles any older save jumping
    /// through every step.
    /// </summary>
    public static class SaveMigrator
    {
        /// <summary>Migrate a freshly-loaded save up to <see cref="GameSaveData.CurrentVersion"/>.
        /// No-op when already current. Returns the same instance for chaining.</summary>
        public static GameSaveData Migrate(GameSaveData data)
        {
            if (data == null) return null;

            int startVersion = data.saveVersion;

            // v1 → v2: bag state introduced. Older saves had no bag; leave it empty so the
            // next PieceBag.Next() call refills + reshuffles. No tile/score loss.
            if (data.saveVersion == 1)
            {
                if (data.bagState == null) data.bagState = new System.Collections.Generic.List<int>();
                data.saveVersion = 2;
            }

            // (Future migrations chain here as additional "if (data.saveVersion == N)" blocks.)

            if (data.saveVersion != GameSaveData.CurrentVersion)
            {
                Debug.LogWarning(
                    $"SaveMigrator: load version {startVersion} did not reach current {GameSaveData.CurrentVersion} " +
                    $"(stopped at {data.saveVersion}). The save may be from a newer build — fields beyond current are ignored.");
            }

            return data;
        }
    }
}
