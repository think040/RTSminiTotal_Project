using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitDataUI : MonoBehaviour
{
    public UnitDataXml unitDataXml;


    // Start is called before the first frame update
    void Start()
    {
        SceneTransition.evToMainScene[0] += toMainScene;
        SceneTransition.evToGameRoomScene[0] += toGameRoomScene;
    }

    void toMainScene()
    {
        unitDataXml.SaveXml();
        SceneTransition.evToMainScene[0] -= toMainScene;
        SceneTransition.evToGameRoomScene[0] -= toGameRoomScene;
    }

    void toGameRoomScene()
    {
        unitDataXml.SaveXml();
        SceneTransition.evToMainScene[0] -= toMainScene;
        SceneTransition.evToGameRoomScene[0] -= toGameRoomScene;
    }

}
