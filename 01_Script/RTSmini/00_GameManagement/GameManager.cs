using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

using Unity.Mathematics;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;



public class GameManager : MonoBehaviour
{
    public bool _inRoom = false;
    public bool _useEditor = false;
    
    //public int vSyncCount = 0;
    //public int targetFPS = 60;
    //public bool libraryMode = false;
    public int[] _unitCounts;
    //public int _arrowCount;
    public int[] _playerNum;
    public float[] _viewRadius;
    public float[] _attackRadius;
    public Color[] _playerColor;
    public int[] _killedCount;
    
    public SelectManager selectManager;
    public UnitManager[] unitManagers;
    public CSMAction csmAction;
    public BattleGroundManager bgManager;
    public GroundManager gManager;
    public ArrowManager arrowManager;
    public TorusManager torusManager;
    public HpbarManager hpbarManager;
    public GameDataXml gameDataXml;
    public GameSettingXml gameSettingXml;
    public UnitDataXml unitDataXml;
    public UnitColorXml unitColorXml;
    public UIManager uiManager;
    public AudioManager audioManager;
    public DebugManager debugManager;

    public static int2[] ic;

    public static KeyCode key_pause = KeyCode.X;

    public static int fpsMode = 0;
    public static bool fpsShow = true;   

    public static bool inRoom
    {
        get; set;
    }

    public static bool useEditor
    { get; set; } = false;
   
    public static int unitCount
    {
        get; set;
    }

    public static int[] unitCounts
    {
        get; private set;
    }

    public static int[] unitCounts_def
    {
        get; private set;
    }

    public static int maxUnitCount
    {
        get; set;
    }

    public static UnitManager[] unitMans;
    public static UnitActor[] unitActors;
    public static Transform[] unitTrs;
    public static CapsuleCollider[] unitCols;
    
    public static int unitManCount
    { 
        get; set; 
    }

    public static int[] playerNum
    {
        get; set;
    }

    public static Color[] playerColor
    {
        get; private set;
    }

    public static int[] killedCount
    {
        get; set;
    }

    public static float[] viewRadius
    {
        get; set;
    }

    public static float[] viewRadius_def
    {
        get; set;
    }

    public static float[] attackRadius
    {
        get; set;
    }

    public static float[] attackRadius_def
    {
        get; set;
    }

    public static bool[] activeData
    {
        get; set;
    }
    
    public static int arrowCount
    {
        get; set;
    }

    public static int arrowCount_def
    {
        get; set;
    }

    public static float[] maxHp
    {
        get; set;
    }

    public static float[] maxHp_def
    {
        get; set;
    }

    public static float[] hitHp
    {
        get; set;
    }

    public static float[] hitHp_def
    {
        get; set;
    }

    public static float[] healHp
    {
        get; set;
    }

    public static float[] healHp_def
    {
        get; set;
    }



    public static ArrowManager arrowMans;
    public static Transform[] arrowTrs
    {
        get; set;
    }

    public static TorusManager torusMan;
    public static HpbarManager hpbarMan;

    public TargetAction targetAction;
    public CullManager cullManager;



    private void Awake()
    {
        inRoom = _inRoom;

        if(inRoom)
        {
            AwakeRoom();
        }
        else
        {
            AwakePlay();
        }

        //UnityEngine.Debug.Log("GameManager.Awake()");
    }

    public void AwakeRoom()
    {
        //QualitySettings.vSyncCount = 0;
        //Application.targetFrameRate = 60;

        maxUnitCount = 4;

        this.InitRoom();

        unitDataXml.Init();
        csmAction.InitRoom();
      
        for (int i = 0; i < unitManagers.Length; i++)
        {
            unitManagers[i].unitIdx = i;
            unitManagers[i].InitInRoom();
        }       

        unitColorXml.Init();

        gManager.Init();

        SceneManager.sceneUnloaded += OnSceneLeft;
    }

