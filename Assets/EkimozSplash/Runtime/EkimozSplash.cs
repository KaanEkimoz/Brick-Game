using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ekimoz.Splash
{
    /// <summary>
    /// The Ekimoz Games splash. It starts by itself before the first scene loads, covers the
    /// screen with its own canvas while the scene comes up underneath, plays once per app launch
    /// and removes itself. Ten blocks fall and stack into an "E", a light runs across them, the
    /// EKIMOZ letters rise in one by one and GAMES settles under them. A tap skips to the end.
    /// Drop the EkimozSplash folder into any Unity project with TextMeshPro; no setup needed.
    /// </summary>
    public class EkimozSplash : MonoBehaviour
    {
        // Timeline, in seconds. Tools/splash/make_splash_sound.py uses the same block timings.
        private const float TileStart = 0.15f;
        private const float TileStagger = 0.07f;
        private const float TileFall = 0.38f;
        private const float ShineStart = 1.18f;
        private const float ShineStagger = 0.035f;
        private const float ShineLength = 0.32f;
        private const float WordStart = 1.3f;
        private const float LetterStagger = 0.06f;
        private const float LetterLength = 0.4f;
        private const float SubStart = 1.75f;
        private const float SubLength = 0.6f;
        private const float HoldUntil = 2.9f;
        private const float FadeLength = 0.45f;
        private const float SkipAfter = 0.5f;

        private const float TileSize = 82f;
        private const float TileGap = 10f;
        private const float FallHeight = 520f;
        private const float LetterRise = 28f;
        private const float SubSpacingFrom = 70f;
        private const float SubSpacingTo = 34f;
        private const float EndScale = 1.04f;
        private const int SortingOrder = 32000;
        private const string FontPath = "EkimozSplash/EkimozSplashFont";
        private const string SoundPath = "EkimozSplash/ekimoz_splash";

        private static readonly Vector2 ReferenceResolution = new Vector2(1080f, 1920f);
        private static readonly Vector2 MarkCenter = new Vector2(0f, 250f);
        private static readonly Color Background = new Color(0.03f, 0.035f, 0.08f, 1f);
        private static readonly Color VignetteColor = new Color(0f, 0f, 0f, 0.55f);
        private static readonly Color GlowColor = new Color(0.45f, 0.4f, 1f, 0.35f);
        private static readonly Color WordColor = Color.white;
        private static readonly Color SubColor = new Color(0.62f, 0.7f, 1f, 1f);
        private static readonly Color[] Gradient =
        {
            new Color(0.2f, 0.9f, 1f, 1f),
            new Color(0.5f, 0.5f, 1f, 1f),
            new Color(1f, 0.38f, 0.75f, 1f),
        };

        // The E on a 3x5 grid, bottom row first, in the order the blocks fall.
        private static readonly Vector2Int[] Cells =
        {
            new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, 2), new Vector2Int(1, 2),
            new Vector2Int(0, 3),
            new Vector2Int(0, 4), new Vector2Int(1, 4), new Vector2Int(2, 4),
        };

        /// <summary>True from launch until the splash has faded out.</summary>
        public static bool IsShowing { get; private set; }

        /// <summary>Fires once the splash is gone and the game is visible.</summary>
        public static event Action OnFinished;

        /// <summary>Set by the game (before the first scene, e.g. at SubsystemRegistration) to mute
        /// the jingle when the player turned sound effects off. Checked when the sound starts.</summary>
        public static Func<bool> SoundAllowed;

        private readonly List<Image> _tiles = new List<Image>();
        private readonly List<Color> _tileColors = new List<Color>();
        private CanvasGroup _group;
        private RectTransform _mark;
        private Image _glow;
        private TextMeshProUGUI _word;
        private TextMeshProUGUI _sub;
        private float _startTime = -1f;
        private float _skipOffset;
        private bool _skipped;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Launch()
        {
            IsShowing = true;
            GameObject go = new GameObject("EkimozSplash");
            DontDestroyOnLoad(go);
            go.AddComponent<EkimozSplash>();
        }

        private void Awake()
        {
            Build();
            Animate(0f);
        }

        private void Update()
        {
            // The clock starts on the first frame, after the first scene has loaded underneath, and
            // follows real time so the picture stays in step with the jingle.
            if (_startTime < 0f)
            {
                _startTime = Time.unscaledTime;
                PlaySound();
            }

            float t = Time.unscaledTime - _startTime + _skipOffset;
            if (!_skipped && t > SkipAfter && t < HoldUntil && Input.GetMouseButtonDown(0))
            {
                _skipped = true;
                _skipOffset += HoldUntil - t;
                t = HoldUntil;
            }

            Animate(t);
            if (t >= HoldUntil + FadeLength) Finish();
        }

        private void Finish()
        {
            IsShowing = false;
            Destroy(gameObject);
            OnFinished?.Invoke();
        }

        private void PlaySound()
        {
            if (SoundAllowed != null && !SoundAllowed()) return;
            AudioClip clip = Resources.Load<AudioClip>(SoundPath);
            if (clip == null) return;
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.ignoreListenerPause = true;
            source.PlayOneShot(clip);
        }

        // --- animation ---

        private void Animate(float t)
        {
            for (int i = 0; i < _tiles.Count; i++) AnimateTile(i, t);

            float glowIn = Smooth(Mathf.InverseLerp(ShineStart - 0.2f, ShineStart + 0.5f, t));
            Color glow = GlowColor;
            glow.a *= glowIn * (1f + 0.25f * Mathf.Sin(t * 3f));
            _glow.color = glow;

            AnimateWord(t);

            float sub = Smooth(Mathf.InverseLerp(SubStart, SubStart + SubLength, t));
            _sub.alpha = sub;
            _sub.characterSpacing = Mathf.Lerp(SubSpacingFrom, SubSpacingTo, sub);

            float fade = Mathf.InverseLerp(HoldUntil, HoldUntil + FadeLength, t);
            _group.alpha = 1f - Smooth(fade);
            float scale = Mathf.Lerp(1f, EndScale, Smooth(fade));
            _mark.localScale = Vector3.one * scale;
            _word.transform.localScale = Vector3.one * scale;
            _sub.transform.localScale = Vector3.one * scale;
        }

        // Falls in with a small overshoot, then flashes white as the light runs across.
        private void AnimateTile(int index, float t)
        {
            Image tile = _tiles[index];
            Vector2 rest = CellPosition(Cells[index]);
            float start = TileStart + index * TileStagger;
            float fall = Mathf.Clamp01((t - start) / TileFall);

            RectTransform rect = tile.rectTransform;
            rect.anchoredPosition = rest + Vector2.up * (FallHeight * (1f - EaseOutBack(fall)));
            float squash = fall >= 1f ? 1f - 0.12f * Mathf.Exp(-(t - start - TileFall) * 18f) * Mathf.Cos((t - start - TileFall) * 30f) : 1f;
            rect.localScale = new Vector3(2f - squash, squash, 1f);

            float shineAt = ShineStart + (Cells[index].x + (4 - Cells[index].y)) * ShineStagger;
            float shine = Mathf.Clamp01(1f - Mathf.Abs(t - shineAt - ShineLength * 0.5f) / (ShineLength * 0.5f));
            Color color = Color.Lerp(_tileColors[index], Color.white, Smooth(shine) * 0.85f);
            color.a = Mathf.Clamp01(fall * 4f);
            tile.color = color;
        }

        // Each letter rises and fades in on its own, by moving that letter's vertices.
        private void AnimateWord(float t)
        {
            _word.ForceMeshUpdate();
            TMP_TextInfo info = _word.textInfo;
            for (int i = 0; i < info.characterCount; i++)
            {
                TMP_CharacterInfo character = info.characterInfo[i];
                if (!character.isVisible) continue;

                float n = Smooth(Mathf.Clamp01((t - WordStart - i * LetterStagger) / LetterLength));
                int material = character.materialReferenceIndex;
                int vertex = character.vertexIndex;
                Vector3[] vertices = info.meshInfo[material].vertices;
                Color32[] colors = info.meshInfo[material].colors32;
                Vector3 offset = Vector3.down * (LetterRise * (1f - n));
                byte alpha = (byte)(255f * n);
                for (int v = 0; v < 4; v++)
                {
                    vertices[vertex + v] += offset;
                    colors[vertex + v].a = alpha;
                }
            }
            _word.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices | TMP_VertexDataUpdateFlags.Colors32);
        }

        // --- building ---

        private void Build()
        {
            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = SortingOrder;
            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.matchWidthOrHeight = 0.5f;
            gameObject.AddComponent<GraphicRaycaster>();
            _group = gameObject.AddComponent<CanvasGroup>();

            Image background = CreateImage(transform, "Background", null, Background);
            Stretch(background.rectTransform);
            background.raycastTarget = true; // nothing underneath reacts to taps during the splash

            Image vignette = CreateImage(transform, "Vignette", SplashArt.Vignette, VignetteColor);
            Stretch(vignette.rectTransform);

            _glow = CreateImage(transform, "Glow", SplashArt.Glow, Color.clear);
            Place(_glow.rectTransform, MarkCenter, new Vector2(900f, 900f));

            GameObject mark = new GameObject("Mark", typeof(RectTransform));
            mark.transform.SetParent(transform, false);
            _mark = (RectTransform)mark.transform;
            Place(_mark, MarkCenter, Vector2.zero);

            for (int i = 0; i < Cells.Length; i++)
            {
                Image tile = CreateImage(_mark, "Block", SplashArt.Block, Color.clear);
                Place(tile.rectTransform, CellPosition(Cells[i]), new Vector2(TileSize, TileSize));
                _tiles.Add(tile);
                _tileColors.Add(GradientAt(Cells[i]));
            }

            TMP_FontAsset font = Resources.Load<TMP_FontAsset>(FontPath);
            _word = CreateText("Word", "EKIMOZ", font, 120f, WordColor, new Vector2(0f, -115f), 18f);
            // Each frame sets every letter's own alpha; starting clear keeps TextMeshPro's first,
            // untouched mesh from flashing the whole word.
            _word.alpha = 0f;
            _sub = CreateText("Sub", "GAMES", font, 46f, SubColor, new Vector2(0f, -215f), SubSpacingFrom);
        }

        // Cyan at the bottom left, through violet, to pink at the top right.
        private static Color GradientAt(Vector2Int cell)
        {
            float t = Mathf.Clamp01((cell.x / 2f + cell.y / 4f) / 2f);
            return t < 0.5f
                ? Color.Lerp(Gradient[0], Gradient[1], t * 2f)
                : Color.Lerp(Gradient[1], Gradient[2], (t - 0.5f) * 2f);
        }

        private static Vector2 CellPosition(Vector2Int cell)
        {
            float step = TileSize + TileGap;
            return new Vector2((cell.x - 1f) * step, (cell.y - 2f) * step);
        }

        private TextMeshProUGUI CreateText(string name, string text, TMP_FontAsset font, float size, Color color, Vector2 position, float spacing)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(transform, false);
            TextMeshProUGUI label = go.AddComponent<TextMeshProUGUI>();
            if (font != null) label.font = font;
            label.text = text;
            label.fontSize = size;
            label.color = color;
            label.characterSpacing = spacing;
            label.alignment = TextAlignmentOptions.Center;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.raycastTarget = false;
            Place(label.rectTransform, position, new Vector2(1000f, size * 1.6f));
            return label;
        }

        private static Image CreateImage(Transform parent, string name, Sprite sprite, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            Image image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void Place(RectTransform rect, Vector2 position, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
        }

        private static float Smooth(float t) => t * t * (3f - 2f * t);

        private static float EaseOutBack(float t)
        {
            const float overshoot = 1.4f;
            float u = t - 1f;
            return 1f + (overshoot + 1f) * u * u * u + overshoot * u * u;
        }
    }
}
