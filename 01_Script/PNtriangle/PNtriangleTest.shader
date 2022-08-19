Shader "Unlit/PNtriangleTest"
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
            Name "PNtriangleTest"

            Cull Back
            ZWrite On
            ZTest LEqual

            BlendOp Add
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex VShader  
            #pragma hull HShader
            #pragma domain DShader
            #pragma geometry GShader
            #pragma fragment PShader   

            //#pragma require tessellation

            //#include "UnityCG.cginc"
            #include "Assets\01_Script\PNtriangle\PNtriangleTest.hlsl"
            ENDHLSL
        }
    }
}
