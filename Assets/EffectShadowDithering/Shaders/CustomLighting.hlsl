#ifndef CUSTOM_LIGHTING_INCLUDED
#define CUSTOM_LIGHTING_INCLUDED

#ifndef SHADERGRAPH_PREVIEW
	#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
// 	#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
// 	#if (SHADERPASS != SHADERPASS_FORWARD)
// 		#undef REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR
// 	#endif
#endif

struct CustomLightingData
{
	// Position and orientation (diffuse lighting)
	float3 positionWS;
	float3 normalWS;
	float3 viewDirectionWS;
	float4 shadowCoord;
	
	// Surface attributes
	float3 albedo;
	float smoothness;
	
	// Dithering pattern value rgb
	float ditherValue;
};

// Translate a [0, 1] smoothness value to an exponent
float GetSmoothnessPower(float rawSmoothness)
{
	return exp2(10 * rawSmoothness + 1);
}

#ifndef SHADERGRAPH_PREVIEW
float3 CustomLightHandling(CustomLightingData d, Light light)
{
	// light strength
	float radiance = light.color * light.shadowAttenuation;

	// diffuse lighting
	float diffuse = saturate(dot(d.normalWS, light.direction));

	// specular lighting
	float specularDot = saturate(dot(d.normalWS, normalize(light.direction + d.viewDirectionWS)));
	float specular = pow(specularDot, GetSmoothnessPower(d.smoothness)) * diffuse;
	
	float3 c = d.albedo * radiance * (diffuse + specular);

	// apply dithering
	float color = step(d.ditherValue, c);
	
	return color;
}
#endif

float3 CalculateCustomLighting(CustomLightingData d)
{
#ifndef SHADERGRAPH_PREVIEW
	// Get the main light. Located in Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl
	half4 shadowMask = 1;
	Light mainLight = GetMainLight(d.shadowCoord, d.positionWS, shadowMask);

	float3 color = 0;
	// Shade the main light
	color += CustomLightHandling(d, mainLight);
	
	return color;
	
#else
	// In preview calculate diffuse approximation
	float3 lightDir = float3(.5, .5,  0);
	float intemsity = saturate(dot(d.normalWS, lightDir))
		+ pow(saturate(dot(d.normalWS, normalize(d.viewDirectionWS + lightDir))), GetSmoothnessPower(d.smoothness));
	return d.albedo * intemsity;

#endif
}


// Wrapper function to call from CustomFunction node
void CalculateCustomLighting_float(float3 Position, float3 Normal, float3 viewDirectionWS, float3 Albedo, float Smoothness, float DitherValue, out float3 Color)
{
	CustomLightingData d;
	d.positionWS = Position;
	d.normalWS = Normal;
	d.viewDirectionWS = viewDirectionWS;
	d.albedo = Albedo;
	d.smoothness = Smoothness;
	d.ditherValue = DitherValue;
	
#ifndef SHADERGRAPH_PREVIEW
	// Calculate the main light shadow coord
	
	// calc position related to the pixel on screen
	float4 positionCS = TransformWorldToHClip(Position);

	// There are two options depending on if cascades are enabled in Unity
	#if SHADOWS_SCREEN
		d.shadowCoord = ComputeScreenPos(positionCS);
	#else
		d.shadowCoord = TransformWorldToShadowCoord(Position);
	#endif
#else
	// No shadows in preview
	d.shadowCoord = 0;
#endif	
	
	Color = CalculateCustomLighting(d);
}

#endif