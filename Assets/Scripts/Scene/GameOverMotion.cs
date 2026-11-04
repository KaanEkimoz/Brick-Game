using System.Collections;
using System.Collections.Generic;
using Extras;
using Gameplay;
using InGame;
using Localization;
using TMPro;
using UnityEngine;

namespace Scene
{
    /// <summary>On the Game Over panel the contents arrive top to bottom, the score counts up to
    /// the final value, and a record gets a pulsing "NEW BEST!" above it.</summary>
    public class GameOverMotion : MonoBehaviour
    {
        private const string EndScoreName = "T_EndScore";
        private const string NewBestText = "NEW BEST!";
        private const float CountSeconds = 0.8f;
        private const float BadgeGap = 115f;
        private const float BadgeFontSize = 44f;
        private const float BadgePulse = 0.08f;
        private const float BadgePeriod = 0.9f;
        private const float CascadeDelay = 0.08f;
        private const float CascadeStep = 0.06f;
        private const float CascadeSeconds = 0.3f;
        private const float CascadeRise = 28f;
        private static readonly Color BadgeColor = new Color(1f, 0.84f, 0.25f, 1f);

        private TextMeshProUGUI _endScore;
        private TextMeshProUGUI _badge;
        private bool _recordThisRun;

        private Transform _panel;

        /// <summary>Points the motion at this scene's panel. Lives on the persistent host,
        /// because the panel stays inactive until the first game over.</summary>
        public void Bind(Transform gameOverPanel)
        {
            _panel = gameOverPanel;
            _endScore = gameOverPanel.Find(EndScoreName)?.GetComponent<TextMeshProUGUI>();
            _badge = null;
            _recordThisRun = false;
        }

        private void Awake()
        {
            ScoreController.OnHighScoreBeaten += HandleRecord;
            PiecesController.OnGameOver += HandleGameOver;
        }

        private void OnDestroy()
        {
            ScoreController.OnHighScoreBeaten -= HandleRecord;
            PiecesController.OnGameOver -= HandleGameOver;
        }

        private void HandleRecord(int score) => _recordThisRun = true;

        private void HandleGameOver()
        {
            bool record = _recordThisRun;
            _recordThisRun = false;
            if (GameMode.IsLevels || _endScore == null) return;

            StartCoroutine(Cascade());
            StartCoroutine(Play(ScoreController.score, record));
        }

        // Hides every item first, so nothing shows before its turn.
        private IEnumerator Cascade()
        {
            List<RectTransform> items = new List<RectTransform>();
            foreach (Transform child in _panel)
                if (child.gameObject.activeSelf && (_badge == null || child != _badge.transform))
                    items.Add((RectTransform)child);
            items.Sort((a, b) => b.anchoredPosition.y.CompareTo(a.anchoredPosition.y));

            CanvasGroup[] groups = new CanvasGroup[items.Count];
            Vector2[] ends = new Vector2[items.Count];
            for (int i = 0; i < items.Count; i++)
            {
                groups[i] = items[i].GetComponent<CanvasGroup>();
                if (groups[i] == null) groups[i] = items[i].gameObject.AddComponent<CanvasGroup>();
                groups[i].alpha = 0f;
                ends[i] = items[i].anchoredPosition;
            }

            float total = CascadeDelay + CascadeStep * items.Count + CascadeSeconds;
            for (float t = 0f; t <= total; t += Time.unscaledDeltaTime)
            {
                for (int i = 0; i < items.Count; i++)
                {
                    if (items[i] == null) continue;
                    float n = Mathf.Clamp01((t - CascadeDelay - i * CascadeStep) / CascadeSeconds);
                    groups[i].alpha = UiEase.OutCubic(n);
                    items[i].anchoredPosition = ends[i] + Vector2.down * (CascadeRise * (1f - UiEase.OutCubic(n)));
                }
                yield return null;
            }
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] == null) continue;
                groups[i].alpha = 1f;
                items[i].anchoredPosition = ends[i];
            }
        }

        private IEnumerator Play(int score, bool record)
        {
            if (_badge != null) _badge.gameObject.SetActive(false);
            yield return null; // after GameOver has written the final text

            string label = Loc.Caps("Score") + "\n";
            for (float t = 0f; t < CountSeconds; t += Time.unscaledDeltaTime)
            {
                if (_endScore == null) yield break;
                _endScore.text = label + Mathf.RoundToInt(score * UiEase.OutCubic(t / CountSeconds));
                yield return null;
            }
            _endScore.text = label + score;

            if (!record) yield break;
            TextMeshProUGUI badge = Badge();
            badge.gameObject.SetActive(true);
            while (badge != null && badge.isActiveAndEnabled)
            {
                float wave = 0.5f - 0.5f * Mathf.Cos(Time.unscaledTime * 2f * Mathf.PI / BadgePeriod);
                badge.rectTransform.localScale = Vector3.one * (1f + BadgePulse * wave);
                badge.alpha = Mathf.Lerp(0.75f, 1f, wave);
                yield return null;
            }
        }

        private TextMeshProUGUI Badge()
        {
            if (_badge != null) return _badge;

            GameObject go = new GameObject("T_NewBest", typeof(RectTransform));
            go.transform.SetParent(_panel, false);
            _badge = go.AddComponent<TextMeshProUGUI>();
            _badge.font = _endScore.font;
            _badge.fontSize = BadgeFontSize;
            _badge.color = BadgeColor;
            _badge.alignment = TextAlignmentOptions.Center;
            _badge.textWrappingMode = TextWrappingModes.NoWrap;
            _badge.raycastTarget = false;
            _badge.text = Loc.T(NewBestText);

            RectTransform rect = _badge.rectTransform;
            RectTransform score = _endScore.rectTransform;
            rect.anchorMin = score.anchorMin;
            rect.anchorMax = score.anchorMax;
            rect.sizeDelta = new Vector2(score.sizeDelta.x, 70f);
            rect.anchoredPosition = score.anchoredPosition + Vector2.up * BadgeGap;
            rect.localRotation = Quaternion.Euler(0f, 0f, 4f);
            return _badge;
        }
    }
}
