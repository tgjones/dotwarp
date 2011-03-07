#define MAX_LIGHTS 16

cbuffer BasicEffectVertexConstants : register(b0)
{
	matrix WorldViewProjection;
	matrix World;
}

cbuffer BasicEffectPixelConstants : register(b0)
{
	float3 CameraPosition = float3(0, 5, 20);

	bool LightingEnabled = true;

	float3 AmbientLightColor = float3(0.3, 0.3, 0.3);

	float3 DiffuseColor = float3(0.1, 0.7, 0.1);
	float3 SpecularColor = float3(1, 1, 1);
	float SpecularPower = 16;

	bool TextureEnabled = false;

	float Alpha = 1;
}

struct DirectionalLight
{
	bool Enabled;
	float3 Direction;
	float3 Color;
};

cbuffer LightConstants : register(b1)
{
	int ActiveDirectionalLights;
	DirectionalLight DirectionalLights[MAX_LIGHTS];
}

sampler Sampler : register(s0);
Texture2D<float4> Texture : register(t0);

struct ColorPair
{
	float3 Diffuse;
	float3 Specular;
};

void CalculateDirectionalLight(DirectionalLight light, float3 position, float3 normal, inout ColorPair current)
{
	float3 directionToLight = -light.Direction;

	float3 diff = saturate(dot(normal, directionToLight)); // diffuse component
	current.Diffuse += light.Color * diff;

	// R = 2 * (N.L) * N - L
	float3 directionToCamera = normalize(CameraPosition - position);
	float3 reflect = normalize(2 * diff * normal - directionToLight); 
	float3 specular = pow(saturate(dot(reflect, directionToCamera)), SpecularPower); // R.V^n
	current.Specular += specular;
}

struct VS_IN
{
	float3 Position : POSITION;
	float3 Normal : NORMAL;
	float2 Uv : TEXCOORD0;
};

struct VS_OUT
{
    float4 Position : SV_POSITION;
	float3 WorldPosition : TEXCOORD0;
	float3 WorldNormal : NORMAL;
	float2 Uv : TEXCOORD1;
};

VS_OUT VS(VS_IN input)
{
    VS_OUT output = (VS_OUT)0;
    output.Position = mul(float4(input.Position, 1), WorldViewProjection);
	output.WorldPosition = mul(float4(input.Position, 1), World).xyz;
	output.WorldNormal = mul(float4(input.Normal, 1), World).xyz;
	output.Uv = input.Uv;
    return output;
}

float4 PS(VS_OUT input) : SV_Target
{
	ColorPair result;

	if (LightingEnabled)
	{
		result.Diffuse = AmbientLightColor;
		result.Specular = float3(0, 0, 0);
		
		// Directional lights.
		for (int i = 0; i < ActiveDirectionalLights; ++i)
			if (DirectionalLights[i].Enabled)
				CalculateDirectionalLight(DirectionalLights[i], input.WorldPosition, input.WorldNormal, result);

		result.Diffuse *= DiffuseColor;
		if (TextureEnabled)
			result.Diffuse *= Texture.Sample(Sampler, input.Uv);
		result.Specular *= SpecularColor;
	}
	else
	{
		result.Diffuse = DiffuseColor;
		if (TextureEnabled)
			result.Diffuse *= Texture.Sample(Sampler, input.Uv);
	}

	float4 diffuse = float4(result.Diffuse, Alpha);
	float4 color = diffuse + float4(result.Specular, 0);
	return color;
}

technique10 Render
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, VS() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS() ) );
    }
}