using System.Collections;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Threading.Tasks;

using Unity.Mathematics;

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

using UserAnimSpace;

public class BattleGroundManager : MonoBehaviour
{
    //public GameObject model;
    public SkinMeshBones skmb;
    
    //public Transform rootTr;
    public Shader gshader;
    public ComputeShader cshader;

    int stCount;
    string[] stNames;
    Mesh[] stMeshes;

    GraphicsBuffer stVtxBuffer;
    GraphicsBuffer stIdxBuffer;

    //CSMAction csmAction;
    //int csmCount;
    //RenderTexture[] drenTexs;
    //Texture2DArray drenTexArray;
    //int passCSM;
    //int passColor;
    //int dw;
    //int dh;
    //float[] dws;
    //float[] dhs;    

    

    //void Awake()
    //{
    //    InitData();
    //    InitBone();
    //    InitMesh();
    //    //InitRendering();
    //
    //    RenderGOM.BeginFrameRender += BeginFrameRender;
    //        
    //    SceneManager.sceneUnloaded += OnSceneLeft;
    //}

    public void Init()
    {
        InitData();
        InitBone();
        InitMesh();
        InitRendering();

        RenderGOM.BeginFrameRender += BeginFrameRender;

        SceneManager.sceneUnloaded += OnSceneLeft;
    }

    void OnSceneLeft(Scene scene)
    {
        OnDestroy();

        Debug.Log("sceneUnloaded()");
    }

    void BeginFrameRender(ScriptableRenderContext context, Camera[] cams)
    {
        vtxComputeSt.ComputeVertex(context);
    }

    void OnDestroy()
    {
        if (vtxComputeSt != null) vtxComputeSt.ReleaseCShader();
        if (stVtxBuffer != null) stVtxBuffer.Dispose();
        if (stIdxBuffer != null) stIdxBuffer.Dispose();

        RenderGOM.BeginFrameRender -= BeginFrameRender;

        RenderGOM.RenderCSM_GS -= RenderCSM;
        RenderGOM.OnRenderCam -= Render;

        SceneManager.sceneUnloaded -= OnSceneLeft;
    }

    private void OnApplicationQuit()
    {
        OnDestroy();
    }

    void Start()
    {
        //InitRendering();
    }

    
    void Update()
    {
        for (int i = 0; i < trs0.Length; i++)
        {
            bns0[i].transform.posL = trs0[i].localPosition;
            bns0[i].transform.rotL = trs0[i].localRotation;
            bns0[i].transform.scaL = trs0[i].localScale;
        }

        Parallel.For(0, bns1.Length,
            (int i) =>
            {
                stW[i] = bns1[i].WfromL;
                //trM[i] = BoneNode.MFromW(stW[i]);                
            });

        //for(int i = 0; i < trs1.Length; i++)
        //{
        //    trs1[i].position = trM[i].c0.xyz;
        //    trs1[i].rotation = new quaternion(trM[i].c1);
        //    trs1[i].localScale = trM[i].c2.xyz;
        //}
    }

    void InitData()
    {        
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

    float4x4[] trM;
    float4x4[] stW;
    VertexComputeSt vtxComputeSt;

   
    BoneNode rootBn;
    BoneNode[] bns0;
    BoneNode[] bns1;

    //public Transform rootTr;
    Transform[] trs0;
    Transform[] trs1;

    //public Transform[] trs;
    //BoneNode[] bns;
    //int trCount;

    void InitBone()
    {                
        {
            rootBn = new BoneNode();
            
            BoneNode.ConstructBone(transform, rootBn);
            bns0 = BoneNode.ToArray(rootBn);
            trs0 = BoneNode.ToArray(transform);
            //bns1 = new BoneNode[bns0.Length - 1];
            //trs1 = new Transform[trs0.Length - 1];

            var tempTr = new List<Transform>();
            var tempBn = new List<BoneNode>();
            for(int i = 0; i < bns0.Length; i++)
            {
                if(bns0[i].parent != null)
                {
                    tempBn.Add(bns0[i]);
                    tempTr.Add(trs0[i]);                                        
                }                
            }
            bns1 = tempBn.ToArray();
            trs1 = tempTr.ToArray();
            
            {
                stW = new float4x4[bns1.Length];
                trM = new float4x4[bns1.Length];

                for (int i = 0; i < stCount; i++)
                {
                    stW[i] = float4x4.zero;
                }

                vtxComputeSt = new VertexComputeSt();
                vtxComputeSt.Init(cshader, stMeshes, stW, 1);
            }
        }
    }

    Material stMte;
    MaterialPropertyBlock stMpb;
    
    void InitMesh()
    {        
        {
            stMte = new Material(gshader);
            stMpb = new MaterialPropertyBlock();            

            SubMeshDescriptor[][] stSmdesc = new SubMeshDescriptor[stCount][];
            int[][][] sbIdx = new int[stCount][][];

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


            Color[] colors = skmb.stColors.ToArray();
            int offset = 0;
            for (int i = 0; i < stCount; i++)
            {
                Mesh mesh = stMeshes[i];               

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

            stVtxBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, vtxCount, Marshal.SizeOf<VertexStatic>());
            stIdxBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Index, idxCount, sizeof(int));

            stVtxBuffer.SetData(vtxData.ToArray());
            stIdxBuffer.SetData(idxData.ToArray());

            stMpb.SetInt("useTex", 0);
            stMpb.SetBuffer("vtxStatic", stVtxBuffer);

            stMpb.SetBuffer("vtxDynamic", vtxComputeSt.vOut);
            stMpb.SetInt("dvCount", vtxComputeSt.dvCount);
        }
    }

