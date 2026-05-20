using UnityEngine;

namespace Gameplay
{
    /// <summary>
    /// Tracks the firing axis of a Laser ability piece. The piece is a single tile, so
    /// rotation input toggles this flag and spins the sprite renderer 90° instead of
    /// moving any tiles around the board. <see cref="AbilityExecutor"/> reads
    /// <see cref="IsHorizontal"/> at settle time to decide between row and column clear.
    /// </summary>
    public class LaserOrientation : MonoBehaviour
    {
        public enum Axis { Horizontal, Vertical }

        [SerializeField] private Axis _axis = Axis.Horizontal;

        public Axis CurrentAxis => _axis;
        public bool IsHorizontal => _axis == Axis.Horizontal;

        /// <summary>
        /// Flips the orientation and rotates every child SpriteRenderer transform 90°
        /// so the visual matches the new axis. Called by PieceRotation when the player
        /// rotates a single-tile laser piece.
        /// </summary>
        public void Toggle()
        {
            _axis = _axis == Axis.Horizontal ? Axis.Vertical : Axis.Horizontal;
            ApplyVisual();
        }

        private void OnEnable()
        {
            ApplyVisual();
        }

        private void ApplyVisual()
        {
            float z = _axis == Axis.Horizontal ? 0f : 90f;
            foreach (SpriteRenderer sr in GetComponentsInChildren<SpriteRenderer>(true))
            {
                Vector3 e = sr.transform.localEulerAngles;
                sr.transform.localEulerAngles = new Vector3(e.x, e.y, z);
            }
        }
    }
}
