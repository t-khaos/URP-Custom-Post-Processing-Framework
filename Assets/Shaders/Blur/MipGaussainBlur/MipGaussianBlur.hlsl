#ifndef _MIPGAUSSIAN_INCLUDED
#define _MIPGAUSSIAN_INCLUDED

#define PI 3.141592654

//当前层级的下采样金字塔图像
Texture2D    _DownSampleTexture;
SamplerState sampler_DownSampleTexture;
//SourceTexture是上采样金字塔的上一层图像

float _Sigma        = 2.0f;
int   _CurrentLevel = 0;
int   _MaxLevel     = 0;

half4 DownSamplePassFragment(Varyings input) : SV_Target
{
    //双线性采样，无需采样四个点，当前uv正好位于四个点的中心
    half3 color = GetSource(input.uv);
    return half4(color, 1.0);
}

float GaussianExp(float sigma2, uint level)
{
    return -(1 << (level << 1)) / (4.0 * PI * sigma2);
};

float GaussianBasis(float sigma2, uint level)
{
    return level < 0.0 ? 0.0 : exp(GaussianExp(sigma2, level));
};

float MipGaussianWeight(float sigma2, uint level)
{
    const float g = GaussianBasis(sigma2, level);

    return (1 << (level << 2)) * g;
}

float MipGaussianBlendWeight()
{
    const float sigma  = _Sigma;
    const float sigma2 = sigma * sigma;
    float       wsum   = 0.0, weight = 0.0;
    for (uint i = _CurrentLevel; i < _MaxLevel; ++i)
    {
        const float w = MipGaussianWeight(sigma2, i);
        weight        = i == _CurrentLevel ? w : weight;
        wsum += w;
    }

    return wsum > 0.0 ? weight / wsum : 1.0;
}


half4 UpSamplePassFragment(Varyings input) : SV_Target
{
    //上一层图像的上采样金字塔图像
    const float3 Color = GetSource(input.uv);
    //当前层级的下采样金字塔图像的层级权重
    const float weight = MipGaussianBlendWeight();
    //当前层级的下采样金字塔图像
    const float3 src = SAMPLE_TEXTURE2D(_DownSampleTexture, sampler_DownSampleTexture, input.uv);
    //插值结果写入当前层级的上采样金字塔图像
    return float4((1 - weight) * Color + weight * src, 1.0);
}



struct DownSampleVaryingsDualKawase
{
    float4 positionCS : SV_POSITION;
    float2 uv : TEXCOORD0;
    float2 uv1 : TEXCOORD1; //左上
    float2 uv2 : TEXCOORD2; //右上
    float2 uv3 : TEXCOORD3; //左下
    float2 uv4 : TEXCOORD4; //右下
};

DownSampleVaryingsDualKawase DownSamplePassVertexDualKawase(uint vertexID : SV_VertexID)
{
    DownSampleVaryingsDualKawase output;

    ScreenSpaceData ssData = GetScreenSpaceData(vertexID);
    output.positionCS      = ssData.positionCS;
    output.uv              = ssData.uv;
    //unity的屏幕坐标原点在左下角，所以uv坐标的y轴需要反转

    //采样单个像素的中心，只得到一个像素的值
    /*output.uv1 = output.uv + float2(-1.5f, 1.5f) * _SourceTexture_TexelSize.xy;
    output.uv2 = output.uv + float2(1.5f, 1.5f) * _SourceTexture_TexelSize.xy;
    output.uv3 = output.uv + float2(-1.5f, -1.5f) * _SourceTexture_TexelSize.xy;
    output.uv4 = output.uv + float2(1.5f, -1.5f) * _SourceTexture_TexelSize.xy;*/

    //采样四个像素的中心，一次性得到四个像素的均值
    output.uv1 = output.uv + float2(-1.0f, 1.0f) * _SourceTexture_TexelSize.xy;
    output.uv2 = output.uv + float2(1.0f, 1.0f) * _SourceTexture_TexelSize.xy;
    output.uv3 = output.uv + float2(-1.0f, -1.0f) * _SourceTexture_TexelSize.xy;
    output.uv4 = output.uv + float2(1.0f, -1.0f) * _SourceTexture_TexelSize.xy;
    

    return output;
}

half4 DownSamplePassFragmentDualKawase(DownSampleVaryingsDualKawase input) : SV_Target
{
    half3 color = 0.0f;

    /*color += GetSource(input.uv) * 8.0f;
    color += GetSource(input.uv1);
    color += GetSource(input.uv2);
    color += GetSource(input.uv3);
    color += GetSource(input.uv4);
    color *= 0.083333f;*/

    color += GetSource(input.uv) * 4.0f;
    color += GetSource(input.uv1);
    color += GetSource(input.uv2);
    color += GetSource(input.uv3);
    color += GetSource(input.uv4);
    color *= 0.125f;

    return half4(color, 1.0);
}

#endif
