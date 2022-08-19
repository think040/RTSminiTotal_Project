using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

using Unity.Mathematics;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Jobs;

using UserAnimSpace;


public class UnitManager : MonoBehaviour
{
    public bool bRender = true;
    public int count;
    public GameObject model;
    public string modelName;
    public SkinMeshBones skmb;
    public UserAnimClip[] clips;

    public Transform rootTr;
    public Shader gshader;
    public ComputeShader cshader;

    public Transform trLight;

    Material skMte;
    MaterialPropertyBlock skMpb;    

    Material stMte;
    MaterialPropertyBlock stMpb;

    float4x4 rootMat;

    string skName;
    Mesh skMesh;
    string[] boneNames;
    BoneNode[] rootBns;
    protected BoneNode[][] bns0;
    BoneNode[][] bns1;
    BoneNode[][] skBns;

    NativeArray<BoneTransform>[] nas;
    WfromLJob[] jobs;
    JobHandle[] handles;

    protected Dictionary<string, BoneCurve[]> dicCurves;

    //public float4x4[] rootW;

    float4x4[] rootM;
    float4x4[] bindpose;
    float4x4[] finalW;

    //VertexCompute vtxCompute;

    public int stCount;
    public string[] stNames;
    Mesh[] stMeshes;
    BoneNode[][] bnsSt;
    public float4x4[][] trM;

    public GameObject[] units;
    public UnitActor[] unitActors;

    //public Scene playScene;    

    public float animSpeed = 1.0f;

    public virtual void Init()
    {

        count = GameManager.ic[unitIdx].y;


        if (count > 0)
        {
            InitData();
            InitBone();
            InitSkinMeshShader();
            InitStaticMeshShader();
            InitRendering();
            InitArray();


            RenderGOM.BeginFrameRender += BeginFrameRender;

            SceneManager.sceneUnloaded += OnSceneLeft;
        }
                        
    }
   

    public virtual void InitInRoom()
    {
        //count = 1;
        count = GameManager.ic[unitIdx].y;

        if (count > 0)
        {
            InitData();
            InitBone();
            InitSkinMeshShader();
            InitStaticMeshShader();
            InitRendering();
            InitArray();
    
    
            RenderGOM.BeginFrameRender += BeginFrameRender;
    
            SceneManager.sceneUnloaded += OnSceneLeft;
        }            
    }

    public Transform trShootPos;
    public string strShootPos;
    public Transform[] trsShootPos;
    public Transform[] trsHitCollider;  

    public void Spawn<T>() where T : UnitActor
    {
        if (count > 0)
        {
            if (trShootPos != null)
            {
                strShootPos = trShootPos.name;
            }

            for (int i = 0; i < count; i++)
            {
                units[i] = GameObject.Instantiate(model, float3.zero, quaternion.identity);

                trs[i] = units[i].transform;
                anims[i] = units[i].GetComponent<UserAnimation>();
                //anims[i].Init(bns0[i], dicCurves);
                anims[i].Init1(bns0[i], dicCurves, i);

                //units[i].SetActive(true);
                unitActors[i] = units[i].GetComponent<UnitActor>();
                unitActors[i].unitIdx = unitIdx;
                unitActors[i].iid = i;
                unitActors[i].offsetIdx = offsetIdx;
                (unitActors[i] as T).Init(stNames, trM[i], hasStMesh);


                trsShootPos[i] = trs[i].Find(strShootPos);
                trsHitCollider[i] = trsShootPos[i].Find("HitCollider");
            }

            InitDebugRendering();
            SendDataToGameManager();

            {
                InitAnim();
                BakeAnimation();
            }
        }
    }

    public void SpawnInRoom<T>() where T : UnitActor
    {
        if (count > 0)
        {
            if (trShootPos != null)
            {
                strShootPos = trShootPos.name;
            }

            for (int i = 0; i < count; i++)
            {
                units[i] = GameObject.Instantiate(model, float3.zero, quaternion.identity);

                trs[i] = units[i].transform;
                anims[i] = units[i].GetComponent<UserAnimation>();
                anims[i].Init(bns0[i], dicCurves);

                //units[i].SetActive(true);
                unitActors[i] = units[i].GetComponent<UnitActor>();
                unitActors[i].unitIdx = unitIdx;
                unitActors[i].iid = i;
                unitActors[i].offsetIdx = offsetIdx;
                (unitActors[i] as T).Init(stNames, trM[i], hasStMesh);


                trsShootPos[i] = trs[i].Find(strShootPos);
                trsHitCollider[i] = trsShootPos[i].Find("HitCollider");
            }

            InitDebugRendering();

            //SendDataToGameManager();
        }
    }

    public void OnSceneLeft(Scene scene)
    {        
        OnDestroy();

        //Debug.Log("sceneUnloaded()");
    }

    public int unitIdx;          
    void SendDataToGameManager()
    {        
        int i0 = offsetIdx;
      
        for(int i = 0; i < count; i++)
        {
            GameManager.unitActors[i0 + i] = unitActors[i];
            GameManager.unitTrs[i0 + i] = unitActors[i].transform;
            GameManager.unitCols[i0 + i] = unitActors[i].GetComponent<CapsuleCollider>();
        }
    }

    public void SetFromTargetAction(ComputeBuffer active_Buffer)
    {
        if(count > 0)
        {
            {
                //skMpb.SetBuffer("active_Buffer", active_Buffer);
                skMpb.SetInt("unitOffset", offsetIdx);
            }

            if (hasStMesh)
            {
                //stMpb.SetBuffer("active_Buffer", active_Buffer);
                stMpb.SetInt("unitOffset", offsetIdx);
            }
        }

    }

    public int offsetIdx
    {
        get { return GameManager.ic[unitIdx].x; }
    }
   

    public virtual void Begin()
    {
        for(int i = 0; i < count; i++)
        {
            unitActors[i].Begin();
        }
    }

    int cullIdx;

    int cullOffset
    {
        get
        {
            return CullManager.cullOffsets[cullIdx];
        }
    }

    public void SetCullData(int cullIdx)
    {
        if(count > 0)
        {
            this.cullIdx = cullIdx;

            {
                skMpb.SetInt("cullOffset", cullOffset);
                //skMpb.SetTexture("", );
            }

            if (hasStMesh)
            {
                stMpb.SetInt("cullOffset", cullOffset);
                //stMpb.SetTexture("", );
            }
        }        
    }

    public void SetCullData(int cullIdx, RenderTexture pvf_tex, RenderTexture ovf_tex)
    {
        if (count > 0)
        {
            this.cullIdx = cullIdx;
                        
            {
                skMpb.SetInt("cullOffset", cullOffset);
                skMpb.SetTexture("cullResult_pvf_Texture", pvf_tex);
                skMpb.SetTexture("cullResult_ovf_Texture", ovf_tex);
            }

            if (hasStMesh)
            {
                stMpb.SetInt("cullOffset", cullOffset);
                stMpb.SetTexture("cullResult_pvf_Texture", pvf_tex);
                stMpb.SetTexture("cullResult_ovf_Texture", ovf_tex);
            }
        }
    }


    public Transform[] trs;

    [HideInInspector]
    public UserAnimation[] anims;
     
    public static UnitManager manager;

    public void InitArray()
    {
        manager = this;             

        units = new GameObject[count];
        trs = new Transform[count];
        anims = new UserAnimation[count];
        unitActors = new UnitActor[count];
        trsShootPos = new Transform[count];
        trsHitCollider = new Transform[count];
    }
    

    //public void SetArray<T>(int i, GameObject unit) where T : UnitActor
    //{
    //    units[i] = unit;
    //    trs[i] = units[i].transform;
    //    anims[i] = units[i].GetComponent<UserAnimation>();
    //    anims[i].Init(bns0[i], dicCurves);
    //
    //    units[i].SetActive(true);
    //    unitActors[i] = units[i].GetComponent<UnitActor>();
    //    (unitActors[i] as T).Init(stNames, trM[i], hasStMesh);
    //}


    void InitData()
    {
        {
            skName = skmb._skNames[0];
            skMesh = skmb.dicSkMesh[skName];
            boneNames = skmb.dicSkBoneNames[skName].names;
        }

        if (skmb._stMesh.Count > 0)
        {
            hasStMesh = true;
        }

        if (hasStMesh)
        {
            stNames = skmb._stNames.ToArray();
            stMeshes = new Mesh[stNames.Length];
            for (int i = 0; i < stNames.Length; i++)
            {
                stMeshes[i] = skmb.dicStMesh[stNames[i]];
            }
            stCount = stNames.Length;
        }
    }
  

    float4x4[] orthoM;
    int[] boneIdx;   //21
    int[] boneIdx_st; //2
    float4[] boneSca_st;

    int boneCount;   //31
    int frameCount;  //16
    int clipCount;   //3

    int bmCount;     //21

    int[] siblingCount;
    int depthCount;
    int bnCount;
    int[] idxMask;
    int[] idxParent;
    int[] idx;
    int2[] idxMP;

