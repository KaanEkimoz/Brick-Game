using System;
using System.Collections.Generic;
using Gameplay;
using InGame;
using UnityEngine;

namespace Tutorial
{
    /// <summary>
    /// Interactive overlay tutorial. Each step shows a panel and waits for a specific
    /// player gesture (move / rotate / hard drop / soft drop) before advancing. Info-only
    /// steps use <see cref="TutorialGesture.Tap"/> and advance on any tap. Seen-state is
    /// persisted in PlayerPrefs per mode; the <see cref="AlwaysShow"/> flag bypasses
    /// that for testing.
    ///
    /// Wiring: attach to a Canvas; populate the core and (optionally) extended step lists
    /// in the inspector. Each entry pairs a step panel GameObject with the gesture that
    /// completes it. The controller hooks itself into the StartCanvas Play buttons at
    /// runtime so the tutorial fires AFTER GameMode is set and StartGame runs.
    /// </summary>
    public class TutorialController : MonoBehaviour
    {
        public enum TutorialGesture
        {
            Tap,        // advance on any tap (info-only steps)
            MoveLR,     // advance when player moves the piece left or right
            Rotate,     // advance on a rotate
            HardDrop,   // advance on a hard drop (fast swipe down)
            SoftDrop,   // advance on a soft drop tick (hold + drag down)
        }

        [Serializable]
        public class TutorialStep
        {
            public GameObject panel;
            public TutorialGesture requiredGesture = TutorialGesture.Tap;
        }

        private const string ClassicSeenKey = "tutorial_classic_seen_v1";
        private const string ExtendedSeenKey = "tutorial_extended_seen_v1";

        [Header("Root overlay (toggled on/off; should NOT be the Canvas itself)")]
        [SerializeField] private GameObject _overlayRoot;

        [Tooltip("Step panels shown to ALL modes (Classic + Extended), in order.")]
        [SerializeField] private List<TutorialStep> _coreSteps = new List<TutorialStep>();

        [Tooltip("Extra step panels shown ONLY in Extended mode, after the core steps.")]
        [SerializeField] private List<TutorialStep> _extendedSteps = new List<TutorialStep>();

        [Tooltip("Optional: 'Tap anywhere to continue' / 'Try the gesture' hint shown beside the panel.")]
        [SerializeField] private GameObject _tapToContinueHint;

        [Tooltip("Optional Skip button — click skips the whole tutorial (still marks it as seen so it won't reappear next run when AlwaysShow is off).")]
        [SerializeField] private GameObject _skipButton;

        [Header("Test (Editor only)")]
#if UNITY_EDITOR
        [Tooltip("EDITOR ONLY — when on, the tutorial shows on EVERY Play (PlayerPrefs seen-flag ignored) so you can test it. " +
                 "This field is compiled OUT of device builds, so it can never accidentally ship as 'always on'. " +
                 "Device builds always behave as if this were false.")]
        [SerializeField] private bool _editorAlwaysShow = true;
#endif

        /// <summary>
        /// True only in the Unity Editor when the test toggle is on. ALWAYS false in device
        /// builds (the toggle field doesn't even exist there), so production always honors the
        /// PlayerPrefs seen-flag + returning-user detection.
        /// </summary>
        private bool AlwaysShow
        {
            get
            {
#if UNITY_EDITOR
                return _editorAlwaysShow;
#else
                return false;
#endif
            }
        }

        [Tooltip("StartCanvas Play button names. The controller adds an OnClick listener at runtime so the tutorial fires after the existing listeners (GameMode set + StartGame).")]
        [SerializeField] private string _classicPlayButtonName = "B_Play";
        [SerializeField] private string _extendedPlayButtonName = "B_PlayExtended";

        private readonly List<TutorialStep> _activeSequence = new List<TutorialStep>();
        private int _index = -1;
        private bool _running;
        private float _savedTimeScale = 1f;

