using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class CamActionSelector : MonoBehaviour
{
    public SettingPanel settingPanel;
    public Slider slMovePace;
    public Slider slZoomPace;
    public Toggle tgMoveMouse;

    public float maxMovePace = 300;
    public float minMovePace = 1;
    public float maxZoomPace = 5;
    public float minZoomPace = 1;

    void Awake()
    {
        settingPanel.OnEnableAction +=
            () =>
            {
                slMovePace.value = CamAction.pacePlane;
                slZoomPace.value = CamAction.paceZoom;
                tgMoveMouse.isOn = CamAction.useMouseForMove;
            };

        slMovePace.maxValue = maxMovePace;
        slMovePace.minValue = minMovePace;
        slMovePace.onValueChanged.AddListener(
            (value) =>
            {
                CamAction.pacePlane = value;
            });

        slZoomPace.maxValue = maxZoomPace;
        slZoomPace.minValue = minZoomPace;
        slZoomPace.onValueChanged.AddListener(
            (value) =>
            {
                CamAction.paceZoom = value;
            });

        tgMoveMouse.onValueChanged.AddListener(
            (value) =>
            {               
                CamAction.useMouseForMove = value;                
            });
    }


    void Start()
    {
        
    }    
}
