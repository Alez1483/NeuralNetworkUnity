public class NeuralNetwork
{
    public int inputCount;
    public Layer[] layers;
    IActivation hiddenActivation;
    IActivation outputActivation;

    public NeuralNetwork(params int[] layerSizes)
    {
        hiddenActivation = new ReLu();
        outputActivation = new Sigmoid();
        inputCount = layerSizes[0];
        layers = new Layer[layerSizes.Length - 1];

        for(int i = 0; i < layers.Length; i++)
        {
            layers[i] = new Layer(layerSizes[i], layerSizes[i + 1], hiddenActivation);
        }
        layers[layers.Length - 1].activation = outputActivation;
    }

    //can wrap around to the begin of the array if end is reached
    public void LearnBatch(DataPoint[] dataPoints, int startIndex, int batchSize, double learnRate, NetworkDataContainer networkData, WeightedInputDerivativeContainer[] wIDContainers, ICost cost)
    {
        System.Threading.Tasks.Parallel.For(0, batchSize, i =>
        {
            UpdateAllGradients(dataPoints[(startIndex + i) % dataPoints.Length], wIDContainers[i], networkData, cost);
        });

        for(int layerIndex = 0; layerIndex < layers.Length; layerIndex++)
        {
            layers[layerIndex].ApplyBiasWeightGradients(learnRate / dataPoints.Length);
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

    public void UpdateAllGradients(DataPoint dataPoint, WeightedInputDerivativeContainer wIDContainer, NetworkDataContainer networkData, ICost cost)
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

        double[] wIDs = wIDContainer.weightedInputDerivatives[layerIndex];

        layer.UpdateOutputLayerWeightedInputDerivatives(layerData, wIDs, dataPoint.expectedOutput, cost);
        layer.UpdateLayerGradients(layerData, wIDs);
        
        for(layerIndex--; layerIndex >= 0; layerIndex--)
        {
            wIDs = wIDContainer.weightedInputDerivatives[layerIndex];

            layer = layers[layerIndex];
            layerData = networkData.GetLayerData(layerIndex);

            layer.UpdateHiddenLayerWeightedInputDerivatives(layerData, wIDs, layers[layerIndex + 1], wIDContainer.weightedInputDerivatives[layerIndex + 1]);
            layer.UpdateLayerGradients(layerData, wIDs);
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
