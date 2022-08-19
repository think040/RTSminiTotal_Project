using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Unity.Mathematics;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class CSMAction : MonoBehaviour
{
    Light lit;
    Camera mainCam;
    int igdt;

    //void Awake()
    //{
    //    lit = GetComponent<Light>();
    //    mainCam = Camera.main;
    //
    //    //csmAction = trLight.GetComponent<CSMAction>();
    //    //float[] csmRange = new[] { 0.0f, 0.4f, 1.0f };
    //    //float[] csmRange = new[] { 0.0f, 1.0f };
    //    float[] csmRange = new[] { 0.0f, 0.1f, 0.25f, 0.5f, 1.0f };
    //    //float[] csmRange = new[] { 0.0f, 0.5f, 0.7f, 0.9f, 1.0f };
    //    dw = 2048;
    //    dh = 2048;
    //    dws = new float[4];
    //    dhs = new float[4];
    //
    //    dws[0] = 2048.0f;
    //    dws[1] = 800.0f;
    //    dws[2] = 1000.0f;
    //    dws[3] = 1200.0f;
    //
    //    dhs[0] = 2048.0f;
    //    dhs[1] = 800.0f;
    //    dhs[2] = 1000.0f;
    //    dhs[3] = 1200.0f;
    //
    //    StartCSM_Job(csmRange);
    //    InitRenderTex();
    //}


    public void Init()
    {
        lit = GetComponent<Light>();
        mainCam = Camera.main;

        //csmAction = trLight.GetComponent<CSMAction>();
        //float[] csmRange = new[] { 0.0f, 0.4f, 1.0f };
        //float[] csmRange = new[] { 0.0f, 1.0f };
        float[] csmRange = new[] { 0.0f, 0.1f, 0.25f, 0.5f, 1.0f };
        //float[] csmRange = new[] { 0.0f, 0.5f, 0.7f, 0.9f, 1.0f };
        dw = 2048;
        dh = 2048;
        dws = new float[4];
        dhs = new float[4];

        dws[0] = 2048.0f;
        dws[1] = 800.0f;
        dws[2] = 1000.0f;
        dws[3] = 1200.0f;

        dhs[0] = 2048.0f;
        dhs[1] = 800.0f;
        dhs[2] = 1000.0f;
        dhs[3] = 1200.0f;

        StartCSM_Job(csmRange);
        InitRenderTex();
    }

    public void InitRoom()
    {
        lit = GetComponent<Light>();
        mainCam = Camera.main;

        //csmAction = trLight.GetComponent<CSMAction>();
        //float[] csmRange = new[] { 0.0f, 0.4f, 1.0f };
        //float[] csmRange = new[] { 0.0f, 1.0f };
        //float[] csmRange = new[] { 0.0f, 0.1f, 0.25f, 0.5f, 1.0f };
        float[] csmRange = new[] { 0.0f, 0.25f, 0.5f, 0.75f, 1.0f };
        //float[] csmRange = new[] { 0.0f, 0.5f, 0.7f, 0.9f, 1.0f };
        dw = 2048;
        dh = 2048;
        dws = new float[4];
        dhs = new float[4];

        dws[0] = 2048.0f;
        dws[1] = 800.0f;
        dws[2] = 1000.0f;
        dws[3] = 1200.0f;

        dhs[0] = 2048.0f;
        dhs[1] = 800.0f;
        dhs[2] = 1000.0f;
        dhs[3] = 1200.0f;

        StartCSM_Job(csmRange);
        InitRenderTex();
    }

    public void InitPlay()
    {
        lit = GetComponent<Light>();
        mainCam = Camera.main;

        //csmAction = trLight.GetComponent<CSMAction>();
        //float[] csmRange = new[] { 0.0f, 0.4f, 1.0f };
        //float[] csmRange = new[] { 0.0f, 1.0f };
        float[] csmRange = new[] { 0.0f, 0.1f, 0.25f, 0.5f, 1.0f };
        //float[] csmRange = new[] { 0.0f, 0.5f, 0.7f, 0.9f, 1.0f };
        dw = 2048;
        dh = 2048;
        dws = new float[4];
        dhs = new float[4];

        dws[0] = 2048.0f;
        dws[1] = 800.0f;
        dws[2] = 1000.0f;
        dws[3] = 1200.0f;

        dhs[0] = 2048.0f;
        dhs[1] = 800.0f;
        dhs[2] = 1000.0f;
        dhs[3] = 1200.0f;

        StartCSM_Job(csmRange);
        InitRenderTex();
    }

    void Start()
    {

    }

    //void Update()
    //{
    //    float dv = 0.01f;
    //
    //    if (Input.GetKey(KeyCode.Keypad6))
    //    {
    //        transform.position += new Vector3(dv, 0.0f, 0.0f);
    //    }
    //    else if (Input.GetKey(KeyCode.Keypad4))
    //    {
    //        transform.position -= new Vector3(dv, 0.0f, 0.0f);
    //    }
    //
    //    if (Input.GetKey(KeyCode.KeypadPlus))
    //    {
    //        transform.position += new Vector3(0.0f, dv, 0.0f);
    //    }
    //    else if (Input.GetKey(KeyCode.KeypadMinus))
    //    {
    //        transform.position -= new Vector3(0.0f, dv, 0.0f);
    //    }
    //
    //    if (Input.GetKey(KeyCode.Keypad8))
    //    {
    //        transform.position += new Vector3(0.0f, 0.0f, dv);
    //    }
    //    else if (Input.GetKey(KeyCode.Keypad5))
    //    {
    //        transform.position -= new Vector3(0.0f, 0.0f, dv);
    //    }
    //}        

    

    [HideInInspector]
    public float[] csmRange
    {
        get; set;
    }
    NativeArray<CfromWJob.CBuffer> cBuffer;
    NativeArray<CfromWJob.Input> input;
    NativeArray<CfromWJob.Output> output;
    [HideInInspector]
    public Matrix4x4[] CV;
    [HideInInspector]
    public Matrix4x4[] CV_depth;
    [HideInInspector]
    public Matrix4x4[] TCV_depth;
    [HideInInspector]
    public float[] endZ;
    [HideInInspector]
    public int csmNum;
    [HideInInspector]
    public float4 dirW;

    [HideInInspector]
    public float4[] pos;
    [HideInInspector]
    public quaternion[] rot;
    [HideInInspector]
    public float4[] fi;


    void SetIGDT()
    {
        GraphicsDeviceType gdt = SystemInfo.graphicsDeviceType;
        if (gdt == GraphicsDeviceType.Direct3D11 || gdt == GraphicsDeviceType.Direct3D12)
        {
            igdt = 0;
        }
        else if (gdt == GraphicsDeviceType.OpenGLCore || gdt == GraphicsDeviceType.OpenGLES3)
        {
            igdt = 1;
        }
        else if (gdt == GraphicsDeviceType.Vulkan)
        {
            igdt = 2;
        }
    }

    public void StartCSM_Job(float[] csmRange)
    {
        SetIGDT();

        this.csmRange = csmRange;
        csmNum = csmRange.Length - 1;
        cBuffer = new NativeArray<CfromWJob.CBuffer>(1, Allocator.Persistent);
        input = new NativeArray<CfromWJob.Input>(csmNum, Allocator.Persistent);
        output = new NativeArray<CfromWJob.Output>(csmNum, Allocator.Persistent);

        CV = new Matrix4x4[csmNum];
        CV_depth = new Matrix4x4[csmNum];
        TCV_depth = new Matrix4x4[csmNum];
        endZ = new float[csmNum];
        //RenderGOM.CV = CV;
        //RenderGOM.CV_depth = CV_depth;
        //RenderGOM.TCV_depth = TCV_depth;
        //RenderGOM.endZ = endZ;       

        pos = new float4[csmNum];
        rot = new quaternion[csmNum];
        fi = new float4[csmNum];
    }

    //public int csmCount;
    public RenderTexture[] drenTexs;
    //public Texture2DArray drenTexArray;
    public int dw;
    public int dh;

    public float[] dws;
    public float[] dhs;
    public const float specularFactor = 2.0f;

    public RenderTexture drenTex_array;
    public RenderTargetIdentifier drenTari;

    public Action AfterCSMUpdate = () => { };

    public bool bCullTest = false;

    public Action CullAction
    { get; set; } = () => { };

    ComputeBuffer CSMdataBuffer;

    struct CSMdata
    {
        public float4x4 CV;
        public float4x4 CV_depth;
    }

    void InitRenderTex()
    {
        //RenderGOM.BeginCameraRender += BeginCameraRender;
        RenderGOM.UpdateCSM += UpdateCSM;

        CSMdataBuffer = new ComputeBuffer(csmNum, Marshal.SizeOf<CSMdata>(), ComputeBufferType.Structured, ComputeBufferMode.SubUpdates);

        //2dArray
        {
            RenderTextureDescriptor rtd = new RenderTextureDescriptor();
            {
                rtd.msaaSamples = 1;
                rtd.depthBufferBits = 24;
                rtd.enableRandomWrite = false;

                rtd.colorFormat = RenderTextureFormat.RFloat;
                rtd.dimension = TextureDimension.Tex2DArray;
                rtd.width = (int)dw;
                rtd.height = (int)dh;
                rtd.volumeDepth = csmNum;
            }
            drenTex_array = new RenderTexture(rtd);
            drenTari = new RenderTargetIdentifier(drenTex_array, 0, CubemapFace.Unknown, -1);
        }        
        RenderGOM.InitCSMData_GS(csmNum);

        RenderGOM.PreRenderCSM_GS += PreRenderCSM_GS;


        SceneManager.sceneUnloaded +=
            (Scene) =>
            {                
                OnDestroy();
            };
    }

    private void OnDestroy()
    {
        DispoesCSM_Job();

        //RenderGOM.BeginCameraRender -= BeginCameraRender;
        RenderGOM.UpdateCSM -= UpdateCSM;
        RenderGOM.PreRenderCSM_GS -= PreRenderCSM_GS;
    }

    void BeginCameraRender(ScriptableRenderContext context, Camera cam)
    {
        this.UpdateCSM_Job(context, cam);
    }

    void UpdateCSM(ScriptableRenderContext context, Camera cam)
    {
        this.UpdateCSM_Job(context, cam);
    }

    void PreRenderCSM_GS(ScriptableRenderContext context, Camera cam)
    {
        CommandBuffer cmd = CommandBufferPool.Get();
        cmd.SetRenderTarget(drenTex_array, 0, CubemapFace.Unknown, -1);
        //cmd.SetRenderTarget(drenTari, 0, CubemapFace.Unknown, -1);
        cmd.ClearRenderTarget(true, true, Color.white, 1.0f);

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

   

    public bool bCalFarPlane = true;

    public void UpdateCSM_Job(ScriptableRenderContext context, Camera vcam)
    {
        Light light = lit;

        //if (bCalFarPlane && vcam.cameraType != CameraType.SceneView)
        if (bCalFarPlane)
        {
            Compute_FarPlane(vcam);
        }
        else
        {
            //if (vcam.cameraType == CameraType.SceneView)
            //{
            //    Camera camera = vcam;
            //    vcam.fieldOfView = mainCam.fieldOfView;
            //    vcam.aspect = mainCam.aspect;
            //    vcam.nearClipPlane = mainCam.nearClipPlane;
            //    //vcam.farClipPlane = mainCam.farClipPlane;
            //    //vcam.farClipPlane = 60.0f;
            //
            //    //Debug.Log("SceneView Far : " + vcam.farClipPlane);
            //}
        }

        if (vcam.cameraType == CameraType.SceneView)
        {
            Camera camera = vcam;
            //vcam.fieldOfView = mainCam.fieldOfView;
            //vcam.aspect = mainCam.aspect;
            //vcam.nearClipPlane = mainCam.nearClipPlane;
            //vcam.farClipPlane = mainCam.farClipPlane;

            //Debug.Log("SceneView Far : " + vcam.farClipPlane);
        }

        if (light.type == LightType.Directional)
        {
            CfromWJob.CBuffer cb = new CfromWJob.CBuffer();
            cb.vfov_2 = vcam.fieldOfView * 0.5f;
            cb.aspect = vcam.aspect;
            cb.near = vcam.nearClipPlane;
            cb.far = vcam.farClipPlane;
            cb.lightCamRot = transform.rotation;
            cb.W = RenderUtil.GetWfromV(vcam);
            cb.C = RenderUtil.GetCfromV(vcam, false);
            cb.igdt = igdt;
            cBuffer[0] = cb;

            float e = 0.00f; // 0.05f;
            for (int i = 0; i < csmNum; i++)
            {
                CfromWJob.Input ip = new CfromWJob.Input();
                ip.zn = csmRange[0];
                //ip.zn = csmRange[i];
                ip.zf = csmRange[i + 1] + e;
                input[i] = ip;
            }

            CfromWJob job = new CfromWJob();
            job.cbuffer = cBuffer;
            job.input = input;
            job.output = output;
            job.Schedule<CfromWJob>(csmNum, 1).Complete();
            //JobHandle handle = job.Schedule<CfromWJob>(csmNum, 1);
            //handle.Complete();

            //for(int i = 0; i < csmNum; i++)
            //{
            //    job.Execute(i);
            //}

            for (int i = 0; i < csmNum; i++)
            {
                CV[i] = job.output[i].CV;
                CV_depth[i] = job.output[i].CV_depth;
                TCV_depth[i] = math.mul(RenderUtil.GetTfromN(), CV_depth[i]);
                endZ[i] = job.output[i].endZ;

                pos[i] = new float4(job.output[i].pos, 1.0f);
                rot[i] = job.output[i].rot;
                fi[i] = job.output[i].fi;
            }
            dirW = new float4(job.output[0].dirW, 0.0f);
        }

        {
            var na = CSMdataBuffer.BeginWrite<CSMdata>(0, csmNum);
            for (int i = 0; i < csmNum; i++)
            {
                CSMdata data;
                data.CV = CV[i];
                data.CV_depth = CV_depth[i];
                na[i] = data;
            }
            CSMdataBuffer.EndWrite<CSMdata>(csmNum);


            CommandBuffer cmd = CommandBufferPool.Get();
            cmd.SetGlobalBuffer("csmDataBuffer", CSMdataBuffer);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
    

    public void DispoesCSM_Job()
    {
        if (cBuffer.IsCreated) cBuffer.Dispose();
        if (input.IsCreated) input.Dispose();
        if (output.IsCreated) output.Dispose();

        if (CSMdataBuffer != null) CSMdataBuffer.Release();
    }

    //static int mode = 0;

    void Update()
    {
        //if(Input.GetKeyDown(KeyCode.N))
        //{
        //    mode++;
        //    if(mode > 2)
        //    {
        //        mode = 0;
        //    }
        //}
        //
        //if (Input.GetKeyDown(KeyCode.M))
        //{
        //    mode--;
        //    if(mode < 0)
        //    {
        //        mode = 2;
        //    }
        //}
    }

    [BurstCompile]
    struct CfromWJob : IJobParallelFor
    {
        public struct CBuffer
        {
            public float vfov_2;
            public float aspect;
            public float near;
            public float far;
            public quaternion lightCamRot;
            public float4x4 W;
            public float4x4 C;
            public int igdt;
        }
        public struct Input
        {
            public float zn;
            public float zf;
        }
        public struct Output
        {
            public float4x4 CV;
            public float4x4 CV_depth;
            public float3 dirW;
            public float endZ;

            public float3 pos;
            public quaternion rot;
            public float4 fi;
        }

        [ReadOnly]
        public NativeArray<CBuffer> cbuffer;
        [ReadOnly]
        public NativeArray<Input> input;
        public NativeArray<Output> output;

       

        [BurstCompile]
        public void Execute(int i)
        {
            Output result = new Output();

            float vfov_2 = cbuffer[0].vfov_2;
            float aspect = cbuffer[0].aspect;
            float near = cbuffer[0].near;
            float far = cbuffer[0].far;
            float fn = far - near;
            float endZ = 0.0f;

            float4x4 C_view = cbuffer[0].C;
            float4x4 W_view = cbuffer[0].W;
            quaternion rot_light = cbuffer[0].lightCamRot;
            int igdt = cbuffer[0].igdt;

            float2 zRatio = new float2(input[i].zn, input[i].zf);
            zRatio = (new float2(near, near) + fn * zRatio);
            float4 posC_view = math.mul(C_view, new float4(0.0f, 0.0f, zRatio.y, 1.0f));
            endZ = posC_view.z / posC_view.w;

            zRatio /= new float2(near, near);
            //float zn = (near + fn * input[i].zn) / near;
            //float zf = (near + fn * input[i].zf) / near;
            float zn = near;
            float zf = far;

            float yn = math.tan(math.radians(vfov_2)) * zn;
            float xn = aspect * yn;

            //
            //Debug.Log(zRatio.ToString());
            float4x4 nP = float4x4.identity;
            float4x4 fP = float4x4.identity;
            nP.c0 = new float4(new float3(+xn, +yn, +zn) * zRatio.x, 1.0f);
            nP.c1 = new float4(new float3(-xn, +yn, +zn) * zRatio.x, 1.0f);
            nP.c2 = new float4(new float3(-xn, -yn, +zn) * zRatio.x, 1.0f);
            nP.c3 = new float4(new float3(+xn, -yn, +zn) * zRatio.x, 1.0f);
            fP.c0 = new float4(new float3(+xn, +yn, +zn) * zRatio.y, 1.0f);
            fP.c1 = new float4(new float3(-xn, +yn, +zn) * zRatio.y, 1.0f);
            fP.c2 = new float4(new float3(-xn, -yn, +zn) * zRatio.y, 1.0f);
            fP.c3 = new float4(new float3(+xn, -yn, +zn) * zRatio.y, 1.0f);
            float3 center = (
                nP.c0.xyz + nP.c1.xyz + nP.c2.xyz + nP.c3.xyz +
                fP.c0.xyz + fP.c1.xyz + fP.c2.xyz + fP.c3.xyz) / 8.0f;
            center = math.mul(W_view, new float4(center, 1.0f)).xyz;

            //
            float4x4 V = float4x4.identity;
            float4x4 V2V = float4x4.identity;
            V = RenderUtil.GetVfromW(center, rot_light);
            //V2V = math.mul(V, cbuffer[0].W);
            V2V = math.mul(V, W_view);
            nP = math.mul(V2V, nP);
            fP = math.mul(V2V, fP);

            // 
            float size = 0.0f;
            float maxZ = 0.0f;
            float minZ = 0.0f;

            float sk = 1.2f;
            //sk = 1.0f;
            unsafe
            {
                float* narrx = stackalloc float[8];
                float* narry = stackalloc float[8];
                float* narrz = stackalloc float[8];

                narrx[0] = math.abs(nP.c0.x); narry[0] = math.abs(nP.c0.y);
                narrx[1] = math.abs(nP.c1.x); narry[1] = math.abs(nP.c1.y);
                narrx[2] = math.abs(nP.c2.x); narry[2] = math.abs(nP.c2.y);
                narrx[3] = math.abs(nP.c3.x); narry[3] = math.abs(nP.c3.y);
                narrx[4] = math.abs(fP.c0.x); narry[4] = math.abs(fP.c0.y);
                narrx[5] = math.abs(fP.c1.x); narry[5] = math.abs(fP.c1.y);
                narrx[6] = math.abs(fP.c2.x); narry[6] = math.abs(fP.c2.y);
                narrx[7] = math.abs(fP.c3.x); narry[7] = math.abs(fP.c3.y);
                size = sk * math.max(Max(narrx, 8), Max(narry, 8));

                narrz[0] = nP.c0.z;
                narrz[1] = nP.c1.z;
                narrz[2] = nP.c2.z;
                narrz[3] = nP.c3.z;
                narrz[4] = fP.c0.z;
                narrz[5] = fP.c1.z;
                narrz[6] = fP.c2.z;
                narrz[7] = fP.c3.z;
                maxZ = Max(narrz, 8);
                minZ = Min(narrz, 8);
            }
            //Debug.Log("minZ : " + minZ.ToString());
            //Debug.Log("maxZ : " + maxZ.ToString());
            //Debug.Log(minZ.ToString() + " " + maxZ.ToString());

            float4x4 C = float4x4.identity;
            float4x4 C_depth = float4x4.identity;

            float3 axisz = math.rotate(rot_light, new float3(0.0f, 0.0f, 1.0f));
            //quaternion _rot = math.conjugate(rot);
            //float3 axisz = math.mul(math.mul(rot, new quaternion(0.0f, 0.0f, 1.0f, 0.0f)), _rot).value.xyz;

            float3 pos = center + minZ * axisz;
            //float3 pos = center;
            V = RenderUtil.GetVfromW(pos, rot_light);

            float outFar = sk * (maxZ - minZ);
            //float outFar = sk * maxZ;
            //float outFar = sk * (math.abs(maxZ) + math.abs(minZ));

            float4 fi = float4.zero;
            //if (igdt == 0 || igdt == 2)
            //{
                //fi = new float4(size, 1.0f, 0.0f, outFar);
                //fi = new float4(size, 1.0f, minZ, maxZ);                            

                //if(CSMAction.mode == 0)
                //{
                //    fi = new float4(size, 1.0f, 0.0f, 1.0f * maxZ);
                //}
                //else if(CSMAction.mode == 1)
                //{
                //    fi = new float4(size, 1.0f, 0.0f, 2.0f * maxZ);
                //}
                //else if (CSMAction.mode == 2)
                //{
                //    fi = new float4(size, 1.0f, 0.0f, 3.0f * maxZ);
                //}

            //}
            //else if(igdt == 1)
            {
                float k = -1.0f;
                //fi = new float4(size, 1.0f, 0.0f, outFar);
                fi = new float4(size, 1.0f, k * outFar, outFar);
                //fi = new float4(size, 1.0f, -outFar, outFar);
            }
            
            RenderUtil.GetCfromV_Ortho_Optimized(fi.x, fi.y, fi.z, fi.w, out C, out C_depth, igdt);
            //C = RenderUtil.GetCfromV_Ortho(size, 1.0f, 0.0f, outFar, true, cbuffer[0].igdt);
            //C_depth = RenderUtil.GetCfromV_Ortho(size, 1.0f, 0.0f, outFar, false, cbuffer[0].igdt);

            result.CV = math.mul(C, V);
            result.CV_depth = math.mul(C_depth, V);
            result.dirW = -axisz;
            result.endZ = endZ;

            result.pos = pos;
            result.rot = rot_light;
            result.fi = fi;

            output[i] = result;
        }

        [BurstCompile]
        unsafe float Max(float* a, int num)
        {
            float max = a[0];
            for (int i = 1; i < num; i++)
            {
                if (a[i] > max)
                {
                    max = a[i];
                }
            }

            return max;
        }

        [BurstCompile]
        unsafe float Min(float* a, int num)
        {
            float min = a[0];
            for (int i = 1; i < num; i++)
            {
                if (a[i] < min)
                {
                    min = a[i];
                }
            }

            return min;
        }
    }


    //
    public static float Compute_FarPlane(Camera cam)
    {
        float fov = cam.fieldOfView;
        float aspect = cam.aspect;
        float near = cam.nearClipPlane;
        float far = cam.farClipPlane;

        if (cam.cameraType == CameraType.SceneView)
        {
            Camera mainCam = Camera.main;
            fov = mainCam.fieldOfView;
            aspect = mainCam.aspect;
            near = mainCam.nearClipPlane;
            //far = mainCam.farClipPlane;
        }

        float3 pos = cam.transform.position;

        float yn = near * math.tan(math.radians(fov * 0.5f));
        float xn = aspect * yn;

        //float3 forward = near * math.normalize(math.rotate(cam.transform.rotation, new float3(0.0f, 0.0f, 1.0f)));
        float4x4 W = cam.transform.localToWorldMatrix;
        float4 forward = W.c2;
        float4x4 vecs = float4x4.zero;
        vecs.c0 = new float4(+xn, +yn, near, 0.0f);
        vecs.c1 = new float4(-xn, +yn, near, 0.0f);
        vecs.c2 = new float4(-xn, -yn, near, 0.0f);
        vecs.c3 = new float4(+xn, -yn, near, 0.0f);
        vecs = math.mul(W, vecs);

        float3x2 plane;
        plane.c0 = new float3(0.0f, -0.1f, 0.0f);
        plane.c1 = new float3(0.0f, 1.0f, 0.0f);
        float3x2 line;
        line.c0 = pos;

        float maxDist = 0.0f;
        int maxIndex = 0;
        unsafe
        {
            float* dist = stackalloc float[4];
            float3x2* lines = stackalloc float3x2[4];

            lines[0].c1 = vecs.c0.xyz;
            lines[1].c1 = vecs.c1.xyz;
            lines[2].c1 = vecs.c2.xyz;
            lines[3].c1 = vecs.c3.xyz;

            for (int i = 0; i < 4; i++)
            {
                lines[i].c0 = pos;
                dist[i] = Compute_Dist(plane, lines[i]);
                //Debug.Log("dist" + i.ToString() + " : " + dist[i].ToString());
            }

            maxDist = max(dist, 4, &maxIndex);

            far = Compute_maxDist(forward.xyz, maxDist, lines[maxIndex].c1);

        }

        cam.farClipPlane = 1.1f * far;
        //cam.farClipPlane = far;
        return far;
    }


    static float Compute_Dist(float3x2 plane, float3x2 line)
    {
        float dist = 0.0f;
        float3 n = -math.normalize(plane.c1);
        float l = math.length(line.c1);
        float3 dir = line.c1 / l;
        float nDotDir = math.dot(n, dir);
        //if (nDotDir > 0.25f)
        if (nDotDir > 0.0001f)
        {
            //dist = math.length((math.dot(n, plane.c0 - line.c0) / math.max(0.0001f, math.dot(n, line.c1))) * line.c1);
            dist = math.length((math.dot(n, plane.c0 - line.c0) / (l * nDotDir) * line.c1));
        }
        else
        {
            //dist = 50.0f;
            //dist = 100.0f;
            dist = 300.0f;
        }

        //return math.min(dist, 200.0f);
        return math.min(dist, 500.0f);
    }

    static float Compute_maxDist(float3 dir, float maxDist, float3 lineDir)
    {
        float max = 0.0f;
        float3 n = math.normalize(dir);
        max = math.dot(n, maxDist * math.normalize(lineDir));

        return max;
    }

    static unsafe float max(float* ptr, int num, int* index)
    {
        float max = 0.0f;
        max = *ptr;
        *index = 0;
        for (int i = 0; i < num; i++)
        {
            if (max < *(ptr + i))
            {
                max = *(ptr + i);
                *index = i;
            }
        }

        return max;
    }
}
