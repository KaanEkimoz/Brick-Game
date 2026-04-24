using System.Collections;
using Piece;
using Tiles;
using UnityEngine;

namespace Extras
{
    /// <summary>
    /// Subscribes to PieceMovement.OnHardDrop and makes each landed tile blink white twice by
    /// swapping its SpriteRenderer.sprite with a generated fully-white sprite, then restoring
    /// the original. No material / shader edits, no particles.
    /// </summary>
    public class HardDropTileFlash : MonoBehaviour
    {
        [SerializeField] private int _blinkCount = 2;
        [SerializeField] private float _flashDuration = 0.06f;
        [SerializeField] private float _gapDuration = 0.05f;

        private static Sprite _whiteSprite;

        private static Sprite GetWhiteSprite()
        {
            if (_whiteSprite != null) return _whiteSprite;
            // 4x4 white texture backed sprite; size doesn't matter because SpriteRenderer scales it.
            _whiteSprite = Sprite.Create(
                Texture2D.whiteTexture,
                new Rect(0f, 0f, Texture2D.whiteTexture.width, Texture2D.whiteTexture.height),
                new Vector2(0.5f, 0.5f),
                Texture2D.whiteTexture.width // ppu so sprite is 1 unit square
            );
            _whiteSprite.name = "HardDropFlashWhite";
            return _whiteSprite;
        }

        private void OnEnable()
        {
            PieceMovement.OnHardDrop += HandleHardDrop;
        }

        private void OnDisable()
        {
            PieceMovement.OnHardDrop -= HandleHardDrop;
        }

        private void HandleHardDrop(TileController[] tiles, int fallDistance)
        {
            if (tiles == null) return;
            foreach (var t in tiles)
            {
                if (t == null) continue;
                var sr = t.GetComponent<SpriteRenderer>();
                if (sr == null) continue;
                StartCoroutine(Flicker(sr));
            }
        }

        private IEnumerator Flicker(SpriteRenderer sr)
        {
            Sprite original = sr.sprite;
            Sprite white = GetWhiteSprite();
            for (int i = 0; i < _blinkCount; i++)
            {
                if (sr == null) yield break;
                sr.sprite = white;
                yield return new WaitForSeconds(_flashDuration);
                if (sr == null) yield break;
                sr.sprite = original;
                if (i < _blinkCount - 1)
                    yield return new WaitForSeconds(_gapDuration);
            }
        }
    }
}