    public void AwakePlay()
    {
        //QualitySettings.vSyncCount = vSyncCount;
        //Application.targetFrameRate = targetFPS;

#if UNITY_EDITOR
        useEditor = _useEditor;
#else
        useEditor = false;
#endif

        maxUnitCount = 256;

        debugManager.Init();
        gameDataXml.Init();
        gameSettingXml.Init();

        SetFPS();

        this.InitPlay();
     
        unitDataXml.Init();
        csmAction.InitPlay();

        selectManager.Init();

        for (int i = 0; i < unitManagers.Length; i++)
        {
            unitManagers[i].unitIdx = i;
            unitManagers[i].Init();
        }

        unitColorXml.Init();

        bgManager.Init();

        arrowManager.Init();

        torusManager.Init();

        hpbarManager.Init();

        targetAction.Init();

        cullManager.Init();

        uiManager.Init();

        audioManager.Init();

        SceneManager.sceneUnloaded += OnSceneLeft;
    }


   

    static GameManager()
    {
        //if(!useEditor)
        {
            int count = 4;

            {
                ic = new int2[count];
                unitCounts = new int[count];
                unitCounts_def = new int[count];

                viewRadius = new float[count];
                viewRadius_def = new float[count];
                attackRadius = new float[count];
                attackRadius_def = new float[count];

                maxHp = new float[count];
                maxHp_def = new float[count];
                hitHp = new float[count];
                hitHp_def = new float[count];
                healHp = new float[count];
                healHp_def = new float[count];
            }

            {
                unitCounts[0] = unitCounts_def[0] = 64;
                unitCounts[1] = unitCounts_def[1] = 64;
                unitCounts[2] = unitCounts_def[2] = 64;
                unitCounts[3] = unitCounts_def[3] = 64;

                //arrowCount = 
                arrowCount_def = 256;  //512

                viewRadius[0] = viewRadius_def[0] = 35.0f;
                viewRadius[1] = viewRadius_def[1] = 30.0f;
                viewRadius[2] = viewRadius_def[2] = 100.0f;
                viewRadius[3] = viewRadius_def[3] = 100.0f;

                attackRadius[0] = attackRadius_def[0] = 30.0f;
                attackRadius[1] = attackRadius_def[1] = 1.5f;
                attackRadius[2] = attackRadius_def[2] = 1.5f;
                attackRadius[3] = attackRadius_def[3] = 1.5f;

                maxHp[0] = maxHp_def[0] = 150.0f;
                maxHp[1] = maxHp_def[1] = 200.0f;
                maxHp[2] = maxHp_def[2] = 200.0f;
                maxHp[3] = maxHp_def[3] = 250f;

                hitHp[0] = hitHp_def[0] = 5.0f;
                hitHp[1] = hitHp_def[1] = 10.0f;
                hitHp[2] = hitHp_def[2] = 10.0f;
                hitHp[3] = hitHp_def[3] = 20.0f;

                healHp[0] = healHp_def[0] = 4.0f;  
                healHp[1] = healHp_def[1] = 4.0f;  
                healHp[2] = healHp_def[2] = 1.0f;  
                healHp[3] = healHp_def[3] = 1.0f;
            }

            {
                unitCount = 0;
                for (int i = 0; i < count; i++)
                {
                    ic[i].x = (i < 1 ? 0 : ic[i - 1].x + ic[i - 1].y);
                    ic[i].y = unitCounts[i];

                    unitCount += unitCounts[i];
                }
            }
        }
    }

    public void InitRoom()
    {
        //if (useEditor)
        {
            
            int cNum = unitCounts.Length;
            ic = new int2[cNum];

            unitCount = 0;
            for (int i = 0; i < cNum; i++)
            {
                unitCounts[i] = _unitCounts[i];
                ic[i].x = (i < 1 ? 0 : ic[i - 1].x + ic[i - 1].y);
                ic[i].y = unitCounts[i];

                unitCount += unitCounts[i];
            }
        }

        {
            playerNum = _playerNum;
            playerColor = _playerColor;
        }

        {
            killedCount = _killedCount;
        }

        unitManCount = unitManagers.Length;
        unitMans = new UnitManager[unitManCount];


        for (int i = 0; i < unitManCount; i++)
        {
            unitMans[i] = unitManagers[i];
        }

        unitActors = new UnitActor[maxUnitCount];
        unitTrs = new Transform[maxUnitCount];
        unitCols = new CapsuleCollider[maxUnitCount];

        activeData = new bool[maxUnitCount];
        for (int i = 0; i < maxUnitCount; i++)
        {
            activeData[i] = false;
        }

        arrowMans = arrowManager;
        arrowTrs = new Transform[arrowCount_def];

        torusMan = torusManager;
        hpbarMan = hpbarManager;
    }


