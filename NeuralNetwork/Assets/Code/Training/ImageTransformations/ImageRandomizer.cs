using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Rendering.CameraUI;

[System.Serializable]
public class ImageRandomizer
{
    [SerializeField, Range(0f, 45f)] float rotationAmount;
    [SerializeField, Range(0f, 20f)] float translationAmount;
    [SerializeField, Range(0f, 2f)] float minScale;
    [SerializeField, Range(0f, 2f)] float maxScale;

    public void RandomizeImage(DataPoint image)
    {
        double[] newData = new double[image.pixelCount];
        RandomizeImage(image, newData);
        image.pixelData = newData;
    }

    public void RandomizeImage(DataPoint image, double[] randomizedData)
    {
        Vector2 halfImageVec = new Vector2(image.imageWidth * 0.5f, image.imageHeight * 0.5f);
        var matrix = Matrix3x3.TranslationMatrix(halfImageVec);
        matrix *= Matrix3x3.ScaleMatrix(Vector2.one / (float)MyMath.RandomRange(minScale, maxScale));
        matrix *= Matrix3x3.RotationMatrix(MyMath.RandomRange(-rotationAmount, rotationAmount));
        (double randX, double randY) = MyMath.RandomInsideUnitCircle();
        matrix *= Matrix3x3.TranslationMatrix(new Vector2((float)randX, (float)randY) * translationAmount - halfImageVec);

        for (int y = 0, i = 0; y < image.imageHeight; y++)
        {
            for (int x = 0; x < image.imageWidth; x++, i++)
            {
                Vector3 transformedPos = matrix * new Vector3(x, y, 1);
                randomizedData[i] = image.GetPixelInterpolated(transformedPos.x, transformedPos.y);
            }
        }
    }

    public void RandomizeImages(DataPoint[] inData, DataPoint[] outData)
    {
        System.Threading.Tasks.Parallel.For(0, inData.Length, i =>
        {
            RandomizeImage(inData[i], outData[i].pixelData);
        });
    }

    public void RandomizeImages(DataPoint[] inData)
    {
        System.Threading.Tasks.Parallel.For(0, inData.Length, i =>
        {
            RandomizeImage(inData[i]);
        });
    }
}
