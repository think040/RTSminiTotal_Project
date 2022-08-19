using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Unity.Mathematics;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using Utility_JSB;

public class ArrowManager : MonoBehaviour
{
    public ComputeShader cshader;
    public Shader gshader;
    public GameObject arrowPrefab;
    public static int cCount
    { get; set; } = 256;

    public static ArrowConst[] arrowConstData;
    public ArrowBoneOut[] bonesOutData;
    NativeArray<ArrowConst> arrowConstNA;

    public GameObject[] arrow;
    //AudioSource[] audioArrow;
    ArrowActor[] arrowActor;       
    
    [HideInInspector]
    public GameObject[] target;
    [HideInInspector]
    public GameObject[] shooter;

    public static Transform[] arrowTr;
    public static Transform[] shootTr;
    public static Transform[] targetTr;
    public static UnitActor[] sActor;
    public static UnitActor[] tActor;

    public float3 aSca = new float3(0.05f, 0.05f, 0.25f);

    ArrowCompute arrowCom;
    //ArrowJob arrowJob;          

    //void Awake()
    //{
    //    Init();
    //}

    public void Init()
    {
        //GameManager.arrowCount = GameManager.unitCounts[0] * 4;
        //GameManager.arrowCount = 256;

        //cCount = GameManager.arrowCount;
        cCount = GameManager.unitCounts[0] * 4;

        if (cCount > 0)
        {
            arrowConstData = new ArrowConst[cCount];
            arrowConstNA = new NativeArray<ArrowConst>(cCount, Allocator.Persistent);
            arrow = new GameObject[cCount];
            target = new GameObject[cCount];
            shooter = new GameObject[cCount];
            aes = new IEnumerator[cCount];

            arrowTr  = new Transform[cCount];
            shootTr  = new Transform[cCount];
            targetTr = new Transform[cCount];
            sActor   = new UnitActor[cCount];
            tActor   = new UnitActor[cCount];

            arrowActor = new ArrowActor[cCount];

            {
                float3 pos;
                quaternion rot = quaternion.identity;
                float dz = 2.0f;
                float dx = 1.0f;
                //float z0 = -8.0f;
                float z0 = -16.0f;
                //float x0 = -15.0f;
                float x0 = -30.0f;
                float z1 = +10.0f;
                //float x1 = +25.0f;
                float x1 = +30.0f;
                int uNum = 16;

                for (int i = 0; i < cCount; i++)
                {
                    //pos = new float3((float)i * 2.0f, 0.0f, 0.0f);
                    pos = new float3(x0 - (float)(i / uNum) * dx, 0.0f, z0 + (float)(i % uNum) * dz);
                    arrow[i] = GameObject.Instantiate(arrowPrefab, pos, rot);
                    arrow[i].name = "arrow" + i.ToString();
                    arrowTr[i] = arrow[i].transform;
                    arrowActor[i] = arrow[i].GetComponent<ArrowActor>();
                }

                //for (int i = 0; i < cCount; i++)
                //{
                //    //pos = new float3((float)i * 2.0f, 0.0f, 0.0f);
                //    pos = new float3(x0 - (float)(i / uNum) * dx, 0.0f, z0 + (float)(i % uNum) * dz);
                //    shooter[i] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                //    shooter[i].transform.position = pos;
                //    shooter[i].transform.rotation = rot;
                //    shooter[i].name = "shooter" + i.ToString();
                //}
                //
                //for (int i = 0; i < cCount; i++)
                //{
                //    //pos = new float3((float)i * 2.0f, 0.0f, 10.0f);
                //    pos = new float3(x1, 0.0f, z0 + (float)(i % uNum) * dz);
                //    target[i] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                //    target[i].transform.position = pos;
                //    target[i].transform.rotation = rot;
                //    target[i].name = "target" + i.ToString();
                //}

                for (int i = 0; i < cCount; i++)
                {
                    ArrowConst ad = new ArrowConst();
                    //ad.p0 = shooter[i].transform.position;
                    //ad.p1 = target[i].transform.position;
                    //ad.pi = arrow[i].transform.position;

                    ad.sca = aSca;
                    ad.u = 0.0f;
                    ad.active = false;
                    //ad.active = true;
                    arrowConstData[i] = ad;
                    arrowConstNA[i] = ad;
                }
            }

            {
                arrowCom = new ArrowCompute();
                arrowCom.Init(cshader, arrowConstData);

                ac = arrowCom.Compute();

                StartCoroutine(ac);
                //GamePlay.UpdateBone += UpdateArrow;
            }

            {
                for (int i = 0; i < cCount; i++)
                {
                    aes[i] = ArrowAction(i);
                }

                arrowCom.PreCompute +=
                    () =>
                    {
                        for (int i = 0; i < cCount; i++)
                        {
                            aes[i].MoveNext();
                        }
                    };
            }

            InitRenderArrow();

            {
                //InitArrowTestButton();
            }

            SendDataToGameManager();
        }        
    }

    public void Begin()
    {
        for (int i = 0; i < cCount; i++)
        {
            arrowActor[i].Begin();
        }
    }

    void SendDataToGameManager()
    {
        //Debug.Log("Arrow count : " + cCount.ToString());
        for (int i = 0; i < cCount; i++)
        {
            //Debug.Log("Arrow idx : " + i.ToString());
            GameManager.arrowTrs[i] = arrow[i].transform;
        }
    }

    public static void ShootArrow(UnitActor sactor, Transform sTr, Transform tTr)
    {
        for (int i = 0; i < cCount; i++)
        {
            if (arrowConstData[i].active == false)
            {
                arrowConstData[i].active = true;
                shootTr[i] = sTr;
                targetTr[i] = tTr;
                sActor[i] = sactor;
                tActor[i] = tTr.GetComponent<UnitActor>();
                break;
            }
        }
    }

    void UpdateArrow()
    {
        ac.MoveNext();
    }

