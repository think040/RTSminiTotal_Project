Shader "Unlit/UnitAnimationBake"
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
            Name "UnitAnimationBakeColor"

            Cull Back
            ZWrite On
            ZTest LEqual

            BlendOp Add
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex VShader           
            #pragma fragment PShader      
            
            #include "Assets\01_Script\BakedAnimationTest\01_Animation\01_GShader\UnitAnimationBakeColor.hlsl"
            ENDHLSL
        }        
    }
}
