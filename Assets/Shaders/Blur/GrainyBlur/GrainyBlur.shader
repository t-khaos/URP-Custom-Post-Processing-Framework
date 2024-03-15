Shader "Hidden/PostProcessing/GrainyBlur" {
    SubShader {
        Tags {
            "RenderType"="Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }
        LOD 200
        ZWrite Off
        Cull Off
        Pass {
            Name "Grainy Blur Pass"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment GrainyBlurPassFragment

            #include "../../Common/PostProcessing.hlsl"
            #include "GrainyBlurPass.hlsl"
            ENDHLSL
        }
    }
}