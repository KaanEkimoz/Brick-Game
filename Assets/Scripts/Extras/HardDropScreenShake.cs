using Piece;
using Tiles;
using UnityEngine;

namespace Extras
{
    /// <summary>
    /// Triggers a very soft camera shake whenever a piece hard-drops onto the board.
    /// Listens to PieceMovement.OnHardDrop so passive falling never shakes — only
    /// the deliberate slam. Magnitude is fixed to ScreenShakeEffect.ShakeLight()
    /// so it stays unobtrusive even when chaining quick drops.
    /// </summary>
    [RequireComponent(typeof(ScreenShakeEffect))]
    public class HardDropScreenShake : MonoBehaviour
    {
        [SerializeField] private ScreenShakeEffect _screenShake;

        [Tooltip("Skip the shake if the piece didn't actually move (a no-op hard drop on a piece " +
                 "that was already touching the floor would otherwise still shake).")]
        [SerializeField] private int _minimumFallCells = 1;

        private void Reset()
        {
            _screenShake = GetComponent<ScreenShakeEffect>();
        }

        private void Awake()
        {
            if (_screenShake == null) _screenShake = GetComponent<ScreenShakeEffect>();
        }

        private void OnEnable()
        {
            PieceMovement.OnHardDrop += HandleHardDrop;
        }

        private void OnDisable()
        {
            PieceMovement.OnHardDrop -= HandleHardDrop;
        }

        private void HandleHardDrop(TileController[] landed, int fallDistance)
        {
            if (_screenShake == null) return;
            if (fallDistance < _minimumFallCells) return;
            _screenShake.ShakeLight();
        }
    }
}
