using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Scene
{
    /// <summary>Shrinks a button while it is held and springs it back on release. A button
    /// that invites a tap can also breathe while it waits.</summary>
    public class ButtonMotion : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        private const float PressedScale = 0.92f;
        private const float PressSeconds = 0.08f;
        private const float ReleaseSeconds = 0.22f;
        private const float PulseAmount = 0.04f;
        private const float PulsePeriod = 1.8f;

        private Vector3 _baseScale;
        private float _press = 1f;
        private bool _held;
        private bool _pulse;
        private Coroutine _tween;

        /// <summary>Lets the button breathe gently while nothing touches it.</summary>
        public void EnablePulse()
        {
            _pulse = true;
        }

        private void Awake()
        {
            _baseScale = transform.localScale;
        }

        private void OnDisable()
        {
            _press = 1f;
            _held = false;
            _tween = null;
            transform.localScale = _baseScale;
        }

        // Leaves the scale alone while idle, so other code can still size the button.
        private void Update()
        {
            if (!_pulse && _tween == null) return;
            float pulse = _pulse && !_held ? 1f + PulseAmount * (0.5f - 0.5f * Mathf.Cos(Time.unscaledTime * 2f * Mathf.PI / PulsePeriod)) : 1f;
            transform.localScale = _baseScale * (_press * pulse);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _held = true;
            Tween(PressedScale, PressSeconds, false);
        }

        public void OnPointerUp(PointerEventData eventData) => Release();

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_held) Release();
        }

        private void Release()
        {
            _held = false;
            Tween(1f, ReleaseSeconds, true);
        }

        private void Tween(float target, float seconds, bool spring)
        {
            if (_tween != null) StopCoroutine(_tween);
            if (isActiveAndEnabled) _tween = StartCoroutine(TweenRoutine(_press, target, seconds, spring));
        }

        private IEnumerator TweenRoutine(float from, float to, float seconds, bool spring)
        {
            for (float t = 0f; t < seconds; t += Time.unscaledDeltaTime)
            {
                float n = t / seconds;
                _press = Mathf.LerpUnclamped(from, to, spring ? UiEase.OutBack(n) : UiEase.OutCubic(n));
                yield return null;
            }
            _press = to;
            _tween = null;
            if (!_pulse) transform.localScale = _baseScale * _press;
        }
    }
}
