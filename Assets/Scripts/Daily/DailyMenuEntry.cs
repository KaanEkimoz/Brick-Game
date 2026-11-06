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
    /// The other ways into the daily challenge: an orange button right under Play on the main
    /// menu, and a strip at the top of the screen after a Classic or Extended game over while
    /// today's challenge is still open.
    /// </summary>
    public class DailyMenuEntry : MonoBehaviour
    {
        private const string SettingsButtonName = "B_Settings";
        private const string MenuButtonName = "B_DailyMain";
        private const float MenuFontSize = 44f;
        private const int StripCanvasOrder = 120;
        private const float StripDelaySeconds = 1f;
        private const float DoneAlpha = 0.6f;

        private static readonly Vector2 ReferenceResolution = new Vector2(1080f, 1920f);
        private static readonly Vector2 StripSize = new Vector2(860f, 120f);
        private static readonly Vector2 StripOffset = new Vector2(0f, -36f);
        private static readonly Color DailyOrange = new Color(1f, 0.58f, 0.18f, 1f);
        private static readonly Color DailyGlow = new Color(1f, 0.78f, 0.45f, 1f);
        private static readonly Color ButtonText = new Color(0.36f, 0.12f, 0.02f, 1f);
        private static readonly Color StripColor = new Color(0.01f, 0.03f, 0.14f, 0.94f);

        [SerializeField] private float _pulseSpeed = 2.6f;
        [Tooltip("How far Settings moves down to make room, as a share of its own height.")]
        [SerializeField] private float _settingsShift = 0.8f;

        private DailyUi _ui;
        private Image _menuImage;
        private GameObject _strip;

        private void Awake()
        {
            _ui = GetComponent<DailyUi>();
        }

        // After LevelUi has built the Play button and the theme has styled the menu.
        private void Start()
        {
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
            if (_menuImage != null)
            {
                float t = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * _pulseSpeed);
                _menuImage.color = Color.Lerp(DailyOrange, DailyGlow, t);
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

            TMP_Text label = daily.GetComponentInChildren<TMP_Text>(true);
            if (label != null)
            {
                label.text = Loc.Caps("Daily");
                label.color = ButtonText;
                Scene.PanelStyle.FitButtonLabel(label, MenuFontSize, ((RectTransform)daily.transform).rect.width);
            }

            // Pulses while today is open; fades once it is done.
            Image image = daily.GetComponent<Image>();
            image.color = DailyOrange;
            if (DailySession.TodayIsOpen) _menuImage = image;
            else daily.gameObject.AddComponent<CanvasGroup>().alpha = DoneAlpha;
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
            text.color = DailyGlow;
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
