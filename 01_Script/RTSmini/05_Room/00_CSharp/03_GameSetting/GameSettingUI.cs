using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSettingUI : MonoBehaviour
{
    public GameSettingXml gameSettingXml;
    
    void Start()
    {
        {            
            SceneTransition.evToPlayScene[0] += toPlayScene;
            SceneTransition.evToMainScene[0] += toMainScene;
        }

        {
            SceneTransition.evToPlayScene[1] += toPlayScene;
            SceneTransition.evToMainScene[1] += toMainScene;
        }
    }   

    void toPlayScene()
    {
        gameSettingXml.SaveXml();

        {
            SceneTransition.evToPlayScene[0] -= toPlayScene;
            SceneTransition.evToMainScene[0] -= toMainScene;
        }

        {
            SceneTransition.evToPlayScene[1] -= toPlayScene;
            SceneTransition.evToMainScene[1] -= toMainScene;
        }
    }

    void toMainScene()
    {
        gameSettingXml.SaveXml();

        {
            SceneTransition.evToPlayScene[0] -= toPlayScene;
            SceneTransition.evToMainScene[0] -= toMainScene;
        }

        {
            SceneTransition.evToPlayScene[1] -= toPlayScene;
            SceneTransition.evToMainScene[1] -= toMainScene;
        }
    }
}
