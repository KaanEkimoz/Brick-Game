using System;
using Board;
using UnityEngine;

namespace Gameplay
{
    /// <summary>
    /// Tracks line clears in Extended mode and, when the bar fills, picks a random ability that the
    /// next spawned tetromino will carry on one tile. Classic mode runs identical code paths but never arms.
    /// </summary>
    public class AbilityCharger : MonoBehaviour
    {
        public const int MaxCharge = 5;

        public static AbilityCharger Instance;
        public static Action<int, int> OnChargeChanged;   // (current, max)
        public static Action<AbilityType> OnAbilityArmed; // pending ability announced to UI

        private int _charge;
        private AbilityType? _pending;

        public int Charge => _charge;
        public bool IsReady => _pending.HasValue;
        public AbilityType? Pending => _pending;

        private void Awake()
        {
            Instance = this;
            _charge = 0;
            _pending = null;
        }

        private void OnEnable()
        {
            BoardController.OnLinesCleared += HandleLinesCleared;
        }

        private void OnDisable()
        {
            BoardController.OnLinesCleared -= HandleLinesCleared;
        }

        // Patch only appears in levels that ask for it by name.
        private static readonly AbilityType[] BarAbilities = { AbilityType.Bomb, AbilityType.Laser, AbilityType.Dynamite };

        private void HandleLinesCleared(int count)
        {
            if (!GameMode.IsExtended) return;
            if (count <= 0) return;
            if (_pending.HasValue) return; // already armed — wait for player to spend it

            _charge = Mathf.Min(_charge + count, MaxCharge);
            OnChargeChanged?.Invoke(_charge, MaxCharge);

            if (_charge >= MaxCharge)
            {
                _pending = BarAbilities[UnityEngine.Random.Range(0, BarAbilities.Length)];
                OnAbilityArmed?.Invoke(_pending.Value);
            }
        }

#if UNITY_EDITOR
        /// <summary>Editor-only: force the bar to full and arm the given ability. Used by the F debug hotkey to test the bar flash without farming line clears.</summary>
        public void DebugForceFill(AbilityType type)
        {
            _charge = MaxCharge;
            OnChargeChanged?.Invoke(_charge, MaxCharge);
            _pending = type;
            OnAbilityArmed?.Invoke(type);
        }
#endif

        /// <summary>Consume the armed ability (called by the spawner when it charges a piece).</summary>
        public AbilityType ConsumePending()
        {
            AbilityType t = _pending ?? AbilityType.Bomb;
            _pending = null;
            _charge = 0;
            OnChargeChanged?.Invoke(_charge, MaxCharge);
            return t;
        }

        /// <summary>Restores the bar from a save file and refreshes the UI.</summary>
        public void RestoreCharge(int charge)
        {
            _charge = Mathf.Clamp(charge, 0, MaxCharge - 1);
            _pending = null;
            OnChargeChanged?.Invoke(_charge, MaxCharge);
        }

        /// <summary>Wipes charge + armed pending and broadcasts the empty bar to UI listeners.
        /// Called from PiecesController.DestroyCurPiece on the in-place Restart path so a
        /// 4/5 charge (or already-armed Bomb/Laser) from the previous run does not carry
        /// into the next — otherwise the next line clear would arm/fire an ability the
        /// player never earned, and the AbilityChargeBarUI would stay falsely full.</summary>
        public void ResetCharge()
        {
            _charge = 0;
            _pending = null;
            OnChargeChanged?.Invoke(_charge, MaxCharge);
        }
    }
}
