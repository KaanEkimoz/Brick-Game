using UnityEngine;

namespace Scene
{
    /// <summary>
    /// Guarantees the StartCanvas (main menu) is active when the scene loads,
    /// no matter what state was saved in the .unity file. We've shipped a build
    /// once with StartCanvas accidentally left inactive — the player launched
    /// the game and saw a blank screen. This bootstrap pulls it back on every
    /// run so an editor-time toggle can never reach the store.
    ///
    /// Attached to a GameObject that itself starts active (e.g. the SaveManager
    /// or any always-on bootstrap object); the reference is set in the Inspector
    /// so we can also find StartCanvas while it's inactive (GameObject.Find
    /// can't see inactive objects).
    /// </summary>
    public class StartCanvasBootstrap : MonoBehaviour
    {
        [Tooltip("StartCanvas (main menu). Bound in the Inspector — must be set " +
                 "even when StartCanvas is inactive in the scene asset.")]
        [SerializeField] private GameObject _startCanvas;

        private void Awake()
        {
            if (_startCanvas == null)
            {
                // Fallback: walk every Canvas in the scene, including inactive ones.
                // Resources.FindObjectsOfTypeAll returns inactive objects too.
                foreach (var c in Resources.FindObjectsOfTypeAll<Canvas>())
                {
                    if (c == null || c.gameObject == null) continue;
                    if (c.gameObject.name != "StartCanvas") continue;
                    if (c.gameObject.scene != gameObject.scene) continue;
                    _startCanvas = c.gameObject;
                    break;
                }
            }

            if (_startCanvas != null && !_startCanvas.activeSelf)
                _startCanvas.SetActive(true);
        }
    }
}
