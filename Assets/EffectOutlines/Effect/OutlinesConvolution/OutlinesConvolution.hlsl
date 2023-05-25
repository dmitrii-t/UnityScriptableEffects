#ifndef OUTLINES_CONVOLUTION_HLSL_H
#define OUTLINES_CONVOLUTION_HLSL_H


float4 Convolution(UnityTexture2D _Tex, float2 _TexelSize, float2 texCoordUV, float3 kernel, float2 filter)
{
	float4 color = float4(0, 0, 0, 0);
	
	for (int i = -1; i <= 1; i++)
	{
		float2 texCoord = texCoordUV + filter * i * _TexelSize;
		float4 texel = tex2D(_Tex, texCoord);
		color += texel * kernel[i + 1];
	}

	return color;
}

void OutlinesConvolution_float(UnityTexture2D _Tex, float2 _TexelSize, float2 texCoordUV, out float4 Out)
{
	const float3 kernel = float3(-1, 0, 1);
	
	float4 color = float4(0, 0, 0, 0);
	
	color += Convolution(_Tex,  _TexelSize,  texCoordUV, kernel, float2(1, 0));
	color += Convolution(_Tex,  _TexelSize,  texCoordUV, kernel, float2(0, 1));
	
	Out = color;
}

#endif // OUTLINES_CONVOLUTION_HLSL_H
