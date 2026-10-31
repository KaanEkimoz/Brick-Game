using System.Collections.Generic;
using InGame;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace InputSystem
{
    /// <summary>
    /// Touch-driven input layer for the Brick Game gameplay scene.
    ///
    /// Gestures (single finger, screen-wide). The gesture's AXIS is decided ONCE, the moment the
    /// finger leaves a small dead zone, from the cumulative travel so far — not per-frame. A single
    /// axis per touch is what kills both classic touch bugs: a horizontal swipe that arcs can no
    /// longer be killed mid-swipe by a stray vertical frame, and a downward hard-drop flick can no
    /// longer sneak in a sideways step on its first diagonal frames.
    ///   - Tap (short, inside dead zone) ........ <see cref="PiecesController.RotateClockwise"/>
    ///   - Horizontal axis (drag/swipe) ......... <see cref="PiecesController.MoveLeft"/> /
    ///                                             <see cref="PiecesController.MoveRight"/>, cumulative
    ///                                             horizontal delta quantised to one cell per step.
    ///                                             May also soft-drop if the finger later drags
    ///                                             clearly below its peak (optional, see field).
    ///   - Vertical axis, held drag ............. <see cref="PiecesController.MoveDown"/> at a tick
    ///                                             interval scaled by how far below peak the finger is.
    ///   - Vertical axis, fast flick on lift .... <see cref="PiecesController.SendPieceToFloor"/>
    ///
    /// Touches that begin on top of an interactive UI element (Pause, ability buttons, menus) are
    /// ignored so the existing UI keeps working untouched.
    /// </summary>
    public class TouchInput : MonoBehaviour
    {
        private enum Axis { Undecided, Horizontal, Vertical }

        [Header("Horizontal step")]
        [Tooltip("Screen pixels the finger travels horizontally per one-cell step. Auto-calibrated " +
                 "from Screen.width on Awake. LOWER = more sensitive (piece tracks the finger more " +
                 "eagerly); HIGHER = calmer. Tune on-device for feel.")]
        [SerializeField] private float _horizontalStepPixels = 40f;

        [Tooltip("Divisor applied to Screen.width on Awake to derive the step. width/13 ≈ a touch " +
                 "more responsive than one visual cell. Smaller divisor = larger step = less sensitive.")]
        [SerializeField] private float _horizontalStepScreenDivisor = 13f;

        [Header("Axis decision (dead zone)")]
        [Tooltip("Finger must leave this radius (px) from the touch start before any move/drop fires. " +
                 "At the moment it crosses, the gesture axis is locked for the rest of the touch from " +
                 "the cumulative dx vs dy. Also doubles as the tap radius.")]
        [SerializeField] private float _tapMaxRadius = 24f;

        [Header("Tap (rotate)")]
        [SerializeField] private float _tapMaxDuration = 0.25f;

        [Header("Hard drop flick (vertical axis only)")]
        [Tooltip("Minimum downward velocity (px/s) required on lift-off to count as a hard drop.")]
        [SerializeField] private float _hardDropMinVelocity = 2200f;

        [Tooltip("Maximum touch duration (seconds) for a hard drop. Anything longer is a controlled " +
                 "soft drop instead, no matter how far the finger moved.")]
        [SerializeField] private float _hardDropMaxDuration = 0.30f;

        [Tooltip("Minimum vertical distance (px) the finger must travel for a hard drop.")]
        [SerializeField] private float _hardDropMinDistance = 120f;

        [Header("Soft drop")]
        [Tooltip("How far the finger must drag below its peak Y before soft drop starts firing.")]
        [SerializeField] private float _softDropActivatePixels = 40f;

        [Tooltip("Allow soft drop while the touch is committed to the HORIZONTAL axis, when the finger " +
                 "later drags clearly below its peak. Lets a single touch move sideways THEN push down. " +
                 "Turn off for strictly one-intent-per-touch controls.")]
        [SerializeField] private bool _softDropInHorizontalAxis = true;

        [Tooltip("In the horizontal axis, soft drop fires only when drop-below-peak exceeds horizontal " +
                 "travel × this factor (keeps a pure sideways drag from drizzling drops). Ignored on the " +
                 "vertical axis, which is already committed to dropping.")]
        [SerializeField] private float _softDropVerticalDominance = 1.4f;

        [Tooltip("Slowest soft-drop tick interval (seconds). Held just past activation drops this often.")]
        [SerializeField] private float _softDropSlowInterval = 0.15f;

        [Tooltip("Fastest soft-drop tick interval (seconds). Held far below activation drops this often.")]
        [SerializeField] private float _softDropFastInterval = 0.039f;

        [Tooltip("Drag distance below activation (pixels) that maps to the fastest tick.")]
        [SerializeField] private float _softDropFullSpeedPixels = 300f;

        // === Touch tracking state ===
        private bool _touching;
        private int _trackedFingerId = -1;
        private Vector2 _touchStart;
        private Vector2 _lastPos;
        private float _touchStartTime;
        private float _accumulatedHorizontalDelta;
        private float _peakY;
        private float _softDropNextTickTime;
        private bool _hardDropFired;
        private bool _movedBeyondTapRadius;
        private Axis _axis;

        private void Awake()
        {
            // Calibrate cell step from screen size so DPI / aspect doesn't change the feel.
            float divisor = Mathf.Max(6f, _horizontalStepScreenDivisor);
            _horizontalStepPixels = Mathf.Max(24f, Screen.width / divisor);
        }

        private void Update()
        {
            // No fingers down — close out an in-flight touch if needed and bail.
            if (Input.touchCount == 0)
            {
                if (_touching) EndTouch();
                return;
            }

            // Find the touch we should react to. If we aren't tracking one yet, claim the first
            // Began that didn't start on UI. Otherwise stay locked to the finger we picked up.
            Touch t = default;
            bool found = false;
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch ti = Input.GetTouch(i);

                if (_touching && ti.fingerId == _trackedFingerId)
                {
                    t = ti; found = true;
                    break;
                }

                if (!_touching && ti.phase == TouchPhase.Began)
                {
                    if (IsOverInteractiveUI(ti.position)) continue;
                    BeginTouch(ti);
                    t = ti; found = true;
                    break;
                }
            }

            if (!found) return;

            switch (t.phase)
            {
                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    HandleMove(t);
                    break;
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    HandleEnd(t);
                    break;
            }
        }

        // Reused list so we don't allocate per finger per frame.
        private static readonly List<RaycastResult> _raycastBuffer = new List<RaycastResult>(16);

        // Returns true ONLY when the touch lands on an actually interactive UI element
        // (Selectable = Button / Toggle / Slider / Dropdown / InputField), not on plain Image
        // backgrounds or TMP text. Without this filter, the board's grid frame, HUD bar and
        // side-panel images would swallow every touch and gameplay input would never fire.
        private static bool IsOverInteractiveUI(Vector2 screenPos)
        {
            if (EventSystem.current == null) return false;

            var ped = new PointerEventData(EventSystem.current) { position = screenPos };
            _raycastBuffer.Clear();
            EventSystem.current.RaycastAll(ped, _raycastBuffer);
            for (int i = 0; i < _raycastBuffer.Count; i++)
            {
                var go = _raycastBuffer[i].gameObject;
                if (go == null) continue;
                if (go.GetComponentInParent<Selectable>() != null) return true;
            }
            return false;
        }

        private void BeginTouch(Touch t)
        {
            _touching = true;
            _trackedFingerId = t.fingerId;
            _touchStart = t.position;
            _lastPos = t.position;
            _touchStartTime = Time.unscaledTime;
            _accumulatedHorizontalDelta = 0f;
            _peakY = t.position.y;
            _softDropNextTickTime = 0f;
            _hardDropFired = false;
            _movedBeyondTapRadius = false;
            _axis = Axis.Undecided;
        }

        private void HandleMove(Touch t)
        {
            Vector2 delta = t.position - _lastPos;
            _lastPos = t.position;

            // Track the highest point the finger has reached. Soft drop measures from this
            // reference so the player can wiggle freely as long as they're moving downward.
            if (t.position.y > _peakY) _peakY = t.position.y;

            // Inside the dead zone nothing fires — this is still a potential tap, and we have too
            // little travel to tell horizontal intent from vertical intent reliably.
            if (!_movedBeyondTapRadius)
            {
                if (Vector2.Distance(t.position, _touchStart) <= _tapMaxRadius) return;
                _movedBeyondTapRadius = true;
            }

            // Decide the axis ONCE, from the cumulative displacement at the moment we leave the dead
            // zone. ~24px of accumulated travel reflects real intent and is immune to single-frame
            // jitter, so a horizontal swipe commits horizontal and a downward flick commits vertical.
            if (_axis == Axis.Undecided)
            {
                float totalDX = Mathf.Abs(t.position.x - _touchStart.x);
                float totalDY = Mathf.Abs(t.position.y - _touchStart.y);
                _axis = totalDX >= totalDY ? Axis.Horizontal : Axis.Vertical;
                // Start the horizontal accumulator fresh from the decision point so the dead-zone
                // travel doesn't instantly jump the piece a cell on commit.
                _accumulatedHorizontalDelta = 0f;
            }

            if (_axis == Axis.Horizontal)
            {
                // Quantise horizontal motion into one-cell steps. Works for both quick swipes and
                // slow continuous drags since we accumulate raw delta either way.
                _accumulatedHorizontalDelta += delta.x;
                while (_accumulatedHorizontalDelta >= _horizontalStepPixels)
                {
                    MoveRight();
                    _accumulatedHorizontalDelta -= _horizontalStepPixels;
                }
                while (_accumulatedHorizontalDelta <= -_horizontalStepPixels)
                {
                    MoveLeft();
                    _accumulatedHorizontalDelta += _horizontalStepPixels;
                }

                // Optional combined gesture: allow soft drop even on a horizontal-committed touch,
                // but only when the finger is clearly dragging down relative to its sideways travel.
                if (_softDropInHorizontalAxis)
                {
                    float belowPeakH = _peakY - t.position.y;
                    float horizTravel = Mathf.Abs(t.position.x - _touchStart.x);
                    if (belowPeakH > horizTravel * _softDropVerticalDominance)
                        TrySoftDrop(belowPeakH);
                }
            }
            else // Axis.Vertical — committed to dropping; never step sideways.
            {
                float belowPeak = _peakY - t.position.y;
                TrySoftDrop(belowPeak);
            }
        }

        // Fires a soft-drop tick (at most one per scaled interval) when the finger is far enough
        // below its peak. The interval scales from slow (just past activation) to fast (deep drag).
        private void TrySoftDrop(float belowPeak)
        {
            if (_hardDropFired || belowPeak < _softDropActivatePixels) return;
            if (Time.unscaledTime < _softDropNextTickTime) return;

            SoftDropTick();
            float range = Mathf.Max(1f, _softDropFullSpeedPixels - _softDropActivatePixels);
            float normalised = Mathf.Clamp01((belowPeak - _softDropActivatePixels) / range);
            float interval = Mathf.Lerp(_softDropSlowInterval, _softDropFastInterval, normalised);
            _softDropNextTickTime = Time.unscaledTime + interval;
        }

        private void HandleEnd(Touch t)
        {
            float duration = Time.unscaledTime - _touchStartTime;
            Vector2 totalDelta = t.position - _touchStart;
            float verticalDistance = -totalDelta.y;     // positive when finger ended below the start
            float verticalVelocity = verticalDistance / Mathf.Max(0.001f, duration);

            // Hard drop only on a touch that committed to the vertical axis — a horizontal or
            // undecided touch can never slam the piece, which is what kept producing side-drift.
            bool wasFastDownSwipe =
                _axis == Axis.Vertical &&
                duration <= _hardDropMaxDuration &&
                verticalDistance >= _hardDropMinDistance &&
                verticalVelocity >= _hardDropMinVelocity &&
                Mathf.Abs(totalDelta.x) < verticalDistance; // mostly vertical, not diagonal

            if (!_hardDropFired && wasFastDownSwipe)
            {
                HardDrop();
                _hardDropFired = true;
            }
            else if (!_movedBeyondTapRadius && duration <= _tapMaxDuration)
            {
                RotateCW();
            }

            EndTouch();
        }

        private void EndTouch()
        {
            _touching = false;
            _trackedFingerId = -1;
        }

        // === Gameplay bridges — null-safe so input during respawn / game over is a no-op. ===

        private static void MoveLeft()
        {
            var c = PiecesController.Instance;
            if (c == null || PiecesController.CurPiece == null) return;
            c.MoveLeft();
        }

        private static void MoveRight()
        {
            var c = PiecesController.Instance;
            if (c == null || PiecesController.CurPiece == null) return;
            c.MoveRight();
        }

        private static void RotateCW()
        {
            var c = PiecesController.Instance;
            if (c == null || PiecesController.CurPiece == null) return;
            c.RotateClockwise();
        }

        private static void SoftDropTick()
        {
            var c = PiecesController.Instance;
            if (c == null || PiecesController.CurPiece == null) return;
            c.MoveDown();
        }

        private static void HardDrop()
        {
            var c = PiecesController.Instance;
            if (c == null || PiecesController.CurPiece == null) return;
            c.SendPieceToFloor();
        }
    }
}
