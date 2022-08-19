using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class TapPanelManager : MonoBehaviour
{
    public Toggle[] tap;
    public GameObject[] panel;
    public Action[] OnTap;

    public int count;

    class Idx
    {
        public int id;
    }

    List<Idx> listIdx;

    private void Awake()
    {
        count = tap.Length;
        OnTap = new Action[count];
        for (int i = 0; i < count; i++)
        {
            OnTap[i] = () => { };
        }

        listIdx = new List<Idx>();
    }

    // Start is called before the first frame update
    void Start()
    {              

        for(int i = 0; i < count; i++)
        {
            if(tap[i].isOn)
            {
                panel[i].SetActive(true);
            }
            else
            {
                panel[i].SetActive(false);
            }

            Idx idx = new Idx { id = i };
            listIdx.Add(idx);

            tap[i].onValueChanged.AddListener(
                (value) =>
                {
                    panel[idx.id].SetActive(value);
                    OnTap[idx.id]();
                });
        }

        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
