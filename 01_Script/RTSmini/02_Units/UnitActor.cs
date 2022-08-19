using System;
using System.Collections;
using System.Collections.Generic;

using Unity.Mathematics;

using UnityEngine;
using UnityEngine.AI;

using UserAnimSpace;

public class UnitActor : MonoBehaviour
{    
    public UserAnimation anim;
    protected UserAnimPlayer player;
    protected Dictionary<string, UserAnimState> dicStates;

    protected string[] stNames;
    protected float4x4[] stM;
    protected Transform[] stTr;
    protected int stCount;
    protected bool hasStMesh;

    //public UnitManager uaMan;

   

    public int offsetIdx
    {
        get; set;
    }
    public int iid;    
    public int unitIdx
    {
        get; set;
    }
    

    protected NavMeshAgent nvAgent;

    public virtual void Init(string[] stNames, float4x4[] stM, bool hasStMesh)
    {
        Init(hasStMesh);

        //this.hasStMesh = hasStMesh;
        if (hasStMesh)
        {
            this.stNames = stNames;
            this.stM = stM;

            this.stCount = transform.childCount;
            stTr = new Transform[stCount];
            for (int i = 0; i < stCount; i++)
            {
                stTr[i] = transform.GetChild(i);
                stTr[i].gameObject.name = stNames[i];
            }
        }

        nvAgent = GetComponent<NavMeshAgent>();
        isActive = true;        
    }

    void Init(bool hasStMesh)
    {
        this.enabled = true;
        anim = GetComponent<UserAnimation>();
        player = anim.player;
        dicStates = anim.dicStates;

        this.hasStMesh = hasStMesh;  
        
        
    }

    public virtual void Begin()
    {
        targetPos = transform.position;

        {
            attackTr = new Transform[3];
            ClearAttackTr();
        }

        {
            float3 sca = transform.localScale;
            float s = math.max(sca.x, sca.z);
            float k = 1.25f;
            bRadius = GetComponent<CapsuleCollider>().radius * s * k;
        }

        {
            GamePlay.PauseAction += PauseAction;
            GamePlay.ResumeAction += ResumeAction;                
        }

        {
            AudioManager.OnEffectPause += OnAudioPause;
            AudioManager.OnEffectResume += OnAudioResume;
        }
    }
    

    public void OnAudioPause()
    {
        //audioAttack.Pause();
        //audioDie.Pause();

        audioAttack.mute = true;
        audioDie.mute = true;
    }

    public void OnAudioResume()
    {
        //audioAttack.UnPause();
        //audioDie.UnPause();
       
        audioAttack.mute = false;
        audioDie.mute = false;
    }

    protected void PauseAction()
    {
        nvAgent.isStopped = true;
        GamePlay.isNvStop = true;

        //audioAttack.Pause();
        //audioDie.Pause();
        //OnAudioPause();
    }

    protected void ResumeAction()
    {
        nvAgent.isStopped = false;
        GamePlay.isNvStop = false;

        //audioAttack.UnPause();
        //audioDie.UnPause();

        //OnAudioResume();

        //if (AudioManager.bEffect)
        //{
        //    OnAudioResume();
        //}      
    }

    
    Transform[] attackTr;

    public Transform positionTr
    {
        get; set;
    }

    public void SetAttackTr(Transform tr, int order)
    {
        if (order < attackTr.Length)
        {
            attackTr[order] = tr;
            
            //{
            //    Debug.LogFormat("SetAttackTr() : {0}", order.ToString());
            //}            
        }
    }


    public Transform GetAttackTr()
    {
        int count = attackTr.Length;
        for (int i = 0; i < count; i++)
        {
            if (attackTr[i] != null)
            {
                if (attackTr[i].GetComponent<UnitActor>().isActive)
                {
                    return attackTr[i];
                }
                else
                {
                    attackTr[i] = null;
                }
            }
        }

        return null;
    }

    public void ClearAttackTr()
    {
        int count = attackTr.Length;
        for (int i = 0; i < count; i++)
        {
            attackTr[i] = null;
        }
    }   

    public Transform targetTr
    {
        get
        {
            int minIdx = minTargetIdx;
            if (minIdx >= 0)
            {
                return GameManager.unitTrs[minIdx];
            }

            return null;
        }
    }

