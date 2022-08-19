#include "../Utility/02_CShader/UtilityCS.hlsl"

struct DataIn
{
    float4x4 data0;
    float4x4 data1;
};

struct DataOut
{
    float4x4 data0;
    float4x4 data1;
};

Texture3D<float4> texIn;
RWTexture3D<float4> texOut;

[numthreads(1, 1, 1)]
void CS_DQtest_tex(uint3 dtid : SV_DispatchThreadID, uint3 gid : SV_GroupID, uint3 gtid : SV_GroupThreadID, uint gidx : SV_GroupIndex)
{
    DualQuaternion a;
    DualQuaternion b;
    
    uint idx = gid.x;
    uint count = 1;
    {        
        DataIn dataIn;
        //dataIn.data0.v4c0 = texIn.Load(int4(idx * 4 + 0, 0.0f, 0.0f, 0.0f));
        //dataIn.data0.v4c1 = texIn.Load(int4(idx * 4 + 1, 0.0f, 0.0f, 0.0f));
        //dataIn.data0.v4c2 = texIn.Load(int4(idx * 4 + 2, 0.0f, 0.0f, 0.0f));
        //dataIn.data0.v4c3 = texIn.Load(int4(idx * 4 + 3, 0.0f, 0.0f, 0.0f));
        
        //dataIn.data1.v4c0 = texIn.Load(int4(idx * 4 + 0, 1.0f, 0.0f, 0.0f));
        //dataIn.data1.v4c1 = texIn.Load(int4(idx * 4 + 1, 1.0f, 0.0f, 0.0f));
        //dataIn.data1.v4c2 = texIn.Load(int4(idx * 4 + 2, 1.0f, 0.0f, 0.0f));
        //dataIn.data1.v4c3 = texIn.Load(int4(idx * 4 + 3, 1.0f, 0.0f, 0.0f));
        
        dataIn.data0.v4c0 = texIn[int3(idx * 4 + 0, 0.0f, 0.0f)];
        dataIn.data0.v4c1 = texIn[int3(idx * 4 + 1, 0.0f, 0.0f)];
        dataIn.data0.v4c2 = texIn[int3(idx * 4 + 2, 0.0f, 0.0f)];
        dataIn.data0.v4c3 = texIn[int3(idx * 4 + 3, 0.0f, 0.0f)];
                
        dataIn.data1.v4c0 = texIn[int3(idx * 4 + 0, 1.0f, 0.0f)];
        dataIn.data1.v4c1 = texIn[int3(idx * 4 + 1, 1.0f, 0.0f)];
        dataIn.data1.v4c2 = texIn[int3(idx * 4 + 2, 1.0f, 0.0f)];
        dataIn.data1.v4c3 = texIn[int3(idx * 4 + 3, 1.0f, 0.0f)];
        
        DualQuaternion q0;
        DualQuaternion q1;
        
        //q0.real = dataIn.data0.v4c0;
        //q0.dual = dataIn.data0.v4c1;
        //q1.real = dataIn.data0.v4c2;
        //q1.dual = dataIn.data0.v4c3;
        
        q0.set(dataIn.data0.v4c0, dataIn.data0.v4c1);
        q1.set(dataIn.data0.v4c2, dataIn.data0.v4c3);
        
        count = (uint) dataIn.data1.v4c0.x;
        
        float u = (float) idx / (float) (count - 1);
        
        float3 rp;
        float3 rb;
        float3 rl;
        DualQuaternion q2 = DualQuaternion::scLerp_debug(q0, q1, u, rp, rb, rl);
        
        float3 t;
        float4 r;
        q2.toRigidParam(r, t);
        
        DataOut dataOut;
               
        //{
        //    dataOut.data0.v4c0 = float4(t, 0.0f);
        //    dataOut.data0.v4c1 = float4(rp, 0.0f);
        //    dataOut.data0.v4c2 = float4(rb, 0.0f);
        //    dataOut.data0.v4c3 = float4(rl, 0.0f);
        //    
        //    dataOut.data1 = 0.0f;
        //}
        
        {
            dataOut.data0 = 0.0f;
        
            dataOut.data1.v4c0 = float4(t, 0.0f);
            dataOut.data1.v4c1 = float4(rp, 0.0f);
            dataOut.data1.v4c2 = float4(rb, 0.0f);
            dataOut.data1.v4c3 = float4(rl, 0.0f);
        }
                                     
        texOut[uint3(idx * 4 + 0, 0.0f, 0.0f)] = dataOut.data0.v4c0;
        texOut[uint3(idx * 4 + 1, 0.0f, 0.0f)] = dataOut.data0.v4c1;
        texOut[uint3(idx * 4 + 2, 0.0f, 0.0f)] = dataOut.data0.v4c2;
        texOut[uint3(idx * 4 + 3, 0.0f, 0.0f)] = dataOut.data0.v4c3;
        
        texOut[uint3(idx * 4 + 0, 1.0f, 0.0f)] = dataOut.data1.v4c0;
        texOut[uint3(idx * 4 + 1, 1.0f, 0.0f)] = dataOut.data1.v4c1;
        texOut[uint3(idx * 4 + 2, 1.0f, 0.0f)] = dataOut.data1.v4c2;
        texOut[uint3(idx * 4 + 3, 1.0f, 0.0f)] = dataOut.data1.v4c3;

    }
}