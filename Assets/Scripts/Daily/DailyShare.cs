using System.Text;
using Localization;
using UnityEngine;

namespace Daily
{
    /// <summary>
    /// Builds the spoiler-free result text (one colored square per clear) and hands it to the
    /// Android share sheet. Outside Android it goes to the clipboard.
    /// </summary>
    public static class DailyShare
    {
        private const int SquaresPerRow = 10;
        private const int MaxSquares = 40;
        private const string StoreUrl = "https://play.google.com/store/apps/details?id=";
        private const string NoClears = "⬛"; // black square

        // Single, double, triple, quad.
        private static readonly string[] ClearSquares =
        {
            "\U0001F7E6", // blue
            "\U0001F7E9", // green
            "\U0001F7E8", // yellow
            "\U0001F7EA", // purple
        };

        public static string BuildText(DailyResult result, int streak)
        {
            StringBuilder text = new StringBuilder();
            text.Append("Brick Game · ").Append(Loc.F("Daily #{0}", result.Day)).Append('\n');
            text.Append(Loc.F("Score: {0}", result.Score));
            if (streak > 1) text.Append("  \U0001F525 ").Append(streak);
            text.Append('\n');

            string clears = result.Clears ?? "";
            if (clears.Length == 0) text.Append(NoClears);
            int shown = Mathf.Min(clears.Length, MaxSquares);
            for (int i = 0; i < shown; i++)
            {
                if (i > 0 && i % SquaresPerRow == 0) text.Append('\n');
                int lines = Mathf.Clamp(clears[i] - '0', 1, ClearSquares.Length);
                text.Append(ClearSquares[lines - 1]);
            }
            if (clears.Length > MaxSquares) text.Append(" +").Append(clears.Length - MaxSquares);

            text.Append("\n\n").Append(StoreUrl).Append(Application.identifier);
            return text.ToString();
        }

        public static void Share(string text)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent"))
                using (AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent"))
                using (AndroidJavaClass player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (AndroidJavaObject activity = player.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    intent.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_SEND"));
                    intent.Call<AndroidJavaObject>("setType", "text/plain");
                    intent.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TEXT"), text);
                    using (AndroidJavaObject chooser = intentClass.CallStatic<AndroidJavaObject>("createChooser", intent, Loc.T("Share")))
                        activity.Call("startActivity", chooser);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[Daily] Share failed: " + e.Message);
                GUIUtility.systemCopyBuffer = text;
            }
#else
            GUIUtility.systemCopyBuffer = text;
            Debug.Log("[Daily] Share text copied:\n" + text);
#endif
        }
    }
}
