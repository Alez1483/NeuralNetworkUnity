using UnityEngine;

public class DataPoint
{
    public int imageWidth;
    public int imageHeight;
    public int pixelCount;
    public double[] pixelData;
    public int label;
    public double[] expectedOutput;

    public DataPoint(double[] pixels, int width, int label)
    {
        pixelData = pixels;
        pixelCount = pixels.Length;
        imageWidth = width;
        imageHeight = pixelCount / width;
        this.label = label;
        expectedOutput = new double[10];
        expectedOutput[label] = 1.0;
    }

    public double GetPixel(int x, int y) => pixelData[x + imageWidth * y];
    public void SetPixel(int x, int y, double value)
    { 
        pixelData[x + imageWidth * y] = value;
    }

    public Texture2D ToTexture()
    {
        var texture = new Texture2D(imageWidth, imageHeight, TextureFormat.RGB24, false);
        texture.filterMode = FilterMode.Point;

        ToTexture(texture);

        return texture;
    }

    public void ToTexture(Texture2D texture)
    {
        if (texture.width != imageWidth || texture.height != imageHeight)
        {
            Debug.LogError("Given texture has wrong dimensions");
            return;
        }

        for (int i = 0; i < pixelCount; i++)
        {
            texture.SetPixel(i % imageWidth, i / imageWidth, Color.white * (float)pixelData[i]);
        }

        texture.Apply(false);
    }
}