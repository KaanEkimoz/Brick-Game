using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace InGame
{
    /// <summary>
    /// Classic 7-bag randomizer used by every modern Tetris implementation. The seven
    /// standard tetrominoes (O,I,S,Z,L,J,T) are placed in a "bag", shuffled, and
    /// dispensed in order; when the bag empties it is refilled and reshuffled. This
    /// eliminates the worst case of pure RNG (long droughts of one piece, runs of
    /// duplicates) while still feeling random over short windows. The same bag is
    /// shared as a static instance because PieceSpawner is a single-spawner game.
    /// </summary>
    public static class PieceBag
    {
        // O, I, S, Z, L, J, T — abilities (Bomb, Laser) bypass the bag completely.
        private static readonly PieceType[] StandardSeven =
        {
            PieceType.O, PieceType.I, PieceType.S, PieceType.Z,
            PieceType.L, PieceType.J, PieceType.T,
        };

        private static readonly List<PieceType> _current = new List<PieceType>(7);

        /// <summary>Pulls the next piece from the bag, refilling + reshuffling when empty.</summary>
        public static PieceType Next()
        {
            if (_current.Count == 0) Refill();
            int idx = _current.Count - 1;
            PieceType pick = _current[idx];
            _current.RemoveAt(idx);
            return pick;
        }

        /// <summary>Resets the bag to empty (next Next() call refills + shuffles).</summary>
        public static void Reset() => _current.Clear();

        /// <summary>Snapshot of the remaining pieces for the save file (int per PieceType).</summary>
        public static List<int> Snapshot()
        {
            var copy = new List<int>(_current.Count);
            for (int i = 0; i < _current.Count; i++)
                copy.Add((int)_current[i]);
            return copy;
        }

        /// <summary>Replace the bag contents from a save. Invalid entries are dropped silently —
        /// a corrupt save just starts the next bag fresh, never crashes the game.</summary>
        public static void RestoreFromSnapshot(List<int> snapshot)
        {
            _current.Clear();
            if (snapshot == null) return;
            for (int i = 0; i < snapshot.Count; i++)
            {
                int v = snapshot[i];
                // Only allow the 7 standard tetrominoes (0..6) into the bag — never Bomb/Laser.
                if (v >= 0 && v <= 6) _current.Add((PieceType)v);
            }
        }

        private static void Refill()
        {
            _current.Clear();
            for (int i = 0; i < StandardSeven.Length; i++)
                _current.Add(StandardSeven[i]);
            // Fisher–Yates shuffle.
            for (int i = _current.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (_current[i], _current[j]) = (_current[j], _current[i]);
            }
        }
    }
}
