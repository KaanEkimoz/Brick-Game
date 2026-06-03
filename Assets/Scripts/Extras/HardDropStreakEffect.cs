using System.Collections;
using Piece;
using Pooling;
using Tiles;
using UnityEngine;

namespace Extras
{
    /// <summary>
    /// Spawns a vertical white streak above each landed tile when the piece hard-drops, sized
    /// to how far the piece fell. The streak fades out over a short window. Stand-alone effect:
    /// no edits to existing prefabs, no particles required.
    /// </summary>
    public class HardDropStreakEffect : MonoBehaviour
    {
        [SerializeField] private float _streakWidth = 0.85f;
        [SerializeField] [Range(0.05f, 1f)] private float _lengthMultiplier = 0.55f;
        [SerializeField] private float _fadeDuration = 0.3f;
        [SerializeField] [Range(0f, 1f)] private float _startAlpha = 0.85f;
        [SerializeField] private Color _streakColor = Color.white;
        [SerializeField] private int _maxFallDistance = 25;
        [SerializeField] private string _sortingLayerName = "Default";
        [SerializeField] private int _sortingOrder = -1;
        [SerializeField] private Material _streakMaterial;

        private Material _runtimeMat;

        private void OnEnable()
        {
            PieceMovement.OnHardDrop += HandleHardDrop;
        }

        private void OnDisable()
        {
            PieceMovement.OnHardDrop -= HandleHardDrop;
        }

        private Material GetMaterial()
        {
            if (_streakMaterial != null) return _streakMaterial;
            if (_runtimeMat == null)
                _runtimeMat = new Material(Shader.Find("Sprites/Default"));
            return _runtimeMat;
        }

        private void HandleHardDrop(TileController[] tiles, int fallDistance)
        {
            if (tiles == null || fallDistance <= 0) return;
            int clamped = Mathf.Min(fallDistance, _maxFallDistance);
            float visualLength = clamped * _lengthMultiplier;
            if (visualLength <= 0.05f) return;
            // Tint the streak with the dropped piece's dominant colour (cyan I, red Z, ...).
            Color tint = _streakColor;
            foreach (var t in tiles)
            {
                if (t == null) continue;
                var sr = t.GetComponentInChildren<SpriteRenderer>();
                if (sr != null && sr.sprite != null) { tint = PieceColorUtil.DominantColor(sr.sprite, _streakColor); break; }
            }
            foreach (var tile in tiles)
            {
                if (tile == null) continue;
                StartCoroutine(SpawnStreak(tile.transform.position, visualLength, tint));
            }
        }

        private IEnumerator SpawnStreak(Vector3 tilePos, float length, Color streakColor)
        {
            // Borrow a streak from the pool; the same LineRenderer GameObject is reused
            // across many hard-drops instead of allocating + destroying every event.
            LineRenderer lr = StreakPool.Acquire();
            lr.transform.position = tilePos;
            lr.material = GetMaterial();
            lr.useWorldSpace = true;
            lr.positionCount = 2;
            lr.SetPosition(0, tilePos);
            lr.SetPosition(1, tilePos + Vector3.up * length);
            lr.startWidth = _streakWidth;
            lr.endWidth = _streakWidth;
            lr.numCapVertices = 2;
            lr.sortingLayerName = _sortingLayerName;
            lr.sortingOrder = _sortingOrder;

            Color startCol = streakColor; startCol.a = _startAlpha;
            Color endCol = streakColor; endCol.a = 0f;
            lr.startColor = startCol;
            lr.endColor = endCol;

            float t = 0f;
            while (t < _fadeDuration)
            {
                t += Time.unscaledDeltaTime; // fade even when paused (timeScale=0)
                float n = Mathf.Clamp01(t / _fadeDuration);
                Color s = streakColor; s.a = _startAlpha * (1f - n);
                Color e = streakColor; e.a = 0f;
                lr.startColor = s;
                lr.endColor = e;
                yield return null;
            }

            StreakPool.Release(lr);
        }
    }
}
