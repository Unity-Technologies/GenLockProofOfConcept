using UnityEngine;
using UnityEngine.UI;

public class TextFromTimeRealtime : MonoBehaviour
{
    private float m_LastTime = 0;
    void Update()
    {
        float t = Time.realtimeSinceStartup;
        GetComponent<Text>().text =
            "Time.realtimeSinceStartup=" + t.ToString("0.000s") + " (delta=" + ((t - m_LastTime)*1000).ToString("0.00ms") + ")";
        m_LastTime = t;
    }
}
