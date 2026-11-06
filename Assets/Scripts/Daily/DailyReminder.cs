using System;
using Localization;
using UnityEngine;
#if UNITY_ANDROID && !UNITY_EDITOR
using Unity.Notifications.Android;
#endif

namespace Daily
{
    /// <summary>
    /// One local notification at 20:00 that reminds a player with a streak to finish the day's
    /// challenge. It is only sent when the player allowed notifications, and it is planned again
    /// whenever the app goes to the background or a day is won.
    /// </summary>
    public static class DailyReminder
    {
        private const int ReminderHour = 20;
        private const int NotificationId = 2001;
        private const string ChannelId = "daily_reminder";
        private const string AskedKey = "daily_reminder_asked";

        /// <summary>Asks for the notification permission once, after the first day won.</summary>
        public static void AskOnce()
        {
            if (PlayerPrefs.GetInt(AskedKey, 0) == 1) return;
            PlayerPrefs.SetInt(AskedKey, 1);
            PlayerPrefs.Save();
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                if (AndroidNotificationCenter.UserPermissionToPost == PermissionStatus.NotRequested)
                    new PermissionRequest();
            }
            catch (Exception e)
            {
                Debug.LogWarning("[Daily] Notification permission request failed: " + e.Message);
            }
#endif
        }

        /// <summary>Cancels the planned reminder and plans the next one, if there should be one.</summary>
        public static void Refresh()
        {
            DateTime? fireTime = NextFireTime(DateTime.Now);
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                AndroidNotificationCenter.CancelNotification(NotificationId);
                if (fireTime == null || AndroidNotificationCenter.UserPermissionToPost != PermissionStatus.Allowed) return;

                AndroidNotificationCenter.RegisterNotificationChannel(new AndroidNotificationChannel(
                    ChannelId, Loc.T("Daily reminder"), Loc.T("A reminder to keep your daily streak"), Importance.Default));
                AndroidNotification notification = new AndroidNotification
                {
                    Title = Loc.T("Keep your streak going"),
                    Text = Loc.F("Today's challenge is waiting. Don't lose your {0}-day streak!", DailyStreak.CurrentStreak),
                    FireTime = fireTime.Value,
                };
                AndroidNotificationCenter.SendNotificationWithExplicitID(notification, ChannelId, NotificationId);
            }
            catch (Exception e)
            {
                Debug.LogWarning("[Daily] Reminder scheduling failed: " + e.Message);
            }
#else
            if (fireTime != null) Debug.Log("[Daily] A reminder would fire at " + fireTime.Value);
#endif
        }

        /// <summary>The next 20:00 on a day that is still open and whose streak can still be
        /// kept; null when the player has no streak to lose.</summary>
        public static DateTime? NextFireTime(DateTime now)
        {
            if (DailyStreak.CurrentStreak <= 0) return null;

            DateTime fire = now.Date.AddHours(ReminderHour);
            if (!DailySession.TodayIsOpen || now >= fire) fire = fire.AddDays(1);

            // A reminder for a day the streak can no longer reach would only annoy.
            return DailyStreak.CanStillKeep(DailyChallenge.DayIndex(fire)) ? fire : (DateTime?)null;
        }
    }
}
