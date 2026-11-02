using System.Collections.Generic;
using Extras;
using Gameplay;
using Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Scene
{
    /// <summary>
    /// Arranges the pause panel: heading and score on top, then the actions by importance, then
    /// sound, then the rarely used restore link. The scene's buttons keep their click chains.
    /// </summary>
    public class PauseMenuLayout : MonoBehaviour
    {
        private const string ResumeName = "B_Resume";
        private const string RestartName = "B_Restart";
        private const string MainMenuName = "B_MainMenu";
        private const string SoundPanelName = "SoundPanel";
        private const string RestoreName = "B_RestorePurchases";
        private const string DialogName = "ConfirmDialog";

        private const float TitleY = 432f;
        private const float ScoreY = 372f;
        private const float ResumeY = 285f;
        private const float RestartY = 198f;
        private const float MainMenuY = 113f;
        private const float HowToPlayY = 28f;
        private const float SoundY = -188f;
        private const float RestoreY = -432f;
        private const float GuideBackY = -385f;
        private const float SoundScale = 1.85f;

        private const float TitleFontSize = 58f;
        private const float ScoreFontSize = 30f;
        private const float PrimaryFontSize = 44f;
        private const float SecondaryFontSize = 34f;
        private const float RestoreFontSize = 24f;
        private const float GuideNameFontSize = 30f;
        private const float GuideEffectFontSize = 26f;
        private const float GuideTopY = 318f;
        private const float GuideRowHeight = 90f;
        private const float GuideIconX = -205f;
        private const float GuideTextX = 62f;
        private const float GuideNameOffset = 18f;

        private static readonly Vector2 PrimarySize = new Vector2(500f, 128f);
        private static readonly Vector2 SecondarySize = new Vector2(470f, 104f);
        private static readonly Vector2 HeadingSize = new Vector2(520f, 80f);
        private static readonly Vector2 ScoreSize = new Vector2(540f, 50f);
        private static readonly Vector2 RestoreSize = new Vector2(420f, 60f);
        private static readonly Vector2 GuideIconSize = new Vector2(78f, 78f);
        private static readonly Vector2 GuideLineSize = new Vector2(400f, 40f);
        private static readonly Color SecondaryColor = new Color(0.02f, 0.08f, 0.32f, 0.9f);
        private static readonly Color RestoreTextColor = new Color(1f, 1f, 1f, 0.7f);

        private struct GuideRow
        {
            public readonly string Icon;
            public readonly string Name;
            public readonly string Effect;

            public GuideRow(string icon, string name, string effect)
            {
                Icon = icon;
                Name = name;
                Effect = effect;
            }
        }

        private static readonly GuideRow[] Controls =
        {
            new GuideRow("Guide/tap", "Tap", "Rotates the piece"),
            new GuideRow("Guide/swipe", "Swipe", "Moves the piece"),
            new GuideRow("Guide/drop", "Swipe down", "Drops the piece"),
        };

        private static readonly GuideRow[] Pieces =
        {
            new GuideRow("Guide/bomb", "Bomb", "Clears a 3x3 area"),
            new GuideRow("Guide/dynamite", "Dynamite", "Clears a 3x4 area"),
            new GuideRow("Guide/laser", "Laser", "Clears a row or column"),
            new GuideRow("Guide/patch", "Patch", "Fills the holes"),
            new GuideRow("Guide/fruit", "Fruit", "Gives bonus points"),
        };

        private static readonly GuideRow LevelGoal = new GuideRow("Icons/icon_leaderboard_neon", "Goal", "Finish it before the limit");

        private TMP_FontAsset _headingFont;
        private Color _headingColor;
        private Button _template;
        private TextMeshProUGUI _score;
        private GameObject _guide;
        private Transform _guideRows;

        private void Awake()
        {
            Button resume = transform.Find(ResumeName).GetComponent<Button>();
            TMP_Text resumeLabel = resume.GetComponentInChildren<TMP_Text>(true);
            _headingFont = resumeLabel.font;
            _headingColor = resumeLabel.color;
            _template = PanelStyle.FindButton();

            CreateHeading(transform, "Paused", TitleY);
            _score = CreateLine(transform, ScoreY, ScoreSize, ScoreFontSize, Color.white);

            StyleButton(resume, new Vector2(0f, ResumeY), PrimarySize, PrimaryFontSize, true);
            StyleButton(transform.Find(RestartName).GetComponent<Button>(), new Vector2(0f, RestartY), SecondarySize, SecondaryFontSize, false);
            StyleButton(transform.Find(MainMenuName).GetComponent<Button>(), new Vector2(0f, MainMenuY), SecondarySize, SecondaryFontSize, false);

            Button howToPlay = CreateButton(transform, "How to Play", new Vector2(0f, HowToPlayY), SecondarySize, SecondaryFontSize, false);
            howToPlay.onClick.AddListener(ShowGuide);

            PlaceSoundPanel();
            StyleRestoreLink();
            BuildGuide();
            KeepDialogsOnTop();
        }

        private void OnEnable()
        {
            if (_guide != null) _guide.SetActive(false);
            _score.text = Loc.F("Score: {0}", ScoreController.score);
            if (!GameMode.IsLevels)
                _score.text += "    " + Loc.F("High Score: {0}", ScoreController.GetCurrentHighScore());
        }

        private void StyleButton(Button button, Vector2 position, Vector2 size, float fontSize, bool primary)
        {
            Image image = button.GetComponent<Image>();
            if (image == null) image = button.gameObject.AddComponent<Image>();
            Image pill = _template.GetComponent<Image>();
            image.sprite = pill.sprite;
            image.type = pill.type;
            image.color = primary
                ? Themes.ThemeManager.PrimaryButtonTint(Color.white)
                : Themes.ThemeManager.SecondaryButtonTint(SecondaryColor);
            button.targetGraphic = image;

            RectTransform rect = (RectTransform)button.transform;
            rect.localScale = Vector3.one;
            Place(rect, position, size);

            TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
            label.color = primary ? Themes.ThemeManager.PrimaryButtonText(PanelStyle.ButtonTextColor) : Color.white;
            label.alignment = TextAlignmentOptions.Center;
            PanelStyle.FitButtonLabel(label, fontSize, size.x);
            label.rectTransform.localScale = Vector3.one;
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.offsetMin = Vector2.zero;
            label.rectTransform.offsetMax = Vector2.zero;
        }

        private Button CreateButton(Transform parent, string text, Vector2 position, Vector2 size, float fontSize, bool primary)
        {
            Button button = Instantiate(_template, parent);
            button.name = "B_" + text.Replace(" ", string.Empty);
            button.gameObject.SetActive(true);
            button.onClick = new Button.ButtonClickedEvent();
            StyleButton(button, position, size, fontSize, primary);
            button.GetComponentInChildren<TMP_Text>(true).text = Loc.Caps(text);
            return button;
        }

        private void PlaceSoundPanel()
        {
            RectTransform sound = (RectTransform)transform.Find(SoundPanelName);
            Vector2 size = sound.rect.size;
            sound.localScale = new Vector3(SoundScale, SoundScale, 1f);
            Place(sound, new Vector2(0f, SoundY), size);
        }

        private void StyleRestoreLink()
        {
            Transform restore = transform.Find(RestoreName);
            restore.GetComponent<Image>().color = Color.clear;
            Place((RectTransform)restore, new Vector2(0f, RestoreY), RestoreSize);

            TMP_Text label = restore.GetComponentInChildren<TMP_Text>(true);
            label.color = RestoreTextColor;
            PanelStyle.FitLine(label, RestoreFontSize);
        }

        // A plain page: the player reads it and goes back to the frozen game.
        private void BuildGuide()
        {
            _guide = ConfirmDialog.CreateBackdrop(transform);
            _guide.name = "HowToPlay";

            CreateHeading(_guide.transform, "How to Play", TitleY);

            GameObject rows = new GameObject("Rows", typeof(RectTransform));
            rows.transform.SetParent(_guide.transform, false);
            Place((RectTransform)rows.transform, Vector2.zero, Vector2.zero);
            _guideRows = rows.transform;

            Button back = CreateButton(_guide.transform, "Back", new Vector2(0f, GuideBackY), SecondarySize, SecondaryFontSize, true);
            PanelStyle.MakeBackLink(back);
            back.onClick.AddListener(() => _guide.SetActive(false));
        }

        private void ShowGuide()
        {
            BuildGuideRows(); // the mode can change between runs
            _guide.transform.SetAsLastSibling();
            _guide.SetActive(true);
        }

        private void BuildGuideRows()
        {
            foreach (Transform row in _guideRows) Destroy(row.gameObject);

            List<GuideRow> rows = new List<GuideRow>();
            if (GameMode.IsLevels) rows.Add(LevelGoal);
            rows.AddRange(Controls);
            if (GameMode.IsExtended) rows.AddRange(Pieces);

            for (int i = 0; i < rows.Count; i++)
                CreateGuideRow(rows[i], GuideTopY - i * GuideRowHeight);
        }

        private void CreateGuideRow(GuideRow row, float y)
        {
            GameObject icon = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            icon.transform.SetParent(_guideRows, false);
            Image image = icon.GetComponent<Image>();
            image.sprite = Resources.Load<Sprite>(row.Icon);
            image.preserveAspect = true;
            image.raycastTarget = false;
            Place((RectTransform)icon.transform, new Vector2(GuideIconX, y), GuideIconSize);

            TextMeshProUGUI name = CreateLine(_guideRows, y + GuideNameOffset, GuideLineSize, GuideNameFontSize, _headingColor);
            name.text = Loc.T(row.Name);
            AlignGuideLine(name, y + GuideNameOffset);

            TextMeshProUGUI effect = CreateLine(_guideRows, y - GuideNameOffset, GuideLineSize, GuideEffectFontSize, Color.white);
            effect.text = Loc.T(row.Effect);
            AlignGuideLine(effect, y - GuideNameOffset);
        }

        private static void AlignGuideLine(TextMeshProUGUI line, float y)
        {
            line.alignment = TextAlignmentOptions.Left;
            line.rectTransform.anchoredPosition = new Vector2(GuideTextX, y);
        }

        private void KeepDialogsOnTop()
        {
            List<Transform> children = new List<Transform>();
            foreach (Transform child in transform) children.Add(child);
            foreach (Transform child in children)
                if (child.name == DialogName) child.SetAsLastSibling();
        }

        private void CreateHeading(Transform parent, string text, float y)
        {
            TextMeshProUGUI heading = CreateLine(parent, y, HeadingSize, TitleFontSize, _headingColor);
            heading.font = _headingFont;
            heading.text = Loc.Caps(text);
            PanelStyle.FitLine(heading, TitleFontSize);
        }

        private static TextMeshProUGUI CreateLine(Transform parent, float y, Vector2 size, float fontSize, Color color)
        {
            GameObject go = new GameObject("Text", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            TextMeshProUGUI label = go.AddComponent<TextMeshProUGUI>();
            label.font = Levels.PlainFont.Asset;
            label.fontSize = fontSize;
            label.color = color;
            label.alignment = TextAlignmentOptions.Center;
            label.raycastTarget = false;
            Place(label.rectTransform, new Vector2(0f, y), size);
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
