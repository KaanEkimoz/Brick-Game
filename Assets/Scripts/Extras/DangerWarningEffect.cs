using Board;
using UnityEngine;
using UnityEngine.UI;

namespace Extras
{
    /// <summary>
    /// Fades a red overlay image in/out as a slow pulse whenever any settled tile
    /// reaches the top N rows of the board. Subtle by design — low alpha, slow
    /// period — so the player notices danger without the HUD pulling attention
    /// away from the piece.
    /// </summary>
    public class DangerWarningEffect : MonoBehaviour
    {
        [SerializeField] private Image _overlay;
        [SerializeField] [Range(1, 6)] private int _rowThreshold = 4;
        [SerializeField] private Color _dangerColor = new Color(1f, 0.2f, 0.2f, 1f);
        [SerializeField] [Range(0f, 0.3f)] private float _minAlpha = 0.04f;
        [SerializeField] [Range(0f, 0.4f)] private float _maxAlpha = 0.18f;
        [SerializeField] private float _period = 1.8f;
        [SerializeField] private float _intensityFadeSpeed = 3f;

        private float _intensity;
        private float _phase;

        private void Start()
        {
            ApplyAlpha(0f);
        }

        private void Update()
        {
            float target = InDanger() ? 1f : 0f;
            _intensity = Mathf.MoveTowards(_intensity, target, Time.deltaTime * _intensityFadeSpeed);

            _phase += Time.deltaTime / Mathf.Max(0.05f, _period);
            if (_phase > 1f) _phase -= 1f;

            float pulse = 0.5f * (1f + Mathf.Sin(_phase * Mathf.PI * 2f));
            float alpha = Mathf.Lerp(_minAlpha, _maxAlpha, pulse) * _intensity;
            ApplyAlpha(alpha);
        }

        private void ApplyAlpha(float a)
        {
            if (_overlay == null) return;
            Color c = _dangerColor;
            c.a = a;
            _overlay.color = c;
        }

        private bool InDanger()
        {
            BoardController bc = BoardController.Instance;
            if (bc == null) return false;
            int startY = bc.gridSizeY - _rowThreshold;
            if (startY < 0) startY = 0;
            for (int y = startY; y < bc.gridSizeY; y++)
            {
                for (int x = 0; x < bc.gridSizeX; x++)
                {
                    if (!bc.IsPosEmpty(new Vector2Int(x, y))) return true;
                }
            }
            return false;
        }
    }
}
