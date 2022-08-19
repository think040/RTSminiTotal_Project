using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitDataToggle : MonoBehaviour
{
    public int id;
    public UIunitSelector unitSelector;

    public Toggle toggle;
    public GameObject panel;

    //public ToggleGroup tGroup0;
    public ToggleGroup tGroup1;
   
    public Button btSave;  
    public Button btDefData;

    //public UIUnitSetting unitSetting;

    public int unitId;   

    public UnitManager unitMan;
    //public HPbarMan hpbarMan;
    //public SelectMan selectMan;

    public UnitDataXml dataXml;

    private void OnEnable()
    {
        
    }

    // Start is called before the first frame update
    void Start()
    {               
        {            
            if (toggle.isOn)
            {
                panel.SetActive(true);                      
            }
            else
            {
                panel.SetActive(false);
            }
        }

        //toggle.onValueChanged.AddListener(
        //  (value) =>
        //  {
        //      panel.SetActive(value);
        //
        //      if (value)
        //      {
        //          foreach (var t in tGroup1.ActiveToggles())
        //          {
        //              t.onValueChanged.Invoke(true);
        //          }
        //      }
        //  });

        unitSelector.onTap[id] +=
            (value) =>
            {
                panel.SetActive(value);

                if (value)
                {
                    foreach (var t in tGroup1.ActiveToggles())
                    {
                        t.onValueChanged.Invoke(true);
                    }
                }

            };

        btDefData.onClick.AddListener(
            () =>
            {
                SetDefData();

                //hpbarMan.ApplyData(unitId);
                //unitMan.ApplyUnitData();

                foreach (var t in tGroup1.ActiveToggles())
                {
                    t.onValueChanged.Invoke(true);
                }
            });

        if (panel.activeSelf)
        {
            foreach (var t in tGroup1.ActiveToggles())
            {
                t.onValueChanged.Invoke(true);
            }
        }

        //btSave.onClick.AddListener(
        //    () =>
        //    {
        //        dataXml.SaveXml();          
        //    });

        //unitSetting.OnEnableAction +=
        //    () =>
        //    {
        //        if (panel.activeSelf)
        //        {
        //            foreach (var t in tGroup1.ActiveToggles())
        //            {
        //                t.onValueChanged.Invoke(true);
        //            }
        //        }
        //
        //        //Debug.Log(panel.name);
        //    };
    }

    public void SetDefData()
    {      
        GameManager.maxHp[unitId]        = GameManager.maxHp_def[unitId];
        GameManager.hitHp[unitId]        = GameManager.hitHp_def[unitId];
        GameManager.healHp[unitId]       = GameManager.healHp_def[unitId];
        GameManager.viewRadius[unitId]   = GameManager.viewRadius_def[unitId];
        GameManager.attackRadius[unitId] = GameManager.attackRadius_def[unitId];
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
