using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class UnitCountSelector : MonoBehaviour
{
    public int unitId;

    public Slider slider;
    public InputField ipField;

    const int minCount = 1;
    const int maxCount = 64;
   
    // Start is called before the first frame update
    private void Awake()
    {
        slider.minValue = (float)minCount;
        slider.maxValue = (float)maxCount;

        slider.onValueChanged.AddListener(
            (value) =>
            {
                int count = Convert.ToInt32(value);
                if (count < minCount)
                {
                    count = minCount;
                }
                else if (maxCount < count)
                {
                    count = maxCount;
                }
                GameManager.unitCounts[unitId] = count;

                ipField.text = count.ToString();
            });

        ipField.onEndEdit.AddListener(
            (value) =>
            {
                slider.value = Convert.ToSingle(value);
            });

        slider.value = GameManager.unitCounts[unitId];
        ipField.text = GameManager.unitCounts[unitId].ToString();
    }

    void Start()
    {
        slider.value = GameManager.unitCounts[unitId];
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
