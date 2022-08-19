using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

using Unity.Mathematics;
using Unity.Burst;
using Unity.Collections;

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Jobs;

using Random = Unity.Mathematics.Random;

public class SelectManager : MonoBehaviour
{
    public static UnitActor[] unitActors;
    public static int2[] modeIc;
    public static SelectMode selectMode;
    public SelectMode _selectMode = SelectMode.UnitAll;
    public static KeyCode key_muti_select = KeyCode.E;  //KeyCode.V  //KeyCode.LeftControl
    public static KeyCode key_hold = KeyCode.Q; //KeyCode.H //KeyCode.Q

    public static KeyCode key_select_group = KeyCode.Space; //KeyCode.Space

    int unitCount;
    static int mIndex;
    static int mCount;
    static int mEnd;

    public static int[] selectData
    {
        get; private set;
    }

    public static int[] selectGroup
    {
        get; set;
    }

    public int[] _selectData;
    public int[] _selectGroup;

    public bool[] activeData;

    Camera mainCam;

    public ComputeShader cshader_rectIn;
    RectIn rectIn;

    public void Init()
    {
        unitCount = GameManager.unitCount;

        if(unitCount > 0)
        {
            unitActors = GameManager.unitActors;

            selectMode = _selectMode;            
            mainCam = Camera.main;

            modeIc = new int2[3];
            {
                modeIc[0] = new int2(GameManager.ic[0].x, GameManager.unitCount);
                modeIc[1] = new int2(GameManager.ic[0].x, GameManager.ic[0].y + GameManager.ic[1].y);
                modeIc[2] = new int2(GameManager.ic[2].x, GameManager.ic[2].y + GameManager.ic[3].y);

                if (selectMode == SelectMode.UnitAll)
                {
                    lmask = LayerMask.GetMask("Unit0", "Unit1");
                    mIndex = modeIc[0].x;
                    mCount = modeIc[0].y;                    
                }
                else if (selectMode == SelectMode.Unit0)
                {
                    lmask = LayerMask.GetMask("Unit0");
                    mIndex = modeIc[1].x;
                    mCount = modeIc[1].y;
                }
                else if (selectMode == SelectMode.Unit1)
                {
                    lmask = LayerMask.GetMask("Unit1");
                    mIndex = modeIc[2].x;
                    mCount = modeIc[2].y;
                }

                mEnd = mIndex + mCount;
            }

            {
                selectData = new int[unitCount];
                _selectData = selectData;

                SetSelectAll(false);
            }

            {
                selectGroup = new int[unitCount];
                _selectGroup = selectGroup;                

                SetSelectGroupAll(-1);
                InitSelectGroup();
            }

            {
                activeData = GameManager.activeData;
            }

            //{
            //    rectIn = new RectIn();
            //    rectIn.Init(cshader_rectIn);
            //}

            StartCoroutine(SelectAction());
            StartCoroutine(SelectActionGroup());
            StartCoroutine(MoveAction());
        }
       
    }

    public void SetSelectAll(bool value)
    {
        for(int i = 0; i < GameManager.unitCount; i++)
        {
            selectData[i] = value ? 1 : 0;
        }
    }


    public void SetSelectGroupAll(int value)
    {
        for(int i = 0; i < unitCount; i++)
        {
            selectGroup[i] = value;
        }
    }

    public void Begin()
    {

        if(unitCount > 0)
        {
            rectIn = new RectIn();
            rectIn.Init(cshader_rectIn);
        }
    }


    void Start()
    {
        
    }

    void OnDestroy()
    {
        if (rectIn != null) rectIn.ReleaseResource();
    }

    public RectTransform rtTrSelect;

    LayerMask lmask;  

