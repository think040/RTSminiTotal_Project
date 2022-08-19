using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class SceneTransition : MonoBehaviour
{
    public int vSyncCount = 0;
    public int targetFPS = 60;

    public Button[] btToMainScene;
    public Button[] btToUnitRoomScene;
    public Button[] btToGameRoomScene;
    public Button[] btToPlayScene;
    
    public static Action[] evToMainScene;
    public static Action[] evToUnitRoomScene;
    public static Action[] evToGameRoomScene;
    public static Action[] evToPlayScene;

    class Idx
    {
        public int id;
    }

    List<Idx> listIdx;

    
    void Awake()
    {
        {
            QualitySettings.vSyncCount = vSyncCount;
            Application.targetFrameRate = targetFPS;
        }

        listIdx = new List<Idx>();

        if (btToMainScene != null)
        {
            evToMainScene = new Action[btToMainScene.Length];
            for (int i = 0; i < btToMainScene.Length; i++)
            {
                var ev = evToMainScene[i] += () => { };
                Idx idx = new Idx { id = i };
                listIdx.Add(idx);
                btToMainScene[i].onClick.AddListener(
                    () =>
                    {                      
                        evToMainScene[idx.id]();
                        SceneManager.LoadScene(0);
                    });
            }
        }

        if (btToUnitRoomScene != null)
        {
            evToUnitRoomScene = new Action[btToUnitRoomScene.Length];
            for (int i = 0; i < btToUnitRoomScene.Length; i++)
            {
                var ev = evToUnitRoomScene[i] += () => { };
                Idx idx = new Idx { id = i };
                listIdx.Add(idx);
                btToUnitRoomScene[i].onClick.AddListener(
                    () =>
                    {                        
                        evToUnitRoomScene[idx.id]();
                        SceneManager.LoadScene(1);
                    });
            }
        }

        if (btToGameRoomScene != null)
        {
            evToGameRoomScene = new Action[btToGameRoomScene.Length];
            for (int i = 0; i < btToGameRoomScene.Length; i++)
            {
                var ev = evToGameRoomScene[i] += () => { };
                Idx idx = new Idx { id = i };
                listIdx.Add(idx);
                btToGameRoomScene[i].onClick.AddListener(
                    () =>
                    {                      
                        evToGameRoomScene[idx.id]();
                        SceneManager.LoadScene(2);
                    });
            }
        }


        if (btToPlayScene != null)
        {
            evToPlayScene = new Action[btToPlayScene.Length];
            for (int i = 0; i < btToPlayScene.Length; i++)
            {
                evToPlayScene[i] += () => { };
                Idx idx = new Idx { id = i };
                listIdx.Add(idx);
                btToPlayScene[i].onClick.AddListener(
                    () =>
                    {                        
                        evToPlayScene[idx.id]();                        
                        SceneManager.LoadScene(3);                        
                    });
            }           
        }
    }
      
    void Start()
    {                
        
    }

    
    void Update()
    {
        
    }    
}
