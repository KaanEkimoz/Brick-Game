using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Scene
{
    /// <summary>Rains confetti over the menu after Premium is bought. The purchase reloads the
    /// scene, so the request waits for the next scene load.</summary>
    public class PremiumCelebration : MonoBehaviour
    {
        private const int PieceCount = 70;
        private const float Seconds = 2.4f;
        private const float StartDelay = 0.3f;
        private const int SortingOrder = 500;
        private static readonly Vector2 ReferenceResolution = new Vector2(1080f, 1920f);
        private static readonly Vector2 PieceSize = new Vector2(16f, 26f);
        private static readonly Color[] Colors =
        {
            new Color(1f, 0.84f, 0.25f), new Color(0.35f, 0.85f, 1f), new Color(1f, 0.4f, 0.65f),
            new Color(0.5f, 1f, 0.55f), Color.white,
        };

        private static bool _pending;

        public static void Request() => _pending = true;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            SceneManager.sceneLoaded += (scene, mode) =>
            {
                if (!_pending) return;
                _pending = false;
                new GameObject(nameof(PremiumCelebration)).AddComponent<PremiumCelebration>();
            };
        }

        private IEnumerator Start()
        {
            for (float t = 0f; t < StartDelay; t += Time.unscaledDeltaTime) yield return null;

            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = SortingOrder;
            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.matchWidthOrHeight = 0.5f;
            yield return null; // the canvas takes its size on the next frame
            Vector2 size = ((RectTransform)transform).rect.size;

            RectTransform[] pieces = new RectTransform[PieceCount];
            Vector2[] velocity = new Vector2[PieceCount];
            float[] spin = new float[PieceCount];
            float[] sway = new float[PieceCount];
            for (int i = 0; i < PieceCount; i++)
            {
                GameObject go = new GameObject("Confetti", typeof(RectTransform), typeof(Image));
                go.transform.SetParent(transform, false);
                Image image = go.GetComponent<Image>();
                image.color = Colors[i % Colors.Length];
                image.raycastTarget = false;
                RectTransform rect = (RectTransform)go.transform;
                rect.sizeDelta = PieceSize * Random.Range(0.7f, 1.2f);
                rect.anchoredPosition = new Vector2(Random.Range(-0.5f, 0.5f) * size.x, size.y * 0.5f + Random.Range(20f, 400f));
                rect.localRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
                pieces[i] = rect;
                velocity[i] = new Vector2(Random.Range(-60f, 60f), -Random.Range(500f, 900f));
                spin[i] = Random.Range(-360f, 360f);
                sway[i] = Random.Range(0f, 6f);
            }

            for (float t = 0f; t < Seconds; t += Time.unscaledDeltaTime)
            {
                float dt = Time.unscaledDeltaTime;
                for (int i = 0; i < PieceCount; i++)
                {
                    RectTransform rect = pieces[i];
                    float drift = Mathf.Sin((t + sway[i]) * 4f) * 40f;
                    rect.anchoredPosition += new Vector2(velocity[i].x + drift, velocity[i].y) * dt;
                    rect.Rotate(0f, 0f, spin[i] * dt);
                }
                yield return null;
            }
            Destroy(gameObject);
        }
    }
}
