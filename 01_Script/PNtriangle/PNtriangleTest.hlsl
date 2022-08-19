cbuffer perObject
{
	float4x4 W;
	float4x4 W_IT;
}

cbuffer perView
{
	float4x4 CV;	
};
			
cbuffer perLight
{
	float4 dirW_light;
};

cbuffer tessFactor
{		
	float4 tFactor;
};

struct IA_Out
{
	float3 posL : POSITION;
	float3 normalL : NORMAL;
};

struct VS_Out
{
	//float3 posL : WORLD_SPACE_CONTROL_POINT_POSITION;
	//float3 normalL : WORLD_SPACE_CONTROL_POINT_NORMAL;
	
	float3 posL : POSTION;
	float3 normalL : NORMAL;
};

struct TS_Out
{
	float Edges[3] : SV_TessFactor;
	float Inside : SV_InsideTessFactor;
};

struct HS_Out
{
	//float3 posL : WORLD_SPACE_CONTROL_POINT_POSITION;
	//float3 normalL : WORLD_SPACE_CONTROL_POINT_NORMAL;
	
	float3 posL : POSTION;
	float3 normalL : NORMAL;
};

struct DS_Out
{
	float4 posL : POSITION;
	float3 normalL : NORMAL;
};

struct GS_Out
{
	float4 posC : SV_Position;
	float3 normalW : NORMAL;
};

struct PS_Out
{
	float4 color : SV_Target;
};


VS_Out VShader(IA_Out vIn)
{
	VS_Out vOut;

	vOut.posL = vIn.posL;
	vOut.normalL = vIn.normalL;

	return vOut;
}

TS_Out TShader(
				InputPatch<VS_Out, 3> ip,
				uint PatchID : SV_PrimitiveID)
{
	TS_Out tsOut;	
	
	tsOut.Edges[0] = tFactor.x;
	tsOut.Edges[1] = tFactor.x;
	tsOut.Edges[2] = tFactor.x;
	tsOut.Inside =   tFactor.x;		

	return tsOut;
}

float3 getEdgeCPoint(InputPatch<VS_Out, 3> ip, int i, int j)
{
	float3 outPos = float3(0.0f, 0.0f, 0.0f);
	outPos = ((2.0f * ip[i].posL + ip[j].posL) +
					(-1.0f) * dot((ip[j].posL - ip[i].posL), ip[i].normalL) * ip[i].normalL)
					/ 3.0f;
	return outPos;
}

float3 getFaceCPoint(InputPatch<VS_Out, 3> ip)
{
	float3 outPos = float3(0.0f, 0.0f, 0.0f);
	float3 E = (
					getEdgeCPoint(ip, 0, 1) + getEdgeCPoint(ip, 1, 0) +
					getEdgeCPoint(ip, 1, 2) + getEdgeCPoint(ip, 2, 1) +
					getEdgeCPoint(ip, 2, 0) + getEdgeCPoint(ip, 0, 2)
					) / 6.0f;
	float3 V = (
					ip[0].posL + ip[1].posL + ip[2].posL
					) / 3.0f;
	outPos = E + (E - V) / 2.0f;

	return outPos;
}

float3 getEdgeCNormal(InputPatch<VS_Out, 3> ip, int i, int j)
{
	float3 outNormal = float3(1.0f, 1.0f, 1.0f);

	outNormal = (ip[i].normalL + ip[j].normalL)
					- 2.0f * (
						dot(ip[i].normalL + ip[j].normalL, ip[j].posL - ip[i].posL) /
						dot(ip[j].posL - ip[i].posL, ip[j].posL - ip[i].posL) *
						(ip[j].posL - ip[i].posL));
	outNormal = normalize(outNormal);

	return outNormal;
}


