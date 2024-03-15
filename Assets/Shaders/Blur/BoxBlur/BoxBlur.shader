Shader "Hidden/PostProcessing/BoxBlur" {
    SubShader {
        Tags {
            "RenderType"="Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }
        LOD 200
        ZWrite Off
        Cull Off
        Pass {
            Name "Box Blur Pass"

            HLSLPROGRAM
            #pragma vertex BoxBlurPassVertex
            #pragma fragment BoxBlurPassFragment

            #include "../../Common/PostProcessing.hlsl"
            #include "BoxBlurPass.hlsl"
            ENDHLSL
        }
    }
}