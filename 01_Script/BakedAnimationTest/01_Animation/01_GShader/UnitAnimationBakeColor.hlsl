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
int dvCount;

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
};

struct RS_Out
{
    float4 posS : SV_POSITION;
    
    float3 posW : TEXCOORD1;
    float3 normalW : NORMAL;
    float3 tangentW : TANGENT;
    
    float2 uv : TEXCOORD0;
    float4 color : COLOR0;       
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
        
    return vOut;
}

PS_Out PShader(RS_Out pIn)
{
    PS_Out pOut;
    float3 c;    
    {
        float3 posW = pIn.posW;
        float3 normalW = normalize(pIn.normalW);
        float3 tangentW = normalize(pIn.tangentW);
        
        c = pIn.color.xyz;
        
        float NdotL = max(0.5f, dot(normalW, dirW_light));
        c *= NdotL;
        
        pOut.color = float4(c, 1.0f);
            
    }    
                          
    return pOut;
}