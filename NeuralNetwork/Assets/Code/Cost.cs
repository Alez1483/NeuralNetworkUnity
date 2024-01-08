using static System.Math;

public struct MeanSquaredError : ICost
{
    public double Cost(double[] output, double[] expectedOutput)
    {
        double cost = 0;

        for(int i = 0; i < output.Length; i++)
        {
            double mean = output[i] - expectedOutput[i];
            cost += mean * mean;
        }

        return cost * 0.5;
    }

    public double CostDerivative(double output, double expetedOutput)
    {
        return output - expetedOutput;
    }
}

public struct CrossEntropy : ICost
{
    public double Cost(double[] output, double[] expectedOutput)
    {
        double cost = 0.0;
        for(int i = 0; i < output.Length; i++)
        {
            if (expectedOutput[i] != 0.0)
            {
                cost += expectedOutput[i] * Log(output[i]);
            }
        }
        return cost;
    }

    public double CostDerivative(double output, double expetedOutput)
    {
        throw new System.NotImplementedException();
    }
}

public interface ICost
{
    double Cost(double[] output, double[] expectedOutput);

    double CostDerivative(double output, double expetedOutput);
}
