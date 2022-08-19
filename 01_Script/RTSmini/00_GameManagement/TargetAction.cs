using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Unity.Mathematics;
using Unity.Collections;

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

using Random = Unity.Mathematics.Random;

public class TargetAction : MonoBehaviour
{   
    public static UnitActor[] unitActors;
    public static int2[] modeIc;
    //public static SelectMode selectMode;

    public ComputeShader cshader;
    int ki_trW;
    int ki_tPos;
    int ki_tEnemy;
    public Transform[] cubeXZtrs;
    public bool bDebug;


    public void Init()
    {
        unitActors = GameManager.unitActors;
        //modeIc = SelectManager.modeIc;
        //selectMode = SelectManager.selectMode;        
        maxCount = GameManager.maxUnitCount;
        count = GameManager.unitCount;

        {            
            ki_trW = cshader.FindKernel("CS_TrW");
            //ki_tPos = cshader.FindKernel("CS_TargetPos");
            ki_tPos = cshader.FindKernel("CS_TargetPos_CubeXZ");
            ki_tEnemy = cshader.FindKernel("CS_TargetEnemy");
        }


        InitResource();

        {
            GamePlay.UpdateAI += UpdateTarget;

            SceneManager.sceneUnloaded += OnSceneLeft;
        }

#if UNITY_EDITOR
        bDebug = true;
#else
            bDebug = false;
#endif
    }

    void OnSceneLeft(Scene scene)
    {
        OnDestroy();
    }

    private void OnDestroy()
    {
        SceneManager.sceneUnloaded -= OnSceneLeft;

        GamePlay.UpdateAI -= UpdateTarget;

        ReleaseResource();
    }

    int maxCount = 256;
    int count;

    ComputeBuffer trM_Buffer;
    ComputeBuffer trW_Buffer;
    ComputeBuffer circle_Buffer;

    ComputeBuffer block_Buffer;
    ComputeBuffer random_Buffer;
    public static ComputeBuffer active_Buffer
    {
        get; set;
    }
    ComputeBuffer cubeXZplane_Buffer;
    ComputeBuffer cubeCenter_Buffer;
    ComputeBuffer refTargetPos_Buffer;
    ComputeBuffer targetPos_Buffer;

    ComputeBuffer unitData_Buffer;
    ComputeBuffer minDist_Buffer;

    ComputeBuffer debug_Buffer;

    Random ran;

    public static float4x4[] trM;
    public static float4x4[] trW;
    public static float4[] circle;

    public static float3[] block;
    public static int[] random;
    public static bool[] active;
    public static float4[] cubeXZplane;
    public static float3[] cubeCenter;      
    public static float3[] refTargetPos;
    public static float3[] targetPos;
    //public static NativeArray<float3> targetPos;


    public static float4[] unitData;
    public static float4[] minDist;
    //public static NativeArray<float4> minDist;


    public float4x4[] _trM;
    public float4x4[] _trW;
    public float4[] _circle;

    public float3[] _block;
    public int[] _random;
    public bool[] _active;
    public float4[] _cubeXZplane;
    public float3[] _cubeCenter;
    public float3[] _refTargetPos;
    public float3[] _targetPos;

    public float4[] _unitData;
    public float4[] _minDist;

    public float4x4[] _debug;

