Shader "My Shader/DepthTest" {
    Properties {}
    SubShader {
        Tags {
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
        }
        Pass {
            Name "Pass"
            
            Blend One OneMinusSrcAlpha

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"

            #pragma vertex vert
            #pragma fragment frag

            struct Attributes {
                float4 positionOS : POSITION;
                float3 normal : NORMAL;
            };

            struct Varyings {
                float4 positionCS : SV_POSITION;
                float4 positionNDC : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
            };

            Varyings vert(Attributes input) {
                Varyings output;

                VertexPositionInputs vertexInputs = GetVertexPositionInputs(input.positionOS);
                output.positionCS = vertexInputs.positionCS;
                output.positionNDC = vertexInputs.positionNDC;
                output.normalWS = TransformObjectToWorldNormal(input.normal);

                return output;
            }

            half4 frag(Varyings input) : SV_Target {
                float2 uv = input.positionNDC.xy / input.positionNDC.w;
                float rawDepth = SampleSceneDepth(uv);
                half3 finalCol = normalize(input.normalWS) * 0.5 + 0.5;

                return half4(finalCol, 0.5);
            }
            ENDHLSL
        }

        Pass {
            Name "DepthOnly"

            Tags {
                "LightMode" = "DepthOnly"
            }

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"

            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment
            ENDHLSL
        }

        Pass {
            Name "DepthNormals"
            Tags {
                "LightMode" = "DepthNormals"
            }

            HLSLPROGRAM
            #pragma vertex DepthNormalsVertex
            #pragma fragment DepthNormalsFragment


            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitDepthNormalsPass.hlsl"
            ENDHLSL
        }

    }
}