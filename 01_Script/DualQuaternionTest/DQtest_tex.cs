using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Unity.Mathematics;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Jobs;

using Utility_JSB;

public class DQtest_tex : MonoBehaviour
{
    public ComputeShader cshader;

    void Start()
    {
        InitCompute();
    }

    public bool bCompute = true;
    void Update()
    {
        if (bCompute)
        {
            Compute();
        }
        else
        {
            Execute();
        }
    }

    void OnDestroy()
    {
        ReleaseResource();
    }

    int ki_dqTest;
    void InitCompute()
    {
        ki_dqTest = cshader.FindKernel("CS_DQtest_tex");

        InitResource();
        WriteDQ();
    }

    [System.Serializable]
    public struct DataIn
    {
        public float4x4 data0;
        public float4x4 data1;
    }

    [System.Serializable]
    public struct DataOut
    {
        public float4x4 data0;
        public float4x4 data1;
    }

    public DataIn[] dataIn;
    public DataOut[] dataOut;
   
    Texture3D texIn;
    RenderTexture texOut;

    public int count = 1;


    DualQuaternion q0;
    DualQuaternion q1;
    DualQuaternion q2;

    //float3 vin;
    float o0;
    float o1;
    float3 tv0;
    float3 tv1;
    float3 ta0;
    float3 ta1;

