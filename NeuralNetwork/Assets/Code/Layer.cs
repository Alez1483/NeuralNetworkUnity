using static System.Math;

public class Layer
{
    public double[] weights;
    public double[] biases;
    public int nodesIn;
    public int nodesOut;

    public double[] weightGradient;
    public double[] biasGradient;

    public IActivation activation;

    public Layer(int nodesIn, int nodesOut, IActivation activation)
    {
        this.nodesIn = nodesIn;
        this.nodesOut = nodesOut;

        weights = new double[nodesIn * nodesOut];
        weightGradient = new double[weights.Length];
        biases = new double[nodesOut];
        biasGradient = new double[biases.Length];

        this.activation = activation;

        RandomizeWeights();
    }

    //j=zero based output index, k=input index
    public double GetWeight(int inIndex, int outIndex) => weights[outIndex * nodesIn + inIndex];

    public double[] EvaluateLayer(double[] input)
    {
        double[] output = new double[nodesOut];

        for(int outIndex = 0, wIndex = 0; outIndex < nodesOut; outIndex++)
        {
            double weightedInput = biases[outIndex];

            for(int inIndex = 0; inIndex < nodesIn; inIndex++, wIndex++)
            {
                weightedInput += input[inIndex] * weights[wIndex];
            }

            output[outIndex] = activation.Activate(weightedInput);
        }

        return output;
    }
    public double[] EvaluateLayer(double[] input, LayerDataContainer layerData)
    {
        layerData.inputs = input;

        double[] output = new double[nodesOut];

        for (int outIndex = 0, wIndex = 0; outIndex < nodesOut; outIndex++)
        {
            double weightedInput = biases[outIndex];

            for (int inIndex = 0; inIndex < nodesIn; inIndex++, wIndex++)
            {
                weightedInput += input[inIndex] * weights[wIndex];
            }

            layerData.weightedInputs[outIndex] = weightedInput;
            double act = activation.Activate(weightedInput);
            layerData.activations[outIndex] = act;
            output[outIndex] = act;
        }

        return output;
    }

    public void UpdateOutputLayerWeightedInputDerivatives(LayerDataContainer outputLayerData, double[] expectedOutput, ICost cost)
    {
        for(int outIndex = 0; outIndex < nodesOut; outIndex++)
        {
            double activationDeriv = activation.ActivationToDerivative(outputLayerData.activations[outIndex]);
            double costDeriv = cost.CostDerivative(outputLayerData.activations[outIndex], expectedOutput[outIndex]);
            outputLayerData.weightedInputDerivatives[outIndex] = activationDeriv * costDeriv;
        }
    }

    public void UpdateHiddenLayerWeightedInputDerivatives(LayerDataContainer layerData, Layer prevLayer, double[] prevLayerWInputDerivatives)
    {
        for(int outIndex = 0; outIndex < nodesOut; outIndex++)
        {
            double activationDeriv = activation.ActivationToDerivative(layerData.activations[outIndex]);
            double newWInputDerivative = 0;

            for (int prevOutIndex = 0; prevOutIndex < prevLayer.nodesOut; prevOutIndex++)
            {
                newWInputDerivative += prevLayer.GetWeight(outIndex, prevOutIndex) * prevLayerWInputDerivatives[prevOutIndex];
            }

            layerData.weightedInputDerivatives[outIndex] = newWInputDerivative * activationDeriv;
        }
    }

    public void ApplyBiasWeightGradients(double learnRate)
    {
        for(int i = 0; i < weightGradient.Length; i++)
        {
            weights[i] -= weightGradient[i] * learnRate;
            weightGradient[i] = 0;
        }

        for(int i = 0; i < biasGradient.Length; i++)
        {
            biases[i] -= biasGradient[i] * learnRate;
            biasGradient[i] = 0;
        }
    }

    //layerData must include up to date weight derivatives
    public void UpdateLayerGradients(LayerDataContainer layerData)
    {
        for(int outIndex = 0, wIndex = 0; outIndex < nodesOut; outIndex++)
        {
            double weightedInputDeriv = layerData.weightedInputDerivatives[outIndex];

            for (int inIndex = 0; inIndex < nodesIn; inIndex++, wIndex++)
            {
                weightGradient[wIndex] += layerData.inputs[inIndex] * weightedInputDeriv;
            }

            biasGradient[outIndex] += weightedInputDeriv;
        }
    }

    //normalized xavier
    public void RandomizeWeights()
    {
        double upper = Sqrt(6.0) / Sqrt(nodesIn + nodesOut);
        double lower = -upper;

        for(int i = 0; i < weights.Length; i++)
        {
            weights[i] = MyRandom.Range(lower, upper);
        }
    }
}
