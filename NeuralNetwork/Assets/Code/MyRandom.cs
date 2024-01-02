using System;

public static class MyRandom
{
    public static Random rnd = new Random();

    public static double Range(double min, double max)
    {
        return min + rnd.NextDouble() * (max - min);
    }
}