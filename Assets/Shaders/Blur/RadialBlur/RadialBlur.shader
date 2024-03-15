Shader "Hidden/PostProcessing/RadialBlur" {
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
            #pragma fragment RadialBlurPassFragment

            #include "../../Common/PostProcessing.hlsl"
            #include "RadialBlurPass.hlsl"
            ENDHLSL
        }
    }
}