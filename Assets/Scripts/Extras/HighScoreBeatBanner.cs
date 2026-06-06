using System.Collections;
using TMPro;
using UnityEngine;

namespace Extras
{
    /// <summary>
    /// Center-screen "NEW BEST!" toast fired the instant the live score crosses the
    /// player's previously saved high score. Listens to ScoreController.OnHighScoreBeaten
    /// which is latched to fire exactly once per run — re-arms on ResetScore.
    ///
    /// Mechanic mirrors AbilityArmedBanner / LevelUpEffect.ShowBanner: activate a root
    /// GameObject, pulse it with a sin curve, deactivate. Time.unscaledDeltaTime so it
    /// keeps playing during paused-by-tutorial / pause-menu moments (the score event
    /// itself only fires during gameplay, but the banner shouldn't freeze if the
    /// player taps Pause mid-burst).
    ///
    /// Headline + sub copy are SerializeFields so they can be tuned without touching
    /// code (e.g. localization can swap "NEW BEST!" → "YENİ REKOR!" in the inspector).
    /// </summary>
    public class HighScoreBeatBanner : MonoBehaviour
    {
        [Tooltip("Root that gets SetActive(true) for the burst. Disabled at rest.")]
        [SerializeField] private GameObject _bannerRoot;

        [Tooltip("Headline text (\"NEW BEST!\"). Optional — leave unset to ignore.")]
        [SerializeField] private TextMeshProUGUI _headlineText;

        [Tooltip("Sub-line that shows the new score (\"Score 12,450\"). Optional.")]
        [SerializeField] private TextMeshProUGUI _subText;

        [Tooltip("Headline copy.")]
        [SerializeField] private string _headlineTemplate = "NEW BEST!";

        [Tooltip("Sub-line template. {score} is replaced with the live score at beat time.")]
        [SerializeField] private string _subTemplate = "Score {score}";

        [SerializeField] private float _duration = 1.4f;
        [SerializeField] private float _scalePeak = 1.25f;

        private Coroutine _running;

        private void OnEnable()
        {
            ScoreController.OnHighScoreBeaten += Handle;
        }

        private void OnDisable()
        {
            ScoreController.OnHighScoreBeaten -= Handle;
            if (_running != null) { StopCoroutine(_running); _running = null; }
            if (_bannerRoot != null) _bannerRoot.SetActive(false);
        }

        private void Handle(int newScore)
        {
            if (_bannerRoot == null) return;
            ApplyCopy(newScore);
            if (_running != null) StopCoroutine(_running);
            _running = StartCoroutine(Show());
        }

        private void ApplyCopy(int newScore)
        {
            if (_headlineText != null)
                _headlineText.text = _headlineTemplate;
            if (_subText != null)
                _subText.text = _subTemplate.Replace("{score}", newScore.ToString("N0"));
        }

        private IEnumerator Show()
        {
            _bannerRoot.SetActive(true);
            Vector3 baseScale = _bannerRoot.transform.localScale;
            float t = 0f;
            while (t < _duration)
            {
                t += Time.unscaledDeltaTime;
                float n = Mathf.Clamp01(t / _duration);
                float pulse = 1f + (_scalePeak - 1f) * Mathf.Sin(n * Mathf.PI);
                _bannerRoot.transform.localScale = baseScale * pulse;
                yield return null;
            }
            _bannerRoot.transform.localScale = baseScale;
            _bannerRoot.SetActive(false);
            _running = null;
        }

        [ContextMenu("Test Burst")]
        private void TestBurst() => Handle(12345);
    }
}
