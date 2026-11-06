using InGame;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Scene
{
    /// <summary>Ignores every tap for a moment after the game ends, so a move made just as the
    /// board fills cannot land on the Game Over buttons and start a new run by accident.</summary>
    public class GameOverInputLock : MonoBehaviour
    {
        private const float LockSeconds = 1f;
        private const int SortingOrder = short.MaxValue; // above every scene canvas

        private GameObject _blocker;
        private float _unlockAt;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Create()
        {
            GameObject go = new GameObject(nameof(GameOverInputLock));
            DontDestroyOnLoad(go);
            go.AddComponent<GameOverInputLock>();
        }

        private void Awake()
        {
            _blocker = new GameObject("GameOverTapBlocker", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster));
            _blocker.transform.SetParent(transform, false);
            Canvas canvas = _blocker.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = SortingOrder;

            // An invisible full-screen graphic catches the taps; nothing is drawn.
            GameObject cover = new GameObject("Cover", typeof(RectTransform), typeof(Image));
            cover.transform.SetParent(_blocker.transform, false);
            RectTransform rect = (RectTransform)cover.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            Image image = cover.GetComponent<Image>();
            image.color = Color.clear;
            image.raycastTarget = true;

            _blocker.SetActive(false);
        }

        private void OnEnable()
        {
            PiecesController.OnGameOver += HandleGameOver;
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void OnDisable()
        {
            PiecesController.OnGameOver -= HandleGameOver;
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }

        private void HandleGameOver()
        {
            _unlockAt = Time.unscaledTime + LockSeconds;
            _blocker.SetActive(true);
        }

        private void HandleSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
        {
            _blocker.SetActive(false);
        }

        private void Update()
        {
            if (_blocker.activeSelf && Time.unscaledTime >= _unlockAt) _blocker.SetActive(false);
        }
    }
}
