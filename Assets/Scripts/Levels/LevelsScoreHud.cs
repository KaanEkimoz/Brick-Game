using Extras;
using Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Levels
{
    /// <summary>
    /// Levels have no high score on the HUD: the score label takes over its whole box during a
    /// Levels run and returns to its scene layout when the run ends.
    /// </summary>
    public class LevelsScoreHud : MonoBehaviour
    {
        private const float LevelsFontSize = 36f;

        private ScoreController _score;
        private RectTransform _scoreRect;
        private LayoutElement _scoreLayout;
        private Vector2 _anchorMin;
        private Vector2 _anchorMax;
        private Vector2 _anchoredPosition;
        private Vector2 _sizeDelta;
        private float _fontSize;
        private bool _filled;

        private void OnEnable()
        {
            GameMode.OnChanged += Apply;
        }

        private void OnDisable()
        {
            GameMode.OnChanged -= Apply;
        }

        private void Start()
        {
            _score = FindObjectOfType<ScoreController>(true);
            _scoreRect = _score.scoreText.rectTransform;
            _scoreLayout = _score.scoreText.GetComponent<LayoutElement>();
            Apply();
        }

        private void Apply()
        {
            if (_score == null || GameMode.IsLevels == _filled) return;

            _filled = GameMode.IsLevels;
            _score.highScoreText.gameObject.SetActive(!_filled);
            if (_filled) Fill();
            else Restore();
        }

        private void Fill()
        {
            _anchorMin = _scoreRect.anchorMin;
            _anchorMax = _scoreRect.anchorMax;
            _anchoredPosition = _scoreRect.anchoredPosition;
            _sizeDelta = _scoreRect.sizeDelta;
            _fontSize = _score.scoreText.fontSize;

            _scoreLayout.ignoreLayout = true;
            _scoreRect.anchorMin = Vector2.zero;
            _scoreRect.anchorMax = Vector2.one;
            _scoreRect.offsetMin = Vector2.zero;
            _scoreRect.offsetMax = Vector2.zero;
            _score.scoreText.fontSize = LevelsFontSize;
        }

        private void Restore()
        {
            _scoreLayout.ignoreLayout = false;
            _scoreRect.anchorMin = _anchorMin;
            _scoreRect.anchorMax = _anchorMax;
            _scoreRect.anchoredPosition = _anchoredPosition;
            _scoreRect.sizeDelta = _sizeDelta;
            _score.scoreText.fontSize = _fontSize;
        }
    }
}
