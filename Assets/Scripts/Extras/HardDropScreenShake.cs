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

        [Tooltip("Skip the shake if the piece fell fewer cells than this. 0 = shake on every hard " +
                 "drop trigger, even a no-op drop on a piece that was already on the floor.")]
        [SerializeField] private int _minimumFallCells = 0;

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
            // Diagnostic log so we can see in the editor console whether the event
            // is reaching us at all — separates "event never fires" from "event fires
            // but the shake call did nothing".
            Debug.Log($"[HardDropScreenShake] HandleHardDrop fallDistance={fallDistance} ss={(_screenShake != null ? _screenShake.name : "<null>")}");
            if (_screenShake == null) return;
            if (fallDistance < _minimumFallCells) return;
            _screenShake.ShakeLight();
        }
    }
}
