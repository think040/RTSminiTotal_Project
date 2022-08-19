cbuffer perObject
{
    float4x4 W;
};

cbuffer perView
{
    float4x4 CV;
    float4x4 CV_depth;
};

struct VertexDynamic
{
    float3 posW;
    float3 normalW;
    float4 tangentW;
};

StructuredBuffer<VertexDynamic> vtxDynamic;

int dvCount;

int cullOffset;
//Texture2D<float> testCull;
//int enableCull;

struct IA_Out
{
    uint vid : SV_VertexID;
    uint iid : SV_InstanceID;
};

struct VS_Out
{
    float4 posC : SV_POSITION;
    float4 posC_depth : TEXCOORD6;
    uint iid : SV_InstanceID;
};

struct RS_Out
{
    float4 posS : SV_POSITION;
    float4 posC_depth : TEXCOORD6;
    uint iid : SV_InstanceID;
};

struct PS_Out
{
    float depth : SV_Target;
};

VS_Out VShader(IA_Out vIn)
{
    VS_Out vOut;
                   
    uint vid = vIn.vid;
    uint iid = vIn.iid;
    
    //bool inOvf = false;
    //
    //if (enableCull == 1)
    //{
    //    if (testCull.Load(int3(cullOffset + iid, 1, 0)) == 0.0f)
    //    {
    //        inOvf = true;
    //    }
    //}
    //else
    //{
    //    inOvf = true;
    //}
    
    //if (inOvf)
    {
        VertexDynamic vtxd = vtxDynamic[iid * dvCount + vid];
        float3 posW = vtxd.posW;
    
        vOut.posC = mul(CV, float4(posW, 1.0f));
        vOut.posC_depth = mul(CV_depth, float4(posW, 1.0f));
    }
    //else
    //{
    //    vOut.posC = float4(0.0f, 0.0f, 0.0f, 1.0f);
    //    vOut.posC_depth = float4(0.0f, 0.0f, 0.0f, 1.0f);
    //}
    
    vOut.iid = iid;
            
    return vOut;
}

PS_Out PShader(RS_Out pIn)
{
    PS_Out pOut;
    uint iid = pIn.iid;
    
    //bool inOvf = false;
    
    //if (enableCull == 1)
    //{
    //    if (testCull.Load(int3(cullOffset + iid, 1, 0)) == 0.0f)
    //    {
    //        inOvf = true;
    //    }
    //}
    //else
    //{
    //    inOvf = true;
    //}
    
    //if (inOvf)
    {
        float4 posC = pIn.posC_depth;
        pOut.depth = posC.z / posC.w;
    }
    //else
    //{
    //    pOut.depth = 1.0f;
    //}
    
    
    return pOut;
}