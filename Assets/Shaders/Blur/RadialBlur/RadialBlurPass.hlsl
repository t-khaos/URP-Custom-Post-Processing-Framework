#ifndef _GAUSSIANBLUR_PASS_INCLUDED
#define _GAUSSIANBLUR_PASS_INCLUDED

#define _Iteration _RadialParams.x
#define _BlurSize _RadialParams.y
#define _RadialCenterX _RadialParams.z
#define _RadialCenterY _RadialParams.w
#define _RadialCenter _RadialParams.zw

float4 _RadialParams;

half4 RadialBlurPassFragment(Varyings input) : SV_Target {
    half3 color = 0.0f;
    float2 blurVector = (_RadialCenter - input.uv) * _BlurSize;

    for (int i = 0; i < _Iteration; i++) {
        color += GetSource(input.uv);
        input.uv += blurVector * (1 + i / _Iteration);
    }

    color /= _Iteration;

    return half4(color, 1.0);
}

#endif
