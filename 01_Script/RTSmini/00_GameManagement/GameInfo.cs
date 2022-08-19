using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using Debug = UnityEngine.Debug;

public class GameInfo : MonoBehaviour
{
    public Text fpsInfo;
    public Text killInfo;
    public Text gameTimeInfo;


    void Awake()
    {
        gameTimer = new Stopwatch();

        {
            fpsTimer = new Stopwatch();
            fpsTimer.Start();

            if (GameManager.useEditor)
            {
                GameTimer.inMin = inMin;
                GameTimer.inSec = inSec;
            }

            //GameTimer.inMin = inMin;
            //GameTimer.inSec = inSec;

            GameTimer.Reset();
            GameTimer.Restart();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        //{
        //    fpsTimer = new Stopwatch();
        //    fpsTimer.Start();
        //
        //    if (GameManager.useEditor)
        //    {
        //        GameTimer.inMin = inMin;
        //        GameTimer.inSec = inSec;
        //    }
        //
        //    //GameTimer.inMin = inMin;
        //    //GameTimer.inSec = inSec;
        //
        //    GameTimer.Reset();
        //    GameTimer.Restart();
        //}        
    }

    void Update()
    {
        ShowFPS();
        ShowKillInfo();
        ShowGameTimer();
    }

    static Stopwatch fpsTimer;
    static int fpsCounter;

    public void ShowFPS()
    {
        if(GameManager.fpsShow)
        {
            fpsInfo.enabled = true;
        }
        else
        {
            fpsInfo.enabled = false;
        }

        fpsCounter++;
        if (fpsTimer.ElapsedMilliseconds > 1000)
        {
            //float fps = (float)fpsCounter;
            float fps = (float)1000.0 * fpsCounter / fpsTimer.ElapsedMilliseconds;
            float timePerFrame = (float)fpsTimer.ElapsedMilliseconds / fpsCounter;
            //fpsInfo.text = string.Format("(FPS : {0:F2}), (msPF : {1:F2} (ms))", fps, timePerFrame);
            fpsInfo.text = string.Format("{0, 3:D0}", (int)fps);

            fpsTimer.Stop();
            fpsTimer.Reset();
            fpsTimer.Start();
            fpsCounter = 0;
        }
    }

    public void ShowKillInfo0()
    {
        //killInfo.text = string.Format("(player0_Kill : {0}), (player1_Kill : {1})", UnitData.killNum[1], UnitData.killNum[0]);
        killInfo.text = string.Format("(Blue: {0, 2:D2}), (Red : {1, 2:D2})", GameManager.killedCount[1], GameManager.killedCount[0]);
    }

    public RectTransform blueBar;
    public RectTransform redBar;
    public Text blueCount;
    public Text redCount;

    public void ShowKillInfo()
    {
        float blueKill = (float)GameManager.killedCount[1];
        float redKill = (float)GameManager.killedCount[0];
        float sum = blueKill + redKill;

        if(sum > 0)
        {
            float blueRatio = blueKill / sum;
            float redRatio = redKill / sum;

            blueBar.anchorMin = new Vector2(0.0f, 0.0f);
            blueBar.anchorMax = new Vector2(blueRatio, 1.0f);

            redBar.anchorMin = new Vector2(1.0f - redRatio, 0.0f);
            redBar.anchorMax = new Vector2(1.0f, 1.0f);           
        }

        blueCount.text = string.Format("{0, 2:D2}", GameManager.killedCount[1]);
        redCount.text = string.Format("{0, 2:D2}", GameManager.killedCount[0]);

        //killInfo.text = string.Format("(player0_Kill : {0}), (player1_Kill : {1})", UnitData.killNum[1], UnitData.killNum[0]);
        //killInfo.text = string.Format("(Blue: {0, 2:D2}), (Red : {1, 2:D2})", GameManager.killedCount[1], GameManager.killedCount[0]);

    }


    public float eTime;
    public int sec;
    public int min;

    public int inSec = 30;
    public int inMin = 3;
    public int toSec = 0;
    public int cdSec = 0;
    public int cdMin = 0;

    void ShowGameTimer()
    {
        //if (Input.GetKeyDown(KeyCode.U))
        //{
        //    GameTimer.Reset();
        //    GameTimer.inSec = inSec;
        //    GameTimer.inMin = inMin;
        //}
        //
        //if (Input.GetKeyDown(KeyCode.I))
        //{
        //    GameTimer.Restart();
        //}
        //
        //if (Input.GetKeyDown(KeyCode.O))
        //{
        //    GameTimer.Stop();
        //}
        //
        //if (Input.GetKeyDown(KeyCode.P))
        //{
        //    GameTimer.Start();
        //}

        GameTimer.Run();
        eTime = GameTimer.eTime;
        sec = GameTimer.sec;
        min = GameTimer.min;

        toSec = GameTimer.toSec;
        cdMin = GameTimer.cdMin;
        cdSec = GameTimer.cdSec;

        if (GameTimer.toSec == 0)
        {
            GameTimer.Stop();
        }

        //gameTimeInfo.text = string.Format("RemainTime : {0}(m) : {1}(s)", GameTimer.cdMin, GameTimer.cdSec);
        gameTimeInfo.text = string.Format("{0, 2:D2} : {1, 2:D2}", GameTimer.cdMin, GameTimer.cdSec);
    }




    //Test    
    static Stopwatch gameTimer;
    public void TestStopWatch()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            gameTimer.Reset();
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            gameTimer.Restart();
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            gameTimer.Stop();
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            gameTimer.Start();
        }

        eTime = (float)(gameTimer.ElapsedMilliseconds) / 1000.0f;
        sec = (int)eTime;
        min = (int)sec / 60;

        toSec = inMin * 60 + inSec - sec;
        cdMin = toSec / 60;
        cdSec = toSec % 60;

        if (toSec == 0)
        {
            gameTimer.Stop();
        }
    }

}

class GameTimer
{
    public static Stopwatch timer;

    public static float eTime;
    public static int sec;
    public static int min;

    public static int inSec = 30;
    public static int inMin = 3;

    public static int toSec = 0;
    public static int cdSec = 0;
    public static int cdMin = 0;

    static GameTimer()
    {
        timer = new Stopwatch();
    }

    public static void Run()
    {
        eTime = (float)(timer.ElapsedMilliseconds) / 1000.0f;
        sec = (int)eTime;
        min = (int)sec / 60;

        toSec = inMin * 60 + inSec - sec;
        cdMin = toSec / 60;
        cdSec = toSec % 60;
    }

    public static void SetTime(int min, int sec)
    {
        inMin = min;
        inSec = sec;
    }

    public static void Reset()
    {
        timer.Reset();
        Run();
    }

    public static void Restart()
    {
        timer.Restart();
        Run();
    }

    public static void Stop()
    {
        timer.Stop();
        Run();
    }

    public static void Start()
    {
        timer.Start();
        Run();
    }

    public static bool isEnd
    {
        get
        {
            if (toSec <= 0)
            {
                return true;
            }

            return false;
        }
    }

    public static bool isRunning
    {
        get
        {          
            return timer.IsRunning;
        }
    }
}
