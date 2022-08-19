using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class SoundEffectToggle : MonoBehaviour
{
    //public AudioManager audioMan;
    public SettingPanel settingPanel;

    public Toggle tgEffect;

    private void Awake()
    {
        settingPanel.OnEnableAction += 
            () =>
            {
                if(AudioManager.bEffect)
                {
                    tgEffect.isOn = true;
                }
                else
                {
                    tgEffect.isOn = false;
                }

            };

        tgEffect.onValueChanged.AddListener(
            (value) =>
            {
                if (value)
                {
                    AudioManager.bEffect = true;
                    AudioManager.OnEffectResume();                                 
                }
                else
                {
                    AudioManager.bEffect = false;
                    AudioManager.OnEffectPause();                   
                }
            });

    }


    void Start()
    {
        
    }   
}
