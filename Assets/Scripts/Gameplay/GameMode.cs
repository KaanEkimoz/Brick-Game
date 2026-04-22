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
