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

        [Header("Big Level Up Banner")]
        [SerializeField] private GameObject _levelUpBanner;
        [SerializeField] private float _bannerDuration = 1.4f;
        [SerializeField] private float _bannerScalePeak = 1.25f;

        private int _lastLevel = 1;
        private Coroutine _running;
        private Coroutine _bannerRunning;

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
            // LoadLevel (Continue with save) fires this event so drop-time recalculates,
            // but it is not a real level-up — sync the gate and skip the banner.
            if (LevelController.IsRestoring)
            {
                _lastLevel = LevelController.CurrentLevel;
                return;
            }

            // LevelController fires on every line clear regardless of whether the level actually changed,
            // so gate here on an actual increase.
            if (LevelController.CurrentLevel <= _lastLevel) return;
            _lastLevel = LevelController.CurrentLevel;

            if (_levelText != null)
            {
                if (_running != null) StopCoroutine(_running);
                _running = StartCoroutine(Pulse());
            }
            if (_levelUpBanner != null)
            {
                if (_bannerRunning != null) StopCoroutine(_bannerRunning);
                _bannerRunning = StartCoroutine(ShowBanner());
            }
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

        private IEnumerator ShowBanner()
        {
            _levelUpBanner.SetActive(true);
            Vector3 baseScale = _levelUpBanner.transform.localScale;
            float t = 0f;
            while (t < _bannerDuration)
            {
                t += Time.deltaTime;
                float n = Mathf.Clamp01(t / _bannerDuration);
                // Scale: pop in, hold, ease down at the tail
                float pulse = 1f + (_bannerScalePeak - 1f) * Mathf.Sin(n * Mathf.PI);
                _levelUpBanner.transform.localScale = baseScale * pulse;
                yield return null;
            }
            _levelUpBanner.transform.localScale = baseScale;
            _levelUpBanner.SetActive(false);
            _bannerRunning = null;
        }

        [ContextMenu("Test Pulse")]
        private void TestPulse()
        {
            if (_levelText != null)
            {
                if (_running != null) StopCoroutine(_running);
                _running = StartCoroutine(Pulse());
            }
            if (_levelUpBanner != null)
            {
                if (_bannerRunning != null) StopCoroutine(_bannerRunning);
                _bannerRunning = StartCoroutine(ShowBanner());
            }
        }
    }
}
