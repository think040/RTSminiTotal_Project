using System;
using System.Collections;
using System.Collections.Generic;
//using Unity.Mathematics;

using UnityEngine;

using UnityEngine.UI;


public class UIDataSelector : MonoBehaviour
{
    public Slider slider;
    public InputField ipField;

    public Text nameText;
    public Text minText;
    public Text maxText;

    public string nameValue = "nameValue";
    public float minValue = 0.0f;
    public float maxValue = 1.0f;

    public Button btApply;

    public Action<float> onValueChanged
    {
        get; set;
    } = (value) => { };
    

    public Action<float> onApply
    {
        get; set;
    } = (color) => { };


    // Start is called before the first frame update
    void OnEnable()
    {
        //nameText.text = nameValue;

        slider.minValue = minValue;
        slider.maxValue = maxValue;
        //minText.text = minValue.ToString();
        //maxText.text = maxValue.ToString();

        slider.onValueChanged.AddListener(
            (value) =>
            {                
                if(value < 10.0f)
                {                  
                    if (value < 1.0f)
                    {
                        ipField.text = string.Format("{0:f2}", value);
                    }
                    else
                    {
                        ipField.text = string.Format("{0:f1}", value);
                    }
                }                
                else
                {
                    ipField.text = string.Format("{0}", (int)value);
                }                
                //ipField.text = value.ToString();

                onValueChanged(value);
            });

        ipField.onEndEdit.AddListener(
            (value) =>
            {
                //slider.onValueChanged.Invoke(Convert.ToSingle(value));
                slider.value = Convert.ToSingle(value);
            });

        btApply.onClick.AddListener(
            () =>
            {
                onApply(slider.value);
            });
    }

    

    // Update is called once per frame
    void Update()
    {

    }    
}
