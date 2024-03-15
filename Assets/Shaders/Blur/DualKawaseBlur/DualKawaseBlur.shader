Shader "Hidden/PostProcessing/DualKawaseBlur" {
    SubShader {
        Tags {
            "RenderType"="Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }
        LOD 200
        ZWrite Off
        Cull Off

        HLSLINCLUDE
            #include "../../Common/PostProcessing.hlsl"
        #include "DualKawaseBlurPass.hlsl"
        ENDHLSL

        Pass {
            Name "Kawase Blur DownSample Pass"

            HLSLPROGRAM
            #pragma vertex DownSamplePassVertex
            #pragma fragment DownSamplePassFragment
            ENDHLSL
        }

        Pass {
            Name "Kawase Blur UpSample Pass"

            HLSLPROGRAM
            #pragma vertex UpSamplePassVertex
            #pragma fragment UpSamplePassFragment
            ENDHLSL
        }
    }
}