    public Transform trLight;


    
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
            stMpb.SetTexture("csmTexArray", drenTexs);                   
        }

        bool bArray = true;       
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
        }

        passCSM = stMte.FindPass("BGShaderDepth_GS");
        passColor = stMte.FindPass("BGShaderColor");

        RenderGOM.RenderCSM_GS += RenderCSM;
        RenderGOM.OnRenderCam += Render;       
    }

    private void RenderCSM(ScriptableRenderContext context, Camera cam)
    {          
        CommandBuffer cmd = CommandBufferPool.Get();
           
        {
            cmd.DrawProcedural(stIdxBuffer, Matrix4x4.identity, stMte, passCSM, MeshTopology.Triangles, stIdxBuffer.count, 1, stMpb);
        }
            
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    private void Render(ScriptableRenderContext context, Camera cam, RenderGOM.PerCamera perCam)
    {
        CommandBuffer cmd = CommandBufferPool.Get();

        float4x4 V = RenderUtil.GetVfromW(cam);

        {
            stMpb.SetMatrix("CV_view", math.mul(RenderUtil.GetCfromV(cam, false), V));
            stMpb.SetMatrix("CV", perCam.CV);
            stMpb.SetMatrixArray("TCV_light", csmAction.TCV_depth);
            stMpb.SetFloatArray("endZ", csmAction.endZ);
            stMpb.SetVector("dirW_light", csmAction.dirW);
            stMpb.SetVector("dirW_view", (Vector3)perCam.dirW_view);
            stMpb.SetVector("posW_view", (Vector3)perCam.posW_view);

            cmd.DrawProcedural(stIdxBuffer, Matrix4x4.identity, stMte, passColor, MeshTopology.Triangles, stIdxBuffer.count, 1, stMpb);
        }

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }



    class VertexComputeSt
    {
        public void ComputeVertex(ScriptableRenderContext context)
        {
            WriteToCShader();
            ExecuteCShaderVertex(context);
            //ReadFromCShader();
        }

        ComputeShader cshader;
        int kindex;

        VertexIn[] vInData;
        PerBone[] perBoneData;
        VertexOut[] vOutData;

        ComputeBuffer vIn;
        ComputeBuffer perBone;
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

        float4x4[] Wbone;
        Mesh stMesh;
        Mesh[] stMeshes;
        public void Init(ComputeShader cshader, Mesh[] stMeshes, float4x4[] Wbone, int insCount)
        {
            this.cshader = cshader;
            this.kindex = cshader.FindKernel("CS_Static_Ins");

            //this.stMesh = stMesh;

            this.Wbone = Wbone;
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

            {
                perBoneData = new PerBone[boneCount];
                perBone = new ComputeBuffer(perBoneData.Length, Marshal.SizeOf<PerBone>(), ComputeBufferType.Structured, ComputeBufferMode.SubUpdates);
                cshader.SetBuffer(kindex, "perBone", perBone);
            }

            {
                vOutData = new VertexOut[vtxCountOut];
                vOut = new ComputeBuffer(vOutData.Length, Marshal.SizeOf<VertexOut>());
                cshader.SetBuffer(kindex, "vOut", vOut);
            }

        }

        public void WriteToCShader()
        {
            for (int i = 0; i < Wbone.Length; i++)
            {
                float4x4 _W = Wbone[i];
                float4x4 _W_IT = math.transpose(math.inverse(_W));
                perBoneData[i].W = (Matrix4x4)_W;
                perBoneData[i].W_IT = (Matrix4x4)_W_IT;
            }

            {
                var na = perBone.BeginWrite<PerBone>(0, perBone.count);
                for (int i = 0; i < perBone.count; i++)
                {
                    na[i] = perBoneData[i];
                }
                perBone.EndWrite<PerBone>(perBone.count);
            }
        }

        public void ExecuteCShaderVertex(ScriptableRenderContext context)
        {
            CommandBuffer cmd = CommandBufferPool.Get();
            //cmd.SetExecutionFlags(CommandBufferExecutionFlags.AsyncCompute);

            cmd.SetComputeBufferParam(cshader, kindex, "vIn", vIn);
            cmd.SetComputeBufferParam(cshader, kindex, "vOut", vOut);
            cmd.SetComputeBufferParam(cshader, kindex, "perBone", perBone);
            cmd.SetComputeVectorParam(cshader, "countInfo", countInfo);

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

        public void ReleaseCShader()
        {
            if (vIn != null) vIn.Release();
            if (perBone != null) perBone.Release();
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

    struct VertexStatic
    {
        public float2 uv;
        public float4 color;
    }
}
