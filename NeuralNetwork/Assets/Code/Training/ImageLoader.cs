using System.IO;
using UnityEngine;

public class ImageLoader
{
    //state machine

    public string ImagePath;
    private FileStream imageStream;
    private BinaryReader imageReader;

    public string LabelPath;
    private FileStream labelStream;
    private BinaryReader labelReader;

    public int width;
    public int height;
    public int dataPoints;

    public void InitializeReaders()
    {
        imageStream?.Dispose();
        imageReader?.Dispose();
        labelStream?.Dispose();
        labelReader?.Dispose();

        imageStream = File.OpenRead(ImagePath);
        imageReader = new BinaryReader(imageStream);
        labelStream = File.OpenRead(LabelPath);
        labelReader = new BinaryReader(labelStream);

        //image stream
        int magicNum = EndiannessHelper.Reverse(imageReader.ReadInt32());

        if (magicNum != 0x00000803)
        {
            UnityEngine.Debug.LogError("Incorrect image file format!");
            return;
        }
        dataPoints = EndiannessHelper.Reverse(imageReader.ReadInt32());
        height = EndiannessHelper.Reverse(imageReader.ReadInt32());
        width = EndiannessHelper.Reverse(imageReader.ReadInt32());

        //label stream
        magicNum = EndiannessHelper.Reverse(labelReader.ReadInt32());
        if (magicNum != 0x00000801)
        {
            UnityEngine.Debug.LogError("Incorrect label file format!");
            return;
        }
        int labelCount = EndiannessHelper.Reverse(labelReader.ReadInt32());
        if (labelCount != dataPoints)
        {
            UnityEngine.Debug.LogError("Label and image count doesn't match!");
            return;
        }
    }

    public DataPoint LoadImage()
    {
        int pixels = width * height;
        double[] pixelData = new double[pixels];

        int count = 0;
        for(int j = pixels - width; j >= 0; j -= width)
        {
            int last = j + width;
            for(int i = j; i < last; i++)
            {
                double value = imageReader.ReadByte() / 255.0;
                pixelData[i] = value;
                count++;
            }
        }
        int label = labelReader.ReadByte();

        return new DataPoint(pixelData, width, label);
    }

    ~ImageLoader()
    {
        imageStream?.Dispose();
        imageReader?.Dispose();
        labelStream?.Dispose();
        labelReader?.Dispose();
    }
}
public static class ImageLoader2
{
    public static DataPoint[] LoadImages(params (string imagePath, string labelPath)[] paths)
    {
        int[] imagesInFiles = new int[paths.Length];
        FileStream[] imageStreams = new FileStream[paths.Length];
        FileStream[] labelStreams = new FileStream[paths.Length];
        BinaryReader[] imageReaders = new BinaryReader[paths.Length];
        BinaryReader[] labelReaders = new BinaryReader[paths.Length];

        int imageCount = 0;

        int width = 0;
        int height = 0;

        for (int fileIndex = 0; fileIndex < paths.Length; fileIndex++)
        {
            int imagesInFile;

            //look how beautifully those equal signs line up :)
            var imageStream = File.OpenRead(paths[fileIndex].imagePath);
            var imageReader = new BinaryReader(imageStream);
            var labelStream = File.OpenRead(paths[fileIndex].labelPath);
            var labelReader = new BinaryReader(labelStream);
            imageStreams[fileIndex] = imageStream;
            imageReaders[fileIndex] = imageReader;
            labelStreams[fileIndex] = labelStream;
            labelReaders[fileIndex] = labelReader;

            //image stream
            int magicNum = EndiannessHelper.Reverse(imageReader.ReadInt32());

            if (magicNum != 0x00000803)
            {
                Debug.LogError("Incorrect image file format!");
                return null;
            }

            imagesInFile = EndiannessHelper.Reverse(imageReader.ReadInt32());
            int fileHeight = EndiannessHelper.Reverse(imageReader.ReadInt32());
            int fileWidth = EndiannessHelper.Reverse(imageReader.ReadInt32());

            if (fileIndex != 0)
            {
                if (fileHeight != height || fileWidth != width)
                {
                    Debug.LogError("Inconsistent image dimensions in different files");
                    return null;
                }
            }

            height = fileHeight;
            width = fileWidth;

            //label stream
            magicNum = EndiannessHelper.Reverse(labelReader.ReadInt32());
            if (magicNum != 0x00000801)
            {
                Debug.LogError("Incorrect label file format!");
                return null;
            }

            int labelsInFile = EndiannessHelper.Reverse(labelReader.ReadInt32());

            if (labelsInFile != imagesInFile)
            {
                Debug.LogError("Label and image count doesn't match!");
                return null;
            }

            imagesInFiles[fileIndex] = imagesInFile;
            imageCount += imagesInFile;
        }

        DataPoint[] allData = new DataPoint[imageCount];
        byte[][] allImageData = new byte[paths.Length][];
        byte[][] allLabelData = new byte[paths.Length][];

        int pixels = width * height;

        for(int fileIndex = 0; fileIndex < paths.Length; fileIndex++)
        {
            allImageData[fileIndex] = imageReaders[fileIndex].ReadBytes(imagesInFiles[fileIndex] * pixels);
            allLabelData[fileIndex] = labelReaders[fileIndex].ReadBytes(imagesInFiles[fileIndex]);
        }

        for(int i = 0; i < paths.Length; i++)
        {
            imageStreams[i].Dispose();
            imageReaders[i].Dispose();
            labelStreams[i].Dispose();
            labelReaders[i].Dispose();
        }

        int startIndex = 0;

        for(int fileIndex = 0; fileIndex < paths.Length; fileIndex++)
        {
            byte[] imageData = allImageData[fileIndex];
            byte[] labelData = allLabelData[fileIndex];

            System.Threading.Tasks.Parallel.For(0, imagesInFiles[fileIndex], i =>
            {
                double[] pixelData = new double[pixels];

                int startPixelIndex = i * pixels;

                for(int j = 0; j < pixelData.Length; j++)
                {
                    pixelData[j] = imageData[startPixelIndex + j] / 255.0;
                }

                int label = labelData[i];

                allData[startIndex + i] = new DataPoint(pixelData, width, label);
            });
            startIndex += imagesInFiles[fileIndex];
        }

        return allData;
    }
}