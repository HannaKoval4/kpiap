using System;

namespace FunctionLibrary;

public static class FunctionCalculator
{
    public static double CalculateA(double p, double x, double y)
    {
        return Math.Tan(p / 9.0) * (x + y);
    }
}
