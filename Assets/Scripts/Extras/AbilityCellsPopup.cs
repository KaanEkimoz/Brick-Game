using System.Collections;
using Board;
using Gameplay;
using TMPro;
using UnityEngine;

namespace Extras
{
    /// <summary>
    /// World-space "+N CELLS!" popup fired the instant a Bomb / Laser ability lands
    /// and destroys at least one tile. Sits over the explosion site (the ability
    /// piece's anchor cell, converted via BoardController.CellToWorld so it tracks
    /// the playfield's actual world origin — not (0,0) — and stays glued to the
    /// visual on phones where the board is moved as a unit).
    ///
    /// Listens to AbilityExecutor.OnAbilityCellsCleared. Spawns a runtime-created
    /// TextMeshPro (3D, NOT UGUI) so it lives in the camera frustum next to the
    /// destroyed tiles, then rises + scales + fades and self-destroys. Decoupled
    /// from the existing ScorePopupEffect — that one shows the bonus POINTS over
    /// the HUD score; this one shows the CELL COUNT over the board, so the player
    /// reads two complementary numbers: "+45" (points earned) and "9 cells!"
    /// (board impact).
    ///
    /// Not pooled — abilities fire at most ~once per several seconds, instances
    /// die in under 1s, GC pressure is negligible.
    /// </summary>
    public class AbilityCellsPopup : MonoBehaviour
    {
        [Tooltip("Font asset for the runtime-created popup. Use the same SDF font as the rest of the HUD.")]
        [SerializeField] private TMP_FontAsset _font;

        [Tooltip("Tint applied to the popup text. Defaults to a warm yellow for contrast against the board.")]
        [SerializeField] private Color _tint = new Color(1f, 0.92f, 0.30f, 1f);

        [SerializeField] private Color _outlineColor = Color.black;
        [SerializeField] [Range(0f, 1f)] private float _outlineWidth = 0.25f;

        [Tooltip("Total lifetime including the fade-out tail.")]
        [SerializeField] private float _duration = 0.95f;

        [Tooltip("World units the popup rises over its lifetime.")]
        [SerializeField] private float _riseDistance = 2.5f;

        [Tooltip("Multiplier the text reaches at the pop-in peak (~25% of duration).")]
        [SerializeField] private float _peakScale = 1.35f;

        [Tooltip("Baseline character size — board cells are 1 unit wide, so values around 4-6 read well.")]
        [SerializeField] private float _fontSize = 5f;

        [Tooltip("Z offset from the board so the popup renders in front of tiles.")]
        [SerializeField] private float _zOffset = -1f;

        [Tooltip("Copy template. {n} is replaced with the cleared cell count.")]
        [SerializeField] private string _template = "+{n} CELLS!";

        private void OnEnable()
        {
            AbilityExecutor.OnAbilityCellsCleared += Handle;
        }

        private void OnDisable()
        {
            AbilityExecutor.OnAbilityCellsCleared -= Handle;
        }

        private void Handle(Vector2Int anchor, int cleared)
        {
            if (cleared <= 0) return;
            StartCoroutine(Spawn(anchor, cleared));
        }

        private IEnumerator Spawn(Vector2Int anchor, int cleared)
        {
            GameObject go = new GameObject("AbilityCellsPopup");
            // Parent to the board so the popup follows any future board reposition
            // (the mobile redesign already moves BoardController root in the scene).
            if (BoardController.Instance != null)
                go.transform.SetParent(BoardController.Instance.transform, false);

            Vector3 worldStart = BoardController.CellToWorld(anchor) + new Vector3(0f, 0f, _zOffset);
            // Re-anchor to world so the popup is independent of the parent's local scale.
            go.transform.position = worldStart;

            TextMeshPro tmp = go.AddComponent<TextMeshPro>();
            if (_font != null) tmp.font = _font;
            tmp.text = _template.Replace("{n}", cleared.ToString());
            tmp.fontSize = _fontSize;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = _tint;
            tmp.outlineColor = _outlineColor;
            tmp.outlineWidth = _outlineWidth;
            tmp.raycastTarget = false;
            tmp.sortingOrder = 100;

            float t = 0f;
            while (t < _duration)
            {
                t += Time.unscaledDeltaTime;
                float n = Mathf.Clamp01(t / _duration);

                // Scale: 0.4 → peak in the first 25%, peak → 1 over the remaining 75%.
                float scaleFactor;
                if (n < 0.25f)
                {
                    float k = n / 0.25f;
                    scaleFactor = Mathf.Lerp(0.4f, _peakScale, 1f - Mathf.Pow(1f - k, 2f));
                }
                else
                {
                    float k = (n - 0.25f) / 0.75f;
                    scaleFactor = Mathf.Lerp(_peakScale, 1f, k);
                }
                go.transform.localScale = Vector3.one * scaleFactor;

                // Rise
                float riseEase = 1f - Mathf.Pow(1f - n, 2f);
                go.transform.position = worldStart + Vector3.up * (_riseDistance * riseEase);

                // Hold alpha until 65%, then fade to 0.
                float alphaMul = n < 0.65f ? 1f : Mathf.Lerp(1f, 0f, (n - 0.65f) / 0.35f);
                Color c = _tint; c.a = alphaMul;
                tmp.color = c;

                yield return null;
            }

            Destroy(go);
        }

        [ContextMenu("Test Popup at (4,8)")]
        private void TestPopup() => Handle(new Vector2Int(4, 8), 9);
    }
}
