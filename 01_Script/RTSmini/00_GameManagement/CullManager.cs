using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Unity.Mathematics;
using Unity.Collections;
using Unity.Burst;

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.Jobs;

public class CullManager : MonoBehaviour
{
    public ComputeShader cshader;
    public Shader gshader;

    int ki_pvf;
    int ki_ovf;

    int ki_pvf_vertex;
    int ki_ovf_vertex;
    int ki_sphere_vertex;
    
    int ki_pvf_cull_sphere;
    int ki_ovf_cull_sphere;

    int ki_sphere_center;

    int pass;
    int pass_pvf;
    int pass_ovf;
    int pass_sphere;

    public int debugCullMode = 0;
    public bool bDrawPvf = true;
    public bool bDrawOvf = true;
    public bool bDrawSp = true;

    public void Init()
    {
        {
            ki_pvf             = cshader.FindKernel("CS_PVF");
            ki_ovf             = cshader.FindKernel("CS_OVF");

            ki_pvf_vertex      = cshader.FindKernel("CS_PVF_Vertex");
            ki_ovf_vertex      = cshader.FindKernel("CS_OVF_Vertex");
            ki_sphere_vertex   = cshader.FindKernel("CS_Sphere_Vertex");
            
            ki_pvf_cull_sphere = cshader.FindKernel("CS_PVF_Cull_Sphere");
            ki_ovf_cull_sphere = cshader.FindKernel("CS_OVF_Cull_Sphere");

            ki_sphere_center = cshader.FindKernel("CS_Sphere_Center");
        }


        {

#if UNITY_EDITOR
            bCullDebug = true;
#else
            bCullDebug = false;
#endif
        }

        InitData();
        InitResource();
        InitDebugRender();

        RenderGOM.Cull += Compute;
        //RenderGOM.CullDebug += ComputeDebug;
        SceneManager.sceneUnloaded += OnSceneLeft;
    }

    UnitManager[] unitMans;
    ArrowManager arrowMans;
    TorusManager torusMan;
    HpbarManager hpbarMan;

    public float[] spRadius;
    public static int[] spCounts;
    public static int spCount = 0;
    public static int[] cullOffsets;
    public int[] _cullOffsets;
    const int maxSpCount = 768; // 256 + 512 = 768       
    public int dpSpCount;

    public int spVtxInCount;
    public int vfVtxCount;

    public int spVtxOutCount;
    public int pvfVtxOutCount;
    public int ovfVtxOutCount;

    public Transform[] spTrs;

    int pvfCount = 1;
    int planePvfCount;
    int groupPvfCount;
    int totalPvfCount;

    int ovfCount;
    int planeOvfCount;
    int groupOvfCount;
    int totalOvfCount;

    public CSMAction csmAction;   
    public float4[] csmPos;
    public quaternion[] csmRot;
    public float4[] csmfi;

    public static bool bCull = true;
    public bool bCullDebug = true;

    public float3[] _spOffset;
    NativeArray<float3> spOffset;
    NativeArray<float3> spCenter;
    TransformAccessArray spTraa;
    SphereAction spAction;


    NativeArray<float4x4> spTrData;
    SphereTransform spTrans;
    int dpSpTrCount;


