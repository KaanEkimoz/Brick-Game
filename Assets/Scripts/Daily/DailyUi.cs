using System;
using System.Collections;
using System.Collections.Generic;
using Extras;
using Levels;
using Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Daily
{
    /// <summary>
    /// The daily challenge screens: a tag on the mode screen, the goal panel before a run and
    /// the result panel after it. Built at runtime on its own canvas above the Levels screens.
    /// </summary>
    public class DailyUi : MonoBehaviour
    {
        private const int CanvasOrder = 110;
        private const float MenuButtonFontSize = 58f;
        private const float ResultDelaySeconds = 0.6f;
        private const float WinDelaySeconds = 1.6f;
        private const int FastDropLevel = 9;
        private const string WinSoundPath = "LevelAudio/level_win";

        private static readonly Vector2 ReferenceResolution = new Vector2(1080f, 1920f);
        private static readonly Vector2 TagSize = new Vector2(330f, 118f);
        private static readonly Vector2 TagOffset = new Vector2(-30f, -40f);
        private const int StreakDays = 7;
        private const float BallSize = 78f;
        private const float BallSpacing = 106f;
        private const int BallTexture = 96;
        private const float RingWidth = 7f;
        private const float DoneAlpha = 0.75f;

        private static readonly Vector2 IntroPanelSize = new Vector2(900f, 1000f);
        private static readonly Vector2 ResultPanelSize = new Vector2(900f, 1080f);
        private static readonly Color FrozenBall = new Color(0.45f, 0.8f, 1f, 1f);
        private static readonly Color EmptyBall = new Color(0.55f, 0.65f, 0.9f, 0.55f);
        private static Sprite _ballSprite;
        private static Sprite _ringSprite;
        private static readonly Vector2 ButtonSize = new Vector2(640f, 150f);
        private static readonly Color PanelBlue = new Color(0.1f, 0.26f, 0.62f, 1f);
        private static readonly Color Dim = new Color(0f, 0f, 0.05f, 0.8f);
        private static readonly Color Accent = new Color(0.2f, 0.95f, 1f, 1f);
        private static readonly Color DailyAccent = new Color(1f, 0.6f, 0.2f, 1f);
        private static readonly Color TagColor = new Color(0.01f, 0.03f, 0.14f, 0.9f);
        private static readonly Color MutedText = new Color(0.8f, 0.86f, 1f, 1f);
        private static readonly Color WinGold = new Color(1f, 0.84f, 0.25f, 1f);

        [SerializeField] private float _tagPulseSpeed = 2.4f;

        private Transform _canvas;
        private GameObject _screen;
        private TMP_FontAsset _font;
        private TMP_FontAsset _plainFont;
        private Image _frame;
        private Button _buttonTemplate;
        private Outline _tagOutline;

        /// <summary>Set before a scene reload to open the daily panel once the menu is back.</summary>
        public static bool OpenIntroOnLoad;

        private void Awake()
        {
            BuildCanvas();
        }

        private void Start()
        {
            ScoreController score = FindAnyObjectByType<ScoreController>(FindObjectsInactive.Include);
            _font = Themes.ThemeManager.ThemeFont(score != null ? score.scoreText.font : null);
            _plainFont = PlainFont.Asset;
            _frame = Scene.PanelStyle.FindFrame();
            _buttonTemplate = Scene.PanelStyle.FindButton();

            if (!OpenIntroOnLoad) return;
            OpenIntroOnLoad = false;
            ShowIntro();
        }

        private void OnEnable()
        {
            LevelUi.OnModeSelectOpened += AddModeTag;
            DailySession.OnRunFinished += HandleRunFinished;
        }

        private void OnDisable()
        {
            LevelUi.OnModeSelectOpened -= AddModeTag;
            DailySession.OnRunFinished -= HandleRunFinished;
        }

        // The tag glows while today's run is still open.
        private void Update()
        {
            if (_tagOutline == null) return;
            float glow = 0.55f + 0.45f * Mathf.Sin(Time.unscaledTime * _tagPulseSpeed);
            Color color = DailyAccent;
            color.a = glow;
            _tagOutline.effectColor = color;
        }

        private void BuildCanvas()
        {
            GameObject go = new GameObject("DailyCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.transform.SetParent(transform, false);
            Canvas canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = CanvasOrder;
            CanvasScaler scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.matchWidthOrHeight = 0.5f;
            _canvas = go.transform;
        }

        // Top-right corner of the mode screen, opposite its back button.
        private void AddModeTag(Transform modeScreen)
        {
            GameObject go = new GameObject("B_Daily", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
            go.transform.SetParent(modeScreen, false);
            go.GetComponent<Image>().color = TagColor;
            Outline outline = go.GetComponent<Outline>();
            outline.effectColor = DailyAccent;
            outline.effectDistance = new Vector2(4f, 4f);
            _tagOutline = DailySession.TodayIsOpen ? outline : null;

            Button button = go.GetComponent<Button>();
            button.onClick.AddListener(() =>
            {
                Scene.PanelPop.CloseAndDestroy(modeScreen.gameObject);
                ShowIntro();
            });

            RectTransform rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = Vector2.one;
            rect.pivot = Vector2.one;
            rect.sizeDelta = TagSize;
            rect.anchoredPosition = TagOffset;

            TextMeshProUGUI title = CreateText(go.transform, Loc.Caps("Daily"), 50f, Color.white, false);
            Scene.PanelStyle.FitLine(title, 50f);
            Place(title.rectTransform, new Vector2(0f, 20f), new Vector2(TagSize.x - 30f, 60f));

            int streak = DailyStreak.CurrentStreak;
            string status = DailySession.TodayIsOpen
                ? (streak > 0 ? Loc.F("Streak: {0}", streak) : Loc.T("New today"))
                : Loc.T("Completed");
            TextMeshProUGUI sub = CreateText(go.transform, Loc.Upper(status), 30f, DailyAccent, true);
            sub.fontStyle = FontStyles.Bold;
            Scene.PanelStyle.FitLine(sub, 30f);
            Place(sub.rectTransform, new Vector2(0f, -30f), new Vector2(TagSize.x - 30f, 44f));
        }

        public void ShowIntro()
        {
            _tagOutline = null;
            int day = DailyChallenge.Today;
            LevelDefinition challenge = DailySession.TodayChallenge;

            RectTransform panel = OpenPanel(IntroPanelSize);
            float top = IntroPanelSize.y * 0.5f;
            AddHeading(panel, top - 95f);
            AddLine(panel, DateLine(day), 42f, DailyAccent, top - 180f, 60f);
            if (challenge != null)
            {
                AddLine(panel, LevelUi.DescribeGoal(challenge), 46f, Color.white, top - 275f, 70f);
                AddLine(panel, RulesLine(challenge), 32f, MutedText, top - 340f, 50f);
            }
            AddStreakRow(panel, top - 525f);

            bool open = DailySession.TodayIsOpen && challenge != null;
            float y = top - 690f;
            Button play = AddButton(panel, open ? "Play" : "Completed", StartRun, y);
            if (!open) ShowAsDone(play);
            AddBackLink(panel, "Back", CloseScreen, y - 150f);
        }

        // The day's twists in a short line, e.g. "40 pieces · No ghost".
        private static string RulesLine(LevelDefinition level)
        {
            List<string> rules = new List<string>();
            if (level.pieceLimit > 0) rules.Add(Loc.F("{0} pieces", level.pieceLimit));
            if (level.timeLimit > 0) rules.Add(Loc.F("{0} seconds", level.timeLimit));
            if (!string.IsNullOrEmpty(level.pieces)) rules.Add(Loc.F("Only {0}", string.Join(" ", level.pieces.ToCharArray())));
            switch (level.abilityMode)
            {
                case "bomb": rules.Add(Loc.T("Bombs")); break;
                case "laser": rules.Add(Loc.T("Lasers")); break;
                case "patch": rules.Add(Loc.T("Patches")); break;
                case "mixed": rules.Add(Loc.T("Mixed abilities")); break;
                case "bar": rules.Add(Loc.T("Ability bar")); break;
            }
            if (level.garbageEvery > 0) rules.Add(Loc.T("Rising garbage"));
            if (level.hideGhost) rules.Add(Loc.T("No ghost"));
            if (level.hideNext) rules.Add(Loc.T("No preview"));
            if (level.dropLevel >= FastDropLevel) rules.Add(Loc.T("High speed"));
            return string.Join("  \u00b7  ", rules);
        }

        // A finished day keeps the button's place, faded and without a tap.
        private static void ShowAsDone(Button button)
        {
            button.onClick.RemoveAllListeners();
            CanvasGroup group = button.gameObject.AddComponent<CanvasGroup>();
            group.alpha = DoneAlpha;
            group.blocksRaycasts = false;
        }

        private void StartRun()
        {
            CloseScreen();
            if (DailySession.Instance != null) DailySession.Instance.Begin();
        }

        private void HandleRunFinished(DailyResult result)
        {
            StartCoroutine(ShowResultSoon(result));
        }

        // A win gets the level confetti and sound first; a loss only a short beat.
        private IEnumerator ShowResultSoon(DailyResult result)
        {
            if (result.Won)
            {
                FindAnyObjectByType<TetrisCelebrationEffect>(FindObjectsInactive.Include)?.Celebrate();
                AudioSource source = GetComponent<AudioSource>();
                if (SFXToggle.IsSfxEnabled && source != null) source.PlayOneShot(Resources.Load<AudioClip>(WinSoundPath));
            }
            yield return new WaitForSecondsRealtime(result.Won ? WinDelaySeconds : ResultDelaySeconds);
            ShowResult(result);
        }

        private void ShowResult(DailyResult result)
        {
            RectTransform panel = OpenPanel(ResultPanelSize);
            float top = ResultPanelSize.y * 0.5f;
            AddHeading(panel, top - 95f);
            AddLine(panel, DateLine(result.Day), 42f, DailyAccent, top - 180f, 60f);

            TextMeshProUGUI banner = CreateText(panel, Loc.Caps(result.Won ? "Completed!" : "Not this time"), 72f,
                result.Won ? WinGold : Color.white, false);
            Scene.PanelStyle.FitLine(banner, 72f);
            Place(banner.rectTransform, new Vector2(0f, top - 285f), new Vector2(800f, 110f));

            LevelDefinition challenge = DailySession.TodayChallenge;
            string detail = result.Won
                ? Loc.F("Score: {0}", result.Score) + "  \u00b7  " + Loc.F("{0} pieces", result.Pieces)
                : (challenge != null ? LevelUi.DescribeGoal(challenge) : "");
            AddLine(panel, detail, 40f, MutedText, top - 370f, 60f);
            AddStreakRow(panel, top - 570f);

            float y = top - 740f;
            if (result.Won) AddButton(panel, "Share", () => DailyShare.Share(DailyShare.BuildText(result, DailyStreak.CurrentStreak)), y);
            else AddButton(panel, "Try Again", StartRun, y);
            AddBackLink(panel, "Menu", () => { if (LevelSession.Instance != null) LevelSession.Instance.ReturnToMenu(); }, y - 150f);
        }

        private static string DateLine(int day)
        {
            return Loc.Upper(DailyChallenge.DateOf(day).ToString("d MMMM", Loc.Culture));
        }

        // The streak count over the last seven days as balls: filled when played, blue when a
        // freeze covered it, an outline when missed, and today's ring in the daily color.
        private void AddStreakRow(Transform panel, float y)
        {
            TextMeshProUGUI count = CreateText(panel, Loc.Caps("Streak") + "  " + DailyStreak.CurrentStreak, 44f, Color.white, false);
            Place(count.rectTransform, new Vector2(0f, y + 95f), new Vector2(780f, 60f));

            int today = DailyChallenge.Today;
            string[] dayNames = Loc.Culture.DateTimeFormat.AbbreviatedDayNames;
            float left = -(StreakDays - 1) * BallSpacing * 0.5f;
            for (int i = 0; i < StreakDays; i++)
            {
                int day = today - (StreakDays - 1) + i;
                DayMark mark = DailyStreak.MarkOf(day);
                bool isToday = day == today;

                Color color = mark == DayMark.Played ? DailyAccent
                    : mark == DayMark.Frozen ? FrozenBall
                    : isToday ? DailyAccent : EmptyBall;
                Image ball = CreateBall(panel, mark == DayMark.None ? RingSprite : BallSprite, color);
                float x = left + i * BallSpacing;
                Place(ball.rectTransform, new Vector2(x, y), new Vector2(BallSize, BallSize));

                string name = dayNames[(int)DailyChallenge.DateOf(day).DayOfWeek];
                TextMeshProUGUI label = CreateText(panel, Loc.Upper(name), 26f, isToday ? DailyAccent : MutedText, true);
                label.fontStyle = FontStyles.Bold;
                Place(label.rectTransform, new Vector2(x, y - 72f), new Vector2(BallSpacing, 40f));
            }
        }

        private static Image CreateBall(Transform parent, Sprite sprite, Color color)
        {
            GameObject go = new GameObject("Ball", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            Image image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        // Drawn once in white and tinted per ball, so no art files are needed.
        private static Sprite BallSprite => _ballSprite != null ? _ballSprite : (_ballSprite = DrawBall(false));
        private static Sprite RingSprite => _ringSprite != null ? _ringSprite : (_ringSprite = DrawBall(true));

        private static Sprite DrawBall(bool ring)
        {
            Texture2D texture = new Texture2D(BallTexture, BallTexture, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            float radius = BallTexture * 0.5f - 1f;
            Vector2 center = new Vector2(BallTexture * 0.5f, BallTexture * 0.5f);
            Vector2 light = center + new Vector2(-0.35f, 0.35f) * radius; // upper left highlight
            Color[] pixels = new Color[BallTexture * BallTexture];
            for (int y = 0; y < BallTexture; y++)
            {
                for (int x = 0; x < BallTexture; x++)
                {
                    Vector2 p = new Vector2(x + 0.5f, y + 0.5f);
                    float d = Vector2.Distance(p, center);
                    float edge = Mathf.Clamp01(radius - d);
                    float alpha;
                    float shade = 1f;
                    if (ring) alpha = edge * Mathf.Clamp01(d - (radius - RingWidth) + 1f);
                    else
                    {
                        alpha = edge;
                        float toLight = Vector2.Distance(p, light) / (radius * 1.6f);
                        shade = Mathf.Lerp(1f, 0.62f, Mathf.Clamp01(toLight));
                    }
                    pixels[y * BallTexture + x] = new Color(shade, shade, shade, alpha);
                }
            }
            texture.SetPixels(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, BallTexture, BallTexture), new Vector2(0.5f, 0.5f), BallTexture);
        }

        // --- building blocks ---

        private RectTransform OpenPanel(Vector2 size)
        {
            CloseScreen();
            _screen = new GameObject("DailyScreen", typeof(RectTransform), typeof(Image));
            _screen.transform.SetParent(_canvas, false);
            RectTransform rect = (RectTransform)_screen.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            _screen.GetComponent<Image>().color = Dim;
            Scene.PanelPop.Add(_screen, Scene.PanelPop.Style.Fade);

            GameObject go = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(_screen.transform, false);
            Image image = go.GetComponent<Image>();
            image.color = PanelBlue;
            if (_frame != null) Scene.PanelStyle.ApplyFrame(image, _frame);
            RectTransform panel = (RectTransform)go.transform;
            Place(panel, Vector2.zero, size);
            Scene.PanelPop.Add(go, Scene.PanelPop.Style.Pop);
            return panel;
        }

        private void CloseScreen()
        {
            Scene.PanelPop.CloseAndDestroy(_screen);
            _screen = null;
        }

        private void AddHeading(Transform panel, float y)
        {
            TextMeshProUGUI heading = CreateText(panel, Loc.Caps("Daily Challenge"), 66f, Accent, false);
            Scene.PanelStyle.FitLine(heading, 66f);
            Place(heading.rectTransform, new Vector2(0f, y), new Vector2(780f, 100f));
        }

        private void AddLine(Transform panel, string text, float size, Color color, float y, float height)
        {
            TextMeshProUGUI line = CreateText(panel, text, size, color, true);
            Place(line.rectTransform, new Vector2(0f, y), new Vector2(780f, height));
        }

        // Same glowing button as the main menu's Play.
        private Button AddButton(Transform panel, string label, Action onClick, float y)
        {
            Button button;
            if (_buttonTemplate != null)
            {
                button = Instantiate(_buttonTemplate, panel);
                button.gameObject.SetActive(true);
                button.transform.localScale = Vector3.one;
                button.onClick = new Button.ButtonClickedEvent();
                TMP_Text text = button.GetComponentInChildren<TMP_Text>(true);
                if (text != null)
                {
                    text.text = Loc.Caps(label);
                    Scene.PanelStyle.FitButtonLabel(text, MenuButtonFontSize, ButtonSize.x);
                    text.color = Scene.PanelStyle.ButtonTextColor;
                    text.alignment = TextAlignmentOptions.Center;
                    text.rectTransform.anchorMin = Vector2.zero;
                    text.rectTransform.anchorMax = Vector2.one;
                    text.rectTransform.offsetMin = Vector2.zero;
                    text.rectTransform.offsetMax = Vector2.zero;
                }
            }
            else
            {
                GameObject go = new GameObject("Button", typeof(RectTransform), typeof(Image), typeof(Button));
                go.transform.SetParent(panel, false);
                go.GetComponent<Image>().color = PanelBlue;
                button = go.GetComponent<Button>();
                TextMeshProUGUI text = CreateText(go.transform, Loc.Caps(label), 44f, Color.white, true);
                Place(text.rectTransform, Vector2.zero, ButtonSize);
            }
            button.onClick.AddListener(() => onClick());
            Place((RectTransform)button.transform, new Vector2(0f, y), ButtonSize);
            return button;
        }

        private void AddBackLink(Transform panel, string label, Action onClick, float y)
        {
            if (_buttonTemplate == null)
            {
                AddButton(panel, label, onClick, y);
                return;
            }
            Button back = Instantiate(_buttonTemplate, panel);
            back.gameObject.SetActive(true);
            back.transform.localScale = Vector3.one;
            back.onClick = new Button.ButtonClickedEvent();
            back.onClick.AddListener(() => onClick());
            TMP_Text text = back.GetComponentInChildren<TMP_Text>(true);
            if (text != null) text.text = Loc.Caps(label);
            Scene.PanelStyle.MakeBackLink(back);
            Place((RectTransform)back.transform, new Vector2(0f, y), ButtonSize);
        }

        private TextMeshProUGUI CreateText(Transform parent, string text, float size, Color color, bool plain)
        {
            GameObject go = new GameObject("Text", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            TextMeshProUGUI label = go.AddComponent<TextMeshProUGUI>();
            TMP_FontAsset font = plain && _plainFont != null ? _plainFont : _font;
            if (font != null) label.font = font;
            label.fontSize = size;
            label.color = color;
            label.alignment = TextAlignmentOptions.Center;
            label.raycastTarget = false;
            label.text = text;
            return label;
        }

        private static void Place(RectTransform rect, Vector2 position, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
        }
    }
}
