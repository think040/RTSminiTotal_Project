using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitColorToggle : MonoBehaviour
{
    public int id;
    public UIunitSelector unitSelector;

    public Toggle toggle;
    public GameObject panel;

    //public ToggleGroup tGroup0;
    public ToggleGroup tGroup1;

    public ColorToggle[] colorToggles;
    public Button btSave;
    

    public Color[] colors;

    public UnitManager unitMan;
    public Color[] skColor;
    public Color[] stColor;

    public Button btDefColor;

    public ArrowManager arrowMan;

    public UnitColorXml colorXml;

    //public UIUnitSetting unitSetting;

    // Start is called before the first frame update
    void OnEnable()
    {       
        for (int i = 0; i < colors.Length; i++)
        {
            //colorToggles[i].color = colors[i];
            //colorToggles[i].toggle.GetComponent<Image>().color = colors[i];
            //colorToggles[i].idx = i;
            //colorToggles[i].onApply +=
            //    (c, idx) =>
            //    {
            //        colors[idx] = c;
            //    };
            
        }

        //colorToggles[0].toggle.onValueChanged.Invoke(true);


        //skColor = unitMan.skColor;
        //stColor = unitMan.stColor;
    }

    private void Start()
    {
        skColor = unitMan.skColor;
        stColor = unitMan.stColor;
        
        {
            {
                //var cs = unitMan.skmb.skColors;                
                //int count = cs.Count;
                //for (int i = 0; i < count; i++)
                //{
                //    skColor[i] = cs[i];
                //}
                //skColor = unitMan.skColor;
            }
        
            {
                //var cs = unitMan.skmb.stColors;
                //int count = cs.Count;
                //for (int i = 0; i < count; i++)
                //{
                //    stColor[i] = cs[i];
                //}
                //stColor = unitMan.stColor;
            }
        }
        
        {
            for(int i = 0; i < colorToggles.Length; i++)
            {
                colorToggles[i].unitMan = unitMan;
                colorToggles[i].skColor = skColor;
                colorToggles[i].stColor = stColor;

                //colorToggles[i].arrowMan = arrowMan;
                //colorToggles[i].arColor = arrowMan.colors;
            }
        }
        
        {
            for (int i = 0; i < colorToggles.Length; i++)
            {
                //colorToggles[i].toggle.onValueChanged.Invoke(true);
                //colorToggles[i].tImage.color = colorToggles[i].GetColor();
                colorToggles[i].InitColor();
            }

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



        btDefColor.onClick.AddListener(
            () =>
            {
                for (int i = 0; i < colorToggles.Length; i++)
                {
                    colorToggles[i].SetDefColor();
                }

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
        //        colorXml.SaveXml();
        //    });

        //unitSetting.OnEnableAction +=
        //   () =>
        //   {
        //       if (panel.activeSelf)
        //       {
        //           foreach (var t in tGroup1.ActiveToggles())
        //           {
        //               t.onValueChanged.Invoke(true);
        //           }
        //       }
        //
        //        //Debug.Log(panel.name);
        //    };
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
