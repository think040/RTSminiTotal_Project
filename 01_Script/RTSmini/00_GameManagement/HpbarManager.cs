using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

using Unity.Mathematics;
using Unity.Burst;
using Unity.Collections;

using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class HpbarManager : MonoBehaviour
{
    void Compute(ScriptableRenderContext context, Camera cam)
    {
        WriteToResource(context, cam);
        DispatchCompute(context, cam);
        ReadFromResource(context, cam);
    }

    void Compute(ScriptableRenderContext context, Camera[] cams)
    {
        WriteToResource(context, cams);
        DispatchCompute(context, cams);
        ReadFromResource(context, cams);
    }


    void Render(ScriptableRenderContext context, Camera cam, RenderGOM.PerCamera perCam)
    {
        RenderHpbar(context, cam, perCam);
    }

    public ComputeShader cshader;
    public Shader gshader;

    public CSMAction csmAction;

    public void Init()
    {
        InitData();
        InitCompute();
        InitRendering();
        InitResource();
        
        //RenderGOM.BeginCameraRender += Compute;
        RenderGOM.BeginFrameRender += Compute;
        RenderGOM.OnRenderCamAlpha += Render;
        
        SceneManager.sceneUnloaded += SceneLeft;
    }

    public void Begin()
    {

    }

    public void SetCullData(RenderTexture pvf_tex)
    {
        int count = GameManager.unitCount;
        if (count > 0)
        {            
            {                
                mpb.SetTexture("cullResult_pvf_Texture", pvf_tex);                
            }          
        }
    }

    void SceneLeft(Scene scene)
    {
        OnDestroy();
    }


    void OnDestroy()
    {
        //RenderGOM.BeginCameraRender -= Compute;
        RenderGOM.BeginFrameRender -= Compute;
        RenderGOM.OnRenderCamAlpha -= Render;
        
        SceneManager.sceneUnloaded -= SceneLeft;
        
        ReleaseResource();
    }

    void Start()
    {

    }

    void Update()
    {

    }

    public float[] _maxHp;
    public float[] _hitHp;
    public float[] _healHp;

    public float[] _hp;

    int unitCount;
    int[] unitCounts;
    Transform[] unitTrs;
    UnitActor[] unitActors;

    public float[] _offset;
    public float3[] offsetVec;

    public float3[] _tScale;
    public float3[] tScales;

    
    public static float[] maxHp
    {
        get; set;
    }
    public float[] maxHps;
    
      
    public static float[] hp
    {
        get; private set;
    }

    public static float[] hitHp
    {
        get; set;
    }

    public static float[] healHp
    {
        get; set;
    }


    public int dpTrWCount;    

    Transform mainCamTr;


    //static bool useEditor
    //{ get; set; } = false;

    static HpbarManager()
    {
        //useEditor = GameManager.useEditor;

        //if(!useEditor)
        {
            //int count = 4;
            //maxHp =  new float[count];
            //hitHp =  new float[count];
            //healHp = new float[count];
            //
            //maxHp[0] = 150.0f;
            //maxHp[1] = 200.0f;
            //maxHp[2] = 200.0f;
            //maxHp[3] = 250f;
            //
            //hitHp[0] = 10.0f;
            //hitHp[1] = 10.0f;
            //hitHp[2] = 5.0f;
            //hitHp[3] = 5.0f;
            //
            //healHp[0] = 4.0f;                        
            //healHp[1] = 4.0f;            
            //healHp[2] = 1.0f;                       
            //healHp[3] = 1.0f;

            maxHp = GameManager.maxHp;
            hitHp = GameManager.hitHp;
            healHp = GameManager.healHp;
        }
    }


    void InitData()
    {
        unitCount = GameManager.unitCount;
        unitCounts = GameManager.unitCounts;
        unitTrs = GameManager.unitTrs;
        unitActors = GameManager.unitActors;

        offsetVec = new float3[unitCount];
        tScales = new float3[unitCount];
        maxHps = new float[unitCount];
        hp = new float[unitCount];
        _hp = hp;


        //useEditor = GameManager.useEditor;
        if(GameManager.useEditor)
        {
            maxHp = _maxHp;
            hitHp = _hitHp;
            healHp = _healHp;
        }        
       
        
        int start = 0;
        for (int i = 0; i < unitCounts.Length; i++)
        {
            for (int j = 0; j < unitCounts[i]; j++)
            {
                int idx = start + j;
                offsetVec[idx] = new float3(0.0f, _offset[i], 0.0f);
                tScales[idx] = _tScale[i];
                //maxHps[idx] = _maxHp[i];
                maxHps[idx] = maxHp[i];
                hp[idx] = maxHps[idx];
            }
            start += unitCounts[i];
        }

        dpTrWCount = (unitCount % 64 == 0) ? (unitCount / 64) : (unitCount / 64 + 1);

        mainCamTr = Camera.main.transform;       
    }

    int ki_trW;
    int ki_Vertex;

    void InitCompute()
    {
        ki_trW = cshader.FindKernel("CS_trW");
        ki_Vertex = cshader.FindKernel("CS_Vertex");
    }

    public Mesh cubeMesh;
    Material mte;
    MaterialPropertyBlock mpb;
    int pass;
    GraphicsBuffer idxBuffer;


    public int vtxInCount;
    public int vtxOutCount;
    
    void InitRendering()
    {       
        mte = new Material(gshader);
        mpb = new MaterialPropertyBlock();

        vtxInCount = cubeMesh.vertexCount;
        vtxOutCount = unitCount * vtxInCount;

        idxBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Index, (int)cubeMesh.GetIndexCount(0), sizeof(int));
        idxBuffer.SetData(cubeMesh.GetIndices(0));

        pass = mte.FindPass("Hpbar");
    }

    ComputeBuffer trW_In_Buffer;
    ComputeBuffer trW_Const_Buffer;
    ComputeBuffer trW_Out_Buffer;
    ComputeBuffer vertex_In_Buffer;
    ComputeBuffer vertex_Out_Buffer;

    public float4x4[] trW_In;
    public float4x4[] trW_Out;
    public float4x4[] trW_Const;
    public Vertex[] vertex_In;
    public Vertex[] vertex_Out;

    ComputeBuffer   player_Num_Buffer;
    ComputeBuffer   pColor_Buffer;

    ComputeBuffer refHp_Buffer;

    void InitResource()
    {
        {
            trW_In_Buffer = new ComputeBuffer(unitCount, Marshal.SizeOf<float4x4>(), ComputeBufferType.Structured, ComputeBufferMode.SubUpdates);
            trW_Out_Buffer = new ComputeBuffer(unitCount, Marshal.SizeOf<float4x4>());
            trW_Const_Buffer = new ComputeBuffer(unitCount, Marshal.SizeOf<float4x4>(), ComputeBufferType.Structured, ComputeBufferMode.Immutable);
            vertex_In_Buffer = new ComputeBuffer(vtxInCount, Marshal.SizeOf<Vertex>(), ComputeBufferType.Structured, ComputeBufferMode.Immutable);
            vertex_Out_Buffer = new ComputeBuffer(vtxOutCount, Marshal.SizeOf<Vertex>());
        }

        {
            trW_In = new float4x4[unitCount];
            trW_Out = new float4x4[unitCount];
            trW_Const = new float4x4[unitCount];
            vertex_In = new Vertex[vtxInCount];
            vertex_Out = new Vertex[vtxOutCount];
        }

        {
            cshader.SetBuffer(ki_trW, "trW_In_Buffer", trW_In_Buffer);
            cshader.SetBuffer(ki_trW, "trW_Const_Buffer", trW_Const_Buffer);
            cshader.SetBuffer(ki_trW, "trW_Out_Buffer", trW_Out_Buffer);

            cshader.SetBuffer(ki_Vertex, "trW_Out_Buffer", trW_Out_Buffer);
            cshader.SetBuffer(ki_Vertex, "vertex_In_Buffer", vertex_In_Buffer);
            cshader.SetBuffer(ki_Vertex, "vertex_Out_Buffer", vertex_Out_Buffer);
        }

        {            
            {
                for (int i = 0; i < unitCount; i++)
                {
                    float4x4 mat = float4x4.zero;
                    mat.c0 = new float4(tScales[i], 0.0f);
                    mat.c1 = new float4(offsetVec[i], 0.0f);
                    mat.c2 = new float4(maxHps[i], 0.0f, 0.0f, 0.0f);
                    trW_Const[i] = mat;                    
                }

                trW_Const_Buffer.SetData(trW_Const);
            }

            {
                List<Vector3> pos = new List<Vector3>();
                List<Vector3> nom = new List<Vector3>();

                cubeMesh.GetVertices(pos);
                cubeMesh.GetNormals(nom);

                for (int i = 0; i < vtxInCount; i++)
                {
                    Vertex vtx;
                    vtx.pos = pos[i];
                    vtx.nom = nom[i];
                    vertex_In[i] = vtx;
                }

                vertex_In_Buffer.SetData(vertex_In);
            }
        }


        //
        int pCount = GameManager.playerColor.Length;
        {
            player_Num_Buffer = new ComputeBuffer(unitCount, sizeof(int), ComputeBufferType.Structured, ComputeBufferMode.Immutable);
            pColor_Buffer = new ComputeBuffer(pCount, Marshal.SizeOf<Color>(), ComputeBufferType.Structured, ComputeBufferMode.Immutable); ;
        }

        {
            {
                int[] pNum = new int[unitCount];
                for (int i = 0; i < unitCount; i++)
                {
                    pNum[i] = unitActors[i].pNum;
                }

                player_Num_Buffer.SetData(pNum);
            }

            {
                Color[] colors = new Color[pCount];
                for(int i = 0; i < pCount; i++)
                {
                    colors[i] = GameManager.playerColor[i];
                }

                pColor_Buffer.SetData(colors);
            }
            
        }

        {
            mpb.SetBuffer("vertex_Out_Buffer", vertex_Out_Buffer);
            mpb.SetInt("dvCount", vtxInCount);
            mpb.SetInt("cullOffset", 0);

            mpb.SetBuffer("player_Num_Buffer", player_Num_Buffer);
            mpb.SetBuffer("pColor_Buffer", pColor_Buffer);
        }

        //
        {
            refHp_Buffer = new ComputeBuffer(unitCount, sizeof(float), ComputeBufferType.Structured, ComputeBufferMode.SubUpdates);
        }

    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void WriteToResource(ScriptableRenderContext context, Camera cam)
    {                
        {
            float3x3 R = new float3x3(mainCamTr.rotation);
            R.c0 *= -1.0f;
            R.c2 *= -1.0f;

            var na = trW_In_Buffer.BeginWrite<float4x4>(0, unitCount);
            for (int i = 0; i < unitCount; i++)
            {
                float4x4 mat = float4x4.zero;
                Transform tr = unitTrs[i];
                mat.c0 = new float4(R.c0, 0.0f);
                mat.c1 = new float4(R.c1, 0.0f);
                mat.c2 = new float4(R.c2, 0.0f);
                mat.c3 = new float4(tr.position, hp[i]);

                na[i] = mat;
            }
            trW_In_Buffer.EndWrite<float4x4>(unitCount);
        }

        {
            var na = refHp_Buffer.BeginWrite<float>(0, unitCount);
            for (int i = 0; i < unitCount; i++)
            {
                na[i] = hp[i] / maxHps[i];
            }
            refHp_Buffer.EndWrite<float>(unitCount);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void DispatchCompute(ScriptableRenderContext context, Camera cam)
    {
        CommandBuffer cmd = CommandBufferPool.Get();

        cmd.DispatchCompute(cshader, ki_trW, dpTrWCount, 1, 1);
        cmd.DispatchCompute(cshader, ki_Vertex, unitCount, 1, 1);

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void ReadFromResource(ScriptableRenderContext context, Camera cam)
    {
        CommandBuffer cmd = CommandBufferPool.Get();

        {
            cmd.RequestAsyncReadback(trW_Out_Buffer,
                (read) =>
                {
                    var na = read.GetData<float4x4>();
                    for (int i = 0; i < unitCount; i++)
                    {
                        trW_Out[i] = na[i];
                    }
                });
        }

        {
            cmd.RequestAsyncReadback(vertex_Out_Buffer,
                (read) =>
                {
                    var na = read.GetData<Vertex>();
                    for (int i = 0; i < vtxOutCount; i++)
                    {
                        vertex_Out[i] = na[i];
                    }
                });
        }

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void WriteToResource(ScriptableRenderContext context, Camera[] cams)
    {        
        {
            float3x3 R = new float3x3(mainCamTr.rotation);
            R.c0 *= -1.0f;
            R.c2 *= -1.0f;

            var na = trW_In_Buffer.BeginWrite<float4x4>(0, unitCount);
            for (int i = 0; i < unitCount; i++)
            {
                float4x4 mat = float4x4.zero;
                Transform tr = unitTrs[i];
                mat.c0 = new float4(R.c0, 0.0f);
                mat.c1 = new float4(R.c1, 0.0f);
                mat.c2 = new float4(R.c2, 0.0f);
                mat.c3 = new float4(tr.position, hp[i]);

                na[i] = mat;
            }
            trW_In_Buffer.EndWrite<float4x4>(unitCount);
        }

        {
            var na = refHp_Buffer.BeginWrite<float>(0, unitCount);
            for(int i = 0; i < unitCount; i++)
            {
                na[i] = hp[i] / maxHps[i];
            }
            refHp_Buffer.EndWrite<float>(unitCount);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void DispatchCompute(ScriptableRenderContext context, Camera[] cams)
    {
        CommandBuffer cmd = CommandBufferPool.Get();

        cmd.DispatchCompute(cshader, ki_trW, dpTrWCount, 1, 1);
        cmd.DispatchCompute(cshader, ki_Vertex, unitCount, 1, 1);

        cmd.SetGlobalBuffer("refHP_Buffer", refHp_Buffer);

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void ReadFromResource(ScriptableRenderContext context, Camera[] cams)
    {
        CommandBuffer cmd = CommandBufferPool.Get();

        {
            cmd.RequestAsyncReadback(trW_Out_Buffer,
                (read) =>
                {
                    var na = read.GetData<float4x4>();
                    for (int i = 0; i < unitCount; i++)
                    {
                        trW_Out[i] = na[i];
                    }
                });
        }

        {
            cmd.RequestAsyncReadback(vertex_Out_Buffer,
                (read) =>
                {
                    var na = read.GetData<Vertex>();
                    for (int i = 0; i < vtxOutCount; i++)
                    {
                        vertex_Out[i] = na[i];
                    }
                });
        }

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void RenderHpbar(ScriptableRenderContext context, Camera cam, RenderGOM.PerCamera perCam)
    {
        {
            mpb.SetMatrix("CV", perCam.CV);
            mpb.SetVector("dirW_light", csmAction.dirW);
        }

        {
            CommandBuffer cmd = CommandBufferPool.Get();

            {
                cmd.DrawProcedural(idxBuffer, Matrix4x4.identity, mte, pass, MeshTopology.Triangles, idxBuffer.count, unitCount, mpb);
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }


    void ReleaseResource()
    {
        ReleaseCBuffer(trW_In_Buffer);
        ReleaseCBuffer(trW_Out_Buffer);
        ReleaseCBuffer(trW_Const_Buffer);
        ReleaseCBuffer(vertex_In_Buffer);
        ReleaseCBuffer(vertex_Out_Buffer);

        ReleaseCBuffer(player_Num_Buffer);
        ReleaseCBuffer(pColor_Buffer);

        ReleaseCBuffer(refHp_Buffer);

        ReleaseGBuffer(idxBuffer);
    }

    void ReleaseCBuffer(ComputeBuffer cBuffer)
    {
        if (cBuffer != null) cBuffer.Release();
    }

    void ReleaseGBuffer(GraphicsBuffer gBuffer)
    {
        if (gBuffer != null) gBuffer.Release();
    }

    [System.Serializable]
    public struct Vertex
    {
        public float3 pos;
        public float3 nom;
    };


}
