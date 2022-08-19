Shader "Unlit/BGShader"
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
            Name "BGShaderColor"

            HLSLPROGRAM
            #pragma vertex VShader           
            #pragma fragment PShader      
            
            #include "Assets/01_Script/RTSmini/03_BattleGround/01_GShader/BGShaderColor.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "BGShaderDepth"

            Cull Off
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex VShader           
            #pragma fragment PShader      
            
            #include "Assets/01_Script/RTSmini/03_BattleGround/01_GShader/BGShaderDepth.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "BGShaderDepth_GS"

            Cull Back
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex VShader  
            #pragma geometry GShader
            #pragma fragment PShader      

            #include "Assets/01_Script/RTSmini/03_BattleGround/01_GShader/BGShaderDepth_GS.hlsl"
            ENDHLSL
        }
    }
}
