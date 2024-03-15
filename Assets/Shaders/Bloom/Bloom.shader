// 参考：https://catlikecoding.com/unity/tutorials/custom-srp/post-processing/
    
Shader "Hidden/PostProcessing/Bloom" {
    SubShader {
        Tags {
            "RenderType"="Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }

        LOD 200
        ZWrite Off
        Cull Off

        HLSLINCLUDE
        #include "../Common/PostProcessing.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "BloomPass.hlsl"
        ENDHLSL

        Pass {
            Name "Bloom Blur Pass"

            HLSLPROGRAM
            #pragma vertex GaussianBlurPassVertex
            #pragma fragment GaussianBlurPassFragment
            ENDHLSL
        }

        Pass {
            Name "Bloom Combine Pass"

            HLSLPROGRAM
            #pragma shader_feature _ _BLOOMADDTIVE _BLOOMSCATTER
            #pragma vertex Vert
            #pragma fragment BloomCombinePassFragment
            ENDHLSL
        }

        Pass {
            Name "Bloom Prefilter Pass"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment BloomPrefilterPass
            ENDHLSL
        }

        Pass {
            Name "Bloom Prefilter Firefiles Pass"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment BloomPrefilterFirefilesPass
            ENDHLSL
        }

        Pass {
            Name "Bloom Scatter Final Pass"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment BloomScatterFinalPass
            ENDHLSL
        }
    }
}