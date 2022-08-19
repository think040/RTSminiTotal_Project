using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;

using UnityEngine;
using UnityEngine.SceneManagement;

public class UnitDataXml : MonoBehaviour
{
    XmlDocument xmlDoc;
    string directory;
    string path;
    string fileName = "UnitData.xml";

    int uCount = 4;
    string[] uName;

    int tCount = 5;
    string[] tName;
    float[][] data;

    public UnitManager[] unitMan;
    public ArrowManager arrowMan;
    //public HPbarMan hpbarMan;
    //public SelectMan selectMan;

    void Awake()
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
#endif
        path = directory + fileName;

        {
            uName = new string[uCount];
            uName[0] = "Archer";
            uName[1] = "Knight";
            uName[2] = "Viking";
            uName[3] = "Giant";
        }

        {
            tName = new string[tCount];
            tName[0] = "maxHp";
            tName[1] = "hitHp";
            tName[2] = "healHp";
            tName[3] = "viewRadius";
            tName[4] = "attackRadius";
        }

        {
            data = new float[uCount][];
            for (int i = 0; i < uCount; i++)
            {
                data[i] = new float[tCount];
            }
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
        //Debug.Log("Application.quitting : UnitDataXml.SaveXml()");
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

    void OnEnable()
    {

    }

    // Start is called before the first frame update
    void Start()
    {
       
    }

    public void toData()
    {
        for (int i = 0; i < uCount; i++)
        {
            data[i][0] = GameManager.maxHp[i];
            data[i][1] = GameManager.hitHp[i];
            data[i][2] = GameManager.healHp[i];
            data[i][3] = GameManager.viewRadius[i];
            data[i][4] = GameManager.attackRadius[i];
        }
    }

    public void fromData()
    {
        for (int i = 0; i < uCount; i++)
        {
            GameManager.maxHp[i] = data[i][0];
            GameManager.hitHp[i] = data[i][1];
            GameManager.healHp[i] = data[i][2];
            GameManager.viewRadius[i] = data[i][3];
            GameManager.attackRadius[i] = data[i][4];
        }
    }

    public void toDataDef()
    {
        for (int i = 0; i < uCount; i++)
        {
            data[i][0] = GameManager.maxHp_def[i];
            data[i][1] = GameManager.hitHp_def[i];
            data[i][2] = GameManager.healHp_def[i];
            data[i][3] = GameManager.viewRadius_def[i];
            data[i][4] = GameManager.attackRadius_def[i];           
        }
    }

    void CreateXml()
    {
        toDataDef();

        XmlDeclaration xmlDec = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", "yes");
        xmlDoc.AppendChild(xmlDec);

        XmlNode root = xmlDoc.CreateNode(XmlNodeType.Element, "UnitData", string.Empty);
        xmlDoc.AppendChild(root);
        {
            XmlAttribute attr = xmlDoc.CreateAttribute("bSaved");
            attr.Value = "false";
            root.Attributes.Append(attr);
        }

        for (int i = 0; i < uCount; i++)
        {
            XmlNode unitNode = xmlDoc.CreateNode(XmlNodeType.Element, uName[i], string.Empty);
            root.AppendChild(unitNode);

            for (int j = 0; j < tCount; j++)
            {
                XmlElement e0 = xmlDoc.CreateElement(tName[j]);
                unitNode.AppendChild(e0);

                e0.InnerText = data[i][j].ToString();
            }
        }

        xmlDoc.Save(path);
    }

    public void SaveXml()
    {
        toData();

        xmlDoc.Load(path);

        {
            XmlAttribute attr = xmlDoc.SelectSingleNode("UnitData").Attributes["bSaved"];

            if (!XmlConvert.ToBoolean(attr.Value))
            {
                attr.Value = "true";
            }
        }

        for (int i = 0; i < uCount; i++)
        {
            XmlNode unitNode = xmlDoc.SelectSingleNode("UnitData/" + uName[i]);

            for (int j = 0; j < tCount; j++)
            {
                XmlNode e0 = unitNode.ChildNodes[j];
                e0.InnerText = data[i][j].ToString();

                //float d0 = 1234.567f;
                //e0.InnerText = d0.ToString();
            }
        }

        xmlDoc.Save(path);
    }

    void LoadXml()
    {
        xmlDoc.Load(path);

        for (int i = 0; i < uCount; i++)
        {
            XmlNode unitNode = xmlDoc.SelectSingleNode("UnitData/" + uName[i]);

            for (int j = 0; j < tCount; j++)
            {
                XmlNode e0 = unitNode.ChildNodes[j];
                data[i][j] = XmlConvert.ToSingle(e0.InnerText);

                //float d0 = 1234.567f;
                //e0.InnerText = d0.ToString();
            }
        }


        {
            fromData();

            //for (int i = 0; i < uCount; i++)
            //{
            //    unitMan[i].ApplyUnitData();
            //}
            //
            //arrowMan.ApplyHitHp();
            //hpbarMan.ApplyData();
            //selectMan.ApplyViewRadius();
        }
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.I))
        //{
        //    CreateXml();
        //    Debug.Log("CreateXml()");
        //}
        //
        //if (Input.GetKeyDown(KeyCode.O))
        //{
        //    SaveXml();
        //    Debug.Log("SaveXml()");
        //}
        //
        //if (Input.GetKeyDown(KeyCode.P))
        //{
        //    LoadXml();
        //    Debug.Log("LoadXml()");
        //}
    }
}
