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

            [Tooltip("If > 0, the step auto-advances after this many unscaled seconds — used for info-only " +
                     "steps that should progress on their own (e.g. the 'Ability Bar' step that just highlights " +
                     "the bar for 5 s then moves on). 0 = manual advance via gesture/tap.")]
            public float autoAdvanceSeconds = 0f;

            [Tooltip("Optional: while this step is showing, pulse the alpha of this GameObject to draw the " +
                     "player's eye (e.g. blink the actual ability bar during the 'Ability Bar' step). " +
                     "Uses an attached CanvasGroup; one is added if missing.")]
            public GameObject blinkTarget;
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

        [Tooltip("Optional 'Tutorial Completed' label (needs a CanvasGroup). Shown briefly and faded out when the player finishes ALL steps — not when they Skip.")]
        [SerializeField] private GameObject _completedLabel;
        [SerializeField] private float _completedHold = 0.7f;
        [SerializeField] private float _completedFade = 1.0f;

        [Tooltip("Optional 'Tutorial Completed' content panel (lives under the shared TutorialCard, shows only on completion — not Skip). Same lightweight panel pattern as the step panels.")]
        [SerializeField] private GameObject _completedCard;
        [SerializeField] private float _completedCardFade = 2.5f;

        [Tooltip("Shared TutorialCard parent — holds Background + Outline + the sol-üst 'Tutorial' label, and contains every step panel + the completion panel as children. Shown while the tutorial is running; faded out slowly when the player completes ALL steps; hidden immediately on Skip.")]
        [SerializeField] private GameObject _tutorialCard;

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

        /// <summary>True while the tutorial overlay is showing a step that the player must satisfy.
        /// Input gates (PiecesController.MoveLeft/Right/Rotate/Hard/SoftDrop) read this to block
        /// off-script actions — e.g. no hard drop while the move-left/right step is up.</summary>
        public static bool IsActive { get; private set; }

        /// <summary>The gesture the current step is waiting for. Only meaningful when IsActive.
        /// Tap = info-only step where NO game action is allowed (player just taps to continue).</summary>
        public static TutorialGesture RequiredGesture { get; private set; } = TutorialGesture.Tap;

        /// <summary>Input-side gate: does the tutorial currently permit this gesture? True when
        /// the tutorial isn't running, or when the running step is waiting for exactly this gesture.</summary>
        public static bool IsGestureAllowed(TutorialGesture g) => !IsActive || RequiredGesture == g;
        private bool _running;
        private float _savedTimeScale = 1f;
        private ParticleSystem[] _pausedParticles;

        private void Awake()
        {
            // Hide every tutorial-owned element until a Play button summons it. The Controller
            // GameObject itself stays active so OnEnable / Start fire and the button hooks attach.
            if (_overlayRoot != null) _overlayRoot.SetActive(false);
            if (_tutorialCard != null) _tutorialCard.SetActive(false);
            if (_tapToContinueHint != null) _tapToContinueHint.SetActive(false);
            if (_skipButton != null) _skipButton.SetActive(false);
            foreach (var s in _coreSteps) if (s != null && s.panel != null) s.panel.SetActive(false);
            foreach (var s in _extendedSteps) if (s != null && s.panel != null) s.panel.SetActive(false);
            if (_completedCard != null) _completedCard.SetActive(false);

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

            // (Removed: volumeLevel signal. VolumeBootstrap.Apply runs
            // [RuntimeInitializeOnLoadMethod(BeforeSceneLoad)] on EVERY launch and seeds
            // volumeLevel = 0.5 on first install, so the key exists before this method runs.
            // That turned first-time players into "returning" users and silently suppressed
            // the tutorial for everyone. Save-file existence + highScore are sufficient.)

            // Signal 2: an in-progress (or legacy) save file exists — a run was started.
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
            Finish(false); // skipped — no "completed" banner
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
            if (extended)
            {
                // Extended mode skips the basic gesture lessons (move/rotate/soft/hard drop) —
                // the player learned those in Classic. We only introduce what's NEW: bomb piece,
                // laser piece, then the ability bar highlight + auto-complete.
                foreach (var s in _extendedSteps)
                    if (s != null && s.panel != null) _activeSequence.Add(s);
            }
            else
            {
                foreach (var s in _coreSteps)
                    if (s != null && s.panel != null) _activeSequence.Add(s);
            }
        }

        private void BeginTutorial()
        {
            _running = true;
            IsActive = true;
            _savedTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            // timeScale 0 freezes particles mid-air, which looks broken. Clear + stop them
            // for the duration of the tutorial; resume looping/ambient ones on Finish.
            FreezeParticles();
            if (_overlayRoot != null) _overlayRoot.SetActive(true);
            if (_tutorialCard != null)
            {
                _tutorialCard.SetActive(true);
                var cg = _tutorialCard.GetComponent<CanvasGroup>();
                if (cg != null) cg.alpha = 1f;
            }
            if (_tapToContinueHint != null) _tapToContinueHint.SetActive(true);
            if (_skipButton != null) _skipButton.SetActive(true);
            foreach (var s in _activeSequence) s.panel.SetActive(false);
            if (_completedCard != null) _completedCard.SetActive(false);
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
                Finish(true); // reached the end — show the "completed" banner
                return;
            }
            var step = _activeSequence[_index];
            step.panel.SetActive(true);
            RequiredGesture = step.requiredGesture;
            UpdateHintText();

            // Optional per-step behaviours: pulse a target (to draw the player's eye) and/or
            // auto-advance after a delay. Both run unscaled — tutorial freezes Time.timeScale.
            if (step.blinkTarget != null) StartCoroutine(BlinkWhileOnStep(step.blinkTarget, _index));
            if (step.autoAdvanceSeconds > 0f) StartCoroutine(AutoAdvanceWhileOnStep(step.autoAdvanceSeconds, _index));
        }

        /// <summary>Pulse the target's alpha (via a CanvasGroup, added if missing) until the
        /// step changes. Stops cleanly when the player advances past this step or the tutorial ends.</summary>
        private System.Collections.IEnumerator BlinkWhileOnStep(GameObject target, int stepIndexAtStart)
        {
            var cg = target.GetComponent<CanvasGroup>();
            if (cg == null) cg = target.AddComponent<CanvasGroup>();
            const float period = 0.6f;   // one full down-up cycle
            const float minAlpha = 0.25f;
            float t = 0f;
            while (_running && _index == stepIndexAtStart)
            {
                t += Time.unscaledDeltaTime;
                // sine pulses between minAlpha and 1.0
                float n = 0.5f * (1f + Mathf.Sin(t * Mathf.PI * 2f / period));
                cg.alpha = Mathf.Lerp(minAlpha, 1f, n);
                yield return null;
            }
            cg.alpha = 1f;
        }

        /// <summary>Wait the given unscaled seconds, then advance ONLY if we're still on the
        /// same step (player may have already advanced manually). Guards against double-advance.</summary>
        private System.Collections.IEnumerator AutoAdvanceWhileOnStep(float seconds, int stepIndexAtStart)
        {
            yield return new WaitForSecondsRealtime(seconds);
            if (_running && _index == stepIndexAtStart) Next();
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

        private void Finish(bool completed)
        {
            _running = false;
            IsActive = false;
            RequiredGesture = TutorialGesture.Tap;
            if (_tapToContinueHint != null) _tapToContinueHint.SetActive(false);
            if (_skipButton != null) _skipButton.SetActive(false);
            if (_overlayRoot != null) _overlayRoot.SetActive(false);
            // Defensively hide EVERY step panel (not just the current one) so nothing can
            // linger on screen after a Skip or after the last step completes.
            foreach (var s in _coreSteps) if (s != null && s.panel != null) s.panel.SetActive(false);
            foreach (var s in _extendedSteps) if (s != null && s.panel != null) s.panel.SetActive(false);
            Time.timeScale = _savedTimeScale;
            ResumeParticles();

            if (completed)
            {
                // Fade the card from its CURRENT state — no content swap. Swapping the active
                // child to a big white "Tutorial Completed!" title made the card flash white
                // for a frame before fading, which Kaan flagged as wrong. The green banner
                // (_completedLabel) carries the "Tutorial Completed" message instead.
                if (_completedCard != null) _completedCard.SetActive(false);
                if (_tutorialCard != null) StartCoroutine(FadeCardOutAfterHold(_tutorialCard, _completedCardFade));
                if (_completedLabel != null) StartCoroutine(FadeOutAfterHold(_completedLabel, _completedFade));
            }
            else
            {
                // Skip: tear down the card immediately, no completion content.
                if (_tutorialCard != null) _tutorialCard.SetActive(false);
                if (_completedCard != null) _completedCard.SetActive(false);
            }
        }

        /// <summary>Generic CanvasGroup-driven fade — used for the green completion banner.</summary>
        private System.Collections.IEnumerator FadeOutAfterHold(GameObject target, float fadeDuration)
        {
            var cg = target.GetComponent<CanvasGroup>();
            target.SetActive(true);
            if (cg != null) cg.alpha = 1f;
            yield return new WaitForSecondsRealtime(_completedHold);
            float t = 0f;
            float dur = Mathf.Max(0.01f, fadeDuration);
            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                if (cg != null) cg.alpha = 1f - (t / dur);
                yield return null;
            }
            if (cg != null) cg.alpha = 0f;
            target.SetActive(false);
        }

        /// <summary>Card-specific exit animation: SLIDES the card up off the top of the screen
        /// rather than fading its alpha. Two earlier attempts (CanvasGroup fade and per-element
        /// alpha fade) both produced a perceived "white wash" mid-fade — the playfield grid
        /// behind the card is brighter than the card's dark BG, so any translucent state lets
        /// the grid bleed through and read as whiteness. Slide-up keeps the card fully opaque
        /// the entire way; once it's past the canvas top edge, it deactivates. No transparency
        /// state at all → no bleed-through.</summary>
        private System.Collections.IEnumerator FadeCardOutAfterHold(GameObject card, float fadeDuration)
        {
            card.SetActive(true);
            var rt = card.GetComponent<UnityEngine.RectTransform>();

            // Ensure the parent CanvasGroup (if any) is at full alpha — earlier code may have
            // dimmed it, and we don't want a sticky half-faded state here.
            var cg = card.GetComponent<CanvasGroup>();
            if (cg != null) cg.alpha = 1f;

            UnityEngine.Vector2 startPos = rt.anchoredPosition;
            // Slide distance: card height + a comfortable margin so the bottom edge clears
            // the visible canvas before we deactivate. rt.rect.height accounts for the actual
            // rendered height regardless of stretch anchors.
            float slideDistance = rt.rect.height + 80f;
            UnityEngine.Vector2 endPos = startPos + new UnityEngine.Vector2(0f, slideDistance);

            yield return new WaitForSecondsRealtime(_completedHold);

            float t = 0f;
            float dur = Mathf.Max(0.01f, fadeDuration);
            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                float n = Mathf.Clamp01(t / dur);
                // Ease-in cubic: starts slow (graceful "lift") then accelerates off-screen.
                float k = n * n * n;
                rt.anchoredPosition = UnityEngine.Vector2.LerpUnclamped(startPos, endPos, k);
                yield return null;
            }

            // Restore the original position so the next showing starts in place.
            rt.anchoredPosition = startPos;
            card.SetActive(false);
        }

        private void FreezeParticles()
        {
            _pausedParticles = UnityEngine.Object.FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None);
            foreach (var ps in _pausedParticles)
                if (ps != null) ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        private void ResumeParticles()
        {
            if (_pausedParticles == null) return;
            foreach (var ps in _pausedParticles)
                if (ps != null && ps.main.playOnAwake) ps.Play(); // restart looping / ambient systems
            _pausedParticles = null;
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
