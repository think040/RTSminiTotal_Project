using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameSettingXml : MonoBehaviour
{
    XmlDocument xmlDoc;
    string directory;
    string path;
    string fileName = "GameSetting.xml";

    string[] dName;
    int dCount = 3;   

    int smCount = 4;

    //public Text debugInfo;

    void Awake()
    {
        //Init();
    }

    private void Update()
    {

    }

    float bFPS;
    float FPSmode;

    float bBGM;
    float BGMmode;    
    float[] vBGM;
    const int bgmCount = 4;

    float bEffect;
    float vEffect;

    float camPacePlane;
    float camPaceZoom;
    float camUseMouseForMove;
    

    public void Init()
    {
        xmlDoc = new XmlDocument();

#if UNITY_EDITOR
        directory = "Assets/01_Script/RTSmini/05_Room/03_Xml/";
        //Debug.Log(directory);
#else
        directory = Application.persistentDataPath + "/";
        Debug.Log(directory);
        //C:\Users\think\AppData\LocalLow\DefaultCompany\RTSmini_Project   //Win_Api
        //C:\Users\think\AppData\Local\Packages\EasyStrategyKit_pzq3xp76mxafg\LocalState    //UWP
#endif
        path = directory + fileName;


        {
            dName = new string[dCount];          
            dName[0] = "Graphic";
            dName[1] = "Sound";
            dName[2] = "Camera";
        }

        {
            vBGM = new float[bgmCount];
        }

        if (!GameManager.useEditor)
        {
            if (!File.Exists(path))
            {
                CreateXml();
            }
            else
            {
                LoadXml();
            }
        }

        {
            DebugManager.info.text = 
                "PacePlane : " + CamAction.pacePlane.ToString() +"\n" +
                "PaceZoom : " + CamAction.paceZoom.ToString() + "\n";
        }

        {
            //OnConstruct();
        }
    }

    private void OnApplicationQuit()
    {
        if (xmlDoc != null)
        {
            OnAppQuit();
        }
    }

    private void OnApplicationPause(bool pause)
    {
        if (xmlDoc != null)
        {
            OnAppQuit();
        }
    }

    void OnAppQuit()
    {
        SaveXml();
        //Debug.Log("Application.quitting : GameSettingXml.SaveXml()");
    }
    
    void OnSceneLeft(Scene scene)
    {
        OnDestroy();
    }
    
    void OnConstruct()
    {
        {
            SceneManager.sceneUnloaded += OnSceneLeft;
        }
    
        {
            Application.quitting += OnAppQuit;
        }
    }
    
    void OnDestroy()
    {
        {
            SceneManager.sceneUnloaded -= OnSceneLeft;
        }
    
        {
            Application.quitting -= OnAppQuit;
        }
    }
    



    public void Begin()
    {
        
    }    

    public void toData()
    {
        //Graphic
        {
            bFPS = GameManager.fpsShow ? 0.0f : 1.0f;
            FPSmode = (float)GameManager.fpsMode;
        }

        //Sound
        {
            {
                bBGM = AudioManager.bBGM ? 0.0f : 1.0f;
                BGMmode = (float)AudioManager.idx_bgm;
                for(int i = 0; i < bgmCount; i++)
                {
                    vBGM[i] = AudioManager.vbgm[i];
                }
            }

            {
                bEffect = AudioManager.bEffect ? 0.0f : 1.0f;
                {
                    vEffect = AudioManager.vEffect;
                }
            }           
        }

        //Camera
        {
            camPacePlane = CamAction.pacePlane;
            camPaceZoom =  CamAction.paceZoom;
            camUseMouseForMove = CamAction.useMouseForMove ? 0.0f : 1.0f;
        }
    }

    public void fromData()
    {
        //Graphic
        {           
            GameManager.fpsShow = bFPS == 0.0f ? true : false;          
            GameManager.fpsMode = (int)FPSmode;
        }

        //Sound
        {
            {               
                AudioManager.bBGM = bBGM == 0.0f ? true : false;
                AudioManager.idx_bgm = (int)BGMmode;
                for (int i = 0; i < bgmCount; i++)
                {                    
                    AudioManager.vbgm[i] = vBGM[i];
                }
            }

            {
                AudioManager.bEffect = bEffect == 0.0f ? true : false;
                {
                    AudioManager.vEffect = vEffect;
                }
            }
        }

        //Camera
        {
            CamAction.pacePlane = camPacePlane;
            CamAction.paceZoom = camPaceZoom;
            CamAction.useMouseForMove = camUseMouseForMove == 0.0f ? true : false;
        }
    }


    void CreateXml()
    {
        {
            toData();
        }

        XmlDeclaration xmlDec = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", "yes");
        xmlDoc.AppendChild(xmlDec);

        XmlNode root = xmlDoc.CreateNode(XmlNodeType.Element, "GameSetting", string.Empty);
        xmlDoc.AppendChild(root);
        {
            XmlAttribute attr = xmlDoc.CreateAttribute("bSaved");
            attr.Value = "false";
            root.Attributes.Append(attr);
        }        

        {
            XmlNode graphicsNode = xmlDoc.CreateNode(XmlNodeType.Element, dName[0], string.Empty);
            root.AppendChild(graphicsNode);

            {
                XmlElement e0 = xmlDoc.CreateElement("FPS");
                graphicsNode.AppendChild(e0);
                {
                    XmlAttribute attr;
                    attr = xmlDoc.CreateAttribute("On_Off");
                    attr.Value = bFPS.ToString();
                    e0.Attributes.Append(attr);
                }

                {
                    XmlAttribute attr;
                    attr = xmlDoc.CreateAttribute("mode");
                    attr.Value = FPSmode.ToString();
                    e0.Attributes.Append(attr);
                }
            }
        }

        {
            XmlNode soundNode = xmlDoc.CreateNode(XmlNodeType.Element, dName[1], string.Empty);
            root.AppendChild(soundNode);

            {
                XmlElement e0 = xmlDoc.CreateElement("BGM");
                soundNode.AppendChild(e0);
                {
                    XmlAttribute attr;
                    attr = xmlDoc.CreateAttribute("On_Off");
                    attr.Value = bBGM.ToString();
                    e0.Attributes.Append(attr);
                }

                {
                    XmlAttribute attr;
                    attr = xmlDoc.CreateAttribute("mode");
                    attr.Value = BGMmode.ToString();
                    e0.Attributes.Append(attr);
                }

                for (int i = 0; i < smCount; i++)
                {
                    XmlElement e1 = xmlDoc.CreateElement("M" + i.ToString());
                    e0.AppendChild(e1);
                    {
                        XmlAttribute attr;
                        attr = xmlDoc.CreateAttribute("volume");
                        attr.Value = vBGM[i].ToString();
                        e1.Attributes.Append(attr);
                    }
                }
            }

            {
                XmlElement e0 = xmlDoc.CreateElement("Effect");
                soundNode.AppendChild(e0);
                {
                    XmlAttribute attr;
                    attr = xmlDoc.CreateAttribute("On_Off");
                    attr.Value = bEffect.ToString();
                    e0.Attributes.Append(attr);
                }

                {
                    XmlAttribute attr;
                    attr = xmlDoc.CreateAttribute("mode");
                    attr.Value = "0";
                    e0.Attributes.Append(attr);
                }

                for (int i = 0; i < 1; i++)
                {
                    XmlElement e1 = xmlDoc.CreateElement("M" + i.ToString());
                    e0.AppendChild(e1);
                    {
                        XmlAttribute attr;
                        attr = xmlDoc.CreateAttribute("volume");
                        attr.Value = vEffect.ToString();
                        e1.Attributes.Append(attr);
                    }
                }
            }
        }

        {
            XmlNode cameraNode = xmlDoc.CreateNode(XmlNodeType.Element, dName[2], string.Empty);
            root.AppendChild(cameraNode);

            {
                XmlElement e0 = xmlDoc.CreateElement("MovePlane");
                cameraNode.AppendChild(e0);
                {
                    XmlAttribute attr;
                    attr = xmlDoc.CreateAttribute("pace");
                    attr.Value = camPacePlane.ToString();
                    e0.Attributes.Append(attr);
                }

                {
                    XmlAttribute attr;
                    attr = xmlDoc.CreateAttribute("useMouse");
                    attr.Value = camUseMouseForMove.ToString();
                    e0.Attributes.Append(attr);
                }
            }

            {
                XmlElement e0 = xmlDoc.CreateElement("Zoom");
                cameraNode.AppendChild(e0);
                {
                    XmlAttribute attr;
                    attr = xmlDoc.CreateAttribute("pace");
                    attr.Value = camPaceZoom.ToString();
                    e0.Attributes.Append(attr);
                }
            }
        }

        xmlDoc.Save(path);
    }
    
    public void SaveXml()
    {
        {
            toData();
        }

        xmlDoc.Load(path);

        {
            XmlAttribute attr = xmlDoc.SelectSingleNode("GameSetting").Attributes["bSaved"];

            if (!XmlConvert.ToBoolean(attr.Value))
            {
                attr.Value = "true";
            }
        }       

        //Graphcis
        {
            XmlNode graphicsNode = xmlDoc.SelectSingleNode("GameSetting/" + dName[0]);

            {
                XmlNode e0 = graphicsNode.ChildNodes[0];
                {
                    XmlAttributeCollection aCol = e0.Attributes;
                    aCol[0].Value = bFPS.ToString();
                    aCol[1].Value = FPSmode.ToString();
                }
            }
        }

        //Sound
        {
            XmlNode soundNode = xmlDoc.SelectSingleNode("GameSetting/" + dName[1]);

            {
                XmlNode e0 = soundNode.ChildNodes[0];
                {
                    XmlAttributeCollection aCol = e0.Attributes;
                    aCol[0].Value = bBGM.ToString();
                    aCol[1].Value = BGMmode.ToString();
                }


                for (int i = 0; i < smCount; i++)
                {
                    XmlNode e1 = e0.ChildNodes[i];
                    {
                        XmlAttributeCollection aCol = e1.Attributes;
                        aCol[0].Value = vBGM[i].ToString();
                    }
                }
            }

            {
                XmlNode e0 = soundNode.ChildNodes[1];
                {
                    XmlAttributeCollection aCol = e0.Attributes;
                    aCol[0].Value = bEffect.ToString();
                    aCol[1].Value = "0";
                }

                for (int i = 0; i < 1; i++)
                {
                    XmlNode e1 = e0.ChildNodes[i];
                    {
                        XmlAttributeCollection aCol = e1.Attributes;
                        aCol[0].Value = vEffect.ToString();
                    }
                }
            }            
        }

        //Camera
        {
            XmlNode cameraNode = xmlDoc.SelectSingleNode("GameSetting/" + dName[2]);

            {
                XmlNode e0 = cameraNode.ChildNodes[0];
                {
                    XmlAttributeCollection aCol = e0.Attributes;
                    aCol[0].Value = camPacePlane.ToString();
                    aCol[1].Value = camUseMouseForMove.ToString();
                }
            }

            {
                XmlNode e0 = cameraNode.ChildNodes[1];
                {
                    XmlAttributeCollection aCol = e0.Attributes;
                    aCol[0].Value = camPaceZoom.ToString();
                }
            }
        }

        xmlDoc.Save(path);
    }
    
    void LoadXml()
    {
        xmlDoc.Load(path);        

        //Graphics
        {
            XmlNode graphicsNode = xmlDoc.SelectSingleNode("GameSetting/" + dName[0]);

            {
                XmlNode e0 = graphicsNode.ChildNodes[0];
                {
                    XmlAttributeCollection aCol = e0.Attributes;
                    bFPS = XmlConvert.ToSingle(aCol[0].Value);
                    FPSmode = XmlConvert.ToSingle(aCol[1].Value);
                }
            }
        }

        //Sound
        {
            XmlNode soundNode = xmlDoc.SelectSingleNode("GameSetting/" + dName[1]);

            {
                XmlNode e0 = soundNode.ChildNodes[0];
                {
                    XmlAttributeCollection aCol = e0.Attributes;                    
                    bBGM = XmlConvert.ToSingle(aCol[0].Value);
                    BGMmode = XmlConvert.ToSingle(aCol[1].Value);
                }

                for (int i = 0; i < smCount; i++)
                {
                    XmlNode e1 = e0.ChildNodes[i];
                    {
                        XmlAttributeCollection aCol = e1.Attributes;                       
                        vBGM[i] = XmlConvert.ToSingle(aCol[0].Value);
                    }
                }
            }

            {
                XmlNode e0 = soundNode.ChildNodes[1];
                {
                    XmlAttributeCollection aCol = e0.Attributes;                    
                    bEffect = XmlConvert.ToSingle(aCol[0].Value);                    
                }

                for (int i = 0; i < 1; i++)
                {
                    XmlNode e1 = e0.ChildNodes[i];
                    {
                        XmlAttributeCollection aCol = e1.Attributes;
                        vEffect = XmlConvert.ToSingle(aCol[0].Value);
                    }
                }
            }
        }

        //Camera
        {
            XmlNode cameraNode = xmlDoc.SelectSingleNode("GameSetting/" + dName[2]);

            {
                XmlNode e0 = cameraNode.ChildNodes[0];
                {
                    XmlAttributeCollection aCol = e0.Attributes;                    
                    camPacePlane = XmlConvert.ToSingle(aCol[0].Value);
                    camUseMouseForMove = XmlConvert.ToSingle(aCol[1].Value);
                }
            }

            {
                XmlNode e0 = cameraNode.ChildNodes[1];
                {
                    XmlAttributeCollection aCol = e0.Attributes;                    
                    camPaceZoom = XmlConvert.ToSingle(aCol[0].Value);
                }
            }
        }

        {
            fromData();
        }
    }
}