    public void InitPlay()
    {


//#if UNITY_EDITOR
//        useEditor = _useEditor;
//#else
//        useEditor = false;
//#endif

        if (useEditor)
        {                        
            unitCounts = _unitCounts;
            //arrowCount = _arrowCount;
            viewRadius = _viewRadius;
            attackRadius = _attackRadius;
            
            int cNum = unitCounts.Length;
            ic = new int2[cNum];

            unitCount = 0;
            for (int i = 0; i < cNum; i++)
            {
                ic[i].x = (i < 1 ? 0 : ic[i - 1].x + ic[i - 1].y);
                ic[i].y = unitCounts[i];

                unitCount += unitCounts[i];
            }

            //arrowCount = unitCounts[0] * 4;
        }
        else
        {
            //unitCounts[0] = unitCounts_def[0];
            //unitCounts[1] = unitCounts_def[1];
            //unitCounts[2] = unitCounts_def[2];
            //unitCounts[3] = unitCounts_def[3];

            //arrowCount = unitCounts[0] * 4;

            //
            //viewRadius[0] = viewRadius_def[0];
            //viewRadius[1] = viewRadius_def[1];
            //viewRadius[2] = viewRadius_def[2];
            //viewRadius[3] = viewRadius_def[3];
            //
            //attackRadius[0] = attackRadius_def[0];
            //attackRadius[1] = attackRadius_def[1];
            //attackRadius[2] = attackRadius_def[2];
            //attackRadius[3] = attackRadius_def[3];

            int cNum = unitCounts.Length;

            unitCount = 0;
            for (int i = 0; i < cNum; i++)
            {                
                ic[i].x = (i < 1 ? 0 : ic[i - 1].x + ic[i - 1].y);
                ic[i].y = unitCounts[i];

                unitCount += unitCounts[i];
            }
        }

        {            
            playerNum = _playerNum;
            playerColor = _playerColor;
        }
       
        {
            killedCount = _killedCount;            
        }
              
        unitManCount = unitManagers.Length;
        unitMans = new UnitManager[unitManCount];
        

        for(int i = 0; i < unitManCount; i++)
        {
            unitMans[i] = unitManagers[i];            
        }

        unitActors = new UnitActor[maxUnitCount];
        unitTrs = new Transform[maxUnitCount];
        unitCols = new CapsuleCollider[maxUnitCount];

        activeData = new bool[maxUnitCount];        
        for(int i = 0; i < maxUnitCount; i++)
        {
            activeData[i] = false;
        }

        arrowMans = arrowManager;       
        arrowTrs = new Transform[arrowCount_def];

        torusMan = torusManager;
        hpbarMan = hpbarManager;
    }




    // Start is called before the first frame update
    void Start()
    {
        if(inRoom)
        {
            StartRoom();
        }
        else
        {
            StartPlay();
        }       
    }

    void StartRoom()
    {
        GamePlay.isResume = true;
        ShowSingleUnitInRoom(0);

        fpsTimer = new Stopwatch();
        fpsTimer.Start();
    }

    void StartPlay()
    {
        for (int i = 0; i < unitManagers.Length; i++)
        {
            unitManagers[i].Begin();
        }

        selectManager.Begin();

        //targetAction.Begin();
        arrowManager.Begin();

        cullManager.Begin();

        uiManager.Begin();

        audioManager.Begin();

        GamePlay.isResume = true;
        GamePlay.isNvStop = false;
        StartCoroutine(TestResumePause());
        StartCoroutine(TestTimeOut());
      
        fpsTimer = new Stopwatch();
        fpsTimer.Start();
    }

    