    void InitData()
    {
        unitMans = GameManager.unitMans;
        arrowMans = GameManager.arrowMans;
        torusMan = GameManager.torusMan;
        hpbarMan = GameManager.hpbarMan;

        {
            sMesh = RenderUtil.CreateSphereMesh(1.0f, 12, 24);
        }

        spCount = 0;
        {
            spCounts = new int[unitMans.Length + 1];
            cullOffsets = new int[unitMans.Length + 1];
            _cullOffsets = new int[unitMans.Length + 1];

            for (int i = 0; i < unitMans.Length; i++)
            {
                _cullOffsets[i] = spCount;
                cullOffsets[i] = spCount;
                
                spCounts[i] = unitMans[i].count;
                spCount += spCounts[i];

                //unitMans[i].SetCullData(i);
            }

            {
                int i = unitMans.Length;

                _cullOffsets[i] = spCount;
                cullOffsets[i] = spCount;
                
                spCounts[i] = ArrowManager.cCount;
                spCount += ArrowManager.cCount;

                //arrowMans.SetCullData(i);
            }

            bCull = true;
            if (spCount == 0) bCull = false;

            dpSpCount = (spCount % 8 == 0) ? (spCount / 8) : (spCount / 8 + 1);
        }

        {
            spTrs = new Transform[spCount];
            _sphere = new float4[spCount];
            spOffset = new NativeArray<float3>(spCount, Allocator.Persistent);
            spCenter = new NativeArray<float3>(spCount, Allocator.Persistent);

            int start = 0;
            int i = 0;
            for (i = 0; i < unitMans.Length; i++)
            {
                for (int j = 0; j < unitMans[i].count; j++)
                {
                    spTrs[start + j] = unitMans[i].trs[j];                    
                    _sphere[start + j] = new float4(0.0f, 0.0f, 0.0f, spRadius[i]);
                    spOffset[start + j] = _spOffset[i];
                }
                start += unitMans[i].count;
            }

            {                
                for (int j = 0; j < ArrowManager.cCount; j++)
                {
                    spTrs[start + j] = arrowMans.arrow[j].transform;
                    _sphere[start + j] = new float4(0.0f, 0.0f, 0.0f, spRadius[i]);
                    spOffset[start + j] = _spOffset[i];
                }
            }            
        }

        {
            csmPos = csmAction.pos;
            csmRot = csmAction.rot;
            csmfi = csmAction.fi;
        }

        {
            pvfCount = 1;
            ovfCount = csmAction.csmNum;           
        }

        {
            groupPvfCount = 1;
            groupOvfCount = 1;

            totalPvfCount = groupPvfCount * pvfCount;
            totalOvfCount = groupOvfCount * ovfCount;

            planePvfCount = totalPvfCount * 6;
            planeOvfCount = totalOvfCount * 6;
        }

        {
            spVtxInCount = sMesh.vertexCount;
            vfVtxCount = cMesh.vertexCount;

            spVtxOutCount = sMesh.vertexCount * spCount;
            pvfVtxOutCount = totalPvfCount * vfVtxCount;
            ovfVtxOutCount = totalOvfCount * vfVtxCount;
        }

        {            
            spTraa = new TransformAccessArray(spTrs);

            spAction = new SphereAction();

            spAction.spOffset = spOffset;
            spAction.spCenter = spCenter;            
        }

        {
            spTrData = new NativeArray<float4x4>(spCount, Allocator.Persistent);

            spTrans = new SphereTransform();
            spTrans.spTr = spTrData;

            dpSpTrCount = (spCount % 64 == 0) ? (spCount / 64) : (spCount / 64 + 1);
        }
    }

    ComputeBuffer info_pvf_Buffer;
    ComputeBuffer info_ovf_Buffer;    
    ComputeBuffer plane_pvf_Buffer;
    ComputeBuffer plane_ovf_Buffer;

    ComputeBuffer vf_vertex_Buffer;    
    ComputeBuffer pvf_vertex_Buffer;
    ComputeBuffer ovf_vertex_Buffer;
    
    ComputeBuffer sphere_Buffer;
    ComputeBuffer sphere_vertex_In_Buffer;    
    ComputeBuffer sphere_vertex_Out_Buffer;

    RenderTexture cullResult_pvf_Texture;
    RenderTexture cullResult_ovf_Texture;

    ComputeBuffer sphere_In_Buffer;
    ComputeBuffer sphere_trM_Buffer;
    ComputeBuffer sphere_Out_Buffer;


    public Info_VF[] _info_pvf;
    public Info_VF[] _info_ovf;
    public float4[] _plane_pvf;
    public float4[] _plane_ovf;

    public Vertex[] _vf_vertex;
    public Vertex[] _pvf_vertex;
    public Vertex[] _ovf_vertex;

    public float4[] _sphere;
    public Vertex[] _sphere_vertex_In;
    public Vertex[] _sphere_vertex_Out;

    public float[] _cullResult_pvf;
    public float[] _cullResult_ovf;

    public float4[] _sphere_out;
    

