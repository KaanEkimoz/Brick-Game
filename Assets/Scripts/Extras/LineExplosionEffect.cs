using Board;
using Gameplay;
using UnityEngine;

namespace Extras
{
    [RequireComponent(typeof(ParticleSystem))]
    public class LineExplosionEffect : MonoBehaviour
    {
        public enum ExplosionAxis { Horizontal, Vertical }

        [SerializeField] private ExplosionAxis _axis = ExplosionAxis.Horizontal;
        [SerializeField] private BoardController _boardController;

        [Header("Per-cell flame")]
        [SerializeField] private int _flamesPerCell = 2;
        [SerializeField] private float _flameMinSize = 0.85f;
        [SerializeField] private float _flameMaxSize = 1.35f;
        [SerializeField] private float _flameMinLifetime = 0.32f;
        [SerializeField] private float _flameMaxLifetime = 0.55f;
        [SerializeField] private Color _flameStartColor = new Color(1f, 0.95f, 0.55f, 1f);

        [Header("Sparks per cell")]
        [SerializeField] private int _sparksPerCell = 3;
        [SerializeField] private float _sparkSpeed = 2.5f;
        [SerializeField] private float _sparkMinSize = 0.18f;
        [SerializeField] private float _sparkMaxSize = 0.4f;
        [SerializeField] private float _sparkMinLifetime = 0.25f;
        [SerializeField] private float _sparkMaxLifetime = 0.5f;
        [SerializeField] private Color _sparkColor = new Color(1f, 0.65f, 0.2f, 1f);

        [Header("Anchor core flash (where the ability piece landed)")]
        [SerializeField] private float _coreSize = 1.4f;
        [SerializeField] private float _coreLifetime = 0.35f;
        [SerializeField] private Color _coreColor = new Color(1f, 1f, 0.9f, 1f);

        private ParticleSystem _particles;

        private void Awake()
        {
            _particles = GetComponent<ParticleSystem>();
        }

        private void OnEnable()
        {
            if (_axis == ExplosionAxis.Horizontal)
                AbilityExecutor.OnRowExploded += PlayRow;
            else
                AbilityExecutor.OnColumnExploded += PlayColumn;
        }

        private void OnDisable()
        {
            if (_axis == ExplosionAxis.Horizontal)
                AbilityExecutor.OnRowExploded -= PlayRow;
            else
                AbilityExecutor.OnColumnExploded -= PlayColumn;
        }

        private void PlayRow(Vector2Int anchor)
        {
            if (_boardController == null || _particles == null) return;
            int len = _boardController.gridSizeX;
            EmitCore(anchor.x, anchor.y);
            for (int x = 0; x < len; x++)
                EmitCell(x, anchor.y, perpendicular: true);
        }

        private void PlayColumn(Vector2Int anchor)
        {
            if (_boardController == null || _particles == null) return;
            int len = _boardController.gridSizeY;
            EmitCore(anchor.x, anchor.y);
            for (int y = 0; y < len; y++)
                EmitCell(anchor.x, y, perpendicular: false);
        }

        private void EmitCore(float cx, float cy)
        {
            if (_coreSize <= 0f) return;
            ParticleSystem.EmitParams core = new ParticleSystem.EmitParams();
            core.position = new Vector3(cx, cy, 0f);
            core.velocity = Vector3.zero;
            core.startSize = _coreSize;
            core.startLifetime = _coreLifetime;
            core.startColor = _coreColor;
            _particles.Emit(core, 1);
        }

        // perpendicular=true → row (sparks shoot mostly vertical)
        // perpendicular=false → column (sparks shoot mostly horizontal)
        private void EmitCell(float cx, float cy, bool perpendicular)
        {
            Vector3 pos = new Vector3(cx, cy, 0f);

            for (int i = 0; i < _flamesPerCell; i++)
            {
                ParticleSystem.EmitParams flame = new ParticleSystem.EmitParams();
                flame.position = pos + new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f), 0f);
                flame.velocity = Vector3.zero;
                flame.startSize = Random.Range(_flameMinSize, _flameMaxSize);
                flame.startLifetime = Random.Range(_flameMinLifetime, _flameMaxLifetime);
                flame.startColor = _flameStartColor;
                _particles.Emit(flame, 1);
            }

            for (int i = 0; i < _sparksPerCell; i++)
            {
                Vector3 vel;
                if (perpendicular)
                    vel = new Vector3(Random.Range(-0.6f, 0.6f), Random.Range(-_sparkSpeed, _sparkSpeed), 0f);
                else
                    vel = new Vector3(Random.Range(-_sparkSpeed, _sparkSpeed), Random.Range(-0.6f, 0.6f), 0f);

                ParticleSystem.EmitParams spark = new ParticleSystem.EmitParams();
                spark.position = pos;
                spark.velocity = vel;
                spark.startSize = Random.Range(_sparkMinSize, _sparkMaxSize);
                spark.startLifetime = Random.Range(_sparkMinLifetime, _sparkMaxLifetime);
                spark.startColor = _sparkColor;
                _particles.Emit(spark, 1);
            }
        }

        [ContextMenu("Test Burst")]
        private void TestBurst()
        {
            if (_particles == null) _particles = GetComponent<ParticleSystem>();
            int mid = 5;
            if (_boardController != null)
                mid = (_axis == ExplosionAxis.Horizontal ? _boardController.gridSizeX : _boardController.gridSizeY) / 2;
            Vector2Int anchor = _axis == ExplosionAxis.Horizontal ? new Vector2Int(mid, 5) : new Vector2Int(5, mid);
            if (_axis == ExplosionAxis.Horizontal) PlayRow(anchor);
            else PlayColumn(anchor);
        }
    }
}
