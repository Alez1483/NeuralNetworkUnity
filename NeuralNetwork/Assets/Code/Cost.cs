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

public interface ICost
{
    double Cost(double[] output, double[] expectedOutput);

    double CostDerivative(double output, double expetedOutput);
}
