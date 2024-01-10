using static System.Math;
using System.Runtime.CompilerServices;

public struct Sigmoid : IActivation
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double Activate(double weightedInput)
    {
        return 1.0 / (1.0 + Exp(-weightedInput));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double Derivate(double weightedInput)
    {
        double activation = Activate(weightedInput);
        return activation * (1.0 - activation);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ActivateLayer(double[] weightedInputs)
    {
        for(int i = 0; i < weightedInputs.Length; i++)
        {
            weightedInputs[i] = 1.0 / (1.0 + Exp(-weightedInputs[i]));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DerivateLayer(double[] weightedInputs)
    {
        ActivateLayer(weightedInputs);

        for(int i = 0; i < weightedInputs.Length; i++)
        {
            weightedInputs[i] = weightedInputs[i] * (1.0 - weightedInputs[i]);
        }
    }
}

public struct ReLu : IActivation
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double Activate(double weightedInput)
    {
        return Max(0.0, weightedInput);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double Derivate(double weightedInput)
    {
        return weightedInput > 0.0 ? 1.0 : 0.0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ActivateLayer(double[] weightedInputs)
    {
        for(int i = 0; i < weightedInputs.Length; i++)
        {
            weightedInputs[i] = Max(0.0, weightedInputs[i]);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DerivateLayer(double[] weightedInputs)
    {
        for(int i = 0; i < weightedInputs.Length; i++)
        {
            weightedInputs[i] = weightedInputs[i] > 0.0 ? 1.0 : 0.0;
        }
    }
}
public struct Softmax : IActivation
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double Activate(double weightedInput)
    {
        throw new System.NotImplementedException();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ActivateLayer(double[] weightedInputs)
    {
        double sum = 0.0;
        for(int i = 0; i < weightedInputs.Length; i++)
        {
            double exp = Exp(weightedInputs[i]);
            weightedInputs[i] = exp;
            sum += exp;
        }

        for(int i = 0; i < weightedInputs.Length; i++)
        {
            weightedInputs[i] /= sum;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double Derivate(double weightedInput)
    {
        throw new System.NotImplementedException();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DerivateLayer(double[] weightedInputs)
    {
        throw new System.NotImplementedException();
    }
}

public interface IActivation
{
    double Activate(double weightedInput);

    double Derivate(double weightedInput);

    void ActivateLayer(double[] weightedInputs);

    void DerivateLayer(double[] weightedInputs);
}
