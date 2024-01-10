public static class EndiannessHelper
{
    public static int Reverse(int value)
    {
        return (value & 0x000000FF) << 24 |
               ((int)(value & 0xFF000000)) >> 24 |
               (value & 0x00FF0000) >> 8 |
               (value & 0x0000FF00) << 8;
    }
}
