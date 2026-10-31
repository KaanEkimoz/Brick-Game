using UnityEngine;

namespace Gameplay
{
    /// <summary>
    /// Tracks the firing axis of a charged Laser piece. Every successful rotation toggles
    /// this flag and spins the charged tile's sprite 90°. <see cref="AbilityExecutor"/> reads
    /// <see cref="IsHorizontal"/> at settle time to decide between row and column clear.
    /// </summary>
    public class LaserOrientation : MonoBehaviour
    {
        public enum Axis { Horizontal, Vertical }

        [SerializeField] private Axis _axis = Axis.Horizontal;

        private SpriteRenderer _visual;

        public Axis CurrentAxis => _axis;
        public bool IsHorizontal => _axis == Axis.Horizontal;

        /// <summary>
        /// Flips the orientation and rotates the charged tile's sprite 90° so the visual
        /// matches the new axis. Called by PieceRotation after a successful rotation.
        /// </summary>
        public void Toggle()
        {
            _axis = _axis == Axis.Horizontal ? Axis.Vertical : Axis.Horizontal;
            ApplyVisual();
        }

        /// <summary>Sets the single tile whose sprite shows the firing axis.</summary>
        public void BindVisual(SpriteRenderer visual)
        {
            _visual = visual;
            ApplyVisual();
        }

        private void ApplyVisual()
        {
            if (_visual == null) return;
            float z = _axis == Axis.Horizontal ? 0f : 90f;
            Vector3 e = _visual.transform.localEulerAngles;
            _visual.transform.localEulerAngles = new Vector3(e.x, e.y, z);
        }
    }
}
