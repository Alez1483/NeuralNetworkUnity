using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PreviewImage : MonoBehaviour
{
    NeuralNetwork network;
    DataPoint[] testImages;

    [SerializeField] TextMeshProUGUI[] percentTexts;
    [SerializeField] RawImage digitImage;
    [SerializeField] Color rightColor, wrongColor;

    Color neutralColor;
    Texture2D digitTexture;
    int index = 0;

    public void OnEnable()
    {
        if (digitTexture == null)
        {
            neutralColor = percentTexts[0].color;
            network = Trainer.Instance.network;
            testImages = Trainer.Instance.testData;
            index = Random.Range(0, testImages.Length);
            digitTexture = testImages[index].ToTexture();
            digitImage.texture = digitTexture;
        }

        UpdateTexture();
    }

    public void NextDigit()
    {
        index = (index + 1) % testImages.Length;

        UpdateTexture();
    }
    void UpdateTexture()
    {
        var dataPoint = testImages[index];
        double[] output = network.Evaluate(dataPoint.pixelData);
        int prediction = network.MaxIndex(output);
        dataPoint.ToTexture(digitTexture);
        for (int i = 0; i < output.Length; i++)
        {
            percentTexts[i].color = i == dataPoint.label ? rightColor : i == prediction ? wrongColor : neutralColor;
            percentTexts[i].text = (output[i] * 100.0).ToString("0.0") + "%";
        }
    }
    public void NextWrong()
    {
        for (int i = 0; i < testImages.Length; i++)
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
    void OnDestroy()
    {
        if (digitTexture != null)
        {
            Destroy(digitTexture);
        }
    }
}
