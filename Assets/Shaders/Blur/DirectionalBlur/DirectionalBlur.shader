Shader "Hidden/PostProcessing/DirectionalBlur" {
    SubShader {
        Tags {
            "RenderType"="Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }
        LOD 200
        ZWrite Off
        Cull Off
        Pass {
            Name "Radial Blur Pass"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment DirectionalBlurPassFragment

            #include "../../Common/PostProcessing.hlsl"
            #include "DirectionalBlurPass.hlsl"
            ENDHLSL
        }
    }
}