namespace FunctionLibrary;

public static class FunctionCalculator
{
    public static double CalculateA(double p, double x, double y)
    {
        if (p == 0)
        {
            return Math.Sqrt(x * y);
        }

        double mean = (Math.Pow(x, p) + Math.Pow(y, p)) / 2.0;
        return Math.Pow(mean, 1.0 / p);
    }
}
