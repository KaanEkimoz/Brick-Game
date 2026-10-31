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
        private const int LeaderboardRowCount = 10;
        private const float LeaderboardTop = 610f;
        private const float LeaderboardTabsY = 480f;
        private const float LeaderboardRowsY = 370f;
        private const float LeaderboardRowHeight = 78f;
        private const float LeaderboardBackY = -560f;

        private static readonly Vector2 LeaderboardPanelSize = new Vector2(880f, 1480f);
        private static readonly Vector2 LeaderboardTabSize = new Vector2(390f, 120f);
        private static readonly Vector2 LeaderboardRowSize = new Vector2(720f, 70f);

        private int _leaderboardRequest;

        private void BuildLeaderboardButton()
        {
            CreateMenuIcon("B_LeaderboardIcon", LeaderboardIconPath, 2, () => ShowLeaderboard(LeaderboardBoard.Classic));
        }

        private void ShowLeaderboard(LeaderboardBoard board)
        {
            Transform screen = OpenScreen();
            screen.GetComponent<Image>().color = IntroDim;

            RectTransform panel = CreatePanel(screen, LeaderboardPanelSize);
            TextMeshProUGUI heading = CreateHeading(panel, Loc.Caps("Leaderboard"), 66f);
            Place(heading.rectTransform, new Vector2(0f, LeaderboardTop), new Vector2(720f, 90f));

            CreateLeaderboardTab(panel, LeaderboardBoard.Classic, "Classic", -200f, board);
            CreateLeaderboardTab(panel, LeaderboardBoard.Extended, "Extended", 200f, board);

            Button back = CreateMenuStyleButton(panel, "Back", CloseScreen);
            Place((RectTransform)back.transform, new Vector2(0f, LeaderboardBackY), WinButtonSize);

            TextMeshProUGUI status = CreatePlainText(panel, Loc.T("Loading..."), 40f, Color.white);
            Place(status.rectTransform, new Vector2(0f, 0f), new Vector2(720f, 200f));

            int request = ++_leaderboardRequest;
            LeaderboardService.LoadTop(board, LeaderboardRowCount, rows =>
            {
                if (request != _leaderboardRequest || panel == null) return;
                ShowLeaderboardRows(panel, status, rows);
            });
        }

        private void CreateLeaderboardTab(Transform panel, LeaderboardBoard board, string label, float x, LeaderboardBoard selected)
        {
            Button tab = CreateMenuStyleButton(panel, label, () => ShowLeaderboard(board));
            Place((RectTransform)tab.transform, new Vector2(x, LeaderboardTabsY), LeaderboardTabSize);
            TMP_Text text = tab.GetComponentInChildren<TMP_Text>(true);
            Scene.PanelStyle.FitButtonLabel(text, 40f, LeaderboardTabSize.x);
            if (board == selected) text.color = SelectedLanguageTextColor;
        }

        private void ShowLeaderboardRows(Transform panel, TextMeshProUGUI status, List<LeaderboardRow> rows)
        {
            if (rows == null)
            {
                status.text = Loc.T("Sign in to Play Games to see the leaderboard");
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

            TextMeshProUGUI rank = CreatePlainText(panel, row.Rank.ToString(), 38f, color);
            rank.alignment = TextAlignmentOptions.Left;
            Place(rank.rectTransform, new Vector2(-300f, y), new Vector2(120f, LeaderboardRowSize.y));

            TextMeshProUGUI name = CreatePlainText(panel, row.Name, 38f, color);
            name.alignment = TextAlignmentOptions.Left;
            name.textWrappingMode = TextWrappingModes.NoWrap;
            name.overflowMode = TextOverflowModes.Ellipsis;
            Place(name.rectTransform, new Vector2(-40f, y), new Vector2(390f, LeaderboardRowSize.y));

            TextMeshProUGUI score = CreatePlainText(panel, row.Score.ToString("N0"), 38f, color);
            score.alignment = TextAlignmentOptions.Right;
            Place(score.rectTransform, new Vector2(270f, y), new Vector2(180f, LeaderboardRowSize.y));
        }
    }
}
