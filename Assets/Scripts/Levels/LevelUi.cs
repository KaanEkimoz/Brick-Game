using System;
using System.Collections;
using Extras;
using Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Levels
{
    /// <summary>
    /// All menu screens: the Play button, mode select, level intro and win dialogs and the objective
    /// line. Built at runtime on its own overlay canvas so the shipped scene stays untouched.
    /// </summary>
    public partial class LevelUi : MonoBehaviour
    {
        private const string PlayButtonName = "B_Play";
        private const string ExtendedButtonName = "B_PlayExtended";
        private const string SettingsIconName = "B_SettingsIcon";
        private const string LanguageIconPath = "Icons/icon_language_neon";
        private const string MenuButtonText = "Play";
        private const string ModeImageFolder = "ModeSelect/";
        private const string WinSoundPath = "LevelAudio/level_win";
        private const string WinTitleText = "LEVEL CLEARED!";
        private const string LanguageKey = "Language";
        private const float MenuButtonFontSize = 58f;
        private const float WinCelebrationSeconds = 2f;
        private const float WinPopSeconds = 0.3f;
        private const float WinPopScale = 1.25f;
        private const int CanvasOrder = 100;
        private const float DividerThickness = 0.007f;
        private const float BackIconSize = 110f;
        private const float LanguageIconGap = 12f;
        private const float LanguageRowHeight = 140f;
        private const float LanguageHeadingHeight = 200f;
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
        private static readonly Vector2 IntroPanelSize = new Vector2(800f, 440f);
        private static readonly Vector2 WinPanelSize = new Vector2(880f, 780f);
        private static readonly Vector2 WinButtonSize = new Vector2(640f, 150f);
        private static readonly Vector2 WinRuleSize = new Vector2(520f, 4f);
        private static readonly Color WinTitleTop = new Color(1f, 0.97f, 0.72f, 1f);
        private static readonly Color WinTitleBottom = new Color(1f, 0.7f, 0.1f, 1f);
        private static readonly Color WinTitleOutline = new Color(0.03f, 0.08f, 0.3f, 1f);
        private static readonly Color LevelsAccent = new Color(1f, 0.84f, 0.25f, 1f);
        private static readonly Color ExtendedAccent = new Color(1f, 0.45f, 0.9f, 1f);
        private static readonly Color BadgeColor = new Color(0.01f, 0.03f, 0.14f, 0.88f);
        private static readonly Vector2 BadgePadding = new Vector2(64f, 22f);
        private static readonly Color SelectedLanguageTextColor = new Color(0.8f, 0.1f, 0.55f, 1f);
        private static readonly Color ModeImageTint = new Color(0.68f, 0.68f, 0.74f, 1f);

        private Transform _canvas;
        private TMP_FontAsset _font;
        private TMP_FontAsset _plainFont;
        private Image _panelFrame;
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
            _panelFrame = Scene.PanelStyle.FindFrame();
            BuildCanvas();
            BuildMenuButton();
            BuildLanguageButton();
            BuildLeaderboardButton();
            BuildGameOverLeaderboard();
            gameObject.AddComponent<Leaderboard.LeaderboardSubmitter>();
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
            if (label != null) label.text = Loc.Caps(MenuButtonText);

            _classicButton.gameObject.SetActive(false);
            _extendedButton.gameObject.SetActive(false);
        }

        // Sits under the gear icon and copies its look.
        private void BuildLanguageButton()
        {
            CreateMenuIcon("B_LanguageIcon", LanguageIconPath, 1, ShowLanguageSelect);
        }

        // Icons stack under the gear; slot 1 is the first place below it.
        private void CreateMenuIcon(string name, string spritePath, int slot, UnityEngine.Events.UnityAction onClick)
        {
            foreach (Button settings in Resources.FindObjectsOfTypeAll<Button>())
            {
                if (settings.name != SettingsIconName || !settings.gameObject.scene.IsValid()) continue;

                Button button = Instantiate(settings, settings.transform.parent);
                button.name = name;
                button.onClick = new Button.ButtonClickedEvent();
                button.onClick.AddListener(onClick);
                button.GetComponent<Image>().sprite = Resources.Load<Sprite>(spritePath);

                RectTransform rect = (RectTransform)button.transform;
                rect.anchoredPosition += Vector2.down * ((rect.rect.height + LanguageIconGap) * slot);
                return;
            }
        }

        // Each language keeps its own name, whichever language is active.
        private void ShowLanguageSelect()
        {
            Transform screen = OpenScreen();
            screen.GetComponent<Image>().color = IntroDim;

            Language[] languages = (Language[])Enum.GetValues(typeof(Language));
            int rows = languages.Length + 1;
            float height = LanguageHeadingHeight + rows * LanguageRowHeight + LanguageRowHeight * 0.5f;
            RectTransform panel = CreatePanel(screen, new Vector2(WinPanelSize.x, height));

            float top = height * 0.5f;
            TextMeshProUGUI heading = CreateHeading(panel, Loc.Caps(LanguageKey), 66f);
            Place(heading.rectTransform, new Vector2(0f, top - LanguageHeadingHeight * 0.6f), new Vector2(740f, 90f));

            for (int i = 0; i < rows; i++)
            {
                float y = top - LanguageHeadingHeight - LanguageRowHeight * (i + 0.5f);
                Button row = i < languages.Length
                    ? CreateLanguageRow(panel, languages[i])
                    : CreateMenuStyleButton(panel, "Back", CloseScreen);
                Place((RectTransform)row.transform, new Vector2(0f, y), WinButtonSize);
            }
        }

        private Button CreateLanguageRow(Transform panel, Language language)
        {
            string name = Loc.NativeName(language).ToUpperInvariant();
            Button row = CreateMenuStyleButton(panel, name, () => SelectLanguage(language));
            if (language == Loc.Current)
                row.GetComponentInChildren<TMP_Text>(true).color = SelectedLanguageTextColor;
            return row;
        }

        // Reloading rebuilds every screen in the new language; this list is only in the menu.
        private void SelectLanguage(Language language)
        {
            if (language == Loc.Current)
            {
                CloseScreen();
                return;
            }
            Loc.Select(language);
            _session.ReturnToMenu();
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

        // Runs in the neon font, like the rest of the HUD.
        private static string Objective(string format, int value, int target)
        {
            return Loc.Upper(Loc.F(format, value, target));
        }

        private string DescribeObjective(LevelDefinition level)
        {
            int value = _session.Progress;
            switch (level.objective)
            {
                case "Lines": return Objective("Clear lines: {0}/{1}", value, level.target);
                case "Score": return Objective("Reach score: {0}/{1}", value, level.target);
                case "AbilityCells": return Objective("Ability cells: {0}/{1}", value, level.target);
                case "Tetrises": return Objective("Tetrises: {0}/{1}", value, level.target);
                case "Combo": return Objective("Combo: {0}/{1}", value, level.target);
                case "Gold": return Objective("Gold left: {0}", value, level.target);
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
            CreateModeLabel(screen, Loc.Caps("Classic"), Loc.T("Endless blocks"), classicAnchor, Accent);
            CreateModeLabel(screen, Loc.Caps("Levels"), Loc.F("Level {0}", NextLevel().id), new Vector2(0.5f, 0.51f), LevelsAccent);
            CreateModeLabel(screen, Loc.Caps("Extended"), Loc.T("Bombs and lasers"), extendedAnchor, ExtendedAccent);
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

            TextMeshProUGUI label = CreatePlainText(go.transform, Loc.Upper(text), 44f, accent);
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

            TextMeshProUGUI label = CreatePlainText(screen, Loc.F("High Score: {0}", highScore), 42f, Color.white);
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

            TextMeshProUGUI title = CreateHeading(panel, Loc.Upper(Loc.F("Level {0}", level.id)), 56f);
            Place(title.rectTransform, new Vector2(0f, 135f), new Vector2(680f, 70f));
            TextMeshProUGUI goal = CreatePlainText(panel, DescribeGoal(level), 50f, Color.white);
            Place(goal.rectTransform, new Vector2(0f, 30f), new Vector2(740f, 70f));
            TextMeshProUGUI hint = CreatePlainText(panel, Loc.LevelHint(level.id, level.hint), 36f, Color.white);
            Place(hint.rectTransform, new Vector2(0f, -80f), new Vector2(720f, 120f));
            return panel;
        }

        private RectTransform CreatePanel(Transform parent, Vector2 size)
        {
            GameObject go = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            Image image = go.GetComponent<Image>();
            image.color = PanelBlue;
            if (_panelFrame != null) Scene.PanelStyle.ApplyFrame(image, _panelFrame);
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
                case "Lines": return Loc.F("Clear {0} lines", level.target);
                case "Score": return Loc.F("Reach {0} points", level.target);
                case "AbilityCells": return Loc.F("Hit {0} cells with abilities", level.target);
                case "Tetrises": return level.target == 1 ? Loc.T("Clear a Tetris") : Loc.F("Clear {0} Tetrises", level.target);
                case "Combo": return Loc.F("Clear lines {0} pieces in a row", level.target);
                case "Gold": return Loc.T("Clear all the gold");
                default: return Loc.T("Clear the board");
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
            Place(heading.rectTransform, new Vector2(0f, 255f), new Vector2(720f, 100f));

            GameObject rule = new GameObject("Rule", typeof(RectTransform), typeof(Image));
            rule.transform.SetParent(panel, false);
            rule.GetComponent<Image>().color = LevelsAccent;
            Place((RectTransform)rule.transform, new Vector2(0f, 180f), WinRuleSize);

            TextMeshProUGUI title = CreatePlainText(panel, $"{Loc.F("Level {0}", level.id)}  \u00b7  {Loc.LevelName(level.id, level.name)}", 46f, Color.white);
            Place(title.rectTransform, new Vector2(0f, 120f), new Vector2(820f, 70f));
            TextMeshProUGUI score = CreatePlainText(panel, Loc.F("Score: {0}", ScoreController.score), 40f, Color.white);
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
            TextMeshProUGUI label = CreateHeading(parent, Loc.Caps(WinTitleText), size);
            label.color = Color.white;
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
            TextMeshProUGUI heading = CreateHeading(panel, Loc.Caps(Scene.ConfirmDialog.MainMenuTitle), 66f);
            Place(heading.rectTransform, new Vector2(0f, 245f), new Vector2(720f, 90f));
            TextMeshProUGUI message = CreatePlainText(panel, Loc.T(Scene.ConfirmDialog.MainMenuMessage), 40f, Color.white);
            Place(message.rectTransform, new Vector2(0f, 145f), new Vector2(740f, 70f));

            Button yes = CreateMenuStyleButton(panel, Scene.ConfirmDialog.ConfirmText, _session.ReturnToMenu);
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
                text.text = Loc.Caps(label);
                Scene.PanelStyle.FitButtonLabel(text, MenuButtonFontSize, WinButtonSize.x);
                text.color = Scene.PanelStyle.ButtonTextColor;
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

        private TextMeshProUGUI CreateHeading(Transform parent, string text, float size)
        {
            TextMeshProUGUI label = CreateText(parent, text, size, Accent);
            Scene.PanelStyle.FitLine(label, size);
            return label;
        }

        // Sentences on panels use the plain font; headings and the HUD keep the game's own.
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
