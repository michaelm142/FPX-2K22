#ifndef _GBUFFERS_H
#define _GBUFFERS_H

#include "Camera.h"

struct GBufferVSInput
{
	float4 Position : POSITION0;
	float3 Normal : NORMAL0;
	float2 uv : TEXCOORD0;
	float3 tangent : BINORMAL0;
	float3 bitangent : BINORMAL1;
};

struct GBufferVSOutput
{
	float4 position : SV_Position;
	float4 depth : NORMAL1;
	float3 tangent : BINORMAL0;
	float3 bitangent : BINORMAL1;
	float3 normal : NORMAL0;
	float2 uv : TEXCOORD0;
};

struct GBufferPSOutput
{
	float4 diffuse : SV_Target0;
	float4 normal : SV_Target1;
	float4 specular : SV_Target2;
	float4 depth : SV_Target3;
};

GBufferVSOutput GBufferVS(GBufferVSInput input)
{
	GBufferVSOutput output;

	float4 worldPosition = mul(input.Position, World);
	output.position = mul(worldPosition, ViewProjection);

	output.tangent = mul(input.tangent, (float3x3)World);
	output.bitangent = mul(input.tangent, (float3x3)World);
	output.normal = mul(input.Normal, (float3x3)World);
	output.uv = input.uv;
	output.depth = float4(output.position.z, output.position.w, 0, 0);

	return output;
}

#endif