Shader "Hidden/PostProcess/SobleFilter" {
    SubShader {
        Tags {
            "RenderType"="Opaque"
        }

        LOD 200
        ZWrite Off
        Cull Off

        HLSLINCLUDE
        #include "Common/PostProcessing.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"
        ENDHLSL

        Pass {
            Name "Sobel Filter"
            HLSLPROGRAM
            #pragma shader_feature RAW_OUTLINE
            #pragma shader_feature POSTERIZE

            float _Delta;
            int _PosterizationCount;

            float sobel(float2 uv) {
                float2 delta = float2(_Delta, _Delta);

                float hr = 0;
                float vt = 0;

                hr += SampleSceneDepth(uv + float2(-1.0, -1.0) * delta) * 1.0;
                hr += SampleSceneDepth(uv + float2(1.0, -1.0) * delta) * -1.0;
                hr += SampleSceneDepth(uv + float2(-1.0, 0.0) * delta) * 2.0;
                hr += SampleSceneDepth(uv + float2(1.0, 0.0) * delta) * -2.0;
                hr += SampleSceneDepth(uv + float2(-1.0, 1.0) * delta) * 1.0;
                hr += SampleSceneDepth(uv + float2(1.0, 1.0) * delta) * -1.0;

                vt += SampleSceneDepth(uv + float2(-1.0, -1.0) * delta) * 1.0;
                vt += SampleSceneDepth(uv + float2(0.0, -1.0) * delta) * 2.0;
                vt += SampleSceneDepth(uv + float2(1.0, -1.0) * delta) * 1.0;
                vt += SampleSceneDepth(uv + float2(-1.0, 1.0) * delta) * -1.0;
                vt += SampleSceneDepth(uv + float2(0.0, 1.0) * delta) * -2.0;
                vt += SampleSceneDepth(uv + float2(1.0, 1.0) * delta) * -1.0;

                return sqrt(hr * hr + vt * vt);
            }

            half4 frag(Varyings input) : SV_Target {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float s = pow(1 - saturate(sobel(input.uv)), 50);
                #ifdef RAW_OUTLINE
                return half4(s.xxx, 1);
                #else
                half4 col = GetSource(input);
                #ifdef POSTERIZE
                col = pow(col, 0.4545);
                float3 c = RgbToHsv(col);
                c.z = round(c.z * _PosterizationCount) / _PosterizationCount;
                col = float4(HsvToRgb(c), col.a);
                col = pow(col, 2.2);
                #endif
                return col * s;
                #endif
            }

            #pragma vertex Vert
            #pragma fragment frag
            ENDHLSL
        }
    }
    FallBack Off
}