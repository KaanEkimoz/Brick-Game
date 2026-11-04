using System.Collections;
using Piece;
using Tiles;
using UnityEngine;

namespace Scene
{
    /// <summary>Squashes a piece's tiles for a moment when it settles, so a landing has weight.
    /// Tiles a line clear removes in the same frame simply drop out of the animation.</summary>
    public class PieceLandingSquash : MonoBehaviour
    {
        private const float Seconds = 0.16f;
        private static readonly Vector3 Squash = new Vector3(1.1f, 0.86f, 1f);

        private void OnEnable()
        {
            PieceMovement.OnPieceSettled += HandleSettled;
        }

        private void OnDisable()
        {
            PieceMovement.OnPieceSettled -= HandleSettled;
        }

        private void HandleSettled()
        {
            if (PieceController.Tiles == null) return;
            foreach (TileController tile in PieceController.Tiles)
                if (tile != null) StartCoroutine(Play(tile.transform));
        }

        private static IEnumerator Play(Transform tile)
        {
            Vector3 baseScale = tile.localScale;
            for (float t = 0f; t < Seconds; t += Time.deltaTime)
            {
                if (tile == null) yield break;
                float n = t / Seconds;
                // In on the first third, then spring back out.
                float amount = n < 0.33f ? n / 0.33f : 1f - UiEase.OutBack((n - 0.33f) / 0.67f);
                tile.localScale = Vector3.Scale(baseScale, Vector3.LerpUnclamped(Vector3.one, Squash, amount));
                yield return null;
            }
            if (tile != null) tile.localScale = baseScale;
        }
    }
}
