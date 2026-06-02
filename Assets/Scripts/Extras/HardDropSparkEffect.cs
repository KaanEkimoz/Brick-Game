using System.Collections;
using Piece;
using Tiles;
using UnityEngine;

namespace Extras
{
    /// <summary>
    /// Sprinkles short white "+"-shaped sparks along each hard-drop streak, layered on top of
    /// HardDropStreakEffect. Each spark is two thin LineRenderers with a point-thick-point
    /// width curve, randomly rotated and timed so the burst reads as twinkles.
    /// </summary>
    public class HardDropSparkEffect : MonoBehaviour
    {
        [SerializeField] private int _sparksPerTile = 3;
        [SerializeField] [Range(0.05f, 1f)] private float _lengthMultiplier = 0.55f;
        [SerializeField] private float _sparkSize = 0.28f;
        [SerializeField] private float _sparkWidth = 0.18f;
        [SerializeField] private float _fadeDuration = 0.35f;
        [SerializeField] private float _spawnSpread = 0.22f;
        [SerializeField] private float _horizontalJitter = 0.2f;
        [SerializeField] [Range(0f, 1f)] private float _startAlpha = 0.95f;
        [SerializeField] private Color _sparkColor = Color.white;
        [SerializeField] private string _sortingLayerName = "Default";
        [SerializeField] private int _sortingOrder = 11;
        [SerializeField] private Material _sparkMaterial;

        private Material _runtimeMat;
        private AnimationCurve _widthCurve;

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
            if (_sparkMaterial != null) return _sparkMaterial;
            if (_runtimeMat == null)
                _runtimeMat = new Material(Shader.Find("Sprites/Default"));
            return _runtimeMat;
        }

        private AnimationCurve GetWidthCurve()
        {
            if (_widthCurve == null)
            {
                _widthCurve = new AnimationCurve();
                _widthCurve.AddKey(0f, 0f);
                _widthCurve.AddKey(0.5f, 1f);
                _widthCurve.AddKey(1f, 0f);
            }
            return _widthCurve;
        }

        private void HandleHardDrop(TileController[] tiles, int fallDistance)
        {
            if (tiles == null || fallDistance <= 0) return;
            float length = fallDistance * _lengthMultiplier;
            if (length <= 0.05f) return;

            // Tint sparks with the dropped piece's dominant colour (cyan I, red Z, ...).
            Color tint = _sparkColor;
            foreach (var t in tiles)
            {
                if (t == null) continue;
                var sr = t.GetComponentInChildren<SpriteRenderer>();
                if (sr != null && sr.sprite != null) { tint = PieceColorUtil.DominantColor(sr.sprite, _sparkColor); break; }
            }

            foreach (var tile in tiles)
            {
                if (tile == null) continue;
                Vector3 basePos = tile.transform.position;
                for (int i = 0; i < _sparksPerTile; i++)
                {
                    float yOffset = Random.Range(0.1f, length);
                    float xOffset = Random.Range(-_horizontalJitter, _horizontalJitter);
                    Vector3 pos = basePos + new Vector3(xOffset, yOffset, 0f);
                    float delay = Random.Range(0f, _spawnSpread);
                    StartCoroutine(DelayedSpawn(pos, delay, tint));
                }
            }
        }

        private IEnumerator DelayedSpawn(Vector3 pos, float delay, Color tint)
        {
            if (delay > 0f) yield return new WaitForSecondsRealtime(delay);
            yield return SpawnSpark(pos, tint);
        }

        private IEnumerator SpawnSpark(Vector3 origin, Color tint)
        {
            GameObject spark = new GameObject("HardDropSpark");
            spark.transform.position = origin;
            spark.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));

            LineRenderer h = BuildArm(spark.transform, Vector3.right, tint);
            LineRenderer v = BuildArm(spark.transform, Vector3.up, tint);

            float t = 0f;
            while (t < _fadeDuration)
            {
                t += Time.unscaledDeltaTime; // fade even when paused (timeScale=0)
                float n = Mathf.Clamp01(t / _fadeDuration);
                // Quick in, slow out — sparkle reads brighter at the start
                float alpha = _startAlpha * Mathf.Pow(1f - n, 2f);
                Color c = tint; c.a = alpha;
                if (h != null) { h.startColor = c; h.endColor = c; }
                if (v != null) { v.startColor = c; v.endColor = c; }
                yield return null;
            }

            if (spark != null) Destroy(spark);
        }

        private LineRenderer BuildArm(Transform parent, Vector3 direction, Color tint)
        {
            GameObject arm = new GameObject("Arm");
            arm.transform.SetParent(parent, false);

            LineRenderer lr = arm.AddComponent<LineRenderer>();
            lr.material = GetMaterial();
            lr.useWorldSpace = false;
            lr.positionCount = 3;
            lr.SetPosition(0, -direction * _sparkSize);
            lr.SetPosition(1, Vector3.zero);
            lr.SetPosition(2, direction * _sparkSize);
            lr.widthCurve = GetWidthCurve();
            lr.widthMultiplier = _sparkWidth;
            lr.numCapVertices = 2;
            lr.sortingLayerName = _sortingLayerName;
            lr.sortingOrder = _sortingOrder;

            Color c = tint; c.a = _startAlpha;
            lr.startColor = c;
            lr.endColor = c;
            return lr;
        }
    }
}
