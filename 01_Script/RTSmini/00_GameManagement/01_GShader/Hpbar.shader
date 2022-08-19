Shader "Unlit/Hpbar"
{
    Properties
    {
        
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100

        Pass
        {
            Name "Hpbar"

            Cull Back
            ZWrite On
            ZTest LEqual
            
            BlendOp Add
            Blend SrcAlpha OneMinusSrcAlpha
            
            HLSLPROGRAM
            #pragma vertex VShader        
            #pragma fragment PShader      
            
            #include "Assets\01_Script\RTSmini\00_GameManagement\01_GShader\Hpbar.hlsl"
            ENDHLSL            
        }
    }
}
