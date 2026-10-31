using UnityEngine;

namespace Extras
{
    /// <summary>Mobile builds default to 30 FPS, which delays touch response.</summary>
    public static class FrameRateBootstrap
    {
        private const int TargetFrameRate = 60;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Apply()
        {
            Application.targetFrameRate = TargetFrameRate;
        }
    }
}