[domain("tri")]
[partitioning("integer")]
//[partitioning("fractional_odd")]
//[partitioning("fractional_even")]
//[partitioning("pow2")]
[outputtopology("triangle_cw")]
[outputcontrolpoints(13)]
[patchconstantfunc("TShader")]
[maxtessfactor(64.0f)]
			HS_Out HShader(
				const InputPatch<VS_Out, 3> ip,
				uint i : SV_OutputControlPointID,
				uint PatchID : SV_PrimitiveID)
{
	HS_Out hsOut;

	hsOut.posL = float3(0.0f, 0.0f, 0.0f);
	hsOut.normalL = float3(1.0f, 1.0f, 1.0f);

	switch (i)
	{
					//vertex control position normal
		case 0:
			hsOut.posL = ip[0].posL;
			hsOut.normalL = ip[0].normalL;
			break;
		case 1:
			hsOut.posL = ip[1].posL;
			hsOut.normalL = ip[1].normalL;
			break;
		case 2:
			hsOut.posL = ip[2].posL;
			hsOut.normalL = ip[2].normalL;
			break;
					//Edge control position 0 , 1
		case 3:
			hsOut.posL = getEdgeCPoint(ip, 0, 1);
			break;
		case 4:
			hsOut.posL = getEdgeCPoint(ip, 1, 0);
			break;
					//Edge control position 1 , 2
		case 5:
			hsOut.posL = getEdgeCPoint(ip, 1, 2);
			break;
		case 6:
			hsOut.posL = getEdgeCPoint(ip, 2, 1);
			break;
					//Edge control position 2 , 0
		case 7:
			hsOut.posL = getEdgeCPoint(ip, 2, 0);
			break;
		case 8:
			hsOut.posL = getEdgeCPoint(ip, 0, 2);
			break;
					//Face control position 0 , 1
		case 9:
			hsOut.posL = getFaceCPoint(ip);
			break;
					//Edge control normal 0 , 1 , 2
		case 10:
			hsOut.normalL = getEdgeCNormal(ip, 0, 1);
			break;
		case 11:
			hsOut.normalL = getEdgeCNormal(ip, 1, 2);
			break;
		case 12:
			hsOut.normalL = getEdgeCNormal(ip, 2, 0);
			break;

	}

	return hsOut;
}

[domain("tri")]
DS_Out DShader(
				const OutputPatch<HS_Out, 13> op,
				float3 bc : SV_DomainLocation,
				TS_Out tsOut)
{
	DS_Out dOut;

	float u = bc.x;
	float v = bc.y;
	float w = bc.z;

				//Control Point
	float3 p300 = op[0].posL;
	float3 p030 = op[1].posL;
	float3 p003 = op[2].posL;

	float3 p210 = op[3].posL;
	float3 p120 = op[4].posL;

	float3 p021 = op[5].posL;
	float3 p012 = op[6].posL;

	float3 p102 = op[7].posL;
	float3 p201 = op[8].posL;

	float3 p111 = op[9].posL;

				//Control Normal
	float3 n200 = op[0].normalL;
	float3 n020 = op[1].normalL;
	float3 n002 = op[2].normalL;

	float3 n110 = op[10].normalL;
	float3 n011 = op[11].normalL;
	float3 n101 = op[12].normalL;

	float3 pos =
					p300 * pow(u, 3) + p030 * pow(v, 3) + p003 * pow(w, 3) +
					3.0f * p210 * pow(u, 2) * v + 3.0f * p120 * u * pow(v, 2) +
					3.0f * p021 * pow(v, 2) * w + 3.0f * p012 * v * pow(w, 2) +
					3.0f * p102 * pow(w, 2) * u + 3.0f * p201 * w * pow(u, 2) +
					6.0f * p111 * u * v * w;

	float3 normal =
					n200 * pow(u, 2) + n020 * pow(v, 2) + n002 * pow(w, 2) +
					2.0f * n110 * u * v +
					2.0f * n011 * v * w +
					2.0f * n101 * w * u;

	float4 posL = float4(pos.x, pos.y, pos.z, 1.0f);
			
	dOut.posL = posL; 
	dOut.normalL = normal; 

	return dOut;
}

[maxvertexcount(3)]
void GShader(triangle DS_Out gIn[3],
				inout TriangleStream<GS_Out> triangleStream)
{
	GS_Out gOut[3];
	for (int i = 0; i < 3; i++)
	{		
		gOut[i].posC = mul(CV, mul(W, gIn[i].posL));
		gOut[i].normalW = mul((float3x3) W_IT, gIn[i].normalL).xyz;
		triangleStream.Append(gOut[i]);
	}

	triangleStream.RestartStrip();
}

PS_Out PShader(GS_Out pIn)
{
	PS_Out rOut;
	float3 color = float3(1.0f, 0.0f, 0.0f);
	float3 nom = normalize(pIn.normalW);
	
	float NdotL = max(0.25f, dot(nom, dirW_light.xyz));
	
	rOut.color = float4(NdotL * color, 1.0f);

	return rOut;
}
