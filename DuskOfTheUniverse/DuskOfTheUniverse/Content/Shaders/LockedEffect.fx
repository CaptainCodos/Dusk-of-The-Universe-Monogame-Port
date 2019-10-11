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

// TODO: add effect parameters here.
sampler colourMap;

float4 BrightenWash(float2 coords: TEXCOORD) : COLOR
{
	float4 colour = tex2D(colourMap, coords);
	colour.rgb = (colour.r, colour.g, colour.b) * 0.3f;
	return colour;
}

technique
{
	pass
	{
		pixelShader = compile ps_3_0 BrightenWash();
	}
}