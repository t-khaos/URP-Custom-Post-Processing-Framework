Shader "Hidden/PostProcessing/KawaseBlur" {
    SubShader {
        Tags {
            "RenderType"="Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }
        LOD 200
        ZWrite Off
        Cull Off
        Pass {
            Name "Kawase Blur Pass"

            HLSLPROGRAM
            #pragma vertex KawaseBlurPassVertex
            #pragma fragment KawaseBlurPassFragment

            #include "../../Common/PostProcessing.hlsl"
            #include "KawaseBlurPass.hlsl"
            ENDHLSL
        }
    }
}