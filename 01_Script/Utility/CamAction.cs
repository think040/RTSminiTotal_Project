using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using Unity.Mathematics;

using UnityEngine;

using Utility_JSB;

public class CamAction : MonoBehaviour
{
    public GameObject orbitCenter;
    public static KeyCode key_orbit = KeyCode.Z;    //KeyCode.V     //KeyCode.LeftShift
    public static KeyCode key_spin = KeyCode.C;       //KeyCode.B     //KeyCode.LeftAlt
    public static int delta = 25;  //50

    public float3 minBound = new float3(-45.0f, +2.0f, -35.0f);
    public float3 maxBound = new float3(+45.0f, +50.0f, +35.0f);
    public float minY = 2.0f;
    public float maxR = 100.0f;


    CamMove camMove;
    // Start is called before the first frame update
    void Start()
    {        
        if(GameManager.inRoom)
        {
            StartRoom();
        }
        else
        {
            StartPlay();
        }

        //StartCoroutine(CameraMove.ZoomInOut(transform, 2.0f));
        //StartCoroutine(CameraMove.MoveParallel_XZ(transform, 0.025f, 100));
        ////StartCoroutine(CameraMove.RotSpin(transform, 0));
        ////StartCoroutine(
        ////    CameraMove.RotOrbitRandomFixed(
        ////        transform, "Plane", "Plane", orbitCenter));
        //
        //StartCoroutine(CameraMove.RotSpin(transform, 0, key_spin));
        //StartCoroutine(
        //    CameraMove.RotOrbitRandomFixed(
        //        transform, "Plane", "Plane", orbitCenter, key_orbit));
    }

    void StartRoom()
    {
        int delta = 0;
        float theta0 = 37.5f;
        float theta1 = 85.0f;
        float3 center = new float3(0.0f, 0.75f, 0.0f);
        float3 rc = new float3(0.35f, 0.5f, 0.0f);
        float r0 = 2.0f;
        float r1 = 8.0f;

        //transform.rotation = quaternion.LookRotation(
        //    math.normalize(center - (float3)transform.position), 
        //    new float3(0.0f, 1.0f, 0.0f));

        //StartCoroutine(CameraMove.ZoomInOut_range(transform, 2.0f, r0, r1, center));

        //{
        //    StartCoroutine(
        //        CameraMove.RotOrbitConstCenterFixed_range(
        //            transform, center, delta, theta0, theta1, key_orbit));
        //
        //    StartCoroutine(CameraMove.ZoomInOut_NonCenter_range(transform, center, 2.0f, r0, r1));
        //    
        //    StartCoroutine(
        //        CameraMove.RotOrbitConstNonCenterFixed_range(
        //            transform, center, theta0, theta1));
        //}

        {
            camMove = new CamMove();

            StartCoroutine(camMove.ZoomInOut_NonCenter_range(transform, center, 0.5f, r0, r1));

            StartCoroutine(
                camMove.RotOrbitConstNonCenterFixed_range(
                    transform, center, theta0, theta1));
        }
        
    }
    
    public static float pacePlane
    { get; set; } = 100.0f;
    
    public static float paceZoom
    { get; set; } = 3.0f;

    public static bool useMouseForMove
    { get; set; } = true;

    public bool _useMouseForMove = true;