    void InitResource()
    {        
        {
            info_pvf_Buffer = new ComputeBuffer(totalPvfCount, Marshal.SizeOf<Info_VF>(), ComputeBufferType.Structured, ComputeBufferMode.SubUpdates);
            info_ovf_Buffer = new ComputeBuffer(totalOvfCount, Marshal.SizeOf<Info_VF>(), ComputeBufferType.Structured, ComputeBufferMode.SubUpdates);            
            plane_pvf_Buffer = new ComputeBuffer(planePvfCount, Marshal.SizeOf<float4>());
            plane_ovf_Buffer = new ComputeBuffer(planeOvfCount, Marshal.SizeOf<float4>());

            vf_vertex_Buffer = new ComputeBuffer(vfVtxCount, Marshal.SizeOf<Vertex>(), ComputeBufferType.Structured, ComputeBufferMode.Immutable);
            pvf_vertex_Buffer = new ComputeBuffer(pvfVtxOutCount, Marshal.SizeOf<Vertex>());
            ovf_vertex_Buffer = new ComputeBuffer(ovfVtxOutCount, Marshal.SizeOf<Vertex>());
            
            sphere_Buffer = new ComputeBuffer(spCount, Marshal.SizeOf<float4>(), ComputeBufferType.Structured, ComputeBufferMode.SubUpdates);
            sphere_vertex_In_Buffer = new ComputeBuffer(spVtxInCount,  Marshal.SizeOf<Vertex>(), ComputeBufferType.Structured, ComputeBufferMode.Immutable);
            sphere_vertex_Out_Buffer = new ComputeBuffer(spVtxOutCount, Marshal.SizeOf<Vertex>());

            sphere_In_Buffer  = new ComputeBuffer(spCount, Marshal.SizeOf<float4>(), ComputeBufferType.Structured, ComputeBufferMode.SubUpdates);
            sphere_trM_Buffer = new ComputeBuffer(spCount, Marshal.SizeOf<float4x4>(), ComputeBufferType.Structured, ComputeBufferMode.SubUpdates);
            sphere_Out_Buffer = new ComputeBuffer(spCount, Marshal.SizeOf<float4>());
        }

        {
            RenderTextureDescriptor rtd = new RenderTextureDescriptor();
            {
                rtd.msaaSamples = 1;
                rtd.depthBufferBits = 0;
                rtd.enableRandomWrite = true;

                rtd.colorFormat = RenderTextureFormat.RFloat;
                rtd.dimension = TextureDimension.Tex3D;
                rtd.width = spCount;                
            }

            {
                rtd.height = pvfCount;
                rtd.volumeDepth = groupPvfCount;
                cullResult_pvf_Texture = new RenderTexture(rtd);
            }

            {
                rtd.height = ovfCount;
                rtd.volumeDepth = groupOvfCount;
                cullResult_ovf_Texture = new RenderTexture(rtd);
            }                        
        }

        {
            cshader.SetInt("spCount", spCount);

            cshader.SetBuffer(ki_pvf, "info_pvf_Buffer", info_pvf_Buffer);
            cshader.SetBuffer(ki_pvf, "plane_pvf_Buffer", plane_pvf_Buffer);
            cshader.SetBuffer(ki_ovf, "info_ovf_Buffer", info_ovf_Buffer);
            cshader.SetBuffer(ki_ovf, "plane_ovf_Buffer", plane_ovf_Buffer);

            cshader.SetBuffer(ki_pvf_vertex, "info_pvf_Buffer", info_pvf_Buffer);
            cshader.SetBuffer(ki_pvf_vertex, "vf_vertex_Buffer", vf_vertex_Buffer);
            cshader.SetBuffer(ki_pvf_vertex, "pvf_vertex_Buffer", pvf_vertex_Buffer);
            cshader.SetBuffer(ki_ovf_vertex, "info_ovf_Buffer", info_ovf_Buffer);
            cshader.SetBuffer(ki_ovf_vertex, "vf_vertex_Buffer", vf_vertex_Buffer);
            cshader.SetBuffer(ki_ovf_vertex, "ovf_vertex_Buffer", ovf_vertex_Buffer);
            
            cshader.SetBuffer(ki_sphere_vertex, "sphere_Buffer", sphere_Buffer);
            cshader.SetBuffer(ki_sphere_vertex, "sphere_Out_Buffer", sphere_Out_Buffer);
            cshader.SetBuffer(ki_sphere_vertex, "sphere_vertex_In_Buffer", sphere_vertex_In_Buffer);
            cshader.SetBuffer(ki_sphere_vertex, "sphere_vertex_Out_Buffer", sphere_vertex_Out_Buffer);

            cshader.SetBuffer(ki_pvf_cull_sphere, "sphere_Out_Buffer", sphere_Out_Buffer);            
            cshader.SetBuffer(ki_pvf_cull_sphere, "sphere_Buffer", sphere_Buffer);
            cshader.SetBuffer(ki_pvf_cull_sphere, "plane_pvf_Buffer", plane_pvf_Buffer);
            cshader.SetBuffer(ki_ovf_cull_sphere, "sphere_Out_Buffer", sphere_Out_Buffer);
            cshader.SetBuffer(ki_ovf_cull_sphere, "sphere_Buffer", sphere_Buffer);            
            cshader.SetBuffer(ki_ovf_cull_sphere, "plane_ovf_Buffer", plane_ovf_Buffer);

            cshader.SetTexture(ki_pvf_cull_sphere, "cullResult_pvf_Texture", cullResult_pvf_Texture);
            cshader.SetTexture(ki_ovf_cull_sphere, "cullResult_ovf_Texture", cullResult_ovf_Texture);

            cshader.SetBuffer(ki_sphere_center, "sphere_In_Buffer",  sphere_In_Buffer);
            cshader.SetBuffer(ki_sphere_center, "sphere_trM_Buffer", sphere_trM_Buffer);
            cshader.SetBuffer(ki_sphere_center, "sphere_Out_Buffer", sphere_Out_Buffer);
        }

        {
            _info_pvf = new Info_VF[totalPvfCount];
            _info_ovf = new Info_VF[totalOvfCount];
            _plane_pvf = new float4[planePvfCount];
            _plane_ovf = new float4[planeOvfCount];

            _vf_vertex = new Vertex[vfVtxCount];
            _pvf_vertex  = new Vertex[pvfVtxOutCount];
            _ovf_vertex  = new Vertex[ovfVtxOutCount];

            //_sphere = new float4[spCount];
            _sphere_vertex_In = new Vertex[spVtxInCount];
            _sphere_vertex_Out = new Vertex[spVtxOutCount];

            _cullResult_pvf = new float[spCount * totalPvfCount];
            _cullResult_ovf = new float[spCount * totalOvfCount];

            _sphere_out = new float4[spCount];
        }

        {
            List<Vector3> pos = new List<Vector3>();
            List<Vector3> nom = new List<Vector3>();

            cMesh.GetVertices(pos);
            cMesh.GetNormals(nom);

            for(int i = 0; i < vfVtxCount; i++)
            {
                Vertex vtx = new Vertex();
                vtx.position = pos[i];
                vtx.normal = nom[i];
                _vf_vertex[i] = vtx;
            }

            vf_vertex_Buffer.SetData(_vf_vertex);
        }

        {
            List<Vector3> pos = new List<Vector3>();
            List<Vector3> nom = new List<Vector3>();

            sMesh.GetVertices(pos);
            sMesh.GetNormals(nom);

            for (int i = 0; i < spVtxInCount; i++)
            {
                Vertex vtx = new Vertex();
                vtx.position = pos[i];
                vtx.normal = nom[i];
                _sphere_vertex_In[i] = vtx;
            }

           sphere_vertex_In_Buffer.SetData(_sphere_vertex_In);
        }
    }

