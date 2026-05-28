using System.Collections.Generic;
using Gameplay;
using UnityEngine;

namespace Tutorial
{
    /// <summary>
    /// One-time, EN-only overlay tutorial shown on the player's first Classic run and,
    /// separately, their first Extended run. Each step is a panel GameObject that plays its
    /// own entrance animation (Animator / tween set up in the scene); this controller only
    /// sequences them and advances on tap. Seen-state is persisted in PlayerPrefs so the
    /// tutorial never repeats once dismissed.
    ///
    /// Gesture copy matches the real <c>TouchInput</c> bindings:
    ///   • Tap            → rotate
    ///   • Swipe L/R      → move
    ///   • Fast swipe down→ hard drop
    ///   • Hold & drag down → soft drop
    ///
    /// Extended adds: charge bar, Bomb (3×3), Laser (rotate → row/column).
    /// New file, no existing code touched (Brick Game convention).
    /// </summary>
    public class TutorialController : MonoBehaviour
    {
        private const string ClassicSeenKey = "tutorial_classic_seen_v1";
        private const string ExtendedSeenKey = "tutorial_extended_seen_v1";

        [Header("Root overlay (blocks input while a step is visible)")]
        [SerializeField] private GameObject _overlayRoot;

        [Tooltip("Step panels shown to ALL modes (Classic + Extended), in order. " +
                 "Each panel plays its own animation on enable.")]
        [SerializeField] private List<GameObject> _coreSteps = new List<GameObject>();

        [Tooltip("Extra step panels shown ONLY in Extended mode, after the core steps.")]
        [SerializeField] private List<GameObject> _extendedSteps = new List<GameObject>();

        [Tooltip("Optional: tap anywhere on this catcher advances to the next step.")]
        [SerializeField] private GameObject _tapToContinueHint;

        private readonly List<GameObject> _activeSequence = new List<GameObject>();
        private int _index = -1;
        private bool _running;

        private void Start()
        {
            if (_overlayRoot != null) _overlayRoot.SetActive(false);

            bool extended = GameMode.IsExtended;
            string key = extended ? ExtendedSeenKey : ClassicSeenKey;
            if (PlayerPrefs.GetInt(key, 0) == 1)
                return; // already seen for this mode

            BuildSequence(extended);
            if (_activeSequence.Count == 0) return;

            PlayerPrefs.SetInt(key, 1);
            PlayerPrefs.Save();
            BeginTutorial();
        }

        private void BuildSequence(bool extended)
        {
            _activeSequence.Clear();
            foreach (var s in _coreSteps)
                if (s != null) _activeSequence.Add(s);
            if (extended)
                foreach (var s in _extendedSteps)
                    if (s != null) _activeSequence.Add(s);
        }

        private void BeginTutorial()
        {
            _running = true;
            if (_overlayRoot != null) _overlayRoot.SetActive(true);
            if (_tapToContinueHint != null) _tapToContinueHint.SetActive(true);
            foreach (var s in _activeSequence) s.SetActive(false);
            _index = -1;
            Next();
        }

        private void Update()
        {
            if (!_running) return;
            // Advance on any tap / click anywhere on the overlay.
            if (Input.GetMouseButtonDown(0) ||
                (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
            {
                Next();
            }
        }

        /// <summary>Advance to the next step, or finish if we're at the end. Also hookable from a UI Button.</summary>
        public void Next()
        {
            if (!_running) return;

            if (_index >= 0 && _index < _activeSequence.Count)
                _activeSequence[_index].SetActive(false);

            _index++;
            if (_index >= _activeSequence.Count)
            {
                Finish();
                return;
            }

            // Enabling the panel triggers its own entrance animation (Animator/tween in scene).
            _activeSequence[_index].SetActive(true);
        }

        private void Finish()
        {
            _running = false;
            if (_tapToContinueHint != null) _tapToContinueHint.SetActive(false);
            if (_overlayRoot != null) _overlayRoot.SetActive(false);
        }

#if UNITY_EDITOR
        /// <summary>Editor helper: wipe seen-flags so the tutorial shows again on next play.</summary>
        [ContextMenu("Reset Tutorial Seen Flags")]
        private void ResetSeenFlags()
        {
            PlayerPrefs.DeleteKey(ClassicSeenKey);
            PlayerPrefs.DeleteKey(ExtendedSeenKey);
            PlayerPrefs.Save();
            Debug.Log("[Tutorial] Seen flags reset.");
        }
#endif
    }
}