    IEnumerator ArrowAction(int i)
    {
        float u; ;
        float da_du = 8.0f;
        float arcLength = 10.0f;

        float yOffset = 1.5f;
        Transform tTr = null;

        while (true)
        {
            if (arrowConstData[i].active)
            {
                //{
                //    ArrowConst ad = arrowConstData[i];
                //    ad.p0 = shooter[i].transform.position;
                //    ad.p1 = target[i].transform.position;
                //    ad.pi = arrow[i].transform.position;
                //    u = ad.u;
                //    arrowConstData[i] = ad;
                //}

                {
                    tTr = targetTr[i];

                    ArrowConst ad = arrowConstData[i];
                    ad.p0 = shootTr[i].position;
                    ad.p1 = tTr.position + new Vector3(0.0f, tTr.localScale.y * yOffset, 0.0f);
                    ad.pi = arrowTr[i].position;
                    u = ad.u;
                    arrowConstData[i] = ad;
                }

                while (0.0f <= u && u <= 1.0f)
                {
                    yield return null;

                    //{
                    //    ArrowOut ao = arrowCom.arrowData[i];
                    //    arrow[i].transform.position = ao.pos.xyz;
                    //    arrow[i].transform.rotation = new quaternion(ao.rot);
                    //    arcLength = ao.pos.w;
                    //
                    //    ArrowConst ad = arrowConstData[i];
                    //    ad.p1 = target[i].transform.position;
                    //    ad.pi = arrow[i].transform.position;
                    //
                    //    u = u + da_du * Time.deltaTime / arcLength;
                    //    ad.u = u;
                    //
                    //    arrowConstData[i] = ad;
                    //}

                    {
                        ArrowOut ao = arrowCom.arrowData[i];
                        arrowTr[i].position = ao.pos.xyz;
                        arrowTr[i].rotation = new quaternion(ao.rot);
                        arcLength = ao.pos.w;

                        ArrowConst ad = arrowConstData[i];
                        ad.p1 = tTr.position + new Vector3(0.0f, tTr.localScale.y * yOffset, 0.0f);
                        ad.pi = arrowTr[i].position;

                        u = u + da_du * Time.deltaTime / arcLength;
                        ad.u = u;

                        arrowConstData[i] = ad;
                    }
                }
                u = 0.0f;

                if (tActor[i].isActive && arrowConstData[i].active)
                {
                    tActor[i].DamageHp(hitHp);                    
                    arrowActor[i].AudioPlay();                    
                }

                {
                    ArrowConst ad = arrowConstData[i];
                    ad.u = u;
                    ad.active = false;
                    arrowConstData[i] = ad;
                }
            }

            yield return null;
        }
    }

    void Start()
    {
        //InitRenderArrow();
    }

    public Color _arrowColor = Color.yellow;
    public static Color[] aColor
    {
        get; set;
    }

    public static Color[] aColorDef
    {
        get; set;
    } = { Color.yellow };

    //public static bool useEditor;
    static ArrowManager()
    {
        aColor = new Color[1];
        //aColor[0] = Color.red;
        aColor[0] = Color.yellow;
    }

    void InitRenderArrow()
    {
        //int passColor;
        //int passDepth;
        {
            //useEditor = GameManager.useEditor;

            if(GameManager.useEditor)
            {
                aColor[0] = _arrowColor;
            }           
        }


        {
            csmAction = light.GetComponent<CSMAction>();
            csmCount = csmAction.csmNum;
            drenTexs = csmAction.drenTex_array;
            //drenTexArray = csmAction.drenTexArray;
            dw = csmAction.dw;
            dh = csmAction.dh;
            dws = csmAction.dws;
            dhs = csmAction.dhs;
            //specularFactor = CSMAction.specularFactor;
            specularPow = 2.0f;
        }

        {
            mte = new Material(gshader);
            mpb = new MaterialPropertyBlock();
            //passColor = mte.FindPass("ArrowColor");
            //passDepth = mte.FindPass("ArrowDepth");

            Mesh mesh = arrowCom.mesh;
            List<int> idx = new List<int>();
            mesh.GetIndices(idx, 0);
            idxCount = idx.Count;

            idxBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Index, idxCount, sizeof(int));
            idxBuffer.SetData(idx.ToArray());

            mpb.SetBuffer("vtxDynamic", arrowCom.vOutBuffer);
            mpb.SetInt("dvCount", arrowCom.dvCount);
            mpb.SetBuffer("arrowConst", arrowCom.arrowConstBuffer);
            mpb.SetColor("arrowColor", aColor[0]);
        }

        //{
        //    colors = new Color[cCount];
        //    for (int i = 0; i < cCount; i++)
        //    {
        //        colors[i] = Color.red;
        //    }
        //
        //    arrowColors = new ComputeBuffer(cCount, Marshal.SizeOf<Color>(), ComputeBufferType.Structured, ComputeBufferMode.SubUpdates);
        //    RenderGOM.BeginFrameRender +=
        //        (context, cams) =>
        //        {
        //            var na = arrowColors.BeginWrite<Color>(0, cCount);
        //            for (int i = 0; i < cCount; i++)
        //            {
        //                na[i] = colors[i];
        //            }
        //            arrowColors.EndWrite<Color>(cCount);
        //        };
        //
        //    mpb.SetBuffer("arrowColors", arrowColors);
        //}
      

        {
            mpb.SetTexture("csmTexArray", drenTexs);
        }
   

        bool bArray = true;
        {
            if (bArray)
            {
                mpb.SetInt("bArray", 1);
            }
            else
            {
                mpb.SetInt("bArray", 0);
            }

            mpb.SetInt("csmWidth", dw);
            mpb.SetInt("csmHeight", dh);
            mpb.SetFloat("specularPow", specularPow);
        }
       
        passCSM = mte.FindPass("ArrowDepth_GS");
        passColor = mte.FindPass("ArrowColor");


        RenderGOM.RenderCSM_GS += RenderCSM;
        RenderGOM.OnRenderCamAlpha += Render;

