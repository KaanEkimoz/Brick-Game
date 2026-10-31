using TMPro;
using UnityEngine;

namespace Levels
{
    /// <summary>The easy-to-read font used by dialogs and panels.</summary>
    public static class PlainFont
    {
        private const string ResourcePath = "Fonts & Materials/LiberationSans SDF";

        private static TMP_FontAsset _asset;

        public static TMP_FontAsset Asset
        {
            get
            {
                if (_asset == null) _asset = Resources.Load<TMP_FontAsset>(ResourcePath);
                return _asset;
            }
        }
    }
}
