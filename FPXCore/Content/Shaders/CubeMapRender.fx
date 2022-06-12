float4x4 World;
float4x4 View;
float4x4 Projection;

#include "Headers/Textureing.h"

struct VertexShaderInput
{
    float4 Position : SV_Position;
};

struct VertexShaderOutput
{
    float4 Position : SV_Position;
	float3 normal : TEXCOORD0;
};

struct PixelShaderOutput
{
	float4 color : SV_Target0;
	float depth : SV_Depth;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
	output.normal = input.Position.xyz;

    // TODO: add your vertex shader code here.

    return output;
}

PixelShaderOutput PixelShaderFunction(VertexShaderOutput input)
{
	PixelShaderOutput output = (PixelShaderOutput)0;

	float4 col = texCUBE(SkyboxSampler, input.normal);
	output.color = float4(col.rgb, 1.0f);
	output.depth = 0.8f;

	return output;
}

technique Technique1
{
    pass Pass1
    {
        // TODO: set renderstates here.

        VertexShader = compile vs_5_0 VertexShaderFunction();
        PixelShader = compile ps_5_0 PixelShaderFunction();
    }
}
