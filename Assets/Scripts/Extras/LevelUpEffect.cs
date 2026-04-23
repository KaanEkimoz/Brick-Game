using System.Collections;
using TMPro;
using UnityEngine;

namespace Extras
{
    /// <summary>
    /// Listens for LevelController.OnLevelIncreased and, only when the level actually ticked up,
    /// pulses the level text's colour and scale. No particles — the existing UI element animates itself.
    /// </summary>
    public class LevelUpEffect : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private float _duration = 0.55f;
        [SerializeField] private float _scalePeak = 1.4f;
        [SerializeField] private Color _flashColor = new Color(1f, 0.85f, 0.2f, 1f);

        private int _lastLevel = 1;
        private Coroutine _running;

        private void OnEnable()
        {
            LevelController.OnLevelIncreased += Handle;
        }

        private void OnDisable()
        {
            LevelController.OnLevelIncreased -= Handle;
        }

        private void Start()
        {
            _lastLevel = LevelController.CurrentLevel;
        }

        private void Handle()
        {
            // LevelController fires on every line clear regardless of whether the level actually changed,
            // so gate here on an actual increase.
            if (LevelController.CurrentLevel <= _lastLevel) return;
            _lastLevel = LevelController.CurrentLevel;
            if (_levelText == null) return;
            if (_running != null) StopCoroutine(_running);
            _running = StartCoroutine(Pulse());
        }

        private IEnumerator Pulse()
        {
            Color originalColor = _levelText.color;
            Vector3 originalScale = _levelText.transform.localScale;
            float t = 0f;
            while (t < _duration)
            {
                t += Time.deltaTime;
                float n = Mathf.Clamp01(t / _duration);
                // Scale pulse: 1 → peak → 1 via a sine curve
                float scaleFactor = 1f + (_scalePeak - 1f) * Mathf.Sin(n * Mathf.PI);
                _levelText.transform.localScale = originalScale * scaleFactor;
                // Colour flash at t=0, easing back to original as time progresses
                _levelText.color = Color.Lerp(_flashColor, originalColor, n);
                yield return null;
            }
            _levelText.transform.localScale = originalScale;
            _levelText.color = originalColor;
            _running = null;
        }

        [ContextMenu("Test Pulse")]
        private void TestPulse()
        {
            if (_levelText == null) return;
            if (_running != null) StopCoroutine(_running);
            _running = StartCoroutine(Pulse());
        }
    }
}