        private void Awake()
        {
            // Hide every tutorial-owned element until a Play button summons it. The Controller
            // GameObject itself stays active so OnEnable / Start fire and the button hooks attach.
            if (_overlayRoot != null) _overlayRoot.SetActive(false);
            if (_tapToContinueHint != null) _tapToContinueHint.SetActive(false);
            if (_skipButton != null) _skipButton.SetActive(false);
            foreach (var s in _coreSteps) if (s != null && s.panel != null) s.panel.SetActive(false);
            foreach (var s in _extendedSteps) if (s != null && s.panel != null) s.panel.SetActive(false);

            // Returning-user check: if this device has any sign of prior Brick Game play,
            // mark both tutorial seen-flags so the update from a pre-tutorial build doesn't
            // suddenly bombard veterans with a tutorial they don't need. Only effective when
            // AlwaysShow is OFF (production).
            if (!AlwaysShow) MarkSeenIfReturningUser();
        }

        private static void MarkSeenIfReturningUser()
        {
            if (PlayerPrefs.GetInt(ClassicSeenKey, 0) == 1 && PlayerPrefs.GetInt(ExtendedSeenKey, 0) == 1)
                return; // already marked, nothing to do

            bool returning = false;

            // Signal 1 (strongest): a high score exists. ScoreController writes "highScore" /
            // "highScore_Extended" the moment the player beats 0 in either mode. This survives
            // game-over (unlike the save file, which is DELETED on game over), so it's the most
            // reliable "this person has played before" marker.
            if (PlayerPrefs.HasKey("highScore")) returning = true;
            else if (PlayerPrefs.HasKey("highScore_Extended")) returning = true;

            // Signal 2: the user touched the volume slider at some point.
            else if (PlayerPrefs.HasKey("volumeLevel")) returning = true;

            // Signal 3: an in-progress (or legacy) save file exists — a run was started.
            else
            {
                string dir = Application.persistentDataPath;
                if (System.IO.File.Exists(System.IO.Path.Combine(dir, "brickgame_save_classic.json"))) returning = true;
                else if (System.IO.File.Exists(System.IO.Path.Combine(dir, "brickgame_save_extended.json"))) returning = true;
                else if (System.IO.File.Exists(System.IO.Path.Combine(dir, "brickgame_save.json"))) returning = true; // pre-v1.2.8 legacy
            }

            if (!returning) return;

            PlayerPrefs.SetInt(ClassicSeenKey, 1);
            PlayerPrefs.SetInt(ExtendedSeenKey, 1);
            PlayerPrefs.Save();
        }

        private void OnEnable()
        {
            PiecesController.OnHorizontalMove += HandleHorizontalMove;
            PiecesController.OnRotate += HandleRotate;
            PiecesController.OnHardDrop += HandleHardDrop;
            PiecesController.OnSoftDrop += HandleSoftDrop;
        }

        private void OnDisable()
        {
            PiecesController.OnHorizontalMove -= HandleHorizontalMove;
            PiecesController.OnRotate -= HandleRotate;
            PiecesController.OnHardDrop -= HandleHardDrop;
            PiecesController.OnSoftDrop -= HandleSoftDrop;
        }

        private void Start()
        {
            HookPlayButtons();
            HookSkipButton();
        }

        private void HookSkipButton()
        {
            if (_skipButton == null) return;
            var btn = _skipButton.GetComponent<UnityEngine.UI.Button>();
            if (btn == null) return;
            btn.onClick.AddListener(SkipTutorial);
        }

        /// <summary>Dismiss the tutorial immediately. Wired up to the Skip button.</summary>
        public void SkipTutorial()
        {
            if (!_running) return;
            Finish();
        }

        private void HookPlayButtons()
        {
            HookButton(_classicPlayButtonName);
            HookButton(_extendedPlayButtonName);
        }

        private void HookButton(string buttonName)
        {
            if (string.IsNullOrEmpty(buttonName)) return;
            var go = GameObject.Find(buttonName);
            if (go == null) return;
            var btn = go.GetComponent<UnityEngine.UI.Button>();
            if (btn == null) return;
            // Unity invokes OnClick listeners in registration order, so this fires AFTER
            // the inspector-bound listeners (which set GameMode.IsExtended + start the game).
            btn.onClick.AddListener(ShowForCurrentMode);
        }

