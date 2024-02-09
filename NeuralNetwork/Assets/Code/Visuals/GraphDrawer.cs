using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.Globalization;

[System.Serializable]
public class GraphDrawer
{
    DataPoint[] testData;
    DataPoint[] trainData;
    int testDataStart = 0;
    int trainDataStart = 0;
    NeuralNetwork network;
    
    NumberFormatInfo percentFormat = new NumberFormatInfo { PercentPositivePattern = 1 };

    //serialized into the unity editor of Trainer.cs
    [Header("Note the train data means the non randomized one")]
    public bool testAgainstTrainData;
    public TextMeshPro testPercentText;
    public TextMeshPro trainPercentText;
    
    public TrailRenderer testTrail;
    public TrailRenderer trainTrail;

    public Transform cameraTransform;
    public GameObject numberPrefab;
    public Camera camera;

    public float camMomentumReductionRate;

    float camWorldWidth;
    int screenWidth = 0;
    int screenHeight = 0;

    Queue<NumberText> numberTexts = new Queue<NumberText>();

    float camPosBorder = 0.0f;
    float camXPos = 0.0f;
    float lastFrameMouseXPos;
    float camXSpeed = 0.0f;
    bool followingGraph = true;

    public void Initialize(DataPoint[] testData, DataPoint[] trainData, NeuralNetwork network)
    {
        this.testData = testData;
        this.trainData = trainData;
        this.network = network;

        testPercentText.color = testTrail.colorGradient.Evaluate(0.5f);

        testTrail.emitting = true;
        testPercentText.text = trainPercentText.text = 0.ToString("P1", percentFormat);
        if (testAgainstTrainData)
        {
            trainPercentText.gameObject.SetActive(true);
            trainPercentText.color = trainTrail.colorGradient.Evaluate(0.5f);
            trainTrail.emitting = true;
        }
        else
        {
            trainPercentText.gameObject.SetActive(false);
            trainTrail.emitting = false;
        }
    }

    public void RunTest(int testSamples, double epochAtm, bool randomize)
    {
        if (!testAgainstTrainData)
        {
            if (randomize)
            {
                testDataStart = Random.Range(0, testData.Length);
            }
            else
            {
                testDataStart = (testDataStart + testSamples) % testData.Length;
            }
            
        }
        else
        {
            if (randomize)
            {
                testDataStart = Random.Range(0, testData.Length);
                trainDataStart = Random.Range(0, testData.Length);
            }
            else
            {
                testDataStart = (testDataStart + testSamples) % testData.Length;
                trainDataStart = (trainDataStart + testSamples) % trainData.Length;
            }
            trainTrail.transform.localPosition = new Vector3((float)epochAtm, EvaluatePerformance(testSamples, trainDataStart, trainData));
        }
        testTrail.transform.localPosition = new Vector3((float)epochAtm, EvaluatePerformance(testSamples, testDataStart, testData));
    }

    readonly object threadLock = new object();
    float EvaluatePerformance(int testSamples, int startIndex, DataPoint[] data)
    {
        int right = 0;
        System.Threading.Tasks.Parallel.For(0, testSamples, i =>
        {
            DataPoint point = data[(startIndex + i) % data.Length];
            double[] outp = network.Evaluate(point.pixelData);

            lock (threadLock)
            {
                if (network.MaxIndex(outp) == point.label)
                {
                    right++;
                }
            }
        });
        return right / (float)testSamples;
    }

    public void Update(double epochAtm)
    {
        bool screenSizeChanged = false;
        if (Screen.width != screenWidth || Screen.height != screenHeight)
        {
            screenSizeChanged = true;
            screenWidth = Screen.width;
            screenHeight = Screen.height;
            camWorldWidth = (camera.orthographicSize * 2f) * ((float)screenWidth / screenHeight);
        }

        UpdateCameraScroll(epochAtm);

        UpdatetPercentageTests();

        //changes the amount of numbers when needed

        if (screenSizeChanged)
        {
            ScreenSizeChanged();
        }

        RecycleNumberTexts();
    }

    void UpdatetPercentageTests()
    {
        int trailPointCount = testTrail.positionCount;

        if (trailPointCount == 0)
        {
            return;
        }

        //binary searching the left and rights points of the trail data for interpolation
        //could be optimized a lot by predicting the new middle point

        int left = 0;
        int right = trailPointCount - 1;

        int middle = (left + right) / 2;

        while (left != middle)
        {
            if (camXPos > testTrail.GetPosition(middle).x)
            {
                left = middle;
            }
            else
            {
                right = middle;
            }
            middle = (left + right) / 2;
        }

        Vector2 leftPos = testTrail.GetPosition(left);
        Vector2 rightPos = testTrail.GetPosition(right);

        //interpolation and clamping (inbuilt on Lerp)
        float percentage = Mathf.Lerp(leftPos.y, rightPos.y, Mathf.InverseLerp(leftPos.x, rightPos.x, camXPos));
        testPercentText.text = percentage.ToString("P1", percentFormat);

        if (testAgainstTrainData)
        {
            Vector2 trainLeftPos = trainTrail.GetPosition(left);
            Vector2 trainRightPos = trainTrail.GetPosition(right);

            float trainPercentage = Mathf.Lerp(trainLeftPos.y, trainRightPos.y, Mathf.InverseLerp(trainLeftPos.x, trainRightPos.x, camXPos));
            trainPercentText.text = trainPercentage.ToString("P1", percentFormat);
        }
    }

