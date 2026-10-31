using UnityEngine;
using UnityEngine.UI;

namespace Levels
{
    /// <summary>
    /// UI graphic that fills a convex four-corner area of its rect with a texture, cropped to
    /// cover the area without stretching, or with a plain color when no texture is set. Touches
    /// outside the area pass through, so slanted buttons can sit side by side.
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public class DiagonalSlice : MaskableGraphic, ICanvasRaycastFilter
    {
        private Texture _texture;
        private Vector2[] _corners; // normalized, in order around the shape

        public override Texture mainTexture => _texture != null ? _texture : s_WhiteTexture;

        public void Setup(Texture texture, Vector2[] corners)
        {
            _texture = texture;
            _corners = corners;
            SetAllDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if (_corners == null) return;

            Rect rect = rectTransform.rect;
            Vector2 min = _corners[0];
            Vector2 max = _corners[0];
            foreach (Vector2 corner in _corners)
            {
                min = Vector2.Min(min, corner);
                max = Vector2.Max(max, corner);
            }

            Vector2 pixelSize = Vector2.Scale(max - min, rect.size);
            Vector2 uvSize = Vector2.one;
            if (_texture != null)
            {
                float areaAspect = pixelSize.x / pixelSize.y;
                float textureAspect = (float)_texture.width / _texture.height;
                uvSize = areaAspect < textureAspect
                    ? new Vector2(areaAspect / textureAspect, 1f)
                    : new Vector2(1f, textureAspect / areaAspect);
            }
            Vector2 uvMin = (Vector2.one - uvSize) * 0.5f;

            foreach (Vector2 corner in _corners)
            {
                Vector2 position = rect.min + Vector2.Scale(corner, rect.size);
                Vector2 t = new Vector2((corner.x - min.x) / (max.x - min.x), (corner.y - min.y) / (max.y - min.y));
                vh.AddVert(position, color, uvMin + Vector2.Scale(t, uvSize));
            }
            vh.AddTriangle(0, 1, 2);
            vh.AddTriangle(0, 2, 3);
        }

        public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
        {
            if (_corners == null) return false;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, eventCamera, out Vector2 local))
                return false;

            Rect rect = rectTransform.rect;
            Vector2 point = new Vector2((local.x - rect.x) / rect.width, (local.y - rect.y) / rect.height);

            // Inside a convex shape, the point sits on the same side of every edge.
            int side = 0;
            for (int i = 0; i < _corners.Length; i++)
            {
                Vector2 a = _corners[i];
                Vector2 b = _corners[(i + 1) % _corners.Length];
                float cross = (b.x - a.x) * (point.y - a.y) - (b.y - a.y) * (point.x - a.x);
                int current = cross > 0f ? 1 : -1;
                if (side != 0 && current != side) return false;
                side = current;
            }
            return true;
        }
    }
}