    IEnumerator SelectAction()
    {
        bool down = false;
        float3 rp0 = float3.zero;
        float3 rp1 = float3.zero;

        {
            rtTrSelect.anchorMin = new Vector2(0.0f, 0.0f);
            rtTrSelect.anchorMax = new Vector2(0.0f, 0.0f);
            rtTrSelect.pivot = new Vector2(0.0f, 0.0f);
        }
        
        while (true)
        {
            if (GamePlay.isResume)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    {
                        //Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
                        Ray ray = RenderUtil.GetRay_WfromS(Input.mousePosition, mainCam);
                        RaycastHit hit;

                        if (Physics.Raycast(ray, out hit, 100, lmask))
                        {
                            UnitActor hActor = hit.transform.GetComponent<UnitActor>();

                            if (hActor != null)
                            {
                                if (hActor.isActive)
                                {
                                    if (Input.GetKey(key_muti_select))
                                    {
                                        if (hActor.isSelected)
                                        { hActor.isSelected = false; }
                                        else
                                        { hActor.isSelected = true; }
                                    }
                                    else
                                    {
                                        SetSelectAll(false);
                                        hActor.isSelected = true;
                                    }
                                }
                            }
                        }
                        else
                        {
                            SetSelectAll(false);
                        }
                    }


                    {
                        down = true;
                        rp0 = Input.mousePosition;
                    }
                }


                if (down)
                {
                    rp1 = Input.mousePosition;
                    if (math.distance(rp0, rp1) > 1.0f)
                    {
                        Rect rt = new Rect(math.min(rp0, rp1).xy, math.abs(rp1 - rp0).xy);
                        //rt = new Rect(new Vector2(100.0f, 100.0f), new Vector2(100.0f, 100.0f));
                        rtTrSelect.offsetMin = rt.min;
                        rtTrSelect.offsetMax = rt.max;

                        rectIn.rect = rt;
                        rectIn.Test();
                    }

                }
            }

                if (Input.GetMouseButtonUp(0))
                {
                    down = false;
                    rtTrSelect.offsetMin = float2.zero;
                    rtTrSelect.offsetMax = float2.zero;
                }               
            //}