    public int pNum
    {
        get
        {
            return GameManager.playerNum[unitIdx];
        }       
    }

    public float vRadius
    {
        get
        {
            return GameManager.viewRadius[unitIdx];
        }
    }

    public float aRadius
    {
        get
        {
            return GameManager.attackRadius[unitIdx];
        }
    }



    public float bRadius;
    //{
    //    get; set;        
    //}

    public float Hp
    {
        get{ return HpbarManager.hp[offsetIdx + iid];}
        set{ HpbarManager.hp[offsetIdx + iid] = value;}
    }

    public float rHp
    {
        get { return Hp / HpbarManager.maxHp[unitIdx]; }
    }

    public float hitHp
    {
        get { return HpbarManager.hitHp[unitIdx]; }
    }

    public float healHp
    {
        get { return HpbarManager.healHp[unitIdx]; }
    }

    public bool isSelected
    {
        get{ return SelectManager.selectData[offsetIdx + iid] == 1 ? true : false; }
        set{ SelectManager.selectData[offsetIdx + iid] = value ? 1 : 0; }
    }

    public int selectGroup
    {
        get { return SelectManager.selectGroup[offsetIdx + iid]; }
        set { SelectManager.selectGroup[offsetIdx + iid] = value; }
    }

    public bool isActive
    {
        get { return GameManager.activeData[offsetIdx + iid]; }
        set 
        { 
            GameManager.activeData[offsetIdx + iid] = value; 
            if (!value) 
            { 
                isSelected = value;
                //selectGroup = -1;
            }             
        }
    }

    public float3 targetPos
    {
        get { return TargetAction.targetPos[offsetIdx + iid]; }
        set 
        {
            int idx = offsetIdx + iid;
            float3 tpos = new float3(value.x, 0.0f, value.z);
            TargetAction.targetPos[idx] = tpos;
            TargetAction.refTargetPos[idx] = tpos;

            //TargetAction.targetPos[idx] = value;
            //TargetAction.refTargetPos[idx] = value;
        }
    }

    public int minTargetIdx
    {
        get
        {
            float4 minDist = TargetAction.minDist[offsetIdx + iid];
            int idx = -1;
            
            if(minDist.z > 0.0f)
            {
                idx = (int)minDist.x;
                return idx;
            }

            _minTargetIdx = idx;
            return idx;
        }
    }

    public int _minTargetIdx;

    public void DamageHp(float dHp)
    {
        float hp = Hp - dHp;
        if (hp < 0.0f)
        {
            hp = 0.0f;
        }

        Hp = hp;
    }

    float dz = 0.1f;
    float dy = 0.1f;
    float dx = 0.1f;
   
    float k = 0.1f;

    float rsRate;

    void BehaveRoom()
    {
        if(Input.GetKeyDown(KeyCode.J))
        {
            anim.PlayCross("Idle");
        }
        else if (Input.GetKeyDown(KeyCode.K))
        {
            anim.PlayCross("Running");
        }
        else if (Input.GetKeyDown(KeyCode.L))
        {
            anim.PlayCross("Attacking");
        }

    }

    void Behave()
    {
        {
            Test_ActionState();
            switch (state)
            {
                case ActionState.Idle:
                    ActState_Idle();
                    break;
                case ActionState.Walk:
                    ActState_Walk();
                    break;
                case ActionState.Attack:
                    ActState_Attack();
                    break;
                case ActionState.Die:
                    ActState_Die();
                    break;
                case ActionState.Sleep:
                    ActState_Sleep();
                    break;
            }
        }
    }

    void Test_ActionState()
    {
        float3 pos = transform.position;
        attackTr[1] = targetTr;

        bool active = isActive;
        float _rHp = rHp;

        //if (active && rHp == 0.0f && GameTimer.isRunning)
        //if (active && _rHp == 0.0f)
        if (active && _rHp <= 0.0f)
        {
            isActive = false;
            SetState_Die();
            GameManager.killedCount[pNum]++;
            //UnitData.killNum[pId]++;
        }
        else if (!active && _rHp > 0.99f)
        {
            isActive = true;
            SetState_Idle();
        }
        else if (active)
        {
            if ((Input.GetKeyDown(SelectManager.key_hold) && isSelected))
            {
                SetState_Idle();
            }
            else
            {
                if (GetAttackTr() != null && positionTr == null)
                {
                    SetState_Attack();
                }
                else
                {
                    if (math.distance(targetPos.xz, pos.xz) < 1.0f)
                    {
                        SetState_Idle();
                    }
                    else
                    {
                        SetState_Walk();
                    }
                }
            }
        }
        else
        {
            SetState_Sleep();
        }
    }