    void OnSceneLeft(Scene scene)
    {
        OnDestroy();
    }

    private void OnDestroy()
    {
        //unitActors = null;
        //unitCols = null;
        //unitTrs = null;
    }

    void SetFPS()
    {
        if (fpsMode == 0)
        {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
        }
        else if (fpsMode == 1)
        {
            Application.targetFrameRate = 90;
            QualitySettings.vSyncCount = 0;
        }
        else if (fpsMode == 2)
        {
            Application.targetFrameRate = 120;
            QualitySettings.vSyncCount = 0;
        }
        else if (fpsMode == 3)
        {
            Application.targetFrameRate = 240;
            QualitySettings.vSyncCount = 0;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(inRoom)
        {
            UpdateRoom();
        }
        else
        {
            UpdatePlay();
        }

        if(Input.GetKeyDown(KeyCode.P))
        {
            fpsMode = (++fpsMode) % 4;

            SetFPS();
        }
    }

    int fpsIdx = 0;
    

    void UpdateRoom()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            roomIdx++;
            if (roomIdx >= unitManCount) roomIdx = 0;
            if (roomIdx < 0) roomIdx = unitManCount - 1;
            ShowSingleUnitInRoom(roomIdx);
        }
    }

    void UpdatePlay()
    {
#if UNITY_EDITOR
        //if(libraryMode)
        //{
        //    Thread.Sleep(30);
        //}
#endif

        //GamePlay.Update();

        //TestResume();
        
        //ShowFPS();
        //ShowKillInfo();
    }


    int roomIdx = 0;    
    public void ShowSingleUnitInRoom(int idx)
    {
        
        for(int i = 0; i < unitManCount; i++)
        {
            if(i == idx)
            {
                unitManagers[i].bRender = true;
            }
            else
            {
                unitManagers[i].bRender = false;
            }
        }
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

    public Text killText;

    public void ShowKillInfo()
    {
        killText.text = string.Format("(Blue: {0, 2:D2}), (Red : {1, 2:D2})", killedCount[1], killedCount[0]);
    }


    public void TestResume()
    {
        if(Input.GetKeyDown(KeyCode.P))
        {
            if(GamePlay.isResume)
            {
                GamePlay.isResume = false;
            }
            else
            {
                GamePlay.isResume = true;
            }
        }
    }  

    private void OnApplicationPause(bool pause)
    {
       
    }

    public GameObject SettingPanel;
    public GameObject ResultPanel;

    public IEnumerator TestResumePause()
    {
        //while(true)
        while (!GameTimer.isEnd)
        {            
            //if(GameTimer.toSec > 5)
            {
                if (Input.GetKeyDown(key_pause) || Input.GetKeyDown(KeyCode.Escape))
                {                    
                    if (GamePlay.isResume)
                    {
                        if(GameTimer.toSec > 5)
                        {
                            GamePlay.PauseAction();

                            yield return new WaitForSeconds(2.0f);                            

                            GamePlay.isResume = false;
                            GameTimer.Stop();

                            SettingPanel.SetActive(true);

                            //yield return null;

                            SettingPanel.GetComponent<SettingPanel>().OnEnableAction();
                        }                        
                    }
                    else
                    {                        
                        {
                            GamePlay.ResumeAction();

                            yield return null;

                            GamePlay.isResume = true;
                            GameTimer.Start();

                            SettingPanel.SetActive(false);
                        }                     
                    }
                }
            }
            
            yield return null;
        }        
    }


    IEnumerator TestTimeOut()
    {               
        while (!GameTimer.isEnd)
        {           
            yield return null;
        }


        {
            GamePlay.PauseAction();

            yield return new WaitForSeconds(2.0f);
            
            ResultPanel.SetActive(true);

            GamePlay.isResume = false;
            GameTimer.Stop();
        }
       
    } 

  
}
