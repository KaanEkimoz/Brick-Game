using System.Collections;
using Gameplay;
using InGame;
using Levels;
using Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Daily
{
    /// <summary>
    /// The other ways into the daily challenge: an orange neon button right under Play on the
    /// main menu, and a strip at the top of the screen after a Classic or Extended game over while
    /// today's challenge is still open.
    /// </summary>
    public class DailyMenuEntry : MonoBehaviour
    {
        private const string SettingsButtonName = "B_Settings";
        private const string PlayButtonName = "B_PlayMain"; // LevelUi builds it from the scene's B_Play
        private const string MenuButtonName = "B_DailyMain";
        private const float MenuFontSize = 36f;
        private const int StripCanvasOrder = 120;
        private const float StripDelaySeconds = 1f;
        private const float DoneAlpha = 0.6f;

        private static readonly Vector2 ReferenceResolution = new Vector2(1080f, 1920f);
        private static readonly Vector2 StripSize = new Vector2(860f, 120f);
        private static readonly Vector2 StripOffset = new Vector2(0f, -36f);
        private const int RoundedTexture = 64;
        private const float CornerRadius = 22f;
        private const float RingWidth = 4f;

        private static readonly Color DailyOrange = new Color(1f, 0.55f, 0.15f, 1f);
        private static readonly Color DailyGlow = new Color(1f, 0.8f, 0.45f, 1f);
        private static readonly Color FaceColor = new Color(0.03f, 0.04f, 0.16f, 0.95f);
        private static readonly Color StripText = new Color(1f, 0.78f, 0.45f, 1f);
        // The visible box inside the Settings-sized button, which leaves out the glow margin.
        private static readonly Vector2 FaceMin = new Vector2(0.1f, 0.22f);
        private static readonly Vector2 FaceMax = new Vector2(0.9f, 0.78f);
        private static Sprite _fillSprite;
        private static Sprite _ringSprite;
        private static readonly Color StripColor = new Color(0.01f, 0.03f, 0.14f, 0.94f);

        [SerializeField] private float _pulseSpeed = 2.6f;
        [Tooltip("How far Settings moves down to make room, as a share of its own height.")]
        [SerializeField] private float _settingsShift = 0.8f;

        private DailyUi _ui;
        private Image _ring;
        private GameObject _strip;

        private void Awake()
        {
            _ui = GetComponent<DailyUi>();
        }

        // After LevelUi has built the Play button and the theme has styled the menu.
        private void Start()
        {
            Scene.MenuButtonStyle.Primary(FindSceneButton(PlayButtonName));
            Scene.MenuButtonStyle.Primary(FindSceneButton(SettingsButtonName));
            BuildMenuButton();
        }

        private void OnEnable()
        {
            PiecesController.OnGameOver += HandleGameOver;
        }

        private void OnDisable()
        {
            PiecesController.OnGameOver -= HandleGameOver;
        }

        private void Update()
        {
            if (_ring != null)
            {
                float t = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * _pulseSpeed);
                _ring.color = Color.Lerp(DailyOrange, DailyGlow, t);
            }

            // A new run, or a scene change, takes the strip away.
            if (_strip != null && PiecesController.Instance != null && PiecesController.Instance.IsRunActive)
                Destroy(_strip);
        }

        // Takes the Settings button's place and moves Settings down, so it sits right under Play.
        private void BuildMenuButton()
        {
            Button settings = FindSceneButton(SettingsButtonName);
            if (settings == null) return;

            Button daily = Instantiate(settings, settings.transform.parent);
            daily.name = MenuButtonName;
            daily.onClick = new Button.ButtonClickedEvent();
            daily.onClick.AddListener(() => _ui.ShowIntro());

            RectTransform settingsRect = (RectTransform)settings.transform;
            settingsRect.anchoredPosition += Vector2.down * (settingsRect.rect.height * _settingsShift);

            // The glowing pill would turn a muddy green under orange, so the copy keeps only its
            // size and tap area and draws a dark rounded box with an orange neon ring instead.
            daily.GetComponent<Image>().color = Color.clear;
            CreateLayer(daily.transform, "Face", FillSprite, FaceColor);
            _ring = CreateLayer(daily.transform, "Ring", RingSprite, DailyOrange);

            TMP_Text label = daily.GetComponentInChildren<TMP_Text>(true);
            if (label != null)
            {
                label.transform.SetAsLastSibling();
                label.text = Loc.Caps("Daily");
                label.color = DailyOrange;
                Scene.PanelStyle.FitButtonLabel(label, MenuFontSize, ((RectTransform)daily.transform).rect.width);
            }

            // Pulses while today is open; fades once it is done.
            if (!DailySession.TodayIsOpen)
            {
                _ring = null;
                daily.gameObject.AddComponent<CanvasGroup>().alpha = DoneAlpha;
            }
        }

        private static Image CreateLayer(Transform button, string name, Sprite sprite, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(button, false);
            RectTransform rect = (RectTransform)go.transform;
            rect.anchorMin = FaceMin;
            rect.anchorMax = FaceMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            Image image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.type = Image.Type.Sliced;
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static Sprite FillSprite => _fillSprite != null ? _fillSprite : (_fillSprite = DrawRounded(false));
        private static Sprite RingSprite => _ringSprite != null ? _ringSprite : (_ringSprite = DrawRounded(true));

        // A white rounded rectangle, or its outline, drawn once and nine-sliced to any size.
        private static Sprite DrawRounded(bool ring)
        {
            int size = RoundedTexture;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            Color[] pixels = new Color[size * size];
            float half = size * 0.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // Signed distance to the rounded edge: negative inside.
                    float qx = Mathf.Abs(x + 0.5f - half) - (half - 1f - CornerRadius);
                    float qy = Mathf.Abs(y + 0.5f - half) - (half - 1f - CornerRadius);
                    float outside = new Vector2(Mathf.Max(qx, 0f), Mathf.Max(qy, 0f)).magnitude;
                    float d = outside + Mathf.Min(Mathf.Max(qx, qy), 0f) - CornerRadius;
                    float alpha = Mathf.Clamp01(0.5f - d);
                    if (ring) alpha *= Mathf.Clamp01(d + RingWidth + 0.5f);
                    pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }
            texture.SetPixels(pixels);
            texture.Apply();
            float border = CornerRadius + 2f;
            return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f, 0,
                SpriteMeshType.FullRect, new Vector4(border, border, border, border));
        }

        private static Button FindSceneButton(string name)
        {
            foreach (Button button in Resources.FindObjectsOfTypeAll<Button>())
                if (button.name == name && button.gameObject.scene.IsValid()) return button;
            return null;
        }

        private void HandleGameOver()
        {
            if (GameMode.IsLevels || !DailySession.TodayIsOpen || DailySession.TodayChallenge == null) return;
            StartCoroutine(ShowStripSoon());
        }

        // After the game over panel has come in.
        private IEnumerator ShowStripSoon()
        {
            yield return new WaitForSecondsRealtime(StripDelaySeconds);
            if (PiecesController.Instance != null && PiecesController.Instance.IsRunActive) yield break;
            BuildStrip();
        }

        private void BuildStrip()
        {
            if (_strip != null) return;

            _strip = new GameObject("DailyStrip", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            _strip.transform.SetParent(transform, false);
            Canvas canvas = _strip.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = StripCanvasOrder;
            CanvasScaler scaler = _strip.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.matchWidthOrHeight = 0.5f;

            GameObject go = new GameObject("Strip", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
            go.transform.SetParent(_strip.transform, false);
            go.GetComponent<Image>().color = StripColor;
            Outline outline = go.GetComponent<Outline>();
            outline.effectColor = DailyOrange;
            outline.effectDistance = new Vector2(4f, 4f);
            go.GetComponent<Button>().onClick.AddListener(OpenDailyFromGameOver);

            RectTransform rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = StripSize;
            rect.anchoredPosition = StripOffset;

            TextMeshProUGUI text = new GameObject("Text", typeof(RectTransform)).AddComponent<TextMeshProUGUI>();
            text.transform.SetParent(go.transform, false);
            if (PlainFont.Asset != null) text.font = PlainFont.Asset;
            text.text = Loc.T("Today's challenge is waiting") + "  >";
            text.fontSize = 40f;
            text.fontStyle = FontStyles.Bold;
            text.color = StripText;
            text.alignment = TextAlignmentOptions.Center;
            text.raycastTarget = false;
            RectTransform textRect = text.rectTransform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            Scene.PanelPop.Add(go, Scene.PanelPop.Style.Pop);
        }

        // The game over screen belongs to another mode; the menu reload opens the daily panel.
        private static void OpenDailyFromGameOver()
        {
            DailyUi.OpenIntroOnLoad = true;
            if (LevelSession.Instance != null) LevelSession.Instance.ReturnToMenu();
        }
    }
}
