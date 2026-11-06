using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Scene
{
    /// <summary>
    /// Readable labels for the main menu's bright buttons. The shipped labels are a faint
    /// see-through blue that blends into the glowing pill; this uses the solid blue the pause
    /// menu uses. An active theme's colors win.
    /// </summary>
    public static class MenuButtonStyle
    {
        public static void Primary(Button button)
        {
            if (button == null) return;
            Image image = button.GetComponent<Image>();
            if (image != null) image.color = Themes.ThemeManager.PrimaryButtonTint(Color.white);
            TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
            if (label != null) label.color = Themes.ThemeManager.PrimaryButtonText(PanelStyle.ButtonTextColor);
        }
    }
}