    void StartPlay()
    {
        //int delta = 100;
        float theta0 = 37.5f;
        float theta1 = 85.0f;

        //{
        //    StartCoroutine(CameraMove.ZoomInOut(transform, 2.0f));
        //    StartCoroutine(CameraMove.MoveParallel_XZ(transform, 0.025f, delta));
        //    //StartCoroutine(CameraMove.RotSpin(transform, 0));
        //    //StartCoroutine(
        //    //    CameraMove.RotOrbitRandomFixed(
        //    //        transform, "Plane", "Plane", orbitCenter));
        //
        //    //StartCoroutine(CameraMove.RotSpin(transform, 0, key_spin));
        //    //StartCoroutine(
        //    //    CameraMove.RotOrbitRandomFixed(
        //    //        transform, "Plane", "Plane", orbitCenter, key_orbit));
        //
        //    StartCoroutine(
        //        CameraMove.RotOrbitRandomFixed_range(
        //            transform, "Plane", "Plane", orbitCenter, theta0, theta1, key_orbit));
        //
        //    StartCoroutine(
        //        CameraMove.RotSpin_range(
        //            transform, theta0, theta1, key_spin));
        //}

        {
            camMove = new CamMove();

            //StartCoroutine(camMove.ZoomInOut(transform, 2.0f));
            //StartCoroutine(camMove.MoveParallel_XZ(transform, 0.025f, delta));

            StartCoroutine(camMove.ZoomInOut(transform));
            StartCoroutine(camMove.MoveParallel_XZ(transform, delta));

            StartCoroutine(
                camMove.RotOrbitRandomFixed_range(
                    transform, "Plane", "Plane", orbitCenter, theta0, theta1, key_orbit));

            StartCoroutine(
                camMove.RotSpin_range(
                    transform, theta0, theta1, key_spin));

            if(GameManager.useEditor)
            {
                useMouseForMove = _useMouseForMove;
            }
        }
                
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void CheckMoveBoundary0()
    {
        {
            float3 pos = transform.position;
            float3 min;
            float3 max;

            min = minBound;
            max = maxBound;

            bool3 bmin = pos < min;
            bool3 bmax = max < pos;

            if (bmin.x)
            {
                pos.x = min.x;
            }

            if (bmin.y)
            {
                pos.y = min.y;
            }

            if (bmin.z)
            {
                pos.z = min.z;
            }

            if (bmax.x)
            {
                pos.x = max.x;
            }

            if (bmax.y)
            {
                pos.y = max.y;
            }

            if (bmax.z)
            {
                pos.z = max.z;
            }

            transform.position = pos;
        }

        //{
        //    float radius = 30.0f;
        //    float3 dir = (float3)transform.position - float3.zero;
        //    float dis = math.length(dir);
        //    if (dis > radius && dis > 1.0f)
        //    {
        //        dir = math.normalize(dir);
        //
        //        transform.position = radius * dir;
        //    }
        //}
    }

    void CheckMoveBoundary()
    {
        {
            float3 pos = transform.position;
            //float minY = 2.0f;
            //float maxR = 100.0f;
           
            float dist = math.distance(pos, float3.zero);

            if(dist > maxR)
            {
                float3 dir = math.normalize(-pos);
                pos += 1.0f * dir;
            }
            
            if(pos.y < minY)
            {
                pos.y += 0.5f;
            }

            transform.position = pos;
        }     
    }

    // Update is called once per frame
    void Update()
    {
        if(!GameManager.inRoom)
        {
            CheckMoveBoundary();
        }

        //if (!GameManager.inRoom)
        //{
        //    if (Input.GetKeyDown(KeyCode.Y))
        //    {
        //        camMove.pacePlane *= 2.0f;
        //    }
        //    else
        //    if (Input.GetKeyDown(KeyCode.U))
        //    {
        //        camMove.pacePlane *= 0.5f;
        //    }
        //
        //    if (Input.GetKeyDown(KeyCode.I))
        //    {
        //        camMove.paceZoom *= 2.0f;
        //    }
        //    else
        //    if (Input.GetKeyDown(KeyCode.O))
        //    {
        //        camMove.paceZoom *= 0.5f;
        //    }
        //}

        //if (!GameManager.inRoom)
        //{
        //    camMove.pacePlane = CamAction.pacePlane;
        //    camMove.paceZoom = CamAction.paceZoom;
        //}



        //{
        //    if (Input.GetKeyDown(KeyCode.Alpha6))
        //    {
        //        if (camMove.activeZoom)
        //        {
        //            camMove.activeZoom = false;
        //        }
        //        else
        //        {
        //            camMove.activeZoom = true;
        //        }
        //    }
        //
        //    if (Input.GetKeyDown(KeyCode.Alpha7))
        //    {
        //        if (camMove.activeMoveParallel)
        //        {
        //            camMove.activeMoveParallel = false;
        //        }
        //        else
        //        {
        //            camMove.activeMoveParallel = true;
        //        }
        //    }
        //
        //    if (Input.GetKeyDown(KeyCode.Alpha8))
        //    {
        //        if (camMove.activeRotOrbit)
        //        {
        //            camMove.activeRotOrbit = false;
        //        }
        //        else
        //        {
        //            camMove.activeRotOrbit = true;
        //        }
        //    }
        //
        //    if (Input.GetKeyDown(KeyCode.Alpha9))
        //    {
        //        if (camMove.activeRotSpin)
        //        {
        //            camMove.activeRotSpin = false;
        //        }
        //        else
        //        {
        //            camMove.activeRotSpin = true;
        //        }
        //    }
        //}


    }


    public class CamMove
    {        
        public bool activeZoom
        { get; set; } = true;
        
        public bool activeMoveParallel
        { get; set; } = true;
        
        public bool activeRotOrbit
        { get; set; } = true;

        public bool activeRotSpin
        { get; set; } = true;

        //public float paceZoom
        //{ get; set; } = 1.0f;
        //
        //public float pacePlane
        //{ get; set; } = 1.0f;


        //Play
        public IEnumerator ZoomInOut(Transform camTrans, float pace)
        {
            while (true)
            {
                if(activeZoom)
                {
                    float delta = Input.GetAxis("Mouse ScrollWheel") * 400.0f * pace * Time.deltaTime;

                    float3 zaxis = math.rotate(camTrans.rotation, new float3(0.0f, 0.0f, 1.0f));
                    float3 pos = camTrans.position;
                    pos = pos + zaxis * delta;

                    camTrans.position = pos;
                }                

                yield return null;               
            }
        }

        public IEnumerator ZoomInOut(Transform camTrans)
        {
            while (true)
            {
                if (activeZoom)
                {
                    float delta = Input.GetAxis("Mouse ScrollWheel") * 400.0f * (2.0f * CamAction.paceZoom) * Time.deltaTime;

                    float3 zaxis = math.rotate(camTrans.rotation, new float3(0.0f, 0.0f, 1.0f));
                    float3 pos = camTrans.position;
                    pos = pos + zaxis * delta;

                    camTrans.position = pos;
                }

                yield return null;
            }
        }

        public IEnumerator MoveParallel_XZ(Transform camTrans, float pace, int delta = 20)
        {
            Camera cam = camTrans.gameObject.GetComponent<Camera>();

            while (true)
            {
                if(activeMoveParallel)
                {
                    Rect rect = cam.pixelRect;
                    
                    Vector3 posf = Input.mousePosition;
                    Vector3Int pos = new Vector3Int((int)posf.x, (int)posf.y, (int)posf.z);

                    int xmin = (int)(rect.min.x);
                    int xmax = (int)(rect.max.x);
                    int ymin = (int)(rect.min.y);
                    int ymax = (int)(rect.max.y);

                    Vector3 camPos = Vector3.zero;
                    float camDelta = 0.05f;

                    //float k = 0.025f;
                    float y0 = 0.1f;
                    camDelta = y0 + pace * camTrans.position.y * Time.deltaTime;

                    bool RKey = false;
                    bool Lkey = false;
                    bool UKey = false;
                    bool DKey = false;

                    //{
                    //    if (Input.GetKey(KeyCode.W))
                    //    {
                    //        UKey = true;
                    //    }
                    //
                    //    if (Input.GetKey(KeyCode.A))
                    //    {
                    //        Lkey = true;
                    //    }
                    //
                    //    if (Input.GetKey(KeyCode.S))
                    //    {
                    //        DKey = true;
                    //    }
                    //
                    //    if (Input.GetKey(KeyCode.D))
                    //    {
                    //        RKey = true;
                    //    }
                    //
                    //    if (((xmin <= pos.x && pos.x <= xmin + delta)
                    //        && (ymin <= pos.y && pos.y <= ymin + delta))
                    //        || (Lkey && !RKey && !UKey && DKey))
                    //    {
                    //        //1
                    //        camPos.x -= camDelta;
                    //        camPos.y -= camDelta;
                    //    }
                    //    else if (((xmin + delta < pos.x && pos.x < xmax - delta)
                    //        && (ymin <= pos.y && pos.y <= ymin + delta))
                    //        || (!Lkey && !RKey && !UKey && DKey))
                    //    {
                    //        //2
                    //        camPos.y -= camDelta;
                    //    }
                    //    else if (((xmax - delta <= pos.x && pos.x <= xmax)
                    //        && (ymin <= pos.y && pos.y <= ymin + delta))
                    //        || (!Lkey && RKey && !UKey && DKey))
                    //    {
                    //        //3
                    //        camPos.x += camDelta;
                    //        camPos.y -= camDelta;
                    //    }
                    //    else if (((xmin <= pos.x && pos.x <= xmin + delta)
                    //         && (ymin + delta < pos.y && pos.y < ymax - delta))
                    //         || (Lkey && !RKey && !UKey && !DKey))
                    //    {
                    //        //4
                    //        camPos.x -= camDelta;
                    //    }
                    //    //else if ((xmin + delta < pos.x && pos.x < xmax - delta)
                    //    //     && (ymin + delta < pos.y && pos.y < ymax - delta)
                    //    //     )
                    //    //{
                    //    //    //5
                    //    //}
                    //    else if (((xmax - delta <= pos.x && pos.x <= xmax)
                    //        && (ymin + delta < pos.y && pos.y < ymax - delta))
                    //        || (!Lkey && RKey && !UKey && !DKey))
                    //    {
                    //        //6
                    //        camPos.x += camDelta;
                    //    }
                    //    else if (((xmin <= pos.x && pos.x <= xmin + delta)
                    //        && (ymax - delta <= pos.y && pos.y <= ymax))
                    //        || (Lkey && !RKey && UKey && !DKey))
                    //    {
                    //        //7
                    //        camPos.x -= camDelta;
                    //        camPos.y += camDelta;
                    //    }
                    //    else if (((xmin + delta < pos.x && pos.x < xmax - delta)
                    //         && (ymax - delta <= pos.y && pos.y <= ymax))
                    //         || (!Lkey && !RKey && UKey && !DKey))
                    //    {
                    //        //8                   
                    //        camPos.y += camDelta;
                    //    }
                    //    else if (((xmax - delta <= pos.x && pos.x <= xmax)
                    //        && (ymax - delta <= pos.y && pos.y <= ymax))
                    //        || (!Lkey && RKey && UKey && !DKey))
                    //    {
                    //        //9
                    //        camPos.x += camDelta;
                    //        camPos.y += camDelta;
                    //    }
                    //
                    //    //Vector3 localPos =
                    //    //    (camTrans.rotation * Vector3.right).normalized * camPos.x +
                    //    //    (camTrans.rotation * Vector3.up).normalized * camPos.y;
                    //    //camTrans.position = camTrans.position + localPos;
                    //}

                    {                       
                        if (Input.GetKey(KeyCode.W) || (ymax - delta <= pos.y && pos.y <= ymax))
                        {
                            UKey = true;
                            camPos.y += camDelta;
                        }

                        if (Input.GetKey(KeyCode.A) || (xmin <= pos.x && pos.x <= xmin + delta))
                        {
                            Lkey = true;
                            camPos.x -= camDelta;
                        }

                        if (Input.GetKey(KeyCode.S) || (ymin <= pos.y && pos.y <= ymin + delta))
                        {
                            DKey = true;
                            camPos.y -= camDelta;
                        }

                        if (Input.GetKey(KeyCode.D) || (xmax - delta <= pos.x && pos.x <= xmax))
                        {
                            RKey = true;
                            camPos.x += camDelta;
                        }
                    }

                    

                    float3 xaxis = math.rotate(camTrans.rotation, new float3(1.0f, 0.0f, 0.0f));
                    float3 yaxis = new float3(0.0f, 1.0f, 0.0f);
                    float3 zaxis = math.cross(xaxis, yaxis);

                    float3 localPos = xaxis * camPos.x + zaxis * camPos.y;
                    camTrans.position += (Vector3)localPos;

                    //camTrans.localPosition = camPos;
                }

                yield return null;
            }
        }

        public IEnumerator MoveParallel_XZ(Transform camTrans, int delta = 20)
        {
            Camera cam = camTrans.gameObject.GetComponent<Camera>();

            while (true)
            {
                if (activeMoveParallel)
                {
                    Rect rect = cam.pixelRect;

                    Vector3 posf = Input.mousePosition;
                    Vector3Int pos = new Vector3Int((int)posf.x, (int)posf.y, (int)posf.z);

                    int xmin = (int)(rect.min.x);
                    int xmax = (int)(rect.max.x);
                    int ymin = (int)(rect.min.y);
                    int ymax = (int)(rect.max.y);

                    Vector3 camPos = Vector3.zero;
                    float camDelta = 0.05f;

                    //float k = 0.025f;
                    float y0 = 0.1f;
                    camDelta = y0 + (0.025f * CamAction.pacePlane) * camTrans.position.y * Time.deltaTime;

                    bool RKey = false;
                    bool Lkey = false;
                    bool UKey = false;
                    bool DKey = false;
                 
                    if(CamAction.useMouseForMove)  
                    {
                        if ((Input.GetKey(CamAction.key_orbit) || Input.GetKey(CamAction.key_spin)) && Input.GetMouseButton(1))
                        {

                        }
                        else
                        {
                            if (Input.GetKey(KeyCode.W) || (ymax - delta <= pos.y && pos.y <= ymax))
                            {
                                UKey = true;
                                camPos.y += camDelta;
                            }

                            if (Input.GetKey(KeyCode.A) || (xmin <= pos.x && pos.x <= xmin + delta))
                            {
                                Lkey = true;
                                camPos.x -= camDelta;
                            }

                            if (Input.GetKey(KeyCode.S) || (ymin <= pos.y && pos.y <= ymin + delta))
                            {
                                DKey = true;
                                camPos.y -= camDelta;
                            }

                            if (Input.GetKey(KeyCode.D) || (xmax - delta <= pos.x && pos.x <= xmax))
                            {
                                RKey = true;
                                camPos.x += camDelta;
                            }
                        }                       
                    }
                    else
                    {
                        if (Input.GetKey(KeyCode.W))
                        {
                            UKey = true;
                            camPos.y += camDelta;
                        }

                        if (Input.GetKey(KeyCode.A))
                        {
                            Lkey = true;
                            camPos.x -= camDelta;
                        }

                        if (Input.GetKey(KeyCode.S))
                        {
                            DKey = true;
                            camPos.y -= camDelta;
                        }

                        if (Input.GetKey(KeyCode.D))
                        {
                            RKey = true;
                            camPos.x += camDelta;
                        }
                    }



                    float3 xaxis = math.rotate(camTrans.rotation, new float3(1.0f, 0.0f, 0.0f));
                    float3 yaxis = new float3(0.0f, 1.0f, 0.0f);
                    float3 zaxis = math.cross(xaxis, yaxis);

                    float3 localPos = xaxis * camPos.x + zaxis * camPos.y;
                    camTrans.position += (Vector3)localPos;

                    //camTrans.localPosition = camPos;
                }

                yield return null;
            }
        }

        public IEnumerator RotOrbitRandomFixed_range(
            Transform camTrans, string planeTag, string planeLayer, GameObject centerObject,
            float theta0 = 45.0f, float theta1 = 85.0f, KeyCode key = KeyCode.LeftShift)
        {
            Camera cam = camTrans.gameObject.GetComponent<Camera>();

            float3 prePos = float3.zero;
            float3 curPos = float3.zero;

            float3 angle = float3.zero;

            bool down = false;

            Ray ray = new Ray();
            RaycastHit[] hits;

            float3 center = float3.zero;
            float radius = 1.0f;
            float3 centerNormal = float3.zero;

            quaternion preRot = quaternion.identity;

            float3 rayDirInView = float3.zero;
            float3 rayDirInWorld = float3.zero;

            quaternion q = quaternion.identity;

            float cosp = 0.0f;
            float sinp = 0.0f;
            float cost = 0.0f;
            float sint = 0.0f;

            float prePhi = 0.0f;
            float preTheta = 0.0f;

            float3x3 m = float3x3.identity;
            float3 xaxis = float3.zero;
            float3 yaxis = float3.zero;
            float3 zaxis = float3.zero;


            while (true)
            {
                if(activeRotOrbit)
                {
                    if (Input.GetMouseButtonDown(1) && Input.GetKey(key))
                    {
                        prePos = Input.mousePosition;
                        preRot = camTrans.rotation;

                        ray = cam.ScreenPointToRay(prePos);

                        float3x3 mat = new float3x3(camTrans.rotation);
                        float3 rayDir = ray.direction.normalized;
                        rayDirInView = math.mul(math.transpose(mat), rayDir);

                        hits = Physics.RaycastAll(ray, 100.0f, LayerMask.GetMask(planeLayer));
                        for (int i = 0; i < hits.GetLength(0); i++)
                        {
                            if (hits[i].collider != null)
                            {
                                if (hits[i].collider.gameObject != null)
                                {
                                    if (hits[i].collider.gameObject.tag == planeTag)
                                    {
                                        center = hits[i].point;
                                        centerNormal = hits[i].normal;
                                        radius = math.distance(center, camTrans.position);
                                        down = true;

                                        {
                                            m = new float3x3(preRot);
                                            xaxis = m.c0;
                                            yaxis = m.c1;
                                            zaxis = m.c2;

                                            {
                                                cosp = math.dot(new float3(1.0f, 0.0f, 0.0f), xaxis);
                                                sinp = math.dot(new float3(0.0f, 1.0f, 0.0f), math.cross(new float3(1.0f, 0.0f, 0.0f), xaxis));
                                                prePhi = math.acos(cosp);
                                                if (sinp < 0)
                                                {
                                                    prePhi *= (-1.0f);
                                                }

                                            }

                                            {
                                                cost = math.dot(new float3(0.0f, 1.0f, 0.0f), yaxis);
                                                preTheta = math.acos(cost);
                                            }
                                        }
                                        break;
                                    }
                                }
                            }
                        }

                        centerObject.SetActive(true);
                        centerObject.transform.position = center;
                    }


                    if (down)
                    {
                        curPos = Input.mousePosition;
                        //angle = (curPos - prePos) * 0.1f;
                        angle = (curPos - prePos) * 0.001f;

                        {
                            float phi = 0.0f;
                            phi = prePhi - angle.x;

                            float theta = 0.0f;
                            theta = math.clamp(preTheta + angle.y, math.radians(theta0), math.radians(theta1));

                            q = math.mul(
                                quaternion.AxisAngle(new float3(0.0f, 1.0f, 0.0f), phi),
                                quaternion.AxisAngle(new float3(1.0f, 0.0f, 0.0f), theta));
                            camTrans.rotation = q;
                        }

                        {
                            float3x3 mat = new float3x3(q);
                            rayDirInWorld = math.mul(mat, rayDirInView);

                            camTrans.position = center + radius * (-rayDirInWorld);
                        }
                    }

                    if (Input.GetMouseButtonUp(1) || !Input.GetKey(key))
                    {
                        down = false;
                        centerObject.SetActive(false);
                    }
                }
               
                yield return null;

                if (!activeRotOrbit)
                {
                    down = false;
                }

            }


        }        

        public IEnumerator RotSpin_range(
            Transform camTrans,
            float theta0 = 45.0f, float theta1 = 85.0f, KeyCode key = KeyCode.LeftAlt)
        {
            Camera cam = camTrans.gameObject.GetComponent<Camera>();

            float3 prePos = float3.zero;
            float3 curPos = float3.zero;

            float3 angle = float3.zero;

            bool down = false;

            quaternion q = quaternion.identity;
            quaternion preRot = quaternion.identity;

            float cosp = 0.0f;
            float sinp = 0.0f;
            float cost = 0.0f;
            float sint = 0.0f;

            float prePhi = 0.0f;
            float preTheta = 0.0f;

            float3x3 m = float3x3.identity;
            float3 xaxis = float3.zero;
            float3 yaxis = float3.zero;
            float3 zaxis = float3.zero;

            while (true)
            {
                if(activeRotSpin)
                {
                    if (Input.GetMouseButtonDown(1) && Input.GetKey(key))
                    {
                        prePos = Input.mousePosition;
                        preRot = camTrans.rotation;
                        down = true;

                        {
                            m = new float3x3(preRot);
                            xaxis = m.c0;
                            yaxis = m.c1;
                            zaxis = m.c2;

                            {
                                cosp = math.dot(new float3(1.0f, 0.0f, 0.0f), xaxis);
                                sinp = math.dot(new float3(0.0f, 1.0f, 0.0f), math.cross(new float3(1.0f, 0.0f, 0.0f), xaxis));
                                prePhi = math.acos(cosp);
                                if (sinp < 0)
                                {
                                    prePhi *= (-1.0f);
                                }
                            }

                            {
                                cost = math.dot(new float3(0.0f, 1.0f, 0.0f), yaxis);
                                preTheta = math.acos(cost);
                            }
                        }
                    }

                    if (down)
                    {
                        curPos = Input.mousePosition;

                        angle = (curPos - prePos) * 0.001f;
                        {
                            float phi = 0.0f;
                            phi = prePhi + angle.x;

                            float theta = 0.0f;
                            theta = math.clamp(preTheta - angle.y, math.radians(theta0), math.radians(theta1));

                            q = math.mul(
                                quaternion.AxisAngle(new float3(0.0f, 1.0f, 0.0f), phi),
                                quaternion.AxisAngle(new float3(1.0f, 0.0f, 0.0f), theta));
                            camTrans.rotation = q;
                        }
                    }

                    if (Input.GetMouseButtonUp(1) || !Input.GetKey(key))
                    {
                        down = false;
                    }
                }    
                
                yield return null;

                if(!activeRotSpin)
                {
                    down = false;
                }
            }

        }


        //Room
        public IEnumerator ZoomInOut_NonCenter_range(Transform camTrans, float3 center, float pace, float r0, float r1)
        {
            Camera cam = camTrans.GetComponent<Camera>();

            //float3 center = float3.zero;

            float3 dir;
            dir = math.normalize(center - (float3)camTrans.position);
            float3x3 mat = new float3x3(camTrans.rotation);
            float3 rayDirInView = math.mul(math.transpose(mat), dir);
            float3 rayDirInWorld;

            while (true)
            {
                if(activeZoom)
                {
                    if(!UIunitRoomManager.inRects)
                    {
                        //float3 centerInView = new float3((float)cam.pixelWidth, (float)cam.pixelHeight, 0.0f) * rc;
                        //Ray ray = cam.ScreenPointToRay(centerInView);
                        //dir = ray.direction.normalized;

                        mat = new float3x3(camTrans.rotation);
                        rayDirInWorld = math.normalize(math.mul(mat, rayDirInView));

                        float delta = Input.GetAxis("Mouse ScrollWheel") * 400.0f * pace * Time.deltaTime;

                        float3 pos = camTrans.position;
                        pos = pos + rayDirInWorld * delta;

                        float r = math.length(center - pos);

                        if (r0 < r && r < r1)
                        {
                            camTrans.position = pos;
                        }
                    }                   
                }                

                yield return null;
            }
        }

        public IEnumerator RotOrbitConstNonCenterFixed_range(
            Transform camTrans, float3 center,
            float theta0 = 45.0f, float theta1 = 85.0f)
        {
            Camera cam = camTrans.gameObject.GetComponent<Camera>();

            float3 prePos = float3.zero;
            float3 curPos = float3.zero;

            float3 angle = float3.zero;

            bool down = false;

            //Ray ray = new Ray();
            //RaycastHit[] hits;

            //float3 center = float3.zero;
            //float3 centerInView = float3.zero;
            //float3 rc = new float3(0.2f, 0.5f, 0.0f);

            float radius = 1.0f;
            //float3 centerNormal = float3.zero;

            quaternion preRot = quaternion.identity;

            float3 rayDirInView = float3.zero;
            float3 rayDirInWorld = float3.zero;

            quaternion q = quaternion.identity;

            float cosp = 0.0f;
            float sinp = 0.0f;
            float cost = 0.0f;
            float sint = 0.0f;

            float prePhi = 0.0f;
            float preTheta = 0.0f;

            float3x3 m = float3x3.identity;
            float3 xaxis = float3.zero;
            float3 yaxis = float3.zero;
            float3 zaxis = float3.zero;


            float3 dir;
            dir = math.normalize(center - (float3)camTrans.position);
            float3x3 mat = new float3x3(camTrans.rotation);
            rayDirInView = math.mul(math.transpose(mat), dir);

            while (true)
            {
                if(activeRotOrbit)
                {
                    if (Input.GetMouseButtonDown(1) && down == false)
                    {
                        prePos = Input.mousePosition;
                        preRot = camTrans.rotation;

                        //centerInView = new float3((float)cam.pixelWidth, (float)cam.pixelHeight, 0.0f) * rc;
                        //ray = cam.ScreenPointToRay(centerInView);
                        //
                        //mat = new float3x3(camTrans.rotation);
                        //float3 rayDir = ray.direction.normalized;
                        //rayDirInView = math.mul(math.transpose(mat), rayDir);

                        radius = math.distance(center, camTrans.position);
                        down = true;

                        {
                            m = new float3x3(preRot);
                            xaxis = m.c0;
                            yaxis = m.c1;
                            zaxis = m.c2;

                            {
                                cosp = math.dot(new float3(1.0f, 0.0f, 0.0f), xaxis);
                                sinp = math.dot(new float3(0.0f, 1.0f, 0.0f), math.cross(new float3(1.0f, 0.0f, 0.0f), xaxis));
                                prePhi = math.acos(cosp);
                                if (sinp < 0)
                                {
                                    prePhi *= (-1.0f);
                                }

                            }

                            {
                                cost = math.dot(new float3(0.0f, 1.0f, 0.0f), yaxis);
                                preTheta = math.acos(cost);
                            }
                        }

                    }

                    if (down)
                    {
                        curPos = Input.mousePosition;
                        //angle = (curPos - prePos) * 0.1f;
                        angle = (curPos - prePos) * 0.001f;

                        {
                            float phi = 0.0f;
                            phi = prePhi - angle.x;

                            float theta = 0.0f;
                            theta = math.clamp(preTheta + angle.y, math.radians(theta0), math.radians(theta1));

                            q = math.mul(
                                quaternion.AxisAngle(new float3(0.0f, 1.0f, 0.0f), phi),
                                quaternion.AxisAngle(new float3(1.0f, 0.0f, 0.0f), theta));
                            camTrans.rotation = q;
                        }

                        {
                            mat = new float3x3(q);
                            rayDirInWorld = math.normalize(math.mul(mat, rayDirInView));

                            camTrans.position = center + radius * (-rayDirInWorld);
                        }
                    }

                    if (Input.GetMouseButtonUp(1))
                    {
                        down = false;
                    }
                }
              
                yield return null;

                if (!activeRotOrbit)
                {
                    down = false;
                }
            }

        }
    }
}
