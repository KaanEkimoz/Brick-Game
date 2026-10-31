using System;
using Gameplay;
using InGame;
using Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Leaderboard
{
    /// <summary>Adds the player's rank and a leaderboard button to the Game Over panel.</summary>
    public class GameOverLeaderboard : MonoBehaviour
    {
        private const string RestartButtonName = "B_Restart";
        private const float ButtonGap = 78f;
        private const float ButtonWidth = 210f;
        private const float ButtonFontSize = 16f;
        private const float RankFontSize = 26f;
        private const float RestartFontSize = 24f;
        private const float RankY = -152f;
        private static readonly Vector2 RankSize = new Vector2(540f, 60f);

        private GameObject _button;
        private TextMeshProUGUI _rank;

        public void Build(Button mainMenu, Action open)
        {
            Transform panel = mainMenu.transform.parent;
            TMP_Text menuLabel = mainMenu.GetComponentInChildren<TMP_Text>(true);

            Button button = Instantiate(mainMenu, panel);
            button.name = "B_Leaderboard";
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(() => open());
            _button = button.gameObject;

            RectTransform rect = (RectTransform)button.transform;
            rect.sizeDelta = new Vector2(ButtonWidth, rect.sizeDelta.y);
            rect.anchoredPosition += Vector2.down * ButtonGap;

            TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
            label.text = Loc.Caps("Leaderboard");
            Scene.PanelStyle.FitLine(label, ButtonFontSize);

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
            Scene.PanelStyle.FitLine(panel.Find(RestartButtonName).GetComponentInChildren<TMP_Text>(true), RestartFontSize);
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
            LeaderboardService.LoadPlayerRank(LeaderboardService.CurrentBoard, rank =>
            {
                if (_rank != null && rank.HasValue)
                    _rank.text = Loc.Upper(Loc.F("Your rank: #{0}", rank.Value));
            });
        }
    }
}