        /// <summary>Public entry: show the tutorial for whatever mode GameMode is currently in.</summary>
        public void ShowForCurrentMode()
        {
            bool extended = GameMode.IsExtended;
            string key = extended ? ExtendedSeenKey : ClassicSeenKey;
            if (!AlwaysShow && PlayerPrefs.GetInt(key, 0) == 1)
                return;

            BuildSequence(extended);
            if (_activeSequence.Count == 0) return;

            if (!AlwaysShow)
            {
                PlayerPrefs.SetInt(key, 1);
                PlayerPrefs.Save();
            }
            BeginTutorial();
        }

        private void BuildSequence(bool extended)
        {
            _activeSequence.Clear();
            foreach (var s in _coreSteps)
                if (s != null && s.panel != null) _activeSequence.Add(s);
            if (extended)
                foreach (var s in _extendedSteps)
                    if (s != null && s.panel != null) _activeSequence.Add(s);
        }

        private void BeginTutorial()
        {
            _running = true;
            _savedTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            if (_overlayRoot != null) _overlayRoot.SetActive(true);
            if (_tapToContinueHint != null) _tapToContinueHint.SetActive(true);
            if (_skipButton != null) _skipButton.SetActive(true);
            foreach (var s in _activeSequence) s.panel.SetActive(false);
            _index = -1;
            Next();
        }

        private void Update()
        {
            if (!_running) return;
            // Tap-anywhere advance ONLY for info-only steps (gesture == Tap).
            if (CurrentGesture() != TutorialGesture.Tap) return;
            if (Input.GetMouseButtonDown(0) ||
                (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
            {
                Next();
            }
        }

        private TutorialGesture CurrentGesture()
        {
            if (_index < 0 || _index >= _activeSequence.Count) return TutorialGesture.Tap;
            return _activeSequence[_index].requiredGesture;
        }

        private void HandleHorizontalMove() { if (_running && CurrentGesture() == TutorialGesture.MoveLR) Next(); }
        private void HandleRotate()         { if (_running && CurrentGesture() == TutorialGesture.Rotate) Next(); }
        private void HandleHardDrop()       { if (_running && CurrentGesture() == TutorialGesture.HardDrop) Next(); }
        private void HandleSoftDrop()       { if (_running && CurrentGesture() == TutorialGesture.SoftDrop) Next(); }

        /// <summary>Advance to the next step, or finish if we're at the end.</summary>
        public void Next()
        {
            if (!_running) return;

            if (_index >= 0 && _index < _activeSequence.Count)
                _activeSequence[_index].panel.SetActive(false);

            _index++;
            if (_index >= _activeSequence.Count)
            {
                Finish();
                return;
            }
            _activeSequence[_index].panel.SetActive(true);
            UpdateHintText();
        }

        private void UpdateHintText()
        {
            if (_tapToContinueHint == null) return;
            var tmp = _tapToContinueHint.GetComponent<TMPro.TMP_Text>();
            if (tmp == null) return;
            tmp.text = HintForGesture(CurrentGesture());
        }

        private static string HintForGesture(TutorialGesture g)
        {
            switch (g)
            {
                case TutorialGesture.MoveLR:   return "Swipe left or right to continue";
                case TutorialGesture.Rotate:   return "Tap the screen to rotate";
                case TutorialGesture.HardDrop: return "Swipe down fast to continue";
                case TutorialGesture.SoftDrop: return "Hold and drag down to continue";
                default:                       return "Tap anywhere to continue";
            }
        }

        private void Finish()
        {
            _running = false;
            if (_tapToContinueHint != null) _tapToContinueHint.SetActive(false);
            if (_skipButton != null) _skipButton.SetActive(false);
            if (_overlayRoot != null) _overlayRoot.SetActive(false);
            // Defensively hide EVERY step panel (not just the current one) so nothing can
            // linger on screen after a Skip or after the last step completes.
            foreach (var s in _coreSteps) if (s != null && s.panel != null) s.panel.SetActive(false);
            foreach (var s in _extendedSteps) if (s != null && s.panel != null) s.panel.SetActive(false);
            Time.timeScale = _savedTimeScale;
        }

#if UNITY_EDITOR
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