    public BoneNode[] bns;
    void InitBone()
    {
        rootBns = new BoneNode[count];
        for (int i = 0; i < count; i++)
        {
            rootBns[i] = new BoneNode();
        }

        //{
        //    BoneNode.ConstructBones(rootTr, rootBns);
        //    bns0 = BoneNode.ToArrayArray(rootBns, out boneCount);
        //    bns1 = BoneNode.ToArrayArrayIns(new BoneNode[] { rootBns[0] });
        //}

        {
            BoneNode.ConstructBones(rootTr, rootBns);
            bns0 = BoneNode.ToArrayArrayForCompute(rootBns);
            boneCount = bns0[0].Length;

            BoneNode.ToArrayForCompute(rootBns[0], out siblingCount, out depthCount, out bnCount, out idxMask, out idxParent, out idx, out idxMP);
        }

        {
            dicCurves = new Dictionary<string, BoneCurve[]>();
            BoneNode.BindBoneInfo(bns0[0]);
            for (int i = 0; i < clips.Length; i++)
            {
                string name = clips[i].name;
                BoneNode.BindBoneCurve(bns0[0], name, clips[i].curves);
                dicCurves[name] = BoneNode.ToBoneCurves(bns0[0], name);
            }

            frameCount = BoneCurve.dqc;
            clipCount = dicCurves.Count;
        }

        //{
        //    nas = BoneNode.CreateNa(bns1);
        //    jobs = BoneNode.CreateJob(nas);
        //    handles = BoneNode.CreateHandle(jobs);
        //}

        {
            skBns = new BoneNode[count][];
            for (int i = 0; i < count; i++)
            {
                skBns[i] = BoneNode.FindBones(bns0[i], boneNames, ref boneIdx);
            }

            bmCount = boneNames.Length;
            //rootW = new float4x4[count];
            //rootM = new float4x4[count];
            bindpose = new float4x4[boneNames.Length];
            finalW = new float4x4[count * boneNames.Length];

            for (int i = 0; i < bindpose.Length; i++)
            {
                bindpose[i] = skMesh.bindposes[i];
            }

            //debug
            orthoM = BoneNode.CheckOrthoNormal(bindpose);

        }

        //{
        //    vtxCompute = new VertexCompute();
        //    vtxCompute.Init(cshader, skMesh, finalW, count);
        //}

        if (hasStMesh)
        {

            bnsSt = new BoneNode[count][];
            trM = new float4x4[count][];
            for (int i = 0; i < count; i++)
            {
                bnsSt[i] = BoneNode.FindBones(bns0[i], stNames, ref boneIdx_st);
                trM[i] = new float4x4[bnsSt[i].Length];
                for (int j = 0; j < bnsSt[i].Length; j++)
                {
                    trM[i][j] = new float4x4();
                }
            }

            boneSca_st = new float4[stCount];
            for (int i = 0; i < stCount; i++)
            {
                if (bnsSt[0][i] != null)
                {
                    boneSca_st[i].xyz = bnsSt[0][i].transform.scaL;
                }
            }


            {
                stWCount = stCount * count;
                stW = new float4x4[stWCount];
                for (int i = 0; i < stW.Length; i++)
                {
                    stW[i] = float4x4.zero;
                }

                //vtxComputeSt = new VertexComputeSt();
                //vtxComputeSt.Init(cshader, stMeshes, stW, count);
            }
        }

        {
            rootMat = BoneNode.GetPfromC(rootTr.localPosition, rootTr.localRotation, rootTr.localScale);
        }

    }


    GraphicsBuffer[] idxBuffers;
    GraphicsBuffer vtxStatic;   

    GraphicsBuffer idxBuffer;

    public bool bSkColorChanged = false;
    Color[] skColorDef;
    public Color[] skColor;

    SubMeshDescriptor[] smDescSk;
    int[][] sbIdxSk;
    VertexStatic[] vtxDataSk;  

    void InitSkinMeshShader()
    {
        skMte = new Material(gshader);
        skMpb = new MaterialPropertyBlock();


        SubMeshDescriptor[] smdesc = new SubMeshDescriptor[skMesh.subMeshCount];
        int[][] idxArray = new int[skMesh.subMeshCount][];

        smDescSk = smdesc;
        sbIdxSk = idxArray;
        skColorDef = skmb.skColors.ToArray();
        {
            for (int i = 0; i < smdesc.Length; i++)
            {
                smdesc[i] = skMesh.GetSubMesh(i);
                idxArray[i] = skMesh.GetIndices(i, true);
            }
        }

        {
            List<int> idx = new List<int>();
            for (int i = 0; i < skMesh.subMeshCount; i++)
            {
                for (int j = 0; j < smdesc[i].indexCount; j++)
                {
                    idx.Add(idxArray[i][j] + smdesc[i].baseVertex);
                }
            }

            idxBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Index, idx.Count, sizeof(int));
            idxBuffer.SetData(idx.ToArray());
        }

        {
            VertexStatic[] vtxStaticData = new VertexStatic[skMesh.vertexCount];
            vtxDataSk = vtxStaticData;
            List<Vector2> uvs = new List<Vector2>();
            skMesh.GetUVs(0, uvs);
            vtxStatic = new GraphicsBuffer(GraphicsBuffer.Target.Structured, skMesh.vertexCount, Marshal.SizeOf<VertexStatic>());

            //if (uvs.Count > 0)
            //{
            //    for (int i = 0; i < skMesh.vertexCount; i++)
            //    {
            //        vtxStaticData[i].uv = uvs[i];
            //        vtxStaticData[i].color = new float4(1.0f, 0.0f, 0.0f, 1.0f);
            //    }
            //
            //    skMpb.SetTexture("diffuseTex", diffuseTex);
            //    skMpb.SetInt("useTex", 1);
            //}
            //else

            {
                int count = skmb.skColors.Count;
                skColor = new Color[count];
                for (int i = 0; i < count; i++)
                {
                    skColor[i] = Color.red;
                }
            }


            {
                for (int i = 0; i < skMesh.vertexCount; i++)
                {
                    vtxStaticData[i].uv = new float2(0.0f, 0.0f);
                    vtxStaticData[i].color = new float4(1.0f, 0.0f, 0.0f, 1.0f);
                }

                for (int i = 0; i < skMesh.subMeshCount; i++)
                {
                    for (int j = 0; j < smdesc[i].indexCount; j++)
                    {
                        Vector4 c;
                        if (bSkColorChanged)
                        {
                            c = skColor[i];
                        }
                        else
                        {
                            c = skmb.skColors[i];
                        }
                        vtxStaticData[idxArray[i][j]].color = c;
                    }
                }
            }

            vtxStatic.SetData(vtxStaticData);
            skMpb.SetBuffer("vtxStatic", vtxStatic);
        }

