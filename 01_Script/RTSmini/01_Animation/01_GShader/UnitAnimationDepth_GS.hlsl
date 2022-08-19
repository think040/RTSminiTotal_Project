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

int cullOffset;
Texture3D<float> cullResult_ovf_Texture;
//Texture2D<float> testCull;
//int enableCull;

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
    
    uint isCull : ISCULL;
    uint iid : SV_InstanceID;
    uint rtIndex : SV_RenderTargetArrayIndex;
};

struct RS_Out
{
    float4 posS : SV_POSITION;
    float4 posC_depth : TEXCOORD6;
    
    uint isCull : ISCULL;
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
    uint iid;
    uint isCull = 0;
    iid = gIn[0].iid;
    
    for (int i = 0; i < 4; i++)
    {
        GS_Out output;
        output.rtIndex = i;
        float4x4 CV = csmDataBuffer[i].CV;
        float4x4 CV_depth = csmDataBuffer[i].CV_depth;
        
        isCull = 0;
        if (cullResult_ovf_Texture.Load(int4(cullOffset + iid, i, 0, 0)) == 0.0f)
        {
            isCull = 1;
        }
        
        for (int j = 0; j < 3; j++)
        {
            if (isCull == 0)
            {
                output.posC = mul(CV, gIn[j].posW);
                output.posC_depth = mul(CV_depth, gIn[j].posW);
            }
            else
            {
                output.posC = float4(0.0f, 0.0f, 0.0f, 1.0f);
                output.posC_depth = float4(0.0f, 0.0f, 0.0f, 1.0f);
            }
                                               
            output.iid = gIn[j].iid;
            output.isCull = isCull;
            gOut.Append(output);
        }
        gOut.RestartStrip();
    }
}

PS_Out PShader(RS_Out pIn)
{
    PS_Out pOut;
    uint iid = pIn.iid;
    uint isCull = pIn.isCull;
    
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
    
    if (isCull == 0)
    {
        float4 posC = pIn.posC_depth;
        pOut.depth = posC.z / posC.w;
        //pOut.depth = 0.0f;
    }
    else
    {
        pOut.depth = 1.0f;
    }
    
    
    return pOut;
}



//Unit Room
VS_Out VShader_Room(IA_Out vIn)
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
void GShader_Room(triangle VS_Out gIn[3], inout TriangleStream<GS_Out> gOut)
{
    uint iid;
    uint isCull = 0;
    iid = gIn[0].iid;
    
    for (int i = 0; i < 4; i++)
    {
        GS_Out output;
        output.rtIndex = i;
        float4x4 CV = csmDataBuffer[i].CV;
        float4x4 CV_depth = csmDataBuffer[i].CV_depth;
        
        isCull = 0;
        //if (cullResult_ovf_Texture.Load(int4(cullOffset + iid, i, 0, 0)) == 0.0f)
        //{
        //    isCull = 1;
        //}
        
        for (int j = 0; j < 3; j++)
        {
            //if (isCull == 0)
            {
                output.posC = mul(CV, gIn[j].posW);
                output.posC_depth = mul(CV_depth, gIn[j].posW);
            }
            //else
            //{
            //    output.posC = float4(0.0f, 0.0f, 0.0f, 1.0f);
            //    output.posC_depth = float4(0.0f, 0.0f, 0.0f, 1.0f);
            //}
                                               
            output.iid = gIn[j].iid;
            output.isCull = isCull;
            gOut.Append(output);
        }
        gOut.RestartStrip();
    }
}

PS_Out PShader_Room(RS_Out pIn)
{
    PS_Out pOut;
    uint iid = pIn.iid;
    uint isCull = pIn.isCull;
    
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
    
    //if (isCull == 0)
    {
        float4 posC = pIn.posC_depth;
        pOut.depth = posC.z / posC.w;
        //pOut.depth = 0.0f;
    }
    //else
    //{
    //    pOut.depth = 1.0f;
    //}
    
    
    return pOut;
}