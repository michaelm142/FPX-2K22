texture2D Scene;
sampler s;

float4x4 World;
float4x4 View;
float4x4 Projection;

// GAUSSIAN BLUR SETTINGS {{{
float Directions = 16.0; // BLUR DIRECTIONS (Default 16.0 - More is better but slower)
float Quality = 3.0; // BLUR QUALITY (Default 4.0 - More is better but slower)
float Size = 1.0; // BLUR SIZE (Radius)
const float Pi = 6.28318530718; // Pi*2
float2 iResolution;
// GAUSSIAN BLUR SETTINGS }}}

struct VertexShaderInput
{
	float4 position : SV_POSITION;
	float2 uv		: TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 position : SV_POSITION;
	float2 uv		: TEXCOORD0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

	float4x4 worldView = mul(World, View);
	float4x4 wvp = mul(worldView, Projection);
	output.position = mul(wvp, input.position);
	output.uv = input.uv;

	return output;
}

float4 MainPS(VertexShaderOutput input) : SV_Target0
//{ void mainImage(out vec4 fragColor, in vec2 fragCoord)
{
	float2 Radius = Size / iResolution.xy;

	// Normalized pixel coordinates (from 0 to 1)
	float2 uv = input.uv / iResolution.xy;
	// Pixel colour
	float4 Color = Scene.Sample(s, uv);

	// Blur calculations
	for (float d = 0.0; d < Pi; d += Pi / Directions)
	{
		for (float i = 1.0 / Quality; i <= 1.0; i += 1.0 / Quality)
		{
			Color += Scene.Sample(s, uv + float2(cos(d),sin(d))*Radius*i);
		}
	}

	// Output to screen
	Color /= Quality * Directions - 15.0;
	return Color;
}

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile vs_5_0 MainVS();
		PixelShader = compile ps_5_0 MainPS();
	}
};