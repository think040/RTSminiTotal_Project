cbuffer perObject
{    
    float4x4 Ws[64];
    float4 color;
}
    		
cbuffer perView
{    
    float4x4 CV;
    float4x4 S;
}


struct IA_Out
{
    float4 posL : POSITION0;
    uint iid : SV_InstanceID;        
};

struct VS_Out
{
    float4 posW : SV_Position;
};

struct GS_Out
{
    float4 posC : SV_Position;
    float4 posCi : TEXCOORD1;
    float4 posC0 : TEXCOORD2;
    float4 posC1 : TEXCOORD3;
    float4 posC2 : TEXCOORD4;
};

struct RS_Out
{
    float4 posS : SV_Position;
    float4 posCi : TEXCOORD1;
    float4 posC0 : TEXCOORD2;
    float4 posC1 : TEXCOORD3;
    float4 posC2 : TEXCOORD4;
};

struct PS_Out
{
    float4 color : SV_Target;
};

VS_Out VShader(IA_Out vIn)
{        
    VS_Out vOut;
    
    float3 posW = float3(0.0f, 0.0f, 0.0f);
      
    uint iid = vIn.iid;    
    posW = mul(Ws[iid], vIn.posL).xyz;
    
    vOut.posW = float4(posW, 1.0f);
    
    return vOut;
}

[maxvertexcount(3)]
void GShader(triangle VS_Out gIn[3], inout TriangleStream<GS_Out> gOut)
{
    GS_Out v[3];
    float4 ps[3];
    int i;
    int j;
        
    for (i = 0; i < 3; i++)
    {
        ps[i] = mul(CV, gIn[i].posW);
        v[i].posC = ps[i];
        v[i].posCi = ps[i];
    }
    
    for (i = 0; i < 3; i++)
    {
        v[i].posC0 = float4(0.0f, 0.0f, 0.0f, 1.0f);
        v[i].posC1 = float4(0.0f, 0.0f, 0.0f, 1.0f);
        v[i].posC2 = float4(0.0f, 0.0f, 0.0f, 1.0f);
        for (j = 0; j < 3; j++)
        {
            if (j == 0)
            {
                v[i].posC0 = ps[j];
            }
            else if (j == 1)
            {
                v[i].posC1 = ps[j];
            }
            else if (j == 2)
            {
                v[i].posC2 = ps[j];
            }
        }
    }
    
    for (i = 0; i < 3; i++)
    {
        gOut.Append(v[i]);
    }
    
    gOut.RestartStrip();
}

PS_Out PShader(RS_Out pIn)
{
    PS_Out pOut;
       
    float2 vi = mul(S, pIn.posCi / pIn.posCi.w).xy;
    float2 v0 = mul(S, pIn.posC0 / pIn.posC0.w).xy;
    float2 v1 = mul(S, pIn.posC1 / pIn.posC1.w).xy;
    float2 v2 = mul(S, pIn.posC2 / pIn.posC2.w).xy;
        
    float2 e0 = v2 - v1;
    float2 e1 = v0 - v2;
    float2 e2 = v1 - v0;
    
    float2 n0 = normalize(float2(e0.y, -e0.x));
    float2 n1 = normalize(float2(e1.y, -e1.x));
    float2 n2 = normalize(float2(e2.y, -e2.x));
    
    float2 p0 = v1;
    float2 p1 = v2;
    float2 p2 = v0;
    
    float d0 = abs(dot(vi - p0, n0));
    float d1 = abs(dot(vi - p1, n1));
    float d2 = abs(dot(vi - p2, n2));
    
    pOut.color = float4(0.0f, 0.0f, 0.0f, 1.0f);
    
    float range = 1.0f;
    if (d0 < range || d1 < range || d2 < range)
    {
        float d = min(d0, min(d1, d2));
        float alpha = exp(-pow(2.0f * (range * 0.5f - d), 2));
        pOut.color = float4(color.r, color.g, color.b, alpha);
    }
    else
    {
        //pOut.color = float4(0.75f, 0.75f, 0.75f, 1.0f);
        pOut.color = float4(0.0f, 0.0f, 0.0f, 0.0f);
    }
    
    return pOut;
}