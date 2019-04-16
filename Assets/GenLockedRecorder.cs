using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor.Media;
#endif

public class GenLockedRecorder : MonoBehaviour
{
    [Tooltip("Camera who's view will be recorded")]
    public Camera Camera;
    [Tooltip("Desired camera rendering dimensions (in pixels)")]
    public int Width;
    public int Height;
    
    [Space(10)]
    
    [Tooltip("Desired rendering framerate relative to the system clock (simulates an external Genlock)")]
    public double GenlockRate = 24;
    [Tooltip("Desired game time framerate (ie. specifies how much game time should elapse between each frame render)")]
    public double GameTimeRate = 24;

    [Space(10)]
    
    [Tooltip("Record rendered frames to disk on exit")]
    public bool Record;
    [Tooltip("Path on disk where recorded frames should be stored")]
    public string MoviePath;
    [Tooltip("Skip recording of the first X frames (to avoid framerate instability during startup)")]
    public ulong SkipFirstFrames = 0;
    [Tooltip("When in Editor, record rendered frames into an MP4 video file")]
    public bool MakeMP4InEditor = false;

    private List<RenderTexture> m_Frames = new List<RenderTexture>();
    private ulong m_LastFakeGenLockCount = 0;
    private ulong m_SkipFirstFrames;
    private RenderTexture m_LastRender;
    private double m_CurrentGenlockRate = 0;

#if !UNITY_2019_2_OR_NEWER
    private double m_CaptureFrameRateError = 0;
#endif
    
    void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 9999;
        StartCoroutine("RecordFrame");
        m_SkipFirstFrames = SkipFirstFrames;
    }

    IEnumerator RecordFrame()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();
            m_LastRender = Camera.targetTexture;
            
            if (Camera.targetTexture == null || Camera.targetTexture.width != Width || Camera.targetTexture.height != Height)
                Camera.targetTexture = new RenderTexture(Width, Height, 0, RenderTextureFormat.ARGB32);
            
            if (m_SkipFirstFrames > 0)
            {
                --m_SkipFirstFrames;
            }
            else if (Record)
            {
                m_Frames.Add(m_LastRender);
                Camera.targetTexture = new RenderTexture(Width, Height, 0, RenderTextureFormat.ARGB32);
            }

            WaitForNextGenLock();

#if UNITY_2019_2_OR_NEWER
            // As of 2019.2, Time.captureDeltaTime has been introduced which opens up a floating point precision way of
            // setting captureFrameRate.
            Time.captureDeltaTime = 1.0f / (float)GameTimeRate;
#else
            // We use Time.captureFramerate to ensure Game time advances by exactly 1.0/Rate at every genlock tick.
            // Vary Time.captureFramerate from one frame to the next to achieve non-integer rates.
            m_CaptureFrameRateError += GameTimeRate - (int)GameTimeRate;
            if (m_CaptureFrameRateError >= 1.0)
            {
                m_CaptureFrameRateError -= 1.0;
                Time.captureFramerate = 1 + (int)GameTimeRate;
            }
            else 
                Time.captureFramerate = (int)GameTimeRate;
#endif
        }
    }

    void WaitForNextGenLock()
    {
        if (m_CurrentGenlockRate != GenlockRate)
        {
            if (GenlockRate < 1)
                GenlockRate = 1;
            m_LastFakeGenLockCount = 0;
            m_CurrentGenlockRate = GenlockRate;
        }

        double t = Time.realtimeSinceStartup;
        double nextGenLockTime = (m_LastFakeGenLockCount + 1) / GenlockRate;
        if (t > nextGenLockTime)
        {
            // This shouldn't normally happen.
            if (m_LastFakeGenLockCount != 0 && m_SkipFirstFrames == 0)
                Debug.LogWarning("Frame drop: Rendering too slow (at " + t.ToString("0.000")+ ")");
            m_LastFakeGenLockCount = (ulong)(t * GenlockRate + 0.5);
            return;
        }


        // This is the actual sleep/busy loop simulating waiting for the genlock signal.
        var sleepTime = nextGenLockTime - t - 0.01f; // conservative sleep
        if (sleepTime>0)
            Thread.Sleep((int)(sleepTime * 1000));
        do // busy-loop the remaining time for accuracy
        {
            t = Time.realtimeSinceStartup;
        } while (t < nextGenLockTime);
        ++m_LastFakeGenLockCount;

        if (t > nextGenLockTime + 0.1 / GenlockRate)
        {
            // This shouldn't normally happen.
            if (t > nextGenLockTime + 1.0 / GenlockRate)
            {
                m_LastFakeGenLockCount = (ulong) (t * GenlockRate + 0.5);
                if (m_SkipFirstFrames == 0)
                   Debug.LogWarning("Frame drop: Waiting for GenLock too slow (at " + t.ToString("0.000")+ ")");
                return;
            }

            if (m_SkipFirstFrames == 0)
                Debug.LogWarning(
                    "Waited " + ((t - nextGenLockTime) / (1.0 / GenlockRate) * 100.0).ToString("0") + "% past next GenLock (at " + t.ToString("0.000")+ ")");
        }
    }

    public RenderTexture GetLastRender()
    {
        return m_LastRender;
    }

    void OnApplicationQuit()
    {
        if (!Record)
            return;

        if (!Directory.Exists(MoviePath))
        {
            Debug.LogError(MoviePath + " does not exist. Cannot save output");
            return;
        }

        Debug.Log("Saving recorded frames to disk...");

        string filePathMP4 = "";
        string filePathDir = "";
        for (int i = 0; i < 9999; ++i)
        {
            filePathMP4 = Path.Combine(MoviePath, "capture_" + i.ToString("0000") + ".mp4");
            filePathDir = Path.Combine(MoviePath, "capture_" + i.ToString("0000"));
            if (!File.Exists(filePathMP4) && !Directory.Exists(filePathDir)) break;
        }

        var textures = new List<Texture2D>();
        foreach (var frame in m_Frames)
        {
            RenderTexture.active = frame;
            var tex = new Texture2D(Width, Height, TextureFormat.RGBA32, false);
            tex.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
            tex.Apply();
            textures.Add(tex);
        }
        RenderTexture.active = null;

#if UNITY_EDITOR
        if (MakeMP4InEditor)
        {
            VideoTrackAttributes videoAttr = new VideoTrackAttributes
            {
                frameRate = new MediaRational((int)(GenlockRate + 0.5)),
                width = (uint)Width,
                height = (uint)Height,
                includeAlpha = false,
                bitRateMode = UnityEditor.VideoBitrateMode.High
            };
            using (var encoder = new MediaEncoder(filePathMP4, videoAttr))
                foreach (var tex in textures)
                    encoder.AddFrame(tex);
            Debug.Log("Recorded " + m_Frames.Count + " frames to " + filePathMP4);
        }
        else
#else
        if (MakeMP4InEditor)
            Debug.Log("Cannot encode MP4 outside of Editor");
#endif
        {
            int f = 0;
            Directory.CreateDirectory(filePathDir);
            foreach (var tex in textures)
            {
                byte[] bytes = tex.EncodeToJPG();
                File.WriteAllBytes(Path.Combine(filePathDir, "frame_" + (f++).ToString("0000") + ".jpg"), bytes);
            }
            Debug.Log("Recorded " + m_Frames.Count + " frames to " + Path.Combine(filePathDir, "frame_XXXX.jpg"));
        }
    }
}
