#ifndef _LIGHT_PARAMS
#define _LIGHT_PARAMS

float3 LightDirection;
float3 LightPosition;
float3 gCameraPos;
float3 AttenuationFactors;

float4 DiffuseColor;
float4 SpecularColor;

float Range;
float Intensity;

float4x4 gInvViewProj; 


float4 CalculateWorldSpacePosition(float2 pixelPosition, float pixelDepth,
	float4x4 inverseViewProjection)
{
	float4 viewPos = mul(float4(pixelPosition, pixelDepth, 1.f), inverseViewProjection);

	return viewPos / viewPos.w;
}
struct GB_VS_OUT
{
	float4 position : POSITION0;
	float2 scrPos : TEXCOORD1;
	float2 uv : TEXCOORD0;
};

GB_VS_OUT VertexShaderFunction(float4 position : POSITION0, float2 uv : TEXCOORD0)
{
	GB_VS_OUT output;

	output.position = position;
	output.scrPos = position.xy;
	output.uv = uv;

	return output;
}

struct LightPsOut
{
	float4 color : SV_Target0;
	float depth : SV_Depth;
};

#endif