    void InitResource()
    {
        trM_Buffer = new ComputeBuffer(maxCount, Marshal.SizeOf<float4x4>(), ComputeBufferType.Structured, ComputeBufferMode.SubUpdates);
        //trM_Buffer = new ComputeBuffer(maxCount, Marshal.SizeOf<float4x4>());
        trW_Buffer = new ComputeBuffer(maxCount, Marshal.SizeOf<float4x4>());
        circle_Buffer = new ComputeBuffer(maxCount, Marshal.SizeOf<float4>());

        block_Buffer = new ComputeBuffer(8, Marshal.SizeOf<float3>(), ComputeBufferType.Structured, ComputeBufferMode.Immutable);
        random_Buffer = new ComputeBuffer(maxCount, Marshal.SizeOf<int>(), ComputeBufferType.Structured, ComputeBufferMode.SubUpdates);
        //active_Buffer = new ComputeBuffer(maxCount, Marshal.SizeOf<bool>(), ComputeBufferType.Structured, ComputeBufferMode.SubUpdates);
        active_Buffer = new ComputeBuffer(maxCount, Marshal.SizeOf<int>(), ComputeBufferType.Structured, ComputeBufferMode.SubUpdates);
        cubeXZplane_Buffer = new ComputeBuffer(16, Marshal.SizeOf<float4>(), ComputeBufferType.Structured, ComputeBufferMode.Immutable);
        cubeCenter_Buffer   = new ComputeBuffer(4, Marshal.SizeOf<float3>(), ComputeBufferType.Structured, ComputeBufferMode.Immutable);
        refTargetPos_Buffer = new ComputeBuffer(maxCount, Marshal.SizeOf<float3>(), ComputeBufferType.Structured, ComputeBufferMode.SubUpdates);
        targetPos_Buffer = new ComputeBuffer(maxCount, Marshal.SizeOf<float3>());

        unitData_Buffer = new ComputeBuffer(maxCount, Marshal.SizeOf<float4>(), ComputeBufferType.Structured, ComputeBufferMode.Immutable);
        minDist_Buffer = new ComputeBuffer(maxCount, Marshal.SizeOf<float4>());

        debug_Buffer = new ComputeBuffer(maxCount, Marshal.SizeOf<float4x4>());

        cshader.SetInt("count", count);

        cshader.SetBuffer(ki_trW, "trM_Buffer", trM_Buffer);
        cshader.SetBuffer(ki_trW, "trW_Buffer", trW_Buffer);
        cshader.SetBuffer(ki_trW, "circle_Buffer", circle_Buffer);

        cshader.SetBuffer(ki_tPos, "block_Buffer", block_Buffer);
        cshader.SetBuffer(ki_tPos, "random_Buffer", random_Buffer);
        cshader.SetBuffer(ki_tPos, "active_Buffer", active_Buffer);
        cshader.SetBuffer(ki_tPos, "trW_Buffer", trW_Buffer);
        cshader.SetBuffer(ki_tPos, "circle_Buffer", circle_Buffer);
        cshader.SetBuffer(ki_tPos, "cubeXZplane_Buffer", cubeXZplane_Buffer);
        cshader.SetBuffer(ki_tPos, "cubeCenter_Buffer",  cubeCenter_Buffer);
        cshader.SetBuffer(ki_tPos, "refTargetPos_Buffer", refTargetPos_Buffer);
        cshader.SetBuffer(ki_tPos, "targetPos_Buffer", targetPos_Buffer);
        cshader.SetBuffer(ki_tPos, "debug_Buffer", debug_Buffer);

        cshader.SetBuffer(ki_tEnemy, "active_Buffer", active_Buffer);        
        cshader.SetBuffer(ki_tEnemy, "unitData_Buffer", unitData_Buffer);
        cshader.SetBuffer(ki_tEnemy, "circle_Buffer", circle_Buffer);
        cshader.SetBuffer(ki_tEnemy, "minDist_Buffer", minDist_Buffer);

        {
            trM = new float4x4[maxCount];
            trW = new float4x4[maxCount];
            circle = new float4[maxCount];

            block = new float3[8];
            random = new int[maxCount];
            active = GameManager.activeData;
            cubeXZplane = new float4[16];
            cubeCenter = new float3[4];
            refTargetPos = new float3[maxCount];
            targetPos = new float3[maxCount];
            //targetPos = new NativeArray<float3>(maxCount, Allocator.Persistent);
            

            unitData = new float4[maxCount];
            minDist = new float4[maxCount];
            //minDist = new NativeArray<float4>(maxCount, Allocator.Persistent);

            _trM = trM;
            _trW = trW;
            _circle = circle;

            _block = block;
            _random = random;
            _active = active;
            _cubeXZplane = cubeXZplane;
            _cubeCenter = cubeCenter;
            _refTargetPos = refTargetPos;
            _targetPos = targetPos;

            _unitData = unitData;
            _minDist = minDist;

            _debug = new float4x4[maxCount];
        }        


        {
            for(int i = 0; i < maxCount; i++)
            {
                trM[i] = float4x4.identity;
                trW[i] = float4x4.identity;

                //Debug.Log("count : " + count.ToString());
                if(i < count)
                {                    
                    float r = GameManager.unitCols[i].radius;
                    float3 sca = GameManager.unitTrs[i].localScale;
                    float s = math.max(sca.x, sca.z);
                
                    circle[i].w = r * s;
                }
            }

            circle_Buffer.SetData(circle);
        }

        {            
            block[0] = new float3(-2.0f, 0.0f, -2.0f);
            block[1] = new float3(+0.0f, 0.0f, -2.0f);
            block[2] = new float3(+2.0f, 0.0f, -2.0f);

            block[3] = new float3(-2.0f, 0.0f, +0.0f);

            block[4] = new float3(+2.0f, 0.0f, +0.0f);

            block[5] = new float3(-2.0f, 0.0f, +2.0f);
            block[6] = new float3(+0.0f, 0.0f, +2.0f);
            block[7] = new float3(+2.0f, 0.0f, +2.0f);

            block_Buffer.SetData(block);
        }

        {
            ran = new Random();
            ran.InitState();           
            
            for(int i = 0; i < maxCount; i++)
            {
                if(i < count)
                {
                    random[i] = 0;
                }
                //targetPos[i] = float3.zero;
            }
        }

        {
            
            for (int i = 0; i < maxCount; i++)
            {
                unitData[i] = float4.zero;
                if (i < count)
                {
                    unitData[i].x = (float)unitActors[i].pNum;
                    unitData[i].y = (float)unitActors[i].vRadius;
                }

                //minDist[i] = float4.zero;
            }

            unitData_Buffer.SetData(unitData);
        }

        unsafe
        {
            for(int i = 0; i < 4; i++)
            {
                float4 center;
                float4x4 plane;
                plane = RenderUtil.Cube_To_XZPlane(cubeXZtrs[i], out center);
        
                float4* ptr = (float4*)&plane;                                
                for(int j = 0; j < 4; j++)
                {
                    cubeXZplane[i * 4 + j] = *(ptr + j);
                }
        
                cubeCenter[i] = center.xyz;
            }
        
            cubeXZplane_Buffer.SetData(cubeXZplane);
            cubeCenter_Buffer.SetData(cubeCenter);            
        }

    }

