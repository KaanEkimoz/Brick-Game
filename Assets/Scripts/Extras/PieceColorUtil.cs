using System.Collections.Generic;
using Piece;
using UnityEngine;

namespace Extras
{
    /// <summary>
    /// Shared helper: the dominant colour of the currently falling piece's tile sprite, so
    /// drop effects (soft-drop trail, hard-drop streak/spark) can tint themselves to match the
    /// piece — cyan I, red Z, etc. — instead of plain white. Averages the sprite's opaque
    /// pixels once per sprite and caches the result, so it's cheap after the first call.
    /// (Mino textures are imported Read/Write enabled so GetPixels works.)
    /// </summary>
    public static class PieceColorUtil
    {
        private static readonly Dictionary<Sprite, Color> _cache = new Dictionary<Sprite, Color>();

        /// <summary>Average colour of a sprite's opaque pixels (cached). Returns fallback if unreadable.</summary>
        public static Color DominantColor(Sprite s, Color fallback)
        {
            if (s == null) return fallback;
            if (_cache.TryGetValue(s, out var cached)) return cached;

            Color result = fallback;
            var tex = s.texture;
            if (tex != null && tex.isReadable)
            {
                var r = s.textureRect;
                var px = tex.GetPixels((int)r.x, (int)r.y, (int)r.width, (int)r.height);
                float rr = 0f, gg = 0f, bb = 0f; int n = 0;
                for (int i = 0; i < px.Length; i++)
                {
                    if (px[i].a <= 0.5f) continue;
                    rr += px[i].r; gg += px[i].g; bb += px[i].b; n++;
                }
                if (n > 0) result = new Color(rr / n, gg / n, bb / n, 1f);
            }
            _cache[s] = result;
            return result;
        }

        /// <summary>Dominant colour of whatever piece is currently in play. Fallback if none.</summary>
        public static Color FallingPieceColor(Color fallback)
        {
            var tiles = PieceController.Tiles;
            if (tiles != null)
            {
                foreach (var t in tiles)
                {
                    if (t == null) continue;
                    var sr = t.GetComponentInChildren<SpriteRenderer>();
                    if (sr != null && sr.sprite != null)
                        return DominantColor(sr.sprite, fallback);
                }
            }
            return fallback;
        }
    }
}
