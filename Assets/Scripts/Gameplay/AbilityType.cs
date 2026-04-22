namespace Gameplay
{
    public enum AbilityType
    {
        Bomb,           // clears a 3x3 area centred on the ability piece
        HorizontalRow,  // clears the entire row the piece lands on
        VerticalColumn, // clears the entire column the piece lands on
    }
}
