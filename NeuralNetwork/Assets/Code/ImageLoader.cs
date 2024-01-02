using System.IO;

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
