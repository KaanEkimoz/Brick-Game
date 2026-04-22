using System;
using System.Collections.Generic;
using InGame;

namespace Persistence
{
    /// <summary>
    /// Serializable snapshot of the game state captured right after a new piece is spawned.
    /// Board holds only settled tiles at this moment — the active piece has not yet called SetTile.
    /// </summary>
    [Serializable]
    public class GameSaveData
    {
        public int saveVersion = 1;

        public int score;
        public int totalClearedLines;
        public int currentLevel;

        public PieceType currentPieceType;
        public PieceType nextPieceType;

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
