using System.Collections;
using System.Collections.Generic;

using Unity.Mathematics;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;

using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.UI;

public class UIunitRoomManager : MonoBehaviour
{

    void Awake()
    {
        {
            rectWorld = new RectWorld();
            rectWorld.Init(rectTrs, bRects);

            StartCoroutine(rectWorld.Schedule());
            _rectOut = rectOut = rectWorld.rectOutData;
        }
    }
    
    void Start()
    {
       
    }
    
    void Update()
    {
        
    }

    void OnDestroy()
    {
        if (rectWorld != null) rectWorld.ReleaseResourece();
    }

    public RectTransform[] rectTrs;
    public bool[] bRects;

    RectWorld rectWorld;
    public RectOut[] _rectOut;
    public static RectOut[] rectOut;

    public static bool inRects
    {
        get
        {           
            if(rectOut != null)
            {
                int count = rectOut.Length;
                for(int i = 0; i < count; i++)
                {
                    if (rectOut[i].bIn)
                    {
                        return true;
                    }                  
                }
            }

            return false;
        }
    }

    public class RectWorld
    {
        public IEnumerator Schedule()
        {
            InitResource();
            while (true)
            {
                WriteToResourece();
                ExecuteJob();
                ReadFromResourece();

                yield return null;
            }
            ReleaseResourece();
        }

        public void Init(RectTransform[] rectTrs, bool[] bRects)
        {
            this.rectTrs = rectTrs;
            this.bRects = bRects;
            this.count = rectTrs.Length;

            rectInData = new RectIn[count];
            rectOutData = new RectOut[count];

            traa = new TransformAccessArray(count);
            rectIn = new NativeArray<RectIn>(count, Allocator.Persistent);
            rectOut = new NativeArray<RectOut>(count, Allocator.Persistent);

            job = new ActionJob();
        }

        int count;
        RectTransform[] rectTrs;
        bool[] bRects;
        public float3 posS;
        RectIn[] rectInData;
        public RectOut[] rectOutData;

        TransformAccessArray traa;
        NativeArray<RectIn> rectIn;
        NativeArray<RectOut> rectOut;

        ActionJob job;

        void InitResource()
        {
            job.rectIn = rectIn;
            job.rectOut = rectOut;
        }

        void WriteToResourece()
        {
            traa.SetTransforms(rectTrs);

            for (int i = 0; i < count; i++)
            {
                RectIn rIn;
                rIn.rect = rectTrs[i].rect;
                rIn.bRect = bRects[i];
                rectIn[i] = rIn;
            }

            job.posS = Input.mousePosition;
        }

        void ExecuteJob()
        {
            job.ScheduleReadOnly<ActionJob>(traa, count).Complete();

            //job.Schedule<ActionJob>(traa).Complete();
        }

        void ReadFromResourece()
        {
            for (int i = 0; i < count; i++)
            {
                rectOutData[i] = rectOut[i];
            }
        }

        public void ReleaseResourece()
        {
            if (traa.isCreated) traa.Dispose();
            if (rectIn.IsCreated) rectIn.Dispose();
            if (rectOut.IsCreated) rectOut.Dispose();
        }

        [BurstCompile]
        struct ActionJob : IJobParallelForTransform
        {
            [ReadOnly]
            public NativeArray<RectIn> rectIn;

            [ReadOnly]
            public float3 posS;



            [WriteOnly]
            public NativeArray<RectOut> rectOut;

            public void Execute(int i, TransformAccess ta)
            {
                float4x4 W = ta.localToWorldMatrix;

                RectOut rOut = new RectOut();
                Rect rIn = rectIn[i].rect;
                bool bRect = rectIn[i].bRect;

                float2 pos = rIn.position;
                float2 size = rIn.size;

                float4 min = math.mul(W, new float4(pos, 0.0f, 1.0f));
                float4 max = math.mul(W, new float4(pos + size, 0.0f, 1.0f));

                pos = min.xy;
                size = max.xy - min.xy;

                if (bRect)
                {
                    rIn.position = pos;
                    rIn.size = size;

                    rOut.rect = rIn;
                    rOut.bIn = rIn.Contains(posS);
                }
                else
                {
                    float2 center = 0.5f * (min.xy + max.xy);
                    float radius = size.x > size.y ? 0.5f * size.y : 0.5f * size.x;

                    rIn.position = center;
                    rIn.size = new float2(radius, radius);

                    rOut.rect = rIn;
                    rOut.bIn = math.distance(center, posS.xy) > radius ? false : true;
                }


                rectOut[i] = rOut;
            }
        }



        //Test
        [BurstCompile]
        struct ActionJob0 : IJobParallelForTransform
        {
            [ReadOnly]
            public NativeArray<Rect> rectIn;

            //[WriteOnly]
            public NativeArray<Rect> rectOut;

            public void Execute(int i, TransformAccess ta)
            {
                float4x4 W = ta.localToWorldMatrix;

                Rect rect = rectIn[i];

                float2 pos = rect.position;
                float2 size = rect.size;

                float4 min = math.mul(W, new float4(pos, 0.0f, 1.0f));
                float4 max = math.mul(W, new float4(pos + size, 0.0f, 1.0f));

                pos = min.xy;
                size = max.xy - min.xy;
                rect.position = pos;
                rect.size = size;

                rectOut[i] = rect;
            }
        }

        [BurstCompile]
        struct ActionJob1 : IJobParallelForTransform
        {
            [ReadOnly]
            public NativeArray<Rect> rectIn;

            [ReadOnly]
            public float3 posS;

            [WriteOnly]
            public NativeArray<RectOut> rectOut;

            public void Execute(int i, TransformAccess ta)
            {
                float4x4 W = ta.localToWorldMatrix;

                RectOut rOut = new RectOut();
                Rect rIn = rectIn[i];

                float2 pos = rIn.position;
                float2 size = rIn.size;

                float4 min = math.mul(W, new float4(pos, 0.0f, 1.0f));
                float4 max = math.mul(W, new float4(pos + size, 0.0f, 1.0f));

                pos = min.xy;
                size = max.xy - min.xy;
                rIn.position = pos;
                rIn.size = size;

                rOut.rect = rIn;
                rOut.bIn = rIn.Contains(posS);

                rectOut[i] = rOut;
            }
        }
    }

    public struct RectIn
    {
        public Rect rect;
        public bool bRect;
    }


    [System.Serializable]
    public struct RectOut
    {
        public Rect rect;
        public bool bIn;
    }
}
