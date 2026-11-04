using System.Collections;
using UnityEngine;

namespace Scene
{
    /// <summary>Plays an entrance each time a panel is shown: a small pop, a slide up from
    /// below, or a plain fade for a full-screen backdrop.</summary>
    [RequireComponent(typeof(RectTransform))]
    public class PanelPop : MonoBehaviour
    {
        public enum Style { Pop, SlideUp, Fade }

        private const float PopSeconds = 0.24f;
        private const float SlideSeconds = 0.4f;
        private const float FadeSeconds = 0.18f;
        private const float PopStartScale = 0.88f;
        private const float SlideDistance = 160f;
        private const float CloseSeconds = 0.14f;

        private Style _style = Style.Pop;
        private CanvasGroup _group;
        private RectTransform _rect;
        private Vector3 _baseScale;
        private Vector2 _basePosition;
        private bool _captured;

        public static PanelPop Add(GameObject target, Style style)
        {
            PanelPop pop = target.GetComponent<PanelPop>();
            if (pop == null) pop = target.AddComponent<PanelPop>();
            pop.SetStyle(style);
            return pop;
        }

        // AddComponent on a shown panel has already started the default entrance.
        private void SetStyle(Style style)
        {
            if (_style == style) return;
            _style = style;
            if (!isActiveAndEnabled) return;
            StopAllCoroutines();
            ResetPose();
            StartCoroutine(Open());
        }

        /// <summary>Fades a screen out and removes it. It stops taking taps at once.</summary>
        public static void CloseAndDestroy(GameObject target)
        {
            if (target == null) return;
            PanelPop pop = target.GetComponent<PanelPop>();
            if (pop == null || !pop.isActiveAndEnabled)
            {
                Destroy(target);
                return;
            }
            pop.StopAllCoroutines();
            pop.StartCoroutine(pop.Close());
        }

        private void Awake()
        {
            _rect = (RectTransform)transform;
            _group = GetComponent<CanvasGroup>();
            if (_group == null) _group = gameObject.AddComponent<CanvasGroup>();
        }

        private void OnEnable()
        {
            if (!_captured)
            {
                _baseScale = _rect.localScale;
                _basePosition = _rect.anchoredPosition;
                _captured = true;
            }
            StopAllCoroutines();
            StartCoroutine(Open());
        }

        private void OnDisable()
        {
            ResetPose();
        }

        private void ResetPose()
        {
            _group.alpha = 1f;
            _rect.localScale = _baseScale;
            _rect.anchoredPosition = _basePosition;
        }

        private IEnumerator Open()
        {
            float seconds = _style == Style.SlideUp ? SlideSeconds : _style == Style.Fade ? FadeSeconds : PopSeconds;
            for (float t = 0f; t < seconds; t += Time.unscaledDeltaTime)
            {
                Apply(t / seconds);
                yield return null;
            }
            Apply(1f);
        }

        private void Apply(float n)
        {
            _group.alpha = UiEase.OutCubic(n * 1.6f);
            switch (_style)
            {
                case Style.Pop:
                    _rect.localScale = _baseScale * Mathf.LerpUnclamped(PopStartScale, 1f, UiEase.OutBack(n));
                    break;
                case Style.SlideUp:
                    _rect.anchoredPosition = _basePosition + Vector2.down * (SlideDistance * (1f - UiEase.OutBack(n)));
                    break;
            }
        }

        private IEnumerator Close()
        {
            _group.blocksRaycasts = false;
            float start = _group.alpha;
            for (float t = 0f; t < CloseSeconds; t += Time.unscaledDeltaTime)
            {
                _group.alpha = Mathf.Lerp(start, 0f, t / CloseSeconds);
                yield return null;
            }
            Destroy(gameObject);
        }
    }
}
