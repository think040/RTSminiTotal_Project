#include "../../../Utility/01_GShader/CSMUtil.hlsl"

struct VertexDynamic
{
    float3 posW;
    float3 normalW;
    float4 tangentW;
};

struct VertexStatic
{
    float2 uv;
    float4 color;
};

StructuredBuffer<VertexDynamic> vtxDynamic;
StructuredBuffer<VertexStatic> vtxStatic;

Texture2D diffuseTex;
SamplerState sampler_diffuseTex;

int dvCount;

int cullOffset;
Texture2D<float> testCull;

struct IA_Out
{
    uint vid : SV_VertexID;
    uint iid : SV_InstanceID;
};

struct VS_Out
{
    float4 posC : SV_POSITION;
    
    float3 posW : TEXCOORD1;
    float3 normalW : NORMAL;
    float3 tangentW : TANGENT;
    
    float2 uv : TEXCOORD0;
    float4 color : COLOR0;
    
    uint iid : SV_InstanceID;
};

struct RS_Out
{
    float4 posS : SV_POSITION;
    
    float3 posW : TEXCOORD1;
    float3 normalW : NORMAL;
    float3 tangentW : TANGENT;
    
    float2 uv : TEXCOORD0;
    float4 color : COLOR0;
    
    uint iid : SV_InstanceID;
};

struct PS_Out
{
    float4 color : SV_Target;
};


VS_Out VShader(IA_Out vIn)
{
    VS_Out vOut;
    uint vid = vIn.vid;
    uint iid = vIn.iid;
    
    //if (testCull.Load(int3(cullOffset + iid, 0, 0)) == 0.0f)
    {
        VertexDynamic vtxd = vtxDynamic[iid * dvCount + vid];
        VertexStatic vtxs = vtxStatic[vid];
    
        vOut.posC = mul(CV, float4(vtxd.posW, 1.0f));
    
        vOut.posW = vtxd.posW;
        vOut.normalW = vtxd.normalW;
        vOut.tangentW = vtxd.tangentW;
    
        vOut.uv = vtxs.uv;
        vOut.color = vtxs.color;
    }
    //else
    //{
    //
    //    vOut.posC = float4(0.0f, 0.0f, 0.0f, 1.0f);
    //
    //    vOut.posW = float3(0.0f, 0.0f, 0.0f);
    //    vOut.normalW = float3(0.0f, 0.0f, 0.0f);
    //    vOut.tangentW = float3(0.0f, 0.0f, 0.0f);
    //
    //    vOut.uv = float2(0.0f, 0.0f);
    //    vOut.color = float4(0.0f, 0.0f, 0.0f, 1.0f);
    //}
    
    vOut.iid = iid;
        
    return vOut;
}

int useTex;

//PS_Out PShader(RS_Out pIn)
//{
//    PS_Out pOut;
//    
//    pOut.color = float4(pIn.color.xyz, 1.0f);
//                          
//    return pOut;
//}

PS_Out PShader(RS_Out pIn)
{
    PS_Out pOut;
    float3 c;
    uint iid = pIn.iid;
    
    //if (testCull.Load(int3(cullOffset + iid, 0, 0)) == 0.0f)
    {
        float3 posW = pIn.posW;
        float3 normalW = normalize(pIn.normalW);
        float3 tangentW = normalize(pIn.tangentW);
    
        float NdotL;
        float sf = CSMUtil::GetShadowFactor_CSM(pIn.posW, pIn.normalW, NdotL);
   
        float k = 1.5f;
        float m = 0.4f;
        float l = 0.4f;
            
        
        float specularFactor;
        //specularFactor = CSMUtil::GetSpecularFactor(pIn.posW, pIn.normalW);
        specularFactor = CSMUtil::GetSpecularFactor_1(pIn.posW, pIn.normalW);
       
        {                        
        //c = k * NdotL * (1.0f - m * sf) * pIn.color.xyz;
            c = k * NdotL * (1.0f - m * sf) * ((1.0f - l) * pIn.color.xyz + l * specularFactor * float3(1.0f, 1.0f, 1.0f));
            
        ////Debug
        //{
        //    float specularFactor = 0.0f;
        //    float3 reflectW;
        //
        //    reflectW = normalize(2 * dot(dirW_light, normalW) * normalW - dirW_light);
        //    specularFactor = pow(max(0.0f, dot(reflectW, dirW_view)), specularPow);
        //
        //    c = abs(pIn.normalW);
        //    c = abs(reflectW);
        //    c = specularFactor * float3(1.0f, 1.0f, 1.0f);
        //}
              
            pOut.color = float4(c.xyz, 1.0f);
        }
    }
    //else
    //{
    //    pOut.color = float4(0.0f, 0.0f, 0.0f, 1.0f);
    //}
    
           
           
    return pOut;
}