using UnityEngine;

namespace UI
{
    /// <summary>
    /// Drives a UI element's RectTransform anchors so its visible area stays inside
    /// Screen.safeArea — the inset rectangle that excludes the iPhone notch, Dynamic Island,
    /// home-indicator bar, and Android display cutouts. Apple's Human Interface Guidelines
    /// require that interactive controls and important content live inside this region,
    /// or App Review can reject for "controls obscured by system UI".
    ///
    /// Drop on any UI root (Canvas, page panel, button bar) and the rect anchors snap to
    /// the safe area on every Awake / orientation change / device-pixel-ratio change.
    /// Cheap to leave running — only updates when safeArea changes.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    [DisallowMultipleComponent]
    public class SafeAreaApplier : MonoBehaviour
    {
        [Tooltip("Apply the inset only on the top edge (notch). Useful for top HUD bars when " +
                 "you want the rest of the UI to stay full-bleed. Defaults off (apply all sides).")]
        [SerializeField] private bool _topOnly;

        [Tooltip("Apply the inset only on the bottom edge (home indicator). Useful for bottom " +
                 "button bars. Defaults off (apply all sides).")]
        [SerializeField] private bool _bottomOnly;

        private RectTransform _rt;
        private Rect _lastSafeArea;
        private Vector2Int _lastScreen;

        private void Awake()
        {
            _rt = GetComponent<RectTransform>();
            Apply();
        }

        private void OnEnable() => Apply();

        private void Update()
        {
            // Detect orientation / screen-size / split-view / safe-area changes cheaply by
            // hashing the relevant inputs. The actual anchor write happens only when one of
            // them moves, so the per-frame cost in the steady state is just two comparisons.
            if (Screen.safeArea != _lastSafeArea ||
                Screen.width != _lastScreen.x ||
                Screen.height != _lastScreen.y)
            {
                Apply();
            }
        }

        private void Apply()
        {
            if (_rt == null) _rt = GetComponent<RectTransform>();

            Rect area = Screen.safeArea;
            _lastSafeArea = area;
            _lastScreen = new Vector2Int(Screen.width, Screen.height);

            // Convert the safe-area rect (pixels) to normalized anchor coords [0..1].
            Vector2 anchorMin = area.position;
            Vector2 anchorMax = area.position + area.size;
            anchorMin.x /= Mathf.Max(1, Screen.width);
            anchorMin.y /= Mathf.Max(1, Screen.height);
            anchorMax.x /= Mathf.Max(1, Screen.width);
            anchorMax.y /= Mathf.Max(1, Screen.height);

            // Apply edge filters — _topOnly keeps everything except the top edge at the
            // physical screen edge so a content area below stays full-bleed; _bottomOnly
            // is the symmetric case.
            if (_topOnly)
            {
                anchorMin = new Vector2(0f, 0f);
                anchorMax = new Vector2(1f, anchorMax.y);
            }
            else if (_bottomOnly)
            {
                anchorMin = new Vector2(0f, anchorMin.y);
                anchorMax = new Vector2(1f, 1f);
            }

            _rt.anchorMin = anchorMin;
            _rt.anchorMax = anchorMax;
            _rt.offsetMin = Vector2.zero;
            _rt.offsetMax = Vector2.zero;
        }
    }
}
