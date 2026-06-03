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
    /// Gestures (single finger, screen-wide):
    ///   - Tap (short, small radius) ............ <see cref="PiecesController.RotateClockwise"/>
    ///   - Horizontal drag / swipe .............. <see cref="PiecesController.MoveLeft"/> /
    ///                                             <see cref="PiecesController.MoveRight"/>
    ///                                             Cumulative horizontal delta is quantised to
    ///                                             one cell width per step, so both a quick
    ///                                             flick and a slow continuous drag feel the same.
    ///   - Fast downward swipe (short + high vy)  <see cref="PiecesController.SendPieceToFloor"/>
    ///   - Held downward drag ................... <see cref="PiecesController.MoveDown"/> at a
    ///                                             tick interval driven by how far the finger has
    ///                                             dragged below its highest reached Y. Far below
    ///                                             peak = fast drop, just past activation = slow.
    ///
    /// Touches that begin on top of a UI element (Pause button, ability button, menus) are
    /// ignored so the existing UI keeps working untouched.
    /// </summary>
    public class TouchInput : MonoBehaviour
    {
        [Header("Horizontal step")]
        [Tooltip("Screen pixels the finger has to travel horizontally before the piece steps one " +
                 "cell. Auto-calibrated from Screen.width on Awake so DPI / aspect doesn't break it.")]
        [SerializeField] private float _horizontalStepPixels = 40f;

        [Tooltip("Per-frame vertical lock — when the frame's |dy| exceeds |dx| × this factor, " +
                 "the horizontal accumulator is ignored AND drained, so a thumb sliding mostly " +
                 "downward (e.g. a hard-drop swipe) can't sneak in a sideways step. Higher = " +
                 "stricter (rarer accidental side-step on vertical gestures). 0 disables the lock.")]
        [SerializeField] private float _horizontalLockRatio = 2.0f;

        [Header("Tap (rotate)")]
        [SerializeField] private float _tapMaxDuration = 0.25f;
        [SerializeField] private float _tapMaxRadius = 24f;

        [Header("Hard drop swipe")]
        [Tooltip("Minimum downward velocity (px/s) required on lift-off to count as a hard drop.")]
        [SerializeField] private float _hardDropMinVelocity = 2200f;

        [Tooltip("Maximum touch duration (seconds) for a hard drop. Anything longer is treated " +
                 "as a controlled soft drop instead, no matter how far the finger moved.")]
        [SerializeField] private float _hardDropMaxDuration = 0.30f;

        [Tooltip("Minimum vertical distance the finger must travel for a hard drop.")]
        [SerializeField] private float _hardDropMinDistance = 120f;

        [Header("Soft drop")]
        [Tooltip("How far the finger must drag below its peak Y before soft drop starts firing.")]
        [SerializeField] private float _softDropActivatePixels = 80f;

        [Tooltip("Soft drop fires only when the touch is clearly more vertical than horizontal. "
               + "Multiplier applied to the horizontal travel — drop-below-peak must exceed "
               + "horizontalTravel × this factor. Higher = stricter (left/right drags less likely "
               + "to accidentally fire soft drop). 1.0 = equal vertical/horizontal threshold.")]
        [SerializeField] private float _softDropVerticalDominance = 1.4f;

        [Tooltip("Slowest soft-drop tick interval (seconds). Held just past activation drops " +
                 "this often.")]
        [SerializeField] private float _softDropSlowInterval = 0.234f;

        [Tooltip("Fastest soft-drop tick interval (seconds). Held far below activation drops " +
                 "this often.")]
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

        private void Awake()
        {
            // Calibrate cell width from screen size. The board is ~10 cells across the playfield
            // but a slightly tighter step than Screen.width / 10 feels more responsive on a phone.
            _horizontalStepPixels = Mathf.Max(28f, Screen.width / 12f);
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
        }

        private void HandleMove(Touch t)
        {
            Vector2 delta = t.position - _lastPos;
            _lastPos = t.position;

            // Track the highest point the finger has reached. Soft drop measures from this
            // reference so the player can wiggle freely as long as they're moving downward.
            if (t.position.y > _peakY) _peakY = t.position.y;

            // Once the finger moves past the tap radius we know this is a drag, not a tap.
            if (!_movedBeyondTapRadius && Vector2.Distance(t.position, _touchStart) > _tapMaxRadius)
                _movedBeyondTapRadius = true;

            // Vertical-lock: if THIS frame is sharply vertical (|dy| dominates |dx| by the
            // configured ratio), do NOT integrate dx into the horizontal step accumulator. Also
            // drain the accumulator slightly so a few prior near-step deltas don't carry over and
            // fire a phantom side-step during a hard-drop swipe.
            float absDX = Mathf.Abs(delta.x);
            float absDY = Mathf.Abs(delta.y);
            bool verticalLocked = _horizontalLockRatio > 0f && absDY > absDX * _horizontalLockRatio;

            if (verticalLocked)
            {
                _accumulatedHorizontalDelta *= 0.5f; // soft drain
            }
            else
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
            }

            // Soft drop while the finger is below its peak Y by at least the activation amount
            // AND the gesture is more vertical than horizontal — otherwise a left/right drag that
            // bobs the thumb down a little would falsely trigger drops.
            float belowPeak = _peakY - t.position.y;
            float horizontalTravel = Mathf.Abs(t.position.x - _touchStart.x);
            bool verticalDominant = belowPeak > horizontalTravel * _softDropVerticalDominance;
            if (!_hardDropFired && verticalDominant && belowPeak >= _softDropActivatePixels)
            {
                if (Time.unscaledTime >= _softDropNextTickTime)
                {
                    SoftDropTick();
                    float range = Mathf.Max(1f, _softDropFullSpeedPixels - _softDropActivatePixels);
                    float normalised = Mathf.Clamp01((belowPeak - _softDropActivatePixels) / range);
                    float interval = Mathf.Lerp(_softDropSlowInterval, _softDropFastInterval, normalised);
                    _softDropNextTickTime = Time.unscaledTime + interval;
                }
            }
        }

        private void HandleEnd(Touch t)
        {
            float duration = Time.unscaledTime - _touchStartTime;
            Vector2 totalDelta = t.position - _touchStart;
            float verticalDistance = -totalDelta.y;     // positive when finger ended below the start
            float verticalVelocity = verticalDistance / Mathf.Max(0.001f, duration);

            bool wasFastDownSwipe =
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
