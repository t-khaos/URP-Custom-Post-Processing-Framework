Shader "Hidden/PostProcessing/BokehBlur" {
    SubShader {
        Tags {
            "RenderType"="Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }
        LOD 200
        ZWrite Off
        Cull Off
        Pass {
            Name "Bokeh Blur Pass"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment BokehBlurPassFragment

            #include "../../Common/PostProcessing.hlsl"
            #include "BokehBlurPass.hlsl"
            ENDHLSL
        }
    }
}