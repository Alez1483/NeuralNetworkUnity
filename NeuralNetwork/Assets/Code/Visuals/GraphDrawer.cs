using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UIElements;

[System.Serializable]
public class GraphDrawer
{
    DataPoint[] testData;
    DataPoint[] trainData;
    int testDataCount;
    int testDataStart = 0;
    int trainDataStart = 0;
    NeuralNetwork network;

    Transform graphTransform;
    Transform trainGraphTransform;
    Transform cameraTransform;
    GameObject numberPrefab;
    Camera camera;

    float camWorldWidth;
    int screenWidth;
    int screenHeight;

    Queue<NumberText> numberTexts = new Queue<NumberText>();

    float camMomentumReductionRate;
    float camPosBorder = 0.0f;
    float camXPos = 0.0f;
    float lastFrameMouseXPos;
    float camXSpeed = 0.0f;
    bool followingGraph = true;

    bool testAgainstTrainData;

    public GraphDrawer(DataPoint[] ted, DataPoint[] trd, NeuralNetwork n, Transform gt, Transform g2t, Transform ct, GameObject np, Camera c, float cMRR, bool tATD)
    {
        testData = ted;
        testDataCount = ted.Length;
        trainData = trd;
        network = n;
        graphTransform = gt;
        trainGraphTransform = g2t;
        cameraTransform = ct;
        numberPrefab = np;
        camera = c;
        camMomentumReductionRate = cMRR;
        testAgainstTrainData = tATD;

        screenWidth = 0;
        screenHeight = 0;
    }

    public void RunTest(int testSamples, double epochAtm, bool randomize)
    {
        if (!testAgainstTrainData)
        {
            if (randomize)
            {
                testDataStart = Random.Range(0, testDataCount);
            }
            else
            {
                testDataStart = (testDataStart + testSamples) % testDataCount;
            }
            
        }
        else
        {
            if (randomize)
            {
                testDataStart = Random.Range(0, testDataCount);
                trainDataStart = Random.Range(0, testData.Length);
            }
            else
            {
                testDataStart = (testDataStart + testSamples) % testDataCount;
                trainDataStart = (trainDataStart + testSamples) % trainData.Length;
            }
            trainGraphTransform.localPosition = new Vector3((float)epochAtm, EvaluatePerformance(testSamples, trainDataStart, trainData));
        }
        graphTransform.localPosition = new Vector3((float)epochAtm, EvaluatePerformance(testSamples, testDataStart, testData));
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

        //scrolling mechanism

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

        //chamges the amount of numbers when needed

        if (screenSizeChanged)
        {
            ScreenSizeChanged();
        }

        //number recycling

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
        //Debug.Log(textWidth);
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
