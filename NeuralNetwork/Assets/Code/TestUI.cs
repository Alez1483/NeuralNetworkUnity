using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class TestUI : MonoBehaviour
{
    [SerializeField] GameObject testUI;
    [SerializeField] GameObject stopLearningButton;
    [SerializeField] TextMeshProUGUI[] percentTexts;

    [SerializeField] RawImage digitImage;
    [SerializeField] Color rightColor, wrongColor;
    Color neutralColor;
    Texture2D digitTexture;
    DataPoint[] testImages;
    int index;
    NeuralNetwork network;

    void Start()
    {
        neutralColor = percentTexts[0].color;
        network = Trainer.Instance.network;
        testImages = Trainer.Instance.testData;
        index = Random.Range(0, testImages.Length);
        digitTexture = testImages[index].ToTexture();
        digitImage.texture = digitTexture;
    }

    public void StopLearning()
    {
        Trainer.Instance.enabled = false;
        testUI.SetActive(true);
        stopLearningButton.SetActive(false);

        NextDigit();
    }

    public void ContinueLearning()
    {
        Trainer.Instance.enabled = true;
        testUI.SetActive(false);
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

    void OnDisable()
    {
        Destroy(digitTexture);
    }
}
