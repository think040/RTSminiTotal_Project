using Unity.Mathematics;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

using UserAnimSpace;


public class ArcherUnitManager : UnitManager
{
    

    public override void Init()
    {       
        base.Init();
       

        Spawn<ArcherActor>();

        float3 s = model.transform.localScale;
        float dx = 2.0f * s.x;
        float dz = 2.0f * s.z;
        float3 xaxis = math.rotate(transform.rotation, new float3(1.0f, 0.0f, 0.0f));
        float3 zaxis = math.rotate(transform.rotation, new float3(0.0f, 0.0f, 1.0f));
        float3 center = transform.position;

        int c = 16;
        float x0 = -(c - 1) * dx * 0.5f;      
        for (int i = 0; i < count; i++)
        {            
            Vector3 pos = new Vector3((float)(i % c) * dx, 0.0f, (float)(i / c) * dz);
            pos = center + xaxis *(x0 + pos.x) - zaxis * pos.z;
            units[i].transform.SetPositionAndRotation(pos, transform.rotation);
            units[i].SetActive(true);
        }

        //{
        //    InitAnim();
        //    BakeAnimation();
        //}

        GamePlay.UpdateBone += UpdateBoneAction;

        SceneManager.sceneUnloaded += OnSceneLeft;
    }

    public override void InitInRoom()
    {
        base.InitInRoom();
        //base.Init();

        //SpawnInRoom<ArcherActor>();
        Spawn<ArcherActor>();

        float3 s = model.transform.localScale;
        float dx = 2.0f * s.x;
        float dz = 2.0f * s.z;
        float3 xaxis = math.rotate(transform.rotation, new float3(1.0f, 0.0f, 0.0f));
        float3 zaxis = math.rotate(transform.rotation, new float3(0.0f, 0.0f, 1.0f));
        float3 center = transform.position;

        int c = 8;
        for (int i = 0; i < count; i++)
        {
            //Vector3 pos = new Vector3((float)(i % c) * dx, 0.0f, (float)(i / c) * dz);
            //pos = center + xaxis * pos.x - zaxis * pos.z;
           
            units[i].transform.SetPositionAndRotation(float3.zero, quaternion.AxisAngle(new float3(0.0f, 1.0f, 0.0f), math.radians(180.0f)));
            units[i].SetActive(true);
        }

        GamePlay.UpdateBone += UpdateBoneAction;

        SceneManager.sceneUnloaded += OnSceneLeft;
    }

    public void OnSceneLeft(Scene scene)
    {
        OnDestroy();
    }


    private void OnEnable()
    {

    }

    public GameObject[] roots;
    private void Start()
    {
        
    }

    protected override void UpdateBoneAction()
    {
        base.UpdateBoneAction();
    }


    private void Update()
    {         
        if(GamePlay.isResume)
        {
            UpdateBoneAction();
        }        
    }

    public void OnDestroy()
    {
        SceneManager.sceneUnloaded -= OnSceneLeft;

        GamePlay.UpdateBone -= UpdateBoneAction;
    }
}
