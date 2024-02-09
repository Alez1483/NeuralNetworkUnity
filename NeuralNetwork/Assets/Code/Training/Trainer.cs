using UnityEngine;
using TMPro;
using System.IO;

public class Trainer : MonoBehaviour
{
    public static Trainer Instance;

    [HideInInspector] public NeuralNetwork network;

    NetworkDataContainer[] networkTrainData;

    ICost cost;

    [Range(0.0f, 1.0f)] public double dataSplit = 0.85;
    [SerializeField] private int[] networkSize;
    [SerializeField, Range(0f, 1f)] private double learnRate = 1;
    [SerializeField, Range(0f, 1f)] private double momentum;
    [SerializeField] private int batchSize;
    private int batchStart;

    [HideInInspector] public DataPoint[] trainData;
    [HideInInspector] public DataPoint[] originalTrainData;
    [HideInInspector] public DataPoint[] testData;

    [Header("Image Randomization")]
    [SerializeField] bool randomizeImages;
    [SerializeField, Range(0, 25)] int randomizeAfterEveryXEpochs;
    int epochIndex = 0;
    [SerializeField] ImageRandomizer imageRandomizer;

    double epochAtm = 0;
    System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();

    [Header("Graph Drawer")]
    [SerializeField] private int testSize = 100;
    [SerializeField] GraphDrawer graphDrawer;

    [SerializeField, Range(0f, 10f)] double graphUpdateRate;
    double lastUpdate;

    void OnEnable()
    {
        lastUpdate = Time.timeAsDouble;
    }

    void Awake()
    {
        Instance = this;
        //initialize
        var hiddenAct = new ReLu();
        cost = new CrossEntropy();
        var outputAct = new Softmax();
        network = new NeuralNetwork(hiddenAct, outputAct, networkSize);
        networkTrainData = new NetworkDataContainer[batchSize];
        for(int i = 0; i <  networkTrainData.Length; i++)
        {
            networkTrainData[i] = new NetworkDataContainer(network);
        }
        
        //load data
        LoadData();

        graphDrawer.Initialize(testData, graphDrawer.testAgainstTrainData? originalTrainData : null, network);

        batchStart = 0;
    }

    void Update()
    {
        timer.Restart();

        do
        {
            network.LearnBatch(trainData, batchStart, batchSize, learnRate, momentum, networkTrainData, cost);

            batchStart += batchSize;
            epochAtm += (batchSize / (double)trainData.Length);

            if (batchStart >= trainData.Length)
            {
                if (randomizeImages)
                {
                    DataSetHelper.SuffleTwoSetsAtSync(trainData, originalTrainData);
                }
                else
                {
                    DataSetHelper.SuffleDataSet(trainData);
                }

                epochIndex++;

                if (randomizeImages && randomizeAfterEveryXEpochs > 0 && epochIndex % randomizeAfterEveryXEpochs == 0)
                {
                    imageRandomizer.RandomizeImages(originalTrainData, trainData);
                }

                batchStart = 0;
            }
        } while (timer.ElapsedMilliseconds < 16);

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
        string trainImagePath = Path.Combine("Assets", "Code", "Data", "TrainImages.idx");
        string trainLabelPath = Path.Combine("Assets", "Code", "Data", "TrainLabels.idx");
        string testImagePath = Path.Combine("Assets", "Code", "Data", "TestImages.idx");
        string testLabelPath = Path.Combine("Assets", "Code", "Data", "TestLabels.idx");

        DataPoint[] allData = ImageLoader.LoadImages((trainImagePath, trainLabelPath), (testImagePath, testLabelPath));

        DataSetHelper.SuffleDataSet(allData);

        (trainData, testData) = DataSetHelper.SplitData(allData, dataSplit);

        if (randomizeImages)
        {
            originalTrainData = trainData;

            trainData = new DataPoint[originalTrainData.Length];

            int pixels = originalTrainData[0].pixelCount;
            int width = originalTrainData[0].imageWidth;

            for (int i = 0; i < trainData.Length; i++)
            {
                trainData[i] = new DataPoint(new double[pixels], width, originalTrainData[i].label);
            }

            imageRandomizer.RandomizeImages(originalTrainData, trainData);
            imageRandomizer.RandomizeImages(testData);
        }
    }
}
