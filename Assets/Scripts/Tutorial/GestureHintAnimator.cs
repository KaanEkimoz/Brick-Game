using UnityEngine;

namespace Tutorial
{
    /// <summary>
    /// Loops a small hand-icon animation that demonstrates the gesture a tutorial step
    /// is asking for. Runs on UNSCALED time because the tutorial pauses the game
    /// (Time.timeScale = 0). Drives a child RectTransform (the hand) plus an optional
    /// CanvasGroup for fades. No tweening library needed.
    /// </summary>
    public class GestureHintAnimator : MonoBehaviour
    {
        public enum Mode { Tap, SwipeHorizontal, HoldDown }

        [SerializeField] private Mode _mode = Mode.Tap;
        [Tooltip("The hand icon RectTransform that gets animated.")]
        [SerializeField] private RectTransform _hand;
        [Tooltip("Optional — fades the hand at the end of swipe/hold loops.")]
        [SerializeField] private CanvasGroup _handGroup;
        [Tooltip("Seconds for one full loop.")]
        [SerializeField] private float _cycle = 1.3f;
        [Tooltip("Pixels of travel for swipe (horizontal) / hold (vertical).")]
        [SerializeField] private float _travel = 110f;

        private Vector2 _home;
        private float _t;

        private void OnEnable()
        {
            if (_hand != null) _home = _hand.anchoredPosition;
            _t = 0f;
            Apply(0f);
        }

        private void OnDisable()
        {
            if (_hand != null)
            {
                _hand.anchoredPosition = _home;
                _hand.localScale = Vector3.one;
            }
            if (_handGroup != null) _handGroup.alpha = 1f;
        }

        private void Update()
        {
            if (_hand == null) return;
            _t += Time.unscaledDeltaTime;
            if (_cycle <= 0f) _cycle = 1f;
            Apply((_t % _cycle) / _cycle);
        }

        private void Apply(float phase)
        {
            switch (_mode)
            {
                case Mode.Tap:
                {
                    // One press-and-release dip per cycle, with a short hold at the bottom.
                    float dip = Mathf.Sin(Mathf.Clamp01(phase / 0.5f) * Mathf.PI); // 0→1→0 over first half
                    _hand.anchoredPosition = _home;
                    _hand.localScale = Vector3.one * (1f - 0.18f * dip);
                    if (_handGroup != null) _handGroup.alpha = 1f;
                    break;
                }
                case Mode.SwipeHorizontal:
                {
                    // Start centered, then swing right and left continuously (sine ping-pong):
                    // 0 → +travel → 0 → -travel → 0. Reads as "move either way".
                    float x = Mathf.Sin(phase * Mathf.PI * 2f) * _travel;
                    _hand.anchoredPosition = _home + new Vector2(x, 0f);
                    _hand.localScale = Vector3.one;
                    if (_handGroup != null) _handGroup.alpha = 1f;
                    break;
                }
                case Mode.HoldDown:
                {
                    // Press (slightly smaller) and drag downward, then fade and reset.
                    float e = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(phase / 0.8f));
                    _hand.anchoredPosition = _home + new Vector2(0f, Mathf.Lerp(_travel * 0.45f, -_travel, e));
                    _hand.localScale = Vector3.one * 0.92f;
                    if (_handGroup != null) _handGroup.alpha = phase > 0.85f ? Mathf.InverseLerp(1f, 0.85f, phase) : 1f;
                    break;
                }
            }
        }
    }
}
