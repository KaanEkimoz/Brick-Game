namespace Tiles
{
    public enum TileKind
    {
        Normal,
        Fixed, // survives clears and abilities, and blocks falling tiles
        Gold,  // the Gold objective is won when none remain
    }
}
