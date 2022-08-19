using System.Collections;
using System.Collections.Generic;
using System;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class AudioManager : MonoBehaviour
{
    //public static bool bAudio = true;
    public static bool bBGM = true;
    public static int idx_bgm = 0;    
    public static float[] vbgm = {1.0f, 0.25f, 0.5f, 0.75f};
   
    public static bool bEffect = false;   
    public static float vEffect = 1.0f;

    public AudioClip[] audioClips;
    public AudioSource audio;
    public static int cCount;

    public Text name_bgm;
    public Button btLeft;
    public Button btRight;
    public Slider sVolume;
 

    //public bool _bAudio = true;
    public bool _bBGM = true;
    public bool _bEffect = true;

    static AudioManager()
    {
        
    }

    void Awake()
    {
        //cCount = audioClips.Length;
        //vDef_bgm = new float[cCount];
        //vbgm = new float[cCount];
        //for (int i = 0; i < cCount; i++)
        //{          
        //    vDef_bgm[i] = 1.0f;
        //}

        //vbgm[0] = vDef_bgm[0] = 1.0f;
        //vbgm[0] = vDef_bgm[1] = 0.25f;
        //vbgm[0] = vDef_bgm[2] = 1.0f;
        //vbgm[0] = vDef_bgm[3] = 1.0f;
        //
        //{
        //    PlayBGM();
        //}
    }    

    void Start()
    {
        cCount = audioClips.Length;

        {
            SceneManager.sceneUnloaded += OnSceneLeft;
        }

        {
            OnBGMPause += BGMPause;
            OnBGMResume += BGMResume;
        }


        //{
        //    SceneManager.sceneUnloaded += OnSceneLeft;
        //}
        //
        //{
        //    OnBGMPause += BGMPause;
        //    OnBGMResume += BGMResume;
        //}

        PlayBGM();

        if (bBGM)
        {
            OnBGMResume();
        }
        else
        {
            OnBGMPause();
        }

        if (bEffect)
        {
            OnEffectResume();
        }
        else
        {
            OnEffectPause();
        }

        DebugManager.info.text += "bBGM : " + bBGM.ToString() + "\n";
    }   

    public void Init()
    {
       
    }

    //public Text debugInfo;

    public void Begin()
    {
        

    }

    private void OnDestroy()
    {
        SceneManager.sceneUnloaded -= OnSceneLeft;

        {
            OnBGMPause -= BGMPause;
            OnBGMResume -= BGMResume;
        }
    }

    void OnSceneLeft(Scene scene)
    {
        OnDestroy();
    }

    public static Action OnBGMPause
    {
        get; set;
    } = () => { };

    public static Action OnBGMResume
    {
        get; set;
    } = () => { };

    public static Action OnEffectPause
    {
        get; set;
    } = () => { };

    public static Action OnEffectResume
    {
        get; set;
    } = () => { };

    public void BGMPause()
    {
        if (audio.isPlaying)
        {
            audio.Pause();
        }
        else
        {
        
        }            
    }

    public void BGMResume()
    {
        if(audio.isPlaying)
        {
            audio.UnPause();
        }
        else
        {        
            PlayBGM();
        }       
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.B))
        //{
        //    idx_bgm++;
        //    if (idx_bgm == cCount)
        //    {
        //        idx_bgm = 0;
        //    }
        //
        //    bBGM = true;
        //    PlayBGM();
        //}
        //
        //if (Input.GetKeyDown(KeyCode.V))
        //{
        //    if (idx_bgm == 0)
        //    {
        //        idx_bgm = cCount;
        //    }
        //    idx_bgm--;
        //
        //    bBGM = true;
        //    PlayBGM();
        //}
        //
        //if(Input.GetKeyDown(KeyCode.G))
        //{
        //    bBGM = true;
        //    PlayBGM();
        //}
        //
        //if (Input.GetKeyDown(KeyCode.H))
        //{
        //    if (bBGM)
        //    {                
        //        OnBGMPause();                           
        //        bBGM = _bBGM = false;             
        //    }
        //    else
        //    {               
        //        OnBGMResume();              
        //        bBGM = _bBGM = true;              
        //    }
        //}

        if (Input.GetKeyDown(KeyCode.J))
        {
            if (bEffect)
            {               
                OnEffectPause();           
                bEffect = _bEffect = false;
            }
            else
            {               
                OnEffectResume();             
                bEffect = _bEffect = true;
            }
        }        
    }

    public void PlayBGM(int idx, float volume = 1.0f)
    {
        //if (bAudio && bBGM)
        if (bBGM)
        {
            audio.clip = audioClips[idx];
            audio.volume = volume;
            audio.Play();
        }
    }

    public void PlayBGM()
    {     
        //if (bBGM)
        {
            audio.clip = audioClips[idx_bgm];
            audio.volume = vbgm[idx_bgm];
            audio.Play();
        }
    }

    public void PlayBGM1()
    {       
        {
            audio.clip = audioClips[idx_bgm];
            audio.volume = vbgm[idx_bgm];
            audio.Play();           
        }
    }

    public void AudioPlay()
    {
        audio.Play();
    }

    public void AudioPause()
    {
        audio.Pause();
    }

    public void AudioStop()
    {
        audio.Stop();
    }


}
