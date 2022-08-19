Shader "Unlit/ArrowShader"
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
            Name "ArrowColor"

            HLSLPROGRAM
            #pragma vertex VShader           
            #pragma fragment PShader      
            
            #include "Assets\01_Script\RTSmini\04_Arrow\01_GShader\ArrowColor.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "ArrowDepth_GS"

            Cull Off
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex VShader           
            #pragma geometry GShader            
            #pragma fragment PShader      
            
            #include "Assets\01_Script\RTSmini\04_Arrow\01_GShader\ArrowDepth_GS.hlsl"
            ENDHLSL
        }
    }
}
