using UnityEngine;
using TMPro;
using System.IO;

public class Trainer : MonoBehaviour
{
    public static Trainer Instance;

    [HideInInspector] public NeuralNetwork network;

    NetworkDataContainer[] networkTrainData;

    [HideInInspector] public DataPoint[] allData;

    ICost cost;

    [Range(0.0f, 1.0f)] public double dataSplit = 0.85;
    [SerializeField] private int[] networkSize;
    [SerializeField] private double learnRate = 1;
    [SerializeField] private double momentum;
    [SerializeField] private int batchSize;
    private int batchStart;

    int trainDataCount;
    [HideInInspector] public DataPoint[] trainData;
    int testDataCount;
    [HideInInspector] public DataPoint[] testData;

    double epochAtm = 0;
    System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();

    [Header("Graph Drawer")]
    [SerializeField] private int testSize = 100;
    [SerializeField] GraphDrawer graphDrawer;

    [SerializeField] double graphUpdateRate;
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
        
        //split data
        (trainData, testData) =  DataSetHelper.SplitData(allData, dataSplit);
        trainDataCount = trainData.Length;
        testDataCount = testData.Length;

        graphDrawer.Initialize(testData, trainData, network);

        batchStart = 0;
    }

    void Update()
    {
        timer.Restart();

        while(timer.ElapsedMilliseconds < 16)
        {
            epochAtm += (batchSize / (double)trainDataCount);
            batchStart += batchSize;
            if (batchStart >= trainData.Length)
            {
                DataSetHelper.SuffleDataSet(trainData);
                batchStart = 0;
            }
            network.LearnBatch(trainData, batchStart, batchSize, learnRate, momentum, networkTrainData, cost);
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
        string trainImagePath = Path.Combine("Assets", "Code", "Data", "TrainImages.idx");
        string trainLabelPath = Path.Combine("Assets", "Code", "Data", "TrainLabels.idx");
        string testImagePath = Path.Combine("Assets", "Code", "Data", "TestImages.idx");
        string testLabelPath = Path.Combine("Assets", "Code", "Data", "TestLabels.idx");

        allData = ImageLoader.LoadImages((trainImagePath, trainLabelPath), (testImagePath, testLabelPath));
    }
}
