using System.Collections;
using System.Collections.Generic;

using Unity.Mathematics;
using Unity.Burst;
using Unity.Collections;

using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class PNtriangleTest : MonoBehaviour
{
    public Mesh mesh;
    public Shader shader;
    public GameObject Light;
    int pass;

    Material mte;
    MaterialPropertyBlock mpb;
  
    public float4 tFactor = new float4(4, 4, 4, 4);

    // Start is called before the first frame update
    void Start()
    {
        mte = new Material(shader);
        mpb = new MaterialPropertyBlock();
        
        pass = mte.FindPass("PNtriangleTest");

        RenderGOM.BeginCameraRender += PreRender;
        RenderGOM.OnRenderCamAlpha += Render;
    }

    void PreRender(ScriptableRenderContext context, Camera cam)
    {
        float4x4 W = RenderUtil.GetW(transform);
        float4x4 W_IT = math.transpose(math.inverse(W));

        mpb.SetMatrix("W", W);
        mpb.SetMatrix("W_IT", W_IT);

        mpb.SetVector("tFactor", tFactor);       
    }

    void Render(ScriptableRenderContext context, Camera cam, RenderGOM.PerCamera perCam)
    {
        {
            mpb.SetMatrix("CV", perCam.CV);

            float3 dirW_light = math.rotate(Light.transform.rotation, new float3(0.0f, 0.0f, 1.0f));
            mpb.SetVector("dirW_light", new float4(-dirW_light, 0.0f));
        }

        {
            CommandBuffer cmd = CommandBufferPool.Get();

            {                
                cmd.DrawMesh(mesh, Matrix4x4.identity, mte, 0, pass, mpb);
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