        SceneManager.sceneUnloaded += OnSceneLeft;           
    }

    public void OnSceneLeft(Scene scene)
    {
        OnDestroy();

        Debug.Log("sceneUnloaded()");
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
        if(cCount > 0)
        {
            this.cullIdx = cullIdx;

            {
                mpb.SetInt("cullOffset", cullOffset);
                //mpb.SetTexture("",);
            }
        }       
    }

    public void SetCullData(int cullIdx, RenderTexture pvf_tex, RenderTexture ovf_tex)
    {
        if (cCount > 0)
        {
            this.cullIdx = cullIdx;

            {
                mpb.SetInt("cullOffset", cullOffset);
                mpb.SetTexture("cullResult_pvf_Texture", pvf_tex);
                mpb.SetTexture("cullResult_ovf_Texture", ovf_tex);
            }          
        }
    }

    private void RenderCSM(ScriptableRenderContext context, Camera cam)
    {
        CommandBuffer cmd = CommandBufferPool.Get();

        {
            cmd.DrawProcedural(idxBuffer, Matrix4x4.identity, mte, passCSM, MeshTopology.Triangles, idxBuffer.count, cCount, mpb);
        }
       

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }


    private void Render(ScriptableRenderContext context, Camera cam, RenderGOM.PerCamera perCam)
    {
        CommandBuffer cmd = CommandBufferPool.Get();

        perCam.V = RenderUtil.GetVfromW(cam);
        {
            mpb.SetMatrix("CV_view", math.mul(RenderUtil.GetCfromV(cam, false), perCam.V));
            mpb.SetMatrix("CV", perCam.CV);
            mpb.SetMatrixArray("TCV_light", csmAction.TCV_depth);
            mpb.SetFloatArray("endZ", csmAction.endZ);
            mpb.SetVector("dirW_light", csmAction.dirW);
            mpb.SetVector("dirW_view", (Vector3)perCam.dirW_view);
            mpb.SetVector("posW_view", (Vector3)perCam.posW_view);

            cmd.DrawProcedural(idxBuffer, Matrix4x4.identity, mte, passColor, MeshTopology.Triangles, idxBuffer.count, cCount, mpb);
        }       

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

   
    

    public void SetCullTexture(RenderTexture testRt)
    {
        mpb.SetTexture("testCull", testRt);
        mpb.SetInt("cullOffset", cullOffset);

        //arrowJob.SetCullTexture(testRt, cullOffset);
    }

    //public int cullOffset;
    public int[] cullIds;
    public float4[] spCull;
    public Transform[] spTr;
  

    Button btArrowTargetPos;
    Button btArrowTargetNeg;
    
    Button btArrowShootPos;
    Button btArrowShootNeg;

   

    // Update is called once per frame
    void Update()
    {
        
    }
    
    IEnumerator ac;
    IEnumerator[] aes;

    IEnumerator aj;
    
    
   
    public float hitHp
    {
        get { return HpbarManager.hitHp[0]; }
    }

    public static float da_du;
   

    int pass;
    Material mte;
    MaterialPropertyBlock mpb;

    GraphicsBuffer idxBuffer;
    int idxCount;

    public Light light;
    

    CSMAction csmAction;
    int csmCount;
    RenderTexture drenTexs;
    Texture2DArray drenTexArray;
    int passCSM;
    int passColor;
    int dw;
    int dh;
    float[] dws;
    float[] dhs;
    float specularPow;

    //public Color arrowColor = Color.red;
    public Color[] colors;
    //float4[] aColors;
    ComputeBuffer arrowColors;
      

    public void ChangeArrowColor(Color color)
    {        
        for (int i = 0; i < cCount; i++)
        {
            colors[i] = color;
        }
        
        {
            var na = arrowColors.BeginWrite<float4>(0, cCount);
            for (int i = 0; i < cCount; i++)
            {
                Color c = colors[i];
                na[i] = new float4(c.r, c.g, c.b, c.a);
            }
            arrowColors.EndWrite<float4>(cCount);
        }       
    }

    public void ChangeArrowColor()
    {        
        {
            var na = arrowColors.BeginWrite<float4>(0, cCount);
            for (int i = 0; i < cCount; i++)
            {
                Color c = colors[i];
                na[i] = new float4(c.r, c.g, c.b, c.a);
            }
            arrowColors.EndWrite<float4>(cCount);
        }
    }

    public static void ChangeArrowColor1(Color color)
    {
        aColor[0] = color;
    }

    public bool bDebugShader = false;
    public Shader dbShader;
    public Color wireColor = Color.green;
    Material dbMte;
    MaterialPropertyBlock dbMpb;
    int pass_wire;
    Mesh spMesh;
    public float spRadius;
    Matrix4x4[] Ws;    

    void OnDestroy()
    {
        //if (arrowJob != null) arrowJob.Dispose();
        if (arrowConstNA.IsCreated) arrowConstNA.Dispose();

        if (arrowCom != null) arrowCom.ReleaseCShader();
        if (idxBuffer != null) idxBuffer.Dispose();

        {
            GamePlay.UpdateBone -= UpdateArrow;
        }

        {
            RenderGOM.RenderCSM_GS -= RenderCSM;
            RenderGOM.OnRenderCamAlpha -= Render;
        }

        SceneManager.sceneUnloaded -= OnSceneLeft;
    }

    private void OnApplicationQuit()
    {
        OnDestroy();
    }

    class ArrowCompute
    {
        public IEnumerator Compute()
        {
            InitCShader();
            while (true)
            {
                //PreCompute().MoveNext();
                if (GamePlay.isResume)
                {
                    PreCompute();

                    WriteToCShader();
                    ExecuteCShader();
                    ReadFromCShader();

                    PostCompute();
                }
                                
                yield return null;
            }
            ReleaseCShader();
        }

        //public Func<IEnumerator> PreCompute;
        public Action PreCompute = () => { };
        public Action PostCompute = () => { };


        ComputeShader cshader;
        int ki_curve;
        int ki_bone;
        int ki_vertex;

        public Mesh mesh
        {
            get; private set;
        }

        public int bCount
        {
            get; private set;
        }
        int cCount;
        int vtxCount;

        int boCount;

        const int vtCount = 1024;
        int vgCount;
        int dpCount;
        public int dvCount
        {
            get; private set;
        }
        int voCount;

        Vector4 countInfo;

        public ComputeBuffer arrowConstBuffer
        {
            get; private set;
        }
        ComputeBuffer boneZBuffer;
        ComputeBuffer vInBuffer;

        ComputeBuffer curvesBuffer;
        ComputeBuffer bonesBuffer;
        ComputeBuffer arrowDataBuffer;

        public ComputeBuffer vOutBuffer
        {
            get; private set;
        }

        public ArrowConst[] arrowConstData
        {
            get; private set;
        }
        float[] boneZData;
        VertexIn[] vInData;

        public ArrowCurveOut[] curvesData
        {
            get; private set;
        }

        public ArrowBoneOut[] bonesData
        {
            get; private set;
        }

        public VertexOut[] vOutData
        {
            get; private set;
        }

        public ArrowOut[] arrowData
        {
            get; private set;
        }

        public void Init(ComputeShader cshader, ArrowConst[] arrowConstData)
        {
            this.cshader = cshader;
            ki_curve = cshader.FindKernel("ArrowCurveCompute");
            ki_bone = cshader.FindKernel("ArrowBoneCompute");
            ki_vertex = cshader.FindKernel("ArrowVertexCompute");

            this.cCount = arrowConstData.Length;

            List<float> bz;
            mesh = RenderUtil.CreateSphereMesh_ForArrow(1.0f, 12, 24, out bz);
            //mesh = RenderUtil.CreateSphereMesh_ForArrow(1.0f, 6, 6, out bz);
            this.boneZData = bz.ToArray();

            this.bCount = boneZData.Length;
            this.vtxCount = mesh.vertexCount;

            this.boCount = bCount * cCount;

            this.vgCount = vtxCount / vtCount + 1;
            this.dvCount = vgCount * vtCount;
            this.dpCount = vgCount * cCount;
            this.voCount = dvCount * cCount;

            countInfo = new Vector4();
            countInfo[0] = cCount;
            countInfo[1] = vtxCount;
            countInfo[2] = vgCount;
            countInfo[3] = bCount;

            {
                this.arrowConstData = arrowConstData;
                vInData = new VertexIn[vtxCount];

                curvesData = new ArrowCurveOut[cCount];
                bonesData = new ArrowBoneOut[boCount];
                arrowData = new ArrowOut[cCount];
                vOutData = new VertexOut[voCount];
            }

            {
                arrowConstBuffer = new ComputeBuffer(cCount, Marshal.SizeOf<ArrowConst>(), ComputeBufferType.Structured, ComputeBufferMode.SubUpdates);
                boneZBuffer = new ComputeBuffer(bCount, sizeof(float));
                vInBuffer = new ComputeBuffer(vtxCount, Marshal.SizeOf<VertexIn>(), ComputeBufferType.Structured, ComputeBufferMode.Immutable);

                curvesBuffer = new ComputeBuffer(cCount, Marshal.SizeOf<ArrowCurveOut>());
                bonesBuffer = new ComputeBuffer(boCount, Marshal.SizeOf<ArrowBoneOut>());
                arrowDataBuffer = new ComputeBuffer(cCount, Marshal.SizeOf<ArrowOut>());
                vOutBuffer = new ComputeBuffer(voCount, Marshal.SizeOf<VertexOut>());
            }
        }

        void InitCShader()
        {
            {
                boneZBuffer.SetData(boneZData);
            }

            {
                List<Vector3> posList = new List<Vector3>();
                List<Vector3> normalList = new List<Vector3>();
                List<Vector4> tangentList = new List<Vector4>();
                List<Vector4> biList = new List<Vector4>();
                mesh.GetVertices(posList);
                mesh.GetNormals(normalList);
                mesh.GetTangents(tangentList);
                mesh.GetUVs(4, biList);

                for (int i = 0; i < vtxCount; i++)
                {
                    vInData[i].posL = posList[i];
                    vInData[i].normalL = normalList[i];
                    vInData[i].tangentL = tangentList[i];
                    vInData[i].boneI = new int4((int)(biList[i].x), 0, 0, 0);
                }

                vInBuffer.SetData(vInData);
            }

            {
                for (int i = 0; i < cCount; i++)
                {
                    for (int j = 0; j < bCount; j++)
                    {
                        bonesData[i * bCount + j].cIndex = i;
                        bonesData[i * bCount + j].bIndex = j;
                    }
                }

                bonesBuffer.SetData(bonesData);
            }

            {
                cshader.SetBuffer(ki_curve, "arrowConst", arrowConstBuffer);
                cshader.SetBuffer(ki_curve, "arrowData", arrowDataBuffer);
                cshader.SetBuffer(ki_curve, "curves", curvesBuffer);

                cshader.SetBuffer(ki_bone, "arrowConst", arrowConstBuffer);
                cshader.SetBuffer(ki_bone, "curves", curvesBuffer);
                cshader.SetBuffer(ki_bone, "boneZ", boneZBuffer);
                cshader.SetBuffer(ki_bone, "arrowData", arrowDataBuffer);
                cshader.SetBuffer(ki_bone, "bones", bonesBuffer);

                cshader.SetBuffer(ki_vertex, "arrowConst", arrowConstBuffer);
                cshader.SetBuffer(ki_vertex, "bones", bonesBuffer);
                cshader.SetBuffer(ki_vertex, "vIn", vInBuffer);
                cshader.SetBuffer(ki_vertex, "vOut", vOutBuffer);
            }
        }

        void WriteToCShader()
        {
            {
                var na = arrowConstBuffer.BeginWrite<ArrowConst>(0, cCount);
                for (int i = 0; i < cCount; i++)
                {
                    na[i] = arrowConstData[i];
                }
                arrowConstBuffer.EndWrite<ArrowConst>(cCount);
            }
        }

        void ExecuteCShader()
        {
            cshader.SetVector("countInfo", countInfo);

            CommandBuffer cmd = CommandBufferPool.Get();

            cmd.DispatchCompute(cshader, ki_curve, cCount, 1, 1);
            cmd.DispatchCompute(cshader, ki_bone, cCount, 1, 1);
            cmd.DispatchCompute(cshader, ki_vertex, dpCount, 1, 1);

            Graphics.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        void ReadFromCShader()
        {
            //curvesBuffer.GetData(curvesData);
            //bonesBuffer.GetData(bonesData);
            //vOutBuffer.GetData(vOutData);

            //arrowDataBuffer.GetData(arrowData);            
            {
                AsyncGPUReadbackRequest agrr = AsyncGPUReadback.Request(arrowDataBuffer);
                agrr.WaitForCompletion();
                {
                    var na = agrr.GetData<ArrowOut>();
                    for(int i = 0; i < cCount; i++)
                    {
                        arrowData[i] = na[i];
                    }
                }
            }

        }

        public void ReleaseCShader()
        {
            if (arrowConstBuffer != null) arrowConstBuffer.Release();
            if (boneZBuffer != null) boneZBuffer.Release();
            if (vInBuffer != null) vInBuffer.Release();
            if (curvesBuffer != null) curvesBuffer.Release();
            if (bonesBuffer != null) bonesBuffer.Release();
            if (arrowDataBuffer != null) arrowDataBuffer.Release();
            if (vOutBuffer != null) vOutBuffer.Release();
        }

    }

    class ArrowJob
    {
        public IEnumerator Compute()
        {
            InitData();
            while (true)
            {
                //if(GamePlay.bResume)
                {
                    PreCompute();

                    //WriteData();
                    //Execute();
                    //ReadData();

                    WriteToNA();
                    Execute();
                    ReadFromNA();
                    //ComputeCShader();
                    //ReadFromCShader();

                    PostCompute();
                }

                yield return null;
            }
            Release();
        }

        public Action PreCompute = () => { };
        public Action PostCompute = () => { };

        ArrowCurveJob curveJob;
        ArrowBoneJob boneJob;

        NativeArray<ArrowConst> arrowConstNA;
        public NativeArray<ArrowCurveOut> curvesNA;
        NativeArray<float> boneZNA;
        public NativeArray<ArrowOut> arrowOutNA;
        public static NativeArray<ArrowBoneOut> bonesNA;

        ComputeShader cshader;
        //int ki_curve;
        //int ki_bone;
        int ki_vertex;


        public Mesh mesh
        {
            get; private set;
        }

        public int bCount
        {
            get; private set;
        }
        int cCount;
        int vtxCount;
        int hbCount;

        int boCount;

        const int vtCount = 1024;
        int vgCount;
        int dpCount;
        public int dvCount
        {
            get; private set;
        }
        int voCount;

        Vector4 countInfo;

        public ComputeBuffer arrowConstBuffer
        {
            get; private set;
        }
        //ComputeBuffer boneZBuffer;
        ComputeBuffer vInBuffer;

        //ComputeBuffer curvesBuffer;
        ComputeBuffer bonesBuffer;
        //ComputeBuffer arrowDataBuffer;

        public ComputeBuffer vOutBuffer
        {
            get; private set;
        }

        public ArrowConst[] arrowConstData
        {
            get; private set;
        }
        float[] boneZData;
        VertexIn[] vInData;

        public ArrowCurveOut[] curvesData
        {
            get; private set;
        }

        public ArrowBoneOut[] bonesData
        {
            get; private set;
        }

        public VertexOut[] vOutData
        {
            get; private set;
        }

        public ArrowOut[] arrowData
        {
            get; private set;
        }

        public void Init(ComputeShader cshader, ArrowConst[] arrowConstData)
        {
            this.cshader = cshader;
            //ki_curve = cshader.FindKernel("ArrowCurveCompute");
            //ki_bone = cshader.FindKernel("ArrowBoneCompute");
            ki_vertex = cshader.FindKernel("ArrowVertexCompute");

            this.cCount = arrowConstData.Length;

            List<float> bz;
            mesh = RenderUtil.CreateSphereMesh_ForArrow(1.0f, 12, 24, out bz);
            //mesh = RenderUtil.CreateSphereMesh_ForArrow(1.0f, 6, 6, out bz);
            this.boneZData = bz.ToArray();

            this.bCount = boneZData.Length;
            this.hbCount = bCount / 2;
            this.vtxCount = mesh.vertexCount;

            this.boCount = bCount * cCount;

            this.vgCount = vtxCount / vtCount + 1;
            this.dvCount = vgCount * vtCount;
            this.dpCount = vgCount * cCount;
            this.voCount = dvCount * cCount;

            countInfo = new Vector4();
            countInfo[0] = cCount;      //32
            countInfo[1] = vtxCount;    //600
            countInfo[2] = vgCount;     //1
            countInfo[3] = bCount;      //25

            {
                arrowData = new ArrowOut[cCount];
                curvesData = new ArrowCurveOut[cCount];

                this.arrowConstData = arrowConstData;
                bonesData = new ArrowBoneOut[boCount];

                vInData = new VertexIn[vtxCount];
                vOutData = new VertexOut[voCount];
            }

            {
                boneZNA = new NativeArray<float>(bCount, Allocator.Persistent);
                curvesNA = new NativeArray<ArrowCurveOut>(cCount, Allocator.Persistent);
                //arrowOutNA = new NativeArray<ArrowOut>(cCount, Allocator.Persistent);

                arrowConstNA = new NativeArray<ArrowConst>(cCount, Allocator.Persistent);
                bonesNA = new NativeArray<ArrowBoneOut>(boCount, Allocator.Persistent);
            }

            {
                //boneZBuffer = new ComputeBuffer(bCount, sizeof(float));               
                //curvesBuffer = new ComputeBuffer(cCount, Marshal.SizeOf<ArrowCurveOut>());                
                //arrowDataBuffer = new ComputeBuffer(cCount, Marshal.SizeOf<ArrowOut>());

                arrowConstBuffer = new ComputeBuffer(cCount, Marshal.SizeOf<ArrowConst>(), ComputeBufferType.Structured, ComputeBufferMode.SubUpdates);
                //bonesBuffer = new ComputeBuffer(boCount, Marshal.SizeOf<ArrowBoneOut>(), ComputeBufferType.Structured, ComputeBufferMode.SubUpdates);
                bonesBuffer = new ComputeBuffer(boCount, Marshal.SizeOf<ArrowBoneOut>());

                vInBuffer = new ComputeBuffer(vtxCount, Marshal.SizeOf<VertexIn>(), ComputeBufferType.Structured, ComputeBufferMode.Immutable);
                vOutBuffer = new ComputeBuffer(voCount, Marshal.SizeOf<VertexOut>());
            }

            {
                curveJob = new ArrowCurveJob();
                boneJob = new ArrowBoneJob();
            }


            RenderGOM.BeginFrameRender += ComputeCShader;
        }


        public void InitData()
        {
            {
                //boneZBuffer.SetData(boneZData);
                //boneZNA.CopyTo(boneZData

                //boneZNA.CopyFrom(boneZData);
                for (int i = 0; i < bCount; i++)
                {
                    boneZNA[i] = boneZData[i];
                }
            }

            {
                List<Vector3> posList = new List<Vector3>();
                List<Vector3> normalList = new List<Vector3>();
                List<Vector4> tangentList = new List<Vector4>();
                List<Vector4> biList = new List<Vector4>();
                mesh.GetVertices(posList);
                mesh.GetNormals(normalList);
                mesh.GetTangents(tangentList);
                mesh.GetUVs(4, biList);

                for (int i = 0; i < vtxCount; i++)
                {
                    vInData[i].posL = posList[i];
                    vInData[i].normalL = normalList[i];
                    vInData[i].tangentL = tangentList[i];
                    vInData[i].boneI = new int4((int)(biList[i].x), 0, 0, 0);
                }

                vInBuffer.SetData(vInData);
            }

            {
                for (int i = 0; i < cCount; i++)
                {
                    for (int j = 0; j < bCount; j++)
                    {
                        int idx = i * bCount + j;
                        bonesData[idx].cIndex = i;
                        bonesData[idx].bIndex = j;
                        bonesNA[idx] = bonesData[idx];
                    }
                }

                //bonesBuffer.SetData(bonesData);
                //bonesNA.CopyTo(bonesData);
                //bonesNA.CopyFrom(bonesData);
            }

            {
                //cshader.SetBuffer(ki_curve, "arrowConst", arrowConstBuffer);
                //cshader.SetBuffer(ki_curve, "arrowData", arrowDataBuffer);
                //cshader.SetBuffer(ki_curve, "curves", curvesBuffer);
                //
                //cshader.SetBuffer(ki_bone, "arrowConst", arrowConstBuffer);
                //cshader.SetBuffer(ki_bone, "curves", curvesBuffer);
                //cshader.SetBuffer(ki_bone, "boneZ", boneZBuffer);
                //cshader.SetBuffer(ki_bone, "arrowData", arrowDataBuffer);
                //cshader.SetBuffer(ki_bone, "bones", bonesBuffer);

                cshader.SetBuffer(ki_vertex, "arrowConst", arrowConstBuffer);
                cshader.SetBuffer(ki_vertex, "bones", bonesBuffer);
                cshader.SetBuffer(ki_vertex, "vIn", vInBuffer);
                cshader.SetBuffer(ki_vertex, "vOut", vOutBuffer);
            }

            {
                curveJob.arrowData = arrowConstNA;
                curveJob.curves = curvesNA;

                boneJob.arrowData = arrowConstNA;
                boneJob.curves = curvesNA;
                boneJob.boneZ = boneZNA;
                boneJob.bones = bonesNA;
                //boneJob.arrowOut = arrowOutNA;

                boneJob.cCount = cCount;
                boneJob.bCount = bCount;
            }
        }


        public void WriteToNA()
        {
            {
                for (int i = 0; i < cCount; i++)
                {
                    arrowConstNA[i] = arrowConstData[i];
                }
                //arrowConstNA.CopyFrom(arrowConstData);

            }
        }

        public void Execute()
        {
            boneJob.Schedule(boCount, 1, curveJob.Schedule(cCount, 1)).Complete();
        }


        public void ReadFromNA()
        {
            {
                var na = arrowConstBuffer.BeginWrite<ArrowConst>(0, cCount);
                for (int i = 0; i < cCount; i++)
                {
                    //na[i] = arrowConstNA[i];
                    na[i] = arrowConstData[i];
                }
                arrowConstBuffer.EndWrite<ArrowConst>(cCount);
            }

            {
                //for (int i = 0; i < boCount; i++)
                //{
                //    bonesData[i] = bonesNA[i];
                //}
                //
                //var na = bonesBuffer.BeginWrite<ArrowBoneOut>(0, boCount);
                //for (int i = 0; i < boCount; i++)
                //{
                //    //na[i] = bonesNA[i];
                //    na[i] = bonesData[i];                    
                //}                
                //bonesBuffer.EndWrite<ArrowBoneOut>(boCount);

                bonesBuffer.SetData(bonesNA);
            }

            for (int i = 0; i < cCount; i++)
            {
                //ArrowBoneOut ab = bonesNA[i * bCount + (bCount / 2)];
                ArrowBoneOut ab = bonesNA[i * bCount + hbCount];
                //ArrowBoneOut ab = bonesNA[i * bCount];
                //ArrowBoneOut ab = bonesNA[i * bCount + 2 * hbCount];
                arrowData[i].pos = new float4(ab.pos, 0.0f);
                //arrowData[i].pos = float4.zero;
                arrowData[i].rot = ab.rot;
            }
        }

        public void ComputeCShader()
        {
            {
                cshader.SetVector("countInfo", countInfo);

                CommandBuffer cmd = CommandBufferPool.Get();

                //cmd.DispatchCompute(cshader, ki_curve, cCount, 1, 1);
                //cmd.DispatchCompute(cshader, ki_bone, cCount, 1, 1);
                //cmd.SetComputeBufferParam(cshader, ki_vertex, "arrowConst", arrowConstBuffer);
                //cmd.SetComputeBufferParam(cshader, ki_vertex, "bones", bonesBuffer);

                cmd.DispatchCompute(cshader, ki_vertex, dpCount, 1, 1);

                Graphics.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }
        }

        public void ComputeCShader(ScriptableRenderContext contex, Camera[] cams)
        {
            ComputeCShader();
        }

        public void ReadFromCShader()
        {
            //vOutBuffer.GetData(vOutData);
        }

        public void SetCullTexture(RenderTexture testRt, int cullOffset)
        {
            cshader.SetTexture(ki_vertex, "testCull", testRt);
            cshader.SetInt("cullOffset", cullOffset);
        }

        public void Dispose()
        {
            if (boneZNA.IsCreated) boneZNA.Dispose();
            if (curvesNA.IsCreated) curvesNA.Dispose();

            if (arrowConstNA.IsCreated) arrowConstNA.Dispose();
            if (bonesNA.IsCreated) bonesNA.Dispose();
            //if (arrowOutNA.IsCreated) arrowOutNA.Dispose();
        }

        public void Release()
        {
            if (arrowConstBuffer != null) arrowConstBuffer.Release();
            if (bonesBuffer != null) bonesBuffer.Release();

            if (vInBuffer != null) vInBuffer.Release();
            if (vOutBuffer != null) vOutBuffer.Release();
        }

        [BurstCompile]
        struct ArrowCurveJob : IJobParallelFor
        {
            [ReadOnly]
            public NativeArray<ArrowConst> arrowData;

            int cnum;
            public NativeArray<ArrowCurveOut> curves;

            public void Execute(int i)
            {
                ArrowConst constData = arrowData[i];

                if (constData.active)
                {
                    float u = constData.u;
                    if (0.0f <= u && u <= 1.0f)
                    {
                        ArrowCurveOut curveData;

                        float3 p0 = constData.p0;
                        float3 p1 = constData.p1;
                        float3 pi = constData.pi;

                        float3 vec = float3.zero;
                        vec = p1 - p0;
                        float dup = -vec.y;
                        float3 dir0 = new float3(vec.x, 0.0f, vec.z);
                        vec = pi - p0;
                        float3 dir1 = new float3(vec.x, 0.0f, vec.z);
                        float3 dir2 = math.normalize(dir0);
                        float3 dir3 = dir1 - dir2 * math.dot(dir2, dir1);
                        float up = math.length(dir0);

                        float3 dp0 = new float3(dir0.x, +up, dir0.z) + (-2.0f) * dir3;
                        float3 dp1 = new float3(dir0.x, -up, dir0.z) + (+2.0f) * dir3;

                        float4x3 MG = HermiteSpline.getMG(p0, p1, dp0, dp1);
                        float arrowLength = HermiteSpline.GetArcLengthMG(MG, 0.001f);
                        float3 sca = arrowData[i].sca;

                        float4x4 m0 = float4x4.identity;
                        m0.c0.x = sca.x;
                        m0.c1.y = sca.y;
                        m0.c2.z = sca.z;
                        m0.c3.z = arrowLength * u;

                        float4x4 m1 = float4x4.identity;
                        m1.c2.z = 1.0f / arrowLength;

                        m0 = math.mul(m1, m0);

                        curveData.dp0 = dp0;
                        curveData.dp1 = dp1;
                        curveData.arcLength = arrowLength;
                        curveData.L = m0;

                        curves[i] = curveData;
                    }
                }
            }
        }

        [BurstCompile]
        struct ArrowBoneJob : IJobParallelFor
        {
            [ReadOnly]
            public NativeArray<ArrowConst> arrowData;
            [ReadOnly]
            public NativeArray<float> boneZ;

            public int cCount;
            [ReadOnly]
            public NativeArray<ArrowCurveOut> curves;

            public int bCount;
            public NativeArray<ArrowBoneOut> bones;
            //public NativeArray<ArrowOut> arrowOut;

            public void Execute(int j)
            {
                int i = bones[j].cIndex;
                int k = bones[j].bIndex;
                ArrowConst constData = arrowData[i];

                if (constData.active)
                {
                    float u = constData.u;
                    if (0.0f <= u && u <= 1.0f)
                    {
                        ArrowCurveOut curveData = curves[i];
                        ArrowBoneOut boneData = bones[j];

                        float3 p0 = constData.p0;
                        float3 p1 = constData.p1;
                        float3 sca = constData.sca;

                        float3 dp0 = curveData.dp0;
                        float3 dp1 = curveData.dp1;
                        float4x4 L = curveData.L;

                        float4x4 bone = float4x4.identity;
                        float4x4 boneIT = float4x4.identity;
                        quaternion rot = quaternion.identity;
                        float3 pos = float3.zero;

                        float4 center = new float4(0.0f, 0.0f, boneZ[k], 1.0f);
                        float l = math.mul(L, center).z;
                        float4x3 MG = HermiteSpline.getMG(p0, p1, dp0, dp1);
                        pos = HermiteSpline.CurveMG(l, MG);

                        float3 tangent = HermiteSpline.TCurveMG(l, MG);
                        float3 normal = HermiteSpline.NCurveMG(l, MG);
                        float3 bnormal = math.normalize(math.cross(tangent, normal));
                        tangent = math.normalize(tangent);
                        normal = math.cross(bnormal, tangent);

                        bone.c0 = new float4(sca.x * normal, 0.0f);
                        bone.c1 = new float4(sca.y * bnormal, 0.0f);
                        bone.c2 = new float4(sca.z * tangent, 0.0f);
                        bone.c3 = new float4(pos, 1.0f);
                        rot = quaternion.LookRotation(math.normalize(bone.c2.xyz), math.normalize(bone.c1.xyz));

                        float3 sca_r = 1.0f / sca;
                        boneIT.c0 = new float4(sca_r.x * normal, 0.0f);
                        boneIT.c1 = new float4(sca_r.y * bnormal, 0.0f);
                        boneIT.c2 = new float4(sca_r.z * tangent, 0.0f);

                        boneData.bone = bone;
                        boneData.boneIT = boneIT;
                        boneData.rot = rot.value;
                        boneData.pos = pos;

                        bones[j] = boneData;

                        //if(k == bCount / 2)
                        //{
                        //    ArrowOut ao;
                        //    ao.pos = new float4(pos, 0.0f);
                        //    ao.rot = rot.value;
                        //    //arrowOut[i] = ao;
                        //}
                    }
                }
            }
        }

        //Test
        public void Init(ComputeShader cshader, NativeArray<ArrowConst> arrowConsts)
        {
            this.cshader = cshader;
            //ki_curve = cshader.FindKernel("ArrowCurveCompute");
            //ki_bone = cshader.FindKernel("ArrowBoneCompute");
            ki_vertex = cshader.FindKernel("ArrowVertexCompute");

            this.cCount = arrowConsts.Length;

            List<float> bz;
            mesh = RenderUtil.CreateSphereMesh_ForArrow(1.0f, 12, 24, out bz);
            //mesh = RenderUtil.CreateSphereMesh_ForArrow(1.0f, 6, 6, out bz);
            this.boneZData = bz.ToArray();

            this.bCount = boneZData.Length;
            this.hbCount = bCount / 2;
            this.vtxCount = mesh.vertexCount;

            this.boCount = bCount * cCount;

            this.vgCount = vtxCount / vtCount + 1;
            this.dvCount = vgCount * vtCount;
            this.dpCount = vgCount * cCount;
            this.voCount = dvCount * cCount;

            countInfo = new Vector4();
            countInfo[0] = cCount;
            countInfo[1] = vtxCount;
            countInfo[2] = vgCount;
            countInfo[3] = bCount;

            {
                arrowData = new ArrowOut[cCount];
                curvesData = new ArrowCurveOut[cCount];

                //this.arrowConstData = arrowConstData;
                bonesData = new ArrowBoneOut[boCount];

                vInData = new VertexIn[vtxCount];
                vOutData = new VertexOut[voCount];
            }

            {
                boneZNA = new NativeArray<float>(bCount, Allocator.Persistent);
                curvesNA = new NativeArray<ArrowCurveOut>(cCount, Allocator.Persistent);
                //arrowOutNA = new NativeArray<ArrowOut>(cCount, Allocator.Persistent);

                arrowConstNA = arrowConsts;
                bonesNA = new NativeArray<ArrowBoneOut>(boCount, Allocator.Persistent);
            }

            {
                //boneZBuffer = new ComputeBuffer(bCount, sizeof(float));               
                //curvesBuffer = new ComputeBuffer(cCount, Marshal.SizeOf<ArrowCurveOut>());                
                //arrowDataBuffer = new ComputeBuffer(cCount, Marshal.SizeOf<ArrowOut>());

                arrowConstBuffer = new ComputeBuffer(cCount, Marshal.SizeOf<ArrowConst>(), ComputeBufferType.Structured, ComputeBufferMode.SubUpdates);
                //bonesBuffer = new ComputeBuffer(boCount, Marshal.SizeOf<ArrowBoneOut>(), ComputeBufferType.Structured, ComputeBufferMode.SubUpdates);
                bonesBuffer = new ComputeBuffer(boCount, Marshal.SizeOf<ArrowBoneOut>());

                vInBuffer = new ComputeBuffer(vtxCount, Marshal.SizeOf<VertexIn>(), ComputeBufferType.Structured, ComputeBufferMode.Immutable);
                vOutBuffer = new ComputeBuffer(voCount, Marshal.SizeOf<VertexOut>());
            }

            {
                curveJob = new ArrowCurveJob();
                boneJob = new ArrowBoneJob();
            }


        }

        public void WriteData()
        {
            {
                var na = arrowConstBuffer.BeginWrite<ArrowConst>(0, cCount);
                for (int i = 0; i < cCount; i++)
                {
                    na[i] = arrowConstData[i];
                    arrowConstNA[i] = arrowConstData[i];
                }
                arrowConstBuffer.EndWrite<ArrowConst>(cCount);
            }

            {
                var na = bonesBuffer.BeginWrite<ArrowBoneOut>(0, boCount);
                for (int i = 0; i < boCount; i++)
                {
                    na[i] = bonesNA[i];
                }
                bonesBuffer.EndWrite<ArrowBoneOut>(boCount);
            }

        }

        public void ReadData()
        {
            for (int i = 0; i < cCount; i++)
            {
                curvesData[i] = curvesNA[i];
            }

            for (int i = 0; i < boCount; i++)
            {
                bonesData[i] = bonesNA[i];
            }

            for (int i = 0; i < cCount; i++)
            {
                ArrowBoneOut ab = bonesNA[i * bCount + (bCount / 2)];
                arrowData[i].pos = new float4(ab.pos, 0.0f);
                arrowData[i].rot = ab.rot;
            }

            vOutBuffer.GetData(vOutData);
        }
    }


    [System.Serializable]
    public struct ArrowConst
    {
        public bool active;

        public float u;
        public float3 sca;

        public float3 pi;
        public float3 p0;
        public float3 p1;
    }

    
    struct ArrowCurveOut
    {
        public float3 dp0;
        public float3 dp1;

        public float arcLength;
        public float4x4 L;
    }

    [System.Serializable]
    public struct ArrowBoneOut
    {
        public int cIndex;
        public int bIndex;

        public float4x4 bone;
        public float4x4 boneIT;

        public float3 pos;
        public float4 rot;
    }

    struct ArrowOut
    {
        public float4 pos;
        public float4 rot;
    };

    struct VertexIn
    {
        public float3 posL;
        public float3 normalL;
        public float4 tangentL;
        public int4 boneI;
        public float4 boneW;
    }

    struct VertexOut
    {
        public float3 posW;
        public float3 normalW;
        public float4 tangentW;
        public int4 boneI;
    }


    //Test    
    void InitArrowTestButton()
    {
        btArrowTargetPos.onClick.AddListener(
            () =>
            {
                for (int i = 0; i < cCount; i++)
                {
                    target[i].transform.position += new Vector3(0.0f, 0.0f, +1.0f);
                }
            });

        btArrowTargetNeg.onClick.AddListener(
            () =>
            {
                for (int i = 0; i < cCount; i++)
                {
                    target[i].transform.position += new Vector3(0.0f, 0.0f, -1.0f);
                }
            });

        btArrowShootPos.onClick.AddListener(
           () =>
           {
               for (int i = 0; i < cCount; i++)
               {
                   shooter[i].transform.position += new Vector3(0.0f, 0.0f, +1.0f);
               }
           });

        btArrowShootNeg.onClick.AddListener(
            () =>
            {
                for (int i = 0; i < cCount; i++)
                {
                    shooter[i].transform.position += new Vector3(0.0f, 0.0f, -1.0f);
                }
            });
    }       

    public void Resume()
    {
        StartCoroutine(aj);
    }

    public void Pause()
    {
        StopCoroutine(aj);
    }
}
