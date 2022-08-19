cbuffer perObject
{
    float4x4 W;
};

//cbuffer perView
//{
//    float4x4 CV;
//    float4x4 CV_depth;
//};

struct VertexDynamic
{
    float3 posW;
    float3 normalW;
    float4 tangentW;
};

StructuredBuffer<VertexDynamic> vtxDynamic;

struct CSMdata
{
    float4x4 CV;
    float4x4 CV_depth;
};

StructuredBuffer<CSMdata> csmDataBuffer;

int dvCount;

//int cullOffset;
//Texture2D<float> testCull;

struct IA_Out
{
    uint vid : SV_VertexID;
    uint iid : SV_InstanceID;
};

struct VS_Out
{
    float4 posW : SV_POSITION;
    uint iid : SV_InstanceID;
};

struct GS_Out
{
    float4 posC : SV_POSITION;
    float4 posC_depth : TEXCOORD6;
    uint iid : SV_InstanceID;
    uint rtIndex : SV_RenderTargetArrayIndex;
};

struct RS_Out
{
    float4 posS : SV_POSITION;
    float4 posC_depth : TEXCOORD6;
    uint iid : SV_InstanceID;
    uint rtIndex : SV_RenderTargetArrayIndex;
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
    
    //if (testCull.Load(int3(cullOffset + iid, 1, 0)) == 0.0f)
    {
        VertexDynamic vtxd = vtxDynamic[iid * dvCount + vid];
        float3 posW = vtxd.posW;
        
        vOut.posW = float4(posW, 1.0f);
        //vOut.posC = mul(CV, float4(posW, 1.0f));
        //vOut.posC_depth = mul(CV_depth, float4(posW, 1.0f));
    }
    //else
    //{
    //    vOut.posC = float4(0.0f, 0.0f, 0.0f, 1.0f);
    //    vOut.posC_depth = float4(0.0f, 0.0f, 0.0f, 1.0f);
    //}
    
    vOut.iid = iid;
            
    return vOut;
}

[maxvertexcount(12)]
void GShader(triangle VS_Out gIn[3], inout TriangleStream<GS_Out> gOut)
{
    for (int i = 0; i < 4; i++)
    {
        GS_Out output;
        output.rtIndex = i;
        float4x4 CV = csmDataBuffer[i].CV;
        float4x4 CV_depth = csmDataBuffer[i].CV_depth;
        
        for (int j = 0; j < 3; j++)
        {
            output.posC = mul(CV, gIn[j].posW);
            output.posC_depth = mul(CV_depth, gIn[j].posW);
            output.iid = gIn[j].iid;
            gOut.Append(output);
        }
        gOut.RestartStrip();
    }
}

PS_Out PShader(RS_Out pIn)
{
    PS_Out pOut;
    uint iid = pIn.iid;
    
    //if (testCull.Load(int3(cullOffset + iid, 1, 0)) == 0.0f)
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