            yield return null;
        }       
    }


    

    void InitSelectGroup()
    {
        if(selectMode == SelectMode.UnitAll)
        {

        }
        else if(selectMode == SelectMode.Unit0)
        {

        }
        else if(selectMode == SelectMode.Unit1)
        {

        }

        int gCount = mCount % 4 == 0 ? mCount / 4 : mCount / 4 + 1;

        for(int i = 0; i < mCount; i++)
        {
            selectGroup[mIndex + i] = (i / gCount) + 1;
        }
    }


    IEnumerator SelectActionGroup()
    {
        while (true)
        {
            if (GamePlay.isResume)
            {
                if (Input.GetKey(key_select_group))
                {
                    if (Input.GetKeyDown(KeyCode.Alpha1))
                    {
                        SelectGroup_Write(1);
                    }
                    else if (Input.GetKeyDown(KeyCode.Alpha2))
                    {
                        SelectGroup_Write(2);
                    }
                    else if (Input.GetKeyDown(KeyCode.Alpha3))
                    {
                        SelectGroup_Write(3);
                    }
                    else if (Input.GetKeyDown(KeyCode.Alpha4))
                    {
                        SelectGroup_Write(4);
                    }
                }
                else
                {
                    if (Input.GetKeyDown(KeyCode.Alpha1))
                    {
                        SelectGroup_Read(1);
                    }
                    else if (Input.GetKeyDown(KeyCode.Alpha2))
                    {
                        SelectGroup_Read(2);
                    }
                    else if (Input.GetKeyDown(KeyCode.Alpha3))
                    {
                        SelectGroup_Read(3);
                    }
                    else if (Input.GetKeyDown(KeyCode.Alpha4))
                    {
                        SelectGroup_Read(4);
                    }


                }

            }

            yield return null;
        }
    }

    void SelectGroup_Write(int id)
    {      
        for (int i = mIndex; i < mEnd; i++)
        {           
            int gId = selectGroup[i];
            int isSelect = selectData[i];
            if (gId == id)
            {                
                if(isSelect == 0)
                {
                    selectGroup[i] = -1;
                }
            }
            else
            {
                if(isSelect == 1)
                {
                    selectGroup[i] = id;
                }
            }
        }
    }
    
    void SelectGroup_Read(int id)
    {        
        for (int i = mIndex; i < mEnd; i++)
        {            
            bool isActive = activeData[i];
            if(isActive)
            {
                int gId = selectGroup[i];
                //if (gId == id)
                if (gId == id)
                {
                    selectData[i] = 1;
                }
                else
                {
                    selectData[i] = 0;
                }
            }
            //else
            //{
            //    selectData[i] = 0;
            //}
        }
    }

   

    
    static public float3 movePos;   

    IEnumerator MoveAction()
    {
        KeyCode key_orbit = CamAction.key_orbit;
        KeyCode key_spin = CamAction.key_spin;

        while (true)
        {
            if (GamePlay.isResume)
            {
                if (Input.GetMouseButtonDown(1) &&
                !Input.GetKey(key_orbit) && !Input.GetKey(key_spin) &&
                GamePlay.isResume && !GamePlay.isNvStop)
                {
                    if (mainCam != null)
                    {
                        //Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
                        Ray ray = RenderUtil.GetRay_WfromS(Input.mousePosition, mainCam);
                        RaycastHit hit;

                        //if (Physics.Raycast(ray, out hit, 100, LayerMask.GetMask("MovePlane")))
                        if (Physics.Raycast(ray, out hit, 100, LayerMask.GetMask("MovePlane", "Unit0", "Unit1")))
                        {
                            movePos = hit.point;
                            Transform htr = hit.transform;

                            for (int i = 0; i < unitCount; i++)
                            {
                                UnitActor uactor = unitActors[i];

                                if (uactor.isActive)
                                {
                                    if (uactor.isSelected)
                                    {
                                        UnitActor hactor = htr.GetComponent<UnitActor>();

                                        if (hactor != null)
                                        {
                                            if (uactor.pNum != hactor.pNum)
                                            {
                                                uactor.SetAttackTr(htr, 0);
                                                uactor.positionTr = null;
                                                //Debug.Log("attack pid");
                                            }
                                            else
                                            {
                                                uactor.ClearAttackTr();
                                                uactor.positionTr = htr;
                                                uactor.targetPos = movePos;
                                            }
                                        }
                                        else
                                        {
                                            uactor.ClearAttackTr();
                                            uactor.positionTr = htr;
                                            uactor.targetPos = movePos;

                                            //inPlane = true;
                                            //StartCoroutine(torusPos.ShowTorus(hitPos));
                                        }


                                    }
                                }
                            }
                        }
                    }
                }
            }
            
            yield return null;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //if(Input.GetKeyDown(KeyCode.Alpha1))
        //{
        //    Debug.Log("KeyCode.Alpha1");
        //}
    }
    

    [System.Serializable]
    public enum SelectMode
    {
        UnitAll,
        Unit0,
        Unit1        
    }

    class RectIn
    {
        public void Test()
        {
            //WriteToNa();
            ExecuteJob();
            //ReadFromNa();

            WriteToResource();
            DispatchCompute();
            ReadFromResource();
        }

        ComputeShader cshader;
        int ki_RenctIn;
        TransformJob job;

        int unitCount;
        //int maxUnitCount;

        SelectMode sMode;
        int mIndex;
        int mCount;
        int idxStart;
        int idxEnd;
        int dpCount;

        //
        Transform[] unitTrs;
        Camera mainCam;
        Transform mainCamTr;

        int[] selectData;

        //
        public Rect rect;
        float4x4 S;
        float4x4 CV;
        
        TransformAccessArray traa;
        NativeArray<float3> naPos;
        
        ComputeBuffer  SCV_Buffer;
        ComputeBuffer  pos_Buffer;
        ComputeBuffer  inRect_Buffer;

        public void Init(ComputeShader cshader)
        {
            this.cshader = cshader;
            ki_RenctIn = cshader.FindKernel("CS_RectIn");
            job = new TransformJob();

            {
                unitCount = GameManager.unitCount;
                //maxUnitCount = GameManager.maxUnitCount;

                sMode = SelectManager.selectMode;
                mIndex = SelectManager.mIndex;
                mCount = SelectManager.mCount;

                idxStart = mIndex;
                idxEnd = mIndex + mCount - 1;

                dpCount = unitCount % 64 == 0 ? unitCount / 64 : unitCount / 64 + 1;
            }

            {
                unitTrs = GameManager.unitTrs;
                mainCam = Camera.main;
                mainCamTr = mainCam.transform;

                selectData = SelectManager.selectData;
            }

            InitResouce();
        }
            
        void InitResouce()
        {
            {
                traa = new TransformAccessArray(unitTrs);
                naPos = new NativeArray<float3>(unitCount, Allocator.Persistent);

                SCV_Buffer = new ComputeBuffer(2, Marshal.SizeOf<float4x4>(), ComputeBufferType.Structured, ComputeBufferMode.SubUpdates);
                pos_Buffer = new ComputeBuffer(unitCount, Marshal.SizeOf<float3>(), ComputeBufferType.Structured, ComputeBufferMode.SubUpdates);
                inRect_Buffer = new ComputeBuffer(unitCount, sizeof(int));
            }

            {
                job.naPos = naPos;
            }

            {
                cshader.SetBuffer(ki_RenctIn, "SCV_Buffer", SCV_Buffer);
                cshader.SetBuffer(ki_RenctIn, "pos_Buffer", pos_Buffer);
                cshader.SetBuffer(ki_RenctIn, "active_Buffer", TargetAction.active_Buffer);
                cshader.SetBuffer(ki_RenctIn, "inRect_Buffer", inRect_Buffer);

                cshader.SetInt("idxStart", idxStart);
                cshader.SetInt("idxEnd", idxEnd);
            }
            
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void WriteToNa()
        {

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ExecuteJob()
        {
            job.Schedule<TransformJob>(traa).Complete();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ReadFromNa()
        {

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void WriteToResource()
        {
            RenderUtil.GetMat_SfromW(mainCamTr, mainCam, out S, out CV);

            {
                var na = SCV_Buffer.BeginWrite<float4x4>(0, 2);
                na[0] = S;
                na[1] = CV;
                SCV_Buffer.EndWrite<float4x4>(2);
            }

            {
                var na = pos_Buffer.BeginWrite<float3>(0, unitCount);
                for(int i = 0; i < unitCount; i++)
                {
                    na[i] = naPos[i];
                }
                pos_Buffer.EndWrite<float3>(unitCount);
            }

            {
                cshader.SetVector("rect", new float4(rect.x, rect.y, rect.width, rect.height));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void DispatchCompute()
        {
            CommandBuffer cmd = CommandBufferPool.Get();

            cmd.DispatchCompute(cshader, ki_RenctIn, dpCount, 1, 1);

            Graphics.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ReadFromResource()
        {
            {
                var read = AsyncGPUReadback.Request(inRect_Buffer);
                read.WaitForCompletion();
                var na = read.GetData<int>();
                for(int i = 0; i < unitCount; i++)
                {
                    selectData[i] = na[i];
                }
            }
        }

        public void ReleaseResource()
        {
            DisposeTraa(traa);
            DisposeNa<float3>(naPos);

            ReleaseCBuffer(SCV_Buffer);
            ReleaseCBuffer(pos_Buffer);
            ReleaseCBuffer(inRect_Buffer);
        }

        void ReleaseCBuffer(ComputeBuffer cBuffer)
        {
            if (cBuffer != null) cBuffer.Release();
        }

        void DisposeNa<T>(NativeArray<T> na) where T : struct
        {
            if (na.IsCreated) na.Dispose();
        }

        void DisposeTraa(TransformAccessArray traa)
        {
            if (traa.isCreated) traa.Dispose();
        }

        [BurstCompile]
        struct TransformJob : IJobParallelForTransform
        {
            [WriteOnly]
            public NativeArray<float3> naPos;

            public void Execute(int i, TransformAccess tra)
            {
                naPos[i] = tra.position;
            }
        }
    }

    //Test
    IEnumerator MoveAction0()
    {
        KeyCode key_orpit = CamAction.key_orbit;
        KeyCode key_spin = CamAction.key_spin;

        while (true)
        {
            if (Input.GetMouseButtonDown(1) && !Input.GetKey(key_orpit) && !Input.GetKey(key_spin))
            {
                if (mainCam != null)
                {
                    //Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
                    Ray ray = RenderUtil.GetRay_WfromS(Input.mousePosition, mainCam);
                    RaycastHit hit;
                    //if (Physics.Raycast(ray, out hit, 100, LayerMask.GetMask("MovePlane", "Unit0", "Unit1")))
                    if (Physics.Raycast(ray, out hit, 100, LayerMask.GetMask("MovePlane")))
                    {
                        movePos = hit.point;

                        for (int i = 0; i < unitCount; i++)
                        {
                            UnitActor actor = unitActors[i];
                            if (actor.isSelected)
                            {
                                actor.targetPos = movePos;
                            }
                        }
                    }
                }
            }

            yield return null;
        }
    }

}




