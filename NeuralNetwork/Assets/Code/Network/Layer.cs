using System.Drawing;
using UnityEngine;
using static System.Math;

public class Layer
{
    public double[] weights;
    public double[] biases;
    public int nodesIn;
    public int nodesOut;

    public double[] weightGradient;
    public double[] biasGradient;

    public double[] weightVelocity;
    public double[] biasVelocity;

    public IActivation activation;

    public Layer(int nodesIn, int nodesOut, IActivation activation)
    {
        this.nodesIn = nodesIn;
        this.nodesOut = nodesOut;

        weights = new double[nodesIn * nodesOut];
        weightGradient = new double[weights.Length];
        weightVelocity = new double[weights.Length];
        biases = new double[nodesOut];
        biasGradient = new double[biases.Length];
        biasVelocity = new double[biases.Length];

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

            output[outIndex] = weightedInput;
        }

        activation.ActivateLayer(output);

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
            output[outIndex] = weightedInput;                                                                                                             
        }

        activation.ActivateLayer(output);
        output.CopyTo(layerData.activations, 0);

        return output;
    }

    public void UpdateOutputLayerWeightedInputDerivatives(LayerDataContainer outputLayerData, double[] expectedOutput, ICost cost)
    {
        //works only with one-hot encoding
        if (activation is Softmax && cost is CrossEntropy)
        {
            for (int outIndex = 0; outIndex < nodesOut; outIndex++)
            {
                outputLayerData.weightedInputDerivatives[outIndex] =  outputLayerData.activations[outIndex] - expectedOutput[outIndex];
            }
            return;
        }

        for(int outIndex = 0; outIndex < nodesOut; outIndex++)
        {
            double activationDeriv = activation.Derivate(outputLayerData.weightedInputs[outIndex]);
            double costDeriv = cost.CostDerivative(outputLayerData.activations[outIndex], expectedOutput[outIndex]);
            outputLayerData.weightedInputDerivatives[outIndex] = activationDeriv * costDeriv;
        }
    }

    public void UpdateHiddenLayerWeightedInputDerivatives(LayerDataContainer layerData, Layer prevLayer, double[] prevLayerWInputDerivatives)
    {
        for(int outIndex = 0; outIndex < nodesOut; outIndex++)
        {
            double activationDeriv = activation.Derivate(layerData.weightedInputs[outIndex]);
            double newWInputDerivative = 0;

            for (int prevOutIndex = 0; prevOutIndex < prevLayer.nodesOut; prevOutIndex++)
            {
                newWInputDerivative += prevLayer.GetWeight(outIndex, prevOutIndex) * prevLayerWInputDerivatives[prevOutIndex];
            }
            layerData.weightedInputDerivatives[outIndex] = newWInputDerivative * activationDeriv;
        }
    }

    public void ApplyBiasWeightGradients(double learnRate, double momentum)
    {
        for(int i = 0; i < weightGradient.Length; i++)
        {
            weightVelocity[i] = weightVelocity[i] * momentum - weightGradient[i] * learnRate;
            weights[i] += weightVelocity[i];
            weightGradient[i] = 0;
        }

        for(int i = 0; i < biasGradient.Length; i++)
        {
            biasVelocity[i] = biasVelocity[i] * momentum - biasGradient[i] * learnRate;
            biases[i] += biasVelocity[i];
            biasGradient[i] = 0;
        }
    }

    //layerData must include up to date weight derivatives
    public void UpdateLayerGradients(LayerDataContainer layerData)
    {
        lock(weightGradient)
        {
            for (int outIndex = 0, wIndex = 0; outIndex < nodesOut; outIndex++)
            {
                double weightedInputDeriv = layerData.weightedInputDerivatives[outIndex];

                for (int inIndex = 0; inIndex < nodesIn; inIndex++, wIndex++)
                {
                    weightGradient[wIndex] += layerData.inputs[inIndex] * weightedInputDeriv;
                }

                biasGradient[outIndex] += weightedInputDeriv;
            }
        }
    }

    //He et al. initialization
    public void RandomizeWeights()
    {
        double stddev = Sqrt(2.0 / nodesIn);

        for(int i = 0; i < weights.Length; i++)
        {
            weights[i] = MyMath.RandomFromNormalDistribution(0.0, stddev);
        }
    }

    public Texture2D WeightsToTexture(int outIndex, int width, int height, Gradient color, double mult)
    {
        var texture = new Texture2D(width, height, TextureFormat.RGB24, false);
        texture.filterMode = FilterMode.Point;

        WeightsToTexture(texture, outIndex, width, height, color, mult);

        return texture;
    }
    public void WeightsToTexture(Texture2D texture, int outIndex, int width, int height, Gradient color, double mult)
    {
        //assumes square texture atm
        if (texture.width != width || texture.height != height)
        {
            Debug.LogError("Given texture has wrong dimensions");
            return;
        }

        for (int i = 0, j = outIndex * nodesIn; i < nodesIn; i++, j++)
        {
            texture.SetPixel(i % width, i / width, color.Evaluate((float)(weights[j] * mult + 0.5)));
        }

        texture.Apply(false);
    }
}
