#include "Headers/LightParameters.h"
#include "Headers/Textureing.h"

float CalculateAttenuation(float3 attenuationFactors, float distanceToLight, float lightRange,
	float numerator = 1.0)
{
	//float const_att = numerator / attenuationFactors.x;
	float quad_att = numerator / (attenuationFactors.z * distanceToLight*distanceToLight);
	//float lin_att = numerator / (attenuationFactors.y * distanceToLight);
	return numerator / (/*const_att + lin_att * distanceToLight +*/ quad_att * distanceToLight * distanceToLight);
}

LightPsOut PixelShaderFunction(GB_VS_OUT input) : COLOR0
{

	float2 pixelPosition = float2(input.scrPos.x, input.scrPos.y);
	float2 texCoord = input.uv;

	float4 diffuse = DiffuseMap.Sample(Sampler, texCoord);
	float4 specular = SpecularMap.Sample(Sampler, texCoord);
	float3 normal = NormalMap.Sample(Sampler, texCoord).xyz;
	float4 misc = DepthMap.Sample(Sampler, texCoord);
	float depth = misc.r / misc.g;
	if (misc.x == 0.0f || misc.y == 0.0f)
		discard;
	float4 posWorld = CalculateWorldSpacePosition(input.scrPos.xy, depth, gInvViewProj);
	float3 toLight = LightPosition - posWorld;
	float distanceToLight = length(toLight);

	toLight = normalize(toLight);
	float attenuation = CalculateAttenuation(Intensity, distanceToLight, Range);

	float distanceFactor = 1.0f - saturate(distanceToLight / Range);

	normal = (normal - 0.5f) * 2.f;
	float nDotL = saturate(dot(normal, toLight)) * distanceFactor * attenuation;

	float4 finalDiffuse = diffuse * nDotL * DiffuseColor;
	float4 finalSpecular = specular * nDotL;

	LightPsOut output = (LightPsOut)0;
	output.color.xyz = (finalDiffuse + finalSpecular).xyz;
	output.color.a = diffuse.a;
	output.depth = misc.x / misc.y;

	return output;
}

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile vs_5_0 VertexShaderFunction();
		PixelShader = compile ps_5_0 PixelShaderFunction();
	}
};