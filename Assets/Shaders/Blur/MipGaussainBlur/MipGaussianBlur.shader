Shader "Hidden/PostProcessing/MipGaussianBlur"
{
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }
        LOD 200
        ZWrite Off
        Cull Off

        HLSLINCLUDE
        #include "../../Common/PostProcessing.hlsl"
        #include "MipGaussianBlur.hlsl"
        ENDHLSL

        Pass
        {
            Name "Mipmap Gaussian DownSample Pass"

            HLSLPROGRAM
            #pragma multi_compile ADVANCED_MIP_GAUSSIAN
            
            #pragma vertex DownSamplePassVertexDualKawase
            #pragma fragment DownSamplePassFragmentDualKawase
            
            /*#pragma vertex Vert
            #pragma fragment DownSamplePassFragment*/
            ENDHLSL
        }

        Pass
        {
            Name "Mipmap Gaussian UpSample Pass"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment UpSamplePassFragment
            ENDHLSL
        }
    }
}