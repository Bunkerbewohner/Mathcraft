// Camera settings.
float4x4 World;
float4x4 View;
float4x4 Projection;


float3 LightDir = normalize(float3(-1, -1, 1));
float3 DiffuseLight = 0.3;
float3 AmbientColor = 0.8;

texture Materials;

sampler MaterialSampler = sampler_state
{
	Texture = (Materials);
	MagFilter = Linear;
};

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float3 InstancePos : POSITION1;
	float3 Normal : NORMAL0;
	float2 TexCoord : TEXCOORD0;
	int MaterialID : TEXCOORD1;	
};


struct VertexShaderOutput
{
	float2 TexCoord : TEXCOORD0;
	float4 Position : POSITION0;
	float4 PositionScreen : TEXCOORD2; // copy of position since POS0 cannot be access in pixel shader
	int MaterialID : TEXCOORD1;	
	float3 Normal : NORMAL0;
};

struct PixelShaderOutput
{
	float4 Color : COLOR0;
};

// Vertex shader helper function shared between the two techniques.
VertexShaderOutput VertexShaderCommon(VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

	float4 pos = input.Position + float4(input.InstancePos * 10, 1);

	// Apply the world and camera matrices to compute the output position.
	float4 worldPosition = mul(pos, World);
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);
	output.PositionScreen = output.Position;
	
	output.TexCoord = input.TexCoord;
	output.MaterialID = input.MaterialID;
	output.Normal = input.Normal;

	return output;
}


// Hardware instancing reads the per-instance world transform from a secondary vertex stream.
VertexShaderOutput HardwareInstancingVertexShader(VertexShaderInput input)
{
	return VertexShaderCommon(input);
}

// Both techniques share this same pixel shader.
PixelShaderOutput PixelShaderFunction(VertexShaderOutput input)
{
	PixelShaderOutput output = (PixelShaderOutput)0;

	int material_offset_x = input.MaterialID % 16;
	int material_offset_y = input.MaterialID / 16;

	float2 material_tex = float2(0, 0);
	material_tex.x = (1.0 / 16.0) * (float)material_offset_x;
	material_tex.y = (1.0 / 16.0) * (float)material_offset_y;

	float2 texcoord = material_tex + input.TexCoord * (1.0 / 8.0);
	float3 color = tex2D(MaterialSampler, texcoord);		

	// Compute lighting, using a simple Lambert model. 
	float diffuseAmount = max(-dot(input.Normal, LightDir), 0);
	float3 lightingResult = saturate(diffuseAmount * color * DiffuseLight + AmbientColor * color);		

	output.Color = float4(lightingResult, 1);
	//output.Color = float4(color, 1);

	return output;
}

// Hardware instancing technique.
technique HardwareInstancing
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 HardwareInstancingVertexShader();
		PixelShader = compile ps_3_0 PixelShaderFunction();
	}
}
