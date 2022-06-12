#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0
	#define PS_SHADERMODEL ps_4_0
#endif

texture2D Scene;
sampler s;

float4x4 World;
float4x4 View;
float4x4 Projection;

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float2 uv		: TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float2 uv		: TEXCOORD0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

	float4x4 worldView = mul(World, View);
	float4x4 wvp = mul(worldView, Projection);
	output.Position = mul(wvp, input.Position);
	output.uv = input.uv;

	return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
	float4 pixel = Scene.Sample(s, input.uv);
	float val = length(pixel.rgb);

	return float4(val, val, val, 1.0f);
}

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};