        //{
        //    skMpb.SetBuffer("vtxDynamic", vtxCompute.vOut);
        //    skMpb.SetInt("dvCount", vtxCompute.dvCount);
        //}

    }

    GraphicsBuffer stVtxBuffer;
    GraphicsBuffer stIdxBuffer;
        
    float4x4[] stW;
    int stWCount;

    public bool hasStMesh = true;

    public bool bStColorChanged = false;
    Color[] stColorDef;
    public Color[] stColor;

    //VertexComputeSt vtxComputeSt;

    SubMeshDescriptor[][] smDescSt;
    int[][][] sbIdxSt;
    VertexStatic[] vtxDataSt;   

    void InitStaticMeshShader()
    {
        if (hasStMesh)
        {
            stMte = new Material(gshader);
            stMpb = new MaterialPropertyBlock();

            SubMeshDescriptor[][] stSmdesc = new SubMeshDescriptor[stCount][];
            int[][][] sbIdx = new int[stCount][][];

            sbIdxSt = sbIdx;
            smDescSt = stSmdesc;
            stColorDef = skmb.stColors.ToArray();

            List<VertexStatic> vtxData = new List<VertexStatic>();
            List<int> idxData = new List<int>();
            int vtxCount = 0;
            int idxCount = 0;
            int[] vBase = new int[stCount];
            int[] iCount = new int[stCount];
            int[] iStart = new int[stCount];
            bool hasUv = false;
            for (int i = 0; i < stCount; i++)
            {
                Mesh mesh = stMeshes[i];
                List<Vector2> uv = new List<Vector2>();

                mesh.GetUVs(0, uv);
                vBase[i] = vtxData.Count;
                for (int j = 0; j < mesh.vertexCount; j++)
                {
                    VertexStatic vtx = new VertexStatic();
                    if (uv.Count > 0)
                    {
                        vtx.uv = uv[j];
                        hasUv = true;
                    }
                    vtxData.Add(vtx);
                }
            }


            Color[] colors;

            {
                int count = skmb.stColors.Count;
                stColor = new Color[count];
                for (int i = 0; i < count; i++)
                {
                    stColor[i] = Color.red;
                }
            }

            if (bStColorChanged)
            {
                colors = stColor;
            }
            else
            {
                colors = skmb.stColors.ToArray();
            }


            int offset = 0;
            for (int i = 0; i < stCount; i++)
            {
                Mesh mesh = stMeshes[i];
                //mesh.RecalculateNormals();
                //mesh.RecalculateTangents();

                stSmdesc[i] = new SubMeshDescriptor[mesh.subMeshCount];
                sbIdx[i] = new int[mesh.subMeshCount][];
                iStart[i] = idxData.Count;
                for (int j = 0; j < mesh.subMeshCount; j++)
                {
                    stSmdesc[i][j] = mesh.GetSubMesh(j);
                    sbIdx[i][j] = mesh.GetIndices(j);
                    for (int k = 0; k < sbIdx[i][j].Length; k++)
                    {
                        int idx = vtxCount + stSmdesc[i][j].baseVertex + sbIdx[i][j][k];
                        idxData.Add(idx);

                        VertexStatic v = vtxData[idx];
                        //v.color.xyz = new float4((Vector4)colors[i * mesh.subMeshCount + j]).xyz;                        
                        v.color.xyz = new float4((Vector4)colors[offset + j]).xyz;
                        v.color.w = (float)i;
                        vtxData[idx] = v;
                    }
                }
                offset += mesh.subMeshCount;

                iCount[i] = idxData.Count - iStart[i];
                vtxCount += mesh.vertexCount;
            }
            idxCount = idxData.Count;

            vtxDataSt = vtxData.ToArray();

            stVtxBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, vtxCount, Marshal.SizeOf<VertexStatic>());
            stIdxBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Index, idxCount, sizeof(int));

            stVtxBuffer.SetData(vtxData.ToArray());
            stIdxBuffer.SetData(idxData.ToArray());

            stMpb.SetBuffer("vtxStatic", stVtxBuffer);

            //stMpb.SetBuffer("vtxDynamic", vtxComputeSt.vOut);
            //stMpb.SetInt("dvCount", vtxComputeSt.dvCount);
        }
    }


    public void ChangeColor(bool bDefColor = false)
    {
        //Skinned Mesh
        {
            Color[] colors;

            if (bDefColor)
            {
                colors = skColorDef;
            }
            else
            {
                colors = skColor;
            }

            for (int i = 0; i < skMesh.subMeshCount; i++)
            {
                for (int j = 0; j < smDescSk[i].indexCount; j++)
                {
                    Vector4 c;

                    c = colors[i];
                    vtxDataSk[sbIdxSk[i][j]].color = c;
                }
            }

            vtxStatic.SetData(vtxDataSk);
        }

        //Static Mesh
        {
            Color[] colors;

            if (bDefColor)
            {
                colors = stColorDef;
            }
            else
            {
                colors = stColor;
            }

            int vtxCount = 0;
            int offset = 0;
            for (int i = 0; i < stCount; i++)
            {
                Mesh mesh = stMeshes[i];

                for (int j = 0; j < mesh.subMeshCount; j++)
                {
                    for (int k = 0; k < sbIdxSt[i][j].Length; k++)
                    {
                        int idx = vtxCount + smDescSt[i][j].baseVertex + sbIdxSt[i][j][k];

                        VertexStatic v = vtxDataSt[idx];
                        v.color.xyz = new float4((Vector4)colors[offset + j]).xyz;
                        //v.color.w = (float)i;
                        vtxDataSt[idx] = v;
                    }
                }
                offset += mesh.subMeshCount;
                vtxCount += mesh.vertexCount;
            }

            stVtxBuffer.SetData(vtxDataSt);
        }
    }

    public void ChangeColorSk(bool bDefColor = false)
    {
        //Skinned Mesh
        {
            Color[] colors;

            if (bDefColor)
            {
                colors = skColorDef;
            }
            else
            {
                colors = skColor;
            }

            for (int i = 0; i < skMesh.subMeshCount; i++)
            {
                for (int j = 0; j < smDescSk[i].indexCount; j++)
                {
                    Vector4 c;

                    c = colors[i];
                    vtxDataSk[sbIdxSk[i][j]].color = c;
                }
            }

            vtxStatic.SetData(vtxDataSk);
        }
    }

    public void ChangeColorSt(bool bDefColor = false)
    {
        //Static Mesh
        {
            Color[] colors;

            if (bDefColor)
            {
                colors = stColorDef;
            }
            else
            {
                colors = stColor;
            }

            int vtxCount = 0;
            int offset = 0;
            for (int i = 0; i < stCount; i++)
            {
                Mesh mesh = stMeshes[i];

                for (int j = 0; j < mesh.subMeshCount; j++)
                {
                    for (int k = 0; k < sbIdxSt[i][j].Length; k++)
                    {
                        int idx = vtxCount + smDescSt[i][j].baseVertex + sbIdxSt[i][j][k];

                        VertexStatic v = vtxDataSt[idx];
                        v.color.xyz = new float4((Vector4)colors[offset + j]).xyz;
                        //v.color.w = (float)i;
                        vtxDataSt[idx] = v;
                    }
                }
                offset += mesh.subMeshCount;
                vtxCount += mesh.vertexCount;
            }

            stVtxBuffer.SetData(vtxDataSt);
        }
    }

    CSMAction csmAction;
    int csmCount;
    RenderTexture drenTexs;
    
    int passCSM;
    int passColor;
    int dw;
    int dh;    
    float specularPow;    

    struct CSMdata
    {
        public float4x4 CV;
        public float4x4 CV_depth;
    }   

    public Color pColor
    {
        get { return GameManager.playerColor[GameManager.playerNum[unitIdx]]; }
    }

    void InitRendering()
    {
        csmAction = trLight.GetComponent<CSMAction>();
        csmCount = csmAction.csmNum;
        drenTexs = csmAction.drenTex_array; 
        dw = csmAction.dw;
        dh = csmAction.dh;      
        //specularFactor = CSMAction.specularFactor;
        specularPow = 2.0f;
        

        {
            skMpb.SetTexture("csmTexArray", drenTexs);           
        }

        if (hasStMesh)
        {
            stMpb.SetTexture("csmTexArray", drenTexs);            
        }

        bool bArray = true;
        {
            if (bArray)
            {
                skMpb.SetInt("bArray", 1);
            }
            else
            {
                skMpb.SetInt("bArray", 0);
            }

            skMpb.SetInt("csmWidth", dw);
            skMpb.SetInt("csmHeight", dh);           
            skMpb.SetFloat("specularPow", specularPow);

            skMpb.SetInt("unitOffset", offsetIdx);
            skMpb.SetColor("pColor", pColor);                        
        }

        if (hasStMesh)
        {
            if (bArray)
            {
                stMpb.SetInt("bArray", 1);
            }
            else
            {
                stMpb.SetInt("bArray", 0);
            }

            stMpb.SetInt("csmWidth", dw);
            stMpb.SetInt("csmHeight", dh);          
            stMpb.SetFloat("specularPow", specularPow);

            stMpb.SetInt("unitOffset", offsetIdx);
            stMpb.SetColor("pColor", pColor);
        }

        //passCSM = skMte.FindPass("UnitAnimationDepth_GS");
        //passColor = skMte.FindPass("UnitAnimationColor");
        
        if(GameManager.inRoom)
        {
            passCSM = skMte.FindPass("UnitAnimationDepth_Room_GS");
            passColor = skMte.FindPass("UnitAnimationColor_Room");
        }
        else
        {
            passCSM = skMte.FindPass("UnitAnimationDepth_GS");
            passColor = skMte.FindPass("UnitAnimationColor");
        }


        RenderGOM.RenderCSM_GS += RenderCSM;
        RenderGOM.OnRenderCamAlpha += Render;               
    }

    RootWJob rootWJob;
    TransformAccessArray traa_rootW;

    AnimPlayerData[] animPlayerData;
    float4x4[] rootW;
    float4x4[,,] boneSampleDataIn;
    float4x4[] boneSampleDataOut;


    int[] boneWTransformData_mask;
    int[] boneWTransformData_parent;
    float4x4[] boneWTransformData_root;
    float4x4[] boneWTransformData_output;

    uint[] boneOffsetDataIdx;
    float4x4[] boneOffsetDataInput;
    float4x4[] boneData;

    uint[] boneStaticData_idx;
    float4[] boneScaData_static;
    float4x4[] boneStaticData_tr;
    float4x4[] boneStaticData;
    float4x4[] boneStaticData_IT;

    ComputeBuffer animPlayer;
    ComputeBuffer boneSample_root;
    Texture3D boneSample_input;
    ComputeBuffer boneSample_output;

    ComputeBuffer boneWTransform_mask;
    ComputeBuffer boneWTransform_parent;
    ComputeBuffer boneWTransform_root;
    ComputeBuffer boneWTransform_output;

    ComputeBuffer boneOffset_idx;
    ComputeBuffer boneOffset_input;
    ComputeBuffer bone;

    ComputeBuffer boneStatic_idx;
    ComputeBuffer boneSca_static;
    ComputeBuffer boneStatic_tr;
    ComputeBuffer boneStatic;
    ComputeBuffer boneStatic_IT;

    int boneSampleCount;
    int boneWTransformCount;
    int boneOffsetCount;
    int boneStaticCount;

    float4 countInfo_sample;
    float4 countInfo_wtransform;
    float4 countInfo_offset;
    float4 countInfo_static;

    BoneCompute boneCompute;
    VertexCompute vtxCompute;
    VertexComputeSt vtxComputeSt;

    public void InitAnim()
    {
        {
            boneSampleCount = count * boneCount;
            boneWTransformCount = count * boneCount;
            boneOffsetCount = count * bmCount;
            boneStaticCount = count * stCount;
        }

        {
            rootWJob = new RootWJob();
            rootWJob.rootW = new NativeArray<float4x4>(count, Allocator.Persistent);
            traa_rootW = new TransformAccessArray(trs);
        }

        {
            animPlayerData = new AnimPlayerData[count];
            rootW = new float4x4[count];
            boneSampleDataIn = new float4x4[clipCount, frameCount, boneCount];
            boneSampleDataOut = new float4x4[boneSampleCount];

            boneWTransformData_mask = new int[boneCount];
            boneWTransformData_parent = new int[boneCount];
            boneWTransformData_root = new float4x4[count];
            boneWTransformData_output = new float4x4[boneWTransformCount];

            boneOffsetDataIdx = new uint[bmCount];
            boneOffsetDataInput = new float4x4[bmCount];
            boneData = new float4x4[boneOffsetCount];

            boneStaticData_idx = new uint[stCount];
            boneScaData_static = new float4[stCount];
            boneStaticData_tr = new float4x4[boneStaticCount];
            boneStaticData = new float4x4[boneStaticCount];
            boneStaticData_IT = new float4x4[boneStaticCount];
        }

        {
            animPlayer = new ComputeBuffer(count, Marshal.SizeOf<AnimPlayerData>(), ComputeBufferType.Structured, ComputeBufferMode.SubUpdates);
            boneSample_root = new ComputeBuffer(count, Marshal.SizeOf<float4x4>(), ComputeBufferType.Structured, ComputeBufferMode.SubUpdates);
            boneSample_input = new Texture3D(4 * boneCount, frameCount, clipCount, TextureFormat.RGBAFloat, false);
            boneSample_output = new ComputeBuffer(boneSampleCount, Marshal.SizeOf<float4x4>());

            boneWTransform_mask = new ComputeBuffer(boneCount, sizeof(int), ComputeBufferType.Structured, ComputeBufferMode.Immutable);
            boneWTransform_parent = new ComputeBuffer(boneCount, sizeof(int), ComputeBufferType.Structured, ComputeBufferMode.Immutable);
            boneWTransform_root = new ComputeBuffer(count, Marshal.SizeOf<float4x4>(), ComputeBufferType.Structured, ComputeBufferMode.SubUpdates);
            boneWTransform_output = new ComputeBuffer(boneWTransformCount, Marshal.SizeOf<float4x4>());

            boneOffset_idx = new ComputeBuffer(bmCount, sizeof(uint), ComputeBufferType.Structured, ComputeBufferMode.Immutable);
            boneOffset_input = new ComputeBuffer(bmCount, Marshal.SizeOf<float4x4>(), ComputeBufferType.Structured, ComputeBufferMode.Immutable);
            bone = new ComputeBuffer(boneOffsetCount, Marshal.SizeOf<float4x4>());

            boneStatic_idx = new ComputeBuffer(stCount, sizeof(uint), ComputeBufferType.Structured, ComputeBufferMode.Immutable);
            boneSca_static = new ComputeBuffer(stCount, Marshal.SizeOf<float4>(), ComputeBufferType.Structured, ComputeBufferMode.Immutable);
            boneStatic_tr = new ComputeBuffer(boneStaticCount, Marshal.SizeOf<float4x4>());
            boneStatic = new ComputeBuffer(boneStaticCount, Marshal.SizeOf<float4x4>());
            boneStatic_IT = new ComputeBuffer(boneStaticCount, Marshal.SizeOf<float4x4>());
        }

        {
            //sample
            for (int i = 0; i < count; i++)
            {
                animPlayerData[i] = new AnimPlayerData();
                anims[i].SetPlayerData(animPlayerData);
            }

            //wTransform
            for (int i = 0; i < boneCount; i++)
            {
                boneWTransformData_mask[i] = idxMask[i];
            }
            boneWTransform_mask.SetData(boneWTransformData_mask);

            for (int i = 0; i < boneCount; i++)
            {
                boneWTransformData_parent[i] = idxParent[i];
            }
            boneWTransform_parent.SetData(boneWTransformData_parent);

            //skin
            for (int i = 0; i < bmCount; i++)
            {
                boneOffsetDataIdx[i] = (uint)boneIdx[i];
            }
            boneOffset_idx.SetData(boneOffsetDataIdx);

            for (int i = 0; i < bmCount; i++)
            {
                boneOffsetDataInput[i] = bindpose[i];
            }
            boneOffset_input.SetData(boneOffsetDataInput);

            //static
            for (int i = 0; i < stCount; i++)
            {
                boneStaticData_idx[i] = (uint)boneIdx_st[i];
            }
            boneStatic_idx.SetData(boneStaticData_idx);

            for (int i = 0; i < stCount; i++)
            {
                boneScaData_static[i] = boneSca_st[i];
            }
            boneSca_static.SetData(boneScaData_static);

            {
                countInfo_sample.x = boneCount;
                countInfo_wtransform.xy = new float2(boneCount, depthCount);
                countInfo_offset.x = bmCount;
                countInfo_static.x = stCount;
            }
        }

        {
            boneCompute = new BoneCompute();
            vtxCompute = new VertexCompute();
            vtxComputeSt = new VertexComputeSt();

            boneCompute.Init(cshader, this);
            vtxCompute.Init(cshader, skMesh, this, count);
            vtxComputeSt.Init(cshader, stMeshes, this, count);
        }

        {
            skMpb.SetBuffer("vtxDynamic", vtxCompute.vOut);
            skMpb.SetInt("dvCount", vtxCompute.dvCount);
        }

        if (hasStMesh)
        {
            stMpb.SetBuffer("vtxDynamic", vtxComputeSt.vOut);
            stMpb.SetInt("dvCount", vtxComputeSt.dvCount);
        }

    }

    void ReleaseAnim()
    {
        Action<ComputeBuffer> ReleaseCBuffer =
            (cbuffer) => { if (cbuffer != null) cbuffer.Release(); cbuffer = null; };

        ReleaseCBuffer(animPlayer);
        ReleaseCBuffer(boneSample_root);
        ReleaseCBuffer(boneSample_output);

        ReleaseCBuffer(boneWTransform_mask);
        ReleaseCBuffer(boneWTransform_parent);
        ReleaseCBuffer(boneWTransform_root);
        ReleaseCBuffer(boneWTransform_output);

        ReleaseCBuffer(boneOffset_idx);
        ReleaseCBuffer(boneOffset_input);
        ReleaseCBuffer(bone);

        ReleaseCBuffer(boneStatic_idx);
        ReleaseCBuffer(boneSca_static);
        ReleaseCBuffer(boneStatic_tr);
        ReleaseCBuffer(boneStatic);
        ReleaseCBuffer(boneStatic_IT);
    }

    private void RenderCSM(ScriptableRenderContext context, Camera cam)
    {               
        if(bRender)
        {
            CommandBuffer cmd = CommandBufferPool.Get();            

            {
                cmd.DrawProcedural(idxBuffer, Matrix4x4.identity, skMte, passCSM, MeshTopology.Triangles, idxBuffer.count, count, skMpb);
            }

            if (hasStMesh)
            {
                cmd.DrawProcedural(stIdxBuffer, Matrix4x4.identity, stMte, passCSM, MeshTopology.Triangles, stIdxBuffer.count, count, stMpb);
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }        
    }

    

    private void Render(ScriptableRenderContext context, Camera cam, RenderGOM.PerCamera perCam)
    {
        if(bRender)
        {
            CommandBuffer cmd = CommandBufferPool.Get();

            perCam.V = RenderUtil.GetVfromW(cam);
            {
                skMpb.SetMatrix("CV_view", math.mul(RenderUtil.GetCfromV(cam, false), perCam.V));
                skMpb.SetMatrix("CV", perCam.CV);
                skMpb.SetMatrixArray("TCV_light", csmAction.TCV_depth);
                skMpb.SetFloatArray("endZ", csmAction.endZ);
                skMpb.SetVector("dirW_light", csmAction.dirW);
                skMpb.SetVector("dirW_view", (Vector3)perCam.dirW_view);
                skMpb.SetVector("posW_view", (Vector3)perCam.posW_view);

                cmd.DrawProcedural(idxBuffer, Matrix4x4.identity, skMte, passColor, MeshTopology.Triangles, idxBuffer.count, count, skMpb);                
            }

            if (hasStMesh)
            {
                stMpb.SetMatrix("CV_view", math.mul(RenderUtil.GetCfromV(cam, false), perCam.V));
                stMpb.SetMatrix("CV", perCam.CV);
                stMpb.SetMatrixArray("TCV_light", csmAction.TCV_depth);
                stMpb.SetFloatArray("endZ", csmAction.endZ);
                stMpb.SetVector("dirW_light", csmAction.dirW);
                stMpb.SetVector("dirW_view", (Vector3)perCam.dirW_view);
                stMpb.SetVector("posW_view", (Vector3)perCam.posW_view);

                cmd.DrawProcedural(stIdxBuffer, Matrix4x4.identity, stMte, passColor, MeshTopology.Triangles, stIdxBuffer.count, count, stMpb);
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }        
    }


    public Shader dbShader;
    Material dbMte;
    MaterialPropertyBlock dbMpb;
    int pass_wire;
    public bool bDebugShader = false;
    public Color wireColor = Color.green;

    Matrix4x4[] Ws0;
    Matrix4x4[] Ws1;

    Mesh dbBodycolMesh;
    bool bdbBodySphere = false;
    public float spBodyRadius = 1.0f;

    Mesh dbHitposMesh;
    bool bdbHitposSphere = false;
    public float spHitposRadius = 1.0f;

    public bool bCapColMeshUpdate = false;
    public bool bColJob = true;

    void InitDebugRendering()
    {
        if (bDebugShader)
        {
            dbMte = new Material(dbShader);
            dbMpb = new MaterialPropertyBlock();
            pass_wire = dbMte.FindPass("BVwireframe");

            Ws0 = new Matrix4x4[count];
            Ws1 = new Matrix4x4[count];

            if (trs[0].GetComponent<SphereCollider>() != null)
            {
                bdbBodySphere = true;
            }
            else if (trs[0].GetComponent<CapsuleCollider>() != null)
            {
                bdbBodySphere = false;
            }

            if (trsHitCollider[0].GetComponent<SphereCollider>() != null)
            {
                bdbHitposSphere = true;
            }
            else if (trsHitCollider[0].GetComponent<CapsuleCollider>() != null)
            {
                bdbHitposSphere = false;
            }
        }

        if (bDebugShader)
        {
            if (bdbBodySphere)
            {
                dbBodycolMesh = RenderUtil.CreateSphereMesh(1.0f, 12, 24);
            }
            else
            {
                if(bCapColMeshUpdate)
                {
                    dbBodycolMesh = new Mesh();
                }
                else
                {
                    CapsuleCollider capCol = unitActors[0].GetComponent<CapsuleCollider>();
                    Transform tr = unitActors[0].transform;
                    dbBodycolMesh = RenderUtil.CreateCapsuleMesh(capCol, tr, 12, 24);
                }
            }


            if (bdbHitposSphere)
            {
                dbHitposMesh = RenderUtil.CreateSphereMesh(1.0f, 12, 24);
            }
            else
            {
                if (bCapColMeshUpdate)
                {
                    dbHitposMesh = new Mesh();
                }
                else
                {
                    CapsuleCollider capCol = this.trsHitCollider[0].GetComponent<CapsuleCollider>();
                    Transform tr = this.trsHitCollider[0];
                    dbHitposMesh = RenderUtil.CreateCapsuleMesh(capCol, tr, 12, 24);
                }                               
            }


            if(!bColJob)
            {
                RenderGOM.OnRenderCamDebug += RenderDebug;
            }
            
            if(!bCapColMeshUpdate && bColJob)
            {
                RenderGOM.OnRenderCamDebug += RenderDebugJob;
            }                     
        }

        if(bDebugShader && !bCapColMeshUpdate && bColJob) 
        {
            jobCols = new WfromLCollider[2];
            
            for(int i = 0; i < jobCols.Length; i++)
            {
                jobCols[i] = new WfromLCollider();
            }
        
            float3 info;
            float3 center;
            if (bdbBodySphere)
            {
                var col = trs[0].GetComponent<SphereCollider>();
                info = float3.zero;
                info.x = col.radius;
                center = col.center;
                jobCols[0].Init(trs, 0, info, center);
            }
            else
            {
                var col = trs[0].GetComponent<CapsuleCollider>();
                info = float3.zero;
                info.x = col.radius;
                info.y = col.direction;
                center = col.center;
                jobCols[0].Init(trs, 1, info, center);
            }
        
            if(bdbHitposSphere)
            {
                var col = trsHitCollider[0].GetComponent<SphereCollider>();
                info = float3.zero;
                info.x = col.radius;
                center = col.center;
                jobCols[1].Init(trsHitCollider, 0, info, center);
            }
            else
            {
                var col = trsHitCollider[0].GetComponent<CapsuleCollider>();
                info = float3.zero;
                info.x = col.radius;
                info.y = col.direction;
                center = col.center;
                jobCols[1].Init(trsHitCollider, 1, info, center);
            }
        }
    }

    WfromLCollider[] jobCols;

    void RenderDebugJob(ScriptableRenderContext context, Camera cam, RenderGOM.PerCamera perCam)
    {
        if(bRender)
        {
            CommandBuffer cmd = CommandBufferPool.Get();

            float4x4 W = float4x4.identity;
            {
                {
                    jobCols[0].Execute();

                    for (int i = 0; i < count; i++)
                    {
                        Ws0[i] = jobCols[0].Ws[i];
                    }
                }

                dbMpb.SetMatrixArray("Ws", Ws0);
                dbMpb.SetMatrix("CV", perCam.CV);
                dbMpb.SetMatrix("S", perCam.S);
                dbMpb.SetColor("color", wireColor);

                cmd.DrawMeshInstancedProcedural(dbBodycolMesh, 0, dbMte, pass_wire, count, dbMpb);
            }


            {
                {
                    jobCols[1].Execute();

                    for (int i = 0; i < count; i++)
                    {
                        Ws1[i] = jobCols[1].Ws[i];
                    }
                }

                dbMpb.SetMatrixArray("Ws", Ws1);
                dbMpb.SetMatrix("CV", perCam.CV);
                dbMpb.SetMatrix("S", perCam.S);
                dbMpb.SetColor("color", wireColor);

                cmd.DrawMeshInstancedProcedural(dbHitposMesh, 0, dbMte, pass_wire, count, dbMpb);
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
       
    }

    void RenderDebug(ScriptableRenderContext context, Camera cam, RenderGOM.PerCamera perCam)
    {
        if (bRender)
        {
            CommandBuffer cmd = CommandBufferPool.Get();

            float4x4 W = float4x4.identity;
            {
                if (bdbBodySphere)
                {
                    for (int i = 0; i < count; i++)
                    {
                        SphereCollider spCol = unitActors[i].GetComponent<SphereCollider>();
                        RenderUtil.GetW_fromSphereCollider(spCol, trs[i], out W);

                        Ws0[i] = W;
                    }
                }
                else  //capsule
                {
                    for (int i = 0; i < count; i++)
                    {
                        CapsuleCollider capCol = unitActors[i].GetComponent<CapsuleCollider>();

                        if (bCapColMeshUpdate)
                        {
                            RenderUtil.GetWMesh_fromCapsuleCollider(capCol, trs[i], dbBodycolMesh, out W);
                        }
                        else
                        {
                            RenderUtil.GetW_fromCapsuleCollider(capCol, trs[i], out W);
                        }

                        Ws0[i] = W;
                    }
                }

                dbMpb.SetMatrixArray("Ws", Ws0);
                dbMpb.SetMatrix("CV", perCam.CV);
                dbMpb.SetMatrix("S", perCam.S);
                dbMpb.SetColor("color", wireColor);

                cmd.DrawMeshInstancedProcedural(dbBodycolMesh, 0, dbMte, pass_wire, count, dbMpb);
            }


            {
                if (bdbHitposSphere)
                {
                    for (int i = 0; i < count; i++)
                    {
                        SphereCollider spCol = trsHitCollider[i].GetComponent<SphereCollider>();
                        RenderUtil.GetW_fromSphereCollider(spCol, trsHitCollider[i], out W);

                        Ws1[i] = W;
                    }
                }
                else  //capsule
                {
                    for (int i = 0; i < count; i++)
                    {
                        CapsuleCollider capCol = trsHitCollider[i].GetComponent<CapsuleCollider>();

                        if (bCapColMeshUpdate)
                        {
                            RenderUtil.GetWMesh_fromCapsuleCollider(capCol, trsHitCollider[i], dbHitposMesh, out W);
                        }
                        else
                        {
                            RenderUtil.GetW_fromCapsuleCollider(capCol, trsHitCollider[i], out W);
                        }

                        Ws1[i] = W;
                    }
                }

                dbMpb.SetMatrixArray("Ws", Ws1);
                dbMpb.SetMatrix("CV", perCam.CV);
                dbMpb.SetMatrix("S", perCam.S);
                dbMpb.SetColor("color", wireColor);

                cmd.DrawMeshInstancedProcedural(dbHitposMesh, 0, dbMte, pass_wire, count, dbMpb);
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        
    }

    public bool bColorChange = false;
    private void Update()
    {
        if (bColorChange)
        {
            if (Input.GetKeyDown(KeyCode.N))
            {
                ChangeColor(true);
            }

            if (Input.GetKeyDown(KeyCode.M))
            {
                ChangeColor(false);
            }
        }
    }   

    public bool bRigid = false;       

    public bool bUpdate = true;        


    protected virtual void UpdateBoneAction()
    {
        if (bUpdate && count > 0)
        {
            float3 ldir = math.rotate(trLight.rotation, new float3(0.0f, 0.0f, 1.0f));
            skMpb.SetVector("lightDir", new Vector4(ldir.x, ldir.y, ldir.z, 0.0f));
            if (hasStMesh)
            {
                stMpb.SetVector("lightDir", new Vector4(ldir.x, ldir.y, ldir.z, 0.0f));
            }


            {
                UpdateSampleAnim();

                UpdateRootM();
                {
                    boneCompute.Compute();
                }
                UpdateStaticM();

                //UpdateLight();
                UpdateAnimSpeed();
            }            
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UpdateSampleAnim()
    {
        float dt = Time.deltaTime;

        Parallel.For(0, count,
            (i) =>
            {
                var player = anims[i].player;
                //player.bRigid = bRigid;
                //player.cState.Sample_Baked(dt);
                player.cState.Sample_Total(dt);
            });

        //for (int i = 0; i < count; i++)
        //{
        //    var player = anims[i].player;
        //    //player.bRigid = bRigid;
        //    player.cState.Sample_Baked(dt);
        //}
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void UpdateRootM()
    {
        {
            rootWJob.Schedule<RootWJob>(traa_rootW).Complete();
        }              
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void UpdateStaticM()
    {
        if (hasStMesh)
        {
            Parallel.For(0, count,
                (int i) =>
                {
                    for (int j = 0; j < stCount; j++)
                    {                       
                        trM[i][j] = boneStaticData_tr[i * stCount + j];
                    }
                });

        }
    }

    float3 lightDir;
    void UpdateLight()
    {
        lightDir = -math.rotate(trLight.rotation, new float3(0.0f, 0.0f, 1.0f));
    }

    void UpdateAnimSpeed()
    {
        Parallel.For(0, count,
            (int i) =>
            {
                anims[i].player.speed = animSpeed;
            });

        //for(int i = 0; i < count; i++)
        //{
        //    anims[i].player.speed = animSpeed;
        //}
    }

    private void LateUpdate()
    {

    }


    public BoneNode bnRoot;

    public BoneNode[] bns0_;
    public BoneNode[] bns1_;

    void BeginFrameRender(ScriptableRenderContext context, Camera[] cams)
    {        
        {
            vtxCompute.ComputeVertex(context);
        }

        if (hasStMesh)
        {
            vtxComputeSt.ComputeVertex(context);
        }
    }

    public void BakeAnimCurve(string clipName, int frameIdx)
    {
        if (count > 0)
        {
            var player = anims[0].player;
            //player.bRigid = bRigid;
            player.bRigid = true;
            anims[0].BakeAnimation(clipName, frameIdx);
        }
    }

    public void BakeAnimation()
    {
        for (int i = 0; i < clipCount; i++)
        {
            for (int j = 0; j < frameCount; j++)
            {
                BakeAnimCurve(clips[i].name, j);

                for (int k = 0; k < boneCount; k++)
                {
                    float4 real = bns0[0][k].transform.dqL.real;
                    float4 dual = bns0[0][k].transform.dqL.dual;

                    boneSampleDataIn[i, j, k].c0 = real;
                    boneSampleDataIn[i, j, k].c1 = dual;

                    boneSample_input.SetPixel(k * 4 + 0, j, i, (Vector4)real);
                    boneSample_input.SetPixel(k * 4 + 1, j, i, (Vector4)dual);
                }               
            }
        }

        boneSample_input.Apply();

        int a = 0;

    }   

    void OnDestroy()
    {
        //if (vtxCompute != null) vtxCompute.ReleaseCShader();
        if (vtxStatic != null) vtxStatic.Dispose();
        if (idxBuffer != null) idxBuffer.Dispose();
        if (idxBuffers != null)
        {
            for (int i = 0; i < idxBuffers.Length; i++)
            {
                idxBuffers[i].Dispose();
            }
        }

        if (hasStMesh)
        {
            //if (vtxComputeSt != null) vtxComputeSt.ReleaseCShader();
            if (stVtxBuffer != null) stVtxBuffer.Dispose();
            if (stIdxBuffer != null) stIdxBuffer.Dispose();
        }

        //{
        //    BoneNode.DeleteNa(nas);
        //}
       
        {
            //if (rootW_na.IsCreated) rootW_na.Dispose(); 

            rootWJob.Dispose();
            if (traa_rootW.isCreated) traa_rootW.Dispose();
        }

        {
            ReleaseAnim();
        }

        {
            vtxCompute.ReleaseCShader();
            vtxComputeSt.ReleaseCShader();
        }
        {
            RenderGOM.BeginFrameRender -= BeginFrameRender;

            RenderGOM.RenderCSM_GS -= RenderCSM;
            RenderGOM.OnRenderCamAlpha -= Render;
        }
        
        //if (bDebugShader && !bColJob)
        //{
        //    RenderGOM.OnRenderCamDebug -= RenderDebug;
        //}
        //
        //if (bDebugShader && !bCapColMeshUpdate && bColJob)
        //{
        //    RenderGOM.OnRenderCamDebug -= RenderDebugJob;              
        //}

        if (bDebugShader)
        {
            if (bColJob)
            {
                if (!bCapColMeshUpdate)
                {
                    RenderGOM.OnRenderCamDebug -= RenderDebugJob;
                }
            }
            else
            {
                RenderGOM.OnRenderCamDebug -= RenderDebug;
            }
        }

        if (jobCols != null)
        {
            for (int i = 0; i < jobCols.Length; i++)
            {
                if (jobCols[i] != null) jobCols[i].Dispose();
            }
        }

        SceneManager.sceneUnloaded -= OnSceneLeft;
    }


    private void OnApplicationQuit()
    {       
        OnDestroy();        
    }

    class BoneCompute
    {
        public void Compute()
        {
            WriteToResource();
            Dispatch();
            ReadFromResource();
        }

        UnitManager unitMan;
        ComputeShader cshader;
        int ki_sample;
        int ki_wtransform;
        int ki_offset;
        int ki_static;

        public void Init(ComputeShader cshader, UnitManager unitMan)
        {
            this.unitMan = unitMan;
            this.cshader = cshader;
            this.ki_sample = cshader.FindKernel("CS_BoneSample");
            this.ki_wtransform = cshader.FindKernel("CS_BoneWTransform");
            this.ki_offset = cshader.FindKernel("CS_BoneOffset");
            this.ki_static = cshader.FindKernel("CS_BoneStatic");

            cshader.SetVector("countInfo_sample", unitMan.countInfo_sample);
            cshader.SetBuffer(ki_sample, "animPlayer", unitMan.animPlayer);           
            cshader.SetTexture(ki_sample, "boneSample_input", unitMan.boneSample_input);
            cshader.SetBuffer(ki_sample, "boneSample_output", unitMan.boneSample_output);

            cshader.SetVector("countInfo_wtransform", unitMan.countInfo_wtransform);
            cshader.SetBuffer(ki_wtransform, "boneWTransform_mask", unitMan.boneWTransform_mask);
            cshader.SetBuffer(ki_wtransform, "boneWTransform_parent", unitMan.boneWTransform_parent);
            cshader.SetBuffer(ki_wtransform, "boneWTransform_root", unitMan.boneWTransform_root);
            cshader.SetBuffer(ki_wtransform, "boneSample_output", unitMan.boneSample_output);
            cshader.SetBuffer(ki_wtransform, "boneWTransform_output", unitMan.boneWTransform_output);


            cshader.SetVector("countInfo_offset", unitMan.countInfo_offset);           
            cshader.SetBuffer(ki_offset, "boneWTransform_output", unitMan.boneWTransform_output);
            cshader.SetBuffer(ki_offset, "boneOffset_idx", unitMan.boneOffset_idx);
            cshader.SetBuffer(ki_offset, "boneOffset_input", unitMan.boneOffset_input);
            cshader.SetBuffer(ki_offset, "bone", unitMan.bone);

            cshader.SetVector("countInfo_static", unitMan.countInfo_static);
            cshader.SetBuffer(ki_static, "boneStatic_idx", unitMan.boneStatic_idx);
            cshader.SetBuffer(ki_static, "boneSca_static", unitMan.boneSca_static);           
            cshader.SetBuffer(ki_static, "boneWTransform_output", unitMan.boneWTransform_output);
            cshader.SetBuffer(ki_static, "boneStatic_tr", unitMan.boneStatic_tr);
            cshader.SetBuffer(ki_static, "bone", unitMan.boneStatic);
            cshader.SetBuffer(ki_static, "bone_IT", unitMan.boneStatic_IT);
        }

        public void WriteToResource()
        {
            {
                var na = unitMan.animPlayer.BeginWrite<AnimPlayerData>(0, unitMan.count);
                for (int i = 0; i < unitMan.count; i++)
                {
                    na[i] = unitMan.animPlayerData[i];
                }
                unitMan.animPlayer.EndWrite<AnimPlayerData>(unitMan.count);
            }

            //{
            //    var na = unitMan.boneSample_root.BeginWrite<float4x4>(0, unitMan.count);
            //    for (int i = 0; i < unitMan.count; i++)
            //    {
            //        na[i] = unitMan.rootW[i];
            //    }
            //    unitMan.boneSample_root.EndWrite<float4x4>(unitMan.count);
            //}

            {
                var na = unitMan.boneWTransform_root.BeginWrite<float4x4>(0, unitMan.count);
                for (int i = 0; i < unitMan.count; i++)
                {
                    //na[i] = unitMan.rootW[i];
                    na[i] = unitMan.rootWJob.rootW[i];
                }
                unitMan.boneWTransform_root.EndWrite<float4x4>(unitMan.count);
            }
        }

        public void Dispatch()
        {
            CommandBuffer cmd = CommandBufferPool.Get();
            cmd.SetExecutionFlags(CommandBufferExecutionFlags.AsyncCompute);            

            {
                cmd.SetComputeVectorParam(cshader, "countInfo_sample", unitMan.countInfo_sample);
                cmd.SetComputeBufferParam(cshader, ki_sample, "animPlayer", unitMan.animPlayer);
                cmd.SetComputeTextureParam(cshader, ki_sample, "boneSample_input", unitMan.boneSample_input);
                cmd.SetComputeBufferParam(cshader, ki_sample, "boneSample_output", unitMan.boneSample_output);

                cmd.SetComputeVectorParam(cshader, "countInfo_wtransform", unitMan.countInfo_wtransform);
                cmd.SetComputeBufferParam(cshader, ki_wtransform, "boneWTransform_mask", unitMan.boneWTransform_mask);
                cmd.SetComputeBufferParam(cshader, ki_wtransform, "boneWTransform_parent", unitMan.boneWTransform_parent);
                cmd.SetComputeBufferParam(cshader, ki_wtransform, "boneWTransform_root", unitMan.boneWTransform_root);
                cmd.SetComputeBufferParam(cshader, ki_wtransform, "boneSample_output", unitMan.boneSample_output);
                cmd.SetComputeBufferParam(cshader, ki_wtransform, "boneWTransform_output", unitMan.boneWTransform_output);


                cmd.SetComputeVectorParam(cshader, "countInfo_offset", unitMan.countInfo_offset);
                cmd.SetComputeBufferParam(cshader, ki_offset, "boneWTransform_output", unitMan.boneWTransform_output);
                cmd.SetComputeBufferParam(cshader, ki_offset, "boneOffset_idx", unitMan.boneOffset_idx);
                cmd.SetComputeBufferParam(cshader, ki_offset, "boneOffset_input", unitMan.boneOffset_input);
                cmd.SetComputeBufferParam(cshader, ki_offset, "bone", unitMan.bone);

                cmd.SetComputeVectorParam(cshader, "countInfo_static", unitMan.countInfo_static);
                cmd.SetComputeBufferParam(cshader, ki_static, "boneStatic_idx", unitMan.boneStatic_idx);
                cmd.SetComputeBufferParam(cshader, ki_static, "boneSca_static", unitMan.boneSca_static);
                cmd.SetComputeBufferParam(cshader, ki_static, "boneWTransform_output", unitMan.boneWTransform_output);
                cmd.SetComputeBufferParam(cshader, ki_static, "boneStatic_tr", unitMan.boneStatic_tr);
                cmd.SetComputeBufferParam(cshader, ki_static, "bone", unitMan.boneStatic);
                cmd.SetComputeBufferParam(cshader, ki_static, "bone_IT", unitMan.boneStatic_IT);
            }

            cmd.DispatchCompute(cshader, ki_sample, unitMan.count, 1, 1);
            cmd.DispatchCompute(cshader, ki_wtransform, unitMan.count, 1, 1);
            cmd.DispatchCompute(cshader, ki_offset, unitMan.count, 1, 1);
            cmd.DispatchCompute(cshader, ki_static, unitMan.count, 1, 1);

            //Graphics.ExecuteCommandBuffer(cmd);
            Graphics.ExecuteCommandBufferAsync(cmd, ComputeQueueType.Urgent);

            CommandBufferPool.Release(cmd);
        }

        public void ReadFromResource()
        {
            bool bDebug = false;

            if (bDebug)
            {
                AsyncGPUReadback.Request(unitMan.boneSample_output,
                (read) =>
                {
                    var na = read.GetData<float4x4>(0);
                    for (int i = 0; i < unitMan.boneSampleCount; i++)
                    {
                        unitMan.boneSampleDataOut[i] = na[i];
                    }
                });

                AsyncGPUReadback.Request(unitMan.boneWTransform_output,
                (read) =>
                {
                    var na = read.GetData<float4x4>(0);
                    for (int i = 0; i < unitMan.boneWTransformCount; i++)
                    {
                        unitMan.boneWTransformData_output[i] = na[i];
                    }
                });

                AsyncGPUReadback.Request(unitMan.bone,
                    (read) =>
                    {
                        var na = read.GetData<float4x4>(0);
                        for (int i = 0; i < unitMan.boneOffsetCount; i++)
                        {
                            unitMan.boneData[i] = na[i];
                        }
                    });

                AsyncGPUReadback.Request(unitMan.boneStatic,
                    (read) =>
                    {
                        var na = read.GetData<float4x4>(0);
                        for (int i = 0; i < unitMan.boneStaticCount; i++)
                        {
                            unitMan.boneStaticData[i] = na[i];
                        }
                    });

                AsyncGPUReadback.Request(unitMan.boneStatic_IT,
                    (read) =>
                    {
                        var na = read.GetData<float4x4>(0);
                        for (int i = 0; i < unitMan.boneStaticCount; i++)
                        {
                            unitMan.boneStaticData_IT[i] = na[i];
                        }
                    });
            }

            {
                AsyncGPUReadback.Request(unitMan.boneStatic_tr,
                   (read) =>
                   {
                       var na = read.GetData<float4x4>(0);
                       for (int i = 0; i < unitMan.boneStaticCount; i++)
                       {
                           unitMan.boneStaticData_tr[i] = na[i];
                       }
                   });
            }
        }

        public void Release()
        {

        }
    }

    class VertexCompute
    {
        public void ComputeVertex(ScriptableRenderContext context)
        {
            WriteToCShader();
            ExecuteCShaderVertex(context);
            //ReadFromCShader();
        }
        UnitManager unitMan;
        ComputeShader cshader;
        int kindex;

        VertexIn[] vInData;
        //PerBone[] perBoneData;
        VertexOut[] vOutData;

        ComputeBuffer vIn;
        //ComputeBuffer perBone;
        public ComputeBuffer vOut { get; private set; }

        int vertexCount;
        int boneCount;
        int insCount;
        int vtxCountOut;

        int vgCount;
        int vtCount = 1024;
        int dpCount;
        public int dvCount;

        Vector4 countInfo;

        //float4x4[] Wbone;
        Mesh skMesh;
        public void Init(ComputeShader cshader, Mesh skMesh, UnitManager unitMan, int insCount)
        {
            this.unitMan = unitMan;
            this.cshader = cshader;
            this.kindex = cshader.FindKernel("CS_BoneVertex");

            //this.Wbone = Wbone;
            this.skMesh = skMesh;
            this.insCount = insCount;

            this.vertexCount = skMesh.vertexCount;
            this.boneCount = insCount * skMesh.bindposes.Length;

            this.vgCount = vertexCount / vtCount + 1;
            this.dpCount = insCount * vgCount;
            this.dvCount = vtCount * vgCount;

            this.vtxCountOut = insCount * dvCount;

            countInfo = new Vector4();
            countInfo[0] = insCount;
            countInfo[1] = vertexCount;
            countInfo[2] = vgCount;
            countInfo[3] = skMesh.bindposes.Length;

            InitCShader();
        }

        public void InitCShader()
        {
            {
                skMesh.RecalculateNormals();
                skMesh.RecalculateTangents();

                vInData = new VertexIn[vertexCount];
                vIn = new ComputeBuffer(vertexCount, Marshal.SizeOf<VertexIn>(), ComputeBufferType.Structured, ComputeBufferMode.SubUpdates);
                cshader.SetBuffer(kindex, "vIn", vIn);

                List<Vector3> posList = new List<Vector3>();
                List<Vector3> normalList = new List<Vector3>();
                List<Vector4> tangetList = new List<Vector4>();
                List<BoneWeight> bwList = new List<BoneWeight>();
                skMesh.GetVertices(posList);
                skMesh.GetNormals(normalList);
                skMesh.GetTangents(tangetList);
                skMesh.GetBoneWeights(bwList);

                for (int i = 0; i < vertexCount; i++)
                {
                    vInData[i].posL = posList[i];
                    vInData[i].normalL = normalList[i];
                    vInData[i].tangentL = tangetList[i];

                    BoneWeight bw = bwList[i];
                    vInData[i].boneI = new int4(bw.boneIndex0, bw.boneIndex1, bw.boneIndex2, bw.boneIndex3);
                    vInData[i].boneW = new float4(bw.weight0, bw.weight1, bw.weight2, bw.weight3);
                }

                var na = vIn.BeginWrite<VertexIn>(0, vertexCount);
                for (int i = 0; i < vertexCount; i++)
                {
                    na[i] = vInData[i];
                }
                vIn.EndWrite<VertexIn>(vertexCount);
            }

            //{
            //    perBoneData = new PerBone[boneCount];
            //    perBone = new ComputeBuffer(perBoneData.Length, Marshal.SizeOf<PerBone>(), ComputeBufferType.Structured, ComputeBufferMode.SubUpdates);
            //    cshader.SetBuffer(kindex, "perBone", perBone);
            //}

            {
                vOutData = new VertexOut[vtxCountOut];
                vOut = new ComputeBuffer(vOutData.Length, Marshal.SizeOf<VertexOut>());
                cshader.SetBuffer(kindex, "vOut", vOut);
            }

            //{
            //    cshader.SetInt("enalbeCull", 1);
            //}

        }

        public void WriteToCShader()
        {
            //for (int i = 0; i < Wbone.Length; i++)
            //{
            //    float4x4 _W = Wbone[i];
            //    float4x4 _W_IT = math.transpose(math.inverse(_W));
            //    perBoneData[i].W = (Matrix4x4)_W;
            //    perBoneData[i].W_IT = (Matrix4x4)_W_IT;
            //}
            //
            //{
            //    var na = perBone.BeginWrite<PerBone>(0, perBone.count);
            //    for (int i = 0; i < perBone.count; i++)
            //    {
            //        na[i] = perBoneData[i];
            //    }
            //    perBone.EndWrite<PerBone>(perBone.count);
            //}
        }

        public void ExecuteCShaderVertex(ScriptableRenderContext context)
        {
            CommandBuffer cmd = CommandBufferPool.Get();
            //cmd.SetExecutionFlags(CommandBufferExecutionFlags.AsyncCompute);

            cmd.SetComputeVectorParam(cshader, "countInfo_vertex", countInfo);
            cmd.SetComputeBufferParam(cshader, kindex, "bone", unitMan.bone);
            cmd.SetComputeBufferParam(cshader, kindex, "vIn", vIn);
            cmd.SetComputeBufferParam(cshader, kindex, "vOut", vOut);

            cmd.DispatchCompute(cshader, kindex, dpCount, 1, 1);

            //context.ExecuteCommandBufferAsync(cmd, ComputeQueueType.Default);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }


        public void ReadFromCShader()
        {
            //if (skMesh.name == "archer skMesh")
            //{
            //    vOut.GetData(vOutData);
            //}
            //else
            {
                vOut.GetData(vOutData);
            }

            //{
            //    var rq = AsyncGPUReadback.Request(vOut);
            //    rq.WaitForCompletion();
            //
            //    var na = rq.GetData<VertexOut>(0);
            //    for(int i = 0; i < na.Length; i++)
            //    {
            //        vOutData[i] = na[i];
            //    }
            //}
        }

        public void SetCullTexture(RenderTexture testRt, int cullOffset)
        {
            cshader.SetTexture(kindex, "testCull", testRt);
            cshader.SetInt("cullOffset", cullOffset);
        }

        public void ReleaseCShader()
        {
            if (vIn != null) vIn.Release();
            //if (perBone != null) perBone.Release();
            if (vOut != null) vOut.Release();
        }

        struct VertexIn
        {
            public float3 posL;
            public float3 normalL;
            public float4 tangentL;
            public int4 boneI;
            public float4 boneW;
        };

        struct PerBone
        {
            public float4x4 W;
            public float4x4 W_IT;
        };

        public struct VertexOut
        {
            public float3 posW;
            public float3 normalW;
            public float4 tangentW;
        };
    }

    class VertexComputeSt
    {
        public void ComputeVertex(ScriptableRenderContext context)
        {
            WriteToCShader();
            ExecuteCShaderVertex(context);
            //ReadFromCShader();
        }
        UnitManager unitMan;
        ComputeShader cshader;
        int kindex;

        VertexIn[] vInData;
        PerBone[] perBoneData;
        VertexOut[] vOutData;

        ComputeBuffer vIn;
        //ComputeBuffer perBone;
        public ComputeBuffer vOut { get; private set; }

        int vertexCount;
        int boneCount;
        int insCount;
        int stCount;
        int vtxCountOut;

        int vgCount;
        int vtCount = 1024;
        int dpCount;
        public int dvCount;

        Vector4 countInfo;

        //float4x4[] Wbone;
        Mesh stMesh;
        Mesh[] stMeshes;

        public void Init(ComputeShader cshader, Mesh[] stMeshes, UnitManager unitMan, int insCount)
        {
            this.unitMan = unitMan;
            this.cshader = cshader;
            this.kindex = cshader.FindKernel("CS_BoneVertex_Static");

            //this.stMesh = stMesh;

            //this.Wbone = Wbone;
            this.stMeshes = stMeshes;
            this.insCount = insCount;
            this.stCount = stMeshes.Length;

            this.vertexCount = 0;
            for (int i = 0; i < stMeshes.Length; i++)
            {
                this.vertexCount += stMeshes[i].vertexCount;
            }

            this.boneCount = insCount * stCount;

            this.vgCount = (vertexCount % vtCount == 0) ? (vertexCount / vtCount) : (vertexCount / vtCount + 1);
            this.dpCount = insCount * vgCount;
            this.dvCount = vtCount * vgCount;

            this.vtxCountOut = insCount * dvCount;

            countInfo = new Vector4();
            countInfo[0] = insCount;
            countInfo[1] = vertexCount;
            countInfo[2] = vgCount;
            countInfo[3] = stCount;

            InitCShader();
        }

        public void InitCShader()
        {
            //{
            //    stMesh.RecalculateNormals();
            //    stMesh.RecalculateTangents();
            //
            //    vInData = new VertexIn[vertexCount];
            //    vIn = new ComputeBuffer(vertexCount, Marshal.SizeOf<VertexIn>(), ComputeBufferType.Structured, ComputeBufferMode.SubUpdates);
            //    cshader.SetBuffer(kindex, "vIn", vIn);
            //
            //    List<Vector3> posList = new List<Vector3>();
            //    List<Vector3> normalList = new List<Vector3>();
            //    List<Vector4> tangetList = new List<Vector4>();
            //    List<BoneWeight> bwList = new List<BoneWeight>();
            //    stMesh.GetVertices(posList);
            //    stMesh.GetNormals(normalList);
            //    stMesh.GetTangents(tangetList);
            //    stMesh.GetBoneWeights(bwList);
            //
            //    for (int i = 0; i < vertexCount; i++)
            //    {
            //        vInData[i].posL = posList[i];
            //        vInData[i].normalL = normalList[i];
            //        vInData[i].tangentL = tangetList[i];
            //
            //        BoneWeight bw = bwList[i];
            //        vInData[i].boneI = new int4(bw.boneIndex0, bw.boneIndex1, bw.boneIndex2, bw.boneIndex3);
            //        vInData[i].boneW = new float4(bw.weight0, bw.weight1, bw.weight2, bw.weight3);
            //    }
            //
            //    var na = vIn.BeginWrite<VertexIn>(0, vertexCount);
            //    for (int i = 0; i < vertexCount; i++)
            //    {
            //        na[i] = vInData[i];
            //    }
            //    vIn.EndWrite<VertexIn>(vertexCount);
            //}

            {
                vInData = new VertexIn[vertexCount];
                vIn = new ComputeBuffer(vertexCount, Marshal.SizeOf<VertexIn>(), ComputeBufferType.Structured, ComputeBufferMode.SubUpdates);
                cshader.SetBuffer(kindex, "vIn", vIn);

                int k = 0;
                for (int i = 0; i < stCount; i++)
                {
                    Mesh mesh = stMeshes[i];
                    mesh.RecalculateNormals();
                    mesh.RecalculateTangents();

                    List<Vector3> pos = new List<Vector3>();
                    List<Vector3> normal = new List<Vector3>();
                    List<Vector4> tangent = new List<Vector4>();

                    mesh.GetVertices(pos);
                    mesh.GetNormals(normal);
                    mesh.GetTangents(tangent);

                    for (int j = 0; j < mesh.vertexCount; j++)
                    {
                        VertexIn vtx = new VertexIn();
                        vtx.posL = pos[j];
                        vtx.normalL = normal[j];
                        vtx.tangentL = tangent[j];
                        vtx.boneI.x = i;

                        vInData[k] = vtx;
                        k++;
                    }
                }

                var na = vIn.BeginWrite<VertexIn>(0, vertexCount);
                for (int i = 0; i < vertexCount; i++)
                {
                    na[i] = vInData[i];
                }
                vIn.EndWrite<VertexIn>(vertexCount);
            }

            //{
            //    perBoneData = new PerBone[boneCount];
            //    perBone = new ComputeBuffer(perBoneData.Length, Marshal.SizeOf<PerBone>(), ComputeBufferType.Structured, ComputeBufferMode.SubUpdates);
            //    cshader.SetBuffer(kindex, "perBone", perBone);
            //}

            {
                vOutData = new VertexOut[vtxCountOut];
                vOut = new ComputeBuffer(vOutData.Length, Marshal.SizeOf<VertexOut>());
                cshader.SetBuffer(kindex, "vOut", vOut);
            }

        }

        public void WriteToCShader()
        {
            //for (int i = 0; i < Wbone.Length; i++)
            //{
            //    float4x4 _W = Wbone[i];
            //    float4x4 _W_IT = math.transpose(math.inverse(_W));
            //    perBoneData[i].W = (Matrix4x4)_W;
            //    perBoneData[i].W_IT = (Matrix4x4)_W_IT;
            //}
            //
            //{
            //    var na = perBone.BeginWrite<PerBone>(0, perBone.count);
            //    for (int i = 0; i < perBone.count; i++)
            //    {
            //        na[i] = perBoneData[i];
            //    }
            //    perBone.EndWrite<PerBone>(perBone.count);
            //}
        }

        public void ExecuteCShaderVertex(ScriptableRenderContext context)
        {
            CommandBuffer cmd = CommandBufferPool.Get();
            //cmd.SetExecutionFlags(CommandBufferExecutionFlags.AsyncCompute);

            cmd.SetComputeVectorParam(cshader, "countInfo_vertex_static", countInfo);
            cmd.SetComputeBufferParam(cshader, kindex, "vIn", vIn);
            cmd.SetComputeBufferParam(cshader, kindex, "bone", unitMan.boneStatic);
            cmd.SetComputeBufferParam(cshader, kindex, "bone_IT", unitMan.boneStatic_IT);
            cmd.SetComputeBufferParam(cshader, kindex, "vOut", vOut);



            cmd.DispatchCompute(cshader, kindex, dpCount, 1, 1);

            //context.ExecuteCommandBufferAsync(cmd, ComputeQueueType.Default);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }


        public void ReadFromCShader()
        {
            //if (skMesh.name == "archer skMesh")
            //{
            //    vOut.GetData(vOutData);
            //}
            //else
            {
                vOut.GetData(vOutData);
            }

            //{
            //    var rq = AsyncGPUReadback.Request(vOut);
            //    rq.WaitForCompletion();
            //
            //    var na = rq.GetData<VertexOut>(0);
            //    for(int i = 0; i < na.Length; i++)
            //    {
            //        vOutData[i] = na[i];
            //    }
            //}
        }

        public void SetCullTexture(RenderTexture testRt, int cullOffset)
        {
            cshader.SetTexture(kindex, "testCull", testRt);
            cshader.SetInt("cullOffset", cullOffset);
        }

        public void ReleaseCShader()
        {
            if (vIn != null) vIn.Release();
            //if (perBone != null) perBone.Release();
            if (vOut != null) vOut.Release();
        }

        struct VertexIn
        {
            public float3 posL;
            public float3 normalL;
            public float4 tangentL;
            public int4 boneI;
            public float4 boneW;
        };

        struct PerBone
        {
            public float4x4 W;
            public float4x4 W_IT;
        };

        public struct VertexOut
        {
            public float3 posW;
            public float3 normalW;
            public float4 tangentW;
        };
    }


    struct RootWJob : IJobParallelForTransform
    {
        [WriteOnly]
        public NativeArray<float4x4> rootW;

        public void Execute(int i, TransformAccess tra)
        {
            rootW[i] = BoneNode.GetPfromC(tra.position, tra.rotation, tra.localScale);

            //rootW[i] = math.mul(rootMat, BoneNode.GetPfromC(M.c0.xyz, M.c1, M.c2.xyz));
            //rootW[i] = math.mul(BoneNode.GetPfromC(M.c0.xyz, M.c1, M.c2.xyz), rootMat);
        }

        public void Dispose()
        {
            if (rootW.IsCreated) rootW.Dispose();
        }
    }


    struct VertexStatic
    {
        public float2 uv;
        public float4 color;
    }


    class WfromLCollider
    {
        public void Execute()
        {
            //WriteToNa();
            ExecuteJob();
            //ReadFromNa();
        }

        WfromLJob job;

        public NativeArray<float4x4> Ws
        {
            get; set;
        }

        TransformAccessArray traa;
        int count;

        public float4x4[] _Ws;

        public void Init(Transform[] trs, int type, float3 info, float3 center)
        {
            count = trs.Length;

            traa = new TransformAccessArray(count);
            Ws = new NativeArray<float4x4>(count, Allocator.Persistent);

            job = new WfromLJob();
            job.Ws = Ws;
            job.type = type;
            job.info = info;
            job.center = center;

            traa.SetTransforms(trs);
            _Ws = new float4x4[count];
        }

        void WriteToNa()
        {

        }

        void ExecuteJob()
        {
            job.Schedule<WfromLJob>(traa).Complete();
        }

        void ReadFromNa()
        {
            for(int i = 0; i < count; i++)
            {
                _Ws[i] = Ws[i];
            }
        }


        public void Dispose()
        {
            DisposeNa<float4x4>(Ws);
            DisposeTraa(traa);                     
        }

        [BurstCompile]
        struct WfromLJob : IJobParallelForTransform
        {
            public int type;
            
            public float3 info;
            public float3 center;

            public NativeArray<float4x4> Ws;

            public void Execute(int i, TransformAccess tr)
            {
                float4x4 W = float4x4.identity;
                float3x3 R = float3x3.identity;
                float3x3 R0 = float3x3.identity;

                float3 pos = tr.position;
                quaternion rot = tr.rotation;
                float3 sca = tr.localScale;

                float r = info.x;
                float d = info.y;
                float3 c = center;
                               
                R = new float3x3(rot);

                if (type == 0)
                {
                    float s = math.max(sca.x, math.max(sca.y, sca.z));

                    W.c3 = new float4(pos + math.mul(R, sca * c), 1.0f);
                    R = R * s * r;
                    W.c0 = new float4(R.c0, 0.0f);
                    W.c1 = new float4(R.c1, 0.0f);
                    W.c2 = new float4(R.c2, 0.0f);
                }
                else if(type == 1)
                {
                    if (d == 0)
                    {                       
                        R0 = new float3x3(math.mul(rot, quaternion.AxisAngle(new float3(0.0f, 0.0f, 1.0f), math.radians(-90.0f))));
                    }
                    else if (d == 1)
                    {
                        R0 = R;
                    }
                    else if (d == 2)
                    {                       
                        R0 = new float3x3(math.mul(rot, quaternion.AxisAngle(new float3(1.0f, 0.0f, 0.0f), math.radians(+90.0f))));
                    }

                    W.c3 = new float4(pos + math.mul(R, sca * c), 1.0f);

                    W.c0 = new float4(R0.c0, 0.0f);
                    W.c1 = new float4(R0.c1, 0.0f);
                    W.c2 = new float4(R0.c2, 0.0f);                   
                }
 
                
               

                Ws[i] = W;
            }
        }

        void DisposeNa<T>(NativeArray<T> na) where T : struct
        {
            if (na.IsCreated) na.Dispose(); 
        }

        void DisposeTraa(TransformAccessArray traa)
        {
            if (traa.isCreated) traa.Dispose();
        }
    }
    
}
