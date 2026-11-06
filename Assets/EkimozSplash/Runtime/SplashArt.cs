using UnityEngine;

namespace Ekimoz.Splash
{
    /// <summary>
    /// The splash's shapes, drawn in code so the package needs no image files: a beveled block,
    /// a soft round glow and a full-screen vignette. All are white and tinted where they are used.
    /// </summary>
    internal static class SplashArt
    {
        private const int BlockSize = 96;
        private const float BlockRadius = 18f;
        private const int GlowSize = 128;
        private const int VignetteSize = 64;

        private static Sprite _block;
        private static Sprite _glow;
        private static Sprite _vignette;

        public static Sprite Block => _block != null ? _block : (_block = DrawBlock());
        public static Sprite Glow => _glow != null ? _glow : (_glow = DrawGlow());
        public static Sprite Vignette => _vignette != null ? _vignette : (_vignette = DrawVignette());

        // A rounded square, lighter at the top and with a thin bright rim, like a lit glass tile.
        private static Sprite DrawBlock()
        {
            int size = BlockSize;
            Color[] pixels = new Color[size * size];
            float half = size * 0.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float qx = Mathf.Abs(x + 0.5f - half) - (half - 1f - BlockRadius);
                    float qy = Mathf.Abs(y + 0.5f - half) - (half - 1f - BlockRadius);
                    float d = new Vector2(Mathf.Max(qx, 0f), Mathf.Max(qy, 0f)).magnitude
                        + Mathf.Min(Mathf.Max(qx, qy), 0f) - BlockRadius;
                    float alpha = Mathf.Clamp01(0.5f - d);

                    float height = (y + 0.5f) / size;
                    float shade = Mathf.Lerp(0.72f, 1f, height);
                    float rim = Mathf.Clamp01(1f - Mathf.Abs(d + 3f) / 2.5f);
                    float top = height > 0.62f ? Mathf.Clamp01((height - 0.62f) * 2.2f) * 0.18f : 0f;
                    float value = Mathf.Clamp01(shade + rim * 0.35f + top);
                    pixels[y * size + x] = new Color(value, value, value, alpha);
                }
            }
            return Create(size, pixels);
        }

        private static Sprite DrawGlow()
        {
            int size = GlowSize;
            Color[] pixels = new Color[size * size];
            float half = size * 0.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float r = new Vector2(x + 0.5f - half, y + 0.5f - half).magnitude / half;
                    float alpha = Mathf.Pow(Mathf.Clamp01(1f - r), 2.2f);
                    pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }
            return Create(size, pixels);
        }

        // Clear in the middle, darker toward the corners.
        private static Sprite DrawVignette()
        {
            int size = VignetteSize;
            Color[] pixels = new Color[size * size];
            float half = size * 0.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float r = new Vector2(x + 0.5f - half, y + 0.5f - half).magnitude / (half * 1.41f);
                    float alpha = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((r - 0.35f) / 0.65f));
                    pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }
            return Create(size, pixels);
        }

        private static Sprite Create(int size, Color[] pixels)
        {
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.SetPixels(pixels);
            texture.Apply(false, true);
            return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
        }
    }
}
