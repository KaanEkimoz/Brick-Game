using Board;
using UnityEngine;

namespace Extras
{
    /// <summary>
    /// Plays one animated explosion per cell along every cleared row. Mirrors the bomb
    /// explosion pattern: a single particle emitted at each cell, each particle plays the
    /// TextureSheetAnimation configured on this prefab's ParticleSystem over its lifetime.
    /// </summary>
    [RequireComponent(typeof(ParticleSystem))]
    public class LineClearEffect : MonoBehaviour
    {
        [SerializeField] private BoardController _boardController;
        [SerializeField] private float _cellSize = 1f;
        [SerializeField] private float _cellLifetime = 0.7f;
        [SerializeField] private Color _cellColor = Color.white;

        private ParticleSystem _particles;

        private void Awake()
        {
            _particles = GetComponent<ParticleSystem>();
        }

        private void OnEnable()
        {
            BoardController.OnLineCleared += Play;
        }

        private void OnDisable()
        {
            BoardController.OnLineCleared -= Play;
        }

        private void Play(int rowY)
        {
            if (_particles == null || _boardController == null) return;
            for (int x = 0; x < _boardController.gridSizeX; x++)
            {
                ParticleSystem.EmitParams p = new ParticleSystem.EmitParams();
                // Route through BoardController.CellToWorld so the cells line up with the
                // (potentially offset) playfield. Raw new Vector3(x, rowY) would always emit
                // at world origin, which after WorldOrigin moved leaves the VFX adrift.
                p.position = BoardController.CellToWorld(x, rowY);
                p.velocity = Vector3.zero;
                p.startSize = _cellSize;
                p.startLifetime = _cellLifetime;
                p.startColor = _cellColor;
                _particles.Emit(p, 1);
            }
        }

        [ContextMenu("Test Burst")]
        private void TestBurst()
        {
            if (_particles == null) _particles = GetComponent<ParticleSystem>();
            int row = _boardController != null ? _boardController.gridSizeY / 2 : 5;
            Play(row);
        }
    }
}