    //scrolling mechanism
    void UpdateCameraScroll(double epochAtm)
    {
        camPosBorder = (float)epochAtm;

        bool mouseDown = Input.GetKeyDown(KeyCode.Mouse0);
        bool mouse = Input.GetKey(KeyCode.Mouse0);

        if (mouse || mouseDown)
        {
            float mouseXPos = Input.mousePosition.x;
            followingGraph = false;

            if (mouseDown)
            {
                lastFrameMouseXPos = mouseXPos;
            }
            if (mouse)
            {
                float mouseDelta = mouseXPos - lastFrameMouseXPos;
                float mouseWDelta = mouseDelta / screenWidth * camWorldWidth;

                camXSpeed = mouseWDelta / Time.deltaTime;

                camXPos -= mouseWDelta;

                lastFrameMouseXPos = mouseXPos;
            }
        }
        else
        {
            if (camXSpeed > 0f)
            {
                camXSpeed -= Time.deltaTime * camMomentumReductionRate;
                camXSpeed = Mathf.Max(camXSpeed, 0f);
            }
            else
            {
                camXSpeed += Time.deltaTime * camMomentumReductionRate;
                camXSpeed = Mathf.Min(camXSpeed, 0f);
            }
            camXPos -= camXSpeed * Time.deltaTime;
        }

        camXPos = Mathf.Max(camXPos, 0.0f);

        if (camXPos >= camPosBorder)
        {
            followingGraph = true;
        }

        if (followingGraph)
        {
            camXPos = camPosBorder;
        }

        cameraTransform.position = new Vector3(camXPos, 0.5f, 0f);
    }

    void RecycleNumberTexts()
    {
        NumberText lastNumber = numberTexts.Peek();
        float camHalfWidth = camWorldWidth / 2f;
        float camLeftPos = cameraTransform.position.x - camHalfWidth;
        float halfTextWidth = lastNumber.tmp.rectTransform.sizeDelta.x / 2f;
        float lastVisiblePos = camLeftPos - halfTextWidth;

        //cycle forwards
        while (lastNumber.transform.position.x < lastVisiblePos)
        {
            int xPosition = lastNumber.xPosition + numberTexts.Count;
            numberTexts.Dequeue();
            numberTexts.Enqueue(lastNumber);
            lastNumber.xPosition = xPosition;
            lastNumber.tmp.text = xPosition.ToString();
            lastNumber = numberTexts.Peek();
        }
        //cycle backwards (scrolled backwards)
        if (lastNumber.xPosition - 1 + halfTextWidth > camLeftPos && lastNumber.xPosition != 0)
        {
            int xPosition = Mathf.Max(Mathf.CeilToInt(camLeftPos - halfTextWidth), 0);
            for (int i = 0; i < numberTexts.Count; i++)
            {
                var text = numberTexts.Dequeue();
                numberTexts.Enqueue(text);
                text.xPosition = xPosition;
                text.tmp.text = xPosition.ToString();
                xPosition++;
            }
        }
    }

    void ScreenSizeChanged()
    {
        //makes sure there's correct amount of numbers available to fit in the screen

        float textWidth = ((RectTransform)numberPrefab.transform).sizeDelta.x;
        int maxNumbersInScreen = Mathf.CeilToInt(camWorldWidth + textWidth);

        if (numberTexts.Count != maxNumbersInScreen)
        {
            float camLeftPos = cameraTransform.position.x - camWorldWidth / 2f;
            int xPosition = Mathf.Max(Mathf.CeilToInt(camLeftPos - textWidth / 2f), 0);

            if (numberTexts.Count > maxNumbersInScreen)
            {
                for (int i = 0; i < maxNumbersInScreen; i++)
                {
                    var text = numberTexts.Dequeue();
                    numberTexts.Enqueue(text);
                    text.xPosition = xPosition;
                    text.tmp.text = xPosition.ToString();
                    xPosition++;
                }
                for (int i = 0; i < numberTexts.Count - maxNumbersInScreen; i++)
                {
                    numberTexts.Dequeue().Destroy();
                }
                return;
            }
            //too few number texts
            for (int i = 0; i < numberTexts.Count; i++)
            {
                var text = numberTexts.Dequeue();
                numberTexts.Enqueue(text);
                text.xPosition = xPosition;
                text.tmp.text = xPosition.ToString();
                xPosition++;
            }
            int iterations = maxNumbersInScreen - numberTexts.Count;
            for (int i = 0; i < iterations; i++)
            {
                var text = new NumberText(numberPrefab, xPosition);
                numberTexts.Enqueue(text);
                text.tmp.text = xPosition.ToString();
                xPosition++;
            }
        }
    }
}

public class NumberText
{
    public Transform transform;
    public TextMeshPro tmp;

    //initial position is copied from the prefab given on initialization
    Vector3 pos;
    int _xPos;
    public int xPosition
    {
        get
        {
            return _xPos;
        }
        set
        {
            _xPos = value;
            pos.x = _xPos;
            transform.position = pos;
        }
    }

    public NumberText(GameObject textPrefab)
    {
        var obj = GameObject.Instantiate(textPrefab);
        transform = obj.transform;
        pos = transform.position;
        tmp = obj.GetComponent<TextMeshPro>();
    }

    public NumberText(GameObject textPrefab, int xPos)
    {
        var obj = GameObject.Instantiate(textPrefab);
        transform = obj.transform;
        pos = transform.position;
        xPosition = xPos;
        tmp = obj.GetComponent<TextMeshPro>();
    }
    public void Destroy()
    {
        GameObject.Destroy(transform.gameObject);
    }
}
