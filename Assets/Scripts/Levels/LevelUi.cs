using System;
using Extras;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Levels
{
    /// <summary>
    /// All Levels screens: the menu button, level list, intro and win dialogs and the objective
    /// line. Built at runtime on its own overlay canvas so the shipped scene stays untouched.
    /// </summary>
    public class LevelUi : MonoBehaviour
    {
        private const string PlayButtonName = "B_Play";
        private const string ExtendedButtonName = "B_PlayExtended";
        private const string MenuButtonText = "Levels";
        private const int CanvasOrder = 100;
        private const int GridColumns = 5;
        private const float CellSize = 170f;
        private const float CellGap = 26f;
        private const float ObjectiveY = 415f; // from the top, between the HUD and the board

        private static readonly Vector2 ReferenceResolution = new Vector2(1080f, 1920f);
        private static readonly Color Backdrop = new Color(0.01f, 0.02f, 0.1f, 0.96f);
        private static readonly Color PanelBlue = new Color(0.1f, 0.26f, 0.62f, 1f);
        private static readonly Color Unlocked = new Color(0.16f, 0.42f, 0.85f, 1f);
        private static readonly Color Locked = new Color(0.08f, 0.1f, 0.2f, 1f);
        private static readonly Color Cleared = new Color(0.1f, 0.62f, 0.55f, 1f);
        private static readonly Color Accent = new Color(0.2f, 0.95f, 1f, 1f);

        private Transform _canvas;
        private TMP_FontAsset _font;
        private TextMeshProUGUI _objectiveText;
        private GameObject _screen;
        private LevelSession _session;

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

        private void BuildMenuButton()
        {
            GameObject play = null;
            GameObject extended = null;
            foreach (Button button in Resources.FindObjectsOfTypeAll<Button>())
            {
                if (!button.gameObject.scene.IsValid()) continue;
                if (button.name == PlayButtonName) play = button.gameObject;
                if (button.name == ExtendedButtonName) extended = button.gameObject;
            }
            if (play == null || extended == null) return;

            GameObject levels = Instantiate(play, play.transform.parent);
            levels.name = "B_PlayLevels";
            RectTransform rect = (RectTransform)levels.transform;
            RectTransform playRect = (RectTransform)play.transform;
            RectTransform extendedRect = (RectTransform)extended.transform;
            rect.anchoredPosition = playRect.anchoredPosition + (playRect.anchoredPosition - extendedRect.anchoredPosition);

            Button click = levels.GetComponent<Button>();
            click.onClick = new Button.ButtonClickedEvent();
            click.onClick.AddListener(ShowLevelSelect);
            TMP_Text label = levels.GetComponentInChildren<TMP_Text>(true);
            if (label != null) label.text = MenuButtonText;
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

        /// <summary>Opens the scrollable list of all levels.</summary>
        public void ShowLevelSelect()
        {
            Transform screen = OpenScreen();

            TextMeshProUGUI title = CreateText(screen, "Levels", 64f, Color.white);
            Place(title.rectTransform, new Vector2(0f, 800f), new Vector2(900f, 110f));

            RectTransform content = BuildScrollContent(screen);
            int unlocked = LevelProgress.Unlocked;
            foreach (LevelDefinition level in LevelCatalog.All)
                CreateLevelCell(content, level, unlocked);

            Button back = CreateButton(screen, "Back", CloseScreen);
            Place((RectTransform)back.transform, new Vector2(0f, -840f), new Vector2(360f, 96f));
        }

        private RectTransform BuildScrollContent(Transform parent)
        {
            GameObject viewport = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D), typeof(Image));
            viewport.transform.SetParent(parent, false);
            viewport.GetComponent<Image>().color = Color.clear;
            RectTransform viewportRect = (RectTransform)viewport.transform;
            Place(viewportRect, new Vector2(0f, -10f), new Vector2(960f, 1480f));

            int rows = Mathf.CeilToInt((float)LevelCatalog.All.Count / GridColumns);
            GameObject content = new GameObject("Content", typeof(RectTransform), typeof(GridLayoutGroup));
            content.transform.SetParent(viewport.transform, false);
            RectTransform contentRect = (RectTransform)content.transform;
            contentRect.anchorMin = new Vector2(0.5f, 1f);
            contentRect.anchorMax = new Vector2(0.5f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.sizeDelta = new Vector2(960f, rows * (CellSize + CellGap));

            GridLayoutGroup grid = content.GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(CellSize, CellSize);
            grid.spacing = new Vector2(CellGap, CellGap);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = GridColumns;
            grid.childAlignment = TextAnchor.UpperCenter;

            ScrollRect scroll = viewport.AddComponent<ScrollRect>();
            scroll.viewport = viewportRect;
            scroll.content = contentRect;
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            return contentRect;
        }

        private void CreateLevelCell(Transform parent, LevelDefinition level, int unlocked)
        {
            bool open = level.id <= unlocked;
            Button cell = CreateButton(parent, level.id.ToString(), () => ShowIntro(level));
            cell.interactable = open;
            Color color = !open ? Locked : level.id < unlocked ? Cleared : Unlocked;
            cell.GetComponent<Image>().color = color;
            ColorBlock colors = cell.colors;
            colors.disabledColor = Color.white;
            cell.colors = colors;
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

            Button back = CreateButton(screen, "Back", ShowLevelSelect);
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

            Button levels = CreateButton(screen, "All levels", _session.ReturnToLevelSelect);
            Place((RectTransform)levels.transform, new Vector2(0f, -300f), new Vector2(520f, 96f));
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
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
        }
    }
}