    void Compute()
    {
        CommandBuffer cmd = CommandBufferPool.Get();

        {
            WriteDQ();
        }

        {
            WriteToResource(cmd);
            DispatchCompute(cmd);
            ReadFromResource(cmd);
        }

        {
            ReadDQ();
        }

        Graphics.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    void Execute()
    {
        for (int i = 0; i < count; i++)
        {
            float3 rp;
            float3 rb;
            float3 rl;
            q2 = DualQuaternion.scLerp_debug(q0, q1, (float)i / (float)(count - 1), out rp, out rb, out rl);


            float3 t;
            float4 r;
            q2.toRigidParam(out r, out t);

            Debug.DrawLine(rp, rl, Color.red, 0.33f);
            Debug.DrawLine(rp, rb, Color.green, 0.33f);
            Debug.DrawLine(rb, t, Color.blue, 0.33f);
        }
    }
    


    void InitResource()
    {
        dataIn = new DataIn[count];
        dataOut = new DataOut[count];

        dataIn[0] = new DataIn();
        dataOut[0] = new DataOut();        

        {
            texIn = new Texture3D(4 * count, 4, 1, TextureFormat.RGBAFloat, false);           
        }

        {
            RenderTextureDescriptor rtd = new RenderTextureDescriptor();
            rtd.colorFormat = RenderTextureFormat.ARGBFloat;
            rtd.dimension = TextureDimension.Tex3D;
            
            rtd.width = 4 * count;
            rtd.height = 4;
            rtd.volumeDepth = 1;
            
            rtd.enableRandomWrite = true;
            rtd.msaaSamples = 1;
            rtd.depthBufferBits = 0;

            texOut = new RenderTexture(rtd);
        }

        {
            dataIn[0].data0 = float4x4.zero;
            dataIn[0].data1 = float4x4.zero;
        }

        {
            float4x4 data0;
            data0.c0 = new float4(10.0f, 10.0f, 10.0f, 10.0f);
            data0.c1 = new float4(20.0f, 20.0f, 20.0f, 20.0f);
            data0.c2 = new float4(10.0f, 10.0f, 10.0f, 10.0f);
            data0.c3 = new float4(20.0f, 20.0f, 20.0f, 20.0f);

            dataIn[0].data0 = data0;
        } 

        {
            cshader.SetTexture(ki_dqTest, "texIn", texIn);
            cshader.SetTexture(ki_dqTest, "texOut", texOut);
        }
    }

    void WriteDQ()
    {
        //
        {
            o0 = 45.0f;
            o1 = 135.0f;
            tv0 = new float3(+1.0f, +0.0f, +0.0f);
            tv1 = new float3(+1.0f, +1.0f, +0.0f);
            //vin = math.normalize(new float3(1.0f, 1.0f, 0.0f));

            ta0 = math.normalize(new float3(+0.0f, +1.0f, +0.0f));
            ta1 = math.normalize(new float3(+0.0f, +1.0f, +0.0f));

            quaternion r0 = quaternion.AxisAngle(ta0, math.radians(o0));
            float3 t0 = math.rotate(r0, tv0);

            quaternion r1 = quaternion.AxisAngle(ta1, math.radians(o1));
            float3 t1 = math.rotate(r1, tv1);

            q0 = new DualQuaternion(r0, t0);
            q1 = new DualQuaternion(r1, t1);
        }

        for (int i = 0; i < count; i++)
        {
            dataIn[i].data0.c0 = q0.real;
            dataIn[i].data0.c1 = q0.dual;
            dataIn[i].data0.c2 = q1.real;
            dataIn[i].data0.c3 = q1.dual;

            dataIn[i].data1.c0 = new float4((float)count, 0.0f, 0.0f, 0.0f);
        }
    }

    void ReadDQ()
    {
        //for (int i = 0; i < count; i++)
        //{
        //    DataOut data = dataOut[i];
        //
        //    float3 t = data.data0.c0.xyz;
        //    float3 rp = data.data0.c1.xyz;
        //    float3 rb = data.data0.c2.xyz;
        //    float3 rl = data.data0.c3.xyz;
        //
        //    Debug.DrawLine(rp, rl, Color.red, 0.33f);
        //    Debug.DrawLine(rp, rb, Color.green, 0.33f);
        //    Debug.DrawLine(rb, t, Color.blue, 0.33f);
        //}

        for (int i = 0; i < count; i++)
        {
            DataOut data = dataOut[i];

            float3 t =  data.data1.c0.xyz;
            float3 rp = data.data1.c1.xyz;
            float3 rb = data.data1.c2.xyz;
            float3 rl = data.data1.c3.xyz;

            Debug.DrawLine(rp, rl, Color.red, 0.33f);
            Debug.DrawLine(rp, rb, Color.green, 0.33f);
            Debug.DrawLine(rb, t, Color.blue, 0.33f);
        }
    }

    void WriteToResource(CommandBuffer cmd)
    {       
        for(int i = 0; i < count; i++)
        {
            Vector4 data0 = dataIn[i].data0.c0;
            Vector4 data1 = dataIn[i].data0.c1;
            Vector4 data2 = dataIn[i].data0.c2;
            Vector4 data3 = dataIn[i].data0.c3;

            texIn.SetPixel(4 * i + 0, 0, 0, data0);
            texIn.SetPixel(4 * i + 1, 0, 0, data1);
            texIn.SetPixel(4 * i + 2, 0, 0, data2);
            texIn.SetPixel(4 * i + 3, 0, 0, data3);
        }

        for (int i = 0; i < count; i++)
        {
            Vector4 data0 = dataIn[i].data1.c0;
            Vector4 data1 = dataIn[i].data1.c1;
            Vector4 data2 = dataIn[i].data1.c2;
            Vector4 data3 = dataIn[i].data1.c3;

            texIn.SetPixel(4 * i + 0, 1, 0, data0);
            texIn.SetPixel(4 * i + 1, 1, 0, data1);
            texIn.SetPixel(4 * i + 2, 1, 0, data2);
            texIn.SetPixel(4 * i + 3, 1, 0, data3);
        }

        texIn.Apply();
    }

    void DispatchCompute(CommandBuffer cmd)
    {
        cmd.DispatchCompute(cshader, ki_dqTest, count, 1, 1);
    }

    void ReadFromResource(CommandBuffer cmd)
    {       
        cmd.RequestAsyncReadback(texOut,
                    (read) =>
                    {
                        for(int i = 0; i < 1; i++)
                        {
                            var na = read.GetData<float4>(i);
                            for (int j = 0; j < count; j++)
                            {
                                dataOut[j].data0.c0 = na[j * 4 + 0];
                                dataOut[j].data0.c1 = na[j * 4 + 1];
                                dataOut[j].data0.c2 = na[j * 4 + 2];
                                dataOut[j].data0.c3 = na[j * 4 + 3];

                                dataOut[j].data1.c0 = na[4 * count + j * 4 + 0];
                                dataOut[j].data1.c1 = na[4 * count + j * 4 + 1];
                                dataOut[j].data1.c2 = na[4 * count + j * 4 + 2];
                                dataOut[j].data1.c3 = na[4 * count + j * 4 + 3];                                                            
                            }
                        }                      
                    });       
    }

    void ReleaseResource()
    {
       
    }

    void ReleaseCBuffer(ComputeBuffer cBuffer)
    {
        if (cBuffer != null) cBuffer.Release();
    }

    void ReleaseTex(Texture tex)
    {
        
    }
}
