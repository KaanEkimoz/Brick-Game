using System;
using System.Collections.Generic;
using Leaderboard;
using Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Levels
{
    public partial class LevelUi
    {
        private const string LeaderboardIconPath = "Icons/icon_leaderboard_neon";
        private const string GameOverPanelName = "GOM_Panel";
        private const string MainMenuButtonName = "B_MainMenu";
        private const string AchievementsIconPath = "Icons/icon_achievements_neon";
        private const int LeaderboardRowCount = 10;
        private const float LeaderboardTop = 610f;
        private const float LeaderboardToggleY = 480f;
        private const float LeaderboardPeriodY = 365f;
        private const float LeaderboardResetY = 290f;
        private const float LeaderboardRowsY = 225f;
        private const float LeaderboardRowHeight = 66f;
        private const float LeaderboardBackY = -560f;
        private const float ToggleOptionOffset = 148f; // half of the visible track, inside its glow
        private const float PeriodFontSize = 25f;
        private const float PeriodLineHeight = 5f;
        private const float PeriodLineOffset = 36f;
        private const float PeriodSpacing = 245f;

        private static readonly Vector2 LeaderboardPanelSize = new Vector2(880f, 1480f);
        private static readonly Vector2 ToggleSize = new Vector2(740f, 130f);
        private static readonly Vector2 ToggleOptionSize = new Vector2(370f, 130f);
        private static readonly Vector2 PeriodTabSize = new Vector2(245f, 70f);
        private static readonly Vector2 PeriodLineSize = new Vector2(190f, PeriodLineHeight);
        private static readonly Vector2 LeaderboardRowSize = new Vector2(720f, 62f);
        private static readonly Color ToggleTrackColor = new Color(0.02f, 0.08f, 0.32f, 0.9f);
        private static readonly Color IdleTabColor = new Color(1f, 1f, 1f, 0.65f);

        private int _leaderboardRequest;

        private void BuildLeaderboardButton()
        {
            CreateMenuIcon("B_LeaderboardIcon", LeaderboardIconPath, 2, () => OpenLeaderboard(LeaderboardBoard.Classic));
            CreateMenuIcon("B_AchievementsIcon", AchievementsIconPath, 3, Achievements.AchievementService.Show);
            CreateMenuIcon("B_PremiumIcon", PremiumIconPath, 4, ShowPremium);
        }

        private void BuildGameOverLeaderboard()
        {
            foreach (Button button in Resources.FindObjectsOfTypeAll<Button>())
            {
                if (button.name != MainMenuButtonName || !button.gameObject.scene.IsValid()) continue;
                if (button.transform.parent.name != GameOverPanelName) continue;

                gameObject.AddComponent<GameOverLeaderboard>()
                    .Build(button, () => OpenLeaderboard(LeaderboardService.CurrentBoard));
                return;
            }
        }

        private void OpenLeaderboard(LeaderboardBoard board)
        {
            LeaderboardService.AllowSignInPrompt();
            ShowLeaderboard(board);
        }

        private void ShowLeaderboard(LeaderboardBoard board, LeaderboardPeriod period = LeaderboardPeriod.AllTime)
        {
            Transform screen = OpenScreen();
            screen.GetComponent<Image>().color = IntroDim;

            RectTransform panel = CreatePanel(screen, LeaderboardPanelSize);
            TextMeshProUGUI heading = CreateHeading(panel, Loc.Caps("Leaderboard"), 66f);
            Place(heading.rectTransform, new Vector2(0f, LeaderboardTop), new Vector2(720f, 90f));

            CreateBoardToggle(panel, board, period);
            CreatePeriodTab(panel, board, LeaderboardPeriod.AllTime, "All Time", -PeriodSpacing, period);
            CreatePeriodTab(panel, board, LeaderboardPeriod.Monthly, "Monthly", 0f, period);
            CreatePeriodTab(panel, board, LeaderboardPeriod.Weekly, "Weekly", PeriodSpacing, period);
            CreateResetLabel(panel, period);

            Button back = CreateMenuStyleButton(panel, "Back", CloseScreen);
            Place((RectTransform)back.transform, new Vector2(0f, LeaderboardBackY), WinButtonSize);

            TextMeshProUGUI status = CreatePlainText(panel, Loc.T("Loading..."), 40f, Color.white);
            Place(status.rectTransform, Vector2.zero, new Vector2(720f, 200f));

            int request = ++_leaderboardRequest;
            LeaderboardService.LoadTop(board, period, LeaderboardRowCount, rows =>
            {
                if (request != _leaderboardRequest || panel == null) return;
                ShowLeaderboardRows(panel, status, rows);
            });
        }

        // A single track with one bright option sliding between the modes.
        private void CreateBoardToggle(Transform panel, LeaderboardBoard board, LeaderboardPeriod period)
        {
            GameObject track = new GameObject("Toggle", typeof(RectTransform), typeof(Image));
            track.transform.SetParent(panel, false);
            Image image = track.GetComponent<Image>();
            if (_classicButton != null)
            {
                Image pill = _classicButton.GetComponent<Image>();
                image.sprite = pill.sprite;
                image.type = pill.type;
            }
            image.color = ToggleTrackColor;
            Place((RectTransform)track.transform, new Vector2(0f, LeaderboardToggleY), ToggleSize);

            CreateToggleOption(track.transform, LeaderboardBoard.Classic, "Classic", -ToggleOptionOffset, board, period);
            CreateToggleOption(track.transform, LeaderboardBoard.Extended, "Extended", ToggleOptionOffset, board, period);
        }

        private void CreateToggleOption(Transform track, LeaderboardBoard option, string label, float x, LeaderboardBoard selected, LeaderboardPeriod period)
        {
            Button button = CreateMenuStyleButton(track, label, () => ShowLeaderboard(option, period));
            Place((RectTransform)button.transform, new Vector2(x, 0f), ToggleOptionSize);

            TMP_Text text = button.GetComponentInChildren<TMP_Text>(true);
            Scene.PanelStyle.FitButtonLabel(text, 38f, ToggleOptionSize.x);
            if (option == selected) return;

            text.color = Accent;
            button.GetComponent<Image>().color = Color.clear;
        }

        private void CreatePeriodTab(Transform panel, LeaderboardBoard board, LeaderboardPeriod period, string label, float x, LeaderboardPeriod selected)
        {
            Button tab = CreateMenuStyleButton(panel, label, () => ShowLeaderboard(board, period));
            tab.GetComponent<Image>().color = Color.clear;
            Place((RectTransform)tab.transform, new Vector2(x, LeaderboardPeriodY), PeriodTabSize);

            TMP_Text text = tab.GetComponentInChildren<TMP_Text>(true);
            Scene.PanelStyle.FitLine(text, PeriodFontSize);
            text.margin = Vector4.zero;
            text.color = period == selected ? Accent : IdleTabColor;
            if (period != selected) return;

            GameObject line = new GameObject("Underline", typeof(RectTransform), typeof(Image));
            line.transform.SetParent(panel, false);
            line.GetComponent<Image>().color = Accent;
            Place((RectTransform)line.transform, new Vector2(x, LeaderboardPeriodY - PeriodLineOffset), PeriodLineSize);
        }

        private void CreateResetLabel(Transform panel, LeaderboardPeriod period)
        {
            string resetTime = LeaderboardSchedule.NextResetText(period, DateTime.UtcNow);
            if (resetTime == null) return;

            TextMeshProUGUI label = CreatePlainText(panel, Loc.F("Resets {0}", resetTime), 30f, Accent);
            label.alignment = TextAlignmentOptions.Right;
            Place(label.rectTransform, new Vector2(0f, LeaderboardResetY), new Vector2(LeaderboardRowSize.x, 50f));
        }

        private void ShowLeaderboardRows(Transform panel, TextMeshProUGUI status, List<LeaderboardRow> rows)
        {
            if (rows == null)
            {
                status.text = Loc.T(LeaderboardService.IsSignedIn ? "Could not load the leaderboard" : "Sign in to Play Games to see the leaderboard");
                return;
            }
            if (rows.Count == 0)
            {
                status.text = Loc.T("No scores yet");
                return;
            }

            status.gameObject.SetActive(false);
            for (int i = 0; i < rows.Count; i++)
                CreateLeaderboardRow(panel, rows[i], LeaderboardRowsY - i * LeaderboardRowHeight);
        }

        private void CreateLeaderboardRow(Transform panel, LeaderboardRow row, float y)
        {
            Color color = row.IsPlayer ? Accent : Color.white;

            TextMeshProUGUI rank = CreatePlainText(panel, row.Rank.ToString(), 36f, color);
            rank.alignment = TextAlignmentOptions.Left;
            Place(rank.rectTransform, new Vector2(-300f, y), new Vector2(120f, LeaderboardRowSize.y));

            TextMeshProUGUI name = CreatePlainText(panel, row.Name, 36f, color);
            name.alignment = TextAlignmentOptions.Left;
            name.textWrappingMode = TextWrappingModes.NoWrap;
            name.overflowMode = TextOverflowModes.Ellipsis;
            Place(name.rectTransform, new Vector2(-40f, y), new Vector2(390f, LeaderboardRowSize.y));

            TextMeshProUGUI score = CreatePlainText(panel, row.Score.ToString("N0", Loc.Culture), 36f, color);
            score.alignment = TextAlignmentOptions.Right;
            Place(score.rectTransform, new Vector2(270f, y), new Vector2(180f, LeaderboardRowSize.y));
        }
    }
}
