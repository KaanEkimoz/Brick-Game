using System;
using Board;
using UnityEngine;

namespace Gameplay
{
    /// <summary>
    /// Tracks line clears in Extended mode and, when the bar fills, picks a random ability that the
    /// next spawned piece will carry. Classic mode runs identical code paths but never arms.
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

        private void HandleLinesCleared(int count)
        {
            if (!GameMode.IsExtended) return;
            if (count <= 0) return;
            if (_pending.HasValue) return; // already armed — wait for player to spend it

            _charge = Mathf.Min(_charge + count, MaxCharge);
            OnChargeChanged?.Invoke(_charge, MaxCharge);

            if (_charge >= MaxCharge)
            {
                _pending = (AbilityType)UnityEngine.Random.Range(0, 3);
                OnAbilityArmed?.Invoke(_pending.Value);
            }
        }

        /// <summary>Consume the armed ability (called by the spawner when an ability piece is emitted).</summary>
        public AbilityType ConsumePending()
        {
            AbilityType t = _pending ?? AbilityType.Bomb;
            _pending = null;
            _charge = 0;
            OnChargeChanged?.Invoke(_charge, MaxCharge);
            return t;
        }
    }
}
