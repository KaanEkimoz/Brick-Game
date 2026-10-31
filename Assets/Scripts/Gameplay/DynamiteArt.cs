using UnityEngine;

namespace Gameplay
{
    /// <summary>The dynamite image, split across the piece's two tiles.</summary>
    public static class DynamiteArt
    {
        private const string TexturePath = "Abilities/dynamite";

        private static Texture2D Texture => Resources.Load<Texture2D>(TexturePath);

        public static Sprite Whole()
        {
            Texture2D texture = Texture;
            return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }

        /// <param name="tileWidth">World width of one tile, so the halves cover it exactly.</param>
        public static Sprite Half(bool top, float tileWidth)
        {
            Texture2D texture = Texture;
            float height = texture.height * 0.5f;
            Rect rect = new Rect(0f, top ? height : 0f, texture.width, height);
            return Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), texture.width / tileWidth);
        }
    }
}
