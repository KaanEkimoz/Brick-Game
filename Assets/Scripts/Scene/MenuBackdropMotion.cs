using UnityEngine;
using UnityEngine.UI;

namespace Scene
{
    /// <summary>Brings the menu background to life without new art. The standard background's
    /// planet is lifted out as its own image and floats slowly; Deep Ocean's moon sits behind
    /// coral, so there the light from the surface sways instead.</summary>
    [RequireComponent(typeof(RectTransform))]
    public class MenuBackdropMotion : MonoBehaviour
    {
        private const string OceanTheme = "deep_ocean";

        // Where the planet sits in Sprites/Background.jpg, as a share of the image.
        private static readonly Vector2 PlanetCenter = new Vector2(0.504f, 0.508f);
        private static readonly Vector2 PlanetRadius = new Vector2(0.27f, 0.141f);
        private const float CutoutScale = 1.12f; // takes in the glow around the rim
        private const float Feather = 0.12f;
        private const int CutoutSize = 512;
        private const float BobHeight = 5f;
        private const float BobPeriod = 6f;
        private const float BreatheAmount = 0.012f;
        private const float BreathePeriod = 9f;

        private const int RayCount = 4;
        private const float RayAlpha = 0.09f;
        private static readonly float[] RayAngles = { -18f, -6f, 7f, 19f };
        private static readonly float[] RayOffsets = { -0.2f, -0.05f, 0.08f, 0.22f };

        private RectTransform _planet;
        private Vector2 _planetBase;
        private RectTransform[] _rays;
        private Image[] _rayImages;

        /// <summary>Adds the motion right above the given background image.</summary>
        public static void Attach(Image background)
        {
            if (background.transform.parent.GetComponentInChildren<MenuBackdropMotion>(true) != null) return;

            GameObject go = new GameObject(nameof(MenuBackdropMotion), typeof(RectTransform));
            go.transform.SetParent(background.transform.parent, false);
            go.transform.SetSiblingIndex(background.transform.GetSiblingIndex() + 1);
            RectTransform rect = (RectTransform)go.transform;
            RectTransform area = background.rectTransform;
            rect.anchorMin = area.anchorMin;
            rect.anchorMax = area.anchorMax;
            rect.pivot = area.pivot;
            rect.sizeDelta = area.sizeDelta;
            rect.anchoredPosition = area.anchoredPosition;

            MenuBackdropMotion motion = go.AddComponent<MenuBackdropMotion>();
            if (Themes.ThemeManager.Selected == OceanTheme) motion.BuildRays();
            else motion.BuildPlanet(background.sprite.texture);
        }

        private void BuildPlanet(Texture source)
        {
            Texture2D cutout = Cutout(source);
            if (cutout == null) return;

            GameObject go = new GameObject("Planet", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(transform, false);
            Image image = go.GetComponent<Image>();
            image.sprite = Sprite.Create(cutout, new Rect(0f, 0f, CutoutSize, CutoutSize), new Vector2(0.5f, 0.5f));
            image.raycastTarget = false;

            Rect area = ((RectTransform)transform).rect;
            _planet = (RectTransform)go.transform;
            _planet.anchorMin = _planet.anchorMax = new Vector2(0.5f, 0.5f);
            _planetBase = new Vector2((PlanetCenter.x - 0.5f) * area.width, (PlanetCenter.y - 0.5f) * area.height);
            _planet.anchoredPosition = _planetBase;
            _planet.sizeDelta = new Vector2(PlanetRadius.x * area.width, PlanetRadius.y * area.height) * (2f * CutoutScale);
        }

        // The texture is not readable, so the planet's square is drawn into a render target
        // and read back, then given a soft round edge.
        private static Texture2D Cutout(Texture source)
        {
            if (source == null) return null;
            Vector2 half = PlanetRadius * CutoutScale;
            Vector2 scale = half * 2f;
            Vector2 offset = PlanetCenter - half;

            RenderTexture target = RenderTexture.GetTemporary(CutoutSize, CutoutSize, 0, RenderTextureFormat.ARGB32);
            RenderTexture previous = RenderTexture.active;
            Graphics.Blit(source, target, scale, offset);
            RenderTexture.active = target;
            Texture2D cutout = new Texture2D(CutoutSize, CutoutSize, TextureFormat.RGBA32, false);
            cutout.ReadPixels(new Rect(0f, 0f, CutoutSize, CutoutSize), 0, 0);
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(target);

            Color32[] pixels = cutout.GetPixels32();
            float middle = CutoutSize * 0.5f;
            for (int y = 0; y < CutoutSize; y++)
                for (int x = 0; x < CutoutSize; x++)
                {
                    float d = new Vector2(x + 0.5f - middle, y + 0.5f - middle).magnitude / middle;
                    float alpha = Mathf.Clamp01((1f - d) / Feather);
                    pixels[y * CutoutSize + x].a = (byte)(alpha * 255f);
                }
            cutout.SetPixels32(pixels);
            cutout.Apply();
            return cutout;
        }

        private void BuildRays()
        {
            Sprite sprite = RaySprite();
            Rect area = ((RectTransform)transform).rect;
            _rays = new RectTransform[RayCount];
            _rayImages = new Image[RayCount];
            for (int i = 0; i < RayCount; i++)
            {
                GameObject go = new GameObject("LightRay", typeof(RectTransform), typeof(Image));
                go.transform.SetParent(transform, false);
                Image image = go.GetComponent<Image>();
                image.sprite = sprite;
                image.raycastTarget = false;
                RectTransform rect = (RectTransform)go.transform;
                rect.anchorMin = rect.anchorMax = new Vector2(0.5f + RayOffsets[i], 1f);
                rect.pivot = new Vector2(0.5f, 1f);
                rect.sizeDelta = new Vector2(area.width * 0.16f, area.height * 0.75f);
                rect.anchoredPosition = Vector2.up * 20f;
                rect.localRotation = Quaternion.Euler(0f, 0f, RayAngles[i]);
                _rays[i] = rect;
                _rayImages[i] = image;
            }
        }

        // Bright at the top, fading down and toward both sides.
        private static Sprite RaySprite()
        {
            const int width = 32;
            const int height = 128;
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    float across = 1f - Mathf.Abs((x + 0.5f) / width * 2f - 1f);
                    float down = (y + 0.5f) / height;
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, across * across * down * down));
                }
            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 1f));
        }

        private void Update()
        {
            float time = Time.unscaledTime;
            if (_planet != null)
            {
                _planet.anchoredPosition = _planetBase + Vector2.up * (BobHeight * Mathf.Sin(time * 2f * Mathf.PI / BobPeriod));
                _planet.localScale = Vector3.one * (1f + BreatheAmount * Mathf.Sin(time * 2f * Mathf.PI / BreathePeriod));
            }

            if (_rays == null) return;
            for (int i = 0; i < _rays.Length; i++)
            {
                float phase = i * 1.7f;
                float sway = Mathf.Sin(time * 0.35f + phase) * 3f;
                _rays[i].localRotation = Quaternion.Euler(0f, 0f, RayAngles[i] + sway);
                float glow = 0.5f + 0.5f * Mathf.Sin(time * 0.5f + phase * 1.3f);
                _rayImages[i].color = new Color(1f, 1f, 1f, RayAlpha * (0.35f + 0.65f * glow));
            }
        }
    }
}
