#ifndef BAYERDITHERING_HLSL_H
#define BAYERDITHERING_HLSL_H


void BayerDithering_float(float4 Color, float4 UV, out float3 Out)
{
	const int bayer_n = 4;
	
	const float3 BLACK = 0.0f;
	const float3 WHITE = 1.0f;
	
	float bayer_matrix_4x4[][bayer_n] = {
		{    -0.5,       0,  -0.375,   0.125 },
		{    0.25,   -0.25,   0.375, - 0.125 },
		{ -0.3125,  0.1875, -0.4375,  0.0625 },
		{  0.4375, -0.0625,  0.3125, -0.1875 },
	};
	
	// float orig_color = get_screen_gradient(s.y);
	//float orig_color = WHITE;
	
	float bayer_r = 16.0f;
	
	float NUM_VALUES = 8.0f;
	
	Out = BLACK;
	
	float bayer_value = bayer_matrix_4x4[UV.y % bayer_n][UV.x % bayer_n];
	
	float output_color = Color.rgb + (bayer_r * bayer_value);
	
	// Color screen blue to white
	if (output_color < NUM_VALUES * 0.5)
	{
		Out = WHITE;
	}

	// Test
	// Out = float3(UV.x, UV.y, 0.0f);
	// Out = Color;
}

#endif
