using UnityEngine;

namespace Gameplay
{
    /// <summary>
    /// Attached to ability-piece prefabs. The executor reads this to know which effect to fire
    /// when this specific piece settles on the board.
    /// </summary>
    public class AbilityMarker : MonoBehaviour
    {
        [SerializeField] private AbilityType _ability;
        public AbilityType Ability => _ability;
        public void SetAbility(AbilityType ability) => _ability = ability;
    }
}
