using System;
using System.Collections.Generic;
using InGame;

namespace Persistence
{
    /// <summary>
    /// Serializable snapshot of the game state captured right after a new piece is spawned.
    /// Board holds only settled tiles at this moment — the active piece has not yet called SetTile.
    ///
    /// Save versions:
    ///   1 — pre-v1.3.0: no bag state (consecutive duplicate pieces possible via raw RNG).
    ///   2 — v1.3.0+: bagState captures the remaining pieces of the active 7-bag so a resumed
    ///                run replays the same draws the player would have seen had they not closed.
    /// SaveMigrator handles v1 → v2 (bagState starts empty, next draw refills the bag).
    /// </summary>
    [Serializable]
    public class GameSaveData
    {
        public const int CurrentVersion = 2;

        public int saveVersion = CurrentVersion;

        public int score;
        public int totalClearedLines;
        public int currentLevel;

        public PieceType currentPieceType;
        public PieceType nextPieceType;

        /// <summary>Remaining pieces in the active 7-bag. Stored as ints (JsonUtility-safe).
        /// Empty list → PieceBag will refill + reshuffle on the next draw, which is the
        /// correct behavior for both fresh saves and migrated v1 saves.</summary>
        public List<int> bagState = new List<int>();

        public List<TileSaveData> tiles = new List<TileSaveData>();
    }

    [Serializable]
    public class TileSaveData
    {
        public int x;
        public int y;
        public int spriteIndex;

        public TileSaveData(int x, int y, int spriteIndex)
        {
            this.x = x;
            this.y = y;
            this.spriteIndex = spriteIndex;
        }
    }
}
