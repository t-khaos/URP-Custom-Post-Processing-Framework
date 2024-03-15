#ifndef _BOKEHBLUR_PASS_INCLUDED
#define _BOKEHBLUR_PASS_INCLUDED

float _BlurSize;
float _Iteration;
float _AreaOffset;
float _AreaSize;
float _AreaSpread;

float TiltShiftMask(float2 uv) {
    float centerY = uv.y * 2.0 - 1.0 + _AreaOffset;
    return pow(abs(centerY * _AreaSize), (1.0f - _AreaSpread));
}

half4 BokehBlurPassFragment(Varyings input) : SV_Target {
    float a = 2.3389f; // 137.5/360*2
    float2x2 rot = float2x2(cos(a), -sin(a), sin(a), cos(a));
    float2 angle = float2(_BlurSize * saturate(TiltShiftMask(input.uv)), 0.0f);
    float r = 1.0f;
    float2 uv = 0.0f;

    float3 accumulator = 0.0f;
    float3 divisor = 0.0f;

    for (int i = 1; i <= _Iteration; i++) {
        r += 1.0f / r;
        angle = mul(rot, angle);
        uv = input.uv + _SourceTexture_TexelSize.xy * angle * r;
        float4 color = GetSource(uv);
        accumulator += color * color;
        divisor += color;
    }

    return half4(accumulator / divisor, 1.0);
}

#endif
