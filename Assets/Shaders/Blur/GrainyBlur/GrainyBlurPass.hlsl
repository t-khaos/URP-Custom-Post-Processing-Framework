#ifndef _GAUSSIANBLUR_PASS_INCLUDED
#define _GAUSSIANBLUR_PASS_INCLUDED

float _Iteration;
float _BlurSize;

float Rand(float2 n) {
    return sin(dot(n, half2(1233.224, 1743.335)));
}

half4 GrainyBlurPassFragment(Varyings input) : SV_Target {
    float2 randomOffset = float2(0.0f, 0.0f);
    float2 random = Rand(input.uv);
    half3 color = 0.0;

    for (int i = 0; i < _Iteration; i++) {
        random = frac(43758.5453 * random + 0.61432);;
        randomOffset.x = (random - 0.5) * 2.0;
        random = frac(43758.5453 * random + 0.61432);
        randomOffset.y = (random - 0.5) * 2.0;

        color += GetSource(input.uv + randomOffset * _SourceTexture_TexelSize.xy * (1.0f + i * _BlurSize));
    }

    color /= _Iteration;

    return half4(color, 1.0);
}

#endif
