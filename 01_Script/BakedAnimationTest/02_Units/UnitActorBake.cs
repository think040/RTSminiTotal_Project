using System;
using System.Collections;
using System.Collections.Generic;

using Unity.Mathematics;

using UnityEngine;
using UnityEngine.AI;

using UserAnimSpace;

public class UnitActorBake : MonoBehaviour
{
    public UserAnimation anim;
    protected UserAnimPlayer player;
    protected Dictionary<string, UserAnimState> dicStates;

    protected string[] stNames;
    protected float4x4[] stM;
    protected Transform[] stTr;
    protected int stCount;
    protected bool hasStMesh;

    // Start is called before the first frame update
    void Start()
    {
        //Init();
    }

    public virtual void Init(string[] stNames, float4x4[] stM, bool hasStMesh)
    {
        //Init(hasStMesh);
        
        this.hasStMesh = hasStMesh;
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

        Init();       
    }

    public void Init()
    {
        this.enabled = true;
        anim = GetComponent<UserAnimation>();
        player = anim.player;
        dicStates = anim.dicStates;


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


        player.SetDirection(AnimDirection.forward);
        //player.SetDirection(AnimDirection.backward);
        player.cState = dicStates["Idle"];
    }

    // Update is called once per frame
    void Update()
    {
        UpdateActor();
        TestAnim();
    }

    void TestAnim()
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

        if (Input.GetKeyDown(KeyCode.U))
        {
            anim.PlayLoop("Idle");
        }
        else if (Input.GetKeyDown(KeyCode.I))
        {
            anim.PlayLoop("Running");
        }
        else if (Input.GetKeyDown(KeyCode.O))
        {
            anim.PlayLoop("Attacking");
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            player.SetDirection(AnimDirection.forward);
        }
        else if(Input.GetKeyDown(KeyCode.M))
        {
            player.SetDirection(AnimDirection.backward);
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
    }

}
