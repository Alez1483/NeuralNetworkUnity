using UnityEngine;
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
        //allData = new DataPoint[70000];
        
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
    }

    void Update()
    {
        timer.Restart();

        while(timer.ElapsedMilliseconds < 16)
        {
            epochAtm += (batchSize / (double)trainDataCount);
            batchStart = (batchStart + batchSize) % trainData.Length;
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
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        sw.Start();

        string trainImagePath = Path.Combine("Assets", "Code", "Data", "TrainImages.idx");
        string trainLabelPath = Path.Combine("Assets", "Code", "Data", "TrainLabels.idx");
        string testImagePath = Path.Combine("Assets", "Code", "Data", "TestImages.idx");
        string testLabelPath = Path.Combine("Assets", "Code", "Data", "TestLabels.idx");

        allData = ImageLoader2.LoadImages((trainImagePath, trainLabelPath), (testImagePath, testLabelPath));

        //ImageLoader imageLoader = new ImageLoader();

        //imageLoader.ImagePath = Path.Combine("Assets", "Code", "Data", "TrainImages.idx");
        //imageLoader.LabelPath = Path.Combine("Assets", "Code", "Data", "TrainLabels.idx");
        //imageLoader.InitializeReaders();

        //int trainImageCount = imageLoader.dataPoints;

        //if (trainImageCount > allData.Length)
        //{
        //    Debug.LogError("Image array too small");
        //    return;
        //}

        //for (int i = 0; i < imageLoader.dataPoints; i++)
        //{
        //    allData[i] = imageLoader.LoadImage();
        //}

        //imageLoader.ImagePath = Path.Combine("Assets", "Code", "Data", "TestImages.idx");
        //imageLoader.LabelPath = Path.Combine("Assets", "Code", "Data", "TestLabels.idx");
        //imageLoader.InitializeReaders();

        //if (trainImageCount + imageLoader.dataPoints > allData.Length)
        //{
        //    Debug.LogError("Image array too small");
        //    return;
        //}

        //for (int i = 0, j = trainImageCount; i < imageLoader.dataPoints; i++, j++)
        //{
        //    allData[j] = imageLoader.LoadImage();
        //}
        sw.Stop();
        print($"optimize heck out of this {sw.Elapsed.TotalMilliseconds} + remember to randomize train data after epochs");
    }
}
