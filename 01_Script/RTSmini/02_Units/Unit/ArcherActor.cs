using System.Collections;
using System.Collections.Generic;
using System.Threading;

using Unity.Mathematics;

using UnityEngine;
using UnityEngine.SceneManagement;

using UserAnimSpace;

public class ArcherActor : UnitActor
{
    void Awake()
    {

    }

    void OnEnable()
    {

    }
       

    void Start()
    {
        //ArcherUnitManager.manager.SetArray<ArcherActor>(iid, gameObject);
        if (audioAttack != null)
        {
            audioAttack.Stop();
        }

        if (audioDie != null)
        {
            audioDie.Stop();
        }
    }
    

    public override void Init(string[] stNames, float4x4[] stM, bool hasStMesh)
    {
        base.Init(stNames, stM, hasStMesh);
        //this.viewRadius = UnitData.kInfo.vRadius;
        //this.attackRadius = UnitData.kInfo.aRadius;
        //this.hitHp =  UnitData.kInfo.hitHp;
        //this.healHp = UnitData.kInfo.healHp;

        //this.viewRadius = UnitData.unitInfo[1].vRadius;
        //this.attackRadius = UnitData.unitInfo[1].aRadius;
        //this.hitHp = UnitData.unitInfo[1].hitHp;
        //this.healHp = UnitData.unitInfo[1].healHp;
        //this.pId = 0;

        dicStates["Idle_Running"] = new UserAnimCross("Idle_Running", dicStates["Idle"], dicStates["Running"], player);
        (dicStates["Idle_Running"] as UserAnimCross).InitTime(0.5f, 0.0f, 1.0f, 0.5f, 1.0f, 0.0f);
        dicStates["Idle_Running"].isRightNow = true;

        dicStates["Running_Idle"] = new UserAnimCross("Running_Idle", dicStates["Running"], dicStates["Idle"], player);
        (dicStates["Running_Idle"] as UserAnimCross).InitTime(0.05f, 0.0f, 0.075f, 0.5f, 1.0f, 0.0f);
        dicStates["Running_Idle"].isRightNow = true;

        dicStates["Running_Attacking"] = new UserAnimCross("Running_Attacking", dicStates["Running"], dicStates["Attacking"], player);
        (dicStates["Running_Attacking"] as UserAnimCross).InitTime(0.5f, 0.0f, 1.0f, 0.5f, 1.0f, 0.0f);
        dicStates["Running_Attacking"].isRightNow = true;

        dicStates["Attacking_Running"] = new UserAnimCross("Attacking_Running", dicStates["Attacking"], dicStates["Running"], player);
        (dicStates["Attacking_Running"] as UserAnimCross).InitTime(0.5f, 0.0f, 1.0f, 0.5f, 1.0f, 0.0f);
        dicStates["Attacking_Running"].isRightNow = true;

        dicStates["Attacking_Idle"] = new UserAnimCross("Attacking_Idle", dicStates["Attacking"], dicStates["Idle"], player);
        (dicStates["Attacking_Idle"] as UserAnimCross).InitTime(0.05f, 0.0f, 0.2f, 0.5f, 1.0f, 0.0f);
        dicStates["Attacking_Idle"].isRightNow = true;

        dicStates["Idle_Attacking"] = new UserAnimCross("Idle_Attacking", dicStates["Idle"], dicStates["Attacking"], player);
        (dicStates["Idle_Attacking"] as UserAnimCross).InitTime(0.5f, 0.0f, 1.0f, 0.5f, 1.0f, 0.0f);
        dicStates["Idle_Attacking"].isRightNow = true;


        //dicStates["Idle_Die"] = new UserAnimCross("Idle_Die", dicStates["Idle"], dicStates["Die"], player);
        //(dicStates["Idle_Die"] as UserAnimCross).InitTime(0.5f, 0.0f, 1.0f, 0.5f, 1.0f, 0.0f);
        //dicStates["Idle_Die"].isRightNow = true;
        //
        //dicStates["Die_Idle"] = new UserAnimCross("Die_Idle", dicStates["Die"], dicStates["Idle"], player);
        //(dicStates["Die_Idle"] as UserAnimCross).InitTime(0.05f, 0.0f, 0.2f, 0.5f, 1.0f, 0.0f);
        //dicStates["Die_Idle"].isRightNow = true;
        //
        //dicStates["Running_Die"] = new UserAnimCross("Running_Die", dicStates["Running"], dicStates["Die"], player);
        //(dicStates["Running_Die"] as UserAnimCross).InitTime(0.5f, 0.0f, 1.0f, 0.5f, 1.0f, 0.0f);
        //dicStates["Running_Die"].isRightNow = true;
        //
        //dicStates["Die_Running"] = new UserAnimCross("Die_Running", dicStates["Die"], dicStates["Running"], player);
        //(dicStates["Die_Running"] as UserAnimCross).InitTime(0.5f, 0.0f, 1.0f, 0.5f, 1.0f, 0.0f);
        //dicStates["Die_Running"].isRightNow = true;
        //
        //dicStates["Attacking_Die"] = new UserAnimCross("Attacking_Die", dicStates["Attacking"], dicStates["Die"], player);
        //(dicStates["Attacking_Die"] as UserAnimCross).InitTime(0.05f, 0.0f, 1.0f, 0.5f, 1.0f, 0.0f);
        //dicStates["Attacking_Die"].isRightNow = true;
        //
        //dicStates["Die_Attacking"] = new UserAnimCross("Die_Attacking", dicStates["Die"], dicStates["Attacking"], player);
        //(dicStates["Die_Attacking"] as UserAnimCross).InitTime(0.5f, 0.0f, 1.0f, 0.5f, 1.0f, 0.0f);
        //dicStates["Die_Attacking"].isRightNow = true;


        player.SetDirection(AnimDirection.forward);
        player.cState = dicStates["Idle"];

        {
            GamePlay.UpdateTransform += UpdateActor;

            SceneManager.sceneUnloaded += OnSceneLeft;
        }
    }

