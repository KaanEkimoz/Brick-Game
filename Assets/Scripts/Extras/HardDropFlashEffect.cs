using Piece;
using UnityEngine;

namespace Extras
{
    /// <summary>
    /// Flashes a bright white particle at each tile of a piece that just hard-dropped.
    /// The alpha curve baked on the particle system flickers (1 → 0 → 1 → 0) so each
    /// tile visibly blinks twice and then fades.
    /// </summary>
    [RequireComponent(typeof(ParticleSystem))]
    public class HardDropFlashEffect : MonoBehaviour
    {
        [SerializeField] private float _flashSize = 1f;
        [SerializeField] private float _flashLifetime = 0.3f;
        [SerializeField] private Color _flashColor = Color.white;

        private ParticleSystem _particles;

        private void Awake()
        {
            _particles = GetComponent<ParticleSystem>();
        }

        private void OnEnable()
        {
            PieceMovement.OnHardDrop += Play;
        }

        private void OnDisable()
        {
            PieceMovement.OnHardDrop -= Play;
        }

        private void Play(Vector2Int[] tiles)
        {
            if (_particles == null || tiles == null) return;
            for (int i = 0; i < tiles.Length; i++)
            {
                ParticleSystem.EmitParams p = new ParticleSystem.EmitParams();
                p.position = new Vector3(tiles[i].x, tiles[i].y, 0f);
                p.velocity = Vector3.zero;
                p.startSize = _flashSize;
                p.startLifetime = _flashLifetime;
                p.startColor = _flashColor;
                _particles.Emit(p, 1);
            }
        }

        [ContextMenu("Test Burst")]
        private void TestBurst()
        {
            if (_particles == null) _particles = GetComponent<ParticleSystem>();
            Play(new[] { new Vector2Int(4, 5), new Vector2Int(5, 5), new Vector2Int(6, 5), new Vector2Int(5, 6) });
        }
    }
}
