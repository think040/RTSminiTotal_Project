using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class GameTimeSelector : MonoBehaviour
{
    public Type type;

    public Slider slider;
    public InputField ipField;

    public int minValue = 5;
    public int maxValue = 60;
    [System.Serializable]
    public enum Type
    {
        Min, Sec
    }

   
    private void Awake()
    {
        slider.minValue = (float)(minValue);
        slider.maxValue = (float)(maxValue);  
        

        slider.onValueChanged.AddListener(
            (value) =>
            {                
                int time = Convert.ToInt32(value);
                if (time < minValue)
                {
                    time = minValue;
                }
                else if (maxValue < time)
                {
                    time = maxValue;
                }

                if (type == Type.Min)
                {
                    GameTimer.inMin = time;
                }
                else if (type == Type.Sec)
                {
                    GameTimer.inSec = time;
                }

                //slider.value = (float)time;
                ipField.text = time.ToString();
            });

        ipField.onEndEdit.AddListener(
            (value) =>
            {
                slider.value = Convert.ToSingle(value);
            });

        if (type == Type.Min)
        {
            slider.value = GameTimer.inMin;
            ipField.text = GameTimer.inMin.ToString();
        }
        else if (type == Type.Sec)
        {
            slider.value = GameTimer.inSec;
            ipField.text = GameTimer.inSec.ToString();
        }
    }

    // Start is called before the first frame update
    void Start()
    {        
        if (type == Type.Min)
        {
            slider.value = GameTimer.inMin;
        }
        else if (type == Type.Sec)
        {
            slider.value = GameTimer.inSec;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
