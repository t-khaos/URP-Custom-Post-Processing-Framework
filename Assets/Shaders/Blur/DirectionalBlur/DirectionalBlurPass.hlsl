#ifndef _DIRECTIONAL_PASS_INCLUDED
#define _DIRECTIONAL_PASS_INCLUDED

#define _Iteration _DirectionalParams.x
#define _BlurSize _DirectionalParams.y
#define _Direction _DirectionalParams.zw

float4 _DirectionalParams;

half4 DirectionalBlurPassFragment(Varyings input) : SV_Target {
    half3 color = 0.0f;

    for (int i = -_Iteration; i < _Iteration; i++) {
        color += GetSource(input.uv - _Direction * _SourceTexture_TexelSize.xy * i);
    }

    color /= 2.0 * _Iteration;

    return half4(color, 1.0);
}

#endif
