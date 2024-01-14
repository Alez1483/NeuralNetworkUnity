using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DigitDrawer : MonoBehaviour
{
    NeuralNetwork network;

    [SerializeField] TextMeshProUGUI[] percentTexts;
    [SerializeField] RawImage digitImage;

    [SerializeField] Gradient textGradient;

    NumberFormatInfo percentFormat = new NumberFormatInfo { PercentPositivePattern = 1 };

    Texture2D digitTexture;
    double[] pixels;

    [SerializeField] Slider radiusSlider;

    [SerializeField, Range(0f, 2f)] float edgeRadius = 1.25f;

    int width;
    int height;

    bool mouseDown = false;
    Vector2 imageSpaceMousePos;

    public void OnEnable()
    {
        if (digitTexture == null)
        {
            var trainer = Trainer.Instance;

            network = trainer.network;
            width = trainer.trainData[0].imageWidth;
            height = trainer.trainData[0].imageHeight;
            pixels = new double[width * height];
            digitTexture = new Texture2D(width, height, TextureFormat.RGB24, false);
            digitTexture.filterMode = FilterMode.Point;
            digitImage.texture = digitTexture;
        }

        UpdateTexture();
    }

    void Update()
    {
        bool mouse0 = Input.GetKey(KeyCode.Mouse0);
        bool mouse1 = Input.GetKey(KeyCode.Mouse1);
        if (mouse0 || mouse1)
        {
            Vector2 mousePos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(digitImage.rectTransform, Input.mousePosition, null, out mousePos);
            mousePos = (mousePos / digitImage.rectTransform.rect.width + Vector2.one * 0.5f) * new Vector2(width, height) - new Vector2(0.5f, 0.5f);

            if (!mouseDown)
            {
                //mouse down
                mouseDown = true;
                imageSpaceMousePos = mousePos;
            }

            float radius = radiusSlider.value;

            double solidRadius = Math.Max(radius - edgeRadius, 0.0);

            for(int i = 0; i < pixels.Length; i++)
            {
                int x = i % width;
                int y = i / height;

                float dist = DistToLineSegment(imageSpaceMousePos, mousePos, new Vector2(x, y));

                double temp;

                if (dist > solidRadius)
                {
                    temp = Math.Min((dist - solidRadius) / (radius - solidRadius), 1.0);
                    temp *= temp;
                }
                else
                {
                    temp = 0.0;
                }
                

                if (mouse0)
                {
                    pixels[i] = Math.Max(1.0f - temp, pixels[i]);
                }
                else
                {
                    pixels[i] = Math.Min(temp, pixels[i]);
                }
            }

            UpdateTexture();

            imageSpaceMousePos = mousePos;
        }
        else
        {
            mouseDown = false;
        }
    }

    void UpdateTexture()
    {
        for(int i = 0; i < pixels.Length; i++)
        {
            digitTexture.SetPixel(i % width, i / width, Color.white * (float)pixels[i]);
        }
        digitTexture.Apply(false);
        
        double[] output = network.Evaluate(pixels);
        int prediction = network.MaxIndex(output);
        for (int i = 0; i < output.Length; i++)
        {
            percentTexts[i].color = textGradient.Evaluate((float)output[i]);
            percentTexts[i].text = output[i].ToString("P1", percentFormat);
        }
    }

    public void ClearImage()
    {
        for(int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = 0.0;
        }
        UpdateTexture();
    }

    float DistToLineSegment(Vector2 a, Vector2 b, Vector2 p)
    {
        Vector2 ab = b - a;

        if (ab == Vector2.zero)
        {
            return Vector2.Distance(a, p);
        }
        float sqrDist = Vector2.Dot(ab, ab);
        Vector2 temp = (p - a) * ab;
        float u = Mathf.Clamp01((temp.x + temp.y) / sqrDist);
        Vector2 xy = a + u * ab;
        Vector2 d = xy - p;
        return Mathf.Sqrt(Vector2.Dot(d , d));
    }

    void OnDestroy()
    {
        if (digitTexture != null)
        {
            Destroy(digitTexture);
        }
    }
}
