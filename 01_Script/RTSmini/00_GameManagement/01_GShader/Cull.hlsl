cbuffer perView
{
    float4x4 CV;
}

cbuffer perLight
{
    float4 dirW_light;
};

cbuffer perObject
{
    float4 color;
};

struct Vertex
{
    float3 posW;
    float3 normalW;    
};


//StructuredBuffer<Vertex> vtx_Buffer;

StructuredBuffer<Vertex> sphere_vertex_Out_Buffer;
StructuredBuffer<Vertex> pvf_vertex_Buffer;
StructuredBuffer<Vertex> ovf_vertex_Buffer;

//int dvCount;

int dvCount_sp;
int dvCount_vf;

int cullOffset;
//Texture3D<float> testCull;
Texture3D<float> cullResult_pvf_Texture;
Texture3D<float> cullResult_ovf_Texture;

int cullMode;

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
    
    uint isCull : ISCULL;
    uint iid : SV_InstanceID;
};

struct RS_Out
{
    float4 posS : SV_POSITION;
    
    float3 posW : TEXCOORD1;
    float3 normalW : NORMAL;   
    
    uint isCull : ISCULL;
    uint iid : SV_InstanceID;
};

struct PS_Out
{
    float4 color : SV_Target;
};


VS_Out VShader_Sphere(IA_Out vIn)
{
    VS_Out vOut;
    uint vid = vIn.vid;
    uint iid = vIn.iid;
    
    uint isCull = 0;
    
    if (cullMode == 0)
    {
        if (cullResult_pvf_Texture.Load(int4(iid, 0, 0, 0)) == 0.0f)
        {
            isCull = 1;
        }
    }
    else if (0 < cullMode && cullMode < 5)
    {
        if (cullResult_ovf_Texture.Load(int4(iid, cullMode - 1, 0, 0)) == 0.0f)
        {
            isCull = 1;
        }
    }
    
    
    //if (isCull == 0)
    {
        Vertex vtx = sphere_vertex_Out_Buffer[iid * dvCount_sp + vid];
            
        vOut.posC = mul(CV, float4(vtx.posW, 1.0f));
    
        vOut.posW = vtx.posW;
        vOut.normalW = vtx.normalW;
    }
    //else
    //{
    //
    //    vOut.posC = float4(0.0f, 0.0f, 0.0f, 1.0f);
    //
    //    vOut.posW = float3(0.0f, 0.0f, 0.0f);
    //    vOut.normalW = float3(0.0f, 0.0f, 0.0f);       
    //}
    
    vOut.iid = iid;
    vOut.isCull = isCull;
        
    return vOut;
}

VS_Out VShader_PVF(IA_Out vIn)
{
    VS_Out vOut;
    uint vid = vIn.vid;
    uint iid = vIn.iid;
    
    uint isCull = 0;
       
    vOut.posC = float4(0.0f, 0.0f, 0.0f, 1.0f);
    vOut.posW = float3(0.0f, 0.0f, 0.0f);
    vOut.normalW = float3(0.0f, 0.0f, 0.0f);
    
    if(cullMode == 0)
    {
        Vertex vtx = pvf_vertex_Buffer[iid * dvCount_vf + vid];
            
        vOut.posC = mul(CV, float4(vtx.posW, 1.0f));
    
        vOut.posW = vtx.posW;
        vOut.normalW = vtx.normalW;              
    }
    else if (cullMode == 5)
    {
        Vertex vtx = pvf_vertex_Buffer[iid * dvCount_vf + vid];
            
        vOut.posC = mul(CV, float4(vtx.posW, 1.0f));
    
        vOut.posW = vtx.posW;
        vOut.normalW = vtx.normalW;
    }
            
    vOut.iid = iid;
    vOut.isCull = 0;
        
    return vOut;
}

VS_Out VShader_OVF(IA_Out vIn)
{
    VS_Out vOut;
    uint vid = vIn.vid;
    uint iid = vIn.iid;
    
    uint isCull = 0;
        
    vOut.posC = float4(0.0f, 0.0f, 0.0f, 1.0f);
    vOut.posW = float3(0.0f, 0.0f, 0.0f);
    vOut.normalW = float3(0.0f, 0.0f, 0.0f);
       
    if(0 < cullMode && cullMode < 5)    
    {
        if(cullMode - 1 == iid)
        {
            Vertex vtx = ovf_vertex_Buffer[iid * dvCount_vf + vid];
            
            vOut.posC = mul(CV, float4(vtx.posW, 1.0f));
    
            vOut.posW = vtx.posW;
            vOut.normalW = vtx.normalW;
        }       
    }  
    else if(cullMode == 5)
    {
        {
            Vertex vtx = ovf_vertex_Buffer[iid * dvCount_vf + vid];
            
            vOut.posC = mul(CV, float4(vtx.posW, 1.0f));
    
            vOut.posW = vtx.posW;
            vOut.normalW = vtx.normalW;
        }
    }
    
    vOut.iid = iid;
    vOut.isCull = 0;
        
    return vOut;
}


PS_Out PShader_Sphere(RS_Out pIn)
{
    PS_Out pOut;
    uint iid = pIn.iid;
    uint isCull = pIn.isCull;
    
    float3 pos = pIn.posW;
    float3 nom = pIn.normalW;
            
    float3 L = dirW_light;
    float3 N = normalize(nom);
    float NdotL = dot(N, L);
    
    float3 c;
    if (isCull == 0)
    {
        c = float3(0.0f, 0.0f, 1.0f);
        c.xyz = c * max(0.25f, NdotL);
    }
    else
    {
        c = float3(1.0f, 0.0f, 0.0f);
        c.xyz = c * max(0.25f, NdotL);
    }
        
    
    pOut.color = float4(c, 0.5f);
           
    return pOut;
}

PS_Out PShader_PVF(RS_Out pIn)
{
    PS_Out pOut;
    uint iid = pIn.iid;
    uint isCull = pIn.isCull;
    
    float3 pos = pIn.posW;
    float3 nom = pIn.normalW;
            
    float3 L = dirW_light;
    float3 N = normalize(nom);
    float NdotL = dot(N, L);
    
    float3 c;
    c = float3(0.0f, 1.0f, 0.0f);
    c.xyz = c * max(0.25f, NdotL);
    
    pOut.color = float4(c, 0.5f);
           
    return pOut;
}

PS_Out PShader_OVF(RS_Out pIn)
{
    PS_Out pOut;
    uint iid = pIn.iid;
    uint isCull = pIn.isCull;
    
    float3 pos = pIn.posW;
    float3 nom = pIn.normalW;
            
    float3 L = dirW_light;
    float3 N = normalize(nom);
    float NdotL = dot(N, L);
    
    float3 c;
    c = float3(0.0f, 1.0f, 0.0f);
    c.xyz = c * max(0.25f, NdotL);
    
    pOut.color = float4(c, 0.5f);
           
    return pOut;
}

