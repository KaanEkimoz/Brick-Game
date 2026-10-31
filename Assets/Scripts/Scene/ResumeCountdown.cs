using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace Scene
{
    /// <summary>
    /// Shows a 3-2-1 countdown over the board before play resumes from pause. Runs on unscaled
    /// time because the game stays frozen until it ends. Added at runtime by PauseTheGame.
    /// </summary>
    public class ResumeCountdown : MonoBehaviour
    {
        private const int StartNumber = 3;
        private const float SecondsPerNumber = 0.8f;
        private const float FontSize = 220f;
        private const float PopScale = 1.5f;
        private static readonly Color TextColor = new Color(0.2f, 0.95f, 1f, 1f);

        private TextMeshProUGUI _label;
        private Coroutine _running;
        private Action _onFinished;

        /// <summary>Creates the label under the HUD canvas, reusing the given font.</summary>
        public void Build(Transform canvas, TMP_FontAsset font)
        {
            GameObject go = new GameObject("ResumeCountdown", typeof(RectTransform));
            go.transform.SetParent(canvas, false);
            RectTransform rect = (RectTransform)go.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            _label = go.AddComponent<TextMeshProUGUI>();
            if (font != null) _label.font = font;
            _label.fontSize = FontSize;
            _label.alignment = TextAlignmentOptions.Center;
            _label.color = TextColor;
            _label.raycastTarget = false;
            go.SetActive(false);
        }

        public void Play(Action onFinished)
        {
            Cancel();
            _onFinished = onFinished;
            _label.transform.SetAsLastSibling();
            _label.gameObject.SetActive(true);
            _running = StartCoroutine(Run());
        }

        public void Cancel()
        {
            if (_running != null) StopCoroutine(_running);
            _running = null;
            if (_label != null) _label.gameObject.SetActive(false);
        }

        // Unscaled time jumps across a background stay, which would end the count instantly.
        private void OnApplicationPause(bool paused)
        {
            if (!paused && _running != null)
                Play(_onFinished);
        }

        private IEnumerator Run()
        {
            for (int number = StartNumber; number > 0; number--)
            {
                _label.text = number.ToString();
                float elapsed = 0f;
                while (elapsed < SecondsPerNumber)
                {
                    elapsed += Time.unscaledDeltaTime;
                    float n = Mathf.Clamp01(elapsed / SecondsPerNumber);
                    _label.transform.localScale = Vector3.one * Mathf.Lerp(PopScale, 1f, n * n);
                    yield return null;
                }
            }

            _running = null;
            _label.gameObject.SetActive(false);
            _onFinished?.Invoke();
        }
    }
}
