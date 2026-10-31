using System;
using System.Collections;
using Extras;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Levels
{
    /// <summary>
    /// All menu screens: the Play button, mode select, level intro and win dialogs and the objective
    /// line. Built at runtime on its own overlay canvas so the shipped scene stays untouched.
    /// </summary>
    public class LevelUi : MonoBehaviour
    {
        private const string PlayButtonName = "B_Play";
        private const string ExtendedButtonName = "B_PlayExtended";
        private const string MenuButtonText = "Play";
        private const string ModeImageFolder = "ModeSelect/";
        private const string WinSoundPath = "LevelAudio/level_win";
        private const string WinTitleText = "LEVEL CLEARED!";
        private const float MenuButtonFontSize = 58f;
        private const float WinCelebrationSeconds = 2f;
        private const float WinPopSeconds = 0.3f;
        private const float WinPopScale = 1.25f;
        private const int CanvasOrder = 100;
        private const float DividerThickness = 0.007f;
        private const float BackIconSize = 110f;
        private const float IntroMinHoldSeconds = 3f;
        private const float IntroMaxHoldSeconds = 7f;
        private const float IntroSecondsPerCharacter = 0.06f;
        private const float IntroTapLockSeconds = 0.5f; // ignores the tap that opened the level
        private const float IntroShrinkSeconds = 0.45f;
        private const float IntroEndScale = 0.3f;
        private const float ObjectiveY = 415f; // from the top, between the HUD and the board

        private static readonly Vector2 ReferenceResolution = new Vector2(1080f, 1920f);
        private static readonly Color Backdrop = new Color(0.01f, 0.02f, 0.1f, 0.96f);
        private static readonly Color PanelBlue = new Color(0.1f, 0.26f, 0.62f, 1f);

        // Slanted borders between the three modes, as heights at the screen's left and right.
        private static readonly Vector2 UpperBorder = new Vector2(0.63f, 0.73f);
        private static readonly Vector2 LowerBorder = new Vector2(0.29f, 0.39f);
        private static readonly Color Accent = new Color(0.2f, 0.95f, 1f, 1f);
        private static readonly Color IntroDim = new Color(0f, 0f, 0.05f, 0.75f);
        private static readonly Vector2 IntroPanelSize = new Vector2(800f, 340f);
        private static readonly Vector2 WinPanelSize = new Vector2(880f, 700f);
        private static readonly Vector2 WinButtonSize = new Vector2(640f, 150f);
        private static readonly Vector2 WinRuleSize = new Vector2(520f, 4f);
        private static readonly Color WinTitleTop = new Color(1f, 0.97f, 0.72f, 1f);
        private static readonly Color WinTitleBottom = new Color(1f, 0.7f, 0.1f, 1f);
        private static readonly Color WinTitleOutline = new Color(0.03f, 0.08f, 0.3f, 1f);
        private static readonly Color LevelsAccent = new Color(1f, 0.84f, 0.25f, 1f);
        private static readonly Color ExtendedAccent = new Color(1f, 0.45f, 0.9f, 1f);
        private static readonly Color BadgeColor = new Color(0.01f, 0.03f, 0.14f, 0.88f);
        private static readonly Vector2 BadgePadding = new Vector2(64f, 22f);
        private static readonly Color MenuButtonTextColor = new Color(0.08f, 0.3f, 0.8f, 1f);
        private static readonly Color ModeImageTint = new Color(0.68f, 0.68f, 0.74f, 1f);

        private Transform _canvas;
        private TMP_FontAsset _font;
        private TMP_FontAsset _plainFont;
        private TextMeshProUGUI _objectiveText;
        private GameObject _screen;
        private LevelSession _session;
        private Button _classicButton;
        private Button _extendedButton;

        private void Awake()
        {
            _session = GetComponent<LevelSession>();
            _font = FindAnyObjectByType<ScoreController>(FindObjectsInactive.Include)?.scoreText.font;
            _plainFont = PlainFont.Asset;
            BuildCanvas();
            BuildMenuButton();
            BuildObjectiveLine();
        }

        private void OnEnable()
        {
            LevelSession.OnProgressChanged += RefreshObjective;
            LevelSession.OnLevelWon += CelebrateWin;
        }

        private void OnDisable()
        {
            LevelSession.OnProgressChanged -= RefreshObjective;
            LevelSession.OnLevelWon -= CelebrateWin;
        }

        // The objective line sits on a canvas above the pause and game over menus.
        private void Update()
        {
            _objectiveText.enabled = !PauseTheGame.IsPaused && !LevelSession.RoundOver;
        }

        private void BuildCanvas()
        {
            GameObject go = new GameObject("LevelCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
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

        // One Play button replaces the Classic and Extended buttons. Those stay hidden in the
        // scene because their click chains are what start the two modes.
        private void BuildMenuButton()
        {
            foreach (Button button in Resources.FindObjectsOfTypeAll<Button>())
            {
                if (!button.gameObject.scene.IsValid()) continue;
                if (button.name == PlayButtonName) _classicButton = button;
                if (button.name == ExtendedButtonName) _extendedButton = button;
            }
            if (_classicButton == null || _extendedButton == null) return;

            GameObject play = Instantiate(_classicButton.gameObject, _classicButton.transform.parent);
            play.name = "B_PlayMain";
            Button click = play.GetComponent<Button>();
            click.onClick = new Button.ButtonClickedEvent();
            click.onClick.AddListener(ShowModeSelect);
            TMP_Text label = play.GetComponentInChildren<TMP_Text>(true);
            if (label != null) label.text = MenuButtonText;

            _classicButton.gameObject.SetActive(false);
            _extendedButton.gameObject.SetActive(false);
        }

        private void BuildObjectiveLine()
        {
            _objectiveText = CreateText(_canvas, "", 34f, Accent);
            RectTransform rect = _objectiveText.rectTransform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.sizeDelta = new Vector2(0f, 110f);
            rect.anchoredPosition = new Vector2(0f, -ObjectiveY);
            _objectiveText.gameObject.SetActive(false);
        }

        private void RefreshObjective()
        {
            LevelDefinition level = _session.Current;
            if (level == null) return;

            _objectiveText.gameObject.SetActive(true);
            string text = DescribeObjective(level);
            if (level.pieceLimit > 0) text += $"\nPieces left: {Mathf.Max(0, level.pieceLimit - _session.PiecesUsed)}";
            if (level.timeLimit > 0) text += $"\nTime: {Mathf.Max(0, _session.TimeLeftSeconds)}";
            _objectiveText.text = text;
        }

        private string DescribeObjective(LevelDefinition level)
        {
            int value = _session.Progress;
            switch (level.objective)
            {
                case "Lines": return $"Clear lines: {value}/{level.target}";
                case "Score": return $"Reach score: {value}/{level.target}";
                case "AbilityCells": return $"Ability cells: {value}/{level.target}";
                case "Tetrises": return $"Tetrises: {value}/{level.target}";
                case "Combo": return $"Combo: {value}/{level.target}";
                case "Gold": return $"Gold left: {value}";
                default: return $"Tiles left: {value}";
            }
        }

        /// <summary>Opens the three-way mode screen.</summary>
        public void ShowModeSelect()
        {
            Transform screen = OpenScreen();
            Vector2 upper = UpperBorder;
            Vector2 lower = LowerBorder;

            CreateModeSlice(screen, "mode_classic", new[]
            {
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(1f, upper.y), new Vector2(0f, upper.x),
            }, StartClassic);
            CreateModeSlice(screen, "mode_levels", new[]
            {
                new Vector2(0f, upper.x), new Vector2(1f, upper.y), new Vector2(1f, lower.y), new Vector2(0f, lower.x),
            }, StartLevels);
            CreateModeSlice(screen, "mode_extended", new[]
            {
                new Vector2(0f, lower.x), new Vector2(1f, lower.y), new Vector2(1f, 0f), new Vector2(0f, 0f),
            }, StartExtended);

            CreateDivider(screen, upper);
            CreateDivider(screen, lower);

            Vector2 classicAnchor = new Vector2(0.5f, 0.84f);
            Vector2 extendedAnchor = new Vector2(0.5f, 0.2f);
            CreateModeLabel(screen, "Classic", "Endless blocks", classicAnchor, Accent);
            CreateModeLabel(screen, "Levels", $"Level {NextLevel().id}", new Vector2(0.5f, 0.51f), LevelsAccent);
            CreateModeLabel(screen, "Extended", "Bombs and lasers", extendedAnchor, ExtendedAccent);
            CreateHighScoreLabel(screen, ScoreController.ClassicHighScore, classicAnchor);
            CreateHighScoreLabel(screen, ScoreController.ExtendedHighScore, extendedAnchor);

            CreateBackIcon(screen);
        }

        // Neon badge with a left chevron, drawn from two slanted quads.
        private void CreateBackIcon(Transform screen)
        {
            Button back = CreateButton(screen, "", CloseScreen);
            back.GetComponent<Image>().color = BadgeColor;
            Outline border = back.gameObject.AddComponent<Outline>();
            border.effectColor = Accent;
            border.effectDistance = new Vector2(3f, 3f);

            RectTransform rect = (RectTransform)back.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = new Vector2(BackIconSize, BackIconSize);
            rect.anchoredPosition = new Vector2(30f, -40f);

            Vector2 tipOuter = new Vector2(0.28f, 0.5f);
            Vector2 tipInner = new Vector2(0.44f, 0.5f);
            CreateSlice(back.transform, null, new[]
            {
                tipOuter, new Vector2(0.52f, 0.8f), new Vector2(0.68f, 0.8f), tipInner,
            }, Accent).raycastTarget = false;
            CreateSlice(back.transform, null, new[]
            {
                tipOuter, tipInner, new Vector2(0.68f, 0.2f), new Vector2(0.52f, 0.2f),
            }, Accent).raycastTarget = false;
        }

        private void CreateModeSlice(Transform screen, string image, Vector2[] corners, Action onClick)
        {
            DiagonalSlice slice = CreateSlice(screen, Resources.Load<Texture2D>(ModeImageFolder + image), corners, ModeImageTint);
            Button button = slice.gameObject.AddComponent<Button>();
            button.targetGraphic = slice;
            button.onClick.AddListener(() => onClick());
        }

        private void CreateDivider(Transform screen, Vector2 border)
        {
            float half = DividerThickness * 0.5f;
            DiagonalSlice divider = CreateSlice(screen, null, new[]
            {
                new Vector2(0f, border.x + half), new Vector2(1f, border.y + half),
                new Vector2(1f, border.y - half), new Vector2(0f, border.x - half),
            }, Accent);
            divider.raycastTarget = false;
        }

        private static DiagonalSlice CreateSlice(Transform parent, Texture texture, Vector2[] corners, Color color)
        {
            GameObject go = new GameObject("Slice", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            RectTransform rect = (RectTransform)go.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            DiagonalSlice slice = go.AddComponent<DiagonalSlice>();
            slice.color = color;
            slice.Setup(texture, corners);
            return slice;
        }

        private void CreateModeLabel(Transform screen, string title, string subtitle, Vector2 anchor, Color accent)
        {
            TextMeshProUGUI titleText = CreateText(screen, title, 118f, Color.white);
            titleText.outlineWidth = 0.25f;
            titleText.outlineColor = Color.black;
            PlaceAt(titleText.rectTransform, anchor, new Vector2(0f, 36f), new Vector2(1060f, 160f));

            CreateModeBadge(screen, subtitle, anchor, accent);
        }

        // Neon tag under a mode title, so the text stays readable over the busy art.
        private void CreateModeBadge(Transform screen, string text, Vector2 anchor, Color accent)
        {
            GameObject go = new GameObject("Badge", typeof(RectTransform), typeof(Image), typeof(Outline));
            go.transform.SetParent(screen, false);
            Image badge = go.GetComponent<Image>();
            badge.color = BadgeColor;
            badge.raycastTarget = false;
            Outline border = go.GetComponent<Outline>();
            border.effectColor = accent;
            border.effectDistance = new Vector2(3f, 3f);

            TextMeshProUGUI label = CreatePlainText(go.transform, text.ToUpperInvariant(), 44f, accent);
            label.fontStyle = FontStyles.Bold;
            label.characterSpacing = 8f;
            label.textWrappingMode = TextWrappingModes.NoWrap;

            Vector2 textSize = label.GetPreferredValues();
            Vector2 size = new Vector2(textSize.x + BadgePadding.x, textSize.y + BadgePadding.y);
            PlaceAt(badge.rectTransform, anchor, new Vector2(0f, -86f), size);
            Place(label.rectTransform, Vector2.zero, size);
        }

        // Shown only once the mode has a saved score.
        private void CreateHighScoreLabel(Transform screen, int highScore, Vector2 anchor)
        {
            if (highScore <= 0) return;

            TextMeshProUGUI label = CreatePlainText(screen, $"High Score: {highScore}", 42f, Color.white);
            label.fontStyle = FontStyles.Bold;
            label.outlineWidth = 0.2f;
            label.outlineColor = Color.black;
            PlaceAt(label.rectTransform, anchor, new Vector2(0f, -166f), new Vector2(1000f, 64f));
        }

        private void StartClassic()
        {
            CloseScreen();
            _classicButton.onClick.Invoke();
        }

        private void StartExtended()
        {
            CloseScreen();
            _extendedButton.onClick.Invoke();
        }

        private void StartLevels()
        {
            ShowIntro(NextLevel());
        }

        // The level to continue from, or the last one once every level is cleared.
        private static LevelDefinition NextLevel()
        {
            int last = LevelCatalog.All.Count;
            return LevelCatalog.Get(Mathf.Min(LevelProgress.Unlocked, last));
        }

        // The board is ready behind a dim layer while a small goal panel shows. The panel then
        // shrinks into the objective line and play starts by itself.
        private void ShowIntro(LevelDefinition level)
        {
            Transform screen = OpenScreen();
            Image dim = screen.GetComponent<Image>();
            dim.color = IntroDim;

            _session.Begin(level);
            _session.SetIntroHold(true);
            float readSeconds = (DescribeGoal(level).Length + level.hint.Length) * IntroSecondsPerCharacter;
            float holdSeconds = Mathf.Clamp(readSeconds, IntroMinHoldSeconds, IntroMaxHoldSeconds);
            StartCoroutine(PlayIntro(dim, BuildIntroPanel(screen, level), holdSeconds));
        }

        private RectTransform BuildIntroPanel(Transform screen, LevelDefinition level)
        {
            RectTransform panel = CreatePanel(screen, IntroPanelSize);

            TextMeshProUGUI title = CreatePlainText(panel, $"Level {level.id}", 46f, Accent);
            Place(title.rectTransform, new Vector2(0f, 110f), new Vector2(740f, 70f));
            TextMeshProUGUI goal = CreatePlainText(panel, DescribeGoal(level), 50f, Color.white);
            Place(goal.rectTransform, new Vector2(0f, 30f), new Vector2(740f, 70f));
            TextMeshProUGUI hint = CreatePlainText(panel, level.hint, 36f, Color.white);
            Place(hint.rectTransform, new Vector2(0f, -80f), new Vector2(720f, 120f));
            return panel;
        }

        private static RectTransform CreatePanel(Transform parent, Vector2 size)
        {
            GameObject go = new GameObject("Panel", typeof(RectTransform), typeof(Image), typeof(Outline));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = PanelBlue;
            Outline border = go.GetComponent<Outline>();
            border.effectColor = Accent;
            border.effectDistance = new Vector2(3f, 3f);
            RectTransform panel = (RectTransform)go.transform;
            Place(panel, Vector2.zero, size);
            return panel;
        }

        // Stays long enough to read, and a tap moves on sooner.
        private IEnumerator PlayIntro(Image dim, RectTransform panel, float holdSeconds)
        {
            float held = 0f;
            while (held < holdSeconds)
            {
                held += Time.unscaledDeltaTime;
                if (held > IntroTapLockSeconds && Input.GetMouseButtonDown(0)) break;
                yield return null;
            }

            Vector3 from = panel.position;
            Vector3 to = _objectiveText.rectTransform.position;
            Color startDim = dim.color;
            Color endDim = startDim;
            endDim.a = 0f;

            float elapsed = 0f;
            while (elapsed < IntroShrinkSeconds)
            {
                elapsed += Time.unscaledDeltaTime;
                float n = Mathf.Clamp01(elapsed / IntroShrinkSeconds);
                float eased = n * n * (3f - 2f * n);
                panel.position = Vector3.Lerp(from, to, eased);
                panel.localScale = Vector3.one * Mathf.Lerp(1f, IntroEndScale, eased);
                dim.color = Color.Lerp(startDim, endDim, eased);
                yield return null;
            }

            CloseScreen();
            _session.SetIntroHold(false);
        }

        private static string DescribeGoal(LevelDefinition level)
        {
            switch (level.objective)
            {
                case "Lines": return $"Clear {level.target} lines";
                case "Score": return $"Reach {level.target} points";
                case "AbilityCells": return $"Hit {level.target} cells with abilities";
                case "Tetrises": return level.target == 1 ? "Clear a Tetris" : $"Clear {level.target} Tetrises";
                case "Combo": return $"Clear lines {level.target} pieces in a row";
                case "Gold": return "Clear all the gold";
                default: return "Clear the board";
            }
        }

        private void CelebrateWin(LevelDefinition level)
        {
            StartCoroutine(PlayWinCelebration(level));
        }

        private IEnumerator PlayWinCelebration(LevelDefinition level)
        {
            // A clear layer keeps the pause button from opening during the celebration.
            Transform screen = OpenScreen();
            screen.GetComponent<Image>().color = Color.clear;

            FindAnyObjectByType<TetrisCelebrationEffect>(FindObjectsInactive.Include)?.Celebrate();
            if (SFXToggle.IsSfxEnabled) GetComponent<AudioSource>().PlayOneShot(Resources.Load<AudioClip>(WinSoundPath));

            TextMeshProUGUI banner = CreateWinTitle(screen, 96f);
            Place(banner.rectTransform, Vector2.zero, new Vector2(1060f, 180f));

            float elapsed = 0f;
            while (elapsed < WinCelebrationSeconds)
            {
                elapsed += Time.unscaledDeltaTime;
                float pop = Mathf.Clamp01(elapsed / WinPopSeconds);
                float scale = pop < 0.7f
                    ? Mathf.Lerp(0f, WinPopScale, pop / 0.7f)
                    : Mathf.Lerp(WinPopScale, 1f, (pop - 0.7f) / 0.3f);
                banner.transform.localScale = Vector3.one * scale;
                yield return null;
            }

            ShowWin(level);
        }

        private void ShowWin(LevelDefinition level)
        {
            Transform screen = OpenScreen();
            screen.GetComponent<Image>().color = IntroDim;

            RectTransform panel = CreatePanel(screen, WinPanelSize);
            TextMeshProUGUI heading = CreateWinTitle(panel, 76f);
            Place(heading.rectTransform, new Vector2(0f, 245f), new Vector2(840f, 100f));

            GameObject rule = new GameObject("Rule", typeof(RectTransform), typeof(Image));
            rule.transform.SetParent(panel, false);
            rule.GetComponent<Image>().color = LevelsAccent;
            Place((RectTransform)rule.transform, new Vector2(0f, 180f), WinRuleSize);

            TextMeshProUGUI title = CreatePlainText(panel, $"Level {level.id}  \u00b7  {level.name}", 46f, Color.white);
            Place(title.rectTransform, new Vector2(0f, 120f), new Vector2(820f, 70f));
            TextMeshProUGUI score = CreatePlainText(panel, $"Score: {ScoreController.score}", 40f, Color.white);
            Place(score.rectTransform, new Vector2(0f, 55f), new Vector2(820f, 60f));

            LevelDefinition next = LevelCatalog.Get(level.id + 1);
            if (next != null)
            {
                Button nextButton = CreateMenuStyleButton(panel, "Next", () => ShowIntro(next));
                Place((RectTransform)nextButton.transform, new Vector2(0f, -75f), WinButtonSize);
            }

            Button menu = CreateMenuStyleButton(panel, "Menu", () => ShowMenuConfirm(screen));
            Place((RectTransform)menu.transform, new Vector2(0f, next != null ? -225f : -150f), WinButtonSize);
        }

        private TextMeshProUGUI CreateWinTitle(Transform parent, float size)
        {
            TextMeshProUGUI label = CreatePlainText(parent, WinTitleText, size, Color.white);
            label.fontStyle = FontStyles.Bold;
            label.enableVertexGradient = true;
            label.colorGradient = new VertexGradient(WinTitleTop, WinTitleTop, WinTitleBottom, WinTitleBottom);
            label.outlineWidth = 0.2f;
            label.outlineColor = WinTitleOutline;
            return label;
        }

        private void ShowMenuConfirm(Transform screen)
        {
            GameObject dim = new GameObject("MenuConfirm", typeof(RectTransform), typeof(Image));
            dim.transform.SetParent(screen, false);
            RectTransform rect = (RectTransform)dim.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            dim.GetComponent<Image>().color = IntroDim;

            RectTransform panel = CreatePanel(dim.transform, WinPanelSize);
            TextMeshProUGUI heading = CreatePlainText(panel, Scene.ConfirmDialog.MainMenuTitle, 66f, Accent);
            Place(heading.rectTransform, new Vector2(0f, 235f), new Vector2(740f, 90f));
            TextMeshProUGUI message = CreatePlainText(panel, Scene.ConfirmDialog.MainMenuMessage, 40f, Color.white);
            Place(message.rectTransform, new Vector2(0f, 145f), new Vector2(740f, 70f));

            Button yes = CreateMenuStyleButton(panel, Scene.ConfirmDialog.MainMenuYes, _session.ReturnToMenu);
            Place((RectTransform)yes.transform, new Vector2(0f, -40f), WinButtonSize);
            Button no = CreateMenuStyleButton(panel, Scene.ConfirmDialog.NoText, () => Destroy(dim));
            Place((RectTransform)no.transform, new Vector2(0f, -190f), WinButtonSize);
        }

        // Same glowing button as the main menu's Play.
        private Button CreateMenuStyleButton(Transform parent, string label, Action onClick)
        {
            if (_classicButton == null) return CreateButton(parent, label, onClick);

            Button button = Instantiate(_classicButton, parent);
            button.gameObject.SetActive(true);
            button.transform.localScale = Vector3.one;
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(() => onClick());

            TMP_Text text = button.GetComponentInChildren<TMP_Text>(true);
            if (text != null)
            {
                text.text = label;
                text.enableAutoSizing = false;
                text.fontSize = MenuButtonFontSize;
                text.color = MenuButtonTextColor;
                text.alignment = TextAlignmentOptions.Center;
                text.rectTransform.anchorMin = Vector2.zero;
                text.rectTransform.anchorMax = Vector2.one;
                text.rectTransform.offsetMin = Vector2.zero;
                text.rectTransform.offsetMax = Vector2.zero;
            }
            return button;
        }

        private Transform OpenScreen()
        {
            CloseScreen();
            _screen = new GameObject("LevelScreen", typeof(RectTransform), typeof(Image));
            _screen.transform.SetParent(_canvas, false);
            RectTransform rect = (RectTransform)_screen.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            _screen.GetComponent<Image>().color = Backdrop;
            return _screen.transform;
        }

        private void CloseScreen()
        {
            if (_screen != null) Destroy(_screen);
            _screen = null;
        }

        private Button CreateButton(Transform parent, string label, Action onClick)
        {
            GameObject go = new GameObject("Button", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = PanelBlue;
            Button button = go.GetComponent<Button>();
            button.onClick.AddListener(() => onClick());

            TextMeshProUGUI text = CreatePlainText(go.transform, label, 44f, Color.white);
            RectTransform rect = text.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return button;
        }

        // Panels and buttons use the plain font; the in-game HUD keeps the game's own.
        private TextMeshProUGUI CreatePlainText(Transform parent, string text, float size, Color color)
        {
            TextMeshProUGUI label = CreateText(parent, text, size, color);
            if (_plainFont != null) label.font = _plainFont;
            return label;
        }

        private TextMeshProUGUI CreateText(Transform parent, string text, float size, Color color)
        {
            GameObject go = new GameObject("Text", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            TextMeshProUGUI label = go.AddComponent<TextMeshProUGUI>();
            if (_font != null) label.font = _font;
            label.fontSize = size;
            label.color = color;
            label.alignment = TextAlignmentOptions.Center;
            label.raycastTarget = false;
            label.text = text;
            return label;
        }

        private static void Place(RectTransform rect, Vector2 position, Vector2 size)
        {
            PlaceAt(rect, new Vector2(0.5f, 0.5f), position, size);
        }

        private static void PlaceAt(RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
        }
    }
}
