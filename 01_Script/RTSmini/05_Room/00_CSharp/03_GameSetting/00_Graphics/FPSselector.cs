using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;


public class FPSselector : MonoBehaviour
{
    public SettingPanel settingPanel;
    public Toggle toggle;
    public int fps;
    public int mode;
    
    private void Awake()
    {
        settingPanel.OnEnableAction +=
           () =>
           {
               if (GameManager.fpsMode == mode)
               {
                   toggle.isOn = true;
               }
           };

        toggle.onValueChanged.AddListener(
            (value) =>
            {
                if (value)
                {
                    GameManager.fpsMode = mode;
                    Application.targetFrameRate = fps;
                    QualitySettings.vSyncCount = 0;
                }
            });                    
    }

    // Start is called before the first frame update
    void Start()
    {
        

    }
}
