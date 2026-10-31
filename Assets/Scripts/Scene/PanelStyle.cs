using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Scene
{
    /// <summary>
    /// The look shared by every panel: the Settings panel's glowing frame, neon headings and
    /// the main menu's bright buttons. Sentences stay in the plain font so they can be read.
    /// </summary>
    public static class PanelStyle
    {
        public const string FramePanelName = "SettingsPanel";
        public const string ButtonName = "B_Play";

        private const float MinFontSizeRatio = 0.55f;
        private const float ButtonGlowRatio = 0.16f; // the button sprite's glow, per side

        public static readonly Color ButtonTextColor = new Color(0.08f, 0.3f, 0.8f, 1f);

        public static Image FindFrame()
        {
            foreach (Image image in Resources.FindObjectsOfTypeAll<Image>())
                if (image.name == FramePanelName && image.gameObject.scene.IsValid()) return image;
            return null;
        }

        public static Button FindButton()
        {
            foreach (Button button in Resources.FindObjectsOfTypeAll<Button>())
                if (button.name == ButtonName && button.gameObject.scene.IsValid()) return button;
            return null;
        }

        /// <summary>Copies a frame onto an image at the same on-screen border thickness.</summary>
        public static void ApplyFrame(Image panel, Image frame)
        {
            panel.sprite = frame.sprite;
            panel.type = frame.type;
            panel.pixelsPerUnitMultiplier = frame.pixelsPerUnitMultiplier * panel.transform.lossyScale.x / frame.transform.lossyScale.x;
            Color color = frame.color;
            color.a = 1f;
            panel.color = color;
        }

        /// <summary>Keeps a heading or button label on one line, shrinking long translations.</summary>
        public static void FitLine(TMP_Text label, float size)
        {
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.enableAutoSizing = true;
            label.fontSizeMax = size;
            label.fontSizeMin = size * MinFontSizeRatio;
            label.fontSize = size;
        }

        /// <summary>Fits a button's label inside the visible button, leaving out the glow.</summary>
        public static void FitButtonLabel(TMP_Text label, float size, float buttonWidth)
        {
            FitLine(label, size);
            float glow = buttonWidth * ButtonGlowRatio;
            label.margin = new Vector4(glow, 0f, glow, 0f);
        }
    }
}
