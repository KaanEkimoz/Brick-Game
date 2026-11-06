using System;
using System.Collections;
using Extras;
using Levels;
using Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Daily
{
    /// <summary>
    /// The daily challenge screens: a tag on the mode screen, the rules panel before a run and
    /// the result panel after it. Built at runtime on its own canvas above the Levels screens.
    /// </summary>
    public class DailyUi : MonoBehaviour
    {
        private const int CanvasOrder = 110;
        private const float MenuButtonFontSize = 58f;
        private const float ResultDelaySeconds = 0.6f;

        private static readonly Vector2 ReferenceResolution = new Vector2(1080f, 1920f);
        private static readonly Vector2 TagSize = new Vector2(330f, 118f);
        private static readonly Vector2 TagOffset = new Vector2(-30f, -40f);
        private static readonly Vector2 PanelSize = new Vector2(900f, 1100f);
        private static readonly Vector2 ButtonSize = new Vector2(640f, 150f);
        private static readonly Color PanelBlue = new Color(0.1f, 0.26f, 0.62f, 1f);
        private static readonly Color Dim = new Color(0f, 0f, 0.05f, 0.8f);
        private static readonly Color Accent = new Color(0.2f, 0.95f, 1f, 1f);
        private static readonly Color DailyAccent = new Color(1f, 0.6f, 0.2f, 1f);
        private static readonly Color TagColor = new Color(0.01f, 0.03f, 0.14f, 0.9f);
        private static readonly Color MutedText = new Color(0.8f, 0.86f, 1f, 1f);

        [SerializeField] private float _tagPulseSpeed = 2.4f;

        private Transform _canvas;
        private GameObject _screen;
        private TMP_FontAsset _font;
        private TMP_FontAsset _plainFont;
        private Image _frame;
        private Button _buttonTemplate;
        private Outline _tagOutline;

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
                : Loc.T("Played today");
            TextMeshProUGUI sub = CreateText(go.transform, Loc.Upper(status), 30f, DailyAccent, true);
            sub.fontStyle = FontStyles.Bold;
            Scene.PanelStyle.FitLine(sub, 30f);
            Place(sub.rectTransform, new Vector2(0f, -30f), new Vector2(TagSize.x - 30f, 44f));
        }

        private void ShowIntro()
        {
            _tagOutline = null;
            int day = DailyChallenge.Today;
            bool played = DailyStreak.TryGetResult(day, out DailyResult today);

            RectTransform panel = OpenPanel();
            float top = PanelSize.y * 0.5f;
            AddHeading(panel, top - 95f);
            AddLine(panel, DateLine(day), 40f, DailyAccent, top - 185f, 60f);
            AddLine(panel, Loc.F("Same pieces for everyone today. Score as much as you can in {0} pieces.", DailyChallenge.PieceLimit),
                38f, Color.white, top - 300f, 130f);

            string status = played
                ? Loc.F("Today's score: {0}", today.Score) + "\n" + Loc.T("More runs are practice.")
                : Loc.T("Only your first run today counts.");
            AddLine(panel, status, 36f, MutedText, top - 440f, 110f);
            AddLine(panel, StreakLine(), 36f, Color.white, top - 555f, 100f);

            float y = top - 690f;
            AddButton(panel, played ? "Practice" : "Play", StartRun, y);
            if (played)
            {
                y -= 160f;
                AddButton(panel, "Share", () => DailyShare.Share(DailyShare.BuildText(today, DailyStreak.CurrentStreak)), y);
            }
            AddBackLink(panel, "Back", CloseScreen, y - 150f);
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

        // A short beat lets the last clear and the game over sound land first.
        private IEnumerator ShowResultSoon(DailyResult result)
        {
            yield return new WaitForSecondsRealtime(ResultDelaySeconds);
            ShowResult(result);
        }

        private void ShowResult(DailyResult result)
        {
            if (result.Official || !DailyStreak.TryGetResult(result.Day, out DailyResult official)) official = result;

            RectTransform panel = OpenPanel();
            float top = PanelSize.y * 0.5f;
            AddHeading(panel, top - 95f);
            AddLine(panel, DateLine(result.Day), 40f, DailyAccent, top - 185f, 60f);

            TextMeshProUGUI score = CreateText(panel, Loc.Caps("Score") + "\n" + result.Score, 76f, Color.white, false);
            Place(score.rectTransform, new Vector2(0f, top - 310f), new Vector2(800f, 180f));

            string detail = Loc.F("Lines: {0}", result.Lines) + "    " + Loc.F("Quads: {0}", result.Quads);
            if (!result.Official) detail += "\n" + Loc.F("Practice. Today's score: {0}", official.Score);
            else if (result.FreezesUsed > 0) detail += "\n" + Loc.F("Streak freezes used: {0}", result.FreezesUsed);
            AddLine(panel, detail, 36f, MutedText, top - 455f, 110f);
            AddLine(panel, StreakLine(), 36f, Color.white, top - 565f, 100f);

            float y = top - 690f;
            AddButton(panel, "Share", () => DailyShare.Share(DailyShare.BuildText(official, DailyStreak.CurrentStreak)), y);
            AddButton(panel, "Play Again", StartRun, y - 150f);
            AddBackLink(panel, "Menu", () => { if (LevelSession.Instance != null) LevelSession.Instance.ReturnToMenu(); }, y - 280f);
        }

        private static string DateLine(int day)
        {
            string date = DailyChallenge.DateOf(day).ToString("d MMMM", Loc.Culture);
            return Loc.Upper(date) + "  ·  " + Loc.F("Daily #{0}", day);
        }

        private static string StreakLine()
        {
            return Loc.F("Streak: {0}", DailyStreak.CurrentStreak) + "    " + Loc.F("Best: {0}", DailyStreak.BestStreak)
                + "\n" + Loc.F("Streak freezes: {0}", DailyStreak.Freezes);
        }

        // --- building blocks ---

        private RectTransform OpenPanel()
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
            Place(panel, Vector2.zero, PanelSize);
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
        private void AddButton(Transform panel, string label, Action onClick, float y)
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
