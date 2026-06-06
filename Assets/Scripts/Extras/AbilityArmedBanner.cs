using System.Collections;
using Gameplay;
using TMPro;
using UnityEngine;

namespace Extras
{
    /// <summary>
    /// Center-screen banner that pops up when the AbilityCharger arms a Bomb or Laser.
    /// Sibling of LevelUpEffect's "Level Up!" banner — the bar-flash + corner-icon signals
    /// (AbilityBarFlashEffect, AbilityChargeBarUI.HandleArmed) sit in the player's
    /// peripheral vision while they focus on the board, so the "you have an ability"
    /// reveal is easy to miss. A 1.2-second center banner forces the eye, then yields
    /// back to gameplay.
    ///
    /// Listens to AbilityCharger.OnAbilityArmed. No-op outside Extended mode because
    /// the event is only fired there.
    /// </summary>
    public class AbilityArmedBanner : MonoBehaviour
    {
        [Tooltip("Root that gets SetActive(true) for the burst. Disabled at rest.")]
        [SerializeField] private GameObject _bannerRoot;

        [Tooltip("Optional headline text (e.g. \"ABILITY READY!\"). Tint changes per ability.")]
        [SerializeField] private TextMeshProUGUI _headlineText;

        [Tooltip("Optional sub-line that swaps copy per ability (e.g. \"Bomb piece next\").")]
        [SerializeField] private TextMeshProUGUI _subText;

        [Tooltip("Tint applied to headline + sub-text when a Bomb is armed.")]
        [SerializeField] private Color _bombColor = new Color(1f, 0.45f, 0.30f, 1f);

        [Tooltip("Tint applied to headline + sub-text when a Laser is armed.")]
        [SerializeField] private Color _laserColor = new Color(0.35f, 0.85f, 1f, 1f);

        [Tooltip("Headline copy. Substring {ability} is replaced with BOMB / LASER.")]
        [SerializeField] private string _headlineTemplate = "{ability} READY!";

        [Tooltip("Sub-line copy. Substring {ability} is replaced with Bomb / Laser.")]
        [SerializeField] private string _subTemplate = "{ability} piece coming up";

        [SerializeField] private float _duration = 1.2f;
        [SerializeField] private float _scalePeak = 1.25f;

        private Coroutine _running;

        private void OnEnable()
        {
            AbilityCharger.OnAbilityArmed += Handle;
        }

        private void OnDisable()
        {
            AbilityCharger.OnAbilityArmed -= Handle;
            if (_running != null) { StopCoroutine(_running); _running = null; }
            if (_bannerRoot != null) _bannerRoot.SetActive(false);
        }

        private void Handle(AbilityType ability)
        {
            if (_bannerRoot == null) return;
            ApplyAbilityCopy(ability);
            if (_running != null) StopCoroutine(_running);
            _running = StartCoroutine(Show());
        }

        private void ApplyAbilityCopy(AbilityType ability)
        {
            string upper = ability == AbilityType.Bomb ? "BOMB" : "LASER";
            string title = ability == AbilityType.Bomb ? "Bomb" : "Laser";
            Color tint = ability == AbilityType.Bomb ? _bombColor : _laserColor;

            if (_headlineText != null)
            {
                _headlineText.text = _headlineTemplate.Replace("{ability}", upper);
                _headlineText.color = tint;
            }
            if (_subText != null)
            {
                _subText.text = _subTemplate.Replace("{ability}", title);
                _subText.color = tint;
            }
        }

        private IEnumerator Show()
        {
            _bannerRoot.SetActive(true);
            Vector3 baseScale = _bannerRoot.transform.localScale;
            float t = 0f;
            while (t < _duration)
            {
                // Use unscaled — banner must play during paused-by-tutorial moments too.
                t += Time.unscaledDeltaTime;
                float n = Mathf.Clamp01(t / _duration);
                // Scale: pop in, hold, ease down at the tail (sin curve through 1→peak→1)
                float pulse = 1f + (_scalePeak - 1f) * Mathf.Sin(n * Mathf.PI);
                _bannerRoot.transform.localScale = baseScale * pulse;
                yield return null;
            }
            _bannerRoot.transform.localScale = baseScale;
            _bannerRoot.SetActive(false);
            _running = null;
        }

        [ContextMenu("Test Bomb Burst")]
        private void TestBomb() => Handle(AbilityType.Bomb);

        [ContextMenu("Test Laser Burst")]
        private void TestLaser() => Handle(AbilityType.Laser);
    }
}
