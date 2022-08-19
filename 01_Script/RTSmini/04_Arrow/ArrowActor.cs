using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

public class ArrowActor : MonoBehaviour
{
    public static float volume;

    static ArrowActor()
    {
        volume = 0.1f;
    }

    public AudioSource audio;

    // Start is called before the first frame update
    void Start()
    {
        ////audio = GetComponent<AudioSource>();
        //
        if(audio != null)
        {
            audio.Stop();
        }
        //
        //{
        //    SceneManager.sceneUnloaded += OnSceneLeft;
        //}
        //
        //{
        //    GamePlay.PauseAction += PauseAction;
        //    GamePlay.ResumeAction += ResumeAction;
        //}
        //
        //{
        //    AudioManager.OnEffectPause += OnAudioPause;
        //    AudioManager.OnEffectResume += OnAudioResume;
        //}       
    }

    public void Begin()
    {
        //audio = GetComponent<AudioSource>();

        if (audio != null)
        {
            audio.Stop();
        }

        {
            SceneManager.sceneUnloaded += OnSceneLeft;
        }

        {
            GamePlay.PauseAction += PauseAction;
            GamePlay.ResumeAction += ResumeAction;
        }

        {
            AudioManager.OnEffectPause += OnAudioPause;
            AudioManager.OnEffectResume += OnAudioResume;
        }
    }

    void OnSceneLeft(Scene scene)
    {
        OnDestroy();
    }

    private void OnDestroy()
    {
        {
            SceneManager.sceneUnloaded -= OnSceneLeft;
        }

        {
            GamePlay.PauseAction -= PauseAction;
            GamePlay.ResumeAction -= ResumeAction;
        }

        {
            AudioManager.OnEffectPause -= OnAudioPause;
            AudioManager.OnEffectResume -= OnAudioResume;
        }
    }

    protected void PauseAction()
    {       
        //if(audio != null)
        {
            //audio.Pause();
            //OnAudioPause();
        }
      
    }

    protected void ResumeAction()
    {
        //if (audio != null)
        {
            //audio.UnPause();
            //OnAudioResume();
        }            
    }

    void OnAudioPause()
    {
        if (audio != null)
        {
            //audio.Pause();          
            audio.mute = true;
        }            
    }

    void OnAudioResume()
    {
        if (audio != null)
        {
            //audio.UnPause();
            audio.mute = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AudioPlay()
    {
        //if (AudioManager.bAudio && AudioManager.bEffect)
        {
            //audio.volume = volume;
            audio.Play();
        }
    }
}
