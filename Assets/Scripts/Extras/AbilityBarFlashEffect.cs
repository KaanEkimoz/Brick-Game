using System.Collections;
using Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Extras
{
    /// <summary>
    /// Flashes a transparent overlay image white when the charge bar arms.
    /// Decoupled from the Slider's Fill so consuming the ability (which drops Fill width to 0)
    /// doesn't interrupt the burst — the overlay always covers the full bar area.
    /// </summary>
    public class AbilityBarFlashEffect : MonoBehaviour
    {
        [SerializeField] private Image _flashOverlay;
        [SerializeField] private float _burstDuration = 0.45f;
        [SerializeField] private Color _flashColor = Color.white;

        private Coroutine _burstRunning;

        private void OnEnable()
        {
            AbilityCharger.OnAbilityArmed += HandleArmed;
        }

        private void OnDisable()
        {
            AbilityCharger.OnAbilityArmed -= HandleArmed;
            if (_burstRunning != null) { StopCoroutine(_burstRunning); _burstRunning = null; }
            ResetOverlay();
        }

        private void ResetOverlay()
        {
            if (_flashOverlay == null) return;
            Color c = _flashColor;
            c.a = 0f;
            _flashOverlay.color = c;
        }

        private void HandleArmed(AbilityType _)
        {
            if (_flashOverlay == null) return;
            if (_burstRunning != null) StopCoroutine(_burstRunning);
            _burstRunning = StartCoroutine(Burst());
        }

        private IEnumerator Burst()
        {
            float t = 0f;
            while (t < _burstDuration)
            {
                t += Time.deltaTime;
                float n = Mathf.Clamp01(t / _burstDuration);
                Color c = _flashColor;
                c.a = 1f - n; // fade from full white to transparent
                _flashOverlay.color = c;
                yield return null;
            }
            ResetOverlay();
            _burstRunning = null;
        }

        [ContextMenu("Test Burst")]
        private void TestBurst()
        {
            if (_flashOverlay == null) return;
            if (_burstRunning != null) StopCoroutine(_burstRunning);
            _burstRunning = StartCoroutine(Burst());
        }
    }
}
