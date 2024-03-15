#ifndef _BLOOM_PASS_INCLUDED
#define _BLOOM_PASS_INCLUDED

struct GaussianVaryings {
    float4 positionCS : SV_POSITION;
    float2 uv : TEXCOORD0;
    float4 uv01 : TEXCOORD1;
    float4 uv23 : TEXCOORD2;
    UNITY_VERTEX_OUTPUT_STEREO
};

TEXTURE2D(_BloomSourceTexture);
SAMPLER(sampler_BloomSourceTexture);

float4 _BloomBlurSize;
float4 _BloomThreshold;
float _BloomIntensity;

GaussianVaryings GaussianBlurPassVertex(uint vertexID : SV_VertexID) {
    GaussianVaryings output;
    ScreenSpaceData ssData = GetScreenSpaceData(vertexID);
    output.positionCS = ssData.positionCS;
    output.uv = ssData.uv;
    output.uv01 = output.uv.xyxy + _BloomBlurSize.xyxy * float4(1.0f, 1.0f, -1.0f, -1.0f) * _SourceTexture_TexelSize.xyxy;
    output.uv23 = output.uv.xyxy + _BloomBlurSize.xyxy * float4(1.0f, 1.0f, -1.0f, -1.0f) * 2.0f * _SourceTexture_TexelSize.xyxy;

    return output;
}

half4 GaussianBlurPassFragment(GaussianVaryings input) : SV_Target {
    half3 color = 0.0;
    color += 0.4026 * GetSource(input.uv);
    color += 0.2442 * GetSource(input.uv01.xy);
    color += 0.2442 * GetSource(input.uv01.zw);
    color += 0.0545 * GetSource(input.uv23.xy);
    color += 0.0545 * GetSource(input.uv23.zw);

    return half4(color, 1.0);
}

half4 BloomCombinePassFragment(Varyings input) : SV_Target {
    half3 lowRes = GetSource(input).rgb;
    half3 highRes = SAMPLE_TEXTURE2D(_BloomSourceTexture, sampler_BloomSourceTexture, input.uv);

    half3 color = 0.0f;
    #if defined _BLOOMADDTIVE
    color = lowRes * _BloomIntensity + highRes;
    #else
    color = lerp(highRes, lowRes, saturate(_BloomIntensity));
    #endif

    return half4(color, 1.0f);
}

float3 ApplyBloomThreshold(float3 color) {
    float brightness = Max3(color.r, color.g, color.b);
    float soft = brightness + _BloomThreshold.y;
    soft = clamp(soft, 0.0f, _BloomThreshold.z);
    soft = soft * soft * _BloomThreshold.w;
    float contribution = max(soft, brightness - _BloomThreshold.x);
    contribution /= max(brightness, 0.00001f);
    return color * contribution;
}

half4 BloomPrefilterPass(Varyings input) : SV_Target {
    half3 color = ApplyBloomThreshold(GetSource(input));
    return half4(color, 1.0f);
}

half4 BloomPrefilterFirefilesPass(Varyings input) : SV_Target {
    half3 color = 0.0;
    float weightSum = 0.0f;
    float2 offsets[] = {float2(0.0f, 0.0f), float2(-1.0f, -1.0f), float2(-1.0f, 1.0f), float2(1.0f, -1.0f), float2(1.0f, 1.0f)};
    for (int i = 0; i < 5; i++) {
        half3 c = GetSource(input.uv + offsets[i] * _SourceTexture_TexelSize.xy * 2.0);
        c = ApplyBloomThreshold(c);
        float w = 1.0 / (Luminance(c) + 1.0f);
        color += c * w;
        weightSum += w;
    }
    color /= weightSum;
    return half4(color, 1.0f);
}

half4 BloomScatterFinalPass(Varyings input) : SV_Target {
    half3 lowRes = GetSource(input).rgb;
    half3 highRes = SAMPLE_TEXTURE2D(_BloomSourceTexture, sampler_BloomSourceTexture, input.uv);
    // 将低分辨率（光扩散）加上高分辨率的低亮度光 来近似补偿能量损失
    lowRes += highRes - ApplyBloomThreshold(highRes);
    return float4(lerp(highRes, lowRes, _BloomIntensity), 1.0f);
}

#endif
