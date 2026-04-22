using UnityEngine;
using UnityEngine.UI;

namespace Gameplay
{
    /// <summary>
    /// Binds a Slider (and optional icon Image) to AbilityCharger events.
    /// Hides the whole root unless Extended mode is active.
    /// </summary>
    public class AbilityChargeBarUI : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private Slider _chargeSlider;
        [SerializeField] private Image _armedIcon;
        [SerializeField] private Sprite _bombSprite;
        [SerializeField] private Sprite _hRowSprite;
        [SerializeField] private Sprite _vRowSprite;

        private void OnEnable()
        {
            AbilityCharger.OnChargeChanged += HandleChargeChanged;
            AbilityCharger.OnAbilityArmed += HandleArmed;
            RefreshVisibility();
        }

        private void OnDisable()
        {
            AbilityCharger.OnChargeChanged -= HandleChargeChanged;
            AbilityCharger.OnAbilityArmed -= HandleArmed;
        }

        private void Start()
        {
            RefreshVisibility();
            HandleChargeChanged(0, AbilityCharger.MaxCharge);
            if (_armedIcon != null) _armedIcon.enabled = false;
        }

        private void RefreshVisibility()
        {
            if (_root != null)
                _root.SetActive(GameMode.IsExtended);
        }

        private void HandleChargeChanged(int current, int max)
        {
            if (_chargeSlider != null)
            {
                _chargeSlider.maxValue = max;
                _chargeSlider.value = current;
            }
            if (_armedIcon != null && current < max)
                _armedIcon.enabled = false;
        }

        private void HandleArmed(AbilityType ability)
        {
            if (_armedIcon == null) return;
            _armedIcon.enabled = true;
            _armedIcon.sprite = ability switch
            {
                AbilityType.Bomb => _bombSprite,
                AbilityType.HorizontalRow => _hRowSprite,
                AbilityType.VerticalColumn => _vRowSprite,
                _ => _armedIcon.sprite,
            };
        }
    }
}
