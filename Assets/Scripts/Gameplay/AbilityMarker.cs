using UnityEngine;

namespace Gameplay
{
    /// <summary>
    /// Added to a charged tetromino at spawn. The executor reads this to know which effect to
    /// fire, and from which tile, when this specific piece settles on the board.
    /// </summary>
    public class AbilityMarker : MonoBehaviour
    {
        [SerializeField] private AbilityType _ability;
        [SerializeField] private int _tileIndex;
        public AbilityType Ability => _ability;
        public int TileIndex => _tileIndex;
        public void SetAbility(AbilityType ability) => _ability = ability;
        public void SetTileIndex(int tileIndex) => _tileIndex = tileIndex;
    }
}
