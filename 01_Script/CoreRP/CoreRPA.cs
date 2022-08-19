using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Unity.Mathematics;
using UnityEngine.EventSystems;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "CoreRPA", menuName = "CoreRPA")]
public class CoreRPA : RenderPipelineAsset
{
    protected override RenderPipeline CreatePipeline()
    {
        return new CoreRP();
    }
}

public class CoreRP : RenderPipeline
{
    protected override void Render(ScriptableRenderContext context, Camera[] cams)
    {
#if UNITY_EDITOR       
        //if (EditorApplication.isPlaying == true && RenderGOM.bRender)
        if (EditorApplication.isPlaying == true)
        {
            RenderGOM.PreFrameRender(context, cams);
            BeginFrameRendering(context, cams);

            foreach (Camera cam in cams)
            {
                {
                    RenderGOM.PreCameraRender(context, cam);
                    BeginCameraRendering(context, cam);

                    RenderGOM.RenderCam(context, cam);

                    EndCameraRendering(context, cam);
                    RenderGOM.PostCameraRender(context, cam);
                }

                //Debug.Log("Cam name : " + cam.gameObject.name);
            }

            EndFrameRendering(context, cams);
            RenderGOM.PostFrameRender(context, cams);
        }
#else        

        //if(RenderGOM.bRender)
        {
            RenderGOM.PreFrameRender(context, cams);
            BeginFrameRendering(context, cams);
            
            foreach (Camera cam in cams)
            {
                RenderGOM.PreCameraRender(context, cam);
                BeginCameraRendering(context, cam);

                RenderGOM.RenderCam(context, cam);

                EndCameraRendering(context, cam);
                RenderGOM.PostCameraRender(context, cam);
            }
            
            EndFrameRendering(context, cams);
            RenderGOM.PostFrameRender(context, cams);
        }
#endif


    }
}


public static class RenderGOM
{
    static RenderGOM()
    {
        GameObject[] rootGO = SceneManager.GetActiveScene().GetRootGameObjects();

        perCam = new PerCamera();
    }    

    public class PerCamera
    {
        public float4x4 V;
        public float4x4 C;
        public float4x4 S;
        public float4x4 CV;
        public float3 dirW_view;
        public float3 posW_view;
    }

    public static PerCamera perCam;

    public static bool bDrawDebug = true;

    public static bool bRender = true;

    public static Action<ScriptableRenderContext, Camera, PerCamera> OnRenderCam
    {
        get; set;
    } = (context, cam, perCam) => { };

    public static Action<ScriptableRenderContext, Camera, PerCamera> OnRenderCamAlpha
    {
        get; set;
    } = (context, cam, perCam) => { };

    public static Action<ScriptableRenderContext, Camera[]> BeginFrameRender
    {
        get; set;
    } = (context, cams) => { };

    public static Action<ScriptableRenderContext, Camera[]> EndFrameRender
    {
        get; set;
    } = (context, cams) => { };

    public static Action<ScriptableRenderContext, Camera> BeginCameraRender
    {
        get; set;
    } = (context, cam) => { };

    public static Action<ScriptableRenderContext, Camera> EndCameraRender
    {
        get; set;
    } = (context, cam) => { };

    public static Action<ScriptableRenderContext, Camera, PerCamera> OnRenderCamDebug
    {
        get; set;
    } = (context, cam, perCam) => { };


    public static void RenderCam(ScriptableRenderContext context, Camera cam)
    {
        float4x4 V = RenderUtil.GetVfromW(cam);
        float4x4 C = RenderUtil.GetCfromV(cam);
        float4x4 S = RenderUtil.GetSfromN(cam);
        float4x4 CV = math.mul(C, V);
        float3 dirW_view = -math.rotate(cam.transform.rotation, new float3(0.0f, 0.0f, 1.0f));
        float3 posW_view = cam.transform.position;

        CommandBuffer cmd = CommandBufferPool.Get();

        cmd.SetRenderTarget(cam.targetTexture);
        cmd.ClearRenderTarget(true, true, Color.cyan);

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);

        if (bRender)
        {           
            {                
                perCam.V = V;
                perCam.C = C;
                perCam.S = S;
                perCam.CV = CV;
                perCam.dirW_view = dirW_view;
                perCam.posW_view = posW_view;

                OnRenderCam(context, cam, perCam);

                OnRenderCamAlpha(context, cam, perCam);
            }
        }

        if (bDrawDebug)
        {
            if (cam.name == "DebugCam" || cam.cameraType == CameraType.SceneView)
            {
                OnRenderCamDebug(context, cam, perCam);
            }
        }

    }

    public static void PreFrameRender(ScriptableRenderContext context, Camera[] cams)
    {
        if (bRender)
        {
            BeginFrameRender(context, cams);
        }
    }

    public static void PreCameraRender(ScriptableRenderContext context, Camera cam)
    {
        if (bRender)
        {
            BeginCameraRender(context, cam);

            UpdateCSM(context, cam);
            Cull(context, cam);
            //CullDebug(context, cam);

            PreRenderCSM_GS(context, cam);
            RenderCSM_GS(context, cam);
        }
    }

    public static void PostCameraRender(ScriptableRenderContext context, Camera cam)
    {
        if (bRender)
        {
            EndCameraRender(context, cam);
        }
    }

    public static void PostFrameRender(ScriptableRenderContext context, Camera[] cams)
    {
        if (bRender)
        {
            EndFrameRender(context, cams);
        }

        context.Submit();
    }

    public static int csmCount;

    public static Action<ScriptableRenderContext, Camera> UpdateCSM
    {
        get; set;
    } = (context, cam) => { };

    public static Action<ScriptableRenderContext, Camera> Cull
    {
        get; set;
    } = (context, cam) => { };

    public static Action<ScriptableRenderContext, Camera> CullDebug
    {
        get; set;
    } = (context, cam) => { };

    public static Action<ScriptableRenderContext, Camera> PreRenderCSM_GS
    {
        get; set;
    } = (context, cam) => { };

    public static Action<ScriptableRenderContext, Camera> RenderCSM_GS
    { 
        get; set; 
    }  = (context, cam) => { };
    
   
    
    public static void InitCSMData_GS(int csmCount)
    {
        RenderGOM.csmCount = csmCount;
        //PreRenderCSM_GS = (context, cam) => { };
        //RenderCSM_GS = (context, cam) => { };        
    }


    //Test
    public static void PreCameraRender0(ScriptableRenderContext context, Camera cam)
    {
        if (bRender)
        {        
            for (int i = 0; i < csmCount; i++)
            {
                PreRenderCSMs[i](context, cam, i);
                RenderCSMs[i](context, cam, i);
            }
        }
    }


    public static Action<ScriptableRenderContext, Camera, int>[] RenderCSMs;
    public static Action<ScriptableRenderContext, Camera, int>[] PreRenderCSMs;

    public static void InitCSMData(int csmCount)
    {
        RenderGOM.csmCount = csmCount;
        PreRenderCSMs = new Action<ScriptableRenderContext, Camera, int>[csmCount];
        RenderCSMs = new Action<ScriptableRenderContext, Camera, int>[csmCount];

        for (int i = 0; i < csmCount; i++)
        {
            PreRenderCSMs[i] = (context, cam, i) => { };
            RenderCSMs[i] = (context, cam, i) => { };
        }
    }
  
}