public class NeuralNetwork
{
    public int inputCount;
    public Layer[] layers;
    IActivation hiddenActivation;
    IActivation outputActivation;

    public NeuralNetwork(IActivation hiddenAct, IActivation outputAct, params int[] layerSizes)
    {
        hiddenActivation = hiddenAct;
        outputActivation = outputAct;
        inputCount = layerSizes[0];
        layers = new Layer[layerSizes.Length - 1];

        for(int i = 0; i < layers.Length; i++)
        {
            layers[i] = new Layer(layerSizes[i], layerSizes[i + 1], hiddenActivation);
        }
        layers[layers.Length - 1].activation = outputActivation;
    }

    //can wrap around to the begin of the array if end is reached
    public void LearnBatch(DataPoint[] dataPoints, int startIndex, int batchSize, double learnRate, double momentum, NetworkDataContainer[] networkData, ICost cost)
    {
        System.Threading.Tasks.Parallel.For(0, batchSize, i =>
        {
            UpdateAllGradients(dataPoints[(startIndex + i) % dataPoints.Length], networkData[i], cost);
        });

        for(int layerIndex = 0; layerIndex < layers.Length; layerIndex++)
        {
            layers[layerIndex].ApplyBiasWeightGradients(learnRate / batchSize, momentum);
        }
    }

    public double[] Evaluate(double[] input)
    {
        for(int i = 0; i < layers.Length; i++)
        {
            input = layers[i].EvaluateLayer(input);
        }

        return input;
    }

    public int Classify(double[] input)
    {
        double[] output = Evaluate(input);
        return MaxIndex(output);
    }

    public void UpdateAllGradients(DataPoint dataPoint, NetworkDataContainer networkData, ICost cost)
    {
        //forward pass

        double[] layerOutput = dataPoint.pixelData;

        for (int i = 0; i < layers.Length; i++)
        {
            layerOutput = layers[i].EvaluateLayer(layerOutput, networkData.GetLayerData(i));
        }

        //backpropagation

        int layerIndex = layers.Length - 1;
        Layer layer = layers[layerIndex];
        LayerDataContainer layerData = networkData.GetLayerData(layerIndex);

        layer.UpdateOutputLayerWeightedInputDerivatives(layerData, dataPoint.expectedOutput, cost);
        layer.UpdateLayerGradients(layerData);
        
        for(layerIndex--; layerIndex >= 0; layerIndex--)
        {

            layer = layers[layerIndex];
            layerData = networkData.GetLayerData(layerIndex);

            layer.UpdateHiddenLayerWeightedInputDerivatives(layerData, layers[layerIndex + 1], networkData.GetLayerData(layerIndex + 1).weightedInputDerivatives);
            layer.UpdateLayerGradients(layerData);
        }
    }

    public Layer this[int i]
    {
        get 
        {
            return layers[i];
        }
        set 
        {
            layers[i] = value;
        }
    }

    public int MaxIndex(double[] input)
    {
        double max = double.MinValue;
        int maxI = 0;
        for (int i = 0; i < input.Length; i++)
        {
            if (input[i] > max)
            {
                max = input[i];
                maxI = i;
            }
        }
        return maxI;
    }
}
