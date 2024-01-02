using static System.Math;

public struct Sigmoid : IActivation
{
    public double Activate(double weightedInput)
    {
        return 1.0 / (1.0 + Exp(-weightedInput));
    }
    public double Derivative(double weightedInput)
    {
        double activation = Activate(weightedInput);
        return activation * (1.0 - activation);
    }

    public double ActivationToDerivative(double activation)
    {
        return activation * (1.0 - activation);
    }
}

public struct ReLu : IActivation
{
    public double Activate(double weightedInput)
    {
        return Max(0.0, weightedInput);
    }

    public double Derivative(double weightedInput)
    {
        return weightedInput > 0.0? 1.0 : 0.0;
    }

    public double ActivationToDerivative(double activation)
    {
        return activation > 0.0 ? 1.0 : 0.0;
    }
}

public interface IActivation
{
    double Activate(double weightedInput);

    double Derivative(double weightedInput);

    double ActivationToDerivative(double activation);
}
