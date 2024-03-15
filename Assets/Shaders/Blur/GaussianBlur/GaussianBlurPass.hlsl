#ifndef _GAUSSIANBLUR_PASS_INCLUDED
#define _GAUSSIANBLUR_PASS_INCLUDED

struct GaussianVaryings {
    float4 positionCS : SV_POSITION;
    float2 uv : TEXCOORD0;
    float4 uv01 : TEXCOORD1;
    float4 uv23 : TEXCOORD2;
    UNITY_VERTEX_OUTPUT_STEREO
};

float4 _GaussianBlurSize;

GaussianVaryings GaussianBlurPassVertex(uint vertexID : SV_VertexID) {
    GaussianVaryings output;
    ScreenSpaceData ssData = GetScreenSpaceData(vertexID);
    output.positionCS = ssData.positionCS;
    output.uv = ssData.uv;
    output.uv01 = output.uv.xyxy + _GaussianBlurSize.xyxy * float4(1.0f, 1.0f, -1.0f, -1.0f) * _SourceTexture_TexelSize.xyxy;
    output.uv23 = output.uv.xyxy + _GaussianBlurSize.xyxy * float4(1.0f, 1.0f, -1.0f, -1.0f) * 2.0f * _SourceTexture_TexelSize.xyxy;

    return output;
}

half4 frag(GaussianVaryings input) : SV_Target {
    half3 color = 0.0;
    color += 0.4026 * GetSource(input.uv);
    color += 0.2442 * GetSource(input.uv01.xy);
    color += 0.2442 * GetSource(input.uv01.zw);
    color += 0.0545 * GetSource(input.uv23.xy);
    color += 0.0545 * GetSource(input.uv23.zw);

    return half4(color, 1.0);
}

#endif
