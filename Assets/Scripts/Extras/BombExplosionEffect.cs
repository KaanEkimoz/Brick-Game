using Gameplay;
using UnityEngine;

namespace Extras
{
    [RequireComponent(typeof(ParticleSystem))]
    public class BombExplosionEffect : MonoBehaviour
    {
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
            AbilityExecutor.OnBombExploded += Play;
        }

        private void OnDisable()
        {
            AbilityExecutor.OnBombExploded -= Play;
        }

        private void Play(Vector2Int anchor)
        {
            if (_particles == null) return;

            // 3x3 grid: one explosion sprite per cell around the anchor.
            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    ParticleSystem.EmitParams p = new ParticleSystem.EmitParams();
                    p.position = new Vector3(anchor.x + dx, anchor.y + dy, 0f);
                    p.velocity = Vector3.zero;
                    p.startSize = _cellSize;
                    p.startLifetime = _cellLifetime;
                    p.startColor = _cellColor;
                    _particles.Emit(p, 1);
                }
            }
        }

        [ContextMenu("Test Burst")]
        private void TestBurst()
        {
            if (_particles == null) _particles = GetComponent<ParticleSystem>();
            Play(Vector2Int.zero);
        }
    }
}
