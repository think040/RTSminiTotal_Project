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

public class DQtest : MonoBehaviour
{
    public ComputeShader cshader;
        
    void Start()
    {
        InitCompute();
    }

    public bool bCompute = true;
    void Update()
    {
        if(bCompute)
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
        ki_dqTest = cshader.FindKernel("CS_DQtest");

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

    ComputeBuffer bufferIn;
    ComputeBuffer bufferOut;

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

        bufferIn = new ComputeBuffer(count, Marshal.SizeOf<DataIn>(), ComputeBufferType.Structured, ComputeBufferMode.SubUpdates);
        bufferOut = new ComputeBuffer(count, Marshal.SizeOf<DataOut>());

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
            cshader.SetBuffer(ki_dqTest, "bufferIn", bufferIn);
            cshader.SetBuffer(ki_dqTest, "bufferOut", bufferOut);
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

        for(int i = 0; i < count; i++)
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
        for (int i = 0; i < count; i++)
        {
            DataOut data = dataOut[i];

            float3 t =  data.data0.c0.xyz;
            float3 rp = data.data0.c1.xyz;
            float3 rb = data.data0.c2.xyz;
            float3 rl = data.data0.c3.xyz;

            Debug.DrawLine(rp, rl, Color.red, 0.33f);
            Debug.DrawLine(rp, rb, Color.green, 0.33f);
            Debug.DrawLine(rb, t, Color.blue, 0.33f);                  
        }
    }

    void WriteToResource(CommandBuffer cmd)
    {        
        {
            var na = bufferIn.BeginWrite<DataIn>(0, count);
            for(int i = 0; i < count; i++)
            {
                na[i] = dataIn[i];
            }
            bufferIn.EndWrite<DataIn>(count);
        }
    }

    void DispatchCompute(CommandBuffer cmd)
    {
        cmd.DispatchCompute(cshader, ki_dqTest, count, 1, 1);
    }

    void ReadFromResource(CommandBuffer cmd)
    {
        cmd.RequestAsyncReadback(bufferOut,
                    (read) =>
                    {
                        var na = read.GetData<DataOut>();
                        for (int i = 0; i < count; i++)
                        {
                            dataOut[i] = na[i];
                        }
                    });

        //AsyncGPUReadback.Request(bufferOut,
        //            (read) =>
        //            {
        //                var na = read.GetData<DataOut>();
        //                for (int i = 0; i < count; i++)
        //                {
        //                    dataOut[i] = na[i];
        //                }
        //            });
    }

    void ReleaseResource()
    {
        ReleaseCBuffer(bufferIn);
        ReleaseCBuffer(bufferOut);
    }

    void ReleaseCBuffer(ComputeBuffer cBuffer)
    {
        if (cBuffer != null) cBuffer.Release();
    }
}
