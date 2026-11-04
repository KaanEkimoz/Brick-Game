using System.Collections.Generic;
using UnityEngine;

namespace Levels
{
    /// <summary>Keeps the mode screen moving: each picture drifts and zooms very slowly, out of
    /// step with the others, and the neon dividers glow up and down.</summary>
    public class ModeScreenMotion : MonoBehaviour
    {
        private const float MaxZoom = 1.08f;
        private const float ZoomPeriod = 11f;
        private const float PanPeriod = 15f;
        private const float GlowPeriod = 2.6f;
        private const float MinGlow = 0.6f;

        private readonly List<DiagonalSlice> _pictures = new List<DiagonalSlice>();
        private readonly List<DiagonalSlice> _dividers = new List<DiagonalSlice>();
        private readonly List<Color> _dividerColors = new List<Color>();

        public void AddPicture(DiagonalSlice slice) => _pictures.Add(slice);

        public void AddDivider(DiagonalSlice slice)
        {
            _dividers.Add(slice);
            _dividerColors.Add(slice.color);
        }

        private void Update()
        {
            float time = Time.unscaledTime;
            for (int i = 0; i < _pictures.Count; i++)
            {
                if (_pictures[i] == null) continue;
                float phase = i * 2.1f;
                float zoom = Mathf.Lerp(1.02f, MaxZoom, 0.5f + 0.5f * Mathf.Sin(time * 2f * Mathf.PI / ZoomPeriod + phase));
                Vector2 pan = new Vector2(Mathf.Sin(time * 2f * Mathf.PI / PanPeriod + phase), Mathf.Cos(time * 2f * Mathf.PI / (PanPeriod * 1.3f) + phase) * 0.6f);
                _pictures[i].SetView(zoom, pan);
            }

            float glow = Mathf.Lerp(MinGlow, 1f, 0.5f + 0.5f * Mathf.Sin(time * 2f * Mathf.PI / GlowPeriod));
            for (int i = 0; i < _dividers.Count; i++)
            {
                if (_dividers[i] == null) continue;
                Color color = _dividerColors[i];
                color.a *= glow;
                _dividers[i].color = color;
            }
        }
    }
}
