using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class SliderCellColor : MonoBehaviour
{
    public Slider slider;
    public InputField ipField;
    public Image image;
    //public Text nameText;
    //public Text minText;
    //public Text maxText;
    //public string nameValue = "nameValue";
    public float minValue = 0.0f;
    public float maxValue = 1.0f;

    public ColorType cType;

    public Action<float> onValueChanged
    {
        get; set;
    } = (value) => { };

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
                ipField.text = string.Format("{0:f2}", value);
                //ipField.text = value.ToString();

                if (cType == ColorType.red)
                {
                    image.color = new Color(value, 0.0f, 0.0f, 1.0f);
                }
                else if(cType == ColorType.green)
                {
                    image.color = new Color(0.0f, value, 0.0f, 1.0f);
                }
                else if(cType == ColorType.blue)
                {
                    image.color = new Color(0.0f, 0.0f, value, 1.0f);
                }                

                onValueChanged(value);                
            });

        ipField.onEndEdit.AddListener(
            (value) =>
            {
                //slider.onValueChanged.Invoke(Convert.ToSingle(value));
                slider.value = Convert.ToSingle(value);
            });
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [Serializable]
    public enum ColorType
    { 
        red, green, blue
    }
}