    public void Begin()
    {
        //int unitManCount = GameManager.unitManCount;
        //UnitManager[] unitMan = GameManager.unitMans;
        //
        //if(unitManCount > 0)
        //{
        //    for(int i = 0; i < unitManCount; i++)
        //    {
        //        unitMan[i].SetFromTargetAction(active_Buffer);
        //    }
        //}
    }

    void UpdateTarget()
    {
        Compute();
    }

    void Update()
    {
        if(GamePlay.isResume)
        {
            Compute();
        }        
    }

    public void Compute()
    {
        WriteToResource();
        DispatcCompute();
        ReadFromCompute();
    }

    void WriteToResource()
    {
        for(int i = 0; i < maxCount; i++)
        {
            if(i < count)
            {
                {
                    Transform tr = GameManager.unitTrs[i];
                    trM[i].c0 = new float4(tr.localPosition, 0.0f);
                    trM[i].c1 = ((quaternion)tr.localRotation).value;
                    trM[i].c2 = new float4(tr.localScale, 0.0f);
                }

                {
                    random[i] = ran.NextInt(0, 7);
                }
            }
        }

        {
            var na = trM_Buffer.BeginWrite<float4x4>(0, maxCount);
            for(int i = 0; i < maxCount; i++)
            {
                na[i] = trM[i];
            }
            trM_Buffer.EndWrite<float4x4>(maxCount);

            //trM_Buffer.SetData(trM);
        }

        {
            var na = random_Buffer.BeginWrite<int>(0, maxCount);
            for(int i = 0; i < maxCount; i++)
            {
                na[i] = random[i];
            }
            random_Buffer.EndWrite<int>(maxCount);
        }

        {
            var na = active_Buffer.BeginWrite<int>(0, maxCount);
            for (int i = 0; i < maxCount; i++)
            {
                na[i] = active[i] ? 1 : 0;
            }
            active_Buffer.EndWrite<int>(maxCount);
        }

        {
            targetPos_Buffer.SetData(targetPos);            
        }

        {
            var na = refTargetPos_Buffer.BeginWrite<float3>(0, maxCount);
            for (int i = 0; i < maxCount; i++)
            {
                na[i] = refTargetPos[i];
            }
            refTargetPos_Buffer.EndWrite<float3>(maxCount);
        }
    }


