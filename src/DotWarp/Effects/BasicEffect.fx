matrix WorldViewProjection;

float3 CameraPosition = float3(0, 5, 20);

float3 AmbientLightColor = float3(0.2, 0.2, 0.2);

float3 Light0Direction = float3(0, -1, 0);
float3 Light0Color = float3(1, 1, 1);

float3 Light1Direction = float3(0, 0, -1);
float3 Light1Color = float3(1, 1, 1);

float3 Light2Direction = float3(1, 0, 0);
float3 Light2Color = float3(1, 1, 1);

float3 DiffuseColor = float3(0.1, 0.7, 0.1);
float3 SpecularColor = float3(0.3, 0.3, 0.3);
float SpecularPower = 16;

float Alpha = 1;

struct ColorPair
{
	float3 Diffuse;
	float3 Specular;
};

void CalculateDirectionalLight(float3 lightDirection, float3 lightColor, float3 normal, inout ColorPair current)
{
	float3 l = -lightDirection;
	float3 h = normalize(CameraPosition + l);
	float4 ret = lit(dot(normal, l), dot(normal, h), SpecularPower);

	current.Diffuse += lightColor * ret.g;
	current.Specular += lightColor * ret.b;
}

ColorPair CalculateLighting(float3 normal)
{
	ColorPair result;

	result.Diffuse = AmbientLightColor;
	result.Specular = float3(0, 0, 0);

	// Directional Light 0
	CalculateDirectionalLight(Light0Direction, Light0Color, normal, result);

	// Directional Light 1
	CalculateDirectionalLight(Light1Direction, Light1Color, normal, result);

	// Directional Light 2
	CalculateDirectionalLight(Light2Direction, Light2Color, normal, result);

	result.Diffuse *= DiffuseColor;
	result.Specular *= SpecularColor;

	return result;
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
	float3 Normal : NORMAL;
	float2 Uv : TEXCOORD0;
};

VS_OUT VS(VS_IN input)
{
    VS_OUT output = (VS_OUT)0;
    output.Position = mul(float4(input.Position, 1), WorldViewProjection);
	output.Normal = input.Normal;
	output.Uv = input.Uv;
    return output;
}

float4 PS( VS_OUT input ) : SV_Target
{
	ColorPair lightResult = CalculateLighting(input.Normal);

	float4 diffuse = float4(lightResult.Diffuse, Alpha);
	float4 color = diffuse + float4(lightResult.Specular, 0);
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