namespace Gameplay
{
    public enum AbilityType
    {
        Bomb,   // clears a 3x3 area centred on the charged tile
        Laser,  // clears the charged tile's row or column; each rotation flips the axis
        Patch,  // fills the lowest covered holes in the stack
    }
}
