Shader "Unlit/UnitAnimation"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            Name "UnitAnimationColor"

            Cull Back
            ZWrite On
            ZTest LEqual

            BlendOp Add
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex VShader           
            #pragma fragment PShader      
            
            #include "Assets\01_Script\RTSmini\01_Animation\01_GShader\UnitAnimationColor.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "UnitAnimationColor_Room"

            Cull Back
            ZWrite On
            ZTest LEqual

            BlendOp Add
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex VShader_Room           
            #pragma fragment PShader_Room      

            #include "Assets\01_Script\RTSmini\01_Animation\01_GShader\UnitAnimationColor.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "UnitAnimationDepth"

            Cull Off
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex VShader           
            #pragma fragment PShader      

            #include "Assets\01_Script\RTSmini\01_Animation\01_GShader\UnitAnimationDepth.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "UnitAnimationDepth_GS"

            Cull Off
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex VShader
            #pragma geometry GShader
            #pragma fragment PShader      

            #include "Assets\01_Script\RTSmini\01_Animation\01_GShader\UnitAnimationDepth_GS.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "UnitAnimationDepth_Room_GS"

            Cull Off
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex      VShader_Room
            #pragma geometry    GShader_Room
            #pragma fragment    PShader_Room      

            #include "Assets\01_Script\RTSmini\01_Animation\01_GShader\UnitAnimationDepth_GS.hlsl"
            ENDHLSL
        }
    }
}
