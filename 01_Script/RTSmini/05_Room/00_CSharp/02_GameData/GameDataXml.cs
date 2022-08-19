using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameDataXml : MonoBehaviour
{
    XmlDocument xmlDoc;
    string directory;
    string path;
    string fileName = "GameData.xml";

    string[] dName;
    int dCount = 2;

    string[] uName;
    int uCount = 4;

    
    public Text textDirectory;

    public bool inRoom = false;

    void Awake()
    {
        //if(textDirectory != null)
        if(inRoom)
        {
            Init();
        }
    }

    private void Update()
    {
        
    }


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
            dName[0] = "UnitCount";
            dName[1] = "Time";
            //dName[2] = "Graphic";
            //dName[3] = "Sound";
        }

        {
            uName = new string[uCount];
            uName[0] = "Archer";
            uName[1] = "Knight";
            uName[2] = "Viking";
            uName[3] = "Giant";
        }


        if(!GameManager.useEditor)
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
        
        if(textDirectory != null)
        {
            textDirectory.text = directory;
        }

        {
            //OnConstruct();
        }
    }

    private void OnApplicationQuit()
    {
        if(xmlDoc != null)
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
        //Debug.Log("Application.quitting : GameDataXml.SaveXml()");
    }

    //void OnSceneLeft(Scene scene)
    //{
    //    OnDestroy();
    //}
    //
    //void OnConstruct()
    //{
    //    {
    //        SceneManager.sceneUnloaded += OnSceneLeft;
    //    }
    //
    //    {
    //        Application.quitting += OnAppQuit;
    //    }
    //}
    //
    //void OnDestroy()
    //{
    //    {
    //        SceneManager.sceneUnloaded -= OnSceneLeft;
    //    }
    //
    //    {
    //        Application.quitting -= OnAppQuit;
    //    }
    //}

    

    public void Begin()
    {

    }

    void CreateXml()
    {        
        XmlDeclaration xmlDec = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", "yes");
        xmlDoc.AppendChild(xmlDec);

        XmlNode root = xmlDoc.CreateNode(XmlNodeType.Element, "GameData", string.Empty);
        xmlDoc.AppendChild(root);
        {
            XmlAttribute attr = xmlDoc.CreateAttribute("bSaved");
            attr.Value = "false";
            root.Attributes.Append(attr);
        }
        
        {
            XmlNode countNode = xmlDoc.CreateNode(XmlNodeType.Element, dName[0], string.Empty);
            root.AppendChild(countNode);

            for (int j = 0; j < uCount; j++)
            {
                XmlElement e0 = xmlDoc.CreateElement(uName[j]);
                countNode.AppendChild(e0);
                
                e0.InnerText = GameManager.unitCounts_def[j].ToString();
            }
        }

        {
            XmlNode timeNode = xmlDoc.CreateNode(XmlNodeType.Element, dName[1], string.Empty);
            root.AppendChild(timeNode);

            {
                XmlElement e0 = xmlDoc.CreateElement("min");
                timeNode.AppendChild(e0);
               
                e0.InnerText = GameTimer.inMin.ToString();
            }

            {
                XmlElement e0 = xmlDoc.CreateElement("sec");
                timeNode.AppendChild(e0);
               
                e0.InnerText = GameTimer.inSec.ToString();
            }

        }

        xmlDoc.Save(path);
    }
   
    public void SaveXml()
    {       
        xmlDoc.Load(path);

        {
            XmlAttribute attr = xmlDoc.SelectSingleNode("GameData").Attributes["bSaved"];

            if (!XmlConvert.ToBoolean(attr.Value))
            {
                attr.Value = "true";
            }
        }
        
        {
            XmlNode countNode = xmlDoc.SelectSingleNode("GameData/" + dName[0]);

            for (int j = 0; j < uCount; j++)
            {
                XmlNode e0 = countNode.ChildNodes[j];
                e0.InnerText = GameManager.unitCounts[j].ToString();               
            }
        }

        {
            XmlNode timeNode = xmlDoc.SelectSingleNode("GameData/" + dName[1]);

            {
                XmlNode e0 = timeNode.ChildNodes[0];             
                e0.InnerText = GameTimer.inMin.ToString();
            }

            {
                XmlNode e0 = timeNode.ChildNodes[1];               
                e0.InnerText = GameTimer.inSec.ToString();
            }
        }

        xmlDoc.Save(path);                    
    }

    void LoadXml()
    {      
        xmlDoc.Load(path);

        {
            XmlNode countNode = xmlDoc.SelectSingleNode("GameData/" + dName[0]);

            for (int j = 0; j < uCount; j++)
            {
                XmlNode e0 = countNode.ChildNodes[j];
                GameManager.unitCounts[j] = XmlConvert.ToInt32(e0.InnerText);
            }
        }

        {
            XmlNode timeNode = xmlDoc.SelectSingleNode("GameData/" + dName[1]);

            {
                XmlNode e0 = timeNode.ChildNodes[0];               
                GameTimer.inMin = XmlConvert.ToInt32(e0.InnerText);
            }

            {
                XmlNode e0 = timeNode.ChildNodes[1];
                GameTimer.inSec = XmlConvert.ToInt32(e0.InnerText);
            }
        }
    }


    //Test
    int smCount = 4;

    void CreateXml1()
    {
        XmlDeclaration xmlDec = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", "yes");
        xmlDoc.AppendChild(xmlDec);

        XmlNode root = xmlDoc.CreateNode(XmlNodeType.Element, "GameData", string.Empty);
        xmlDoc.AppendChild(root);
        {
            XmlAttribute attr = xmlDoc.CreateAttribute("bSaved");
            attr.Value = "false";
            root.Attributes.Append(attr);
        }

        {
            XmlNode countNode = xmlDoc.CreateNode(XmlNodeType.Element, dName[0], string.Empty);
            root.AppendChild(countNode);

            for (int j = 0; j < uCount; j++)
            {
                XmlElement e0 = xmlDoc.CreateElement(uName[j]);
                countNode.AppendChild(e0);

                e0.InnerText = GameManager.unitCounts_def[j].ToString();
            }
        }

        {
            XmlNode timeNode = xmlDoc.CreateNode(XmlNodeType.Element, dName[1], string.Empty);
            root.AppendChild(timeNode);

            {
                XmlElement e0 = xmlDoc.CreateElement("min");
                timeNode.AppendChild(e0);

                e0.InnerText = "0";
            }

            {
                XmlElement e0 = xmlDoc.CreateElement("sec");
                timeNode.AppendChild(e0);

                e0.InnerText = "0";
            }
        }

        {
            XmlNode graphicsNode = xmlDoc.CreateNode(XmlNodeType.Element, dName[2], string.Empty);
            root.AppendChild(graphicsNode);

            {
                XmlElement e0 = xmlDoc.CreateElement("FPS");
                graphicsNode.AppendChild(e0);
                {
                    XmlAttribute attr;
                    attr = xmlDoc.CreateAttribute("On_Off");
                    attr.Value = "On";
                    e0.Attributes.Append(attr);
                }

                {
                    XmlAttribute attr;
                    attr = xmlDoc.CreateAttribute("mode");
                    attr.Value = "0";
                    e0.Attributes.Append(attr);
                }
            }
        }

        {
            XmlNode soundNode = xmlDoc.CreateNode(XmlNodeType.Element, dName[3], string.Empty);
            root.AppendChild(soundNode);

            {
                XmlElement e0 = xmlDoc.CreateElement("BGM");
                soundNode.AppendChild(e0);
                {
                    XmlAttribute attr;
                    attr = xmlDoc.CreateAttribute("On_Off");
                    attr.Value = "On";
                    e0.Attributes.Append(attr);
                }

                {
                    XmlAttribute attr;
                    attr = xmlDoc.CreateAttribute("mode");
                    attr.Value = "0";
                    e0.Attributes.Append(attr);
                }

                for (int i = 0; i < smCount; i++)
                {
                    XmlElement e1 = xmlDoc.CreateElement("M" + i.ToString());
                    e0.AppendChild(e1);
                    {
                        XmlAttribute attr;
                        attr = xmlDoc.CreateAttribute("volume");
                        attr.Value = "0";
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
                    attr.Value = "On";
                    e0.Attributes.Append(attr);
                }
            }

        }

        xmlDoc.Save(path);
    }

    public void SaveXml1()
    {
        xmlDoc.Load(path);

        {
            XmlAttribute attr = xmlDoc.SelectSingleNode("GameData").Attributes["bSaved"];

            if (!XmlConvert.ToBoolean(attr.Value))
            {
                attr.Value = "true";
            }
        }

        {
            XmlNode countNode = xmlDoc.SelectSingleNode("GameData/" + dName[0]);

            for (int j = 0; j < uCount; j++)
            {
                XmlNode e0 = countNode.ChildNodes[j];
                e0.InnerText = GameManager.unitCounts[j].ToString();
            }
        }

        {
            XmlNode timeNode = xmlDoc.SelectSingleNode("GameData/" + dName[1]);

            {
                XmlNode e0 = timeNode.ChildNodes[0];
                e0.InnerText = "0";
            }

            {
                XmlNode e0 = timeNode.ChildNodes[1];
                e0.InnerText = "0";
            }
        }

        {
            XmlNode graphicsNode = xmlDoc.SelectSingleNode("GameData/" + dName[2]);
        
            {
                XmlNode e0 = graphicsNode.ChildNodes[0];
                {
                    XmlAttributeCollection aCol = e0.Attributes;
                    aCol[0].Value = "On";
                    aCol[1].Value = "0";
                }              
            }                    
        }

        {
            XmlNode soundNode = xmlDoc.SelectSingleNode("GameData/" + dName[3]);

            {
                XmlNode e0 = soundNode.ChildNodes[0];
                {
                    XmlAttributeCollection aCol = e0.Attributes;
                    aCol[0].Value = "On";
                    aCol[1].Value = "0";
                }
               

                for (int i = 0; i < smCount; i++)
                {
                    XmlNode e1 = e0.ChildNodes[i];                   
                    {
                        XmlAttributeCollection aCol = e0.Attributes;
                        aCol[0].Value = "0";
                    }
                }
            }

            {
                XmlNode e0 = soundNode.ChildNodes[1];
                {
                    XmlAttributeCollection aCol = e0.Attributes;
                    aCol[0].Value = "On";                   
                }
            }
        }

        xmlDoc.Save(path);
    }
   
    void LoadXml1()
    {
        xmlDoc.Load(path);

        {
            XmlNode countNode = xmlDoc.SelectSingleNode("GameData/" + dName[0]);

            for (int j = 0; j < uCount; j++)
            {
                XmlNode e0 = countNode.ChildNodes[j];
                GameManager.unitCounts[j] = XmlConvert.ToInt32(e0.InnerText);
            }
        }

        {
            XmlNode timeNode = xmlDoc.SelectSingleNode("GameData/" + dName[1]);

            {
                XmlNode e0 = timeNode.ChildNodes[0];
            }

            {
                XmlNode e0 = timeNode.ChildNodes[1];
            }
        }

        {
            XmlNode graphicsNode = xmlDoc.SelectSingleNode("GameData/" + dName[2]);

            {
                XmlNode e0 = graphicsNode.ChildNodes[0];
                {
                    XmlAttributeCollection aCol = e0.Attributes;
                    //aCol[0].Value = "On";
                    //aCol[1].Value = "0";
                }
            }
        }

        {
            XmlNode soundNode = xmlDoc.SelectSingleNode("GameData/" + dName[3]);

            {
                XmlNode e0 = soundNode.ChildNodes[0];
                {
                    XmlAttributeCollection aCol = e0.Attributes;
                    //aCol[0].Value = "On";
                    //aCol[1].Value = "0";
                }


                for (int i = 0; i < smCount; i++)
                {
                    XmlNode e1 = e0.ChildNodes[i];
                    {
                        XmlAttributeCollection aCol = e0.Attributes;
                        //aCol[0].Value = "0";
                    }
                }
            }

            {
                XmlNode e0 = soundNode.ChildNodes[1];
                {
                    XmlAttributeCollection aCol = e0.Attributes;
                    //aCol[0].Value = "On";
                }
            }
        }
    }
}
