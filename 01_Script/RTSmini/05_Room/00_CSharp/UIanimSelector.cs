using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using UserAnimSpace;

public class UIanimSelector : MonoBehaviour
{    
    public GameManager gameManager;
    public Toggle[] tgAnim;
    UnitManager[] unitMan;
    UserAnimation[] anims;

    int unitCount;
    int animCount;

    string[] animName = { "Idle", "Running", "Attacking" };

    class Idx
    {
        public int id;
    }

    List<Idx> listIdx;

    // Start is called before the first frame update
    void Start()
    {
        unitCount = gameManager.unitManagers.Length;
        animCount = tgAnim.Length;

        unitMan = gameManager.unitManagers;
        anims = new UserAnimation[unitCount];

        for(int i = 0; i < unitCount; i++)
        {
            anims[i] = unitMan[i].anims[0];
        }

        listIdx = new List<Idx>();

        for (int i = 0; i < animCount; i++)
        {
            Idx idx = new Idx { id = i };
            listIdx.Add(idx);

            tgAnim[i].onValueChanged.AddListener(
                (value) =>
                {
                    for(int j = 0; j < unitCount; j++)
                    {
                        anims[j].PlayCross(animName[idx.id]);
                    }
                });
        }
    }  
}
