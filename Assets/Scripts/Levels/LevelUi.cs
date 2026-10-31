using System;
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
        private const int CanvasOrder = 100;
        private const float DividerThickness = 0.007f;
        private const float ObjectiveY = 415f; // from the top, between the HUD and the board

        private static readonly Vector2 ReferenceResolution = new Vector2(1080f, 1920f);
        private static readonly Color Backdrop = new Color(0.01f, 0.02f, 0.1f, 0.96f);
        private static readonly Color PanelBlue = new Color(0.1f, 0.26f, 0.62f, 1f);

        // Slanted borders between the three modes, as heights at the screen's left and right.
        private static readonly Vector2 UpperBorder = new Vector2(0.63f, 0.73f);
        private static readonly Vector2 LowerBorder = new Vector2(0.29f, 0.39f);
        private static readonly Color Accent = new Color(0.2f, 0.95f, 1f, 1f);

        private Transform _canvas;
        private TMP_FontAsset _font;
        private TextMeshProUGUI _objectiveText;
        private GameObject _screen;
        private LevelSession _session;
        private Button _classicButton;
        private Button _extendedButton;

        private void Awake()
        {
            _session = GetComponent<LevelSession>();
            _font = FindAnyObjectByType<ScoreController>(FindObjectsInactive.Include)?.scoreText.font;
            BuildCanvas();
            BuildMenuButton();
            BuildObjectiveLine();
        }

        private void OnEnable()
        {
            LevelSession.OnProgressChanged += RefreshObjective;
            LevelSession.OnLevelWon += ShowWin;
        }

        private void OnDisable()
        {
            LevelSession.OnProgressChanged -= RefreshObjective;
            LevelSession.OnLevelWon -= ShowWin;
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

            CreateModeLabel(screen, "Classic", "Endless blocks", new Vector2(0.5f, 0.84f));
            CreateModeLabel(screen, "Levels", $"Level {NextLevel().id}", new Vector2(0.5f, 0.51f));
            CreateModeLabel(screen, "Extended", "Bombs and lasers", new Vector2(0.5f, 0.2f));

            Button back = CreateButton(screen, "Back", CloseScreen);
            RectTransform backRect = (RectTransform)back.transform;
            backRect.anchorMin = backRect.anchorMax = new Vector2(0f, 1f);
            backRect.pivot = new Vector2(0f, 1f);
            backRect.sizeDelta = new Vector2(200f, 84f);
            backRect.anchoredPosition = new Vector2(30f, -40f);
        }

        private void CreateModeSlice(Transform screen, string image, Vector2[] corners, Action onClick)
        {
            DiagonalSlice slice = CreateSlice(screen, Resources.Load<Texture2D>(ModeImageFolder + image), corners, Color.white);
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

        private void CreateModeLabel(Transform screen, string title, string subtitle, Vector2 anchor)
        {
            TextMeshProUGUI titleText = CreateText(screen, title, 84f, Color.white);
            titleText.outlineWidth = 0.25f;
            titleText.outlineColor = Color.black;
            PlaceAt(titleText.rectTransform, anchor, new Vector2(0f, 24f), new Vector2(1000f, 120f));

            TextMeshProUGUI subtitleText = CreateText(screen, subtitle, 36f, Accent);
            subtitleText.outlineWidth = 0.25f;
            subtitleText.outlineColor = Color.black;
            PlaceAt(subtitleText.rectTransform, anchor, new Vector2(0f, -58f), new Vector2(1000f, 60f));
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

        private void ShowIntro(LevelDefinition level)
        {
            Transform screen = OpenScreen();
            AddDialogText(screen, $"Level {level.id}", level.name, level.hint);

            Button start = CreateButton(screen, "Start", () =>
            {
                CloseScreen();
                _session.Begin(level);
            });
            Place((RectTransform)start.transform, new Vector2(0f, -160f), new Vector2(520f, 110f));

            Button back = CreateButton(screen, "Back", ShowModeSelect);
            Place((RectTransform)back.transform, new Vector2(0f, -300f), new Vector2(520f, 96f));
        }

        private void ShowWin(LevelDefinition level)
        {
            Transform screen = OpenScreen();
            AddDialogText(screen, "Level complete!", level.name, $"Score: {ScoreController.score}");

            LevelDefinition next = LevelCatalog.Get(level.id + 1);
            if (next != null)
            {
                Button nextButton = CreateButton(screen, "Next level", () => ShowIntro(next));
                Place((RectTransform)nextButton.transform, new Vector2(0f, -160f), new Vector2(520f, 110f));
            }

            Button menu = CreateButton(screen, "Main menu", _session.ReturnToMenu);
            Place((RectTransform)menu.transform, new Vector2(0f, -300f), new Vector2(520f, 96f));
        }

        private void AddDialogText(Transform screen, string heading, string title, string body)
        {
            TextMeshProUGUI headingText = CreateText(screen, heading, 56f, Accent);
            Place(headingText.rectTransform, new Vector2(0f, 330f), new Vector2(980f, 120f));
            TextMeshProUGUI titleText = CreateText(screen, title, 46f, Color.white);
            Place(titleText.rectTransform, new Vector2(0f, 200f), new Vector2(980f, 100f));
            TextMeshProUGUI bodyText = CreateText(screen, body, 34f, Color.white);
            Place(bodyText.rectTransform, new Vector2(0f, 40f), new Vector2(900f, 190f));
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

            TextMeshProUGUI text = CreateText(go.transform, label, 40f, Color.white);
            RectTransform rect = text.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return button;
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