    void DispatcCompute()
    {
        {
            CommandBuffer cmd = CommandBufferPool.Get();

            cmd.DispatchCompute(cshader, ki_trW, 4, 1, 1);
            cmd.DispatchCompute(cshader, ki_tPos, maxCount, 1, 1);
            cmd.DispatchCompute(cshader, ki_tEnemy, maxCount, 1, 1);
            
            cmd.SetGlobalBuffer("active_Buffer", active_Buffer);

            Graphics.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }

    void DispatcCompute1()
    {
        {
            CommandBuffer cmd = CommandBufferPool.Get();

            cmd.DispatchCompute(cshader, ki_trW, 4, 1, 1);

            Graphics.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        {
            CommandBuffer cmd = CommandBufferPool.Get();

            cmd.SetExecutionFlags(CommandBufferExecutionFlags.AsyncCompute);
            cmd.DispatchCompute(cshader, ki_tPos, maxCount, 1, 1);
            cmd.DispatchCompute(cshader, ki_tEnemy, maxCount, 1, 1);

            Graphics.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
       
    }

    


    void ReadFromCompute()
    {
        {
            {
                var read = AsyncGPUReadback.Request(targetPos_Buffer);
                read.WaitForCompletion();
                for (int i = 0; i < maxCount; i++)
                {
                    targetPos[i] = read.GetData<float3>()[i];
                }
            }
        
            {
                var read = AsyncGPUReadback.Request(minDist_Buffer);
                read.WaitForCompletion();
                for (int i = 0; i < maxCount; i++)
                {
                    minDist[i] = read.GetData<float4>()[i];
                }
            }
        
        }

        if(bDebug)
        {
            {
                var read = AsyncGPUReadback.Request(trW_Buffer);
                read.WaitForCompletion();
                for (int i = 0; i < maxCount; i++)
                {
                    trW[i] = read.GetData<float4x4>()[i];
                }
            }

            {
                var read = AsyncGPUReadback.Request(circle_Buffer);
                read.WaitForCompletion();
                for (int i = 0; i < maxCount; i++)
                {
                    circle[i] = read.GetData<float4>()[i];
                }
            }


            //{
            //    var read = AsyncGPUReadback.Request(targetPos_Buffer);
            //    read.WaitForCompletion();
            //    for (int i = 0; i < maxCount; i++)
            //    {
            //        targetPos[i] = read.GetData<float3>()[i];
            //    }
            //}
            //
            //{
            //    var read = AsyncGPUReadback.Request(minDist_Buffer);
            //    read.WaitForCompletion();
            //    for (int i = 0; i < maxCount; i++)
            //    {
            //        minDist[i] = read.GetData<float4>()[i];
            //    }
            //}


            {
                var read = AsyncGPUReadback.Request(debug_Buffer);
                read.WaitForCompletion();
                for (int i = 0; i < maxCount; i++)
                {
                    _debug[i] = read.GetData<float4x4>()[i];
                }
            }
        }        
    }

    public void ReleaseResource()
    {
        ReleaseCBuffer(trM_Buffer);
        ReleaseCBuffer(trW_Buffer);
        ReleaseCBuffer(circle_Buffer);

        ReleaseCBuffer(block_Buffer);
        ReleaseCBuffer(random_Buffer);
        ReleaseCBuffer(active_Buffer);
        ReleaseCBuffer(cubeXZplane_Buffer);
        ReleaseCBuffer(cubeCenter_Buffer);
        ReleaseCBuffer(refTargetPos_Buffer);
        ReleaseCBuffer(targetPos_Buffer);
        ReleaseCBuffer(debug_Buffer);

        ReleaseCBuffer(unitData_Buffer);
        ReleaseCBuffer(minDist_Buffer);
        

        //DisposeNa<float3>(targetPos);
        //DisposeNa<float4>(minDist);
    }

    void ReleaseCBuffer(ComputeBuffer cbuffer)
    {
        if (cbuffer != null) cbuffer.Release();
    }

    void DisposeNa<T>(NativeArray<T> na) where T : struct
    {
        if (na.IsCreated) na.Dispose();
    }
}
