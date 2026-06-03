using System.Collections;
using InGame;
using Piece;
using Pooling;
using Tiles;
using UnityEngine;

namespace Extras
{
    /// <summary>
    /// Each time the player drives the piece down (keyboard MoveDown or held soft-drop), spawn
    /// a short vertical streak above each active tile — same visual language as
    /// HardDropStreakEffect, just a single tile tall. The automatic gravity tick does NOT
    /// fire OnSoftDrop, so passive falling leaves no trail.
    /// </summary>
    public class SoftDropTrailEffect : MonoBehaviour
    {
        [SerializeField] private float _streakWidth = 1f;
        [SerializeField] private float _streakLength = 1f;
        [SerializeField] private float _fadeDuration = 0.18f;
        [SerializeField] [Range(0f, 1f)] private float _startAlpha = 0.6f;
        [SerializeField] private bool _useTileColor = true;
        [SerializeField] private Color _fallbackColor = Color.white;
        [SerializeField] private string _sortingLayerName = "Default";
        [SerializeField] private int _sortingOrder = -1;
        [SerializeField] private Material _streakMaterial;

        private Material _runtimeMat;

        private void OnEnable()
        {
            PiecesController.OnSoftDrop += HandleSoftDrop;
        }

        private void OnDisable()
        {
            PiecesController.OnSoftDrop -= HandleSoftDrop;
        }

        private Material GetMaterial()
        {
            if (_streakMaterial != null) return _streakMaterial;
            if (_runtimeMat == null)
                _runtimeMat = new Material(Shader.Find("Sprites/Default"));
            return _runtimeMat;
        }

        private void HandleSoftDrop()
        {
            TileController[] tiles = PieceController.Tiles;
            if (tiles == null) return;
            foreach (var tile in tiles)
            {
                if (tile == null) continue;
                Color streakColor = _fallbackColor;
                if (_useTileColor)
                {
                    SpriteRenderer sr = tile.GetComponentInChildren<SpriteRenderer>();
                    // sr.color is just a white tint — the actual hue lives in the sprite texture,
                    // so pull the sprite's dominant colour (cyan I, red Z, ...).
                    if (sr != null) streakColor = PieceColorUtil.DominantColor(sr.sprite, _fallbackColor);
                }
                StartCoroutine(SpawnStreak(tile.transform.position, streakColor));
            }
        }

        private IEnumerator SpawnStreak(Vector3 tilePos, Color tint)
        {
            // Soft-drop fires many times per second when the player holds the gesture; pooling
            // the LineRenderer GameObjects eliminates the per-tick alloc + GC sawtooth.
            LineRenderer lr = StreakPool.Acquire();
            lr.transform.position = tilePos;
            lr.material = GetMaterial();
            lr.useWorldSpace = true;
            lr.positionCount = 2;
            lr.SetPosition(0, tilePos);
            lr.SetPosition(1, tilePos + Vector3.up * _streakLength);
            lr.startWidth = _streakWidth;
            lr.endWidth = _streakWidth;
            lr.numCapVertices = 2;
            lr.sortingLayerName = _sortingLayerName;
            lr.sortingOrder = _sortingOrder;

            Color startCol = tint; startCol.a = _startAlpha;
            Color endCol = tint; endCol.a = 0f;
            lr.startColor = startCol;
            lr.endColor = endCol;

            float t = 0f;
            while (t < _fadeDuration)
            {
                // Unscaled so the streak still fades out (and self-destructs) when the game is
                // paused with Time.timeScale = 0 — e.g. while the tutorial overlay is up.
                // Otherwise deltaTime is 0 and the trail hangs in mid-air forever.
                t += Time.unscaledDeltaTime;
                float n = Mathf.Clamp01(t / _fadeDuration);
                Color s = tint; s.a = _startAlpha * (1f - n);
                Color e = tint; e.a = 0f;
                lr.startColor = s;
                lr.endColor = e;
                yield return null;
            }

            StreakPool.Release(lr);
        }
    }
}
