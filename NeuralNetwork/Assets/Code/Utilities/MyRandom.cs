using System;
using static System.Math;

public static class MyRandom
{
    public static Random rnd = new Random();

    public static double Range(double min, double max)
    {
        return min + rnd.NextDouble() * (max - min);
    }

    public static double RandomFromNormalDistribution(double mean, double stddev)
    {
        double a = 1.0 - rnd.NextDouble();
        double b = 1.0 - rnd.NextDouble();
        double c = Sqrt(-2.0 * Log(a)) * Cos(2.0 * PI * b);
        return c * stddev + mean;
    }
}