using System.Collections;
using UnityEngine;

namespace Scene
{
    /// <summary>Brings a list item in after a short delay, so rows and cards arrive one by one.</summary>
    [RequireComponent(typeof(RectTransform))]
    public class StaggerIn : MonoBehaviour
    {
        private const float Step = 0.05f;
        private const float Seconds = 0.28f;
        private const float Rise = 36f;

        private float _delay;

        public static void Add(Component item, int index)
        {
            StaggerIn stagger = item.gameObject.AddComponent<StaggerIn>();
            stagger._delay = index * Step;
        }

        private IEnumerator Start()
        {
            RectTransform rect = (RectTransform)transform;
            CanvasGroup group = GetComponent<CanvasGroup>();
            if (group == null) group = gameObject.AddComponent<CanvasGroup>();
            Vector2 end = rect.anchoredPosition;

            group.alpha = 0f;
            rect.anchoredPosition = end + Vector2.down * Rise;
            for (float t = 0f; t < _delay; t += Time.unscaledDeltaTime) yield return null;

            for (float t = 0f; t < Seconds; t += Time.unscaledDeltaTime)
            {
                float n = t / Seconds;
                group.alpha = UiEase.OutCubic(n * 1.5f);
                rect.anchoredPosition = end + Vector2.down * (Rise * (1f - UiEase.OutCubic(n)));
                yield return null;
            }
            group.alpha = 1f;
            rect.anchoredPosition = end;
        }
    }
}
