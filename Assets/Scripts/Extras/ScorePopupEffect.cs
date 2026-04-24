using System.Collections;
using TMPro;
using UnityEngine;

namespace Extras
{
    /// <summary>
    /// Spawns a "+N" popup right over the score TextMeshProUGUI each time
    /// ScoreController reports a positive score gain. Borrows the score text's
    /// font so the popup matches the HUD, then pops in, rises, and fades out.
    /// </summary>
    public class ScorePopupEffect : MonoBehaviour
    {
        [SerializeField] private TMP_Text _scoreText;
        [SerializeField] private float _fontSize = 40f;
        [SerializeField] private float _duration = 0.9f;
        [SerializeField] [Range(0f, 1f)] private float _startAlpha = 0.75f;
        [SerializeField] private float _riseDistance = 60f;
        [SerializeField] private float _peakScale = 1.25f;
        [SerializeField] private float _randomX = 20f;
        [SerializeField] private Color _popupColor = Color.white;
        [SerializeField] private Color _outlineColor = Color.black;
        [SerializeField] [Range(0f, 1f)] private float _outlineWidth = 0.2f;

        private void OnEnable()
        {
            ScoreController.OnScoreAdded += HandleScoreAdded;
        }

        private void OnDisable()
        {
            ScoreController.OnScoreAdded -= HandleScoreAdded;
        }

        private void HandleScoreAdded(int delta, int total)
        {
            if (delta <= 0) return;
            if (_scoreText == null) return;
            StartCoroutine(SpawnPopup(delta));
        }

        private IEnumerator SpawnPopup(int delta)
        {
            GameObject go = new GameObject("ScorePopup");
            go.transform.SetParent(_scoreText.transform.parent, false);

            RectTransform srt = _scoreText.rectTransform;
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchorMin = srt.anchorMin;
            rt.anchorMax = srt.anchorMax;
            rt.pivot = srt.pivot;
            rt.sizeDelta = srt.sizeDelta;

            Vector2 startPos = srt.anchoredPosition + new Vector2(Random.Range(-_randomX, _randomX), 0f);
            rt.anchoredPosition = startPos;
            rt.localRotation = Quaternion.identity;
            rt.localScale = Vector3.one;

            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.font = _scoreText.font;
            tmp.text = "+" + delta;
            tmp.fontSize = _fontSize;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableWordWrapping = false;
            tmp.raycastTarget = false;

            Color baseColor = _popupColor; baseColor.a = _startAlpha;
            tmp.color = baseColor;
            tmp.outlineColor = _outlineColor;
            tmp.outlineWidth = _outlineWidth;

            float t = 0f;
            while (t < _duration)
            {
                t += Time.deltaTime;
                float n = Mathf.Clamp01(t / _duration);

                // Scale: 0.3 → peak in first 25%, peak → 1 in the remaining 75%
                float scaleFactor;
                if (n < 0.25f)
                {
                    float k = n / 0.25f;
                    scaleFactor = Mathf.Lerp(0.3f, _peakScale, 1f - Mathf.Pow(1f - k, 2f));
                }
                else
                {
                    float k = (n - 0.25f) / 0.75f;
                    scaleFactor = Mathf.Lerp(_peakScale, 1f, k);
                }
                rt.localScale = Vector3.one * scaleFactor;

                // Rise
                float riseEase = 1f - Mathf.Pow(1f - n, 2f);
                rt.anchoredPosition = startPos + Vector2.up * (_riseDistance * riseEase);

                // Hold alpha to 60%, then fade
                float alphaMul = n < 0.6f ? 1f : Mathf.Lerp(1f, 0f, (n - 0.6f) / 0.4f);
                Color c = _popupColor; c.a = _startAlpha * alphaMul;
                tmp.color = c;

                yield return null;
            }

            Destroy(go);
        }

        [ContextMenu("Test Popup")]
        private void TestPopup()
        {
            HandleScoreAdded(40, 40);
        }
    }
}
