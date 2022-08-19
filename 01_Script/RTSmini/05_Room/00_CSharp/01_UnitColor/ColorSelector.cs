using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;


public class ColorSelector : MonoBehaviour
{
    //public Slider[] slider;
    //public InputField[] ipField;
    public Text nameText;
    public Text minText;
    public Text maxText;
    public string nameValue = "nameValue";
    public float minValue = 0.0f;
    public float maxValue = 1.0f;

    public SliderCellColor red;
    public SliderCellColor green;
    public SliderCellColor blue;
    public Image color;
    public Button btApply;

    public Action<Color> onValueChanged
    {
        get; set;
    } = (value) => { };

    public Action<Color> onApply
    {
        get; set;
    } = (color) => { };

    // Start is called before the first frame update
    void OnEnable()
    {
        nameText.text = nameValue;
        
        minText.text = minValue.ToString();
        maxText.text = maxValue.ToString();
       

        red.onValueChanged +=
             (value) =>
             {
                 Color c = color.color;
                 c.r = value;
                 c.a = 1.0f;
                 color.color = c;

                 onValueChanged(c);
             };

        green.onValueChanged +=
             (value) =>
             {
                 Color c = color.color;
                 c.g = value;
                 c.a = 1.0f;
                 color.color = c;

                 onValueChanged(c);
             };

        blue.onValueChanged +=
             (value) =>
             {
                 Color c = color.color;
                 c.b = value;
                 c.a = 1.0f;
                 color.color = c;

                 onValueChanged(c);
             };

        btApply.onClick.AddListener(
            () =>
            {
                onApply(color.color);
            });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
