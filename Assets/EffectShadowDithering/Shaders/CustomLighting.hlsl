#ifndef CUSTOM_LIGHTING_INCLUDED
#define CUSTOM_LIGHTING_INCLUDED

#ifndef SHADERGRAPH_PREVIEW
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#endif

struct CustomLightingData
{
	// Position and orientation
	float3 normalWS;
	
	// Surface attributes
	float3 albedo;
};

#ifndef SHADERGRAPH_PREVIEW
float3 CustomLightHandling(CustomLightingData d, Light light)
{
	// light strength
	float radiance = light.color;

	float diffuse = saturate(dot(d.normalWS, light.direction));

	float3 color = d.albedo * radiance * diffuse;

	return color;
}
#endif

float3 CalculateCustomLighting(CustomLightingData d)
{
#ifndef SHADERGRAPH_PREVIEW
	// Get the main light. Located in Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl
	Light mainLight = GetMainLight();

	float3 color = 0;
	// Shade the main light
	color += CustomLightHandling(d, mainLight);
	
	return color;
#else
	// In preview calculate simple diffuse
	float3 lightDir = float3(.5, .5,  0);
	float intemsity = saturate(dot(d.normalWS, lightDir));
	return d.albedo * intemsity;
#endif
}


// Wrapper function to call from CustomFunction node
void CalculateCustomLighting_float(float3 Normal, float3 Albedo, out float3 Color)
{
	CustomLightingData d;
	d.normalWS = Normal;
	d.albedo = Albedo;

	Color = CalculateCustomLighting(d);
}

// Wrapper function to call from CustomFunction node
void CalculateCustomLighting_half(float3 Normal, half3 Albedo, out half3 Color)
{
	// Default
	Color = half3(1.,0.,0.);
}

#endif