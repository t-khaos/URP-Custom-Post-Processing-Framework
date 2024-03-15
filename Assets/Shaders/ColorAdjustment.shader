Shader "Hidden/PostProcess/ColorAdjusments" {
    Properties {
        [HideInInspector] _MainTex ("Base (RGB)", 2D) = "white" {}
    }

    SubShader {
        Tags {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }
        
        LOD 200
        ZWrite Off
        Cull Off
        
        HLSLINCLUDE
        #include "Common/PostProcessing.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        ENDHLSL
        
        Pass {
            name "ColorAdjustmentPass"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag

            #pragma shader_feature EXPOSURE
            #pragma shader_feature CONTRAST
            #pragma shader_feature COLOR_FILTER
            #pragma shader_feature HUE_SHIFT
            #pragma shader_feature SATURATION

            // 曝光度 对比度 色相偏移 饱和度
            float4 _ColorAdjustments;
            float4 _ColorFilter;

            // 后曝光
            half3 ColorAdjustmentExposure(half3 color) {
                return color * _ColorAdjustments.x;
            }

            // 对比度
            half3 ColorAdjustmentContrast(float3 color) {
                // 为了更好的效果 将颜色从线性空间转换到logC空间（因为要取美术中灰）
                color = LinearToLogC(color);
                // 从颜色中减去均匀的中间灰度，然后通过对比度进行缩放，然后在中间添加中间灰度
                color = (color - ACEScc_MIDGRAY) * _ColorAdjustments.y + ACEScc_MIDGRAY;
                return LogCToLinear(color);
            }

            // 颜色滤镜
            half3 ColorAdjustmentColorFilter(float3 color) {
                color = SRGBToLinear(color);
                color = color * _ColorFilter.rgb;
                return color;
            }

            // 色相偏移
            half3 ColorAdjustmentHueShift(half3 color) {
                // 将颜色格式从rgb转换为hsv
                color = RgbToHsv(color);
                // 将色相偏移添加到h
                float hue = color.x + _ColorAdjustments.z;
                // 如果色相超出范围 将其截断
                color.x = RotateHue(hue, 0.0, 1.0);
                // 将颜色格式从hsv转换为rgb
                return HsvToRgb(color);
            }

            // 饱和度
            half3 ColorAdjustmentSaturation(half3 color) {
                // 获取颜色的亮度
                float luminance = Luminance(color);
                // 从颜色中减去亮度，然后通过饱和度进行缩放，然后在中间添加亮度
                return (color - luminance) * _ColorAdjustments.w + luminance;
            }

            half3 ColorAdjustment(half3 color) {
                // 防止颜色值过大的潜在隐患
                color = min(color, 60.0);
                // 后曝光
                #ifdef EXPOSURE
                color = ColorAdjustmentExposure(color);
                #endif
                // 对比度
                #ifdef CONTRAST
                color = ColorAdjustmentContrast(color);
                #endif
                // 颜色滤镜
                #ifdef COLOR_FILTER
                color = ColorAdjustmentColorFilter(color);
                #endif
                // 当对比度增加时，会导致颜色分量变暗，在这之后将颜色钳位
                color = max(color, 0.0);
                // 色相偏移
                #ifdef HUE_SHIFT
                color = ColorAdjustmentHueShift(color);
                #endif
                // 饱和度
                #ifdef SATURATION
                color = ColorAdjustmentSaturation(color);
                #endif
                // 当饱和度增加时，可能产生负数，在这之后将颜色钳位
                return max(color, 0.0);
                return color;
            }

            half4 frag(Varyings input) : SV_Target {
                half3 color = GetSource(input).xyz;
                half3 finalCol = ColorAdjustment(color);
                return half4(finalCol, 1.0);
            }
            ENDHLSL
        }
    }
}