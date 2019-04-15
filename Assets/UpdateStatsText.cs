using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpdateStatsText : MonoBehaviour
{
    [Tooltip("Apply decaying averaging to framerate statistics (0 corresponds to no averaging)")]
    [Range(0,0.999f)]
    public float Smooth = 0.0f;
	public Text Text;
//    public GenLockRecorder Genlock;
    
    float lastRealtime = 0.0f;
    float lastTime = 0.0f;

    float elapsedRealtime = 0.0f;
    float elapsedTime = 0.0f;

    void Update()
    {
        float realtime = Time.realtimeSinceStartup;
        float time = Time.time;
        
        float deltaRealtime = realtime - lastRealtime;
        float deltaTime = time - lastTime;

        if (elapsedRealtime > 1.2 * deltaRealtime ||
            elapsedRealtime < 0.8 * deltaRealtime ||
            elapsedTime > 1.2 * deltaTime ||
            elapsedTime < 0.8 * deltaTime)
        {
            elapsedRealtime = deltaRealtime;
            elapsedTime = deltaTime;
        }
        else
        {
            elapsedRealtime = Smooth * elapsedRealtime + (1.0f - Smooth) * deltaRealtime;
            elapsedTime = Smooth * elapsedTime + (1.0f - Smooth) * deltaTime;
        }
        if (lastRealtime > 0.0f && time > 0.0f)
        {
            Text.text =
                "Render FPS: " + (1.0f/elapsedRealtime).ToString("0.0") + " (" + (elapsedRealtime*1000).ToString("0.0ms") + ")\n" +
                "  Game FPS: " + (1.0f/elapsedTime).ToString("0.0") + " (" + (elapsedTime*1000).ToString("0.0ms") + ")";
        }
        lastRealtime = realtime;
        lastTime = time;
    }
}
