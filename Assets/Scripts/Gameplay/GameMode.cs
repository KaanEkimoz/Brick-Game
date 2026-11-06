using System;

namespace Gameplay
{
    /// <summary>
    /// Global flag set by the main menu to pick between Classic and Extended play.
    /// Reset on each scene reload — B_Play clears it, B_PlayExtended raises it.
    /// </summary>
    public static class GameMode
    {
        public static event Action OnChanged;

        private static bool _isExtended;
        private static bool _isLevels;

        /// <summary>True while a Levels run is active. Levels never touch the run saves and
        /// keep their own high score.</summary>
        public static bool IsLevels
        {
            get => _isLevels;
            set
            {
                if (_isLevels == value) return;
                _isLevels = value;
                OnChanged?.Invoke();
            }
        }

        /// <summary>True during a daily challenge run, which runs inside Levels. Keeps its own
        /// high score.</summary>
        public static bool IsDaily { get; set; }

        public static bool IsExtended
        {
            get => _isExtended;
            set
            {
                if (_isExtended == value) return;
                _isExtended = value;
                OnChanged?.Invoke();
            }
        }
    }
}
