#ifndef DIFFERENCE_OF_GAUSSIANS_HLSL_H
#define DIFFERENCE_OF_GAUSSIANS_HLSL_H

#define PI 3.14159265358979323846f

float gaussian(float sigma, float pos)
{
	return (1.0f / sqrt(2.0f * PI * sigma * sigma)) * exp(-(pos * pos) / (2.0f * sigma * sigma));
}

float luminance(float3 color)
{
	return dot(color, float3(0.299f, 0.587f, 0.114f));
}

float4 GaussianBlurX_float(UnityTexture2D _Tex, float2 _TexelSize, float2 UV, float Sigma, float K, float GaussianKernelSize)
{
	float2 col = 0;
	float  kernelSum1 = 0.0f;
	float  kernelSum2 = 0.0f;

	for(int x = -GaussianKernelSize; x <= GaussianKernelSize; ++x)
	{
		float4 texel = tex2D(_Tex, UV + float2(x, 0) * _TexelSize.xy);
		
		float c = luminance(texel);
		float gauss1 = gaussian(Sigma, x);
		float gauss2 = gaussian(Sigma * K, x);

		col.r += c * gauss1;
		kernelSum1 += gauss1;

		col.g += c * gauss2;
		kernelSum2 += gauss2;
	}

	return float4(col.r / kernelSum1, col.g / kernelSum2, 0, 0);
}

float4 GaussianBlurY_float(UnityTexture2D _Tex, float2 _TexelSize, float2 UV, float Sigma, float K, float GaussianKernelSize)
{
	float2 col = 0;
	float kernelSum1 = 0.0f;
	float kernelSum2 = 0.0f;

	for (int y = -GaussianKernelSize; y <= GaussianKernelSize; ++y) {
		float4 c = tex2D(_Tex, UV + float2(0, y) * _TexelSize.xy);
		float gauss1 = gaussian(Sigma, y);
		float gauss2 = gaussian(Sigma * K, y);

		col.r += c.r * gauss1;
		kernelSum1 += gauss1;

		col.g += c.g * gauss2;
		kernelSum2 += gauss2;
	}

	return float4(col.r / kernelSum1, col.g / kernelSum2, 0, 0);
}

#define _Invert 0
#define _Thresholding 1
#define _Tanh 1

void DifferenceOfGaussians_float(UnityTexture2D Tex,  float2 TexelSize, float2 UV,  float Sigma, float Threshold, float K,
	float GaussianKernelSize, float Tau, float Phi, out float4 Out)
{
	Out = 0;
	
	float4 R = GaussianBlurX_float(Tex, TexelSize, UV, Sigma, K, GaussianKernelSize);
	float4 G = GaussianBlurY_float(Tex, TexelSize, UV, Sigma, K, GaussianKernelSize);
	float4 D = (R - Tau * G);

	if (_Thresholding) {
		if (_Tanh)
			D = (D >= Threshold) ? 1 : 1 + tanh(Phi * (D - Threshold));
		else
			D = (D >= Threshold) ? 1 : 0;
	}

	if (_Invert)
		D = 1 - D;

	Out = D;
}

#endif
