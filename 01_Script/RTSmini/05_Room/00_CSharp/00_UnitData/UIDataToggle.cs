using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class UIDataToggle : MonoBehaviour
{
    public Toggle toggle;
    public UIDataSelector selector;
    public Text lable;

    public string nameValue = "nameValue";
    public float minValue = 0.0f;
    public float maxValue = 1.0f;

    public int unitId;
    public int dataId;

    public UnitManager unitMan;
    //public ArrowManager arrowMan;
    //public HPbarMan hpbarMan;
    //public SelectMan selectMan;

    void OnEnable()
    {
        lable.text = nameValue;

        toggle.onValueChanged.AddListener(
            (value) =>
            {
                if (value)
                {
                    selector.nameText.text = nameValue;
                    selector.slider.minValue = minValue;
                    selector.slider.maxValue = maxValue;
                    selector.minText.text = minValue.ToString();
                    selector.maxText.text = maxValue.ToString();

                    selector.slider.value = GetData();
                    //selector.onApply = ApplyData;
                    selector.onApply = SetData;
                }
                else
                {
                    
                }
            });
        
    }


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ApplyData(float data)
    {
        //SetData(data);
        //
        //if(dataId == 0)
        //{
        //    hpbarMan.ApplyData(unitId);
        //}
        //else
        //{
        //    unitMan.ApplyUnitData();
        //    //selectMan.ApplyViewRadius();
        //}  
        //
        //if(unitId == 0 && dataId == 1)
        //{
        //    arrowMan.ApplyHitHp();
        //}
    }

    public float GetData()
    {        
        if(dataId == 0)
        {
            return GameManager.maxHp[unitId];
        }
        else if(dataId == 1)
        {
            return GameManager.hitHp[unitId];
        }
        else if (dataId == 2)
        {
            return GameManager.healHp[unitId];
        }
        else if (dataId == 3)
        {
            return GameManager.viewRadius[unitId];
        }
        else if (dataId == 4)
        {
            return GameManager.attackRadius[unitId];
        }

        return 0.0f;
    }

    public void SetData(float data)
    {       
        if (dataId == 0)
        {
            GameManager.maxHp[unitId] = data;
        }
        else if (dataId == 1)
        {
            GameManager.hitHp[unitId] = data;
        }
        else if (dataId == 2)
        {
            GameManager.healHp[unitId] = data;
        }
        else if (dataId == 3)
        {
            GameManager.viewRadius[unitId] = data;
        }
        else if (dataId == 4)
        {
            GameManager.attackRadius[unitId] = data;
        }
    }
}
