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

public class ComputeAnimTest : MonoBehaviour
{
    public ComputeShader cshader;

    void Start()
    {
        InitCompute();
    }
   
    void Update()
    {
        Compute();
    }

    void OnDestroy()
    {
        ReleaseResource();
    }

    int kidx;
    void InitCompute()
    {
        kidx = cshader.FindKernel("CSMain");

        InitResource();        
    }    

    void Compute()
    {
        CommandBuffer cmd = CommandBufferPool.Get();
       
        {
            WriteToResource(cmd);
            DispatchCompute(cmd);
            ReadFromResource(cmd);
        }
      
        Graphics.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    const int countDepth = 5;
    const int countData = 31;
    //const int countMaxData = 1024;

    float4 countInfo;
    int[]       data_mask;
    int[]       data_parent;
    public float4[]    data_input;
    public float4[]    data_output;

    ComputeBuffer   buffer_mask;
    ComputeBuffer   buffer_parent;
    ComputeBuffer   buffer_input;
    ComputeBuffer   buffer_output;

    void InitResource()
    {
        {
            countInfo.x = countDepth;
            countInfo.y = countData;            
        }

        {
            //data_mask = new int[countData];
            //data_parent = new int[countData];
            data_mask = new[] {
                0,
                1, 1,
                2, 2, 2, 2,
                3, 3, 3, 3, 3, 3, 3, 3,
                4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4};
            data_parent = new[] {
                -1,
                0, 0,
                1, 1, 2, 2,
                3, 3, 4, 4, 5, 5, 6, 6,
                7, 7, 8, 8, 9, 9, 10, 10, 11, 11, 12, 12, 13, 13, 14, 14};


            data_input = new float4[countData];                        
            data_output = new float4[countData];
        }

        {
            buffer_mask = new ComputeBuffer(countData, sizeof(int), ComputeBufferType.Structured, ComputeBufferMode.Immutable);
            buffer_parent = new ComputeBuffer(countData, sizeof(int), ComputeBufferType.Structured, ComputeBufferMode.Immutable);
            buffer_input = new ComputeBuffer(countData, Marshal.SizeOf<float4>(), ComputeBufferType.Structured, ComputeBufferMode.SubUpdates);
            buffer_output = new ComputeBuffer(countData, Marshal.SizeOf<float4>());
        }

        {
            buffer_mask.SetData(data_mask);
            buffer_parent.SetData(data_parent);
        }


        {
            for(int i = 0; i < countData; i++)
            {
                data_input[i] = new float4(1.0f, 0.0f, 0.0f, (float)data_mask[i]);
            }
        }

        {
            cshader.SetVector("countInfo", countInfo);
            cshader.SetBuffer(kidx, "buffer_mask", buffer_mask);
            cshader.SetBuffer(kidx, "buffer_parent", buffer_parent);
            cshader.SetBuffer(kidx, "buffer_input", buffer_input);
            cshader.SetBuffer(kidx, "buffer_output", buffer_output);
        }
    }


    void WriteToResource(CommandBuffer cmd)
    {
        {
            var na = buffer_input.BeginWrite<float4>(0, countData);
            for(int i = 0; i < countData; i++)
            {
                na[i] = data_input[i];
            }
            buffer_input.EndWrite<float4>(countData);
        }
    }

    void DispatchCompute(CommandBuffer cmd)
    {
        cmd.DispatchCompute(cshader, kidx, 1, 1, 1);
    }

    void ReadFromResource(CommandBuffer cmd)
    {
        {
            cmd.RequestAsyncReadback(buffer_output,
                (read) =>
                {
                    var na = read.GetData<float4>(0);
                    for(int i = 0; i < countData; i++)
                    {
                        data_output[i] = na[i];
                    }
                    
                });
        }
    }

    void ReleaseResource()
    {
        ReleaseCBuffer(buffer_mask);
        ReleaseCBuffer(buffer_parent);
        ReleaseCBuffer(buffer_input);
        ReleaseCBuffer(buffer_output);
    }

    void ReleaseCBuffer(ComputeBuffer cBuffer)
    {
        if (cBuffer != null) cBuffer.Release();
    }
}
