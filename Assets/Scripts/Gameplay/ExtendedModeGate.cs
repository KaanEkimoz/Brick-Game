namespace Gameplay
{
    /// <summary>
    /// Tiny helper used by the main-menu Play buttons to set GameMode.IsExtended
    /// without resorting to inline script in Unity Events.
    /// </summary>
    public class ExtendedModeGate : UnityEngine.MonoBehaviour
    {
        public void EnterExtendedMode() => GameMode.IsExtended = true;
        public void EnterClassicMode() => GameMode.IsExtended = false;
    }
}
