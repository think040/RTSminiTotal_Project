using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class UIunitSelector : MonoBehaviour
{
    public GameManager gameManager;
    public Toggle[] tgUnit;
    public Action<bool>[] onTap;

    int unitCount;

    class Idx
    {
        public int id;
    }

    List<Idx> listIdx;

    private void OnEnable()
    {
        unitCount = gameManager.unitManagers.Length;

        onTap = new Action<bool>[unitCount];
        for (int i = 0; i < unitCount; i++)
        {
            onTap[i] = (value) => { };
        }

        listIdx = new List<Idx>();

        for (int i = 0; i < unitCount; i++)
        {
            Idx idx = new Idx { id = i };
            listIdx.Add(idx);
            tgUnit[i].onValueChanged.AddListener(
                (value) =>
                {
                    if (value)
                    {
                        int id = idx.id;
                        gameManager.ShowSingleUnitInRoom(id);
                        ActTap(id);
                    }
                });
        }

    }

    void ActTap(int id)
    {
        for(int i = 0; i < unitCount; i++)
        {
            if(i == id)
            {
                onTap[i](true);
            }
            else
            {
                onTap[i](false);
            }           
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }    
}
