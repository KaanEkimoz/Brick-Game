using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay
{
    /// <summary>
    /// Vertical ability bar: one segment per charge, filling from the bottom, with a bolt icon
    /// on top. Built at runtime inside the bar root by AbilityChargeBarUI. It replaces the old
    /// horizontal Slider, whose flash overlay is kept and stretched over the whole bar.
    /// </summary>
    public class SegmentedChargeBar : MonoBehaviour
    {
        private const string FlashOverlayPath = "Slider/Fill Area/BurstOverlay";
        private const float BoltOffset = 0.06f; // the bolt art sits left of its sprite's centre
        private const float SegmentsTop = 0.86f;
        private const float SegmentsBottom = 0.02f;
        private const float SidePadding = 0.14f;
        private const float SegmentGap = 0.012f;
        private const float PunchDuration = 0.22f;
        private const float PunchScale = 1.2f;

        private static readonly Color BottomColor = new Color(1f, 0.2f, 0.95f, 1f);
        private static readonly Color TopColor = new Color(0.2f, 0.95f, 1f, 1f);
        private static readonly Color EmptyColor = new Color(0.03f, 0.07f, 0.2f, 0.9f);
        private static readonly Color BoltColor = new Color(0.45f, 0.9f, 1f, 1f);
        private static readonly Color PanelColor = new Color(0.06f, 0.16f, 0.42f, 0.8f);
        private static readonly Color BorderColor = new Color(0.25f, 0.6f, 1f, 0.7f);

        private Image[] _segments;
        private Outline[] _glows;
        private int _shownCharge;

        /// <summary>Builds the bar. Called while the root may still be inactive.</summary>
        public void Build(Slider oldSlider, Sprite boltSprite)
        {
            Transform flashOverlay = transform.Find(FlashOverlayPath);
            if (flashOverlay != null) flashOverlay.SetParent(transform, false);
            if (oldSlider != null) oldSlider.gameObject.SetActive(false);

            // The old panel sprite has transparent margins that stretch with the bar, so the
            // panel is a plain rect that matches the scene anchors exactly. It covers a tall
            // strip and must not swallow swipes that start on it.
            Image rootImage = GetComponent<Image>();
            if (rootImage != null)
            {
                rootImage.sprite = null;
                rootImage.color = PanelColor;
                rootImage.raycastTarget = false;
                Outline border = gameObject.AddComponent<Outline>();
                border.effectColor = BorderColor;
                border.effectDistance = new Vector2(2f, 2f);
            }

            _segments = new Image[AbilityCharger.MaxCharge];
            _glows = new Outline[AbilityCharger.MaxCharge];
            for (int i = 0; i < _segments.Length; i++)
                CreateSegment(i);

            if (boltSprite != null) CreateBolt(boltSprite);
            if (flashOverlay != null) flashOverlay.SetAsLastSibling();
        }

        private void OnEnable()
        {
            AbilityCharger.OnChargeChanged += HandleChargeChanged;
            _shownCharge = AbilityCharger.Instance != null ? AbilityCharger.Instance.Charge : 0;
            ApplyCharge(_shownCharge);
        }

        private void OnDisable()
        {
            AbilityCharger.OnChargeChanged -= HandleChargeChanged;
        }

        private void CreateSegment(int index)
        {
            float height = (SegmentsTop - SegmentsBottom) / _segments.Length;
            float bottom = SegmentsBottom + height * index;

            Image segment = CreateImage("Segment" + index);
            RectTransform rect = segment.rectTransform;
            rect.anchorMin = new Vector2(SidePadding, bottom + SegmentGap);
            rect.anchorMax = new Vector2(1f - SidePadding, bottom + height - SegmentGap);

            Outline glow = segment.gameObject.AddComponent<Outline>();
            glow.effectDistance = new Vector2(3f, 3f);
            _segments[index] = segment;
            _glows[index] = glow;
        }

        private void CreateBolt(Sprite boltSprite)
        {
            Image bolt = CreateImage("Bolt");
            bolt.sprite = boltSprite;
            bolt.preserveAspect = true;
            bolt.color = BoltColor;
            bolt.rectTransform.anchorMin = new Vector2(SidePadding + BoltOffset, SegmentsTop + SegmentGap);
            bolt.rectTransform.anchorMax = new Vector2(1f - SidePadding + BoltOffset, 0.98f);
        }

        private Image CreateImage(string objectName)
        {
            GameObject go = new GameObject(objectName, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(transform, false);
            RectTransform rect = (RectTransform)go.transform;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            Image image = go.GetComponent<Image>();
            image.raycastTarget = false;
            return image;
        }

        private void HandleChargeChanged(int current, int max)
        {
            int previous = _shownCharge;
            _shownCharge = current;
            ApplyCharge(current);

            for (int i = previous; i < current && i < _segments.Length; i++)
                StartCoroutine(Punch(_segments[i].rectTransform));
        }

        private void ApplyCharge(int charge)
        {
            if (_segments == null) return;

            for (int i = 0; i < _segments.Length; i++)
            {
                bool lit = i < charge;
                float t = (float)i / (_segments.Length - 1);
                Color litColor = Color.Lerp(BottomColor, TopColor, t);
                _segments[i].color = lit ? litColor : EmptyColor;
                _segments[i].rectTransform.localScale = Vector3.one;
                _glows[i].enabled = lit;
                litColor.a = 0.45f;
                _glows[i].effectColor = litColor;
            }
        }

        private IEnumerator Punch(RectTransform segment)
        {
            float elapsed = 0f;
            while (elapsed < PunchDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float n = Mathf.Clamp01(elapsed / PunchDuration);
                segment.localScale = Vector3.one * Mathf.Lerp(PunchScale, 1f, n);
                yield return null;
            }
            segment.localScale = Vector3.one;
        }
    }
}
