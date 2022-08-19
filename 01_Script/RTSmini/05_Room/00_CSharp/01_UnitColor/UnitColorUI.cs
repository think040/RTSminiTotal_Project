using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitColorUI : MonoBehaviour
{
    public UnitColorXml unitColorXml;


    // Start is called before the first frame update
    void Start()
    {
        SceneTransition.evToMainScene[0] += toMainScene;
        SceneTransition.evToGameRoomScene[0] += toGameRoomScene;
    }

    void toMainScene()
    {
        unitColorXml.SaveXml();
        SceneTransition.evToMainScene[0] -= toMainScene;
        SceneTransition.evToGameRoomScene[0] -= toGameRoomScene;
    }

    void toGameRoomScene()
    {
        unitColorXml.SaveXml();
        SceneTransition.evToMainScene[0] -= toMainScene;
        SceneTransition.evToGameRoomScene[0] -= toGameRoomScene;
    }    
}
