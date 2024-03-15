#ifndef _BOKEHBLUR_PASS_INCLUDED
#define _BOKEHBLUR_PASS_INCLUDED

float _BlurSize;
float _Iteration;

half4 BokehBlurPassFragment(Varyings input) : SV_Target {
    half3 color = GetSource(input);

    float a = 2.3389f; // 137.5/360*2
    float2x2 rot = float2x2(cos(a), -sin(a), sin(a), cos(a));
    float2 angle = float2(_BlurSize, 0.0f);
    float r = 0.0f;
    float2 uv = 0.0f;

    for (int i = 1; i <= _Iteration; i++) {
        r = sqrt(i);
        angle = mul(rot, angle);
        uv = input.uv + _SourceTexture_TexelSize.xy * angle * r;
        color += GetSource(uv);
    }

    color /= _Iteration;

    return half4(color, 1.0);
}

#endif
