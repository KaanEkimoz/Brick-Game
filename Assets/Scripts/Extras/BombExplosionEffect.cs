using Gameplay;
using UnityEngine;

namespace Extras
{
    /// <summary>
    /// Plays a two-layer explosion at the bomb anchor: one big short-lived flash that grows
    /// then shrinks, plus a ring of small sparks shooting outward. Drives a single
    /// ParticleSystem via Emit() with custom EmitParams so we don't need sub-emitters.
    /// </summary>
    [RequireComponent(typeof(ParticleSystem))]
    public class BombExplosionEffect : MonoBehaviour
    {
        [Header("Flash Core (single big particle)")]
        [SerializeField] private float _flashSize = 4f;
        [SerializeField] private float _flashLifetime = 0.4f;
        [SerializeField] private Color _flashColor = new Color(1f, 0.95f, 0.4f, 1f);

        [Header("Sparks Ring")]
        [SerializeField] private int _sparkCount = 22;
        [SerializeField] private float _sparkMinSpeed = 2.5f;
        [SerializeField] private float _sparkMaxSpeed = 6f;
        [SerializeField] private float _sparkMinSize = 0.18f;
        [SerializeField] private float _sparkMaxSize = 0.45f;
        [SerializeField] private float _sparkMinLifetime = 0.3f;
        [SerializeField] private float _sparkMaxLifetime = 0.55f;
        [SerializeField] private Color _sparkColor = new Color(1f, 0.55f, 0.15f, 1f);

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
            Vector3 worldPos = new Vector3(anchor.x, anchor.y, 0f);

            // Big core flash
            var flash = new ParticleSystem.EmitParams
            {
                position = worldPos,
                velocity = Vector3.zero,
                startSize = _flashSize,
                startLifetime = _flashLifetime,
                startColor = _flashColor,
            };
            _particles.Emit(flash, 1);

            // Radial sparks
            for (int i = 0; i < _sparkCount; i++)
            {
                float angle = (i / (float)_sparkCount) * Mathf.PI * 2f
                              + Random.Range(-0.15f, 0.15f);
                float speed = Random.Range(_sparkMinSpeed, _sparkMaxSpeed);
                var spark = new ParticleSystem.EmitParams
                {
                    position = worldPos,
                    velocity = new Vector3(Mathf.Cos(angle) * speed, Mathf.Sin(angle) * speed, 0f),
                    startSize = Random.Range(_sparkMinSize, _sparkMaxSize),
                    startLifetime = Random.Range(_sparkMinLifetime, _sparkMaxLifetime),
                    startColor = _sparkColor,
                };
                _particles.Emit(spark, 1);
            }
        }
    }
}
