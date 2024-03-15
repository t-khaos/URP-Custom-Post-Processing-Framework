#ifndef _DUALKAWASEBLUR_PASS_INCLUDED
#define _DUALKAWASEBLUR_PASS_INCLUDED

float _BlurSize;

struct DownSampleVaryings {
    float4 positionCS : SV_POSITION;
    float2 uv : TEXCOORD0;
    float4 uv2 : TEXCOORD1; // 左上 右下
};

DownSampleVaryings DownSamplePassVertex(uint vertexID : SV_VertexID) {
    DownSampleVaryings output;

    ScreenSpaceData ssData = GetScreenSpaceData(vertexID);
    output.positionCS = ssData.positionCS;
    output.uv = ssData.uv;
    output.uv2 = output.uv.xyxy + _BlurSize * float4(-0.5f, -0.5f, 0.5f, 0.5f) * _SourceTexture_TexelSize.xyxy;

    return output;
}

half4 DownSamplePassFragment(DownSampleVaryings input) : SV_Target {
    half3 color = 0.0;

    color += GetSource(input.uv) * 4.0f;
    color += GetSource(input.uv2.xy);
    color += GetSource(input.uv2.zy);
    color += GetSource(input.uv2.xw);
    color += GetSource(input.uv2.zw);

    color *= 0.125f;

    return half4(color, 1.0);
}


struct UpSampleVaryings {
    float4 positionCS : SV_POSITION;
    float2 uv : TEXCOORD0;
    float4 uv01 : TEXCOORD1; // 左上 左下
    float4 uv23 : TEXCOORD2; // 中上 中下
    float4 uv45 : TEXCOORD3; // 右上 右下
    float4 uv67 : TEXCOORD4; // 中左 中右
};

UpSampleVaryings UpSamplePassVertex(uint vertexID : SV_VertexID) {
    UpSampleVaryings output;

    ScreenSpaceData ssData = GetScreenSpaceData(vertexID);
    output.positionCS = ssData.positionCS;
    output.uv = ssData.uv;

    _BlurSize *= 0.5f;
    
    output.uv01.xy = output.uv + float2(-1.0f, -1.0f) * _SourceTexture_TexelSize.xy * _BlurSize;
    output.uv01.zw = output.uv + float2(-1.0f, 1.0f) * _SourceTexture_TexelSize.xy * _BlurSize;
    output.uv23.xy = output.uv + float2(0.0f, -2.0f) * _SourceTexture_TexelSize.xy * _BlurSize;
    output.uv23.zw = output.uv + float2(0.0f, 2.0f) * _SourceTexture_TexelSize.xy * _BlurSize;
    output.uv45.xy = output.uv + float2(1.0f, -1.0f) * _SourceTexture_TexelSize.xy * _BlurSize;
    output.uv45.zw = output.uv + float2(1.0f, 1.0f) * _SourceTexture_TexelSize.xy * _BlurSize;
    output.uv67.xy = output.uv + float2(-2.0f, 0.0f) * _SourceTexture_TexelSize.xy * _BlurSize;
    output.uv67.zw = output.uv + float2(2.0f, 0.0f) * _SourceTexture_TexelSize.xy * _BlurSize;

    return output;
}

half4 UpSamplePassFragment(UpSampleVaryings input) : SV_Target {
    half3 color = 0.0;

    color += GetSource(input.uv01.xy);
    color += GetSource(input.uv01.zw);
    color += GetSource(input.uv23.xy) * 2.0f;
    color += GetSource(input.uv23.zw) * 2.0f;
    color += GetSource(input.uv45.xy);
    color += GetSource(input.uv45.zw);
    color += GetSource(input.uv67.xy) * 2.0f;
    color += GetSource(input.uv67.zw) * 2.0f;
    color *= 0.083333;

    return half4(color, 1.0);
}


#endif
