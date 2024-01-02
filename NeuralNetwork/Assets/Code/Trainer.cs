using UnityEngine;
using System.IO;

public class Trainer : MonoBehaviour
{
    NeuralNetwork network;

    NetworkDataContainer networkTrainData;

    DataPoint[] allData;

    ICost cost;

    [Range(0.0f, 1.0f)] public double dataSplit = 0.85;
    [SerializeField] private double learnRate = 1;
    [SerializeField] private int batchSize;
    private int batchStart;

    int trainDataCount;
    DataPoint[] trainData;
    int testDataCount;
    DataPoint[] testData;

    double epochAtm = 0;
    System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();

    [SerializeField] Transform graphTransform;

    [SerializeField] double graphUpdateRate;
    double lastUpdate;

    void Start()
    {
        cost = new MeanSquaredError();
        network = new NeuralNetwork(784, 64, 10);
        networkTrainData = new NetworkDataContainer(network);
        
        allData = new DataPoint[70000];

        LoadData();

        (trainData, testData) =  DataSetHelper.SplitData(allData, dataSplit);
        trainDataCount = trainData.Length;
        testDataCount = testData.Length;

        networkTrainData = new NetworkDataContainer(network);

        batchStart = 0;

        double[] input = trainData[0].pixelData;

        graphTransform.GetComponent<TrailRenderer>().emitting = true;
        lastUpdate = Time.timeAsDouble;
    }

    void Update()
    {
        timer.Restart();

        while(timer.ElapsedMilliseconds < 16)
        {
            epochAtm += (batchSize / (double)trainData.Length);
            batchStart = (batchStart + batchSize) % trainData.Length;
            network.LearnBatch(trainData, batchStart, batchSize, learnRate, networkTrainData, cost);
        }

        timer.Stop();

        if (Time.timeAsDouble - lastUpdate > graphUpdateRate)
        {
            lastUpdate += graphUpdateRate;
            DebugCost();
        }
    }

    void DebugCost()
    {
        double cos = 0;
        int right = 0;
        for (int i = 0; i < testData.Length; i++)
        {
            double[] outp = network.Evaluate(testData[i].pixelData);
            if (network.MaxIndex(outp) == testData[i].label)
            {
                right++;
            }
            cos += cost.Cost(outp, testData[i].expectedOutput);
        }
        graphTransform.localPosition = new Vector2((float)epochAtm, right / (float)testData.Length);
        //print("Cost atm is: " + (cos / testData.Length) + " and correctness: " + (right / (double)testData.Length));
    }

    void LoadData()
    {
        ImageLoader imageLoader = new ImageLoader();

        imageLoader.ImagePath = Path.Combine("Assets", "Code", "Data", "TrainImages.idx");
        imageLoader.LabelPath = Path.Combine("Assets", "Code", "Data", "TrainLabels.idx");
        imageLoader.InitializeReaders();

        int trainImageCount = imageLoader.dataPoints;

        if (trainImageCount > allData.Length)
        {
            Debug.LogError("Image array too small");
            return;
        }

        for (int i = 0; i < imageLoader.dataPoints; i++)
        {
            allData[i] = imageLoader.LoadImage();
        }

        imageLoader.ImagePath = Path.Combine("Assets", "Code", "Data", "TestImages.idx");
        imageLoader.LabelPath = Path.Combine("Assets", "Code", "Data", "TestLabels.idx");
        imageLoader.InitializeReaders();

        if (trainImageCount + imageLoader.dataPoints > allData.Length)
        {
            Debug.LogError("Image array too small");
            return;
        }

        for (int i = 0, j = trainImageCount; i < imageLoader.dataPoints; i++, j++)
        {
            allData[j] = imageLoader.LoadImage();
        }
    }
}
