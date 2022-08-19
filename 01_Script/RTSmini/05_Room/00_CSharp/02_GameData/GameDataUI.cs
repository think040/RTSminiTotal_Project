using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

public class GameDataUI : MonoBehaviour
{
    public GameDataXml gameDataXml;
   
    void Start()
    {       
        {            
            SceneTransition.evToPlayScene[0] += toPlayScene;
            SceneTransition.evToUnitRoomScene[0] += toUnitRoomScene;
        }      
    }
  
    void toPlayScene()
    {       
        gameDataXml.SaveXml();
        SceneTransition.evToPlayScene[0] -= toPlayScene;
        SceneTransition.evToUnitRoomScene[0] -= toUnitRoomScene;
    }

    void toUnitRoomScene()
    {
        gameDataXml.SaveXml();
        SceneTransition.evToPlayScene[0] -= toPlayScene;
        SceneTransition.evToUnitRoomScene[0] -= toUnitRoomScene;        
    }     
}
