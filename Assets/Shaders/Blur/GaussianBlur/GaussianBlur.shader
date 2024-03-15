Shader "Hidden/PostProcessing/GaussianBlur" {
    SubShader {
        Tags {
            "RenderType"="Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }
        LOD 200
        ZWrite Off
        Cull Off
        
        Pass {
            Name "Gaussian Blur Pass"

            HLSLPROGRAM
            #pragma vertex GaussianBlurPassVertex
            #pragma fragment frag

            #include "../../Common/PostProcessing.hlsl"
            #include "GaussianBlurPass.hlsl"
            ENDHLSL
        }
        
    }
}