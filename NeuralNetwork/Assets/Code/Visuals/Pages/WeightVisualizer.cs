using UnityEngine;
using UnityEngine.UI;

public class WeightVisualizer : MonoBehaviour
{
    DataPoint[] testImages;
    NeuralNetwork network;

    [SerializeField] RawImage weightImage;
    [SerializeField] Gradient weightGradient;
    [SerializeField] double weightDebugScaleMultiplier;
    
    Texture2D weightTeture;
    int weightIndex;

    void OnEnable()
    {
        if (weightTeture == null)
        {
            network = Trainer.Instance.network;
            testImages = Trainer.Instance.testData;
            weightTeture = network[0].WeightsToTexture(0, testImages[0].imageWidth, testImages[0].imageHeight, weightGradient, weightDebugScaleMultiplier);
            weightIndex = 0;
            weightImage.texture = weightTeture;
        }

        UpdateTexture();
    }

    public void StopLearning()
    {
        NextNeuron();
    }

    void UpdateTexture()
    {
        network[0].WeightsToTexture(weightTeture, weightIndex, testImages[0].imageWidth, testImages[0].imageHeight, weightGradient, weightDebugScaleMultiplier);
    }

    public void NextNeuron()
    {
        weightIndex = (weightIndex + 1) % network[0].nodesOut;
        UpdateTexture();
    }
    public void PreviousNeuron()
    {
        weightIndex = (weightIndex + network[0].nodesOut - 1) % network[0].nodesOut;
        UpdateTexture();
    }

    void OnDestroy()
    {
        if (weightTeture != null)
        {
            Destroy(weightTeture);
        }
    }
}
