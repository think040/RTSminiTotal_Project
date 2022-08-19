using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class BGMselector : MonoBehaviour
{
    public AudioManager audioMan;
    public SettingPanel settingPanel;
    public TapPanelManager tapManager;

    public Text txName;
    public Button btLeft;
    public Button btRight;
    public Slider slVolume;
    public Toggle tgPause;

    // Start is called before the first frame update

    void Awake()
    {
        settingPanel.OnEnableAction +=
           () =>
           {
               tgPause.isOn = AudioManager.bBGM;
               txName.text = audioMan.audioClips[AudioManager.idx_bgm].name;
               slVolume.value = AudioManager.vbgm[AudioManager.idx_bgm];
           };
    }

    void Start()
    {
        //settingPanel.OnEnableAction +=
        //    () =>
        //    {
        //        tgPause.isOn = AudioManager.bBGM;
        //        txName.text = audioMan.audioClips[AudioManager.idx_bgm].name;
        //        slVolume.value = AudioManager.vbgm[AudioManager.idx_bgm];                
        //    };

        //tapManager.OnTap[1] +=
        //    () =>
        //    {
        //        tgPause.isOn = AudioManager.bBGM;
        //        txName.text = audioMan.audioClips[AudioManager.idx_bgm].name;
        //        slVolume.value = AudioManager.vbgm[AudioManager.idx_bgm];
        //    };

        btLeft.onClick.AddListener(
            () =>
            {
                if (AudioManager.idx_bgm == 0)
                {
                    AudioManager.idx_bgm = AudioManager.cCount;
                }
                AudioManager.idx_bgm--;

                AudioManager.bBGM = true;
                audioMan.PlayBGM();

                tgPause.isOn = true;
                txName.text = audioMan.audioClips[AudioManager.idx_bgm].name;
                slVolume.value = AudioManager.vbgm[AudioManager.idx_bgm];
            });

        btRight.onClick.AddListener(
            () =>
            {
                AudioManager.idx_bgm++;
                if (AudioManager.idx_bgm == AudioManager.cCount)
                {
                    AudioManager.idx_bgm = 0;
                }

                AudioManager.bBGM = true;
                audioMan.PlayBGM();

                tgPause.isOn = true;
                txName.text = audioMan.audioClips[AudioManager.idx_bgm].name;
                slVolume.value = AudioManager.vbgm[AudioManager.idx_bgm];
            });

        slVolume.maxValue = 1.0f;
        slVolume.minValue = 0.0f;
        slVolume.onValueChanged.AddListener(
            (value) =>
            {
                AudioManager.vbgm[AudioManager.idx_bgm] = value;
                audioMan.audio.volume = value;
            });

        tgPause.onValueChanged.AddListener(
            (value) =>
            {               
                if(value)
                {
                    AudioManager.bBGM = true;
                    AudioManager.OnBGMResume();                                     
                }
                else
                {
                    AudioManager.bBGM = false;
                    AudioManager.OnBGMPause();                             
                }
            });
    }   
    
}