    private object lockObject;

    bool bArrowStarted = false;
    public Transform shootTr;

    protected override void ActState_Attack()
    {
        //anim.PlayCross("Attacking");

        Transform _targetTr = GetAttackTr();
        float _aRadius = aRadius;
        if (_targetTr != null)
        {
            float3 forward0 = math.rotate(transform.rotation, new float3(0.0f, 0.0f, 1.0f));
            float3 forward1 = (float3)_targetTr.position - (float3)transform.position;
            float dist = math.length(forward1);

            //float r0 = bRadius;
            //float r1 = targetTr.GetComponent<UnitActor>().bRadius;
            //_aRadius = (r0 + r1);
            //float r = targetTr.GetComponent<UnitActor>().attackRadius;
            if (0.1f < dist && dist <= _aRadius)
            {
                nvAgent.isStopped = true;
                targetPos = transform.position;
                positionTr = null;
                //nvAgent.SetDestination(transform.position);
                forward1 = math.normalize(new float3(forward1.x, 0.0f, forward1.z));
                float cosA = math.dot(forward0, forward1);
                if (cosA < 0.98f)
                {
                    anim.PlayCross("Running");

                    float sinA = math.dot(math.cross(forward0, forward1), new float3(0.0f, 1.0f, 0.0f));

                    float da_dt = 45.0f;
                    if (sinA > 0.0f)
                    {
                        da_dt *= +1.0f;
                    }
                    else
                    {
                        da_dt *= -1.0f;
                    }
                    transform.rotation = math.mul(transform.rotation,
                        quaternion.AxisAngle(new float3(0.0f, 1.0f, 0.0f), math.radians(da_dt * Time.fixedDeltaTime)));
                }
                else
                {
                    anim.PlayCross("Attacking");

                    transform.rotation = quaternion.LookRotation(forward1, new float3(0.0f, 1.0f, 0.0f));

                    UserAnimPlayer player = anim.player;
                    UserAnimState animState = player.cState;
                    float stime = 0.10f;
                    if (animState is UserAnimLoop)
                    {
                        UserAnimLoop animLoop = (animState as UserAnimLoop);
                        if (animLoop.name == "Attacking")
                        {
                            float ut = animLoop.ut;

                            if (math.abs(ut - stime) < 0.05f)
                            {
                                if (bArrowStarted == false)
                                {
                                    ArrowManager.ShootArrow(this, shootTr, _targetTr);
                                    bArrowStarted = true;

                                    //AudioPlay(0);
                                    AudioAttackPlay();
                                }
                            }
                            else
                            {
                                bArrowStarted = false;
                            }
                        }
                    }
                }
            }
            else if (_aRadius < dist)
            {
                anim.PlayCross("Running");
                //targetPos = targetTr.position;
              
                //{
                //    nvAgent.isStopped = false;
                //}
                //
                //if (!nvAgent.isStopped)
                //{
                //    //SetPositionTr(null);
                //    //SetTargetPos(targetTr.position);
                //    nvAgent.SetDestination(_targetTr.position);
                //}

                if (!GamePlay.isNvStop)
                {
                    nvAgent.isStopped = false;
                    nvAgent.SetDestination(_targetTr.position);
                }
                else
                {
                    nvAgent.isStopped = true;
                }
            }
        }
    }


    void Update()
    {
        if (GamePlay.isResume)
        {
            UpdateActor();
        }        

        //if (Input.GetKeyDown(KeyCode.A))
        //{
        //    base.anim.PlayCross("Idle");
        //}
        //else if (Input.GetKeyDown(KeyCode.S))
        //{
        //    base.anim.PlayCross("Running");
        //}
        //else if (Input.GetKeyDown(KeyCode.D))
        //{
        //    base.anim.PlayCross("Attacking");
        //}
    }

    protected override void UpdateActor()
    {
        base.UpdateActor();
    }

    void OnSceneLeft(Scene scene)
    {
        OnDestroy();
    }

    private void OnDestroy()
    {
        SceneManager.sceneUnloaded -= OnSceneLeft;

        GamePlay.UpdateTransform -= UpdateActor;
        GamePlay.PauseAction -= PauseAction;
        GamePlay.ResumeAction -= ResumeAction;

        {
            AudioManager.OnEffectPause -= OnAudioPause;
            AudioManager.OnEffectResume -= OnAudioResume;
        }
    }

    //Audio
    public static float[] volume;
    public static float[] volumeDefault;

    static ArcherActor()
    {
        volumeDefault = new float[2];
        volumeDefault[0] = 1.0f;
        volumeDefault[1] = 0.75f;

        volume = new float[2];
        for (int i = 0; i < 2; i++)
        {
            volume[i] = volumeDefault[i];
        }        
    }

    public override void AudioAttackPlay()
    {
        //if (AudioManager.bAudio && AudioManager.bEffect)
        {
            //audioAttack.volume = volume[0];
            audioAttack.Play();                           
        }
    }

    public override void AudioDiePlay()
    {
        //if (AudioManager.bAudio && AudioManager.bEffect)
        {
            //audioDie.volume = volume[1];
            audioDie.Play();            
        }
    }

}
