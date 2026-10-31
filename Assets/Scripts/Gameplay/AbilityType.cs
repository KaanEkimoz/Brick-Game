namespace Gameplay
{
    public enum AbilityType
    {
        Bomb,   // single-tile piece that clears a 3x3 area around where it lands
        Laser,  // clears the charged tile's row or column; each rotation flips the axis
        Patch,  // fills the lowest covered holes in the stack
        Dynamite, // two-tile piece that clears a 3x4 area along its direction
    }
}
