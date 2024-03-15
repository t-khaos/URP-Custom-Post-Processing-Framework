#ifndef _BOXBLUR_PASS_INCLUDED
#define _BOXBLUR_PASS_INCLUDED

struct KawaseVaryings {
    float4 positionCS : SV_POSITION;
    float2 uv : TEXCOORD0;
    float4 uv2 : TEXCOORD1;
};

float _BlurSize;

KawaseVaryings BoxBlurPassVertex(uint vertexID : SV_VertexID) {
    KawaseVaryings output;

    ScreenSpaceData ssData = GetScreenSpaceData(vertexID);
    output.positionCS = ssData.positionCS;
    output.uv = ssData.uv;
    output.uv2 = output.uv.xyxy + _BlurSize * float4(-1.0f, -1.0f, 1.0f, 1.0f) * _SourceTexture_TexelSize.xyxy;

    return output;
}

half4 BoxBlurPassFragment(KawaseVaryings input) : SV_Target {
    half3 color = 0.0;

    color += GetSource(input.uv2.xy) * 0.25f;
    color += GetSource(input.uv2.zy) * 0.25f;
    color += GetSource(input.uv2.xw) * 0.25f;
    color += GetSource(input.uv2.zw) * 0.25f;

    return half4(color, 1.0);
}

#endif
