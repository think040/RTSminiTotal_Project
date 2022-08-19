Shader "Unlit/DebugShader"
{
    Properties
    {
        
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            Name "BVwireframe"

            Cull Back
            ZWrite On
            ZTest LEqual

            BlendOp Add
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex VShader                        
            #pragma geometry GShader
            #pragma fragment PShader        
            
            #include "BVwireframe.hlsl"
            ENDHLSL
        }
    }
}
