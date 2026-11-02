using UnityEngine;
using UnityEngine.UI;

namespace Scene
{
    /// <summary>Shows a Toggle as an on/off switch. Follows isOn each frame because the prefs
    /// restore it without notifying, and runs on unscaled time for the pause menu.</summary>
    public class ToggleSwitch : MonoBehaviour
    {
        private const float Width = 48f;
        private const float Height = 28f;
        private const float KnobInset = 4f;
        private const float KnobSpeed = 12f;

        private Toggle _toggle;
        private Image _track;
        private RectTransform _knob;
        private Color _onColor;
        private Color _offColor;
        private float _position;

        public void Build(Toggle toggle, float rowWidth, Sprite pill, Color onColor, float offAlpha)
        {
            _toggle = toggle;
            _onColor = onColor;
            _offColor = new Color(1f, 1f, 1f, offAlpha);

            toggle.transition = Selectable.Transition.None;
            Graphic knob = toggle.graphic;
            toggle.graphic = null;

            RectTransform rect = (RectTransform)transform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.localScale = Vector3.one;
            rect.sizeDelta = new Vector2(Width, Height);
            rect.anchoredPosition = new Vector2((rowWidth - Width) * 0.5f, 0f);

            _track = GetComponent<Image>();
            _track.sprite = pill;
            _track.type = Image.Type.Sliced;

            Image knobImage = (Image)knob;
            knobImage.sprite = pill;
            knobImage.type = Image.Type.Sliced;
            knobImage.color = Color.white;
            knobImage.canvasRenderer.SetAlpha(1f);
            _knob = knobImage.rectTransform;
            _knob.anchorMin = _knob.anchorMax = _knob.pivot = new Vector2(0.5f, 0.5f);
            _knob.sizeDelta = Vector2.one * (Height - KnobInset * 2f);

            _position = toggle.isOn ? 1f : 0f;
            Show();
        }

        private void Update()
        {
            if (_toggle == null) return;
            _position = Mathf.MoveTowards(_position, _toggle.isOn ? 1f : 0f, KnobSpeed * Time.unscaledDeltaTime);
            Show();
        }

        private void Show()
        {
            float travel = (Width - Height) * 0.5f;
            _knob.anchoredPosition = new Vector2(Mathf.Lerp(-travel, travel, _position), 0f);
            _track.color = Color.Lerp(_offColor, _onColor, _position);
        }
    }
}