    Material mte;
    MaterialPropertyBlock mpb;

    public Mesh cMesh;
    Mesh sMesh;

    GraphicsBuffer idxVf_Buffer;
    GraphicsBuffer idxSp_Buffer;

    

    void InitDebugRender()
    {
        mte = new Material(gshader);
        mpb = new MaterialPropertyBlock();

        pass = mte.FindPass("Cull");
        pass_pvf = mte.FindPass("Cull_Pvf");
        pass_ovf = mte.FindPass("Cull_Ovf");
        pass_sphere = mte.FindPass("Cull_Sphere");

        //Debug.Log("sMesh vertex count : " + sMesh.vertexCount.ToString());
        //Debug.Log("cMesh vertex count : " + cMesh.vertexCount.ToString());

        idxVf_Buffer = new GraphicsBuffer(GraphicsBuffer.Target.Index, (int)cMesh.GetIndexCount(0), sizeof(int));
        idxSp_Buffer = new GraphicsBuffer(GraphicsBuffer.Target.Index, (int)sMesh.GetIndexCount(0), sizeof(int));

        idxVf_Buffer.SetData(cMesh.GetIndices(0));
        idxSp_Buffer.SetData(sMesh.GetIndices(0));

        mpb.SetTexture("cullResult_pvf_Texture", cullResult_pvf_Texture);
        mpb.SetTexture("cullResult_ovf_Texture", cullResult_ovf_Texture);

        mpb.SetBuffer("sphere_vertex_Out_Buffer", sphere_vertex_Out_Buffer);
        mpb.SetBuffer("pvf_vertex_Buffer", pvf_vertex_Buffer);
        mpb.SetBuffer("ovf_vertex_Buffer", ovf_vertex_Buffer);

        mpb.SetInt("dvCount_sp", sMesh.vertexCount);
        mpb.SetInt("dvCount_vf", cMesh.vertexCount);

        RenderGOM.OnRenderCamDebug += RenderDebugRender;
    }

