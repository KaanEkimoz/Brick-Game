using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Scene
{
    /// <summary>A few faint lights drift up behind a menu, so the screen never sits still but
    /// nothing asks for attention. Deep Ocean gets bubbles, the standard look soft glows.</summary>
    [RequireComponent(typeof(RectTransform))]
    public class MenuAmbience : MonoBehaviour
    {
        private const int Count = 14;
        private const int TextureSize = 64;
        private const float MinSize = 12f;
        private const float MaxSize = 38f;
        private const float MinSpeed = 18f;
        private const float MaxSpeed = 42f;
        private const float MinLife = 7f;
        private const float MaxLife = 12f;
        private const float SwayAmount = 18f;
        private const float MaxAlpha = 0.3f;
        private const string OceanTheme = "deep_ocean";
        private static readonly Color[] GlowColors =
        {
            new Color(0.45f, 0.85f, 1f), new Color(0.75f, 0.55f, 1f), new Color(1f, 1f, 1f),
        };

        private static Sprite _glow;
        private static Sprite _bubble;

        private readonly List<Mote> _motes = new List<Mote>();
        private RectTransform _area;
        private bool _bubbles;

        private class Mote
        {
            public RectTransform Rect;
            public Image Image;
            public Vector2 Start;
            public float Speed;
            public float Life;
            public float Age;
            public float Phase;
            public Color Color;
        }

        /// <summary>Fills the parent with drifting lights at the given sibling slot.</summary>
        public static MenuAmbience Add(Transform parent, int siblingIndex)
        {
            RectTransform rect = Create(parent, siblingIndex);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return rect.gameObject.AddComponent<MenuAmbience>();
        }

        /// <summary>Covers the same area as a sibling, for a parent smaller than the screen.</summary>
        public static MenuAmbience Over(RectTransform area)
        {
            RectTransform rect = Create(area.parent, area.GetSiblingIndex() + 1);
            rect.anchorMin = area.anchorMin;
            rect.anchorMax = area.anchorMax;
            rect.pivot = area.pivot;
            rect.sizeDelta = area.sizeDelta;
            rect.anchoredPosition = area.anchoredPosition;
            return rect.gameObject.AddComponent<MenuAmbience>();
        }

        private static RectTransform Create(Transform parent, int siblingIndex)
        {
            GameObject go = new GameObject(nameof(MenuAmbience), typeof(RectTransform));
            go.transform.SetParent(parent, false);
            if (siblingIndex >= 0) go.transform.SetSiblingIndex(siblingIndex);
            return (RectTransform)go.transform;
        }

        private void Awake()
        {
            _area = (RectTransform)transform;
            _bubbles = Themes.ThemeManager.Selected == OceanTheme;
            Sprite sprite = _bubbles ? Bubble() : Glow();

            for (int i = 0; i < Count; i++)
            {
                GameObject go = new GameObject("Mote", typeof(RectTransform), typeof(Image));
                go.transform.SetParent(transform, false);
                Image image = go.GetComponent<Image>();
                image.sprite = sprite;
                image.raycastTarget = false;
                Mote mote = new Mote { Rect = (RectTransform)go.transform, Image = image };
                Respawn(mote, Random.value);
                _motes.Add(mote);
            }
        }

        private void Update()
        {
            Vector2 size = _area.rect.size;
            float dt = Time.unscaledDeltaTime;
            foreach (Mote mote in _motes)
            {
                mote.Age += dt;
                if (mote.Age >= mote.Life)
                {
                    Respawn(mote, 0f);
                    continue;
                }

                float n = mote.Age / mote.Life;
                float sway = Mathf.Sin(mote.Phase + mote.Age * 0.8f) * SwayAmount;
                mote.Rect.anchoredPosition = mote.Start + new Vector2(sway, mote.Speed * mote.Age);
                // Fades in, holds, and fades out before it reaches the top.
                float alpha = Mathf.Sin(n * Mathf.PI) * MaxAlpha;
                Color color = mote.Color;
                color.a = alpha;
                mote.Image.color = color;

                if (mote.Rect.anchoredPosition.y > size.y * 0.5f + MaxSize) Respawn(mote, 0f);
            }
        }

        // A fresh mote starts low; at launch they are spread over their lives so none pop in together.
        private void Respawn(Mote mote, float startAge)
        {
            Vector2 size = _area.rect.size;
            if (size.x <= 0f) size = new Vector2(1080f, 1920f);

            float diameter = Random.Range(MinSize, MaxSize);
            mote.Rect.sizeDelta = Vector2.one * diameter;
            mote.Speed = Random.Range(MinSpeed, MaxSpeed);
            mote.Life = Random.Range(MinLife, MaxLife);
            mote.Age = startAge * mote.Life;
            mote.Phase = Random.Range(0f, Mathf.PI * 2f);
            mote.Start = new Vector2(Random.Range(-0.5f, 0.5f) * size.x, Random.Range(-0.5f, 0.1f) * size.y);
            mote.Color = _bubbles ? Color.white : GlowColors[Random.Range(0, GlowColors.Length)];
            mote.Image.color = Color.clear;
        }

        private static Sprite Glow()
        {
            if (_glow == null) _glow = MakeSprite(d => Mathf.Pow(Mathf.Clamp01(1f - d), 2f));
            return _glow;
        }

        private static Sprite Bubble()
        {
            // A thin bright rim around a faint fill.
            if (_bubble == null) _bubble = MakeSprite(d => d > 1f ? 0f : Mathf.Clamp01(1f - Mathf.Abs(d - 0.86f) / 0.12f) + 0.12f);
            return _bubble;
        }

        private static Sprite MakeSprite(System.Func<float, float> alphaAt)
        {
            Texture2D texture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false);
            float half = TextureSize * 0.5f;
            for (int y = 0; y < TextureSize; y++)
                for (int x = 0; x < TextureSize; x++)
                {
                    float d = new Vector2(x + 0.5f - half, y + 0.5f - half).magnitude / half;
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alphaAt(d)));
                }
            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, TextureSize, TextureSize), new Vector2(0.5f, 0.5f));
        }
    }
}
