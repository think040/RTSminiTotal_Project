using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class FPStext : MonoBehaviour
{
    public SettingPanel settingPanel;
    public Toggle tgShow;

    private void Awake()
    {
        settingPanel.OnEnableAction +=
         () =>
         {
             if (GameManager.fpsShow)
             {
                 tgShow.isOn = true;
             }
             else
             {
                 tgShow.isOn = false;
             }
         };


        tgShow.onValueChanged.AddListener(
            (value) =>
            {
                GameManager.fpsShow = value;
            });
    }

    void Start()
    {
       
    }    
}
