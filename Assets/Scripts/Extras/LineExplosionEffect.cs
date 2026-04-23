using System.Collections;
using Board;
using Gameplay;
using UnityEngine;

namespace Extras
{
    /// <summary>
    /// Two-layer line/column ability VFX:
    ///   1) beam — a short bright strip that spans the entire row or column from the anchor.
    ///   2) cell explosions — a 22-frame animated explosion in every affected cell.
    /// Beam emits instantly; cells emit in the same frame but last longer so the beam reads
    /// as the "first" layer and the explosions bloom over it.
    /// </summary>
    public class LineExplosionEffect : MonoBehaviour
    {
        public enum ExplosionAxis { Horizontal, Vertical }

        [SerializeField] private ExplosionAxis _axis = ExplosionAxis.Horizontal;
        [SerializeField] private BoardController _boardController;

        [Header("Beam")]
        [SerializeField] private ParticleSystem _beamParticles;
        [SerializeField] private float _beamThickness = 0.6f;
        [SerializeField] private float _beamLifetime = 0.2f;
        [SerializeField] private Color _beamColor = new Color(1f, 0.95f, 0.55f, 1f);

        [Header("Per-cell explosion")]
        [SerializeField] private ParticleSystem _cellParticles;
        [SerializeField] private float _cellSize = 1f;
        [SerializeField] private float _cellLifetime = 0.7f;
        [SerializeField] private Color _cellColor = Color.white;

        [Header("Ripple timing")]
        [Tooltip("Delay between successive outward steps of the explosion ripple from the anchor cell.")]
        [SerializeField] private float _cellRippleInterval = 0.04f;

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
            if (_boardController == null) return;
            int len = _boardController.gridSizeX;
            float maxSpan = 2f * Mathf.Max(anchor.x + 0.5f, len - 0.5f - anchor.x);
            EmitBeam(new Vector3(anchor.x, anchor.y, 0f), new Vector3(maxSpan, _beamThickness, 1f));
            StartCoroutine(RippleCells(anchor, len, horizontal: true));
        }

        private void PlayColumn(Vector2Int anchor)
        {
            if (_boardController == null) return;
            int len = _boardController.gridSizeY;
            float maxSpan = 2f * Mathf.Max(anchor.y + 0.5f, len - 0.5f - anchor.y);
            EmitBeam(new Vector3(anchor.x, anchor.y, 0f), new Vector3(_beamThickness, maxSpan, 1f));
            StartCoroutine(RippleCells(anchor, len, horizontal: false));
        }

        // Emits one explosion at the anchor immediately, then steps outward one cell at a time
        // in both directions, with _cellRippleInterval between steps.
        private IEnumerator RippleCells(Vector2Int anchor, int len, bool horizontal)
        {
            int axis = horizontal ? anchor.x : anchor.y;
            EmitAt(anchor, axis, horizontal);

            int maxDist = Mathf.Max(axis, len - 1 - axis);
            WaitForSeconds wait = new WaitForSeconds(_cellRippleInterval);

            for (int d = 1; d <= maxDist; d++)
            {
                yield return wait;
                int left = axis - d;
                int right = axis + d;
                if (left >= 0) EmitAt(anchor, left, horizontal);
                if (right < len) EmitAt(anchor, right, horizontal);
            }
        }

        private void EmitAt(Vector2Int anchor, int axisValue, bool horizontal)
        {
            Vector3 pos = horizontal
                ? new Vector3(axisValue, anchor.y, 0f)
                : new Vector3(anchor.x, axisValue, 0f);
            EmitCell(pos);
        }

        private void EmitBeam(Vector3 position, Vector3 size3D)
        {
            if (_beamParticles == null) return;
            ParticleSystem.EmitParams p = new ParticleSystem.EmitParams();
            p.position = position;
            p.velocity = Vector3.zero;
            p.startLifetime = _beamLifetime;
            p.startColor = _beamColor;
            p.startSize3D = size3D;
            _beamParticles.Emit(p, 1);
        }

        private void EmitCell(Vector3 position)
        {
            if (_cellParticles == null) return;
            ParticleSystem.EmitParams p = new ParticleSystem.EmitParams();
            p.position = position;
            p.velocity = Vector3.zero;
            p.startSize = _cellSize;
            p.startLifetime = _cellLifetime;
            p.startColor = _cellColor;
            _cellParticles.Emit(p, 1);
        }

        [ContextMenu("Test Burst")]
        private void TestBurst()
        {
            int mid = 5;
            if (_boardController != null)
                mid = (_axis == ExplosionAxis.Horizontal ? _boardController.gridSizeX : _boardController.gridSizeY) / 2;
            Vector2Int anchor = _axis == ExplosionAxis.Horizontal
                ? new Vector2Int(mid, 5)
                : new Vector2Int(5, mid);
            if (_axis == ExplosionAxis.Horizontal) PlayRow(anchor);
            else PlayColumn(anchor);
        }
    }
}
