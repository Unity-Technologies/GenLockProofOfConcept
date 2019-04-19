using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpdateStatsText : MonoBehaviour
{
    [Tooltip("Number of frames to average together in frame rate statistics (1 corresponds to no averaging)")]
    [Range(1,999)]
    public int SampleWindow = 50;
    public Text Text;

    float lastRealtime = 0.0f;
    float lastTime = 0.0f;

    List<float> realtimeDeltas = new List<float>();
    int realtimeDeltaPos = 0;
    float realtimeDeltasSum = 0.0f;

    List<float> timeDeltas = new List<float>();
    int timeDeltaPos = 0;
    float timeDeltasSum = 0.0f;

    static float GetAverage(ref List<float> values, ref int valuePos, ref float sum, float newValue, int historySize)
    {
        if (historySize <= 0)
            historySize = 1;

        if (values.Count < historySize)
        {
            while (valuePos != 0)
            {
                values.Add(values[0]);
                values.RemoveAt(0);
                --valuePos;
            }
            values.Add(newValue);
            sum += newValue;
            return -1.0f;
        }

        while (values.Count > historySize)
        {
            sum -= values[valuePos];
            values.RemoveAt(valuePos);
            if (valuePos >= historySize)
                valuePos = 0;
        }

        sum = sum - values[valuePos] + newValue;
        values[valuePos] = newValue;
        valuePos = (valuePos + 1) % historySize;
        return sum / historySize;
    }

    void Update()
    {
        float realtime = Time.realtimeSinceStartup;
        float time = Time.time;

        if (lastRealtime > 0.0f && time > 0.0f)
        {
            float deltaRealtime = GetAverage(
                ref realtimeDeltas,
                ref realtimeDeltaPos,
                ref realtimeDeltasSum,
                realtime - lastRealtime,
                SampleWindow
            );

            float deltaTime = GetAverage(
                ref timeDeltas,
                ref timeDeltaPos,
                ref timeDeltasSum,
                time - lastTime,
                SampleWindow
            );

            if (deltaRealtime >= 0.0f && deltaTime >= 0.0f)
                Text.text =
                    "Render FPS: " + (1.0f/deltaRealtime).ToString("0.0") + " (" + (deltaRealtime*1000).ToString("0.0ms") + ")\n" +
                    "  Game FPS: " + (1.0f/deltaTime).ToString("0.0") + " (" + (deltaTime*1000).ToString("0.0ms") + ")";
        }

        lastRealtime = realtime;
        lastTime = time;
    }
}
