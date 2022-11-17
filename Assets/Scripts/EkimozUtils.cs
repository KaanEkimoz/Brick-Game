public class EkimozUtils
{
    /// <summary>
    /// True modulo operation that works for positive and negative values.
    /// </summary>
    /// <param name="x">The dividend</param>
    /// <param name="m">The divisor</param>
    /// <returns>Returns the remainder of x divided by m</returns>
    public static int Mod(int x, int m)
    {
        return (x % m + m) % m;
    }
}

