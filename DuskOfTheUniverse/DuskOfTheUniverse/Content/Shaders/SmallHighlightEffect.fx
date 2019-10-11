#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float4x4 World;
float4x4 View;
float4x4 Projection;

float r;
float g;
float b;

// TODO: add effect parameters here.
sampler colourMap : register(s0);

float4 BrightenWash(float2 coords: TEXCOORD) : COLOR0
{
	float4 colour = tex2D(colourMap, coords);
	colour = float4(r, g, b, colour.a);
	return colour;
}

technique
{
	pass
	{
		pixelShader = compile ps_3_0 BrightenWash();
	}
}
