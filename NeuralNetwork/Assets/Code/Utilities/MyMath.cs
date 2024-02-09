using System;
using static System.Math;
using Random = System.Random;
using UnityEngine;

public static class MyMath
{
    public static Random rnd = new Random();

    public static double RandomRange(double min, double max)
    {
        lock (rnd)
        {
            return min + rnd.NextDouble() * (max - min);
        }
    }
    public static int RandomRange(int min, int max)
    {
        lock (rnd)
        {
            return rnd.Next(min, max);
        }
    }

    static readonly object lock1 = new object();

    public static double Random01()
    {
            return rnd.NextDouble();
    }

    public static (double x, double y) RandomInsideUnitCircle()
    {
        lock (rnd)
        {
            double angle = (rnd.NextDouble() * 2.0 * PI);
            double dist = Sqrt(rnd.NextDouble());
            return (Sin(angle) * dist, Cos(angle) * dist);
        }
    }

    public static double Lerp(double min, double max, double t)
    {
        t = Clamp(t, 0.0, 1.0);
        return min + t * (max - min);
    }

    public static double RandomFromNormalDistribution(double mean, double stddev)
    {
        double a;
        double b;

        lock (rnd)
        {
            a = 1.0 - rnd.NextDouble();
            b = 1.0 - rnd.NextDouble();
        }
        double c = Sqrt(-2.0 * Log(a)) * Cos(2.0 * PI * b);
        return c * stddev + mean;
    }
}