    void RenderDebugRender(ScriptableRenderContext context, Camera cam, RenderGOM.PerCamera perCam)
    {
        {
            mpb.SetInt("cullMode", debugCullMode);
            mpb.SetMatrix("CV", perCam.CV);
            mpb.SetVector("dirW_light", csmAction.dirW);                    
        }

        {
            CommandBuffer cmd = CommandBufferPool.Get();

            {                
                if (bDrawSp)
                {                                        
                    cmd.DrawProcedural(idxSp_Buffer, Matrix4x4.identity, mte, pass_sphere, MeshTopology.Triangles, idxSp_Buffer.count, spCount, mpb);
                }
            }

            {               
                if(bDrawPvf)
                {                                     
                    cmd.DrawProcedural(idxVf_Buffer, Matrix4x4.identity, mte, pass_pvf, MeshTopology.Triangles, idxVf_Buffer.count, pvfCount, mpb);
                }
                
                if(bDrawOvf)
                {                                   
                    cmd.DrawProcedural(idxVf_Buffer, Matrix4x4.identity, mte, pass_ovf, MeshTopology.Triangles, idxVf_Buffer.count, ovfCount, mpb);
                }               
            }
            
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
        
    }

    void ReleaseDebugResource()
    {
        ReleaseGBuffer(idxVf_Buffer);
        ReleaseGBuffer(idxSp_Buffer);
    }

    void OnSceneLeft(Scene scene)
    {
        OnDestroy();
    }

    void OnDestroy()
    {                        
        RenderGOM.Cull -= Compute;
        //RenderGOM.CullDebug -= ComputeDebug;
        RenderGOM.OnRenderCamDebug -= RenderDebugRender;
        SceneManager.sceneUnloaded -= OnSceneLeft;

        ReleaseResource();
        ReleaseDebugResource();
    }

    void ReleaseResource()
    {
        ReleaseCBuffer(info_pvf_Buffer);
        ReleaseCBuffer(info_ovf_Buffer);
        ReleaseCBuffer(plane_pvf_Buffer);
        ReleaseCBuffer(plane_ovf_Buffer);
        
        ReleaseCBuffer(vf_vertex_Buffer);
        ReleaseCBuffer(pvf_vertex_Buffer);
        ReleaseCBuffer(ovf_vertex_Buffer);
        
        ReleaseCBuffer(sphere_Buffer);
        ReleaseCBuffer(sphere_vertex_In_Buffer);
        ReleaseCBuffer(sphere_vertex_Out_Buffer);

        ReleaseCBuffer(sphere_In_Buffer );
        ReleaseCBuffer(sphere_trM_Buffer);
        ReleaseCBuffer(sphere_Out_Buffer);

        ReleaseRenTexture(cullResult_pvf_Texture);
        ReleaseRenTexture(cullResult_ovf_Texture);

        DisposeNa<float3>(spOffset);
        DisposeNa<float3>(spCenter);
        DisposeTraa(spTraa);

        DisposeNa<float4x4>(spTrData);
    }

    public void Begin()
    {
        {
            for (int i = 0; i < unitMans.Length; i++)
            {
                unitMans[i].SetCullData(i, cullResult_pvf_Texture, cullResult_ovf_Texture);
            }

            {
                int i = unitMans.Length;
                arrowMans.SetCullData(i, cullResult_pvf_Texture, cullResult_ovf_Texture);
            }

            {
                torusMan.SetCullData(cullResult_pvf_Texture);
                hpbarMan.SetCullData(cullResult_pvf_Texture);
            }
        }        
    }

    
    void Start()
    {
        
    }

   
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            debugCullMode++;
            if(debugCullMode > 5)
            {
                debugCullMode = 0;
            }

            //debugCullMode = (++debugCullMode) % 6;
        }

