#ifndef OUTLINES_CONVOLUTION_HLSL_H
#define OUTLINES_CONVOLUTION_HLSL_H


void OutlinesConvolution_float(UnityTexture2D _Tex, float2 _TexelSize, float2 UV, out float4 Out)
{
	// float filter[9] = {
	// 	0., -1., 0.,
	// 	-1., 4., -1.,
	// 	0., -1., 0.
	// };

	float filter[9] = {
		-1., -1., -1.,
		-1., 8., -1.,
		-1., -1., -1.
	};

	float4 c = 0;

	int x, y, i = 0;

	int xx = 0, yy = 0;

	for(x = 0; x < 5; x++)
	{
		xx = x - 2;
		for(y = 0; y < 5; y++)
		{
			yy = y - 2;
			i = abs(xx) + 3 * abs(yy);
			c.r += filter[i] * tex2D(_Tex, UV + _TexelSize * float2(xx, yy)).r;
		}
	}

	

	
	Out = c;
}

#endif // OUTLINES_CONVOLUTION_HLSL_H
