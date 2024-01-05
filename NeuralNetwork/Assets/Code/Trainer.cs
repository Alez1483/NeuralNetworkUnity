using UnityEngine;
using System.IO;

public class Trainer : MonoBehaviour
{
    NeuralNetwork network;

    NetworkDataContainer networkTrainData;
    WeightedInputDerivativeContainer[] weightedInputDerivatives;

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

    GraphDrawer graphDrawer;

    [Header("Graph Drawer")]
    [SerializeField] private int testSize = 100;
    [SerializeField] Transform graphTransform;
    [SerializeField] bool testAgainstTrainData;
    [SerializeField] Transform trainGraphTransform;
    [SerializeField] Transform cameraTransform;
    [SerializeField] GameObject numberPrefab;
    [SerializeField] Camera mainCamera;
    [SerializeField] float graphMomentumReductionRate;

    [SerializeField] double graphUpdateRate;
    double lastUpdate;

    void Start()
    {
        //initialize
        cost = new MeanSquaredError();
        network = new NeuralNetwork(784, 64, 10);
        networkTrainData = new NetworkDataContainer(network);
        weightedInputDerivatives = new WeightedInputDerivativeContainer[batchSize];
        for(int i = 0; i < weightedInputDerivatives.Length; i++)
        {
            weightedInputDerivatives[i] = new WeightedInputDerivativeContainer(network);
        }

        allData = new DataPoint[70000];
        
        //load data
        LoadData();
        
        //split data
        (trainData, testData) =  DataSetHelper.SplitData(allData, dataSplit);
        trainDataCount = trainData.Length;
        testDataCount = testData.Length;

        graphDrawer = new GraphDrawer(testData, trainData, network, graphTransform, trainGraphTransform, cameraTransform, numberPrefab, mainCamera, graphMomentumReductionRate, testAgainstTrainData);

        batchStart = 0;

        graphTransform.GetComponent<TrailRenderer>().emitting = true;
        if (testAgainstTrainData)
        {
            trainGraphTransform.GetComponent<TrailRenderer>().emitting = true;
        }
        lastUpdate = Time.timeAsDouble;
    }

    void Update()
    {
        timer.Restart();

        while(timer.ElapsedMilliseconds < 16)
        {
            epochAtm += (batchSize / (double)trainDataCount);
            batchStart = (batchStart + batchSize) % trainData.Length;
            network.LearnBatch(trainData, batchStart, batchSize, learnRate, networkTrainData, weightedInputDerivatives, cost);
        }

        timer.Stop();

        if (Time.timeAsDouble - lastUpdate > graphUpdateRate)
        {
            lastUpdate += graphUpdateRate;
            graphDrawer.RunTest(testSize, epochAtm, true);
        }

        graphDrawer.Update(epochAtm);
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
