using UnityEngine;

namespace Scene
{
    /// <summary>Easing curves shared by the menu motion.</summary>
    public static class UiEase
    {
        private const float Overshoot = 1.2f;

        public static float OutCubic(float t)
        {
            t = 1f - Mathf.Clamp01(t);
            return 1f - t * t * t;
        }

        /// <summary>Passes the end a little and settles back, for things that pop in.</summary>
        public static float OutBack(float t)
        {
            t = Mathf.Clamp01(t) - 1f;
            return 1f + (Overshoot + 1f) * t * t * t + Overshoot * t * t;
        }
    }
}
