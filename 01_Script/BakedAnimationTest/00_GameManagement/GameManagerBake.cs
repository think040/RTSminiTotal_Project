using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using UnityEngine;
using UnityEngine.UI;

public class GameManagerBake : MonoBehaviour
{
    public int vSyncCount = 0;

    void Awake()
    {
        QualitySettings.vSyncCount = vSyncCount;

        fpsTimer = new Stopwatch();
        fpsTimer.Start();
    }


    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        ShowFPS();
    }

    static Stopwatch fpsTimer;
    static int fpsCounter;
    public Text fpsInfo;

    public void ShowFPS()
    {
        fpsCounter++;
        if (fpsTimer.ElapsedMilliseconds > 1000)
        {
            //float fps = (float)fpsCounter;
            float fps = (float)1000.0 * fpsCounter / fpsTimer.ElapsedMilliseconds;
            float timePerFrame = (float)fpsTimer.ElapsedMilliseconds / fpsCounter;
            fpsInfo.text = string.Format("(FPS : {0:F2}), (msPF : {1:F2} (ms))", fps, timePerFrame);

            fpsTimer.Stop();
            fpsTimer.Reset();
            fpsTimer.Start();
            fpsCounter = 0;
        }
    }
}
