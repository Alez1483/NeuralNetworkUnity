using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class TestUI : MonoBehaviour
{
    [SerializeField] GameObject UI;
    [SerializeField] GameObject[] UIs;
    int activeUI = 0;
    [SerializeField] GameObject stopLearningButton;
    [SerializeField] TextMeshProUGUI[] percentTexts;

    [SerializeField] RawImage digitImage;
    [SerializeField] Color rightColor, wrongColor;
    Color neutralColor;
    Texture2D digitTexture;
    DataPoint[] testImages;
    int index;
    NeuralNetwork network;

    [SerializeField] RawImage weightImage;
    [SerializeField] Gradient weightGradient;
    Texture2D weightTeture;
    [SerializeField] double weightDebugScaleMultiplier;
    int weightIndex;

    void Start()
    {
        UI.SetActive(false);

        neutralColor = percentTexts[0].color;
        network = Trainer.Instance.network;
        testImages = Trainer.Instance.testData;
        index = Random.Range(0, testImages.Length);
        digitTexture = testImages[index].ToTexture();
        digitImage.texture = digitTexture;

        weightTeture = network.layers[0].WeightsToTexture(0, testImages[0].imageWidth, testImages[0].imageHeight, weightGradient, weightDebugScaleMultiplier);
        weightIndex = -1;
        weightImage.texture = weightTeture;
    }

    public void StopLearning()
    {
        Trainer.Instance.enabled = false;
        UI.SetActive(true);
        stopLearningButton.SetActive(false);

        NextNeuron();
        NextDigit();
    }

    public void ContinueLearning()
    {
        Trainer.Instance.enabled = true;
        UI.SetActive(false);
        stopLearningButton.SetActive(true);
    }

    public void NextDigit()
    {
        index = (index + 1) % testImages.Length;
        var dataPoint = testImages[index];
        double[] output = network.Evaluate(dataPoint.pixelData);
        int prediction = network.MaxIndex(output);
        dataPoint.ToTexture(digitTexture);
        for(int i = 0; i < output.Length; i++)
        {
            percentTexts[i].color = i == dataPoint.label? rightColor : i == prediction? wrongColor : neutralColor;
            percentTexts[i].text = (output[i] * 100.0).ToString("0.0") + "%";
        }
    }
    public void NextWrong()
    {
        for(int i = 0; i < testImages.Length; i++)
        {
            index = (index + 1) % testImages.Length;
            DataPoint dataPoint = testImages[index];
            double[] output = network.Evaluate(dataPoint.pixelData);
            int prediction = network.MaxIndex(output);

            if (prediction != dataPoint.label)
            {
                dataPoint.ToTexture(digitTexture);
                for (int j = 0; j < output.Length; j++)
                {
                    percentTexts[j].color = j == dataPoint.label ? rightColor : j == prediction ? wrongColor : neutralColor;
                    percentTexts[j].text = (output[j] * 100.0).ToString("0.0") + "%";
                }
                return;
            }
        }
    }

    public void NextUI()
    {
        UIs[activeUI].SetActive(false);
        activeUI = (activeUI + 1) % UIs.Length;
        UIs[activeUI].SetActive(true);
    }
    public void PreviousUI()
    {
        UIs[activeUI].SetActive(false);
        activeUI = (activeUI + UIs.Length - 1) % UIs.Length;
        UIs[activeUI].SetActive(true);
    }

    public void NextNeuron()
    {
        Layer firstLayer = network.layers[0];
        weightIndex = (weightIndex + 1) % firstLayer.nodesOut;
        firstLayer.WeightsToTexture(weightTeture, weightIndex, testImages[0].imageWidth, testImages[0].imageHeight, weightGradient, weightDebugScaleMultiplier);
    }
    public void PreviousNeuron()
    {
        Layer firstLayer = network.layers[0];
        weightIndex = (weightIndex + firstLayer.nodesOut - 1) % firstLayer.nodesOut;
        firstLayer.WeightsToTexture(weightTeture, weightIndex, testImages[0].imageWidth, testImages[0].imageHeight, weightGradient, weightDebugScaleMultiplier);
    }

    void OnDisable()
    {
        Destroy(digitTexture);
        if (weightTeture != null)
        {
            Destroy(weightTeture);
        }
    }
}
