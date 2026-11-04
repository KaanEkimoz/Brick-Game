using UnityEngine;

namespace Scene
{
    /// <summary>Lets the menu logo drift slowly up and down.</summary>
    [RequireComponent(typeof(RectTransform))]
    public class LogoFloat : MonoBehaviour
    {
        private const float Height = 7f;
        private const float Period = 3.2f;

        private RectTransform _rect;
        private Vector2 _base;

        private void Awake()
        {
            _rect = (RectTransform)transform;
            _base = _rect.anchoredPosition;
        }

        private void OnDisable()
        {
            _rect.anchoredPosition = _base;
        }

        private void Update()
        {
            _rect.anchoredPosition = _base + Vector2.up * (Height * Mathf.Sin(Time.unscaledTime * 2f * Mathf.PI / Period));
        }
    }
}
