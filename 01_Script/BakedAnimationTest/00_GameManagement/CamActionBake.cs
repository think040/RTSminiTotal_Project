using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using Unity.Mathematics;

using UnityEngine;



public class CamActionBake : MonoBehaviour
{
    public GameObject orbitCenter;
    public static KeyCode key_orbit = KeyCode.Z;    //KeyCode.V     //KeyCode.LeftShift
    public static KeyCode key_spin = KeyCode.C;       //KeyCode.B     //KeyCode.LeftAlt
    public static int delta = 50;

    public float3 minBound = new float3(-45.0f, +2.0f, -35.0f);
    public float3 maxBound = new float3(+45.0f, +50.0f, +35.0f);
    public float minY = 2.0f;
    public float maxR = 100.0f;


    CamAction.CamMove camMove;
    // Start is called before the first frame update
    void Start()
    {
        StartPlay();
    }
   

    void StartPlay()
    {
        //int delta = 100;
        float theta0 = 37.5f;
        float theta1 = 85.0f;      

        {
            camMove = new CamAction.CamMove();

            StartCoroutine(camMove.ZoomInOut(transform, 2.0f));
            StartCoroutine(camMove.MoveParallel_XZ(transform, 0.025f, delta));

            //StartCoroutine(
            //    camMove.RotOrbitRandomFixed_range(
            //        transform, "Plane", "Plane", orbitCenter, theta0, theta1, key_orbit));

            StartCoroutine(
                camMove.RotSpin_range(
                    transform, theta0, theta1, key_spin));
        }

    }    

    // Update is called once per frame
    void Update()
    {
        
    }
}