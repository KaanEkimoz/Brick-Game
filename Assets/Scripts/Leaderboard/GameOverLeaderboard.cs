using System;
using Gameplay;
using InGame;
using Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Leaderboard
{
    /// <summary>Adds the player's rank and a leaderboard icon to the Game Over panel.</summary>
    public class GameOverLeaderboard : MonoBehaviour
    {
        private const string RestartButtonName = "B_Restart";
        private const string EndScoreName = "T_EndScore";
        private const string HighScoreName = "T_HighScore";
        private const string IconPath = "Icons/icon_leaderboard_neon";
        private const float RankFontSize = 26f;
        private const float RestartFontSize = 24f;
        private const float HighScoreWidth = 1200f; // keeps a long translated label on one line
        private const float EndScoreY = 30f;
        private const float HighScoreY = -115f;
        private const float RankY = -190f;
        private const float IconY = -256f;
        private const float RestartY = -348f;
        private const float MainMenuY = -438f;
        private static readonly Vector2 RankSize = new Vector2(540f, 60f);
        private static readonly Vector2 IconSize = new Vector2(96f, 96f);

        private GameObject _button;
        private TextMeshProUGUI _rank;

        public void Build(Button mainMenu, Action open)
        {
            Transform panel = mainMenu.transform.parent;
            TMP_Text menuLabel = mainMenu.GetComponentInChildren<TMP_Text>(true);
            Transform restart = panel.Find(RestartButtonName);

            MoveTo(panel.Find(EndScoreName), EndScoreY);
            Transform highScore = panel.Find(HighScoreName);
            MoveTo(highScore, HighScoreY);
            ((RectTransform)highScore).sizeDelta = new Vector2(HighScoreWidth, ((RectTransform)highScore).sizeDelta.y);
            MoveTo(restart, RestartY);
            MoveTo(mainMenu.transform, MainMenuY);

            GameObject icon = new GameObject("B_Leaderboard", typeof(RectTransform), typeof(Image), typeof(Button));
            icon.transform.SetParent(panel, false);
            Image image = icon.GetComponent<Image>();
            image.sprite = Resources.Load<Sprite>(IconPath);
            image.preserveAspect = true;
            icon.GetComponent<Button>().onClick.AddListener(() => open());
            RectTransform rect = (RectTransform)icon.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = IconSize;
            rect.anchoredPosition = new Vector2(0f, IconY);
            _button = icon;

            GameObject go = new GameObject("T_Rank", typeof(RectTransform));
            go.transform.SetParent(panel, false);
            _rank = go.AddComponent<TextMeshProUGUI>();
            _rank.font = menuLabel.font;
            _rank.fontSize = RankFontSize;
            _rank.color = menuLabel.color;
            _rank.alignment = TextAlignmentOptions.Center;
            _rank.raycastTarget = false;
            _rank.rectTransform.anchorMin = _rank.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            _rank.rectTransform.sizeDelta = RankSize;
            _rank.rectTransform.anchoredPosition = new Vector2(0f, RankY);
            _rank.text = string.Empty;

            // Longer translations would wrap into the buttons below.
            Scene.PanelStyle.FitLine(restart.GetComponentInChildren<TMP_Text>(true), RestartFontSize);
        }

        private static void MoveTo(Transform target, float y)
        {
            RectTransform rect = (RectTransform)target;
            rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, y);
        }

        private void OnEnable()
        {
            PiecesController.OnGameOver += HandleGameOver;
            LeaderboardSubmitter.OnRunSettled += ShowRank;
        }

        private void OnDisable()
        {
            PiecesController.OnGameOver -= HandleGameOver;
            LeaderboardSubmitter.OnRunSettled -= ShowRank;
        }

        private void HandleGameOver()
        {
            if (_button == null) return;
            _button.SetActive(!GameMode.IsLevels);
            _rank.text = string.Empty;
        }

        private void ShowRank()
        {
            // Scores wait on the device until the player signs in, so say how to join the board.
            if (!GameMode.IsLevels && !LeaderboardService.IsSignedIn)
            {
                _rank.text = Loc.T("Sign in to Play Games to see the leaderboard");
                Scene.PanelStyle.FitLine(_rank, RankFontSize);
                return;
            }

            LeaderboardService.LoadPlayerRank(LeaderboardService.CurrentBoard, rank =>
            {
                if (_rank != null && rank.HasValue)
                    _rank.text = Loc.Upper(Loc.F("Your rank: #{0}", rank.Value));
            });
        }
    }
}
