public class NetworkDataContainer
{
    public LayerDataContainer[] layerDataContainers;

    public NetworkDataContainer(NeuralNetwork network)
    {
        layerDataContainers = new LayerDataContainer[network.layers.Length];

        for(int i = 0; i < layerDataContainers.Length; i++)
        {
            layerDataContainers[i] = new LayerDataContainer(network.layers[i]);
        }
    }

    public LayerDataContainer GetLayerData(int index) => layerDataContainers[index];
}

public class LayerDataContainer
{
    public double[] inputs;
    public double[] weightedInputs;
    public double[] activations;
    //public double[] weightedInputDerivatives;

    public LayerDataContainer(Layer layer)
    {
        weightedInputs= new double[layer.nodesOut];
        activations = new double[layer.nodesOut];
        //weightedInputDerivatives= new double[layer.nodesOut];
    }
}

//different class so we can copy it for multithreading
public class WeightedInputDerivativeContainer
{
    public double[][] weightedInputDerivatives;

    public WeightedInputDerivativeContainer(NeuralNetwork network)
    {
        weightedInputDerivatives = new double[network.layers.Length][];

        for(int i = 0; i < weightedInputDerivatives.Length; i++)
        {
            weightedInputDerivatives[i] = new double[network.layers[i].nodesOut];
        }
    }
}
