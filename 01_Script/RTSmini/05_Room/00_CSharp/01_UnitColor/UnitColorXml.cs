using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;

using UnityEngine;
using UnityEngine.SceneManagement;

public class UnitColorXml : MonoBehaviour
{
    XmlDocument xmlDoc;
    string directory;
    string path;
    string fileName = "UnitColor.xml";

    int uCount = 4;
    int[] cCount;    
    string[] uName;
    string[] cName;

    string[] ctName;
    int[][] ccCount;

    public UnitManager[] unitMan;
    public ArrowManager arrowMan;

    Color[][][] data;
    Color[][][] dataDef;
   
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
        //Debug.Log(directory);
#endif
        path = directory + fileName;

        {
            cCount = new int[uCount];
            cCount[0] = 9;
            cCount[1] = 8;
            cCount[2] = 8;
            cCount[3] = 5;
        }       

        {
            uName = new string[uCount];
            uName[0] = "Archer";
            uName[1] = "Knight";
            uName[2] = "Viking";
            uName[3] = "Giant";
        }

        {           
            cName = new string[4];
            cName[0] = "r";
            cName[1] = "g";
            cName[2] = "b";
            cName[3] = "a";
        }

        {
            ctName = new string[3];
            ctName[0] = "Skinned";
            ctName[1] = "Static";
            ctName[2] = "Arrow";
        }

        {
            ccCount = new int[uCount][];
            for (int i = 0; i < uCount; i++)
            {
                ccCount[i] = new int[3];
            }         
        }        

        ////
        dataDef = new Color[uCount][][];

        for (int i = 0; i < uCount; i++)
        {
            dataDef[i] = new Color[3][];

            dataDef[i][0] = unitMan[i].skmb.skColors.ToArray();
            dataDef[i][1] = unitMan[i].skmb.stColors.ToArray();
            dataDef[i][2] = ArrowManager.aColor;

            //ccCount[i][0] = unitMan[i].skColor.Length;
            //ccCount[i][1] = unitMan[i].stColor.Length;
        }       

        data = new Color[uCount][][];

        for (int i = 0; i < uCount; i++)
        {
            data[i] = new Color[3][];

            data[i][0] = unitMan[i].skColor;
            data[i][1] = unitMan[i].stColor;
            data[i][2] = ArrowManager.aColor;

            ccCount[i][0] = unitMan[i].skColor.Length;
            ccCount[i][1] = unitMan[i].stColor.Length;
        }

        {
            //ccCount[0][2] = 1;
            ccCount[0][2] = 1;

            ccCount[1][2] = 0;

            ccCount[2][2] = 0;

            ccCount[3][2] = 0;
        }

        if (!File.Exists(path))
        {
            CreateXml();

            for (int i = 0; i < uCount; i++)
            {
                unitMan[i].ChangeColorSk();
                unitMan[i].ChangeColorSt();
            }
        }
        else
        {
            LoadXml();

            for (int i = 0; i < uCount; i++)
            {
                unitMan[i].ChangeColorSk();
                unitMan[i].ChangeColorSt();
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
        //Debug.Log("Application.quitting : UnitColorXml.SaveXml()");
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


    private void OnEnable()
    {

    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //if(Input.GetKeyDown(KeyCode.Z))
        //{
        //    CreateXml();
        //    Debug.Log("CreateXml()");
        //}
        //
        //if(Input.GetKeyDown(KeyCode.X))
        //{
        //    SaveXml();
        //    Debug.Log("SaveXml()");
        //}
    }
  
    void CreateXml()
    {      
        XmlDeclaration xmlDec = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", "yes");
        xmlDoc.AppendChild(xmlDec);

        XmlNode root = xmlDoc.CreateNode(XmlNodeType.Element, "UnitColor", string.Empty);
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

            for (int j = 0; j < 3; j++)
            {
                XmlElement e0 = xmlDoc.CreateElement(ctName[j]);
                unitNode.AppendChild(e0);
                {
                    XmlAttribute attr;
                    attr = xmlDoc.CreateAttribute("count");
                    attr.Value = ccCount[i][j].ToString();
                    e0.Attributes.Append(attr);
                }

                for (int k = 0; k < ccCount[i][j]; k++)
                {
                    XmlElement e1 = xmlDoc.CreateElement("c" + k.ToString());
                    e0.AppendChild(e1);
                    {
                        XmlAttribute attr;

                        for (int l = 0; l < 4; l++)
                        {
                            attr = xmlDoc.CreateAttribute(cName[l]);
                            //attr.Value = iColor[l].ToString();
                            data[i][j][k][l] = dataDef[i][j][k][l];
                            attr.Value = dataDef[i][j][k][l].ToString();
                            e1.Attributes.Append(attr);
                        }
                    }
                }
            }
        }

        xmlDoc.Save(path);
    }

    public void SaveXml()
    {        
        xmlDoc.Load(path);

        {
            XmlAttribute attr = xmlDoc.SelectSingleNode("UnitColor").Attributes["bSaved"];

            if (!XmlConvert.ToBoolean(attr.Value))
            {
                attr.Value = "true";
            }
        }

        for (int i = 0; i < uCount; i++)
        {
            XmlNode unitNode = xmlDoc.SelectSingleNode("UnitColor/" + uName[i]);

            for (int j = 0; j < 3; j++)
            {
                XmlNode e0 = unitNode.ChildNodes[j];
                //XmlAttributeCollection aCol = e0.Attributes;

                for (int k = 0; k < ccCount[i][j]; k++)
                {
                    XmlNode e1 = e0.ChildNodes[k];
                    XmlAttributeCollection aCol = e1.Attributes;
                    for (int l = 0; l < 4; l++)
                    {
                        aCol[l].Value = data[i][j][k][l].ToString();
                    }
                }
            }
        }

        xmlDoc.Save(path);
    }

    public void LoadXml()
    {       
        xmlDoc.Load(path);

        for (int i = 0; i < uCount; i++)
        {
            XmlNode unitNode = xmlDoc.SelectSingleNode("UnitColor/" + uName[i]);

            for (int j = 0; j < 3; j++)
            {
                XmlNode e0 = unitNode.ChildNodes[j];
                //XmlAttributeCollection aCol = e0.Attributes;

                for (int k = 0; k < ccCount[i][j]; k++)
                {
                    XmlNode e1 = e0.ChildNodes[k];
                    XmlAttributeCollection aCol = e1.Attributes;
                    for (int l = 0; l < 4; l++)
                    {
                        data[i][j][k][l] = XmlConvert.ToSingle(aCol[l].Value);
                    }
                }
            }
        }       
    }


    public bool bSaved
    {
        get
        {
            xmlDoc.Load(path);

            XmlAttribute attr = xmlDoc.SelectSingleNode("UnitColor").Attributes["bSaved"];
            return XmlConvert.ToBoolean(attr.Value);
        }
    }
}