        if (Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            debugCullMode--;
            if (debugCullMode < 0)
            {
                debugCullMode = 5;
            }
        }
    }

    void Compute(ScriptableRenderContext context, Camera cam)
    {
        if(cam.cameraType != CameraType.SceneView)
        {
            WriteToResource(context, cam);
            DispatchCompute(context, cam);
            ReadFromResource(context, cam);
        }      
    }
   
    void WriteToResource(ScriptableRenderContext context, Camera cam)
    {
        {
            var na = info_pvf_Buffer.BeginWrite<Info_VF>(0, pvfCount);
            for(int i = 0; i < pvfCount; i++)
            {
                Info_VF ivf = new Info_VF();
                Transform camTr = cam.transform;

                ivf.fi = new float4(cam.fieldOfView, cam.aspect, cam.nearClipPlane, cam.farClipPlane);
                ivf.pos = camTr.position;
                ivf.rot = ((quaternion)(camTr.rotation)).value;

                na[i] = ivf;
            }
            info_pvf_Buffer.EndWrite<Info_VF>(pvfCount);
        }

        {
            var na = info_ovf_Buffer.BeginWrite<Info_VF>(0, ovfCount);
            for (int i = 0; i < ovfCount; i++)
            {
                Info_VF ivf = new Info_VF();

                ivf.fi = csmfi[i];
                ivf.pos = csmPos[i].xyz;
                ivf.rot = csmRot[i].value;

                na[i] = ivf;
            }
            info_ovf_Buffer.EndWrite<Info_VF>(ovfCount);
        }

        //{
        //    var na = sphere_Buffer.BeginWrite<float4>(0, spCount);          
        //    for(int i = 0; i < spCount; i++)
        //    {
        //        _sphere[i].xyz = spTrs[i].position;               
        //        na[i] = _sphere[i];
        //    }
        //    sphere_Buffer.EndWrite<float4>(spCount);
        //}

        //{
        //    spAction.Schedule<SphereAction>(spTraa).Complete();
        //
        //    var na = sphere_Buffer.BeginWrite<float4>(0, spCount);
        //    for (int i = 0; i < spCount; i++)
        //    {
        //        _sphere[i].xyz = spCenter[i];
        //        na[i] = _sphere[i];
        //    }
        //    sphere_Buffer.EndWrite<float4>(spCount);
        //}

        {
            spTrans.Schedule<SphereTransform>(spTraa).Complete();

            {
                var na = sphere_In_Buffer.BeginWrite<float4>(0, spCount);
                for (int i = 0; i < spCount; i++)
                {
                    _sphere[i].xyz = spOffset[i];
                    na[i] = _sphere[i];
                }
                sphere_In_Buffer.EndWrite<float4>(spCount);
            }

            {
                var na = sphere_trM_Buffer.BeginWrite<float4x4>(0, spCount);
                for (int i = 0; i < spCount; i++)
                {
                    na[i] = spTrData[i];
                }
                sphere_trM_Buffer.EndWrite<float4x4>(spCount);
            }                      
        }
    }
    
    void DispatchCompute(ScriptableRenderContext context, Camera cam)
    {
        CommandBuffer cmd = CommandBufferPool.Get();

        cmd.DispatchCompute(cshader, ki_pvf, totalPvfCount, 1, 1);
        cmd.DispatchCompute(cshader, ki_ovf, totalOvfCount, 1, 1);

        cmd.DispatchCompute(cshader, ki_sphere_center, dpSpTrCount, 1, 1);
        
        if(bCullDebug)
        {
            cmd.DispatchCompute(cshader, ki_pvf_vertex, totalPvfCount, 1, 1);
            cmd.DispatchCompute(cshader, ki_ovf_vertex, totalOvfCount, 1, 1);
            cmd.DispatchCompute(cshader, ki_sphere_vertex, spCount, 1, 1);
        }
               
        cmd.DispatchCompute(cshader, ki_pvf_cull_sphere, dpSpCount, pvfCount, 1);
        cmd.DispatchCompute(cshader, ki_ovf_cull_sphere, dpSpCount, ovfCount, 1);
       
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);     
    }
    
    void ReadFromResource(ScriptableRenderContext context, Camera cam)
    {
        if(bCullDebug)
        {
            CommandBuffer cmd = CommandBufferPool.Get();

            {
                cmd.RequestAsyncReadback(plane_pvf_Buffer,
                    (read) =>
                    {
                        var na = read.GetData<float4>();
                        for (int i = 0; i < planePvfCount; i++)
                        {
                            _plane_pvf[i] = na[i];
                        }
                    });
            }

            {
                cmd.RequestAsyncReadback(plane_ovf_Buffer,
                    (read) =>
                    {
                        var na = read.GetData<float4>();
                        for (int i = 0; i < planeOvfCount; i++)
                        {
                            _plane_ovf[i] = na[i];
                        }
                    });
            }

            {
                cmd.RequestAsyncReadback(pvf_vertex_Buffer,
                    (read) =>
                    {
                        var na = read.GetData<Vertex>();
                        for (int i = 0; i < pvfVtxOutCount; i++)
                        {
                            _pvf_vertex[i] = na[i];
                        }
                    });
            }

            {
                cmd.RequestAsyncReadback(ovf_vertex_Buffer,
                    (read) =>
                    {
                        var na = read.GetData<Vertex>();
                        for (int i = 0; i < ovfVtxOutCount; i++)
                        {
                            _ovf_vertex[i] = na[i];
                        }
                    });
            }

            {
                cmd.RequestAsyncReadback(sphere_vertex_Out_Buffer,
                    (read) =>
                    {
                        var na = read.GetData<Vertex>();
                        for (int i = 0; i < spVtxOutCount; i++)
                        {
                            _sphere_vertex_Out[i] = na[i];
                        }
                    });
            }

            {
                cmd.RequestAsyncReadback(cullResult_pvf_Texture,
                    (read) =>
                    {
                        for (int i = 0; i < groupPvfCount; i++)
                        {
                            var na = read.GetData<float>(i);
                            for (int j = 0; j < pvfCount; j++)
                            {
                                for (int k = 0; k < spCount; k++)
                                {
                                    _cullResult_pvf[i * pvfCount * spCount + j * spCount + k] = na[j * spCount + k];
                                }
                            }
                        }
                    });
            }

            {
                cmd.RequestAsyncReadback(cullResult_ovf_Texture,
                    (read) =>
                    {
                        for (int i = 0; i < groupOvfCount; i++)
                        {
                            var na = read.GetData<float>(i);
                            for (int j = 0; j < ovfCount; j++)
                            {
                                for (int k = 0; k < spCount; k++)
                                {
                                    _cullResult_ovf[i * ovfCount * spCount + j * spCount + k] = na[j * spCount + k];
                                }
                            }
                        }
                    });
            }

            {
                cmd.RequestAsyncReadback(sphere_Out_Buffer,
                    (read) =>
                    {
                        var na = read.GetData<float4>();
                        for (int i = 0; i < spCount; i++)
                        {
                            _sphere_out[i] = na[i];
                        }
                    });
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
         
    }


    

    void ReleaseCBuffer(ComputeBuffer cbuffer)
    {
        if (cbuffer != null) cbuffer.Release();
    }

    void ReleaseGBuffer(GraphicsBuffer gbuffer)
    {
        if (gbuffer != null) gbuffer.Release();
    }

    void ReleaseRenTexture(RenderTexture tex)
    {
        if (tex != null) tex.Release();
    }

    void DisposeNa<T>(NativeArray<T> na) where T : struct
    {
        if (na.IsCreated) na.Dispose();
    }

    void DisposeTraa(TransformAccessArray traa)
    {
        if (traa.isCreated) traa.Dispose();
    }

    [System.Serializable]
    public struct Info_VF
    {
        public float4 fi;
        public float3 pos;
        public float4 rot;
    }

    [System.Serializable]
    public struct Vertex
    {
        public float3 position;
        public float3 normal;
    }

    [BurstCompile]
    struct SphereAction : IJobParallelForTransform
    {
        [ReadOnly]
        public NativeArray<float3> spOffset;

        public NativeArray<float3> spCenter;

        public void Execute(int i, TransformAccess traa)
        {
            float3 posL = spOffset[i];
            float4x4 W = traa.localToWorldMatrix;
            float3 posW;

            posW = math.mul(W, new float4(posL, 1.0f)).xyz;

            spCenter[i] = posW;
        }
    }

    [BurstCompile]
    struct SphereTransform : IJobParallelForTransform
    {        
        public NativeArray<float4x4> spTr;

        public void Execute(int i, TransformAccess traa)
        {
            float4x4 tr = float4x4.zero;

            tr.c0.xyz = traa.localPosition;
            tr.c1 = ((quaternion)traa.localRotation).value;
            tr.c2.xyz = traa.localScale;

            spTr[i] = tr;
        }
    }


    //Test
    void ComputeDebug(ScriptableRenderContext context, Camera cam)
    {
        if (cam.cameraType == CameraType.SceneView)
        {
            WriteToResourceDebug(context, cam);
            DispatchComputeDebug(context, cam);
            ReadFromResourceDebug(context, cam);
        }
    }

    void WriteToResourceDebug(ScriptableRenderContext context, Camera cam)
    {

    }

    void DispatchComputeDebug(ScriptableRenderContext context, Camera cam)
    {
        CommandBuffer cmd = CommandBufferPool.Get();

        //cmd.DispatchCompute(cshader, ki_pvf, totalPvfCount, 1, 1);
        //cmd.DispatchCompute(cshader, ki_ovf, totalOvfCount, 1, 1);

        cmd.DispatchCompute(cshader, ki_pvf_vertex, totalPvfCount, 1, 1);
        cmd.DispatchCompute(cshader, ki_ovf_vertex, totalOvfCount, 1, 1);
        cmd.DispatchCompute(cshader, ki_sphere_vertex, spCount, 1, 1);

        //cmd.DispatchCompute(cshader, ki_pvf_cull_sphere, dpSpCount, pvfCount, 1);
        //cmd.DispatchCompute(cshader, ki_ovf_cull_sphere, dpSpCount, ovfCount, 1);

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    void ReadFromResourceDebug(ScriptableRenderContext context, Camera cam)
    {
        CommandBuffer cmd = CommandBufferPool.Get();

        //{
        //    cmd.RequestAsyncReadback(plane_pvf_Buffer,
        //        (read) =>
        //        {
        //            var na = read.GetData<float4>();
        //            for (int i = 0; i < planePvfCount; i++)
        //            {
        //                _plane_pvf[i] = na[i];
        //            }
        //        });
        //}
        //
        //{
        //    cmd.RequestAsyncReadback(plane_ovf_Buffer,
        //        (read) =>
        //        {
        //            var na = read.GetData<float4>();
        //            for (int i = 0; i < planeOvfCount; i++)
        //            {
        //                _plane_ovf[i] = na[i];
        //            }
        //        });
        //}

        {
            cmd.RequestAsyncReadback(pvf_vertex_Buffer,
                (read) =>
                {
                    var na = read.GetData<Vertex>();
                    for (int i = 0; i < pvfVtxOutCount; i++)
                    {
                        _pvf_vertex[i] = na[i];
                    }
                });
        }

        {
            cmd.RequestAsyncReadback(ovf_vertex_Buffer,
                (read) =>
                {
                    var na = read.GetData<Vertex>();
                    for (int i = 0; i < ovfVtxOutCount; i++)
                    {
                        _ovf_vertex[i] = na[i];
                    }
                });
        }

        {
            cmd.RequestAsyncReadback(sphere_vertex_Out_Buffer,
                (read) =>
                {
                    var na = read.GetData<Vertex>();
                    for (int i = 0; i < spVtxOutCount; i++)
                    {
                        _sphere_vertex_Out[i] = na[i];
                    }
                });
        }

        //{
        //    cmd.RequestAsyncReadback(cullResult_pvf_Texture,
        //        (read) =>
        //        {
        //            for (int i = 0; i < groupPvfCount; i++)
        //            {
        //                var na = read.GetData<float>(i);
        //                for (int j = 0; j < pvfCount; j++)
        //                {
        //                    for (int k = 0; k < spCount; k++)
        //                    {
        //                        _cullResult_pvf[i * pvfCount * spCount + j * spCount + k] = na[j * spCount + k];
        //                    }
        //                }
        //            }
        //        });
        //}
        //
        //{
        //    cmd.RequestAsyncReadback(cullResult_ovf_Texture,
        //        (read) =>
        //        {
        //            for (int i = 0; i < groupOvfCount; i++)
        //            {
        //                var na = read.GetData<float>(i);
        //                for (int j = 0; j < ovfCount; j++)
        //                {
        //                    for (int k = 0; k < spCount; k++)
        //                    {
        //                        _cullResult_ovf[i * ovfCount * spCount + j * spCount + k] = na[j * spCount + k];
        //                    }
        //                }
        //            }
        //        });
        //}

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }
}
