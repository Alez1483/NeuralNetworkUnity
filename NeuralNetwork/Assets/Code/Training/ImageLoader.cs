using System.IO;
using UnityEngine;

public static class ImageLoader
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

            int imagesInFile = imagesInFiles[fileIndex];

            System.Threading.Tasks.Parallel.For(0, imagesInFile, i =>
            {
                double[] pixelData = new double[pixels];

                int startPixelIndex = i * pixels;

                //two loops to flip the image vertically
                for (int rowStartIndex = pixels - width; rowStartIndex >= 0; rowStartIndex -= width)
                {
                    int lastIndex = rowStartIndex + width;
                    for (int rowIndex = rowStartIndex; rowIndex < lastIndex; rowIndex++)
                    {
                        double value = imageData[startPixelIndex] / 255.0;
                        pixelData[rowIndex] = value;
                        startPixelIndex++;
                    }
                }

                int label = labelData[i];

                allData[startIndex + i] = new DataPoint(pixelData, width, label);
            });
            startIndex += imagesInFiles[fileIndex];
        }
        return allData;
    }
}