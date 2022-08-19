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


public class TorusManager : MonoBehaviour
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
        RenderTorus(context, cam, perCam);
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

    int unitCount;
    int[] unitCounts;
    Transform[] unitTrs;

    public float[] _offset;
    public float3[] offsetVec;
    
    public float[] _tScale;
    public float3[] tScales;

    public float3 tSca;

    public int dpTrWCount;


    int[] selectData;

    void InitData()
    {
        unitCount = GameManager.unitCount;
        unitCounts = GameManager.unitCounts;
        unitTrs = GameManager.unitTrs;
        
        offsetVec = new float3[unitCount];
        tScales = new float3[unitCount];
        
        int start = 0;
        for (int i = 0; i < unitCounts.Length; i++)
        {            
            for (int j = 0; j < unitCounts[i]; j++)
            {
                offsetVec[start + j] = new float3(0.0f, _offset[i], 0.0f);
                tScales[start + j] = new float3(1.0f, 1.0f, 1.0f) * _tScale[i];
            }
            start += unitCounts[i];
        }

        dpTrWCount = (unitCount % 64 == 0) ? (unitCount / 64) : (unitCount / 64 + 1);

        selectData = SelectManager.selectData;
    }

    int ki_trW;
    int ki_Vertex;
    
    void InitCompute()
    {
        ki_trW = cshader.FindKernel("CS_trW");
        ki_Vertex = cshader.FindKernel("CS_Vertex");
    }

    Mesh torusMesh;
    Material mte;
    MaterialPropertyBlock mpb;
    int pass;
    GraphicsBuffer idxBuffer;

    
    public int vtxInCount;
    public int vtxOutCount;

    public float4 tInfo = new float4(1.0f, 2.0f, 24.0f, 24.0f);

    void InitRendering()
    {       
        torusMesh = RenderUtil.CreateTorusMesh(tInfo.x, tInfo.y, (int)tInfo.z, (int)tInfo.w);
        mte = new Material(gshader);
        mpb = new MaterialPropertyBlock();

        vtxInCount = torusMesh.vertexCount;
        vtxOutCount = unitCount * vtxInCount;

        idxBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Index, (int)torusMesh.GetIndexCount(0), sizeof(int));
        idxBuffer.SetData(torusMesh.GetIndices(0));

        pass = mte.FindPass("Torus");
    }

    ComputeBuffer trW_In_Buffer;
    ComputeBuffer trW_Out_Buffer;
    ComputeBuffer vertex_In_Buffer;
    ComputeBuffer vertex_Out_Buffer;

    public float4x4[]   trW_In;
    public float4x4[]   trW_Out;
    public Vertex[]     vertex_In;
    public Vertex[]     vertex_Out;

    ComputeBuffer isSelect_Buffer;

    void InitResource()
    {
        {
            trW_In_Buffer = new ComputeBuffer(unitCount, Marshal.SizeOf<float4x4>(), ComputeBufferType.Structured, ComputeBufferMode.SubUpdates);
            trW_Out_Buffer = new ComputeBuffer(unitCount, Marshal.SizeOf<float4x4>());
            vertex_In_Buffer = new ComputeBuffer(vtxInCount, Marshal.SizeOf<Vertex>(), ComputeBufferType.Structured, ComputeBufferMode.Immutable);
            vertex_Out_Buffer = new ComputeBuffer(vtxOutCount, Marshal.SizeOf<Vertex>());
        }

        {
            trW_In = new float4x4[unitCount];
            trW_Out = new float4x4[unitCount];
            vertex_In = new Vertex[vtxInCount];
            vertex_Out = new Vertex[vtxOutCount];
        }

        {
            cshader.SetBuffer(ki_trW, "trW_In_Buffer", trW_In_Buffer);
            cshader.SetBuffer(ki_trW, "trW_Out_Buffer", trW_Out_Buffer);

            cshader.SetBuffer(ki_Vertex, "trW_Out_Buffer", trW_Out_Buffer);
            cshader.SetBuffer(ki_Vertex, "vertex_In_Buffer", vertex_In_Buffer);
            cshader.SetBuffer(ki_Vertex, "vertex_Out_Buffer", vertex_Out_Buffer);
        }

        {
            {
                var na = trW_In_Buffer.BeginWrite<float4x4>(0, unitCount);
                for (int i = 0; i < unitCount; i++)
                {
                    float4x4 mat = float4x4.zero;
                    //mat.c2 = new float4(tSca, 0.0f);
                    mat.c2 = new float4(tScales[i], 0.0f);
                    mat.c3 = new float4(offsetVec[i], 0.0f);
                    trW_In[i] = mat;

                    na[i] = mat;
                }
                trW_In_Buffer.EndWrite<float4x4>(unitCount);
            }

            {
                List<Vector3> pos = new List<Vector3>();
                List<Vector3> nom = new List<Vector3>();

                torusMesh.GetVertices(pos);
                torusMesh.GetNormals(nom);

                for(int i = 0; i < vtxInCount; i++)
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
        {
            isSelect_Buffer = new ComputeBuffer(unitCount, sizeof(int), ComputeBufferType.Structured, ComputeBufferMode.SubUpdates);
        }

        {
            mpb.SetBuffer("vertex_Out_Buffer", vertex_Out_Buffer);
            mpb.SetInt("dvCount", vtxInCount);

            mpb.SetBuffer("isSelect_Buffer", isSelect_Buffer);
        }
        
    }

   

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void WriteToResource(ScriptableRenderContext context, Camera cam)
    {
        {
            var na = trW_In_Buffer.BeginWrite<float4x4>(0, unitCount);
            for (int i = 0; i < unitCount; i++)
            {
                float4x4 mat = trW_In[i];
                Transform tr = unitTrs[i];
                mat.c0.xyz = tr.position;
                mat.c1 = ((quaternion)(tr.rotation)).value;

                na[i] = mat;
            }
            trW_In_Buffer.EndWrite<float4x4>(unitCount);
        }

        {
            var na = isSelect_Buffer.BeginWrite<int>(0, unitCount);
            for(int i = 0; i < unitCount; i++)
            {
                na[i] = selectData[i];
            }
            isSelect_Buffer.EndWrite<int>(unitCount);
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
                    for(int i = 0; i < unitCount; i++)
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
                    for(int i = 0; i < vtxOutCount; i++)
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
            var na = trW_In_Buffer.BeginWrite<float4x4>(0, unitCount);
            for (int i = 0; i < unitCount; i++)
            {
                float4x4 mat = trW_In[i];
                Transform tr = unitTrs[i];
                mat.c0.xyz = tr.position;
                mat.c1 = ((quaternion)(tr.rotation)).value;

                na[i] = mat;
            }
            trW_In_Buffer.EndWrite<float4x4>(unitCount);
        }

        {
            var na = isSelect_Buffer.BeginWrite<int>(0, unitCount);
            for (int i = 0; i < unitCount; i++)
            {
                na[i] = selectData[i];
            }
            isSelect_Buffer.EndWrite<int>(unitCount);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void DispatchCompute(ScriptableRenderContext context, Camera[] cams)
    {
        CommandBuffer cmd = CommandBufferPool.Get();

        cmd.DispatchCompute(cshader, ki_trW, dpTrWCount, 1, 1);
        cmd.DispatchCompute(cshader, ki_Vertex, unitCount, 1, 1);

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
    void RenderTorus(ScriptableRenderContext context, Camera cam, RenderGOM.PerCamera perCam)
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
        ReleaseCBuffer(trW_In_Buffer    );        
        ReleaseCBuffer(trW_Out_Buffer   );
        ReleaseCBuffer(vertex_In_Buffer );
        ReleaseCBuffer(vertex_Out_Buffer);

        ReleaseCBuffer(isSelect_Buffer);
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
