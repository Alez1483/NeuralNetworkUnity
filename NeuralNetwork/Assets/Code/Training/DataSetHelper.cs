using UnityEngine;

public static class DataSetHelper
{
    public static (DataPoint[] trainData, DataPoint[] testData) SplitData(DataPoint[] allData, double split)
    {
        SuffleDataSet(allData);

        int trainCount = (int)(allData.Length * split);
        int testCount = allData.Length - trainCount;

        DataPoint[] trainData = new DataPoint[trainCount];
        DataPoint[] testData = new DataPoint[testCount];

        int i;
        for(i = 0; i < trainCount; i++)
        {
            trainData[i] = allData[i];
        }
        for(int j = 0; j < testCount; j++, i++)
        {
            testData[j] = allData[i];
        }
        return (trainData, testData);
    }

    public static void SuffleDataSet(DataPoint[] data)
    {
        for(int i = 0; i < data.Length - 1; i++)
        {
            int randIndex = Random.Range(i, data.Length);
            DataPoint temp = data[randIndex];
            data[randIndex] = data[i];
            data[i] = temp;
        }
    }

    public static void SuffleTwoSetsAtSync(DataPoint[] data1, DataPoint[] data2)
    {
        for (int i = 0; i < data1.Length - 1; i++)
        {
            int randIndex = Random.Range(i, data1.Length);

            DataPoint temp1 = data1[randIndex];
            DataPoint temp2 = data2[randIndex];

            data1[randIndex] = data1[i];
            data2[randIndex] = data2[i];

            data1[i] = temp1;
            data2[i] = temp2;
        }
    }
}