    public ActionState state;

    public void SetState_Idle()
    {
        state = ActionState.Idle;
    }

    public void SetState_Walk()
    {
        state = ActionState.Walk;
    }

    public void SetState_Attack()
    {
        state = ActionState.Attack;
    }

    public void SetState_Die()
    {
        state = ActionState.Die;
    }

    public void SetState_Sleep()
    {
        state = ActionState.Sleep;
    }

    protected virtual void ActState_Idle()
    {
        float3 pos = transform.position;
        anim.PlayCross("Idle");
        nvAgent.isStopped = true;        
        targetPos = pos;       
        positionTr = null;
        //nvAgent.SetDestination(pos);
    }

    protected virtual void ActState_Walk()
    {
        anim.PlayCross("Running");

        //nvAgent.isStopped = false;
        //if (!nvAgent.isStopped)
        //{
        //    nvAgent.SetDestination(targetPos);
        //}
        
        if(!GamePlay.isNvStop)
        {
            nvAgent.isStopped = false;
            nvAgent.SetDestination(targetPos);
        }
        else
        {
            nvAgent.isStopped = true;
        }
    }

    public float _da_dt = 45.0f;

    protected virtual void ActState_Attack()
    {
        //anim.PlayCross("Attacking");

        Transform _targetTr = GetAttackTr();
        float _aRadius = aRadius;
        if (_targetTr != null)
        {           
            float3 forward0 = math.rotate(transform.rotation, new float3(0.0f, 0.0f, 1.0f));
            float3 forward1 = (float3)_targetTr.position - (float3)transform.position;
            float dist = math.length(forward1);

            float r0 = bRadius;
            //float r1 = targetTr.GetComponent<UnitActor>().bRadius;
            float r1 = 0.0f;
            if(targetTr != null)
            {
                r1 = targetTr.GetComponent<UnitActor>().bRadius;
            }

            _aRadius = (r0 + r1);
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

                    float da_dt = _da_dt * 50.0f * Time.deltaTime;
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
                }
            }
            else if (_aRadius < dist)
            {
                anim.PlayCross("Running");
                            
                //{
                //    nvAgent.isStopped = false;
                //}
                //
                //if (!nvAgent.isStopped)
                //{                   
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

    protected virtual void ActState_Die()
    {
        float3 pos = transform.position;
        anim.PlayCross("Idle");
        nvAgent.isStopped = true;       
        targetPos = pos;
        positionTr = null;
        //nvAgent.SetDestination(pos);

        if (GameTimer.isRunning)
        {
            //AudioPlay(1, 0.5f);
            //AudioDiePlay(0.25f);
            AudioDiePlay();
            //Debug.Log("Die sound");
        }
    }

    protected virtual void ActState_Sleep()
    {
        float hp = Hp;
        ////hp += healHp;
        hp += (Time.deltaTime * 10.0f * healHp);
        Hp = hp;


        float3 pos = transform.position;
        anim.PlayCross("Idle");
        nvAgent.isStopped = true;
        targetPos = pos;
        positionTr = null;
        //nvAgent.SetDestination(pos);
    }

    protected void UpdateActor0()
    {
        if (hasStMesh)
        {
            for (int i = 0; i < stCount; i++)
            {
                float4x4 M = stM[i];

                stTr[i].position = M.c0.xyz;
                stTr[i].rotation = new quaternion(M.c1);
                stTr[i].localScale = M.c2.xyz;
            }
        }

        
        {
            if (Input.GetMouseButton(0))
            {
                if (Input.GetKey(KeyCode.D))
                {
                    transform.rotation *= quaternion.AxisAngle(new float3(+1.0f, 0.0f, 0.0f), +dx * k);
                }

                //if (Input.GetKey(KeyCode.A))
                //{
                //    transform.rotation *= quaternion.AxisAngle(new float3(+1.0f, 0.0f, 0.0f), -dx * k);
                //}

                if (Input.GetKey(KeyCode.W))
                {
                    transform.rotation *= quaternion.AxisAngle(new float3(0.0f, +1.0f, +0.0f), +dy * k);
                }

                if (Input.GetKey(KeyCode.S))
                {
                    transform.rotation *= quaternion.AxisAngle(new float3(0.0f, 0.0f, +1.0f), +dz * k);
                }
            }
            else
            {
                if (Input.GetKey(KeyCode.D))
                {
                    Vector3 dr = new Vector3(dx, 0.0f, 0.0f);
                    transform.position += dr;
                }

                if (Input.GetKey(KeyCode.A))
                {
                    Vector3 dr = new Vector3(dx, 0.0f, 0.0f);
                    transform.position -= dr;
                }

                if (Input.GetKey(KeyCode.W))
                {
                    Vector3 dr = new Vector3(0.0f, 0.0f, dz);
                    transform.position += dr;
                }

                if (Input.GetKey(KeyCode.S))
                {
                    Vector3 dr = new Vector3(0.0f, 0.0f, dz);
                    transform.position -= dr;
                }
            }

            {
                if (Input.GetKeyDown(KeyCode.Z))
                {
                    anim.PlayCross("Idle");
                }
                else if (Input.GetKeyDown(KeyCode.X))
                {
                    anim.PlayCross("Running");
                }
                else if (Input.GetKeyDown(KeyCode.C))
                {
                    anim.PlayCross("Attacking");
                }                
            }
        }

        //if(Input.GetKeyDown(KeyCode.Space))
        //{
        //    targetPos = SelectManager.movePos;
        //
        //    nvAgent.SetDestination(SelectManager.movePos);
        //}

        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    nvAgent.SetDestination(targetPos);
        //}

        if(nvAgent.enabled)
        {
            if (math.distance((float3)transform.position, targetPos) < 1.0f)  //1.5f
            {
                targetPos = transform.position;
                nvAgent.SetDestination(transform.position);
                nvAgent.isStopped = true;
            }
            else
            {
                nvAgent.isStopped = false;
                nvAgent.SetDestination(targetPos);
            }
        
            //nvAgent.SetDestination(transform.position);
        }
       
    }

    protected virtual void UpdateActor()
    {
        if (hasStMesh)
        {
            for (int i = 0; i < stCount; i++)
            {
                float4x4 M = stM[i];

                stTr[i].position = M.c0.xyz;
                stTr[i].rotation = new quaternion(M.c1);
                stTr[i].localScale = M.c2.xyz;
            }
        }        

        if(GameManager.inRoom)
        {
            BehaveRoom();
        }
        else
        {
            Behave();
        }
       
        //Debug.Log("nvagent.isStopped : " + nvAgent.isStopped.ToString());
    }


    public void OnTriggerEnter(Collider other)
    {
        //Debug.LogFormat("{0} to {1}", LayerMask.LayerToName(gameObject.layer), LayerMask.LayerToName(other.gameObject.layer));       

        //Debug.LogFormat("{0} from {1}", transform.root.name, other.transform.root.name);
        //Debug.Log("OnTriggerEnter()");

        if (isActive)
        {
            HitActor hitActor = other.GetComponent<HitActor>();
            if (hitActor != null)
            {
                UnitActor actor = hitActor.unitActor;
                if (actor.isActive && actor.state == ActionState.Attack)
                {                   
                    {
                        //DamageHp(hitHp);
                        DamageHp(actor.hitHp);
                        actor.AudioAttackPlay();
                    }
                }
            }
        }
    }


    float3 p0;
    float3 p1;
    quaternion r0;
    quaternion r1;


    //[PunRPC]
    //public virtual void SetIId(int i)
    //{
    //    iid = i;
    //}
    public AudioSource audioAttack;
    public AudioSource audioDie;

    public virtual void AudioAttackPlay()
    {
        //if (AudioMan.bAudio && AudioMan.bEffect)
        //{
        //    //audioDie.volume = volume;
        //    audioAttack.Play();
        //}
    }

    public virtual void AudioDiePlay()
    {
        //if (AudioMan.bAudio && AudioMan.bEffect)
        //{            
        //    audioDie.Play();
        //}
    }


    [Serializable]
    public enum ActionState
    {
        Idle, Walk, Attack, Die, Sleep
    }

}
